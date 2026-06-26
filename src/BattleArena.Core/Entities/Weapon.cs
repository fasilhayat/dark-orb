namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Weapon : IAttackSource
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ArchetypeWeapon Archetype { get; init; }
    public DieType DamageDie { get; init; }
    public DamageType DamageType { get; init; }
    public AttackType AttackType { get; init; }
    public int DamageCount { get; init; } = 1;
    public int Hands { get; init; } = 1;
    public GearQuality Quality { get; init; } = GearQuality.Common;
    public int AttackBonus { get; init; }
    public int MinimumStrength { get; init; }
    public int ElementalDamage { get; init; }
    public ElementalType ElementalType { get; init; } = ElementalType.None;
    public int FlatDamageBonus { get; init; }
    public bool UsesIntelligence => false;
    public bool IsFinesse => Archetype switch
    {
        ArchetypeWeapon.Dagger => true,
        ArchetypeWeapon.ShortSword => true,
        ArchetypeWeapon.Sword => true,
        ArchetypeWeapon.Spear => true,
        _ => false,
    };
}
