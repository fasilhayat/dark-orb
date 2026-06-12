# Feature - Static Tile Map Renderer

Project: Dark Orb

File: `feature-static-tile-map-renderer.md`

Dependencies: 01 (Tile Model and Coordinate System)

---

## Objective

Render a 2D tile grid as an isometric map using Avalonia controls. No interaction, no movement — just visible tiles on screen.

---

## Scope

New files:

```
BattleArena.Gui/
├── Models/World/
│   ├── TileMap.cs              # 2D grid of Tile, map dimensions
│   └── TestMapData.cs          # Static sample map factory (for development)
│
├── Rendering/
│   └── TileRenderer.cs         # Draws a TileMap into an Avalonia Canvas
│
├── Views/World/
│   └── WorldView.axaml         # UserControl hosting the tile canvas
│   └── WorldView.axaml.cs      # Code-behind for initial render
│
└── ViewModels/World/
    └── WorldViewModel.cs       # Holds TileMap, bounds, exposes render data
```

### TileMap

```csharp
public class TileMap
{
    public int Width { get; }
    public int Height { get; }
    public Tile this[int x, int y] { get; }
}
```

Flat array internally, row-major. Populated by `TestMapData` in this task — no file loading or DB.

### TestMapData

A static factory returning a hardcoded `TileMap` (e.g., 20×20 with a grass field, a road, a pond). This is throwaway development scaffolding — delete when real map data arrives (task 11).

### TileRenderer

Renders the `TileMap` using the existing Avalonia approach (`ItemsControl` with `DataTemplates` or `Canvas` with `RenderTransform` for the isometric projection). Each tile is drawn as a colored `Border` or `Rectangle` positioned via `IsometricCoordinateTranslator`.

Per-tile color mapping:
- Grass = `#4a7c3f`
- Road = `#8b7355`
- Forest = `#2d5a1e`
- Water = `#2980b9`
- Mountain = `#7f8c8d`
- DungeonFloor = `#555555`
- DungeonWall = `#333333`
- Bridge = `#6b4226`

### WorldView.axaml

A `UserControl` that sits in a new "World Exploration" phase within MainWindow. The existing phase navigation pattern is used:

```xml
<Grid IsVisible="{Binding IsWorldPhase}">
    <views:WorldView DataContext="{Binding WorldViewModel}" />
</Grid>
```

Add a button on the main menu to enter this phase.

### WorldViewModel

```csharp
public class WorldViewModel : INotifyPropertyChanged
{
    public TileMap Map { get; }
}
```

Follows existing `INotifyPropertyChanged` pattern (no MVVM framework).

---

## Constraints

- All new files go in `Models/World/`, `Rendering/`, `Views/World/`, `ViewModels/World/`
- Do not modify `MainWindow.axaml` beyond adding the new phase Grid + menu button
- Do not modify `MainWindowViewModel` beyond adding `IsWorldPhase` and a `WorldViewModel` property
- Existing combat phases must be untouched

---

## Acceptance Criteria

- [ ] `TileMap` class with indexed tile access
- [ ] `TestMapData` provides a 20×20 hardcoded map
- [ ] `TileRenderer` draws colored tiles in isometric layout
- [ ] `WorldView` UserControl renders inside MainWindow via phase toggle
- [ ] Main menu has a button to enter world view
- [ ] All tiles use correct colors per type
- [ ] No existing combat code modified
