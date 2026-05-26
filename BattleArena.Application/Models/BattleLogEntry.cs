namespace BattleArena.Application.Models;

// A single event recorded during a battle simulation.
// EventType values: TurnMeterGain, TurnStart, Attack, Damage, FumblePenalty, TurnEnd, Death
public class BattleLogEntry
{
    public int Tick { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Present on Attack events
    public int? DieRoll { get; set; }
    public int? AttackPower { get; set; }
    public int? DefensePower { get; set; }
    public bool? IsHit { get; set; }
    public bool? IsCritical { get; set; }
    public bool? IsFumble { get; set; }
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
}
