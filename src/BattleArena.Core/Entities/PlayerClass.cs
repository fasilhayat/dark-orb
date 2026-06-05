namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class PlayerClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DieType HitDie { get; set; }
    public int BaseStrikeRating { get; set; }
    public int MovementBonus { get; set; }

    // ── Weapon & combat system ──────────────────────────────────────────────

    /// <summary>Base number of attacks per turn for this class.</summary>
    public int AttacksPerTurn { get; set; } = 1;

    /// <summary>
    /// Attacks per turn when wielding a bow (applies to Ranger).
    /// 0 means no special bonus.
    /// </summary>
    public int BowAttacksPerTurn { get; set; }

    /// <summary>
    /// Armor restriction: null means unrestricted, "Light" means light only,
    /// "LightOrMedium" means light or medium only.
    /// </summary>
    public string? ArmorRestriction { get; set; }

    /// <summary>Can this class dual-wield weapons?</summary>
    public bool CanDualWield { get; set; }

    /// <summary>
    /// Turnmeter cost multiplier when switching weapon types (e.g. ranged to melee).
    /// 0 = no cost, 1 = full cost, 0.5 = half cost.
    /// </summary>
    public double WeaponSwitchCostMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Bonus attack power when wielding a two-handed weapon.
    /// Applied in CombatStatsService.
    /// </summary>
    public int TwoHandedWeaponBonus { get; set; }

    /// <summary>
    /// Bonus attack power when wielding a shield.
    /// </summary>
    public int ShieldBonusDamage { get; set; }

    /// <summary>
    /// Ranged weapon attack bonus (for classes that specialize in bows).
    /// </summary>
    public int RangedAttackBonus { get; set; }
}
