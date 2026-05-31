namespace BattleArena.Core.Models;

using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Mutable bag of values passed through the <see cref="Interfaces.ICombatModifier"/> pipeline.
/// Modifiers accumulate deltas into <see cref="AttackPowerDelta"/> /
/// <see cref="DefensePowerDelta"/>; the caller applies them to the base stats
/// and reports the effective totals in <c>AttackResult</c>.
/// </summary>
public class CombatModifierContext
{
    // ── Inputs (read-only for modifiers) ─────────────────────────────────────

    public required Character      Attacker         { get; init; }
    public required Character      Defender         { get; init; }
    public required IAttackSource  Source           { get; init; }
    public          EngagementRange Range           { get; init; } = EngagementRange.Melee;

    /// <summary>Base attack power before any modifiers (from <c>CombatantStats</c>).</summary>
    public required int            BaseAttackPower  { get; init; }

    /// <summary>Base defense power before any modifiers (from <c>CombatantStats</c>).</summary>
    public required int            BaseDefensePower { get; init; }

    // ── Outputs (accumulated by modifiers) ───────────────────────────────────

    /// <summary>Net adjustment to the attacker's attack power for this exchange.</summary>
    public int AttackPowerDelta  { get; set; }

    /// <summary>Net adjustment to the defender's defense power for this exchange.</summary>
    public int DefensePowerDelta { get; set; }
}
