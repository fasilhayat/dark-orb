namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;
using System.Linq;

internal class TurnProcessor
{
    private readonly IDiceService _dice;
    private readonly IStatusEffectService _statusEffect;
    private readonly IActionDecisionSource _heroActionSource;
    private readonly IActionDecisionSource _enemyActionSource;
    private readonly ITargetSelector _heroTargetSelector;
    private readonly ITargetSelector _enemyTargetSelector;
    private readonly SpellProcessor _spellProcessor;
    private readonly TurnMeterProcessor _turnMeterProcessor;
    private readonly CombatLogger _logger;

    public TurnProcessor(
        IDiceService dice,
        IStatusEffectService statusEffect,
        IActionDecisionSource heroActionSource,
        IActionDecisionSource enemyActionSource,
        ITargetSelector heroTargetSelector,
        ITargetSelector enemyTargetSelector,
        SpellProcessor spellProcessor,
        TurnMeterProcessor turnMeterProcessor,
        CombatLogger logger)
    {
        _dice = dice;
        _statusEffect = statusEffect;
        _heroActionSource = heroActionSource;
        _enemyActionSource = enemyActionSource;
        _heroTargetSelector = heroTargetSelector;
        _enemyTargetSelector = enemyTargetSelector;
        _spellProcessor = spellProcessor;
        _turnMeterProcessor = turnMeterProcessor;
        _logger = logger;
    }

