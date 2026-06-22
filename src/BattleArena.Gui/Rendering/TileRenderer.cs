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
        if (_hoveredTile is { } old && TilePolygons.TryGetValue(old, out var oldPoly))
        {
            var oldType = _hoveredMap?[old.TileX, old.TileY].Type ?? TileType.Grass;
            Bitmap? tex = null;
            try { tex = CurrentTileset?.GetTile(oldType); }
            catch { }
            oldPoly.Fill = new SolidColorBrush(GetTileColor(oldType), tex is not null ? 0.0 : 1.0);
        }

        _hoveredTile = pos;

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

            // Texture layer — native 64×64 centred on hex, clipped with slight
            // overlap to eliminate anti-aliasing gaps between adjacent tiles.
            Bitmap? texture = null;
            try { texture = CurrentTileset?.GetTile(tile.Type); }
            catch { }

            if (texture is not null)
            {
                const double texSize = 64;
                var imgCx = isoCenter.X + offsetX;
                var imgCy = isoCenter.Y + offsetY;
                var imgLeft = imgCx - texSize / 2.0;
                var imgTop = imgCy - texSize / 2.0;

                // Expand clip vertices by 0.5px outward so adjacent textures overlap
                var overlap = 0.5;
                var cx2 = (points.Min(p => p.X) + points.Max(p => p.X)) / 2.0;
                var cy2 = (points.Min(p => p.Y) + points.Max(p => p.Y)) / 2.0;
                var expandedVerts = points.ConvertAll(p =>
                {
                    var dx = p.X - cx2;
                    var dy = p.Y - cy2;
                    var len = Math.Sqrt(dx * dx + dy * dy);
                    return len > 0
                        ? new Point(p.X + dx / len * overlap - imgLeft,
                                     p.Y + dy / len * overlap - imgTop)
                        : new Point(p.X - imgLeft, p.Y - imgTop);
                });

                var img = new Image
                {
                    Source = texture,
                    Stretch = Stretch.None,
                    Width = texSize,
                    Height = texSize,
                    Clip = new PolylineGeometry(expandedVerts, true),
                };
                Canvas.SetLeft(img, imgLeft);
                Canvas.SetTop(img, imgTop);
                target.Children.Add(img);
            }

            var hex = new Polygon
            {
                Points = points,
                Fill = new SolidColorBrush(color, texture is not null ? 0.0 : 1.0),
                Stroke = texture is not null ? null : new SolidColorBrush(Color.Parse("#1a1a1a")),
                StrokeThickness = texture is not null ? 0 : 0.5,
            };

            Canvas.SetLeft(hex, 0);
            Canvas.SetTop(hex, 0);
            target.Children.Add(hex);

            TilePolygons[new TilePosition(x, y)] = hex;
        }

        // Fog of war overlay — darken tiles not visible to friendly units
        if (FogOfWar.CurrentFog is not null)
        {
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
            {
                if (FogOfWar.IsVisible(x, y)) continue;

                var tilePos = new TilePosition(x, y);
                var flatCenter = HexGrid.GridToScreen(tilePos, HexSize);
                var isoVerts = HexGrid.GetHexVerticesIsometric(flatCenter.X, flatCenter.Y, HexSize);
                var points = isoVerts.ConvertAll(v => new Point(v.X + offsetX, v.Y + offsetY));

                var fog = new Polygon
                {
                    Points = points,
                    Fill = new SolidColorBrush(Color.Parse("#000000"), 0.65),
                    Stroke = null,
                };
                Canvas.SetLeft(fog, 0);
                Canvas.SetTop(fog, 0);
                target.Children.Add(fog);
            }
        }
    }
}
