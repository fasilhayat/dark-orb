namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Spell : IAttackSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpellSchool School { get; set; }
    public int ManaCost { get; set; }
    public int SpellLevel { get; set; }
    public int DamageCount { get; set; } = 1;
    public DieType DamageDie { get; set; }
    public DamageType DamageType { get; set; }
    public AttackType AttackType { get; set; } = AttackType.Ranged;
    public int AttackBonus { get; set; }
    public int FlatDamageBonus { get; set; }
    public ElementalType ElementalType { get; set; } = ElementalType.None;
    public int ElementalDamage { get; set; }
    public bool UsesIntelligence => true;
}
