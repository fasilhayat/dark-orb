namespace BattleArena.Presentation;

public class CharDisplayState
{
    public required string Name { get; init; }
    public required int MaxHp { get; init; }
    public required bool IsHero { get; init; }
    public int Hp { get; set; }
    public int Tm { get; set; }
    public int MaxMana { get; set; }
    public int Mana { get; set; }
    public bool IsAlive { get; set; } = true;
    public string Weapon { get; set; } = "";
}
