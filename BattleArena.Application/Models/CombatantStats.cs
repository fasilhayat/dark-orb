namespace BattleArena.Application.Models;

public class CombatantStats
{
    public int ClassAccuracyBase { get; set; }
    public int LevelScaling { get; set; }
    public int AttributeModifier { get; set; }
    public int WeaponAttackBonus { get; set; }
    public int SkillModifiers { get; set; }
    public int BuffModifiers { get; set; }
    public int RacialModifiers { get; set; }
    public int ItemSetBonuses { get; set; }
    public int AttackPower => ClassAccuracyBase + LevelScaling + AttributeModifier + WeaponAttackBonus + SkillModifiers + BuffModifiers + RacialModifiers + ItemSetBonuses;

    public int EffectiveAC { get; set; }
    public int DexterityModifier { get; set; }
    public int ShieldBonus { get; set; }
    public int DefensiveBuffs { get; set; }
    public int DefenseRacialModifiers { get; set; }
    public int DefenseItemSetBonuses { get; set; }
    public int DefensePower => EffectiveAC + DexterityModifier + ShieldBonus + DefensiveBuffs + DefenseRacialModifiers + DefenseItemSetBonuses;
}
