namespace BattleArena.Gui.Rendering;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.World;
using Sprites;

public static class TileRenderer
{
    public const int TileWidth = 64;
    public const int TileHeight = 32;

    public static Tileset? CurrentTileset { get; set; }
    public static Dictionary<TilePosition, Polygon> TilePolygons { get; } = new();

    private static TilePosition? _hoveredTile;
    private static readonly Dictionary<TileType, Color> BaseColors = new()
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

    public static Color GetTileColor(TileType type) =>
        BaseColors.GetValueOrDefault(type, Colors.Magenta);

    private static Color GetHoverColor(TileType type)
    {
        var c = GetTileColor(type);
        // Brighten by mixing with white
        return Color.FromArgb(255,
            (byte)Math.Min(255, c.R + 60),
            (byte)Math.Min(255, c.G + 60),
            (byte)Math.Min(255, c.B + 60));
    }

    public static void SetHoveredTile(TilePosition? pos)
    {
        // Restore previous hovered tile color
        if (_hoveredTile is { } old && TilePolygons.TryGetValue(old, out var oldPoly))
            oldPoly.Fill = new SolidColorBrush(GetTileColor(_hoveredMap?[old.TileX, old.TileY].Type ?? TileType.Grass));

        _hoveredTile = pos;

        // Set new hovered tile color
        if (pos is { } p && TilePolygons.TryGetValue(p, out var newPoly))
        {
            var type = _hoveredMap?[p.TileX, p.TileY].Type ?? TileType.Grass;
            newPoly.Fill = new SolidColorBrush(GetHoverColor(type));
        }
    }

    private static TileMap? _hoveredMap;

    public static (double OffsetX, double OffsetY) GetCanvasOffset(TileMap map)
    {
        var (minX, _, minY, _) = GetMapBounds(map);
        return (-minX, -minY);
    }

    private static (int MinX, int MaxX, int MinY, int MaxY) GetMapBounds(TileMap map)
    {
        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            var screen = IsometricCoordinateTranslator.TileToScreen(
                new TilePosition(x, y), TileWidth, TileHeight);
            if (screen.X < minX) minX = (int)screen.X;
            if (screen.X > maxX) maxX = (int)screen.X;
            if (screen.Y < minY) minY = (int)screen.Y;
            if (screen.Y > maxY) maxY = (int)screen.Y;
        }
        return (minX, maxX, minY, maxY);
    }

    public static void RenderMap(TileMap map, Canvas target, Viewport? clipViewport = null)
    {
        target.Children.Clear();
        TilePolygons.Clear();
        _hoveredTile = null;
        _hoveredMap = map;

        var (minX, maxX, minY, maxY) = GetMapBounds(map);
        var offsetX = -minX;
        var offsetY = -minY;

        target.Width = maxX - minX + TileWidth;
        target.Height = maxY - minY + TileHeight;

        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            if (clipViewport.HasValue && !clipViewport.Value.Contains(x, y))
                continue;

            var tile = map[x, y];
            var screen = IsometricCoordinateTranslator.TileToScreen(
                new TilePosition(x, y), TileWidth, TileHeight);

            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY;

            var color = GetTileColor(tile.Type);
            var diamond = new Polygon
            {
                Points = new List<Point>
                {
                    new(cx, cy),
                    new(cx + TileWidth / 2.0, cy + TileHeight / 2.0),
                    new(cx, cy + TileHeight),
                    new(cx - TileWidth / 2.0, cy + TileHeight / 2.0),
                },
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.Parse("#1a1a1a")),
                StrokeThickness = 0.5,
            };

            Canvas.SetLeft(diamond, 0);
            Canvas.SetTop(diamond, 0);
            target.Children.Add(diamond);

            TilePolygons[new TilePosition(x, y)] = diamond;
        }
    }
}
