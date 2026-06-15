# Feature - NPC System and Movement

Project: Dark Orb

File: `feature-npc-system.md`

Dependencies: 06 (Pathfinding)

---

## Objective

Place NPCs on the map with basic wandering/patrol AI, and render them alongside the player.

---

## Scope

New files:

```
BattleArena.Gui/
├── Models/World/
│   ├── NpcEntity.cs              # NPC state: position, behavior type, patrol route
│   └── NpcBehavior.cs            # Enum: Stationary, Patrolling, Wandering
│
├── ViewModels/World/
│   └── NpcViewModel.cs           # Per-NPC display state
│
└── Rendering/
    └── NpcController.cs          # Tick-based AI updates for all NPCs
```

Modified files:

```
BattleArena.Gui/
├── Models/World/
│   └── TestMapData.cs            # Modify: add NPC spawn points
│
├── ViewModels/World/
│   └── WorldViewModel.cs         # Modify: hold NPC collection
│
└── Rendering/
    └── CharacterRenderer.cs      # Modify: draw NPCs (different color)
```

### NpcEntity

```csharp
public class NpcEntity
{
    public string Name { get; set; }
    public TilePosition Position { get; set; }
    public NpcBehavior Behavior { get; set; }
    public IReadOnlyList<TilePosition>? PatrolRoute { get; set; }
    public FacingDirection Facing { get; set; }
}
```

### NpcBehavior

| Value | Behavior |
|-------|----------|
| `Stationary` | Stands in place, never moves |
| `Patrolling` | Walks a fixed route (loop) |
| `Wandering` | Picks random adjacent passable tile every N ticks |

### NpcController

Runs on a timer (every ~2 seconds). For each NPC:
- **Wandering**: pick a random adjacent passable tile → pathfind → move
- **Patrolling**: advance along patrol route → pathfind → move
- **Stationary**: no-op

### TestMapData changes

Add 2–3 NPCs to the sample map:
- One stationary merchant
- One patrolling guard
- One wandering villager

### Rendering

NPCs render as colored shapes (orange `#e67e22`). Different shape (square vs player's circle) for visual distinction. Name label above each NPC.

---

## Acceptance Criteria

- [ ] NPCs visible on the map with distinct visual from player
- [ ] Stationary NPCs stay in place
- [ ] Patrolling NPCs walk their route loop
- [ ] Wandering NPCs move to random adjacent tiles
- [ ] NPC movement is smoothly animated (same as player)
- [ ] Player can walk through NPC tiles (no collision — future task)
- [ ] No changes to combat code