    public async Task ProcessCrowdControlledActorsAsync(
        int tick, List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states)
        {
            if (!s.Character.IsAlive || !s.Meter.IsReady) continue;
            var ccLabel = s.Character.TryGetCrowdControlLabel();
            if (ccLabel is null) continue;

            var expired = _statusEffect.TickAll(s.Character);
            await StatusEffectProcessor.NotifyExpiredEffectsAsync(tick, s.Character, expired, notify);

            if (s.QueuedSpell is not null)
            {
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = s.Character.Name,
                    EventType = "SpellLost",
                    AttackSourceName = s.QueuedSpell.Spell.Name,
                    Message = $"{s.Character.Name} loses concentration on {s.QueuedSpell.Spell.Name} — crowd controlled!"
                });
                s.QueuedSpell = null;
            }

            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = s.Character.Name,
                EventType = "SkippedTurn",
                CcLabel = ccLabel,
                Message = $"{s.Character.Name} is {ccLabel} and cannot act!"
            });
        }
    }

    public async Task<ActorSetup?> SetupActorAttackAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        if (actorState.QueuedSpell is not null)
            return await HandleQueuedSpellAsync(tick, actorState, states, notify, ct);
        return await HandleNewAttackSetupAsync(tick, actorState, states, lastAttackerOf, notify, ct);
    }

    private async Task<ActorSetup?> HandleQueuedSpellAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var qs = actorState.QueuedSpell!;

        if (qs.RemainingCost > 0)
        {
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "SpellCharging",
                Message = $"{actorState.Character.Name} is charging {qs.Spell.Name}  (need {qs.RemainingCost} more TM)"
            });
            return null;
        }

        actorState.QueuedSpell = null;
        var target = qs.Target;

        if (!target.IsAlive)
        {
            if (qs.Spell.IsHealing)
            {
                var liveAllies = states
                    .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive)
                    .Select(s => s.Character).ToList();
                var healTarget = liveAllies
                    .Where(a => a.CurrentHitPoints < a.MaxHitPoints)
                    .MinBy(a => a.CurrentHitPoints);
                target = healTarget ?? actorState.Character;
            }
            else
            {
                var liveEnemies = states
                    .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive).ToList();
                if (liveEnemies.Count == 0) return null;
                var reSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                target = await reSelector.SelectTargetAsync(
                    actorState.Character, liveEnemies.Select(s => s.Character), ct);
            }
        }

        var actualTmCost = actorState.Character.ComputeSpellTurnMeterCost(qs.Spell);
        await _spellProcessor.DeductManaCostAsync(tick, actorState, qs.Spell, notify);
        return new ActorSetup(qs.Spell, target, actualTmCost, IsSpell: true);
    }

    private async Task<ActorSetup?> HandleNewAttackSetupAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var enemies = states
            .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive).ToList();
        if (enemies.Count == 0) return null;

        var allies = states
            .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive)
            .Select(s => s.Character).ToList();

        var decisionSource = actorState.PartyIndex == 0 ? _heroActionSource : _enemyActionSource;
        var attackSource = await decisionSource.ChooseAttackAsync(
            actorState.Character, actorState.AttackSource,
            enemies.Select(s => s.Character).ToList(), allies, tick, ct);

        if (attackSource is null)
        {
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "SkippedTurn",
                Message = $"{actorState.Character.Name} skips their turn."
            });
            actorState.Meter.IsActive = false;
            await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
            return null;
        }

        if (attackSource is MoveIntent)
        {
            var speed = actorState.Character.EffectiveMovementSpeed;
            var from = actorState.EngagementRange;

            // Root / immobilize check — cannot move if movement is 0 or negative
            if (speed <= 0 || actorState.Character.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Root))
            {
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = actorState.Character.Name,
                    EventType = "SkippedTurn",
                    Message = $"{actorState.Character.Name} is rooted and cannot move!"
                });
                actorState.Meter.IsActive = false;
                await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
                return null;
            }

            // Every 30 ft of effective speed = 1 band of movement
            var bands = Math.Max(1, speed / 30);
            var to = from;
            for (var b = 0; b < bands; b++)
                to = to switch
                {
                    EngagementRange.Melee => EngagementRange.Short,
                    EngagementRange.Short => EngagementRange.Long,
                    EngagementRange.Long => EngagementRange.Melee,
                    _ => EngagementRange.Melee
                };
            actorState.EngagementRange = to;

            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "Move",
                Message = $"{actorState.Character.Name} moves {speed} ft ({from} → {to})."
            });

            // Deduct TM for moving, then continue choosing an attack
            const int MoveTmCost = 30;
            actorState.Meter.CurrentValue = Math.Max(0, actorState.Meter.CurrentValue - MoveTmCost);
            attackSource = await decisionSource.ChooseAttackAsync(
                actorState.Character, actorState.AttackSource,
                enemies.Select(s => s.Character).ToList(), allies, tick, ct);
            if (attackSource is null)
            {
                actorState.Meter.IsActive = false;
                await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
                return null;
            }
            if (attackSource is MoveIntent)
            {
                await notify(new CombatLogEntry
                {
                    Tick = tick, ActorName = actorState.Character.Name,
                    EventType = "SkippedTurn",
                    Message = $"{actorState.Character.Name} tries to move again but has no TM left."
                });
                actorState.Meter.IsActive = false;
                await notify(_turnMeterProcessor.BuildAfterTurnEntry(actorState, tick, TurnmeterState.TurnThreshold));
                return null;
            }
        }

        var isSpell = attackSource is Spell;
        var meterNow = actorState.Meter.CurrentValue;
        var tmCost = isSpell ? actorState.Character.ComputeSpellTurnMeterCost((Spell)attackSource) : 100;

        if (attackSource is UnarmedStrike && actorState.Character.MemorizedSpells.Count > 0)
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "InsufficientMana",
                Message = $"{actorState.Character.Name} lacks mana for spells — resorting to unarmed strike!"
            });

        if (isSpell && meterNow < tmCost)
        {
            await _spellProcessor.QueueSpellAsync(tick, actorState, (Spell)attackSource, enemies, allies, tmCost, meterNow, notify, ct, _heroTargetSelector, _enemyTargetSelector);
            return null;
        }

        Character target;
        if (attackSource is Spell castSpell && castSpell.IsHealing)
        {
            var healTarget = allies
                .Where(a => a.CurrentHitPoints < a.MaxHitPoints)
                .MinBy(a => a.CurrentHitPoints);
            target = healTarget ?? actorState.Character;
        }
        else
        {
            target = await SelectActorTargetAsync(actorState, enemies, lastAttackerOf, ct);
        }
        await _spellProcessor.DeductManaCostAsync(tick, actorState, isSpell ? (Spell)attackSource : null, notify);
        return new ActorSetup(attackSource, target, tmCost, isSpell);
    }

    private async Task<Character> SelectActorTargetAsync(
        CombatantState actorState, List<CombatantState> enemies,
        Dictionary<Character, Character> lastAttackerOf, CancellationToken ct)
    {
        if (actorState.IsSummoned && actorState.SummonedBy is { } master)
        {
            var last = lastAttackerOf.GetValueOrDefault(master);
            if (last?.IsAlive == true && enemies.Any(e => e.Character == last))
                return last;
            return enemies.OrderBy(e => e.Character.CurrentHitPoints).First().Character;
        }
        var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
        return await selector.SelectTargetAsync(actorState.Character, enemies.Select(s => s.Character), ct);
    }
}
