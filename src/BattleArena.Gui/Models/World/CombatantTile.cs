namespace BattleArena.Gui.Models.World;

public class CombatantTile
{
    public string Name { get; set; } = "";
    public TilePosition Position { get; set; }
    public bool IsHero { get; set; }
    public int MaxHp { get; set; } = 100;
    public int CurrentHp { get; set; } = 100;
}
