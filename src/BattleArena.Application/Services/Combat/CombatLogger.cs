namespace BattleArena.Application.Services.Combat;

using Application.Interfaces;
using Application.Models;
using Application.Models.Combat;
using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Handles all combat event logging and log entry construction.
/// </summary>
public class CombatLogger
{

    public CombatLogEntry BuildTurnMeterGainEntry(int tick, CombatantState s)
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
            Message         = $"{s.Character.Name} gains turn meter. ({before} → {s.Meter.CurrentValue})"
        };
    }

    public CombatLogEntry BuildAfterTurnEntry(CombatantState state, int tick, int tmCost, ITurnmeterService turnmeterService)
    {
        var before = state.Meter.CurrentValue;
        state.Meter = turnmeterService.AfterTurn(state.Meter, tmCost);
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
        int tick, string actorName, string targetName, string attackSourceName, bool isSpell,
        AttackResult result, int attackRoll, int defenseRoll, int attackPower, int defensePower,
        DamageType damageType, int? spellLevel = null, int? casterLevel = null, int rollIndex = 0)
    {
        var ctx = CombatNarrator.GetContext(
            attackRoll, attackRoll + attackPower, defensePower, defenseRoll,
            result.IsHit || result.IsCriticalHit, result.IsCriticalHit, result.IsFumble);
        var phrase = CombatNarrator.GetPhrase(actorName, targetName, ctx, isSpell, damageType, rollIndex: _ => rollIndex);
        
        return new CombatLogEntry
        {
            Tick                = tick,
            ActorName           = actorName,
            TargetName          = targetName,
            AttackSourceName    = attackSourceName,
            EventType           = "Attack",
            DieRoll             = attackRoll,
            DefenseRoll         = defenseRoll,
            AttackPower         = attackPower,
            DefensePower        = defensePower,
            IsHit               = result.IsHit,
            IsCritical          = result.IsCriticalHit,
            IsFumble            = result.IsFumble,
            IsTotalReversal     = result.IsTotalReversal,
            IsDevastatingStrike = result.IsDevastatingStrike,
            IsPerfectParry      = result.IsPerfectParry,
            IsClash             = result.IsClash,
            DamageDealt         = result.Damage,
            IsSpell             = isSpell,
            SpellLevel          = spellLevel,
            CasterLevel         = casterLevel,
            Phrase              = phrase,
            Message             = phrase
        };
    }

    public string GetOutcomeTag(AttackResult result) =>
        result switch
        {
            { IsTotalReversal: true }     => "TOTAL REVERSAL!",
            { IsDevastatingStrike: true }  => "DEVASTATING STRIKE!!!",
            { IsClash: true }              => "CLASH!",
            { IsPerfectParry: true }       => "PERFECT PARRY!",
            { IsCriticalHit: true }        => "CRITICAL HIT!",
            { IsFumble: true }             => "FUMBLE!",
            { IsHit: true }                => "HIT",
            _                              => "MISS"
        };

    public string GetCritTag(AttackResult result) =>
        result switch
        {
            { IsDevastatingStrike: true }  => "[DEVASTATING]",
            { IsCriticalHit: true }        => "[CRIT]",
            _                              => ""
        };

    public CombatLogEntry BuildDamageEntry(
        int tick, string targetName, int damage, int hpBefore, int hpAfter)
    {
        return new CombatLogEntry
        {
            Tick            = tick,
            ActorName       = targetName,
            EventType       = "Damage",
            DamageDealt     = damage,
            TargetHpBefore  = hpBefore,
            TargetHpAfter   = hpAfter,
            Message         = $"{targetName} takes {damage} damage. HP: {hpBefore} → {hpAfter}"
        };
    }

    public CombatLogEntry BuildRoundStartEntry(int tick, int roundNumber) => new()
    {
        Tick        = tick,
        EventType   = "RoundStart",
        RoundNumber = roundNumber,
        Message     = $"══ Round {roundNumber} begins ══"
    };

    public CombatLogEntry BuildRoundEndEntry(int tick, int roundNumber) => new()
    {
        Tick        = tick,
        EventType   = "RoundEnd",
        RoundNumber = roundNumber,
        Message     = $"── Round {roundNumber} ends ──"
    };

    public CombatLogEntry BuildManaRegenEntry(int tick, string name, int manaBefore, int manaAfter, int regen) => new()
    {
        Tick        = tick,
        ActorName   = name,
        EventType   = "ManaRegen",
        ManaRegen   = regen,
        ManaAfter   = manaAfter,
        Message     = $"{name} regenerates {regen} mana. ({manaBefore} → {manaAfter})"
    };

    public CombatLogEntry BuildManaCostEntry(int tick, string name, string spellName, int manaBefore, int manaAfter, int cost) => new()
    {
        Tick             = tick,
        ActorName        = name,
        EventType        = "ManaDeduct",
        AttackSourceName = spellName,
        ManaCost         = cost,
        ManaAfter        = manaAfter,
        Message          = $"{name} spends {cost} mana on {spellName}. ({manaBefore} → {manaAfter})"
    };
}