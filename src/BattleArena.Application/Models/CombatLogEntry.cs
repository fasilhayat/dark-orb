namespace BattleArena.Application.Models;

// A single event recorded during a combat simulation.
// EventType values: TurnMeterGain, TurnStart, Attack, Damage, FumblePenalty, TurnEnd, Death
public class CombatLogEntry
{
    public int Tick { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Present on Attack events
    public int? DieRoll { get; set; }
    public int? DefenseRoll { get; set; }
    public int? AttackPower { get; set; }
    public int? DefensePower { get; set; }
    public bool? IsHit { get; set; }
    public bool? IsCritical { get; set; }
    public bool? IsFumble { get; set; }
    public bool? IsPerfectParry { get; set; }
    public bool? IsClash { get; set; }
    public bool? IsDevastatingStrike { get; set; }
    public bool? IsTotalReversal { get; set; }
    public int? DamageDealt { get; set; }

    // Present on TurnMeterGain and TurnEnd events
    public int? TurnMeterBefore { get; set; }
    public int? TurnMeterAfter { get; set; }

    // Snapshot of ready/active state at the moment this entry was recorded.
    // IsReady  = meter >= 100 (character may act this tick).
    // IsActive = character is currently resolving their turn.
    public bool IsReady { get; set; }
    public bool IsActive { get; set; }

    // Present on Damage events
    public int? TargetHpBefore { get; set; }
    public int? TargetHpAfter { get; set; }

    // Narrative flavour phrase set by CombatNarrator on Attack events
    public string? Phrase { get; set; }

    // The weapon or spell used in this action (present on TurnStart and Attack events).
    public string? AttackSourceName { get; set; }
    // True when the attack source is a spell, false for melee/ranged weapons.
    public bool IsSpell { get; set; }

    // The name of the character being targeted (present on TurnStart and Attack events).
    public string? TargetName { get; set; }

    // True when the status effect being applied is a self/ally buff (not a hostile debuff).
    public bool? IsBuff { get; set; }

    // Name of a status effect involved in this event (present on DoTTick, EffectApplied events).
    public string? StatusEffectName { get; set; }

    // Populated on EffectResisted events
    public int? ResistRoll { get; set; }
    public int? ResistThreshold { get; set; }

    // Populated on ManaRegen events
    public int? ManaRegen { get; set; }
    // Populated on ManaDeduct events
    public int? ManaCost { get; set; }
    // Snapshot after mana change on ManaRegen / ManaDeduct events
    public int? ManaAfter { get; set; }

    // The character currently taking their turn (null between turns).
    // Set by the simulator on every entry so consumers never have to track it.
    public string? ActiveActorName { get; set; }
    public int? RoundNumber { get; set; }
    public string? SummonedPetName { get; set; }

    // Populated on SkippedTurn events — "stunned", "rooted", "feared"
    public string? CcLabel { get; set; }

    // Snapshot of every living combatant's turn-meter value at the moment this
    // TurnStart was emitted.  Used by PlayTurnBased to display correct TM bars.
    public Dictionary<string, int>? TurnMeterSnapshot { get; set; }
}
