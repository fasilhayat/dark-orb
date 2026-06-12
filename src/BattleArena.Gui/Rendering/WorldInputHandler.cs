namespace BattleArena.Gui.Rendering;

using Avalonia.Input;
using Models.World;
using ViewModels.World;

public class WorldInputHandler
{
    private readonly WorldViewModel _vm;
    private double _dragStartX;
    private double _dragStartY;
    private double _lastPointerX;
    private double _lastPointerY;
    public bool IsDragging { get; private set; }

    public event Action<TilePosition>? MoveRequested;
    public event Action<IReadOnlyList<TilePosition>>? PathRequested;
    public event Action? InteractRequested;

    public WorldInputHandler(WorldViewModel vm)
    {
        _vm = vm;
    }

    public void OnPointerPressed(double x, double y)
    {
        IsDragging = true;
        _dragStartX = x;
        _dragStartY = y;
        _lastPointerX = x;
        _lastPointerY = y;
    }

    public void OnPointerMoved(double x, double y, Action applyTransform)
    {
        if (!IsDragging) return;

        var dx = x - _lastPointerX;
        var dy = y - _lastPointerY;
        _lastPointerX = x;
        _lastPointerY = y;

        _vm.Camera.Pan(dx, dy);
        applyTransform();
    }

    public void OnPointerReleased(double x, double y, Action applyTransform)
    {
        if (!IsDragging) return;

        var dx = x - _dragStartX;
        var dy = y - _dragStartY;
        var dragDist = Math.Sqrt(dx * dx + dy * dy);

        if (dragDist < 10 && !_vm.Player.IsMoving)
        {
            var tile = ScreenToTile(x, y);
            HandleMoveRequest(tile);
        }

        IsDragging = false;
    }

    private void HandleMoveRequest(TilePosition target)
    {
        if (!IsPassable(target)) return;

        var player = _vm.Player;
        var pdx = Math.Abs(target.TileX - player.TilePosition.TileX);
        var pdy = Math.Abs(target.TileY - player.TilePosition.TileY);

        if (pdx <= 1 && pdy <= 1 && (pdx != 0 || pdy != 0))
        {
            // Adjacent tile — move directly
            MoveRequested?.Invoke(target);
        }
        else
        {
            // Distant tile — use A*
            var path = Pathfinder.FindPath(_vm.Map, player.TilePosition, target);
            if (path.IsReachable && path.Waypoints.Count > 0)
                PathRequested?.Invoke(path.Waypoints);
        }
    }

    public void OnPointerWheelChanged(double deltaY, Action applyTransform)
    {
        if (deltaY > 0)
            _vm.Camera.ZoomIn();
        else if (deltaY < 0)
            _vm.Camera.ZoomOut();

        applyTransform();
    }

    public bool OnKeyDown(Key key, Action applyTransform)
    {
        if (_vm.Player.IsMoving) return false;

        // Interact key
        if (key is Key.E or Key.Return)
        {
            if (_vm.ActiveInteraction is not null)
            {
                InteractRequested?.Invoke();
                return true;
            }
            return false;
        }

        var (dx, dy) = key switch
        {
            Key.W or Key.Up => (0, -1),
            Key.S or Key.Down => (0, 1),
            Key.A or Key.Left => (-1, 0),
            Key.D or Key.Right => (1, 0),
            _ => (0, 0)
        };

        if (dx == 0 && dy == 0) return false;

        var current = _vm.Player.TilePosition;
        var target = new TilePosition(current.TileX + dx, current.TileY + dy);

        if (IsPassable(target))
        {
            MoveRequested?.Invoke(target);
            return true;
        }

        return false;
    }

    private bool IsPassable(TilePosition target)
    {
        var map = _vm.Map;
        if (target.TileX < 0 || target.TileX >= map.Width ||
            target.TileY < 0 || target.TileY >= map.Height)
            return false;

        return map[target.TileX, target.TileY].IsPassable;
    }

    private TilePosition ScreenToTile(double screenX, double screenY)
    {
        var canvasPos = _vm.Camera.ScreenToCanvas(screenX, screenY);
        var (offsetX, offsetY) = TileRenderer.GetCanvasOffset(_vm.Map);
        var tileSpace = new PixelPosition(canvasPos.X - offsetX, canvasPos.Y - offsetY);
        return IsometricCoordinateTranslator.ScreenToTile(
            tileSpace, TileRenderer.TileWidth, TileRenderer.TileHeight);
    }
}
