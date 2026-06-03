namespace BattleArena.Core.Entities;

public class BestiaryEntry
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int StrengthBonus { get; set; }
    public int DexterityBonus { get; set; }
    public int StaminaBonus { get; set; }
    public int IntelligenceBonus { get; set; }
    public int WisdomBonus { get; set; }
    public int CharismaBonus { get; set; }
    public int MaxHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public string AttackDescription { get; set; } = string.Empty;
    public string SpecialAbilities { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
