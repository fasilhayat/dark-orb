namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class ArmorSlots
{
    public Armor? Head { get; set; }
    public Armor? Chest { get; set; }
    public Armor? Hands { get; set; }
    public Armor? Waist { get; set; }
    public Armor? Boots { get; set; }
    public Armor? Neck { get; set; }
    public Armor? Back { get; set; }
    public Armor? LeftRing { get; set; }
    public Armor? RightRing { get; set; }
    public Armor? Ornament { get; set; }
    public Shield? Shield { get; set; }
    public Weapon? RightHand { get; set; }
    public Weapon? LeftHand { get; set; }

    private Armor?[] AllSlots => [Head, Chest, Hands, Waist, Boots, Neck, Back, LeftRing, RightRing, Ornament];
    private Armor?[] GearSlots => [Head, Chest, Hands, Waist, Boots, Neck, Back];

    public int TotalArmorClass => SumSlots(a => a.ArmorClass, GearSlots);
    public int TotalMitigation => SumSlots(a => a.Mitigation, GearSlots);
    public int TotalTurnMeterPenalty => SumSlots(a => a.TurnMeterPenalty, GearSlots);
    public int TotalTurnMeterCostReduction => SumSlots(a => a.TurnMeterCostReduction, GearSlots);
    public int TotalManaRegenBonus => SumSlots(a => a.ManaRegenBonus, AllSlots);
    public int TotalStrengthBonus => SumSlots(a => a.StrengthBonus, AllSlots);
    public int TotalMaxManaBonus => SumSlots(a => a.MaxManaBonus, AllSlots);
    public int TotalSpellSlotsBonus => SumSlots(a => a.SpellSlotsBonus, Head, Neck, Back, Hands);

    public int TotalMovementPenalty => SumSlots(a => a.MovementPenalty, GearSlots);

    public int TotalResistance(ResistanceType type) =>
        SumSlots(a => a.Resistances.Where(r => r.Type == type).Sum(r => r.Value),
            GearSlots);

    private static int SumSlots(Func<Armor, int> selector, params Armor?[] slots)
    {
        var total = 0;
        for (var i = 0; i < slots.Length; i++)
            if (slots[i] != null)
                total += selector(slots[i]!);
        return total;
    }
}
