# Feature - Player Character and Movement

Project: Dark Orb

File: `feature-player-character-movement.md`

Dependencies: 02 (Static Tile Map Renderer), 03 (Camera System)

---

## Objective

Place a player character on the isometric map and allow tile-by-tile movement with smooth interpolation between tiles.

---

## Scope

New/modified files:

```
BattleArena.Gui/
├── Models/World/
│   ├── CharacterEntity.cs        # Player state: TilePosition, FacingDirection, MovementState
│   └── FacingDirection.cs        # Enum: North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
│
├── ViewModels/World/
│   ├── WorldViewModel.cs         # Modify: add PlayerCharacter property
│   └── PlayerViewModel.cs        # Animation state, tile position binding
│
├── Views/World/
│   └── WorldView.axaml           # Modify: overlay player sprite on tile map
│
└── Rendering/
    └── CharacterRenderer.cs      # Draws a character at a tile position
```

### CharacterEntity

```csharp
public class CharacterEntity
{
    public TilePosition Position { get; set; }
    public FacingDirection Facing { get; set; }
    public bool IsMoving { get; set; }
}
```

Simple state holder — no logic.

### PlayerViewModel

```csharp
public class PlayerViewModel : INotifyPropertyChanged
{
    public TilePosition TilePosition { get; }
    public PixelPosition ScreenPosition { get; }  // interpolated
    public FacingDirection Facing { get; }
    public bool IsMoving { get; }
}
```

Follows existing `INotifyPropertyChanged` pattern. Handles smooth interpolation: when the player moves from tile A to tile B, the screen position animates over ~200ms.

### Movement

- Click on a passable tile → player moves to that tile (direct, no pathfinding yet — pathfinding is task 06)
- The movement is one tile at a time for now
- Animated via `DispatcherTimer` or `Task.Delay` loop (matching existing GUI animation patterns)
- `IsPassable` check from `TileMap`

### CharacterRenderer

Draws a colored rectangle/ellipse at the player's `ScreenPosition`. Use a distinct color (e.g., cyan `#00e5ff`). This is a placeholder visual — real sprites come in task 11.

### FacingDirection

Determined by the direction of the last move (e.g., moving from (5,5) to (6,4) → SouthEast). Affects future sprite rendering.

---

## Acceptance Criteria

- [ ] Player character visible on the tile map as a colored shape
- [ ] Click on adjacent passable tile moves the player there
- [ ] Movement is smoothly animated (not teleporting)
- [ ] Player cannot move onto impassable tiles
- [ ] Facing direction updates after each move
- [ ] No changes to combat code
