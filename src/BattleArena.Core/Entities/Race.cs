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
}
