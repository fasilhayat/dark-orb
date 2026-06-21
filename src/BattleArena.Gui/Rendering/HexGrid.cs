namespace BattleArena.Gui.Rendering;

using System;
using System.Collections.Generic;
using Avalonia;
using Models.World;

public static class HexGrid
{
    public static double HexWidth(double size) => Math.Sqrt(3) * size;
    public static double HexHeight(double size) => 2.0 * size;

    /// <summary>Flat (top-down) hex center from tile position.</summary>
    public static PixelPosition GridToScreen(TilePosition pos, double size)
    {
        var q = pos.TileX;
        var r = pos.TileY - pos.TileX / 2;

        var cx = size * (Math.Sqrt(3) * q + Math.Sqrt(3) / 2.0 * r);
        var cy = size * (1.5 * r);
        return new PixelPosition(cx, cy);
    }

    /// <summary>Tile position from flat (top-down) screen coordinates.</summary>
    public static TilePosition ScreenToGrid(PixelPosition screen, double size)
    {
        var rf = screen.Y * 2.0 / (3.0 * size);
        var qf = screen.X / (Math.Sqrt(3) * size) - rf / 2.0;

        var xf = qf;
        var zf = rf;
        var yf = -xf - zf;

        var rx = Math.Round(xf);
        var ry = Math.Round(yf);
        var rz = Math.Round(zf);

        var xDiff = Math.Abs(rx - xf);
        var yDiff = Math.Abs(ry - yf);
        var zDiff = Math.Abs(rz - zf);

        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        else if (yDiff > zDiff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        var col = (int)rx;
        var row = (int)rz + col / 2;
        return new TilePosition(col, row);
    }

    /// <summary>Isometric projection: flat → screen.</summary>
    public static PixelPosition FlatToIsometric(PixelPosition flat)
    {
        return new PixelPosition(flat.X - flat.Y, (flat.X + flat.Y) * 0.5);
    }

    /// <summary>Inverse isometric projection: screen → flat.</summary>
    public static PixelPosition IsometricToFlat(PixelPosition iso)
    {
        return new PixelPosition((iso.X + 2 * iso.Y) / 2, (2 * iso.Y - iso.X) / 2);
    }

    /// <summary>Isometric hex center from tile position.</summary>
    public static PixelPosition GridToScreenIsometric(TilePosition pos, double size)
    {
        return FlatToIsometric(GridToScreen(pos, size));
    }

    /// <summary>Tile position from isometric screen coordinates.</summary>
    public static TilePosition ScreenToGridIsometric(PixelPosition screen, double size)
    {
        return ScreenToGrid(IsometricToFlat(screen), size);
    }

    /// <summary>Flat hexagon vertices.</summary>
    public static List<Point> GetHexVertices(double cx, double cy, double size)
    {
        var points = new List<Point>(6);
        for (var i = 0; i < 6; i++)
        {
            var angle = Math.PI / 180.0 * (60.0 * i - 90.0);
            points.Add(new Point(
                cx + size * Math.Cos(angle),
                cy + size * Math.Sin(angle)));
        }
        return points;
    }

    /// <summary>Isometric hexagon vertices from flat center.</summary>
    public static List<Point> GetHexVerticesIsometric(double flatCx, double flatCy, double size)
    {
        var points = new List<Point>(6);
        for (var i = 0; i < 6; i++)
        {
            var angle = Math.PI / 180.0 * (60.0 * i - 90.0);
            var vx = flatCx + size * Math.Cos(angle);
            var vy = flatCy + size * Math.Sin(angle);
            points.Add(new Point(vx - vy, (vx + vy) * 0.5));
        }
        return points;
    }

    /// <summary>Canvas bounds in isometric space (accounts for hexagon extents).</summary>
    public static (double MinX, double MaxX, double MinY, double MaxY) GetCanvasBoundsIsometric(
        TileMap map, double size)
    {
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;
        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            var flat = GridToScreen(new TilePosition(x, y), size);
            var verts = GetHexVerticesIsometric(flat.X, flat.Y, size);
            foreach (var v in verts)
            {
                if (v.X < minX) minX = v.X;
                if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
            }
        }
        return (minX, maxX, minY, maxY);
    }

    /// <summary>Canvas offset for isometric space.</summary>
    public static (double OffsetX, double OffsetY) GetCanvasOffsetIsometric(TileMap map, double size)
    {
        var (minX, _, minY, _) = GetCanvasBoundsIsometric(map, size);
        return (-minX, -minY);
    }

    /// <summary>Hex distance between two tile positions (axial coordinate system).</summary>
    public static int TileDistance(TilePosition a, TilePosition b)
    {
        var aq = a.TileX;
        var ar = a.TileY - a.TileX / 2;
        var bq = b.TileX;
        var br = b.TileY - b.TileX / 2;
        return (Math.Abs(aq - bq) + Math.Abs(ar - br) + Math.Abs(aq + ar - bq - br)) / 2;
    }
}
