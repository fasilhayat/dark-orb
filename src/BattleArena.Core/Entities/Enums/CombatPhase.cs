namespace BattleArena.Core.Entities.Enums;

/// <summary>
/// Identifies which phase of the combat resolution a modifier participates in.
/// Only <see cref="AttackRoll"/> is active in v1; additional phases will be
/// wired when the corresponding pipeline steps are implemented.
/// </summary>
public enum CombatPhase
{
    /// <summary>
    /// Modifies attack power and/or defense power before the hit/miss roll is resolved.
    /// </summary>
    AttackRoll
}
