namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

// Drives the turn-based battle loop for any NvN configuration (1v1, 1vN, up to 6vN).
//
//   1. Every tick all living combatants gain turnmeter (TurnSpeed + DEX mod - armor penalty).
//   2. All combatants whose meter reaches 100 act that tick, highest meter first.
//   3. Each actor picks a random living enemy via ITargetSelector and resolves an attack.
//   4. HP can go negative: 0 to -9 = knocked out, -10 or lower = dead.
//   5. A fumble applies -2 AttackPower for the fumbler's next turn.
//   6. Battle ends when one party has no living members, or maxTicks is exhausted.
//   7. Every event is recorded in a BattleLogEntry with full detail.
public class BattleSimulator : IBattleSimulator
{
    private readonly ICombatService       _combat;
    private readonly ITurnmeterService    _turnmeter;
    private readonly IStatusEffectService _statusEffect;
    private readonly IDiceService         _dice;
    // Separate selectors so hero and enemy parties can use different strategies.
    private readonly ITargetSelector      _heroTargetSelector;
    private readonly ITargetSelector      _enemyTargetSelector;

    public BattleSimulator(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        IDiceService dice,
        ITargetSelector? heroTargetSelector  = null,
        ITargetSelector? enemyTargetSelector = null)
    {
        _combat              = combat;
        _turnmeter           = turnmeter;
        _statusEffect        = statusEffect;
        _dice                = dice;
        _heroTargetSelector  = heroTargetSelector  ?? new RandomTargetSelector();
        _enemyTargetSelector = enemyTargetSelector ?? new RandomTargetSelector();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    // Party-vs-party async entry point — supports 1v1, 1vN, NvN (hero side max 6).
    // Observer receives every event in real time (use for GUI animation).
    // CancellationToken allows forfeiting or time-out from the caller.
    public async Task<BattleResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = 1000,
        IBattleObserver? observer = null,
        CancellationToken ct = default)
    {
        var log    = new List<BattleLogEntry>();
        var states = BuildCombatantStates(heroParty, enemyParty);

        // Log + notify the observer for every event in one call.
        async Task Notify(BattleLogEntry entry)
        {
            log.Add(entry);
            if (observer != null)
                await observer.OnEventAsync(entry, ct);
        }

        for (var tick = 1; tick <= maxTicks; tick++)
        {
            ct.ThrowIfCancellationRequested();

            // ── TICK: advance meters for every living combatant ────────────────
            foreach (var s in states.Where(s => s.Character.IsAlive))
            {
                s.SnapshotMeter();
                s.Meter = _turnmeter.Tick(s.Character, s.Meter);
                await Notify(BuildTurnMeterGainEntry(tick, s));
            }

            // ── ACTING ORDER: all ready combatants not CC'd, highest meter first ──
            var acting = states
                .Where(s => s.Character.IsAlive && s.Meter.IsReady && !IsCrowdControlled(s.Character))
                .OrderByDescending(s => s.Meter.CurrentValue)
                .ToList();

            // Log skipped turns for CC'd characters who are ready but cannot act
            foreach (var s in states.Where(s =>
                s.Character.IsAlive && s.Meter.IsReady && IsCrowdControlled(s.Character)))
            {
                await Notify(new BattleLogEntry
                {
                    Tick      = tick,
                    ActorName = s.Character.Name,
                    EventType = "SkippedTurn",
                    Message   = $"{s.Character.Name} is {GetCrowdControlLabel(s.Character)} and cannot act!"
                });
            }

            if (acting.Count == 0) continue;

            foreach (var actorState in acting)
            {
                if (!actorState.Character.IsAlive) continue; // killed earlier this tick

                var enemies = states
                    .Where(s => s.PartyIndex != actorState.PartyIndex && s.Character.IsAlive)
                    .ToList();

                if (enemies.Count == 0) break; // party already wiped this tick

                // ── RESOLVE ATTACK SOURCE ──────────────────────────────────────
                var attackSource = ResolveAttackSource(actorState);

                actorState.Meter.IsActive = true;

                var meterNow = actorState.Meter.CurrentValue;
                var isSpell  = attackSource is Spell;

                // ── TARGET SELECTION ───────────────────────────────────────────
                // Selected before TurnStart is logged so TargetName can be stamped.
                // Heroes (partyIndex 0) use the hero selector; enemies use the enemy selector.
                // GUI implementations await player input here; AI returns immediately.
                var selector = actorState.PartyIndex == 0 ? _heroTargetSelector : _enemyTargetSelector;
                var target   = await selector.SelectTargetAsync(
                    actorState.Character,
                    enemies.Select(s => s.Character),
                    ct);

                await Notify(new BattleLogEntry
                {
                    Tick             = tick,
                    ActorName        = actorState.Character.Name,
                    EventType        = "TurnStart",
                    TurnMeterBefore  = meterNow,
                    IsReady          = true,
                    IsActive         = true,
                    AttackSourceName = attackSource.Name,
                    IsSpell          = isSpell,
                    TargetName       = target.Name,
                    Message          = $"{actorState.Character.Name} takes their turn  (TM: {meterNow})"
                });

                // ── DOT TICK: DamageOverTime at start of actor's turn ────────────
                var dotResult = await ProcessActorDoTAsync(tick, actorState, heroParty, enemyParty, log, Notify);
                if (dotResult is not null)
                {
                    actorState.Meter.IsActive = false;
                    await Notify(BuildAfterTurnEntry(actorState, tick));
                    return dotResult;
                }

                // ── TICK ALL EFFECTS (decrement durations) ────────────────────
                var expired = _statusEffect.TickAll(actorState.Character);
                await NotifyExpiredEffectsAsync(tick, actorState.Character, expired, Notify);

                // ── ATTACK RESOLUTION ──────────────────────────────────────────
                var result = _combat.ResolveAttack(actorState.Character, target, attackSource);
                await Notify(BuildAttackEntry(tick, actorState.Character.Name, attackSource.Name, isSpell, target.Name, result, attackSource.DamageType));

                // ── APPLY DAMAGE ───────────────────────────────────────────────
                if (result.IsHit)
                {
                    var hpBefore = target.CurrentHitPoints;
                    target.CurrentHitPoints -= result.Damage;

                    // Only emit a Damage event when damage actually got through.
                    // A 0-damage hit (fully absorbed by armor mitigation) is already
                    // communicated by the Attack event's IsHit=true — no separate entry.
                    if (result.Damage > 0)
                        await Notify(BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

                    // ── ON-HIT EFFECTS (spell after-effects) ──────────────────
                    if (attackSource is Spell spell)
                        await ProcessOnHitEffectsAsync(tick, target, spell, Notify);

                    if (target.CurrentHitPoints <= 0)
                    {
                        await Notify(BuildDefeatEntry(tick, target));
                        var targetPartyIndex = actorState.PartyIndex == 0 ? 1 : 0;
                        var defResult = BuildDefeatResult(tick, targetPartyIndex, target, heroParty, enemyParty, log);
                        if (defResult is not null)
                        {
                            actorState.Meter.IsActive = false;
                            await Notify(BuildAfterTurnEntry(actorState, tick));
                            return defResult;
                        }
                    }
                }

                // ── FUMBLE PENALTY ─────────────────────────────────────────────
                if (result.IsFumble)
                {
                    _statusEffect.Apply(actorState.Character, new StatusEffect
                    {
                        Name                = "Fumble Penalty",
                        Type                = StatusEffectType.Debuff,
                        AttackPowerModifier = -2,
                        Duration            = 1,
                        StackRule           = StackRule.NoStack,
                        Source              = "Fumble"
                    });
                    await Notify(new BattleLogEntry
                    {
                        Tick      = tick,
                        ActorName = actorState.Character.Name,
                        EventType = "FumblePenalty",
                        Message   = $"[FUMBLE] {actorState.Character.Name} fumbles! -2 AttackPower applied for next turn."
                    });
                }

                // ── END TURN ───────────────────────────────────────────────────
                actorState.Meter.IsActive = false;
                await Notify(BuildAfterTurnEntry(actorState, tick));
            }
        }

        return new BattleResult { MaxTicksReached = true, TotalTicks = maxTicks, Log = log };
    }

    // 1v1 async convenience wrapper.
    public Task<BattleResult> SimulateAsync(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        IBattleObserver? observer = null,
        CancellationToken ct = default) =>
        SimulateAsync(
            Party.Solo(fighter,  fighterAttack),
            Party.Solo(opponent, opponentAttack),
            maxTicks, observer, ct);

    // Sync wrappers — safe for console/test contexts (no sync context).
    // Do not call from a UI thread.
    public BattleResult Simulate(Party heroParty, Party enemyParty, int maxTicks = 1000) =>
        SimulateAsync(heroParty, enemyParty, maxTicks).GetAwaiter().GetResult();

    public BattleResult Simulate(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000) =>
        SimulateAsync(fighter, fighterAttack, opponent, opponentAttack, maxTicks)
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

    private static IAttackSource ResolveAttackSource(CombatantState state)
    {
        if (state.AttackSource is not null) return state.AttackSource;

        var spells = state.Character.MemorizedSpells;
        if (spells.Count == 0)
            throw new InvalidOperationException(
                $"{state.Character.Name} has no weapon or memorized spells.");

        return spells[Random.Shared.Next(spells.Count)];
    }

    private BattleLogEntry BuildAfterTurnEntry(CombatantState state, int tick)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = _turnmeter.AfterTurn(state.Meter);
        return new BattleLogEntry
        {
            Tick            = tick,
            ActorName       = state.Character.Name,
            EventType       = "TurnEnd",
            TurnMeterBefore = before,
            TurnMeterAfter  = state.Meter.CurrentValue,
            IsReady         = state.Meter.IsReady,
            IsActive        = false,
            Message         = $"{state.Character.Name} ends turn.  TM: {before} -> {state.Meter.CurrentValue}"
        };
    }

