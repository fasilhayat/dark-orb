using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class Armor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ArmorClass { get; set; }
    public string Category { get; set; } = string.Empty;
    public int MaxDexterityBonus { get; set; }
    public bool StealthDisadvantage { get; set; }
    public int StrengthRequirement { get; set; }
    public GearQuality Quality { get; set; } = GearQuality.Common;
    public int ArmorClassBonus { get; set; }
}
