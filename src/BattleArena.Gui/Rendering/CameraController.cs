namespace BattleArena.Gui.Rendering;

using Models.World;

public class CameraController
{
    public const double MinZoom = 0.5;
    public const double MaxZoom = 3.0;
    public const double ZoomStep = 0.15;

    public double Zoom { get; private set; } = 1.0;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }

    public PixelPosition ScreenToCanvas(double screenX, double screenY)
    {
        return new PixelPosition(
            (screenX - OffsetX) / Zoom,
            (screenY - OffsetY) / Zoom);
    }

    public void Pan(double dx, double dy)
    {
        OffsetX += dx;
        OffsetY += dy;
    }

    public void ZoomIn()
    {
        Zoom = Math.Clamp(Zoom + ZoomStep, MinZoom, MaxZoom);
    }

    public void ZoomOut()
    {
        Zoom = Math.Clamp(Zoom - ZoomStep, MinZoom, MaxZoom);
    }

    public void CenterOn(double canvasWidth, double canvasHeight, double viewWidth, double viewHeight)
    {
        OffsetX = (viewWidth - canvasWidth * Zoom) / 2.0;
        OffsetY = (viewHeight - canvasHeight * Zoom) / 2.0;
    }

    public Viewport GetViewport(int mapWidth, int mapHeight, int tileWidth, int tileHeight,
        double viewWidth, double viewHeight)
    {
        // Canvas-space coordinates of the visible area corners
        var left = -OffsetX / Zoom;
        var top = -OffsetY / Zoom;
        var right = (viewWidth - OffsetX) / Zoom;
        var bottom = (viewHeight - OffsetY) / Zoom;

        // Approximate tile bounds (conservative — includes tiles partially on screen)
        var minX = (int)Math.Floor((left / (tileWidth / 2.0) + top / (tileHeight / 2.0)) / 2.0) - 1;
        var maxX = (int)Math.Ceiling((right / (tileWidth / 2.0) + bottom / (tileHeight / 2.0)) / 2.0) + 1;
        var minY = (int)Math.Floor((bottom / (tileHeight / 2.0) - right / (tileWidth / 2.0)) / 2.0) - 1;
        var maxY = (int)Math.Ceiling((top / (tileHeight / 2.0) - left / (tileWidth / 2.0)) / 2.0) + 1;

        return new Viewport(
            Math.Clamp(minX, 0, mapWidth - 1),
            Math.Clamp(minY, 0, mapHeight - 1),
            Math.Clamp(maxX, 0, mapWidth - 1),
            Math.Clamp(maxY, 0, mapHeight - 1));
    }
}
