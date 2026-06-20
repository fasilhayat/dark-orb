namespace BattleArena.Gui.Rendering;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.World;
using Sprites;

public static class TileRenderer
{
    public const double HexSize = 22;

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
        return Color.FromArgb(255,
            (byte)Math.Min(255, c.R + 60),
            (byte)Math.Min(255, c.G + 60),
            (byte)Math.Min(255, c.B + 60));
    }

    public static void SetHoveredTile(TilePosition? pos)
    {
        if (_hoveredTile is { } old && TilePolygons.TryGetValue(old, out var oldPoly))
            oldPoly.Fill = new SolidColorBrush(GetTileColor(_hoveredMap?[old.TileX, old.TileY].Type ?? TileType.Grass));

        _hoveredTile = pos;

        if (pos is { } p && TilePolygons.TryGetValue(p, out var newPoly))
        {
            var type = _hoveredMap?[p.TileX, p.TileY].Type ?? TileType.Grass;
            newPoly.Fill = new SolidColorBrush(GetHoverColor(type));
        }
    }

    private static TileMap? _hoveredMap;

    public static (double OffsetX, double OffsetY) GetCanvasOffset(TileMap map)
    {
        return HexGrid.GetCanvasOffsetIsometric(map, HexSize);
    }

    public static void RenderMap(TileMap map, Canvas target, Viewport? clipViewport = null)
    {
        target.Children.Clear();
        TilePolygons.Clear();
        _hoveredTile = null;
        _hoveredMap = map;

        var (minX, maxX, minY, maxY) = HexGrid.GetCanvasBoundsIsometric(map, HexSize);
        var offsetX = -minX;
        var offsetY = -minY;
        target.Width = maxX - minX;
        target.Height = maxY - minY;

        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            if (clipViewport.HasValue && !clipViewport.Value.Contains(x, y))
                continue;

            var tile = map[x, y];
            var flatCenter = HexGrid.GridToScreen(new TilePosition(x, y), HexSize);

            var isoCenter = HexGrid.FlatToIsometric(flatCenter);
            var cx = isoCenter.X + offsetX;
            var cy = isoCenter.Y + offsetY;

            var color = GetTileColor(tile.Type);
            var rawVerts = HexGrid.GetHexVerticesIsometric(flatCenter.X, flatCenter.Y, HexSize);
            var hex = new Polygon
            {
                Points = rawVerts.ConvertAll(v => new Point(v.X + offsetX, v.Y + offsetY)),
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.Parse("#1a1a1a")),
                StrokeThickness = 0.5,
            };

            Canvas.SetLeft(hex, 0);
            Canvas.SetTop(hex, 0);
            target.Children.Add(hex);

            TilePolygons[new TilePosition(x, y)] = hex;
        }
    }
}
