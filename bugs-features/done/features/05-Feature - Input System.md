# Feature - Input System

Project: Dark Orb

File: `feature-input-system.md`

Dependencies: 03 (Camera System), 04 (Player Character and Movement)

---

## Objective

Consolidate keyboard and mouse input for the world view into a dedicated handler, separate from existing MainWindow event handlers.

---

## Scope

New files:

```
BattleArena.Gui/
└── Rendering/
    └── WorldInputHandler.cs   # Keyboard + mouse input for world view
```

Modified files:

```
BattleArena.Gui/
├── Views/World/
│   └── WorldView.axaml.cs     # Forward key/mouse events to WorldInputHandler
│
└── ViewModels/World/
    └── WorldViewModel.cs      # Modify: expose WorldInputHandler reference
```

### WorldInputHandler

```csharp
public class WorldInputHandler
{
    public void OnKeyDown(Key key);
    public void OnMouseClick(PixelPosition screenPos, TilePosition tilePos);
    public void OnMouseDrag(PixelPosition delta);
    public void OnMouseWheel(int delta);
}
```

### Keyboard bindings

| Key | Action |
|-----|--------|
| W / Up | Move player North |
| S / Down | Move player South |
| A / Left | Move player West |
| D / Right | Move player East |
| Arrow keys | Same as WASD (two sets bound) |

Movement keys move the player 1 tile in that direction. The input handler checks `Tile.IsPassable` before issuing the move.

### Mouse bindings

| Action | Behavior |
|--------|----------|
| Click on passable tile | Move player to that tile (direct path, no A* — task 06) |
| Drag on empty map space | Pan camera (delegates to CameraController) |
| Wheel scroll | Zoom in/out (delegates to CameraController) |

### Integration

`WorldView.axaml.cs` attaches `KeyDown`, `PointerPressed`, `PointerMoved`, `PointerReleased`, and `PointerWheelChanged` events and forwards them to `WorldInputHandler`.

The input handler routes to:
- `CameraController` for pan/zoom
- `PlayerViewModel` for movement
- (future) interaction system for clicking on NPCs/objects

---

## Acceptance Criteria

- [ ] WASD and arrow keys move the player one tile per press
- [ ] Clicking a passable tile moves the player there
- [ ] Mouse drag pans the camera
- [ ] Mouse wheel zooms
- [ ] Input handler is a single isolated class (not scattered in code-behind)
- [ ] Existing MainWindow input handling for combat is unaffected
