namespace BattleArena.Gui.Rendering;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.World;
using Sprites;
using ViewModels.World;

public static class CharacterRenderer
{
    public const double PlayerSize = 28;
    public const double NpcSize = 24;

    public static Tileset? CurrentTileset { get; set; }

    private static readonly SolidColorBrush PlayerFill = new(Color.Parse("#00e5ff"));
    private static readonly Pen PlayerBorder = new(new SolidColorBrush(Color.Parse("#ffffff")), 2.5);
    private static readonly SolidColorBrush NpcFill = new(Color.Parse("#e67e22"));
    private static readonly Pen NpcBorder = new(new SolidColorBrush(Color.Parse("#ffffff")), 1.5);

    public static void RenderPlayer(PlayerViewModel player, TileMap map, Canvas target)
    {
        RemoveExisting<Ellipse>(target);
        RemoveExisting<Avalonia.Controls.Image>(target, "player");

        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(map);
        var screen = IsometricCoordinateTranslator.TileToScreen(
            player.TilePosition, TileRenderer.TileWidth, TileRenderer.TileHeight);
        var cx = screen.X + offsetX;
        var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;

        var sprite = CurrentTileset?.GetPlayerSprite();
        if (sprite is not null)
        {
            var image = new Avalonia.Controls.Image
            {
                Source = sprite,
                Width = PlayerSize + 8,
                Height = PlayerSize + 8,
                Tag = "player",
            };
            Canvas.SetLeft(image, cx - (PlayerSize + 8) / 2.0);
            Canvas.SetTop(image, cy - (PlayerSize + 8) / 2.0);
            target.Children.Add(image);
        }
        else
        {
            var ellipse = new Ellipse
            {
                Width = PlayerSize,
                Height = PlayerSize,
                Fill = PlayerFill,
                Stroke = PlayerBorder.Brush,
                StrokeThickness = PlayerBorder.Thickness,
            };
            Canvas.SetLeft(ellipse, cx - PlayerSize / 2.0);
            Canvas.SetTop(ellipse, cy - PlayerSize / 2.0);
            target.Children.Add(ellipse);
        }
    }

    public static void RenderNpcs(IReadOnlyList<NpcEntity> npcs, TileMap map, Canvas target)
    {
        RemoveExisting<StackPanel>(target);

        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(map);

        foreach (var npc in npcs)
        {
            TilePosition pos;
            if (npc.IsMoving)
            {
                var t = Math.Clamp(
                    (DateTime.UtcNow - npc.MoveStartTime).TotalMilliseconds / 400.0, 0.0, 1.0);
                pos = new TilePosition(
                    (int)(npc.MoveFrom.TileX + (npc.MoveTo.TileX - npc.MoveFrom.TileX) * t),
                    (int)(npc.MoveFrom.TileY + (npc.MoveTo.TileY - npc.MoveFrom.TileY) * t));
            }
            else
            {
                pos = npc.Position;
            }

            var screen = IsometricCoordinateTranslator.TileToScreen(
                pos, TileRenderer.TileWidth, TileRenderer.TileHeight);
            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;

            var container = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var sprite = CurrentTileset?.GetNpcSprite(npc.Name);
            if (sprite is not null)
            {
                container.Children.Add(new Avalonia.Controls.Image
                {
                    Source = sprite,
                    Width = NpcSize + 4,
                    Height = NpcSize + 4,
                });
            }
            else
            {
                container.Children.Add(new Rectangle
                {
                    Width = NpcSize,
                    Height = NpcSize,
                    Fill = NpcFill,
                    Stroke = NpcBorder.Brush,
                    StrokeThickness = NpcBorder.Thickness,
                });
            }

            container.Children.Add(new TextBlock
            {
                Text = npc.Name,
                Foreground = new SolidColorBrush(Color.Parse("#cccccc")),
                FontSize = 9,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            });

            Canvas.SetLeft(container, cx - NpcSize / 2.0);
            Canvas.SetTop(container, cy - NpcSize / 2.0 - 20);
            target.Children.Add(container);
        }
    }

    private static void RemoveExisting<T>(Canvas target, string? tag = null) where T : Control
    {
        var existing = target.Children.OfType<T>()
            .Where(c => tag is null || (c.Tag?.ToString() == tag))
            .ToList();
        foreach (var e in existing)
            target.Children.Remove(e);
    }
}
