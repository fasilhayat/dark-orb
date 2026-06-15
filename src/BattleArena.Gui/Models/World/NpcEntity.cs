namespace BattleArena.Gui.Models.World;

public class NpcEntity
{
    public string Name { get; set; } = "";
    public TilePosition Position { get; set; }
    public NpcBehavior Behavior { get; set; }
    public IReadOnlyList<TilePosition>? PatrolRoute { get; set; }
    public FacingDirection Facing { get; set; }

    // Animation state
    public bool IsMoving { get; set; }
    public TilePosition MoveFrom { get; set; }
    public TilePosition MoveTo { get; set; }
    public DateTime MoveStartTime { get; set; }
}
