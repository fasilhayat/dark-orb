namespace BattleArena.Application.Models;

public class DamageContext
{
    public int WeaponDiceRoll { get; set; }
    public int AttributeModifier { get; set; }
    public int FlatBonuses { get; set; }
    public int LevelScaling { get; set; }
    public int BaseDamage { get; set; }
    public float TypeMultiplier { get; set; } = 1.0f;
    public int ArmorMitigation { get; set; }
    public int ElementalModifiers { get; set; }
    public int FinalDamage { get; set; }
}