    private static BattleLogEntry BuildTurnMeterGainEntry(int tick, CombatantState s)
    {
        var before = s.PrevMeter;
        return new BattleLogEntry
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

    private static BattleLogEntry BuildDefeatEntry(int tick, Character target) => new()
    {
        Tick      = tick,
        ActorName = target.Name,
        EventType = target.IsDead ? "Death" : "KnockedOut",
        Message   = target.IsDead
            ? $"[DEAD] {target.Name} has been slain! (HP: {target.CurrentHitPoints})"
            : $"{target.Name} is unconscious! (HP: {target.CurrentHitPoints})"
    };

    private static BattleLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result, DamageType damageType = DamageType.Slashing)
    {
        var outcome = result.IsCriticalHit ? "CRITICAL HIT!" :
                      result.IsFumble      ? "FUMBLE!"       :
                      result.IsHit         ? "HIT"           : "MISS";

        var msg = $"{actorName} [{attackSourceName}] -> {targetName}: " +
                  $"d20={result.HitRoll} + AP={result.AttackPower} = {result.HitRoll + result.AttackPower} " +
                  $"vs DP={result.DefensePower} -> {outcome}";

        if (result.IsHit && result.DamageContext is { } dc)
        {
            var critTag = result.IsCriticalHit ? " [x2 CRIT]" : "";
            msg += $" | Dmg: roll({dc.WeaponDiceRoll}) + attr({dc.AttributeModifier}) + flat({dc.FlatBonuses})" +
                   $" = {dc.BaseDamage}{critTag} x{dc.TypeMultiplier:0.0} - mit({dc.ArmorMitigation}) + elem({dc.ElementalModifiers}) = {dc.FinalDamage}";
        }

        var ctx    = CombatNarrator.GetContext(
            result.HitRoll, result.HitRoll + result.AttackPower, result.DefensePower,
            result.IsHit || result.IsCriticalHit, result.IsCriticalHit, result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx, isSpell, damageType);

        return new BattleLogEntry
        {
            Tick             = tick,
            ActorName        = actorName,
            EventType        = "Attack",
            DieRoll          = result.HitRoll,
            AttackPower      = result.AttackPower,
            DefensePower     = result.DefensePower,
            IsHit            = result.IsHit,
            IsCritical       = result.IsCriticalHit,
            IsFumble         = result.IsFumble,
            DamageDealt      = result.Damage,
            AttackSourceName = attackSourceName,
            IsSpell          = isSpell,
            TargetName       = targetName,
            Phrase           = phrase,
            Message          = msg
        };
    }

