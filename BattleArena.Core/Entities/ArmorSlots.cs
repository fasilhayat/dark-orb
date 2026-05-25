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
    public Weapon? RightHand { get; set; }
    public Weapon? LeftHand { get; set; }

    public int TotalArmorClass => SumSlots(Head, Chest, Hands, Waist, Boots, Neck, Back);

    private static int SumSlots(params Armor?[] slots)
    {
        var total = 0;
        for (var i = 0; i < slots.Length; i++)
            if (slots[i] != null)
                total += slots[i]!.ArmorClass;
        return total;
    }
}
