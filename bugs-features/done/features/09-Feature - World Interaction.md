# Feature - World Interaction

Project: Dark Orb

File: `feature-world-interaction.md`

Dependencies: 05 (Input System), 06 (Pathfinding), 07 (NPC System), 08 (Dungeon System)

---

## Objective

Add interactive objects to the world: doors, lootable objects, NPC dialogue triggers, and zone transition markers that the player can activate with a key press or click.

---

## Scope

New files:

```
BattleArena.Gui/
├── Models/World/
│   ├── WorldObject.cs            # Base: position, type, interaction behavior
│   └── WorldObjectType.cs        # Enum: Door, Chest, Sign, Transition, NpcTrigger
│
├── ViewModels/World/
│   └── InteractionPrompt.cs      # UI model: "Press E to open" banner
│
└── Views/World/
    └── InteractionOverlay.axaml  # Floating prompt + interaction result text
```

Modified files:

```
BattleArena.Gui/
├── Models/World/
│   └── TestMapData.cs            # Modify: add objects to the sample map
│
├── ViewModels/World/
│   └── WorldViewModel.cs         # Modify: hold objects, handle interaction
│
└── Rendering/
    └── WorldInputHandler.cs      # Modify: interact key (E / Enter)
```

### WorldObject

```csharp
public class WorldObject
{
    public TilePosition Position { get; set; }
    public WorldObjectType Type { get; set; }
    public string Label { get; set; }         // "Chest", "Wooden Door", etc.
    public bool IsInteractable { get; set; }
    public Func<PlayerViewModel, string>? OnInteract { get; set; }  // returns feedback text
}
```

### Interact key

Pressing **E** (or **Enter**) when adjacent to an interactable object triggers it:

| Object Type | Interaction |
|-------------|-------------|
| `Door` | Toggle open/closed (changes passable flag) |
| `Chest` | Show "You found X!" message (placeholder) |
| `Sign` | Display sign text in overlay |
| `Transition` | Already handled by MapManager (task 08) |
| `NpcTrigger` | Show "Talk" prompt (dialogue system deferred) |

### Proximity detection

When the player is adjacent to an interactable object, an `InteractionPrompt` appears: "Press E to [action]". Implemented as an overlay UserControl bound to `WorldViewModel.ActiveInteraction`.

### Door mechanics

- Closed door: impassable, renders as brown rectangle
- Open door: passable, renders differently (gray, thinner)
- Player presses E next to closed door → it opens
- Player presses E next to open door → it closes

### Chest

- Placeholder interaction: shows a floating message "You found some gold!" for 2 seconds
- Future: loot table, inventory integration

---

## Acceptance Criteria

- [ ] Player can press E to interact with adjacent objects
- [ ] Doors toggle open/closed (affects pathfinding passability)
- [ ] Interaction prompt appears when near interactable objects
- [ ] Signs display readable text
- [ ] Chest shows feedback message
- [ ] All objects render with distinct visuals on the map
- [ ] No changes to combat code
