namespace BattleArena.Gui.Rendering.Sprites;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Models.World;

public sealed class SpriteSheet
{
    private readonly Bitmap?[] _frames;

    /// <summary>
    /// Loads tiles4.png — 3×3 grid of equal frames in TileType enum order:
    ///   Grass(0) Road(1) Forest(2)
    ///   Water(3) Mountain(4) DungeonFloor(5)
    ///   DungeonWall(6) Bridge(7) DungeonEntrance(8)
    /// </summary>
    private const double TargetSize = 64;

    public SpriteSheet(string sheetPath)
    {
        var sheet = new Bitmap(sheetPath);
        var cols = 3;
        var rows = 3;
        var fw = sheet.PixelSize.Width / cols;
        var fh = sheet.PixelSize.Height / rows;
        _frames = new Bitmap[cols * rows];

        for (var i = 0; i < _frames.Length; i++)
        {
            var col = i % cols;
            var row = i / cols;

            // Downscale the 418px frame to 64px to match the intended art size
            var frame = new RenderTargetBitmap(new PixelSize((int)TargetSize, (int)TargetSize));
            using var ctx = frame.CreateDrawingContext();
            ctx.DrawImage(sheet,
                new Rect(col * fw, row * fh, fw, fh),
                new Rect(0, 0, TargetSize, TargetSize));
            ctx.Dispose();
            _frames[i] = frame;
        }
    }

    public Bitmap? GetTile(TileType type)
    {
        var i = (int)type;
        return i >= 0 && i < _frames.Length ? _frames[i] : null;
    }

    public void Dispose()
    {
        foreach (var f in _frames) f?.Dispose();
    }
}
