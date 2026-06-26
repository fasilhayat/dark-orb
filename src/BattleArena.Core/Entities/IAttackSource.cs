namespace BattleArena.Core.Entities;

using Core.Entities.Enums;
using System.Text.Json.Serialization;

[JsonDerivedType(typeof(Weapon), typeDiscriminator: "weapon")]
[JsonDerivedType(typeof(Spell), typeDiscriminator: "spell")]
[JsonDerivedType(typeof(UnarmedStrike), typeDiscriminator: "unarmed")]
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
    bool IsFinesse { get; }
}
