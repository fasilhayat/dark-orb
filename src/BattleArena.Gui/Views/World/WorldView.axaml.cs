namespace BattleArena.Gui.Views.World;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Models.World;
using Rendering;
using ViewModels.World;

public partial class WorldView : UserControl
{
    private WorldInputHandler? _input;
    private TilePosition? _lastHoveredCombatant;
    private TilePosition? _lastHoveredTile;

    public WorldView()
    {
        InitializeComponent();
        Focusable = true;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is WorldViewModel vm)
        {
            _input = new WorldInputHandler(vm.Camera);

            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(WorldViewModel.Map))
                    Dispatcher.UIThread.Post(() => RenderMap(vm), DispatcherPriority.Background);
            };
            Dispatcher.UIThread.Post(() => RenderMap(vm), DispatcherPriority.Background);
            Dispatcher.UIThread.Post(() => CenterMap(vm), DispatcherPriority.Loaded);
        }
    }

    public event Action<string>? CombatantHovered;
    public Canvas GetMapCanvas() => MapCanvas;

    private void RenderMap(WorldViewModel vm)
    {
        TileRenderer.RenderMap(vm.Map, MapCanvas);
        CharacterRenderer.RenderCombatants(vm.Combatants, vm.Map, MapCanvas);
        ZoneNameText.Text = vm.ZoneName;
        _lastHoveredCombatant = null;
        _lastHoveredTile = null;
        Dispatcher.UIThread.Post(() => CenterMap(vm), DispatcherPriority.Loaded);
    }

    private void CenterMap(WorldViewModel vm)
    {
        var w = MapClipRegion.Bounds.Width;
        var h = MapClipRegion.Bounds.Height;
        if (w <= 0 || h <= 0) return;
        vm.Camera.CenterOn(MapCanvas.Width, MapCanvas.Height, w, h);
        ApplyTransform(vm);
    }

    private void ApplyTransform(WorldViewModel vm)
    {
        var camera = vm.Camera;
        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform(camera.Zoom, camera.Zoom));
        group.Children.Add(new TranslateTransform(camera.OffsetX, camera.OffsetY));
        MapCanvas.RenderTransform = group;
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not WorldViewModel vm) return;

        if (_input is { IsDragging: true })
        {
            _input.OnPointerMoved(e.GetPosition(MapClipRegion).X, e.GetPosition(MapClipRegion).Y);
            ApplyTransform(vm);
        }
        else
        {
            var pos = e.GetPosition(MapCanvas);
            UpdateHover(vm, pos.X, pos.Y);
        }
    }

    private void UpdateHover(WorldViewModel vm, double canvasX, double canvasY)
    {
        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(vm.Map);

        // Check combatant hover first
        CombatantTile? hoveredCombatant = null;
        foreach (var c in vm.Combatants)
        {
            var screen = IsometricCoordinateTranslator.TileToScreen(
                c.Position, TileRenderer.TileWidth, TileRenderer.TileHeight);
            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;
            var dx = canvasX - cx;
            var dy = canvasY - cy;
            if (Math.Sqrt(dx * dx + dy * dy) < 20)
            {
                hoveredCombatant = c;
                break;
            }
        }

        var newCombatantPos = hoveredCombatant?.Position;
        if (newCombatantPos != _lastHoveredCombatant)
        {
            _lastHoveredCombatant = newCombatantPos;
            CharacterRenderer.HoveredCombatant = newCombatantPos;
            CharacterRenderer.RenderCombatants(vm.Combatants, vm.Map, MapCanvas);
            CombatantHovered?.Invoke(hoveredCombatant?.Name ?? "");
            TileInfoText.Text = hoveredCombatant is not null
                ? $"{hoveredCombatant.Name}  HP: {hoveredCombatant.CurrentHp}/{hoveredCombatant.MaxHp}"
                : "";
        }

        // Tile hover: when combatant is hovered, highlight their tile; else highlight cursor tile
        TilePosition? tileToHighlight;
        if (hoveredCombatant is not null)
        {
            tileToHighlight = hoveredCombatant.Position;
        }
        else
        {
            var tileSpace = new PixelPosition(canvasX - offsetX, canvasY - offsetY);
            var tile = IsometricCoordinateTranslator.ScreenToTile(
                new PixelPosition(tileSpace.X, tileSpace.Y - TileRenderer.TileHeight / 2.0),
                TileRenderer.TileWidth, TileRenderer.TileHeight);
            tileToHighlight = tile.TileX >= 0 && tile.TileX < vm.Map.Width &&
                              tile.TileY >= 0 && tile.TileY < vm.Map.Height
                ? tile : null;
        }

        if (tileToHighlight != _lastHoveredTile)
        {
            _lastHoveredTile = tileToHighlight;
            TileRenderer.SetHoveredTile(tileToHighlight);
        }

        if (hoveredCombatant is null && tileToHighlight is not null)
        {
            TileInfoText.Text = $"Tile ({tileToHighlight.Value.TileX}, {tileToHighlight.Value.TileY})";
        }
    }

    private void OnMapPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(MapClipRegion);
        _input?.OnPointerPressed(pos.X, pos.Y);
        e.Pointer.Capture(MapClipRegion);
        Focus();
    }

    private void OnMapPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _input?.OnPointerReleased();
    }

    private void OnMapPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _input?.OnPointerWheelChanged(e.Delta.Y);
        if (DataContext is WorldViewModel vm)
            ApplyTransform(vm);
        e.Handled = true;
    }
}
