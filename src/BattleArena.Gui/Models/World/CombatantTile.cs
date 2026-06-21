namespace BattleArena.Gui.Models.World;

public class CombatantTile
{
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string Race { get; set; } = "";
    public TilePosition Position { get; set; }
    public bool IsHero { get; set; }
    public int MaxHp { get; set; } = 100;
    public int CurrentHp { get; set; } = 100;

    /// <summary>Base sight radius in hexes. Modified by race and class bonuses.</summary>
    public int SightRadius
    {
        get
        {
            var sight = 5;
            sight += Race.ToLowerInvariant() switch
            {
                "elf" or "high elf" or "dark elf" or "wood elf" or "forest elf" => 2,
                "dwarf" or "mountain dwarf" or "hill dwarf" => 1,
                _ => 0,
            };
            sight += ClassName.ToLowerInvariant() switch
            {
                "ranger" => 2,
                "rogue" => 1,
                _ => 0,
            };
            return Math.Max(2, sight);
        }
    }
}
