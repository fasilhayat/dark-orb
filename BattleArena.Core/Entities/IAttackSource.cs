namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public interface IAttackSource
{
    string Name { get; }
    int DamageCount { get; }
    DieType DamageDie { get; }
    DamageType DamageType { get; }
    AttackType AttackType { get; }
    int AttackBonus { get; }
    int FlatDamageBonus { get; }
    ElementalType ElementalType { get; }
    int ElementalDamage { get; }
    bool UsesIntelligence { get; }
}
