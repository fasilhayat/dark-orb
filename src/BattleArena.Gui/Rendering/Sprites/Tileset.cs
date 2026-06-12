namespace BattleArena.Gui.Rendering.Sprites;

using Avalonia.Media.Imaging;
using Models.World;

public class Tileset
{
    private readonly SpriteCache _cache;

    public Tileset(SpriteCache cache)
    {
        _cache = cache;
    }

    public Bitmap? GetTile(TileType type)
    {
        return _cache.GetSprite(TextureKey.Tile(type)) ?? PlaceholderGenerator.CreateTilePlaceholder(type);
    }

    public Bitmap? GetPlayerSprite()
    {
        return _cache.GetSprite(TextureKey.Player) ?? PlaceholderGenerator.CreatePlayerPlaceholder();
    }

    public Bitmap? GetNpcSprite(string name)
    {
        return _cache.GetSprite(TextureKey.Npc(name)) ?? PlaceholderGenerator.CreateNpcPlaceholder(name);
    }
}
