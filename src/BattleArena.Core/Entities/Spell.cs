namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Spell : IAttackSource
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SpellSchool School { get; init; }
    public int ManaCost { get; init; }
    /// <summary>Cost as a percentage of a full turn (100 = 100 % = one full turn).</summary>
    public int TurnMeterCost { get; init; } = 100;
    public int SpellLevel { get; init; }
    public int DamageCount { get; init; } = 1;
    public DieType DamageDie { get; init; }
    public DamageType DamageType { get; init; }
    public AttackType AttackType { get; init; } = AttackType.Spell;
    public int AttackBonus { get; init; }
    public int FlatDamageBonus { get; init; }
    public ElementalType ElementalType { get; init; } = ElementalType.None;
    public int ElementalDamage { get; init; }
    public bool UsesIntelligence => School switch
    {
        SpellSchool.Deity => false,
        _ => true,
    };
    public bool IsFinesse => false;
    public bool IsHealing => DamageType == DamageType.Healing;
    public bool IsGroupHeal => IsHealing && Name.Contains("Mass");
    public string Tags { get; init; } = string.Empty;
    public Pet? SummonedPet { get; init; }
    public List<StatusEffect> OnHitEffects { get; init; } = new();
}
