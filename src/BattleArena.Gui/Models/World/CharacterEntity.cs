namespace BattleArena.Gui.Models.World;

public class CharacterEntity
{
    public TilePosition Position { get; set; }
    public FacingDirection Facing { get; set; }
    public bool IsMoving { get; set; }
}
