namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Armor
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ArmorClass { get; init; }
    public string Category { get; init; } = string.Empty;
    public string CategoryName => Category;
    public int MaxDexterityBonus { get; init; }
    public bool StealthDisadvantage { get; init; }
    public int StrengthRequirement { get; init; }
    public GearQuality Quality { get; init; } = GearQuality.Common;
    public int ArmorClassBonus { get; init; }
    public int Mitigation { get; init; }
    public int TurnMeterPenalty { get; init; }
    public int TurnMeterCostReduction { get; init; }
    public int StrengthBonus { get; init; }
    public int ManaRegenBonus { get; init; }
    public int MaxManaBonus { get; init; }
    public int SpellSlotsBonus { get; init; }
    public int MovementPenalty { get; init; }
    public List<ResistanceBonus> Resistances { get; init; } = new();
}
