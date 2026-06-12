namespace BattleArena.Gui.Models.World;

public class WorldObject
{
    public TilePosition Position { get; set; }
    public WorldObjectType Type { get; set; }
    public string Label { get; set; } = "";
    public bool IsInteractable { get; set; } = true;

    // Door-specific
    public bool IsOpen { get; set; }

    // Sign-specific
    public string SignText { get; set; } = "";
}
