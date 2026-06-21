namespace BattleArena.Gui.Rendering.Sprites;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Models.World;

public sealed class SpriteSheet
{
    /// <summary>Source rectangle per TileType in the 1330×1182 sprite sheet.</summary>
    private static readonly (int X, int Y, int W, int H)[] TileRects =
    {
        /* Grass           */ (0,   0,   443, 394),
        /* Road            */ (443, 0,   443, 394),
        /* Forest          */ (886, 0,   444, 394),
        /* Water           */ (0,   394, 443, 394),
        /* Mountain        */ (443, 394, 443, 394),
        /* DungeonFloor    */ (886, 394, 444, 394),
        /* DungeonWall     */ (0,   788, 443, 394),
        /* Bridge          */ (443, 788, 443, 394),
        /* DungeonEntrance */ (886, 788, 444, 394),
    };

    private readonly Bitmap _sheet;
    private readonly Bitmap?[] _frames;

    public SpriteSheet(string path)
    {
        _sheet = new Bitmap(path);
        _frames = new Bitmap[TileRects.Length];

        for (var i = 0; i < TileRects.Length; i++)
        {
            var (x, y, w, h) = TileRects[i];
            var frame = new RenderTargetBitmap(new PixelSize(w, h));
            using var ctx = frame.CreateDrawingContext();
            ctx.DrawImage(_sheet, new Rect(x, y, w, h), new Rect(0, 0, w, h));
            ctx.Dispose();
            _frames[i] = frame;
        }
    }

    public Bitmap? GetTile(TileType type)
    {
        var i = (int)type;
        return i >= 0 && i < _frames.Length ? _frames[i] : null;
    }

    public void Dispose() => _sheet.Dispose();
}
