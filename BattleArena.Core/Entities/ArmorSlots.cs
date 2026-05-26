namespace BattleArena.Core.Entities;

public class ArmorSlots
{
    public Armor? Head { get; set; }
    public Armor? Chest { get; set; }
    public Armor? Hands { get; set; }
    public Armor? Waist { get; set; }
    public Armor? Boots { get; set; }
    public Armor? Neck { get; set; }
    public Armor? Back { get; set; }
    public Shield? Shield { get; set; }
    public Weapon? RightHand { get; set; }
    public Weapon? LeftHand { get; set; }

    public int TotalArmorClass => SumSlots(armor => armor.ArmorClass, Head, Chest, Hands, Waist, Boots, Neck, Back);
    public int TotalMitigation => SumSlots(armor => armor.Mitigation, Head, Chest, Hands, Waist, Boots, Neck, Back);
    public int TotalTurnMeterPenalty => SumSlots(armor => armor.TurnMeterPenalty, Head, Chest, Hands, Waist, Boots, Neck, Back);

    private static int SumSlots(Func<Armor, int> selector, params Armor?[] slots)
    {
        var total = 0;
        for (var i = 0; i < slots.Length; i++)
            if (slots[i] != null)
                total += selector(slots[i]!);
        return total;
    }
}
