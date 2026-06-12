# Feature - Pathfinding

Project: Dark Orb

File: `feature-pathfinding.md`

Dependencies: 04 (Player Character and Movement), 05 (Input System)

---

## Objective

Implement A* pathfinding on the tile grid so the player (and later NPCs) can navigate to any passable tile with obstacle avoidance.

---

## Scope

New files:

```
BattleArena.Gui/
└── Rendering/
    ├── Pathfinder.cs             # A* implementation
    └── PathRequest.cs            # Immutable request/result data types
```

Modified files:

```
BattleArena.Gui/
├── ViewModels/World/
│   └── PlayerViewModel.cs        # Modify: accept path instead of single-tile move
│
└── Rendering/
    └── WorldInputHandler.cs      # Modify: use pathfinder for click-to-move
```

### Pathfinder

```csharp
public static class Pathfinder
{
    public static PathResult FindPath(TileMap map, TilePosition start, TilePosition end);
}
```

### A* implementation details

- Standard Manhattan distance heuristic
- Movement cost from `Tile.MovementCost`
- Impassable tiles are excluded from the graph
- Supports cardinal + diagonal movement (8-directional)
- Returns ordered list of `TilePosition` waypoints

### PathRequest / PathResult

```csharp
public readonly record struct PathRequest(TileMap Map, TilePosition Start, TilePosition End);

public readonly record struct PathResult(
    IReadOnlyList<TilePosition> Waypoints,
    bool IsReachable,
    int TotalCost);
```

### Integration

- Click-to-move on an unreachable tile → no movement, no error (silent ignore)
- Click-to-move on a reachable tile → `Pathfinder.FindPath` → `PlayerViewModel.FollowPath(waypoints)`
- `PlayerViewModel.FollowPath` dequeues waypoints one at a time with smooth animation
- WASD/arrow keys still move one tile directly (bypass pathfinding)

### Performance

The A* implementation must handle maps up to 100×100 tiles without visible delay on the UI thread. Use `ValueTuple` for open set priority and a simple closed-set lookup. No threading needed for the initial 20×20 maps.

---

## Acceptance Criteria

- [ ] A* pathfinding works on the tile grid
- [ ] Click on a distant passable tile navigates the player there via shortest path
- [ ] Obstacles (impassable tiles) are correctly avoided
- [ ] Click on an unreachable tile does nothing
- [ ] WASD movement still works one tile at a time (bypasses pathfinding)
- [ ] Pathfinding completes within a single frame on 20×20 maps
- [ ] No changes to combat code
