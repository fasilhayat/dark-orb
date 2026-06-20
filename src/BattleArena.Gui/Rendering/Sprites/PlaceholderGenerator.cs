namespace BattleArena.Gui.Rendering.Sprites;

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Models.World;

public static class PlaceholderGenerator
{
    private static readonly Dictionary<TileType, Color> TileColors = new()
    {
        [TileType.Grass] = Color.Parse("#4a7c3f"),
        [TileType.Road] = Color.Parse("#8b7355"),
        [TileType.Forest] = Color.Parse("#2d5a1e"),
        [TileType.Water] = Color.Parse("#2980b9"),
        [TileType.Mountain] = Color.Parse("#7f8c8d"),
        [TileType.DungeonFloor] = Color.Parse("#555555"),
        [TileType.DungeonWall] = Color.Parse("#333333"),
        [TileType.Bridge] = Color.Parse("#6b4226"),
        [TileType.DungeonEntrance] = Color.Parse("#440000"),
    };

    public static Bitmap CreateTilePlaceholder(TileType type)
    {
        var color = TileColors.GetValueOrDefault(type, Colors.Magenta);
        return CreateHexBitmap(TileRenderer.HexSize, color, Color.Parse("#1a1a1a"));
    }

    public static Bitmap CreatePlayerPlaceholder()
    {
        return CreateCircleBitmap(28, Color.Parse("#00e5ff"), Colors.White);
    }

    public static Bitmap CreateNpcPlaceholder(string name)
    {
        return CreateCircleBitmap(24, Color.Parse("#e67e22"), Colors.White);
    }

    private static Bitmap CreateHexBitmap(double size, Color fill, Color stroke)
    {
        var hexW = (int)Math.Ceiling(HexGrid.HexWidth(size));
        var hexH = (int)Math.Ceiling(HexGrid.HexHeight(size));
        var bitmap = new RenderTargetBitmap(new PixelSize(hexW, hexH));
        using var ctx = bitmap.CreateDrawingContext();

        var cx = hexW / 2.0;
        var cy = hexH / 2.0;
        var verts = HexGrid.GetHexVertices(cx, cy, size);
        var points = new List<Point>();
        foreach (var v in verts)
            points.Add(v);

        var geom = new PolylineGeometry(points, true);
        ctx.DrawGeometry(new SolidColorBrush(fill), new Pen(new SolidColorBrush(stroke), 0.5), geom);
        ctx.Dispose();
        return bitmap;
    }

    private static Bitmap CreateCircleBitmap(int size, Color fill, Color stroke)
    {
        var bitmap = new RenderTargetBitmap(new PixelSize(size, size));
        using var ctx = bitmap.CreateDrawingContext();

        ctx.DrawEllipse(new SolidColorBrush(fill), new Pen(new SolidColorBrush(stroke), 2),
            new Rect(0, 0, size, size));
        ctx.Dispose();
        return bitmap;
    }
}
