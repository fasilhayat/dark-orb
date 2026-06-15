namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

internal class AttackResolver
{
    private readonly ICombatService _combat;
    private readonly IDiceService _dice;
    private readonly CombatLogger _logger;
    private readonly VictoryEvaluator _victoryEvaluator;
    private readonly TurnMeterProcessor _turnMeterProcessor;
    private readonly StatusEffectProcessor _statusEffectProcessor;
    private readonly SpellProcessor _spellProcessor;

    public AttackResolver(
        ICombatService combat, IDiceService dice, CombatLogger logger,
        VictoryEvaluator victoryEvaluator,
        TurnMeterProcessor turnMeterProcessor,
        StatusEffectProcessor statusEffectProcessor,
        SpellProcessor spellProcessor)
    {
        _combat = combat;
        _dice = dice;
        _logger = logger;
        _victoryEvaluator = victoryEvaluator;
        _turnMeterProcessor = turnMeterProcessor;
        _statusEffectProcessor = statusEffectProcessor;
        _spellProcessor = spellProcessor;
    }

    public async Task<CombatResult?> ResolveAttackOutcomeAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        if (result.IsClash)
            return await ProcessClashAsync(
                tick, actorState, setup, result, states, stateMap, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        if (result.IsHit)
            return await ProcessHitAsync(
                tick, actorState, setup, result, states, stateMap, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        return null;
    }

    // Refactored-caller convenience overload.
    public async Task<CombatResult?> ProcessClashAsync(
        int tick, int currentRound, CombatantState actorState,
        IAttackSource source, Character target,
        Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        VictoryEvaluator victoryEvaluator, TerrainType terrain,
        Func<CombatLogEntry, Task> notify)
    {
        var result = _combat.ResolveAttack(actorState.Character, target, source,
            actorState.EngagementRange, terrain);
        var empty = new List<CombatantState>();
        return await ProcessClashAsync(tick, actorState,
            new ActorSetup(source, target, 0, source is Spell),
            result, empty, stateMap, lastAttackerOf, heroParty, enemyParty, log, notify);
    }

    public async Task<CombatResult?> ProcessClashAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target = setup.Target;
        var defenderState = stateMap[target];
        var counterSource = defenderState.AttackSource ?? (IAttackSource)UnarmedStrike.Default;
        var counterDc = _combat.ResolveDamage(target, actorState.Character, counterSource);
        var counterDamage = Math.Max(0, counterDc.FinalDamage / 2);

        if (result.Damage > 0)
        {
            var defHpBefore = target.CurrentHitPoints;
            target.CurrentHitPoints -= result.Damage;
            lastAttackerOf[target] = actorState.Character;
            await notify(_logger.BuildDamageEntry(tick, target.Name, result.Damage, defHpBefore, target.CurrentHitPoints));
        }
        if (counterDamage > 0)
        {
            var atkHpBefore = actorState.Character.CurrentHitPoints;
            actorState.Character.CurrentHitPoints -= counterDamage;
            await notify(_logger.BuildDamageEntry(tick, actorState.Character.Name, counterDamage, atkHpBefore, actorState.Character.CurrentHitPoints));
        }

        await notify(new CombatLogEntry
        {
            Tick = tick, ActorName = actorState.Character.Name,
            EventType = "Clash", TargetName = target.Name,
            Message = $"[CLASH] Both weapons collide! {actorState.Character.Name} and {target.Name} exchange glancing blows."
        });

        if (actorState.Character.CurrentHitPoints <= 0)
        {
            await notify(_logger.BuildDefeatEntry(tick, actorState.Character));
            var r = _victoryEvaluator.BuildDefeatResult(tick, actorState.PartyIndex, actorState.Character, heroParty, enemyParty, log);
            if (r is not null) return r;
        }
        if (target.CurrentHitPoints <= 0)
        {
            await notify(_logger.BuildDefeatEntry(tick, target));
            var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
            var r = _victoryEvaluator.BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
            if (r is not null)
            {
                var deadState = stateMap.GetValueOrDefault(target);
                if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
                return r;
            }
        }
        return null;
    }

    public async Task<CombatResult?> ProcessHitAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, CombatantState> stateMap,
        Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target = setup.Target;
        var hpBefore = target.CurrentHitPoints;
        target.CurrentHitPoints -= result.Damage;
        lastAttackerOf[target] = actorState.Character;

        if (result.Damage > 0)
            await notify(_logger.BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

        if (result.IsDevastatingStrike)
            await notify(new CombatLogEntry
            {
                Tick = tick, ActorName = actorState.Character.Name,
                EventType = "DevastatingStrike", TargetName = target.Name,
                DamageDealt = result.Damage,
                Message = $"[DEVASTATING STRIKE] {actorState.Character.Name} shatters {target.Name}'s guard! x3 damage!"
            });

        if (target.CurrentHitPoints > 0 && setup.Source is Spell hitSpell)
            await _statusEffectProcessor.ProcessOnHitEffectsAsync(tick, actorState.Character, target, hitSpell, notify);

        await _spellProcessor.ProcessSpellDisruptionAsync(tick, setup, result, stateMap, notify);
        await _spellProcessor.ProcessConcentrationAsync(tick, target, result, stateMap, notify);

        if (target.CurrentHitPoints > 0) return null;

        await notify(_logger.BuildDefeatEntry(tick, target));
        var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
        var defResult = _victoryEvaluator.BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
        if (defResult is not null)
        {
            var deadState = stateMap.GetValueOrDefault(target);
            if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
            return defResult;
        }
        return null;
    }
}
