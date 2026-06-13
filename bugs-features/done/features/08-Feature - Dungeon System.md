# Feature - Dungeon System

Project: Dark Orb

File: `feature-dungeon-system.md`

Dependencies: 06 (Pathfinding)

---

## Objective

Support multiple maps with transitions between them. The world map and dungeon interiors are separate `TileMap` instances, and the player can cross map boundaries at designated transition points.

---

## Scope

New/modified files:

```
BattleArena.Gui/
├── Models/World/
│   ├── MapTransition.cs          # Record: source edge/position → target map + spawn position
│   └── ZoneDefinition.cs         # Map metadata: name, tileset, ambient color
│
├── ViewModels/World/
│   └── WorldViewModel.cs         # Modify: support current map switching
│
├── Views/World/
│   └── WorldView.axaml           # Modify: ambient overlay per zone
│
└── Rendering/
    └── MapManager.cs             # Holds multiple maps, handles transitions
```

### MapTransition

```csharp
public readonly record struct MapTransition(
    string TargetMapId,
    TilePosition SpawnPosition,
    string? TriggerTileType = null  // e.g., "DungeonEntrance" → auto-trigger
)
```

Placed at specific tile coordinates on a map. When the player steps on that tile, the active map switches.

### ZoneDefinition

```csharp
public record ZoneDefinition(string Name, string MapId, Color AmbientTint)
```

### MapManager

```csharp
public class MapManager
{
    public TileMap CurrentMap { get; }
    public ZoneDefinition CurrentZone { get; }
    public void SwitchToMap(string mapId, TilePosition spawnPos);
}
```

### TestMapData changes

Add a second map: a small 10×10 dungeon interior with DungeonFloor/DungeonWall tiles. Place an entrance tile on the world map that transitions to the dungeon.

### Visual feedback

- Zone name displayed briefly when entering a new map
- Dungeon interior uses a darker ambient overlay (semi-transparent dark overlay)

---

## Acceptance Criteria

- [ ] Two maps exist: world map + dungeon interior
- [ ] Player stepping on the entrance tile transitions to the dungeon
- [ ] Dungeon has a different tile palette (DungeonFloor/DungeonWall)
- [ ] Dungeon uses a darker ambient overlay
- [ ] Player can return from dungeon to world map via exit tile
- [ ] Pathfinding works correctly on both maps
- [ ] NPCs exist only on the world map (not in dungeon — deferred)
- [ ] No changes to combat code
