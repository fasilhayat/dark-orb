namespace BattleArena.Core.Entities;

public class Race
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseMovementSpeed { get; set; } = 30;
    public int StaminaBonus { get; set; }
    public int HitPointBonus { get; set; }
    public Dictionary<string, int> AbilityBonuses { get; set; } = new();
    public List<Feat> Feats { get; set; } = new();
    public int StrengthMin { get; set; } = 3;
    public int DexterityMin { get; set; } = 3;
    public int StaminaMin { get; set; } = 3;
    public int IntelligenceMin { get; set; } = 3;
    public int WisdomMin { get; set; } = 3;
    public int CharismaMin { get; set; } = 3;
    public int StrengthMax { get; set; } = 18;
    public int DexterityMax { get; set; } = 18;
    public int StaminaMax { get; set; } = 18;
    public int IntelligenceMax { get; set; } = 18;
    public int WisdomMax { get; set; } = 18;
    public int CharismaMax { get; set; } = 18;
}
