namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

// Drives the full turn-based battle loop:
//   1. Each tick both combatants gain turnmeter based on TurnSpeed + DEX + buffs - armor penalties.
//   2. When a combatant's meter reaches 100 they act (highest meter acts first if both ready).
//   3. Acting means resolving an attack; damage is applied to the target's HP.
//   4. A fumble applies a -2 AttackPower status effect for the fumbler's next turn.
//   5. After acting, 100 is subtracted from the actor's meter.
//   6. Every event is recorded in a BattleLogEntry with full detail.
//   7. HP can go negative — 0 to -9 = knocked out (KO), -10 or lower = dead.
//   8. Battle ends when a combatant is KO'd or killed, or maxTicks is exhausted.
public class BattleSimulator : IBattleSimulator
{
    private readonly ICombatService _combat;
    private readonly ITurnmeterService _turnmeter;
    private readonly IStatusEffectService _statusEffect;

    public BattleSimulator(ICombatService combat, ITurnmeterService turnmeter, IStatusEffectService statusEffect)
    {
        _combat = combat;
        _turnmeter = turnmeter;
        _statusEffect = statusEffect;
    }

    public BattleResult Simulate(
        Character fighter, IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000)
    {
        var log = new List<BattleLogEntry>();
        var fighterMeter = new TurnmeterState { CharacterId = fighter.Id, CharacterName = fighter.Name, CurrentValue = 0 };
        var opponentMeter = new TurnmeterState { CharacterId = opponent.Id, CharacterName = opponent.Name, CurrentValue = 0 };

        for (var tick = 1; tick <= maxTicks; tick++)
        {
            // ── TICK ──────────────────────────────────────────────────────────────
            var prevFM = fighterMeter.CurrentValue;
            var prevOM = opponentMeter.CurrentValue;
            fighterMeter  = _turnmeter.Tick(fighter,  fighterMeter);
            opponentMeter = _turnmeter.Tick(opponent, opponentMeter);

            log.Add(BuildTurnMeterGainEntry(tick, fighter.Name,  prevFM, fighterMeter,  opponentMeter));
            log.Add(BuildTurnMeterGainEntry(tick, opponent.Name, prevOM, opponentMeter, fighterMeter));

            if (!fighterMeter.CanTakeTurn && !opponentMeter.CanTakeTurn)
                continue;

            // ── ACTING ORDER ──────────────────────────────────────────────────────
            // Both may act on the same tick; highest meter value goes first.
            bool[] actingOrder;
            if (fighterMeter.CanTakeTurn && opponentMeter.CanTakeTurn)
                actingOrder = fighterMeter.CurrentValue >= opponentMeter.CurrentValue
                    ? new[] { true, false } : new[] { false, true };
            else
                actingOrder = fighterMeter.CanTakeTurn ? new[] { true } : new[] { false };

            foreach (var isFighter in actingOrder)
            {
                var actor = isFighter ? fighter : opponent;
                var target = isFighter ? opponent : fighter;
                var attackSource = isFighter ? fighterAttack : opponentAttack;
                if (attackSource is null)
                {
                    var spells = actor.MemorizedSpells;
                    if (spells.Count == 0)
                        throw new InvalidOperationException($"{actor.Name} has no weapon or memorized spells.");

                    attackSource = spells[Random.Shared.Next(spells.Count)];
                }

                // Mark the actor as active for the duration of their turn.
                if (isFighter) fighterMeter.IsActive  = true;
                else           opponentMeter.IsActive = true;

                // Expire duration-based status effects (e.g., fumble penalty) at turn start.
                _statusEffect.TickAll(actor);

                // The target may have already been knocked out or killed by the other combatant this tick.
                if (target.CurrentHitPoints <= 0)
                {
                    if (isFighter) fighterMeter.IsActive  = false;
                    else           opponentMeter.IsActive = false;
                    continue;
                }

                var meterNow = isFighter ? fighterMeter.CurrentValue : opponentMeter.CurrentValue;
                log.Add(new BattleLogEntry
                {
                    Tick = tick,
                    ActorName = actor.Name,
                    EventType = "TurnStart",
                    TurnMeterBefore = meterNow,
                    IsReady  = true,
                    IsActive = true,
                    Message = $"{actor.Name} takes their turn  (TM: {meterNow})"
                });

                // ── RESOLVE ATTACK ─────────────────────────────────────────────────
                var result = _combat.ResolveAttack(actor, target, attackSource);
                log.Add(BuildAttackEntry(tick, actor.Name, attackSource.Name, target.Name, result));

                // ── APPLY DAMAGE ───────────────────────────────────────────────────
                if (result.IsHit)
                {
                    var hpBefore = target.CurrentHitPoints;
                    // Allow HP to go negative: 0 to -9 = knocked out, -10 or lower = dead.
                    target.CurrentHitPoints = target.CurrentHitPoints - result.Damage;
                    log.Add(BuildDamageEntry(tick, target.Name, result.Damage, hpBefore, target.CurrentHitPoints));

                    if (target.CurrentHitPoints <= 0)
                    {
                        var defeatEvent = target.IsDead ? "Death" : "KnockedOut";
                        var defeatMsg   = target.IsDead
                            ? $"[DEAD] {target.Name} has been slain! (HP: {target.CurrentHitPoints})"
                            : $"[KO] {target.Name} is knocked unconscious! (HP: {target.CurrentHitPoints})";

                        log.Add(new BattleLogEntry
                        {
                            Tick = tick,
                            ActorName = target.Name,
                            EventType = defeatEvent,
                            Message = defeatMsg
                        });

                        if (isFighter) fighterMeter.IsActive  = false;
                        else           opponentMeter.IsActive = false;

                        if (isFighter)
                            RecordAfterTurn(ref fighterMeter, tick, log, actor.Name);
                        else
                            RecordAfterTurn(ref opponentMeter, tick, log, actor.Name);

                        return new BattleResult
                        {
                            Winner = actor,
                            Loser = target,
                            LoserStatus = target.VitalStatus,
                            TotalTicks = tick,
                            Log = log
                        };
                    }
                }

                // ── FUMBLE PENALTY ─────────────────────────────────────────────────
                if (result.IsFumble)
                {
                    _statusEffect.Apply(actor, new StatusEffect
                    {
                        Name = "Fumble Penalty",
                        Type = StatusEffectType.Debuff,
                        AttackPowerModifier = -2,
                        Duration = 1,
                        StackRule = StackRule.NoStack,
                        Source = "Fumble"
                    });
                    log.Add(new BattleLogEntry
                    {
                        Tick = tick,
                        ActorName = actor.Name,
                        EventType = "FumblePenalty",
                        Message = $"[FUMBLE] {actor.Name} fumbles! -2 AttackPower applied for next turn."
                    });
                }

                // ── END TURN ───────────────────────────────────────────────────────
                if (isFighter) fighterMeter.IsActive  = false;
                else           opponentMeter.IsActive = false;

                if (isFighter)
                    RecordAfterTurn(ref fighterMeter, tick, log, actor.Name);
                else
                    RecordAfterTurn(ref opponentMeter, tick, log, actor.Name);
            }
        }

        return new BattleResult { MaxTicksReached = true, TotalTicks = maxTicks, Log = log };
    }

