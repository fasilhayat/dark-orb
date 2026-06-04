namespace BattleArena.Presentation;

public class CharDisplayState
{
    public required string Name { get; init; }
    public required int MaxHp { get; init; }
    public int Level { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string Sex { get; init; } = "Unknown";
    public required string Race { get; init; }
    public int Hp { get; set; }
    public int Tm { get; set; }
    public int MaxMana { get; set; }
    public int Mana { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsTmLocked { get; set; }
    public string? CcStatus { get; set; }
    public string Weapon { get; set; } = "";
    public string WeaponStats { get; init; } = "";
    public string ArmorName { get; init; } = "";
    public int ArmorClass { get; init; }
    public int StrikeRating { get; init; }
    public int MagicResistance { get; init; }
    public List<string> ActiveEffects { get; set; } = new();
}
