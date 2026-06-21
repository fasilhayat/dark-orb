namespace BattleArena.Gui.Rendering.Sprites;

using Avalonia.Media.Imaging;
using Models.World;

public class Tileset
{
    private readonly SpriteCache _cache;
    private readonly SpriteSheet? _sheet;

    public Tileset(SpriteCache cache, SpriteSheet? sheet = null)
    {
        _cache = cache;
        _sheet = sheet;
    }

    public Bitmap? GetTile(TileType type)
    {
        if (_sheet?.GetTile(type) is { } t)
            return t;
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
