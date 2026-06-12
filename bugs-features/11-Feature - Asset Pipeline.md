# Feature - Asset Pipeline

Project: Dark Orb

File: `feature-asset-pipeline.md`

Dependencies: 02 (Static Tile Map Renderer), 04 (Player Character), 07 (NPC System)

---

## Objective

Replace all placeholder colored shapes with sprite-based rendering. Load and cache tile spritesheets, character sprites, and environment object images from the asset directory.

---

## Scope

New files:

```
BattleArena.Gui/
├── Assets/World/                          # Sprite assets directory
│   ├── tiles/
│   ├── characters/
│   └── objects/
│
├── Rendering/
│   ├── SpriteCache.cs                     # Load + cache bitmap sprites
│   ├── Tileset.cs                         # Maps TileType → sprite frame
│   └── SpriteAtlas.cs                     # Spritesheet frame definitions
│
└── Models/World/
    └── TextureKey.cs                      # Enum or string identifier for sprites
```

Modified files:

```
BattleArena.Gui/
├── BattleArena.Gui.csproj                # Modify: include new asset directory
├── Rendering/
│   ├── TileRenderer.cs                    # Modify: use sprites instead of colored borders
│   └── CharacterRenderer.cs               # Modify: use sprites instead of colored shapes
│
└── Models/World/
    └── Tile.cs                            # Modify: add TextureKey field
```

### SpriteCache

```csharp
public class SpriteCache
{
    public Bitmap GetSprite(string path);
    public void PreloadAll();
}
```

Loads sprite PNGs from `Assets/World/` on startup. Caches in a `Dictionary<string, Bitmap>`. Pre-warm with `PreloadAll()` during the loading screen.

### Tileset

```csharp
public class Tileset
{
    public Bitmap GetTileTexture(TileType type);
}
```

Maps each `TileType` to the correct sprite. If a sprite file is missing, falls back to the colored-border renderer (graceful degradation).

### Character sprites

- Player: `Assets/World/characters/player.png` (simple icon/avatar — 64×64 isometric)
- NPCs: `Assets/World/characters/npc_merchant.png`, `npc_guard.png`, `npc_villager.png`

### Artist notes

- Initial sprites can be simple colored icons created in a paint tool — placeholder until real art
- Tiles should be diamond-shaped isometric sprites (e.g., 128×64 pixels)
- All sprites are loaded, not embedded as Avalonia resources

---

## Acceptance Criteria

- [ ] `SpriteCache` loads and caches PNG sprites from `Assets/World/`
- [ ] `TileRenderer` uses sprites from the tileset instead of colored borders
- [ ] `CharacterRenderer` uses sprite images instead of colored shapes
- [ ] Missing sprites fall back gracefully to placeholder colors
- [ ] All sprites preloaded at app startup (not loaded per-frame)
- [ ] No changes to combat code