    // Subtracts 100 from the meter and records a TurnEnd log entry.
    private void RecordAfterTurn(ref TurnmeterState meter, int tick, List<BattleLogEntry> log, string actorName)
    {
        var before = meter.CurrentValue;
        meter = _turnmeter.AfterTurn(meter);
        log.Add(new BattleLogEntry
        {
            Tick = tick,
            ActorName = actorName,
            EventType = "TurnEnd",
            TurnMeterBefore = before,
            TurnMeterAfter  = meter.CurrentValue,
            IsReady  = meter.IsReady,
            IsActive = false,
            Message  = $"{actorName} ends turn.  TM: {before} -> {meter.CurrentValue}"
        });
    }

    // Records a turnmeter gain event. Both meters are passed so IsReady/IsActive
    // can be stamped on the entry from the current state of the named actor.
    private static BattleLogEntry BuildTurnMeterGainEntry(
        int tick, string name, int before,
        TurnmeterState actorMeter, TurnmeterState otherMeter) => new()
    {
        Tick = tick,
        ActorName = name,
        EventType = "TurnMeterGain",
        TurnMeterBefore = before,
        TurnMeterAfter  = actorMeter.CurrentValue,
        IsReady  = actorMeter.IsReady,
        IsActive = actorMeter.IsActive,
        Message  = $"{name}  TM: {before} -> {actorMeter.CurrentValue}  (+{actorMeter.CurrentValue - before})"
    };

    private static BattleLogEntry BuildAttackEntry(int tick, string actorName, string attackSourceName, string targetName, AttackResult result)
    {
        var outcome = result.IsCriticalHit ? "CRITICAL HIT!" :
                      result.IsFumble     ? "FUMBLE!" :
                      result.IsHit        ? "HIT" : "MISS";

        var msg = $"{actorName} [{attackSourceName}] -> {targetName}: d20={result.HitRoll} + AP={result.AttackPower} = {result.HitRoll + result.AttackPower} vs DP={result.DefensePower} -> {outcome}";

        if (result.IsHit && result.DamageContext is { } dc)
        {
            var critTag = result.IsCriticalHit ? " [x2 CRIT]" : "";
            msg += $" | Dmg: roll({dc.WeaponDiceRoll}) + attr({dc.AttributeModifier}) + flat({dc.FlatBonuses}) = {dc.BaseDamage}{critTag} x{dc.TypeMultiplier:0.0} - mit({dc.ArmorMitigation}) + elem({dc.ElementalModifiers}) = {dc.FinalDamage}";
        }

        // Pick a narrative phrase graded on roll quality
        var ctx    = CombatNarrator.GetContext(
            result.HitRoll,
            result.HitRoll + result.AttackPower,
            result.DefensePower,
            result.IsHit || result.IsCriticalHit,
            result.IsCriticalHit,
            result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx);

        return new BattleLogEntry
        {
            Tick = tick,
            ActorName = actorName,
            EventType = "Attack",
            DieRoll = result.HitRoll,
            AttackPower = result.AttackPower,
            DefensePower = result.DefensePower,
            IsHit = result.IsHit,
            IsCritical = result.IsCriticalHit,
            IsFumble = result.IsFumble,
            DamageDealt = result.Damage,
            Phrase = phrase,
            Message = msg
        };
    }

    private static BattleLogEntry BuildDamageEntry(int tick, string targetName, int damage, int hpBefore, int hpAfter) => new()
    {
        Tick = tick,
        ActorName = targetName,
        EventType = "Damage",
        DamageDealt = damage,
        TargetHpBefore = hpBefore,
        TargetHpAfter = hpAfter,
        Message = $"{targetName} takes {damage} damage.  HP: {hpBefore} → {hpAfter}"
    };
}
