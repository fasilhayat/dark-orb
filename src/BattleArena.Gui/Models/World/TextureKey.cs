namespace BattleArena.Gui.Models.World;

public static class TextureKey
{
    public static string Tile(TileType type) => $"tiles/{type}.png";
    public static string Player => "characters/player.png";
    public static string Npc(string name) => $"characters/npc_{name.ToLowerInvariant()}.png";
}
