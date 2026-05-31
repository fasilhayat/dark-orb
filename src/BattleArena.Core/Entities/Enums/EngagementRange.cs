namespace BattleArena.Core.Entities.Enums;

/// <summary>
/// The distance between attacker and defender when an attack is resolved.
/// Currently defaults to Melee everywhere. The full distance system will set
/// this from position state when it is implemented.
/// </summary>
public enum EngagementRange
{
    /// <summary>Within striking distance — standard melee or point-blank ranged.</summary>
    Melee,

    /// <summary>Close but not adjacent — bow/crossbow optimal range.</summary>
    Short,

    /// <summary>Far from the target — maximum ranged effectiveness, unreachable by melee.</summary>
    Long
}
