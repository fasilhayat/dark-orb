using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArchetypeWeapon Archetype { get; set; }
    public DieType DamageDie { get; set; }
    public DamageType DamageType { get; set; }
    public AttackType AttackType { get; set; }
    public int DamageCount { get; set; } = 1;
    public int Hands { get; set; } = 1;
    public GearQuality Quality { get; set; } = GearQuality.Common;
    public int AttackBonus { get; set; }
}
