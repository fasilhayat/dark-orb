namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

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
    public int Mitigation { get; set; }
    public int TurnMeterPenalty { get; set; }
    public int TurnMeterCostReduction { get; set; }
    public int ManaRegenBonus { get; set; }
    public int MaxManaBonus { get; set; }
    public List<ResistanceBonus> Resistances { get; set; } = new();
}
