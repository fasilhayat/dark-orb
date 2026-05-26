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
    // Separate selectors so hero and enemy parties can use different strategies.
    private readonly ITargetSelector      _heroTargetSelector;
    private readonly ITargetSelector      _enemyTargetSelector;

    public BattleSimulator(
        ICombatService combat,
        ITurnmeterService turnmeter,
        IStatusEffectService statusEffect,
        ITargetSelector? heroTargetSelector  = null,
        ITargetSelector? enemyTargetSelector = null)
    {
        _combat              = combat;
        _turnmeter           = turnmeter;
        _statusEffect        = statusEffect;
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

            // ── ACTING ORDER: all ready combatants, highest meter first ────────
            var acting = states
                .Where(s => s.Character.IsAlive && s.Meter.IsReady)
                .OrderByDescending(s => s.Meter.CurrentValue)
                .ToList();

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
                _statusEffect.TickAll(actorState.Character);

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

                // ── ATTACK RESOLUTION ──────────────────────────────────────────
                var result = _combat.ResolveAttack(actorState.Character, target, attackSource);
                await Notify(BuildAttackEntry(tick, actorState.Character.Name, attackSource.Name, isSpell, target.Name, result));

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

                    if (target.CurrentHitPoints <= 0)
                    {
                        await Notify(BuildDefeatEntry(tick, target));

                        // Check whether the entire opposing party is now defeated.
                        var opposingParty = actorState.PartyIndex == 0 ? enemyParty : heroParty;
                        if (opposingParty.IsDefeated)
                        {
                            actorState.Meter.IsActive = false;
                            await Notify(BuildAfterTurnEntry(actorState, tick));

                            var winningParty = actorState.PartyIndex == 0 ? heroParty : enemyParty;
                            return new BattleResult
                            {
                                WinningParty = winningParty,
                                LosingParty  = opposingParty,
                                LoserStatus  = target.VitalStatus,
                                TotalTicks   = tick,
                                Log          = log
                            };
                        }
                        // Party not fully wiped — continue to next actor.
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
            : $"[KO] {target.Name} is knocked unconscious! (HP: {target.CurrentHitPoints})"
    };

    private static BattleLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result)
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
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx);

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
