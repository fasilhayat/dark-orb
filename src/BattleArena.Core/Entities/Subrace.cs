namespace BattleArena.Core.Entities;

public class Subrace
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StrengthBonus { get; set; }
    public int DexterityBonus { get; set; }
    public int StaminaBonus { get; set; }
    public int IntelligenceBonus { get; set; }
    public int WisdomBonus { get; set; }
    public int CharismaBonus { get; set; }
    public int HitPointBonus { get; set; }
    public List<Feat> Feats { get; set; } = new();
}
