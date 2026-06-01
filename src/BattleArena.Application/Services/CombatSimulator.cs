namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

// Drives the turn-based combat loop for any NvN configuration (1v1, 1vN, up to 6vN).
//
//   1. Every tick all living combatants gain turnmeter (TurnSpeed + DEX mod - armor penalty).
//   2. All combatants whose meter reaches 100 act that tick, highest meter first.
//   3. Each actor picks a random living enemy via ITargetSelector and resolves an attack.
//   4. HP can go negative: 0 to -9 = knocked out, -10 or lower = dead.
//   5. A fumble applies -2 AttackPower for the fumbler's next turn.
//   6. Combat ends when one party has no living members, or maxTicks is exhausted.
//   7. Every event is recorded in a CombatLogEntry with full detail.
public class CombatSimulator : ICombatSimulator
{
    private readonly ICombatService       _combat;
    private readonly ITurnmeterService    _turnmeter;
    private readonly IStatusEffectService _statusEffect;
    private readonly IDiceService         _dice;
    private readonly ITargetSelector      _heroTargetSelector;
    private readonly ITargetSelector      _enemyTargetSelector;
    private readonly IActionDecisionSource      _heroActionSource;
    private readonly IActionDecisionSource      _enemyActionSource;

    private TerrainType _terrain = TerrainType.Plains;

    public CombatSimulator(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        IDiceService dice,
        ITargetSelector? heroTargetSelector  = null,
        ITargetSelector? enemyTargetSelector = null,
        IActionDecisionSource? heroActionSource  = null,
        IActionDecisionSource? enemyActionSource = null)
    {
        _combat              = combat;
        _turnmeter           = turnmeter;
        _statusEffect        = statusEffect;
        _dice                = dice;
        _heroTargetSelector  = heroTargetSelector  ?? new RandomTargetSelector();
        _enemyTargetSelector = enemyTargetSelector ?? new RandomTargetSelector();
        _heroActionSource    = heroActionSource    ?? new AutoActionDecisionSource(dice);
        _enemyActionSource   = enemyActionSource   ?? new AutoActionDecisionSource(dice);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    // Party-vs-party async entry point — supports 1v1, 1vN, NvN (hero side max 6).
    // Observer receives every event in real time (use for GUI animation).
    // CancellationToken allows forfeiting or time-out from the caller.
    public async Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains)
    {
        _terrain = terrain;
        const int RoundLength = 10;

        var log            = new List<CombatLogEntry>();
        var states         = BuildCombatantStates(heroParty, enemyParty);
        var currentRound   = 0;
        var lastAttackerOf = new Dictionary<Character, Character>();

        // Log + notify the observer for every event in one call.
        // Automatically stamps the currently-acting character so consumers
        // never need to track it themselves.
        async Task Notify(CombatLogEntry entry)
        {
            entry.ActiveActorName = states.FirstOrDefault(s => s.Meter.IsActive)?.Character.Name;
            log.Add(entry);
            if (observer != null)
                await observer.OnEventAsync(entry, ct);
        }

        for (var tick = 1; tick <= maxTicks; tick++)
        {
            ct.ThrowIfCancellationRequested();
            _dice.CurrentTick = tick;

            if ((tick - 1) % RoundLength == 0)
            {
                currentRound++;
                await Notify(new CombatLogEntry
                {
                    Tick = tick,
                    EventType = "RoundStart",
                    RoundNumber = currentRound,
                    Message = $"══ Round {currentRound} begins ══"
                });
            }

            await ProcessTickMeterAndManaAsync(tick, states, Notify);
            await ProcessCrowdControlledActorsAsync(tick, states, Notify);

            foreach (var actorState in GetActingOrder(states))
            {
                if (!actorState.Character.IsAlive) continue;
                var turnResult = await ProcessActingActorAsync(
                    tick, currentRound, actorState, states, lastAttackerOf,
                    heroParty, enemyParty, log, Notify, ct);
                if (turnResult is not null) return turnResult;
            }

            if (tick % RoundLength == 0)
            {
                await ExpireSummonedPetsAsync(tick, currentRound, states, Notify);

                await Notify(new CombatLogEntry
                {
                    Tick = tick,
                    EventType = "RoundEnd",
                    RoundNumber = currentRound,
                    Message = $"── Round {currentRound} ends ──"
                });
            }
        }

        return new CombatResult { MaxTicksReached = true, TotalTicks = maxTicks, Log = log, Seed = _dice.Seed, Party1 = heroParty, Party2 = enemyParty };
    }

    // 1v1 async convenience wrapper.
    public Task<CombatResult> SimulateAsync(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(
            Party.Solo(fighter,  fighterAttack),
            Party.Solo(opponent, opponentAttack),
            maxTicks, observer, ct, terrain);

    // Sync wrappers — safe for console/test contexts (no sync context).
    // Do not call from a UI thread.
    public CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = 1000,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(heroParty, enemyParty, maxTicks, terrain: terrain).GetAwaiter().GetResult();

    public CombatResult Simulate(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        TerrainType terrain = TerrainType.Plains) =>
        SimulateAsync(fighter, fighterAttack, opponent, opponentAttack, maxTicks, terrain: terrain)
            .GetAwaiter().GetResult();

    // ── Private helpers ────────────────────────────────────────────────────────

    private static List<CombatantState> BuildCombatantStates(Party heroParty, Party enemyParty)
    {
        var states = new List<CombatantState>();
        foreach (var m in heroParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 0));
        foreach (var m in enemyParty.Members)
            states.Add(new CombatantState(m.Character, m.AttackSource, partyIndex: 1));
        return states;
    }

