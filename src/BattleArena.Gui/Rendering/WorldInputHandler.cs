namespace BattleArena.Gui.Rendering;

using Models.World;

public class WorldInputHandler
{
    private readonly CameraController _camera;
    private double _dragStartX;
    private double _dragStartY;
    private double _lastPointerX;
    private double _lastPointerY;
    public bool IsDragging { get; private set; }

    public WorldInputHandler(CameraController camera)
    {
        _camera = camera;
    }

    public void OnPointerPressed(double x, double y)
    {
        IsDragging = true;
        _dragStartX = x;
        _dragStartY = y;
        _lastPointerX = x;
        _lastPointerY = y;
    }

    public void OnPointerMoved(double x, double y)
    {
        if (!IsDragging) return;
        var dx = x - _lastPointerX;
        var dy = y - _lastPointerY;
        _lastPointerX = x;
        _lastPointerY = y;
        _camera.Pan(dx, dy);
    }

    public void OnPointerReleased()
    {
        IsDragging = false;
    }

    public void OnPointerWheelChanged(double deltaY)
    {
        if (deltaY > 0) _camera.ZoomIn();
        else if (deltaY < 0) _camera.ZoomOut();
    }
}
