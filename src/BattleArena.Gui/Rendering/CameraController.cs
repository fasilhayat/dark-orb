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

    public Viewport GetViewport(int mapWidth, int mapHeight, double hexSize,
        double viewWidth, double viewHeight)
    {
        var left = -OffsetX / Zoom;
        var top = -OffsetY / Zoom;
        var right = (viewWidth - OffsetX) / Zoom;
        var bottom = (viewHeight - OffsetY) / Zoom;

        var topLeft = HexGrid.ScreenToGridIsometric(new PixelPosition(left, top), hexSize);
        var bottomRight = HexGrid.ScreenToGridIsometric(new PixelPosition(right, bottom), hexSize);

        var minX = Math.Min(topLeft.TileX, bottomRight.TileX) - 1;
        var maxX = Math.Max(topLeft.TileX, bottomRight.TileX) + 1;
        var minY = Math.Min(topLeft.TileY, bottomRight.TileY) - 1;
        var maxY = Math.Max(topLeft.TileY, bottomRight.TileY) + 1;

        return new Viewport(
            Math.Clamp(minX, 0, mapWidth - 1),
            Math.Clamp(minY, 0, mapHeight - 1),
            Math.Clamp(maxX, 0, mapWidth - 1),
            Math.Clamp(maxY, 0, mapHeight - 1));
    }
}
