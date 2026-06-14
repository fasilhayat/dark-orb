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
    private TilePosition? _lastHovered;

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
        _lastHovered = null;
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

        CombatantTile? hovered = null;
        foreach (var c in vm.Combatants)
        {
            var screen = IsometricCoordinateTranslator.TileToScreen(
                c.Position, TileRenderer.TileWidth, TileRenderer.TileHeight);
            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;

            var dx = canvasX - cx;
            var dy = canvasY - cy;
            var dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist < 20)
            {
                hovered = c;
                break;
            }
        }

        var newPos = hovered?.Position;
        if (newPos == _lastHovered) return;

        _lastHovered = newPos;
        CharacterRenderer.HoveredCombatant = newPos;
        CharacterRenderer.RenderCombatants(vm.Combatants, vm.Map, MapCanvas);

        if (hovered is not null)
            CombatantHovered?.Invoke(hovered.Name);

        TileInfoText.Text = hovered is not null
            ? $"{hovered.Name}  HP: {hovered.CurrentHp}/{hovered.MaxHp}"
            : "";
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
