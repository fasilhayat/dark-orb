namespace BattleArena.Gui.Views.World;

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Models.World;
using Rendering;
using ViewModels.World;

public partial class WorldView : UserControl
{
    private static readonly TimeSpan MoveDuration = TimeSpan.FromMilliseconds(200);

    private WorldInputHandler? _input;
    private DispatcherTimer? _moveTimer;
    private TilePosition _moveFrom;
    private TilePosition _moveTo;
    private DateTime _moveStartTime;

    private Queue<TilePosition>? _pathQueue;
    private DispatcherTimer? _npcAnimTimer;
    private TilePosition? _lastHoveredTile;

    public event Action<string, string>? CombatEncounterRequested;

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
            _input = new WorldInputHandler(vm);
            _input.MoveRequested += target => Dispatcher.UIThread.Post(() => StartMove(vm, target));
            _input.PathRequested += waypoints => Dispatcher.UIThread.Post(() => FollowPath(vm, waypoints));
            _input.InteractRequested += () => Dispatcher.UIThread.Post(() => HandleInteract(vm));

            vm.CombatEncounterTriggered += (hero, enemy) =>
                Dispatcher.UIThread.Post(() => CombatEncounterRequested?.Invoke(hero, enemy));

            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(WorldViewModel.Map))
                {
                    Dispatcher.UIThread.Post(() => RenderMap(vm), DispatcherPriority.Background);
                    Dispatcher.UIThread.Post(() => UpdateZoneDisplay(vm), DispatcherPriority.Background);
                }
            };
            Dispatcher.UIThread.Post(() => RenderMap(vm), DispatcherPriority.Background);
            Dispatcher.UIThread.Post(() => CenterMap(vm), DispatcherPriority.Loaded);
            Dispatcher.UIThread.Post(() => UpdateZoneDisplay(vm), DispatcherPriority.Background);
            Dispatcher.UIThread.Post(() => UpdateNpcState(vm), DispatcherPriority.Background);

            StartNpcAnimation(vm);
        }
    }

    private void UpdateZoneDisplay(WorldViewModel vm)
    {
        ZoneNameText.Text = vm.ZoneName;
        AmbientOverlay.Background = new SolidColorBrush(Color.Parse(vm.MapManager.CurrentZone.AmbientColor));
    }

    private void UpdateNpcState(WorldViewModel vm)
    {
        if (vm.ZoneName == "Cave")
            vm.NpcController?.Stop();
        else
            vm.NpcController?.Start();
    }

    private void StartNpcAnimation(WorldViewModel vm)
    {
        _npcAnimTimer?.Stop();
        _npcAnimTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _npcAnimTimer.Tick += (_, _) =>
        {
            foreach (var npc in vm.Npcs)
                NpcController.UpdateNpcAnimation(npc);

            if (vm.Npcs.Any(n => n.IsMoving))
                CharacterRenderer.RenderNpcs(vm.Npcs, vm.Map, MapCanvas);
        };
        _npcAnimTimer.Start();
    }

    private void RenderMap(WorldViewModel vm)
    {
        var vw = MapClipRegion.Bounds.Width;
        var vh = MapClipRegion.Bounds.Height;
        Viewport? viewport = vw > 0 && vh > 0
            ? vm.Camera.GetViewport(vm.Map.Width, vm.Map.Height,
                TileRenderer.TileWidth, TileRenderer.TileHeight, vw, vh)
            : null;

        TileRenderer.RenderMap(vm.Map, MapCanvas, viewport);
        RenderWorldObjects(vm);
        CharacterRenderer.RenderPlayer(vm.Player, vm.Map, MapCanvas);
        CharacterRenderer.RenderNpcs(vm.Npcs, vm.Map, MapCanvas);
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

    private static FacingDirection GetFacingDirection(TilePosition from, TilePosition to)
    {
        var dx = to.TileX - from.TileX;
        var dy = to.TileY - from.TileY;
        return (dx, dy) switch
        {
            (0, -1) => FacingDirection.North,
            (1, -1) => FacingDirection.NorthEast,
            (1, 0) => FacingDirection.East,
            (1, 1) => FacingDirection.SouthEast,
            (0, 1) => FacingDirection.South,
            (-1, 1) => FacingDirection.SouthWest,
            (-1, 0) => FacingDirection.West,
            (-1, -1) => FacingDirection.NorthWest,
            _ => FacingDirection.South
        };
    }

    private void FollowPath(WorldViewModel vm, IReadOnlyList<TilePosition> waypoints)
    {
        _pathQueue = new Queue<TilePosition>(waypoints);
        AdvancePath(vm);
    }

    private void AdvancePath(WorldViewModel vm)
    {
        if (_pathQueue is null || _pathQueue.Count == 0)
        {
            _pathQueue = null;
            return;
        }

        var next = _pathQueue.Dequeue();
        StartMove(vm, next);
    }

    private void StartMove(WorldViewModel vm, TilePosition target)
    {
        _moveFrom = vm.Player.TilePosition;
        _moveTo = target;
        _moveStartTime = DateTime.UtcNow;
        vm.Player.IsMoving = true;

        _moveTimer?.Stop();
        _moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _moveTimer.Tick += (_, _) => AnimateMove(vm);
        _moveTimer.Start();
    }

    private void AnimateMove(WorldViewModel vm)
    {
        var elapsed = DateTime.UtcNow - _moveStartTime;
        var t = Math.Clamp(elapsed.TotalMilliseconds / MoveDuration.TotalMilliseconds, 0.0, 1.0);

        var tileX = _moveFrom.TileX + (_moveTo.TileX - _moveFrom.TileX) * t;
        var tileY = _moveFrom.TileY + (_moveTo.TileY - _moveFrom.TileY) * t;

        var pos = TileToCanvasPos(vm.Map, (int)tileX, (int)tileY);

        var playerShape = MapCanvas.Children.OfType<Ellipse>().FirstOrDefault();
        if (playerShape is not null)
        {
            Canvas.SetLeft(playerShape, pos.X - CharacterRenderer.PlayerSize / 2.0);
            Canvas.SetTop(playerShape, pos.Y - CharacterRenderer.PlayerSize / 2.0);
        }

        if (t >= 1.0)
        {
            _moveTimer?.Stop();
            _moveTimer = null;
            vm.Player.TilePosition = _moveTo;
            vm.Player.Facing = GetFacingDirection(_moveFrom, _moveTo);
            vm.Player.IsMoving = false;

            var transition = vm.MapManager.GetTransition(_moveTo);
            if (transition is not null)
            {
                _pathQueue = null;
                vm.MapManager.SwitchTo(transition.Value.TargetMapId, transition.Value.SpawnPosition);
                return;
            }

            CharacterRenderer.RenderPlayer(vm.Player, vm.Map, MapCanvas);

            vm.CheckProximity();
            UpdateInteractionPrompt(vm);

            if (_pathQueue is { Count: > 0 })
                AdvancePath(vm);
        }
    }

    private PixelPosition TileToCanvasPos(TileMap map, int tileX, int tileY)
    {
        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(map);
        var screen = IsometricCoordinateTranslator.TileToScreen(
            new TilePosition(tileX, tileY),
            TileRenderer.TileWidth, TileRenderer.TileHeight);
        return new PixelPosition(
            screen.X + offsetX,
            screen.Y + offsetY + TileRenderer.TileHeight / 2.0);
    }

    // ── Hover: tile changes color ──────────────────────────────

    private void UpdateHover(WorldViewModel vm, double canvasX, double canvasY)
    {
        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(vm.Map);
        var tileSpace = new PixelPosition(canvasX - offsetX, canvasY - offsetY);
        var tile = IsometricCoordinateTranslator.ScreenToTile(
            new PixelPosition(tileSpace.X, tileSpace.Y - TileRenderer.TileHeight / 2.0),
            TileRenderer.TileWidth, TileRenderer.TileHeight);

        if (tile.TileX < 0 || tile.TileX >= vm.Map.Width ||
            tile.TileY < 0 || tile.TileY >= vm.Map.Height)
        {
            if (_lastHoveredTile is not null)
            {
                TileRenderer.SetHoveredTile(null);
                _lastHoveredTile = null;
            }
            TileInfoText.Text = "";
            return;
        }

        if (_lastHoveredTile == tile) return;

        _lastHoveredTile = tile;
        TileRenderer.SetHoveredTile(tile);

        var tileType = vm.Map[tile.TileX, tile.TileY].Type;
        var passable = vm.Map[tile.TileX, tile.TileY].IsPassable;
        var cost = vm.Map[tile.TileX, tile.TileY].MovementCost;
        TileInfoText.Text = $"{tileType}  ({tile.TileX},{tile.TileY})  {(passable ? $"cost {cost}" : "blocked")}";
    }

    // ── World objects ──────────────────────────────────────────

    private void RenderWorldObjects(WorldViewModel vm)
    {
        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(vm.Map);

        foreach (var obj in vm.WorldObjects)
        {
            var screen = IsometricCoordinateTranslator.TileToScreen(
                obj.Position, TileRenderer.TileWidth, TileRenderer.TileHeight);
            var cx = screen.X + offsetX;
            var cy = screen.Y + offsetY + TileRenderer.TileHeight / 2.0;

            Shape shape;
            if (obj.Type == WorldObjectType.Door)
            {
                shape = new Rectangle
                {
                    Width = obj.IsOpen ? 20 : 24,
                    Height = obj.IsOpen ? 6 : 24,
                    Fill = new SolidColorBrush(Color.Parse(obj.IsOpen ? "#666666" : "#8B5E3C")),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                };
            }
            else if (obj.Type == WorldObjectType.Chest)
            {
                shape = new Rectangle
                {
                    Width = 16, Height = 12,
                    Fill = new SolidColorBrush(Color.Parse("#DAA520")),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                };
            }
            else if (obj.Type == WorldObjectType.Sign)
            {
                shape = new Rectangle
                {
                    Width = 8, Height = 14,
                    Fill = new SolidColorBrush(Color.Parse("#708090")),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                };
            }
            else
            {
                continue;
            }

            Canvas.SetLeft(shape, cx - shape.Width / 2.0);
            Canvas.SetTop(shape, cy - shape.Height);
            MapCanvas.Children.Add(shape);
        }
    }

    // ── Interaction ────────────────────────────────────────────

    private void UpdateInteractionPrompt(WorldViewModel vm)
    {
        if (vm.ActiveInteraction is { } obj)
        {
            var action = obj.Type switch
            {
                WorldObjectType.Door => obj.IsOpen ? "close" : "open",
                WorldObjectType.Chest => "loot",
                WorldObjectType.Sign => "read",
                _ => "interact"
            };
            InteractionText.Text = $"Press E to {action} {obj.Label}";
            InteractionOverlay.IsVisible = true;
        }
        else
        {
            InteractionOverlay.IsVisible = false;
        }
    }

    private void HandleInteract(WorldViewModel vm)
    {
        var result = vm.Interact();
        if (string.IsNullOrEmpty(result)) return;

        InteractionText.Text = result;
        InteractionOverlay.IsVisible = true;

        if (vm.ActiveInteraction?.Type == WorldObjectType.Door)
            RenderWorldObjects(vm);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            UpdateInteractionPrompt(vm);
        };
        timer.Start();
    }

    // ── Pointer events ─────────────────────────────────────────

    private void OnMapPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(MapClipRegion);
        _input?.OnPointerPressed(pos.X, pos.Y);
        e.Pointer.Capture(MapClipRegion);
        Focus();
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not WorldViewModel vm) return;

        if (_input is { IsDragging: true })
        {
            var pos = e.GetPosition(MapClipRegion);
            _input.OnPointerMoved(pos.X, pos.Y, () => ApplyTransform(vm));
        }
        else
        {
            var pos = e.GetPosition(MapCanvas);
            UpdateHover(vm, pos.X, pos.Y);
        }
    }

    private void OnMapPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var pos = e.GetPosition(MapClipRegion);
        _input?.OnPointerReleased(pos.X, pos.Y, () => ApplyTransform((WorldViewModel)DataContext!));
    }

    private void OnMapPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _input?.OnPointerWheelChanged(e.Delta.Y, () => ApplyTransform((WorldViewModel)DataContext!));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is WorldViewModel vm)
            _input?.OnKeyDown(e.Key, () => ApplyTransform(vm));

        e.Handled = true;
    }
}