    private CombatLogEntry BuildAfterTurnEntry(CombatantState state, int tick, int tmCost = 100)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = _turnmeter.AfterTurn(state.Meter, tmCost);
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = state.Character.Name,
            EventType       = "TurnEnd",
            TurnMeterBefore = before,
            TurnMeterAfter  = state.Meter.CurrentValue,
            IsReady         = state.Meter.IsReady,
            IsActive        = false,
            Message         = $"{state.Character.Name} ends turn.  TM: {before} -> {state.Meter.CurrentValue} (cost: {tmCost})"
        };
    }

    private static CombatLogEntry BuildTurnMeterGainEntry(int tick, CombatantState s)
    {
        var before = s.PrevMeter;
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = s.Character.Name,
            EventType       = "TurnMeterGain",
            TurnMeterBefore = before,
            TurnMeterAfter  = s.Meter.CurrentValue,
            IsReady         = s.Meter.IsReady,
            IsActive        = s.Meter.IsActive,
            Message         = $"{s.Character.Name}  TM: {before} -> {s.Meter.CurrentValue}  (+{s.Meter.CurrentValue - before})"
        };
    }

    private static CombatLogEntry BuildDefeatEntry(int tick, Character target) => new()
    {
        Tick      = tick,
        ActorName = target.Name,
        EventType = target.IsDead ? "Death" : "KnockedOut",
        Message   = target.IsDead
            ? $"[DEAD] {target.Name} has been slain! (HP: {target.CurrentHitPoints})"
            : $"{target.Name} is unconscious! (HP: {target.CurrentHitPoints})"
    };

    private static CombatLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result, DamageType damageType = DamageType.Slashing)
    {
        var outcome = result.IsDevastatingStrike ? "DEVASTATING STRIKE!!!" :
                      result.IsTotalReversal     ? "TOTAL REVERSAL!"       :
                      result.IsClash             ? "CLASH!"                :
                      result.IsPerfectParry      ? "PERFECT PARRY!"        :
                      result.IsCriticalHit       ? "CRITICAL HIT!"         :
                      result.IsFumble            ? "FUMBLE!"               :
                      result.IsHit               ? "HIT"                   : "MISS";

        var msg = $"{actorName} [{attackSourceName}] -> {targetName}: " +
                  $"d20_atk={result.HitRoll} d20_def={result.DefenseRoll} + AP={result.AttackPower} " +
                  $"vs DP={result.DefensePower} -> {outcome}";

        if (result.IsHit && result.DamageContext is { } dc)
        {
            var critTag = result.IsCriticalHit      ? " [x2 CRIT]"     :
                          result.IsDevastatingStrike ? " [x3 DEVAS]"    :
                          result.IsClash             ? " [x0.5 CLASH]"  : "";
            msg += $" | Dmg: roll({dc.WeaponDiceRoll}) + attr({dc.AttributeModifier}) + flat({dc.FlatBonuses}) + lvl({dc.LevelScaling})" +
                   $" = {dc.BaseDamage}{critTag} x{dc.TypeMultiplier:0.0} - mit({dc.ArmorMitigation}) + elem({dc.ElementalModifiers}) = {result.Damage}";
        }

        var ctx    = CombatNarrator.GetContext(
            result.HitRoll, result.HitRoll + result.AttackPower, result.DefensePower,
            result.IsHit || result.IsCriticalHit, result.IsCriticalHit, result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx, isSpell, damageType);

        return new CombatLogEntry
        {
            Tick                = tick,
            ActorName           = actorName,
            EventType           = "Attack",
            DieRoll             = result.HitRoll,
            DefenseRoll         = result.DefenseRoll,
            AttackPower         = result.AttackPower,
            DefensePower        = result.DefensePower,
            IsHit               = result.IsHit,
            IsCritical          = result.IsCriticalHit,
            IsFumble            = result.IsFumble,
            IsPerfectParry      = result.IsPerfectParry  ? true : null,
            IsClash             = result.IsClash         ? true : null,
            IsDevastatingStrike = result.IsDevastatingStrike ? true : null,
            IsTotalReversal     = result.IsTotalReversal ? true : null,
            DamageDealt         = result.Damage,
            AttackSourceName    = attackSourceName,
            IsSpell             = isSpell,
            TargetName          = targetName,
            Phrase              = phrase,
            Message             = msg
        };
    }

    private static CombatLogEntry BuildDamageEntry(
        int tick, string targetName, int damage, int hpBefore, int hpAfter) => new()
    {
        Tick           = tick,
        ActorName      = targetName,
        EventType      = "Damage",
        DamageDealt    = damage,
        TargetHpBefore = hpBefore,
        TargetHpAfter  = hpAfter,
        Message        = $"{targetName} takes {damage} damage.  HP: {hpBefore} -> {hpAfter}"
    };

    // ── Healing helpers ────────────────────────────────────────────────────────

    private async Task<CombatResult?> ProcessHealingSpellAsync(
        int tick, CombatantState actorState, ActorSetup setup, Spell spell,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        if (spell.IsGroupHeal)
        {
            var allies = states
                .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive && s.Character.CurrentHitPoints < s.Character.MaxHitPoints)
                .ToList();

            foreach (var ally in allies)
            {
                var healAmount = _combat.ResolveHealing(actorState.Character, ally.Character, spell, _terrain);
                var hpBefore = ally.Character.CurrentHitPoints;
                ally.Character.CurrentHitPoints = Math.Min(ally.Character.MaxHitPoints, hpBefore + healAmount);
                await notify(new CombatLogEntry
                {
                    Tick            = tick,
                    ActorName       = ally.Character.Name,
                    EventType       = "Healed",
                    DamageDealt     = healAmount,
                    TargetHpBefore  = hpBefore,
                    TargetHpAfter   = ally.Character.CurrentHitPoints,
                    AttackSourceName = spell.Name,
                    IsSpell         = true,
                    Message         = $"{ally.Character.Name} is healed for {healAmount} by {spell.Name}.  HP: {hpBefore} -> {ally.Character.CurrentHitPoints}"
                });
            }
            return null;
        }

        // Single-target heal: pick the ally with the lowest HP.
        var target = states
            .Where(s => s.PartyIndex == actorState.PartyIndex && s.Character.IsAlive && s.Character.CurrentHitPoints < s.Character.MaxHitPoints)
            .OrderBy(s => s.Character.CurrentHitPoints)
            .FirstOrDefault();

        if (target is null) return null;

        var heal = _combat.ResolveHealing(actorState.Character, target.Character, spell, _terrain);
        var hpB = target.Character.CurrentHitPoints;
        target.Character.CurrentHitPoints = Math.Min(target.Character.MaxHitPoints, hpB + heal);
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = target.Character.Name,
            EventType       = "Healed",
            DamageDealt     = heal,
            TargetHpBefore  = hpB,
            TargetHpAfter   = target.Character.CurrentHitPoints,
            AttackSourceName = spell.Name,
            IsSpell         = true,
            Message         = $"{target.Character.Name} is healed for {heal} by {spell.Name}.  HP: {hpB} -> {target.Character.CurrentHitPoints}"
        });
        return null;
    }

    // ── Self-buff helpers ─────────────────────────────────────────────────────

    private async Task ProcessSelfBuffsAsync(
        int tick, Character caster, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var template in spell.OnHitEffects)
        {
            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                ResistanceType       = template.ResistanceType,
                ResistanceBonuses    = template.ResistanceBonuses,
                Duration             = template.Duration,
                DamagePerTurn        = template.DamagePerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier    = template.TurnMeterModifier,
                ManaRegenModifier    = template.ManaRegenModifier,
                StackRule            = template.StackRule,
                ApplicationChance    = template.ApplicationChance,
                Source               = spell.Name
            };

            _statusEffect.Apply(caster, effect);
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = caster.Name,
                EventType        = "EffectApplied",
                StatusEffectName = effect.Name,
                AttackSourceName = spell.Name,
                IsBuff           = true,
                Message          = $"{caster.Name} gains {effect.Name} from {spell.Name}!"
            });
        }
    }

    // ── Status effect helpers ─────────────────────────────────────────────────

    private bool IsCrowdControlled(Character character) =>
        _statusEffect.HasEffectType(character, StatusEffectType.Stun) ||
        _statusEffect.HasEffectType(character, StatusEffectType.Root);

    private static string GetCrowdControlLabel(Character character)
    {
        var effect = character.ActiveStatusEffects.FirstOrDefault(
            e => e.Type is StatusEffectType.Stun or StatusEffectType.Root);
        return effect?.Type switch
        {
            StatusEffectType.Stun => "stunned",
            StatusEffectType.Root => "rooted",
            _                     => "crowd-controlled"
        };
    }

    private int RollDie(DieType die) => _dice.Roll(die);

    // ── Extracted acting-loop helpers ───────────────────────────────────────

    private async Task<CombatResult?> ProcessActorDoTAsync(
        int tick, CombatantState actorState,
        Party heroParty, Party enemyParty,
        List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var dotEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .ToList())
        {
            var dotName = dotEffect.Name;
            var dotDmg  = dotEffect.DamagePerTurn;
            actorState.Character.CurrentHitPoints -= dotDmg;

            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = actorState.Character.Name,
                EventType        = "DoTTick",
                DamageDealt      = dotDmg,
                TargetHpAfter    = actorState.Character.CurrentHitPoints,
                StatusEffectName = dotName,
                Message          = $"{actorState.Character.Name} suffers {dotDmg} {dotName} damage."
            });

            if (actorState.Character.CurrentHitPoints <= 0)
            {
                await notify(BuildDefeatEntry(tick, actorState.Character));
                var defResult = BuildDefeatResult(
                    tick, actorState.PartyIndex, actorState.Character,
                    heroParty, enemyParty, log);
                if (defResult is not null) return defResult;
            }
        }
        return null;
    }

    private async Task ProcessOnHitEffectsAsync(
        int tick, Character target, Spell spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell.OnHitEffects.Count == 0) return;

        foreach (var template in spell.OnHitEffects)
        {
            var dmgPerTurn = template.DamagePerTurn;
            if (dmgPerTurn <= 0 && template.DoTDamageCount > 0)
                for (var i = 0; i < template.DoTDamageCount; i++)
                    dmgPerTurn += RollDie(template.DoTDamageDie);

            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                ResistanceType       = template.ResistanceType,
                ResistanceBonuses    = template.ResistanceBonuses,
                Duration             = template.Duration,
                DamagePerTurn        = dmgPerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier    = template.TurnMeterModifier,
                StackRule            = template.StackRule,
                ApplicationChance    = template.ApplicationChance,
                Source               = spell.Name
            };

            var resistance = target.ComputeResistance(effect.ResistanceType);
            var appResult = _statusEffect.TryApply(target, effect, resistance, _dice);

            if (appResult.Applied)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = target.Name,
                    EventType        = "EffectApplied",
                    StatusEffectName = effect.Name,
                    Message          = $"{target.Name} is afflicted with {effect.Name}!"
                });
            }
            else if (appResult.WasResisted)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = target.Name,
                    EventType        = "EffectResisted",
                    StatusEffectName = effect.Name,
                    ResistRoll       = appResult.Roll,
                    ResistThreshold  = appResult.TotalResistance,
                    Message          = $"{target.Name} resists {effect.Name}! (rolled {appResult.Roll} vs {appResult.TotalResistance} resistance)"
                });
            }
        }
    }

    private static async Task NotifyExpiredEffectsAsync(
        int tick, Character character,
        IReadOnlyList<string> expired,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var name in expired)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = character.Name,
                EventType        = "EffectExpired",
                StatusEffectName = name,
                Message          = $"{name} has worn off {character.Name}."
            });
        }
    }

    private static async Task ExpireSummonedPetsAsync(
        int tick,
        int currentRound,
        List<CombatantState> states,
        Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states.Where(s =>
            s.IsSummoned &&
            s.Character.IsAlive &&
            s.SummonExpiryRound > 0 &&
            s.SummonExpiryRound <= currentRound).ToList())
        {
            s.Character.CurrentHitPoints = -999;
            await notify(new CombatLogEntry
            {
                Tick = tick,
                EventType = "PetExpired",
                ActorName = s.Character.Name,
                SummonedPetName = s.Character.Name,
                RoundNumber = currentRound,
                Message = $"{s.Character.Name} fades away as the summoning ends."
            });
        }
    }

    private CombatResult? BuildDefeatResult(
        int tick, int defeatedPartyIndex,
        Character defeatedCharacter,
        Party heroParty, Party enemyParty,
        List<CombatLogEntry> log)
    {
        var losingParty = defeatedPartyIndex == 0 ? heroParty : enemyParty;
        if (!losingParty.IsDefeated) return null;

        return new CombatResult
        {
            WinningParty = defeatedPartyIndex == 0 ? enemyParty : heroParty,
            LosingParty  = losingParty,
            LoserStatus  = defeatedCharacter.VitalStatus,
            TotalTicks   = tick,
            Log          = log,
            Seed         = _dice.Seed,
            Party1       = heroParty,
            Party2       = enemyParty
        };
    }

    // ── Tick-level orchestration ────────────────────────────────────────────────

    private List<CombatantState> GetActingOrder(List<CombatantState> states) =>
        states
            .Where(s => s.Character.IsAlive && s.Meter.IsReady && !IsCrowdControlled(s.Character))
            .OrderByDescending(s => s.Meter.CurrentValue)
            .ToList();

    private async Task ProcessTickMeterAndManaAsync(
        int tick, List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states.Where(s => s.Character.IsAlive))
        {
            s.SnapshotMeter();
            s.Meter = _turnmeter.Tick(s.Character, s.Meter);
            await notify(BuildTurnMeterGainEntry(tick, s));
        }
        foreach (var s in states.Where(s => s.Character.IsAlive && s.Character.MaxMana > 0))
        {
            var regen      = s.Character.ManaRegenPerTick;
            var manaBefore = s.Character.CurrentMana;
            s.Character.CurrentMana = Math.Min(s.Character.EffectiveMaxMana, manaBefore + regen);
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = s.Character.Name,
                EventType = "ManaRegen",
                ManaRegen = regen,
                ManaAfter = s.Character.CurrentMana,
                Message   = $"{s.Character.Name} regens {regen} mana  ({manaBefore} -> {s.Character.CurrentMana})"
            });
        }
        foreach (var s in states.Where(s => s.QueuedSpell is not null && s.Character.IsAlive))
            s.QueuedSpell!.RemainingCost -= _turnmeter.ComputeGainPerTick(s.Character);
    }

    private async Task ProcessCrowdControlledActorsAsync(
        int tick, List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        foreach (var s in states
            .Where(s => s.Character.IsAlive && s.Meter.IsReady && IsCrowdControlled(s.Character))
            .ToList())
        {
            var expired = _statusEffect.TickAll(s.Character);
            await NotifyExpiredEffectsAsync(tick, s.Character, expired, notify);

            if (s.QueuedSpell is not null)
            {
                await notify(new CombatLogEntry
                {
                    Tick             = tick,
                    ActorName        = s.Character.Name,
                    EventType        = "SpellLost",
                    AttackSourceName = s.QueuedSpell.Spell.Name,
                    Message          = $"{s.Character.Name} loses concentration on {s.QueuedSpell.Spell.Name} — crowd controlled!"
                });
                s.QueuedSpell = null;
            }

            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = s.Character.Name,
                EventType = "SkippedTurn",
                Message   = $"{s.Character.Name} is {GetCrowdControlLabel(s.Character)} and cannot act!"
            });
        }
    }

    // ── Per-actor turn orchestration ────────────────────────────────────────────

    private async Task<CombatResult?> ProcessActingActorAsync(
        int tick, int currentRound, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var setup = await SetupActorAttackAsync(tick, actorState, states, lastAttackerOf, notify, ct);
        if (setup is null) return null;

        actorState.Meter.IsActive = true;
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            EventType        = "TurnStart",
            TurnMeterBefore  = actorState.Meter.CurrentValue,
            IsReady          = true,
            IsActive         = true,
            AttackSourceName = setup.Source.Name,
            IsSpell          = setup.IsSpell,
            TargetName       = setup.Target.Name,
            Message          = $"{actorState.Character.Name} takes their turn  (TM: {actorState.Meter.CurrentValue})"
        });

        if (await TryHandlePetSummonAsync(tick, actorState, setup, states, currentRound, notify))
        {
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return null;
        }

        var dotResult = await ProcessActorDoTAsync(tick, actorState, heroParty, enemyParty, log, notify);
        if (dotResult is not null)
        {
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return dotResult;
        }

        var expired = _statusEffect.TickAll(actorState.Character);
        await NotifyExpiredEffectsAsync(tick, actorState.Character, expired, notify);

        // ── Healing spells take a different path ──────────────────────────
        if (setup.Source is Spell castSpell && castSpell.IsHealing)
        {
            var healResult = await ProcessHealingSpellAsync(tick, actorState, setup, castSpell, states, notify);
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return healResult;
        }

        var result = _combat.ResolveAttack(actorState.Character, setup.Target, setup.Source, actorState.EngagementRange, _terrain);
        await notify(BuildAttackEntry(tick, actorState.Character.Name, setup.Source.Name, setup.IsSpell, setup.Target.Name, result, setup.Source.DamageType));

        var outcome = await ResolveAttackOutcomeAsync(
            tick, actorState, setup, result, states, lastAttackerOf,
            heroParty, enemyParty, log, notify);
        if (outcome is not null)
        {
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
            return outcome;
        }

        await ApplyDefenderTmBoostAsync(tick, actorState, setup.Target, result, states, notify);
        await ApplyFumblePenaltyAsync(tick, actorState, result, notify);

        // ── Self-buffs from protective spells ────────────────────────────
        if (setup.Source is Spell spellWithBuffs && spellWithBuffs.OnHitEffects.Count > 0)
            await ProcessSelfBuffsAsync(tick, actorState.Character, spellWithBuffs, notify);

        actorState.Meter.IsActive = false;
        await notify(BuildAfterTurnEntry(actorState, tick, setup.TmCost));
        return null;
    }

    // ── Attack setup (queued-spell path vs new-attack path) ─────────────────────

    private sealed record ActorSetup(IAttackSource Source, Character Target, int TmCost, bool IsSpell);

    private async Task<ActorSetup?> SetupActorAttackAsync(
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
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "SpellCharging",
                Message   = $"{actorState.Character.Name} is charging {qs.Spell.Name}  (need {qs.RemainingCost} more TM)"
            });
            return null;
        }

        actorState.QueuedSpell = null;
        var target = qs.Target;

        if (!target.IsAlive)
        {
            var liveEnemies = states
                .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                .ToList();
            if (liveEnemies.Count == 0) return null;
            var reSelector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
            target = await reSelector.SelectTargetAsync(
                actorState.Character, liveEnemies.Select(s => s.Character), ct);
        }

        await DeductManaCostAsync(tick, actorState, qs.Spell, notify);
        return new ActorSetup(qs.Spell, target, 100, IsSpell: true);
    }

    private async Task<ActorSetup?> HandleNewAttackSetupAsync(
        int tick, CombatantState actorState,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var enemies = states
            .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
            .ToList();
        if (enemies.Count == 0) return null;

        var decisionSource = actorState.PartyIndex == 0 ? _heroActionSource : _enemyActionSource;
        var attackSource = await decisionSource.ChooseAttackAsync(
            actorState.Character,
            actorState.AttackSource,
            enemies.Select(s => s.Character).ToList(),
            tick,
            ct);

        if (attackSource is null)
        {
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "SkippedTurn",
                Message   = $"{actorState.Character.Name} skips their turn."
            });
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, 100));
            return null;
        }

        if (attackSource is MoveIntent)
        {
            var speed = actorState.Character.EffectiveMovementSpeed;
            var from = actorState.EngagementRange;
            actorState.EngagementRange = from switch
            {
                EngagementRange.Melee => EngagementRange.Short,
                EngagementRange.Long => EngagementRange.Short,
                EngagementRange.Short => EngagementRange.Melee,
                _ => EngagementRange.Melee
            };
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "Move",
                Message   = $"{actorState.Character.Name} moves 15 ft ({from} → {actorState.EngagementRange}). Speed: {speed} ft"
            });
            actorState.Meter.IsActive = false;
            await notify(BuildAfterTurnEntry(actorState, tick, 100));
            return null;
        }

        var isSpell = attackSource is Spell;
        var meterNow = actorState.Meter.CurrentValue;
        var tmCost = isSpell ? actorState.Character.ComputeSpellTurnMeterCost((Spell)attackSource) : 100;

        if (!isSpell && actorState.Character.MemorizedSpells.Count > 0)
            await notify(new CombatLogEntry
            {
                Tick      = tick,
                ActorName = actorState.Character.Name,
                EventType = "InsufficientMana",
                Message   = $"{actorState.Character.Name} lacks mana for spells — resorting to unarmed strike!"
            });

        if (isSpell && meterNow < tmCost)
        {
            await QueueSpellAsync(tick, actorState, (Spell)attackSource, enemies, tmCost, meterNow, notify, ct);
            return null;
        }

        var target = await SelectActorTargetAsync(actorState, enemies, lastAttackerOf, ct);
        await DeductManaCostAsync(tick, actorState, isSpell ? (Spell)attackSource : null, notify);
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

    private async Task QueueSpellAsync(
        int tick, CombatantState actorState, Spell spell,
        List<CombatantState> enemies, int tmCost, int meterNow,
        Func<CombatLogEntry, Task> notify, CancellationToken ct)
    {
        var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
        var target   = await selector.SelectTargetAsync(
            actorState.Character, enemies.Select(s => s.Character), ct);
        actorState.QueuedSpell = new QueuedSpellInfo(spell, target, tmCost - meterNow);
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            EventType        = "SpellQueued",
            AttackSourceName = spell.Name,
            TargetName       = target.Name,
            TurnMeterBefore  = meterNow,
            IsSpell          = true,
            Message          = $"{actorState.Character.Name} begins charging {spell.Name} on {target.Name}  (need {tmCost - meterNow} more TM)"
        });
    }

    private async Task DeductManaCostAsync(
        int tick, CombatantState actorState, Spell? spell,
        Func<CombatLogEntry, Task> notify)
    {
        if (spell is null || spell.ManaCost <= 0) return;
        var before = actorState.Character.CurrentMana;
        actorState.Character.CurrentMana = Math.Max(0, before - spell.ManaCost);
        await notify(new CombatLogEntry
        {
            Tick             = tick,
            ActorName        = actorState.Character.Name,
            EventType        = "ManaDeduct",
            ManaCost         = spell.ManaCost,
            ManaAfter        = actorState.Character.CurrentMana,
            AttackSourceName = spell.Name,
            Message          = $"{actorState.Character.Name} spends {spell.ManaCost} mana to cast {spell.Name}. ({before} -> {actorState.Character.CurrentMana})"
        });
    }

    // ── Pet summoning ────────────────────────────────────────────────────────────

    private async Task<bool> TryHandlePetSummonAsync(
        int tick, CombatantState actorState, ActorSetup setup,
        List<CombatantState> states, int currentRound,
        Func<CombatLogEntry, Task> notify)
    {
        if (!setup.IsSpell || setup.Source is not Spell castSpell || castSpell.SummonedPet is null)
            return false;

        var pet         = castSpell.SummonedPet;
        var expiryRound = pet.SummonDurationRounds > 0 ? currentRound + pet.SummonDurationRounds : 0;

        var petChar = new Character
        {
            Name             = pet.Name,
            MaxHitPoints     = pet.MaxHitPoints,
            CurrentHitPoints = pet.MaxHitPoints,
            StrikeRating     = pet.StrikeRating,
            TurnSpeed        = pet.TurnSpeed,
            Strength         = pet.Strength,
            Level            = 1,
            ClassId          = 8,
            Equipment        = new ArmorSlots
            {
                Chest = new Armor
                {
                    Name              = $"{pet.Name} Hide",
                    ArmorClass        = pet.ArmorClass,
                    MaxDexterityBonus = 6
                }
            }
        };
        var petWeapon = new Weapon
        {
            Name        = $"{pet.Name}'s Attack",
            DamageDie   = pet.DamageDie,
            DamageCount = pet.DamageCount,
            AttackBonus = pet.AttackBonus,
            DamageType  = pet.DamageType,
            AttackType  = AttackType.Melee,
        };
        states.Add(new CombatantState(petChar, petWeapon, actorState.PartyIndex)
        {
            SummonedBy        = actorState.Character,
            SummonExpiryRound = expiryRound,
        });

        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = actorState.Character.Name,
            EventType       = "PetSummoned",
            SummonedPetName = pet.Name,
            RoundNumber     = currentRound,
            Message         = $"{actorState.Character.Name} summons {pet.Name}!" +
                              (expiryRound > 0 ? $"  (lasts until end of round {expiryRound})" : "  (until slain)")
        });
        return true;
    }

    // ── Attack outcome dispatch ──────────────────────────────────────────────────

    private async Task<CombatResult?> ResolveAttackOutcomeAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        if (result.IsClash)
            return await ProcessClashAsync(
                tick, actorState, setup, result, states, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        if (result.IsHit)
            return await ProcessHitAsync(
                tick, actorState, setup, result, states, lastAttackerOf,
                heroParty, enemyParty, log, notify);
        return null;
    }

    private async Task<CombatResult?> ProcessClashAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target        = setup.Target;
        var defenderState = states.First(s => s.Character == target);
        var counterSource = defenderState.AttackSource ?? (IAttackSource)UnarmedStrike.Default;
        var counterDc     = _combat.ResolveDamage(target, actorState.Character, counterSource);
        var counterDamage = Math.Max(0, counterDc.FinalDamage / 2);

        if (result.Damage > 0)
        {
            var defHpBefore = target.CurrentHitPoints;
            target.CurrentHitPoints -= result.Damage;
            lastAttackerOf[target] = actorState.Character;
            await notify(BuildDamageEntry(tick, target.Name, result.Damage, defHpBefore, target.CurrentHitPoints));
        }
        if (counterDamage > 0)
        {
            var atkHpBefore = actorState.Character.CurrentHitPoints;
            actorState.Character.CurrentHitPoints -= counterDamage;
            await notify(BuildDamageEntry(tick, actorState.Character.Name, counterDamage, atkHpBefore, actorState.Character.CurrentHitPoints));
        }

        await notify(new CombatLogEntry
        {
            Tick       = tick,
            ActorName  = actorState.Character.Name,
            EventType  = "Clash",
            TargetName = target.Name,
            Message    = $"[CLASH] Both weapons collide! {actorState.Character.Name} and {target.Name} exchange glancing blows."
        });

        if (actorState.Character.CurrentHitPoints <= 0)
        {
            await notify(BuildDefeatEntry(tick, actorState.Character));
            var r = BuildDefeatResult(tick, actorState.PartyIndex, actorState.Character, heroParty, enemyParty, log);
            if (r is not null) return r;
        }
        if (target.CurrentHitPoints <= 0)
        {
            await notify(BuildDefeatEntry(tick, target));
            var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
            var r = BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
            if (r is not null)
            {
                var deadState = states.FirstOrDefault(s => s.Character == target);
                if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
                return r;
            }
        }
        return null;
    }

    private async Task<CombatResult?> ProcessHitAsync(
        int tick, CombatantState actorState, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Dictionary<Character, Character> lastAttackerOf,
        Party heroParty, Party enemyParty, List<CombatLogEntry> log,
        Func<CombatLogEntry, Task> notify)
    {
        var target   = setup.Target;
        var hpBefore = target.CurrentHitPoints;
        target.CurrentHitPoints -= result.Damage;
        lastAttackerOf[target] = actorState.Character;

        if (result.Damage > 0)
            await notify(BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

        if (result.IsDevastatingStrike)
            await notify(new CombatLogEntry
            {
                Tick        = tick,
                ActorName   = actorState.Character.Name,
                EventType   = "DevastatingStrike",
                TargetName  = target.Name,
                DamageDealt = result.Damage,
                Message     = $"[DEVASTATING STRIKE] {actorState.Character.Name} shatters {target.Name}'s guard! x3 damage!"
            });

        if (setup.Source is Spell hitSpell)
            await ProcessOnHitEffectsAsync(tick, target, hitSpell, notify);

        await ProcessSpellDisruptionAsync(tick, setup, result, states, notify);
        await ProcessConcentrationAsync(tick, target, result, states, notify);

        if (target.CurrentHitPoints > 0) return null;

        await notify(BuildDefeatEntry(tick, target));
        var targetPartyIdx = actorState.PartyIndex == 0 ? 1 : 0;
        var defResult = BuildDefeatResult(tick, targetPartyIdx, target, heroParty, enemyParty, log);
        if (defResult is not null)
        {
            var deadState = states.FirstOrDefault(s => s.Character == target);
            if (deadState?.QueuedSpell is not null) deadState.QueuedSpell = null;
            return defResult;
        }
        return null;
    }

    private async Task ProcessSpellDisruptionAsync(
        int tick, ActorSetup setup, AttackResult result,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        if (setup.Source.AttackType != AttackType.Melee) return;
        if (result.Damage <= 0 || setup.Target.MemorizedSpells.Count == 0) return;
        var targetState = states.First(s => s.Character == setup.Target);
        await TryApplySpellDisruptionAsync(tick, targetState, notify);
    }

    private async Task TryApplySpellDisruptionAsync(
        int tick, CombatantState targetState, Func<CombatLogEntry, Task> notify)
    {
        if (targetState.Meter.CurrentValue <= 0) return;
        if (_dice.Roll(DieType.D100) > 20) return;
        var tmLoss = Math.Min(25, targetState.Meter.CurrentValue);
        var before = targetState.Meter.CurrentValue;
        targetState.Meter.CurrentValue -= tmLoss;
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = targetState.Character.Name,
            EventType       = "SpellDisrupted",
            TurnMeterBefore = before,
            TurnMeterAfter  = targetState.Meter.CurrentValue,
            Message         = $"{targetState.Character.Name}'s spellcasting is disrupted! TM reduced by {tmLoss}."
        });
    }

    private async Task ProcessConcentrationAsync(
        int tick, Character target, AttackResult result,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        if (result.Damage <= 0) return;
        var concState = states.FirstOrDefault(s => s.Character == target);
        if (concState?.QueuedSpell is null) return;
        var dc   = Math.Max(10, result.Damage / 2);
        var roll = _dice.Roll(DieType.D20) + concState.Character.Level;
        if (roll < dc)
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "SpellLost",
                AttackSourceName = concState.QueuedSpell.Spell.Name,
                Message          = $"{target.Name} loses concentration on {concState.QueuedSpell.Spell.Name}! (rolled {roll} vs DC {dc})"
            });
            concState.QueuedSpell = null;
        }
        else
        {
            await notify(new CombatLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "ConcentrationPass",
                AttackSourceName = concState.QueuedSpell.Spell.Name,
                Message          = $"{target.Name} maintains concentration on {concState.QueuedSpell.Spell.Name}. (rolled {roll} vs DC {dc})"
            });
        }
    }

    // ── Miss consequences ────────────────────────────────────────────────────────

    private async Task ApplyDefenderTmBoostAsync(
        int tick, CombatantState actorState, Character target, AttackResult result,
        List<CombatantState> states, Func<CombatLogEntry, Task> notify)
    {
        if (!result.IsPerfectParry && !result.IsTotalReversal) return;
        var defenderState = states.First(s => s.Character == target);
        var tmBefore      = defenderState.Meter.CurrentValue;
        defenderState.Meter.CurrentValue += result.DefenderTmBonus;
        var eventType = result.IsTotalReversal ? "TotalReversal" : "PerfectParry";
        var msg       = result.IsTotalReversal
            ? $"[TOTAL REVERSAL] {target.Name} capitalises on {actorState.Character.Name}'s fumble! +{result.DefenderTmBonus} TM. ({tmBefore} -> {defenderState.Meter.CurrentValue})"
            : $"[PERFECT PARRY] {target.Name} deflects {actorState.Character.Name}'s attack! +{result.DefenderTmBonus} TM. ({tmBefore} -> {defenderState.Meter.CurrentValue})";
        await notify(new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = target.Name,
            EventType       = eventType,
            TurnMeterBefore = tmBefore,
            TurnMeterAfter  = defenderState.Meter.CurrentValue,
            Message         = msg
        });
    }

    private async Task ApplyFumblePenaltyAsync(
        int tick, CombatantState actorState, AttackResult result,
        Func<CombatLogEntry, Task> notify)
    {
        if (!result.IsFumble) return;
        var penaltyName = result.IsTotalReversal ? "Total Reversal Penalty" : "Fumble Penalty";
        _statusEffect.Apply(actorState.Character, new StatusEffect
        {
            Name                = penaltyName,
            Type                = StatusEffectType.Debuff,
            AttackPowerModifier = result.AttackPowerPenalty,
            Duration            = 1,
            StackRule           = StackRule.NoStack,
            Source              = "Fumble"
        });
        if (result.IsTotalReversal) return;
        await notify(new CombatLogEntry
        {
            Tick      = tick,
            ActorName = actorState.Character.Name,
            EventType = "FumblePenalty",
            Message   = $"[FUMBLE] {actorState.Character.Name} fumbles! -2 AttackPower applied for next turn."
        });
    }

    // Tracks a spell being charged over multiple ticks.
    private class QueuedSpellInfo
    {
        public Spell     Spell         { get; }
        public Character Target        { get; set; }
        public int       RemainingCost { get; set; }

        public QueuedSpellInfo(Spell spell, Character target, int remainingCost)
        {
            Spell         = spell;
            Target        = target;
            RemainingCost = remainingCost;
        }
    }

    // Tracks per-combatant state during a simulation run.
    private class CombatantState
    {
        public Character         Character         { get; }
        public IAttackSource?    AttackSource      { get; }
        public int               PartyIndex        { get; }   // 0 = hero party, 1 = enemy party
        public TurnmeterState    Meter             { get; set; }
        public int               PrevMeter         { get; set; }  // value before this tick's gain
        public QueuedSpellInfo?  QueuedSpell       { get; set; }
        public Character?        SummonedBy        { get; set; }
        public int               SummonExpiryRound { get; set; }
        public bool              IsSummoned        => SummonedBy is not null;

        /// <summary>
        /// Distance to the current target. Defaults to <see cref="EngagementRange.Melee"/>.
        /// Will be set by the distance system once position tracking is implemented,
        /// enabling ranged-at-distance bonuses and melee-out-of-reach penalties.
        /// </summary>
        public EngagementRange EngagementRange { get; set; } = EngagementRange.Melee;

        public CombatantState(Character character, IAttackSource? attackSource, int partyIndex)
        {
            Character    = character;
            AttackSource = attackSource;
            PartyIndex   = partyIndex;
            Meter        = new TurnmeterState { CharacterId = character.Id, CharacterName = character.Name };
        }

        // Called at the start of each tick before Tick() is applied.
        public void SnapshotMeter() => PrevMeter = Meter.CurrentValue;
    }
}