    private static BattleLogEntry BuildDamageEntry(
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

    private int RollDie(DieType die) => die switch
    {
        DieType.D4  => Random.Shared.Next(1, 5),
        DieType.D6  => Random.Shared.Next(1, 7),
        DieType.D8  => Random.Shared.Next(1, 9),
        DieType.D10 => Random.Shared.Next(1, 11),
        DieType.D12 => Random.Shared.Next(1, 13),
        DieType.D20 => Random.Shared.Next(1, 21),
        _           => 0
    };

    // ── Extracted acting-loop helpers ───────────────────────────────────────

    private async Task<BattleResult?> ProcessActorDoTAsync(
        int tick, CombatantState actorState,
        Party heroParty, Party enemyParty,
        List<BattleLogEntry> log,
        Func<BattleLogEntry, Task> notify)
    {
        foreach (var dotEffect in actorState.Character.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .ToList())
        {
            var dotName = dotEffect.Name;
            var dotDmg  = dotEffect.DamagePerTurn;
            actorState.Character.CurrentHitPoints -= dotDmg;

            await notify(new BattleLogEntry
            {
                Tick             = tick,
                ActorName        = actorState.Character.Name,
                EventType        = "DoTTick",
                DamageDealt      = dotDmg,
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
        Func<BattleLogEntry, Task> notify)
    {
        if (spell.OnHitEffects.Count == 0) return;

        foreach (var template in spell.OnHitEffects)
        {
            if (Random.Shared.Next(100) >= template.ApplicationChance)
                continue;

            var dmgPerTurn = template.DamagePerTurn;
            if (dmgPerTurn <= 0 && template.DoTDamageCount > 0)
                for (var i = 0; i < template.DoTDamageCount; i++)
                    dmgPerTurn += RollDie(template.DoTDamageDie);

            var effect = new StatusEffect
            {
                Name                 = template.Name,
                Type                 = template.Type,
                Duration             = template.Duration,
                DamagePerTurn        = dmgPerTurn,
                AttackPowerModifier  = template.AttackPowerModifier,
                DefensePowerModifier = template.DefensePowerModifier,
                TurnMeterModifier    = template.TurnMeterModifier,
                StackRule            = template.StackRule,
                Source               = spell.Name
            };
            _statusEffect.Apply(target, effect);

            await notify(new BattleLogEntry
            {
                Tick             = tick,
                ActorName        = target.Name,
                EventType        = "EffectApplied",
                StatusEffectName = effect.Name,
                Message          = $"{target.Name} is afflicted with {effect.Name}!"
            });
        }
    }

    private static async Task NotifyExpiredEffectsAsync(
        int tick, Character character,
        IReadOnlyList<string> expired,
        Func<BattleLogEntry, Task> notify)
    {
        foreach (var name in expired)
        {
            await notify(new BattleLogEntry
            {
                Tick             = tick,
                ActorName        = character.Name,
                EventType        = "EffectExpired",
                StatusEffectName = name,
                Message          = $"{name} has worn off {character.Name}."
            });
        }
    }

    private static BattleResult? BuildDefeatResult(
        int tick, int defeatedPartyIndex,
        Character defeatedCharacter,
        Party heroParty, Party enemyParty,
        List<BattleLogEntry> log)
    {
        var losingParty = defeatedPartyIndex == 0 ? heroParty : enemyParty;
        if (!losingParty.IsDefeated) return null;

        return new BattleResult
        {
            WinningParty = defeatedPartyIndex == 0 ? enemyParty : heroParty,
            LosingParty  = losingParty,
            LoserStatus  = defeatedCharacter.VitalStatus,
            TotalTicks   = tick,
            Log          = log
        };
    }

    // Tracks per-combatant state during a simulation run.
    private class CombatantState
    {
        public Character      Character    { get; }
        public IAttackSource? AttackSource { get; }
        public int            PartyIndex   { get; }   // 0 = hero party, 1 = enemy party
        public TurnmeterState Meter        { get; set; }
        public int            PrevMeter    { get; set; }  // value before this tick's gain

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
