namespace BattleArena.Gui.Rendering;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.World;

public static class CharacterRenderer
{
    public const double ArrowWidth = 28;
    public const double ArrowHeight = 32;

    public static TilePosition? HoveredCombatant { get; set; }

    public static void RenderCombatants(IReadOnlyList<CombatantTile> combatants, TileMap map, Canvas target)
    {
        var old = target.Children.OfType<StackPanel>().ToList();
        foreach (var s in old)
            target.Children.Remove(s);

        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(map);

        foreach (var c in combatants)
        {
            var screen = IsometricCoordinateTranslator.TileToScreen(
                c.Position, TileRenderer.TileWidth, TileRenderer.TileHeight);
            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;

            var container = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            // Down-arrow polygon
            var fillColor = c.IsHero ? "#44cc44" : "#cc4444";
            var isHovered = HoveredCombatant == c.Position;
            if (isHovered)
                fillColor = c.IsHero ? "#88ff88" : "#ff8888";

            var arrow = new Polygon
            {
                Points = new List<Point>
                {
                    new(0, 0),
                    new(ArrowWidth, 0),
                    new(ArrowWidth, ArrowHeight * 0.6),
                    new(ArrowWidth * 0.7, ArrowHeight * 0.6),
                    new(ArrowWidth / 2, ArrowHeight),
                    new(ArrowWidth * 0.3, ArrowHeight * 0.6),
                    new(0, ArrowHeight * 0.6),
                },
                Fill = new SolidColorBrush(Color.Parse(fillColor)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1.5,
            };
            container.Children.Add(arrow);

            // Name tooltip (only shown on hover)
            if (isHovered)
            {
                container.Children.Add(new TextBlock
                {
                    Text = c.Name,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 10,
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0),
                });
            }

            Canvas.SetLeft(container, cx - ArrowWidth / 2.0);
            Canvas.SetTop(container, cy - ArrowHeight);
            target.Children.Add(container);
        }
    }
}
