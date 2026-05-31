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
    /// <summary>
    /// Base attack power computed from character stats and equipment.
    /// Combat-phase modifiers (range, terrain, weather …) are applied on top of
    /// this by the <c>ICombatModifier</c> pipeline inside <c>CombatService</c>.
    /// </summary>
    public int AttackPower => ClassAccuracyBase + LevelScaling + AttributeModifier + WeaponAttackBonus + SkillModifiers + BuffModifiers + RacialModifiers + ItemSetBonuses;

    public int EffectiveAC { get; set; }
    public int DexterityModifier { get; set; }
    public int ShieldBonus { get; set; }
    public int DefensiveBuffs { get; set; }
    public int DefenseRacialModifiers { get; set; }
    public int DefenseItemSetBonuses { get; set; }
    public int LevelDefenseBonus { get; set; }
    /// <summary>
    /// Magic resistance converted to d20 scale (ComputeResistance(Magic) / 5).
    /// Populated for spell attacks; 0 for physical attacks.
    /// </summary>
    public int MagicResistanceBonus { get; set; }
    /// <summary>
    /// Base defense power computed from character stats and equipment.
    /// Combat-phase modifiers are applied on top of this by the <c>ICombatModifier</c> pipeline.
    /// </summary>
    public int DefensePower => EffectiveAC + DexterityModifier + ShieldBonus + DefensiveBuffs + DefenseRacialModifiers + DefenseItemSetBonuses + LevelDefenseBonus + MagicResistanceBonus;
}
