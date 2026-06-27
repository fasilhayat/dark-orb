namespace BattleArena.Application.Services.Combat;

using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

public class CombatLogger
{
    public CombatLogEntry BuildTurnMeterGainEntry(
        int tick, string actorName, int prevMeter, int currentMeter, bool isReady, bool isActive)
    {
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = actorName,
            EventType       = "TurnMeterGain",
            TurnMeterBefore = prevMeter,
            TurnMeterAfter  = currentMeter,
            IsReady         = isReady,
            IsActive        = isActive,
            Message         = $"{actorName}  TM: {prevMeter} -> {currentMeter}  (+{currentMeter - prevMeter})"
        };
    }

    public CombatLogEntry BuildAfterTurnEntry(int tick, string actorName, int tmBefore, int tmAfter, int tmCost)
    {
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = actorName,
            EventType       = "TurnEnd",
            TurnMeterBefore = tmBefore,
            TurnMeterAfter  = tmAfter,
            IsReady         = false,
            IsActive        = false,
            Message         = $"{actorName} ends turn.  TM: {tmBefore} -> {tmAfter} (cost: {tmCost})"
        };
    }

    public CombatLogEntry BuildDefeatEntry(int tick, Character target) => new()
    {
        Tick      = tick,
        ActorName = target.Name,
        EventType = target.IsDead ? "Death" : "KnockedOut",
        Message   = target.IsDead
            ? $"[DEAD] {target.Name} has been slain! (HP: {target.CurrentHitPoints})"
            : $"{target.Name} is unconscious! (HP: {target.CurrentHitPoints})"
    };

    public CombatLogEntry BuildAttackEntry(
        int tick, string actorName, string attackSourceName, bool isSpell,
        string targetName, AttackResult result, DamageType damageType = DamageType.Slashing,
        int? spellLevel = null, int? casterLevel = null, Func<int, int>? rollIndex = null)
    {
        var outcome = GetOutcomeTag(result);
        var msg = $"{actorName} [{attackSourceName}] -> {targetName}: " +
                  $"d20_atk={result.HitRoll} d20_def={result.DefenseRoll} + AP={result.AttackPower} " +
                  $"vs DP={result.DefensePower} -> {outcome}";

        if (result.IsHit && result.DamageContext is { } dc)
        {
            var critTag = GetCritTag(result);
            msg += $" | Dmg: roll({dc.WeaponDiceRoll}) + attr({dc.AttributeModifier}) + flat({dc.FlatBonuses}) + lvl({dc.LevelScaling})" +
                   $" = {dc.BaseDamage}{critTag} x{dc.TypeMultiplier.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} - mit({dc.ArmorMitigation}) + elem({dc.ElementalModifiers}) = {result.Damage}";
        }

        var ctx    = CombatNarrator.GetContext(
            result.HitRoll, result.HitRoll + result.AttackPower, result.DefensePower, result.DefenseRoll,
            result.IsHit || result.IsCriticalHit, result.IsCriticalHit, result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx, isSpell, damageType, rollIndex);

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
            SpellLevel          = spellLevel,
            CasterLevel         = casterLevel,
            Phrase              = phrase,
            Message             = msg
        };
    }

    public string GetOutcomeTag(AttackResult result) =>
        result.IsDevastatingStrike ? "DEVASTATING STRIKE!!!" :
        result.IsTotalReversal     ? "TOTAL REVERSAL!"       :
        result.IsClash             ? "CLASH!"                :
        result.IsPerfectParry      ? "PERFECT PARRY!"        :
        result.IsCriticalHit       ? "CRITICAL HIT!"         :
        result.IsFumble            ? "FUMBLE!"               :
        result.IsHit               ? "HIT"                   : "MISS";

    public string GetCritTag(AttackResult result) =>
        result.IsCriticalHit       ? " [x2 CRIT]"    :
        result.IsDevastatingStrike ? " [x3 DEVAS]"   :
        result.IsClash             ? " [x0.5 CLASH]" : "";

    public CombatLogEntry BuildDamageEntry(
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

    public CombatLogEntry BuildRoundStartEntry(int tick, int roundNumber) => new()
    {
        Tick = tick, EventType = "RoundStart", RoundNumber = roundNumber,
        Message = $"══ Round {roundNumber} begins ══"
    };

    public CombatLogEntry BuildRoundEndEntry(int tick, int roundNumber) => new()
    {
        Tick = tick, EventType = "RoundEnd", RoundNumber = roundNumber,
        Message = $"── Round {roundNumber} ends ──"
    };

    public CombatLogEntry BuildManaRegenEntry(int tick, string name, int manaBefore, int manaAfter, int regen) => new()
    {
        Tick = tick, ActorName = name, EventType = "ManaRegen",
        ManaRegen = regen, ManaAfter = manaAfter,
        Message = $"{name} regenerates {regen} mana. ({manaBefore} → {manaAfter})"
    };

    public CombatLogEntry BuildManaCostEntry(int tick, string name, string spellName, int manaBefore, int manaAfter, int cost) => new()
    {
        Tick = tick, ActorName = name, EventType = "ManaDeduct",
        AttackSourceName = spellName, ManaCost = cost, ManaAfter = manaAfter,
        Message = $"{name} spends {cost} mana on {spellName}. ({manaBefore} → {manaAfter})"
    };

    // Convenience overload — accepts the model CombatantState (for refactored processors).
    internal CombatLogEntry BuildTurnMeterGainEntry(int tick, CombatantState s) =>
        BuildTurnMeterGainEntry(tick, s.Character.Name, s.PrevMeter, s.Meter.CurrentValue, s.Meter.IsReady, s.Meter.IsActive);
}
