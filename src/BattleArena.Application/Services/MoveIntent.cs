namespace BattleArena.Application.Services;

using Core.Entities;
using Core.Entities.Enums;

public sealed class MoveIntent : IAttackSource
{
    public string Name => "Move";
    public int DamageCount => 0;
    public DieType DamageDie => DieType.D4;
    public DamageType DamageType => DamageType.Bludgeoning;
    public AttackType AttackType => AttackType.Melee;
    public int AttackBonus => 0;
    public int FlatDamageBonus => 0;
    public ElementalType ElementalType => ElementalType.None;
    public int ElementalDamage => 0;
    public bool UsesIntelligence => false;
    public bool IsFinesse => false;
}
