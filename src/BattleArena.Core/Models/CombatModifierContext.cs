namespace BattleArena.Core.Models;

using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Mutable bag of values passed through the <see cref="Interfaces.ICombatModifier"/> pipeline.
/// Each <see cref="CombatPhase"/> uses a subset of these fields.
/// Modifiers mutate the delta/multiplier fields; the caller applies them to base values.
/// </summary>
public class CombatModifierContext
{
    // ── Inputs (shared, read-only) ──────────────────────────────────────────

    public required Character      Attacker         { get; init; }
    public required Character      Defender         { get; init; }
    public required IAttackSource  Source           { get; init; }
    public          EngagementRange Range           { get; init; } = EngagementRange.Melee;
    public          TerrainType    Terrain          { get; init; } = TerrainType.Plains;

    /// <summary>Base attack power before any modifiers (from <c>CombatantStats</c>).</summary>
    public required int            BaseAttackPower  { get; init; }

    /// <summary>Base defense power before any modifiers (from <c>CombatantStats</c>).</summary>
    public required int            BaseDefensePower { get; init; }

    // ── Outputs (accumulated by modifiers) ──────────────────────────────────

    // AttackRoll phase
    /// <summary>Net adjustment to the attacker's attack power for this exchange.</summary>
    public int AttackPowerDelta  { get; set; }

    /// <summary>Net adjustment to the defender's defense power for this exchange.</summary>
    public int DefensePowerDelta { get; set; }

    // DamageCalculation phase
    /// <summary>Flat adjustment to final damage (added after mitigation).</summary>
    public int DamageDelta { get; set; }

    /// <summary>Multiplicative adjustment to base damage (1.0 = no change).</summary>
    public double DamageMultiplier { get; set; } = 1.0;

    // Healing phase
    /// <summary>Flat adjustment to healing amount.</summary>
    public int HealingPowerDelta { get; set; }

    /// <summary>Multiplicative adjustment to healing (1.0 = no change).</summary>
    public double HealingMultiplier { get; set; } = 1.0;
}
