# Feature - Camera System

Project: Dark Orb

File: `feature-camera-system.md`

Dependencies: 02 (Static Tile Map Renderer)

---

## Objective

Add a camera/viewport that pans and zooms over the isometric tile map. The renderer draws only the visible portion.

---

## Scope

New/modified files:

```
BattleArena.Gui/
├── Rendering/
│   ├── CameraController.cs      # Zoom, pan, center logic
│   └── Viewport.cs              # Visible tile bounds calculator
│
├── Views/World/
│   └── WorldView.axaml.cs       # Modify: forward mouse drag to camera
│
└── ViewModels/World/
    └── WorldViewModel.cs        # Modify: add CameraController, expose zoom
```

### CameraController

```csharp
public class CameraController
{
    public double Zoom { get; set; }           // 0.5 – 3.0, default 1.0
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public TilePosition CenterTile { get; }     // derived

    public void Pan(double dx, double dy);
    public void ZoomIn();
    public void ZoomOut();
    public void CenterOn(TilePosition tile);
    public Viewport GetViewport(int mapWidth, int mapHeight, int tileWidth, int tileHeight);
}
```

### Viewport

```csharp
public readonly record struct Viewport(
    int MinTileX, int MinTileY,
    int MaxTileX, int MaxTileY)
```

Calculates which tiles are visible given camera offset + zoom. TileRenderer uses this to skip offscreen tiles.

### WorldView interaction

- Mouse drag → `CameraController.Pan(dx, dy)`
- Mouse wheel → `CameraController.ZoomIn() / ZoomOut()`
- The `TileRenderer` applies `CameraController.Zoom` as a `ScaleTransform` and offset as `TranslateTransform`

### Performance

The viewport culling ensures only visible tiles are drawn. For the initial 20×20 map this doesn't matter, but the architecture must support the spec's 500+ visible tile target.

---

## Acceptance Criteria

- [ ] `CameraController` with pan, zoom, center-on-tile
- [ ] Mouse drag pans the map
- [ ] Mouse wheel zooms in/out (clamped 0.5–3.0)
- [ ] Only visible tiles are rendered (viewport culling)
- [ ] Tile rendering respects camera offset and zoom
- [ ] No changes to combat code
