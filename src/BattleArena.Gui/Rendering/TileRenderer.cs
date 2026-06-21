namespace BattleArena.Gui.Rendering;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        // Unhover previous tile — restore normal fill
        if (_hoveredTile is { } old && TilePolygons.TryGetValue(old, out var oldPoly))
        {
            var type = _hoveredMap?[old.TileX, old.TileY].Type ?? TileType.Grass;
            var hasTexture = CurrentTileset?.GetTile(type) is not null;
            oldPoly.Fill = new SolidColorBrush(GetTileColor(type), hasTexture ? 0.4 : 1.0);
        }

        _hoveredTile = pos;

        // Hover new tile — semi-transparent white overlay
        if (pos is { } p && TilePolygons.TryGetValue(p, out var newPoly))
            newPoly.Fill = new SolidColorBrush(Colors.White, 0.25);
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
            var isoVerts = HexGrid.GetHexVerticesIsometric(flatCenter.X, flatCenter.Y, HexSize);
            var points = isoVerts.ConvertAll(v => new Point(v.X + offsetX, v.Y + offsetY));

            // Texture layer (bottom) — clipped to the hex shape
            Bitmap? texture = null;
            try { texture = CurrentTileset?.GetTile(tile.Type); }
            catch { /* texture unavailable — use solid colour only */ }

            if (texture is not null)
            {
                // Image must be sized to the hex bounding box, with Clip relative
                // to the image's own origin.  Points are in canvas coords, so we
                // compute the hex AABB and translate vertices to local space.
                var bx = points.Min(p => p.X);
                var by = points.Min(p => p.Y);
                var bw = points.Max(p => p.X) - bx;
                var bh = points.Max(p => p.Y) - by;
                var localVerts = points.ConvertAll(p => new Point(p.X - bx, p.Y - by));

                var img = new Image
                {
                    Source = texture,
                    Stretch = Stretch.Fill,
                    Width = bw,
                    Height = bh,
                    Clip = new PolylineGeometry(localVerts, true),
                };
                Canvas.SetLeft(img, bx);
                Canvas.SetTop(img, by);
                target.Children.Add(img);
            }

            // Solid colour overlay (top) — semi-transparent so texture shows through
            var hex = new Polygon
            {
                Points = points,
                Fill = new SolidColorBrush(color, texture is not null ? 0.4 : 1.0),
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
