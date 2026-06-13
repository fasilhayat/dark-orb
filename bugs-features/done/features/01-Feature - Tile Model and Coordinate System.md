# Feature - Tile Model and Coordinate System

Project: Dark Orb

File: `feature-tile-coordinate-system.md`

Dependencies: None (foundation task)

---

## Objective

Create the mathematical and data-model foundation for all isometric rendering. This task delivers the coordinate translator and tile data types — no pixels are drawn yet.

---

## Scope

New files only, all under `BattleArena.Gui/`:

```
BattleArena.Gui/
├── Models/
│   └── World/
│       ├── TileType.cs             # Enum: Grass, Road, Forest, Water, Mountain, DungeonFloor, DungeonWall, Bridge
│       ├── Tile.cs                 # Record: TileType, MovementCost, IsPassable, (future: TextureKey, LightingModifier)
│       └── TilePosition.cs         # Record: logical (TileX, TileY) with projection helpers
│
└── Rendering/
    └── IsometricCoordinateTranslator.cs  # Static projection service
```

### TileType

```csharp
public enum TileType
{
    Grass,
    Road,
    Forest,
    Water,
    Mountain,
    DungeonFloor,
    DungeonWall,
    Bridge
}
```

Not exhaustive — add new types as needed in later tasks.

### Tile

```csharp
public record Tile(TileType Type, int MovementCost, bool IsPassable)
```

Default movement costs implied by TileType (e.g., Water = impassable, Forest = 2, Road = 1). No DB or config loading in this task.

### TilePosition

```csharp
public readonly record struct TilePosition(int TileX, int TileY)
```

### IsometricCoordinateTranslator

```csharp
public static class IsometricCoordinateTranslator
{
    public static PixelPosition TileToScreen(TilePosition tile, int tileWidth, int tileHeight)
    public static TilePosition ScreenToTile(PixelPosition screen, int tileWidth, int tileHeight)
}
```

Formula (from spec):

```csharp
screenX = (tileX - tileY) * (tileWidth / 2);
screenY = (tileX + tileY) * (tileHeight / 2);
```

- `PixelPosition` is a simple `record struct` with `X`/`Y` doubles.
- Translator must be a single isolated service — conversion logic must never be duplicated.

---

## Location

All files go in new directories:
- `BattleArena.Gui/Models/World/`
- `BattleArena.Gui/Rendering/`

Completely separate from existing `Models/CharacterDisplayItem.cs` and `Models/SpellDisplayItem.cs`.

---

## Non-goals

- No rendering or drawing
- No tile map data structures (TileGrid/MapChunk — deferred to task 02)
- No performance optimization
- No unit of measurement beyond pixels and tiles

---

## Acceptance Criteria

- [ ] `TileType` enum defined with the 8 above values
- [ ] `Tile` record with Type, MovementCost, IsPassable
- [ ] `TilePosition` record struct
- [ ] `IsometricCoordinateTranslator` converts Tile→Screen and Screen→Tile correctly
- [ ] Round-trip invariant: for any tile with even projection values, ScreenToTile(TileToScreen(t)) == t
- [ ] No changes to existing code outside new directories
