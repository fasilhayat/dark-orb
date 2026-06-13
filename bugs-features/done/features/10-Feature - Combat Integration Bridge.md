# Feature - Combat Integration Bridge

Project: Dark Orb

File: `feature-combat-integration-bridge.md`

Dependencies: 09 (World Interaction), all preceding isometric tasks. Must NOT be started until tasks 01–09 are stable.

---

## Objective

Connect the isometric world view to the existing `CombatSimulator` engine. When the player encounters an enemy (via map trigger or NPC interaction), the combat screen activates with the characters and enemies from the world state, and post-combat results update the world state.

---

## Scope

New files:

```
BattleArena.Gui/
└── Services/
    └── CombatBridge.cs            # World→Combat transition coordinator
```

Modified files:

```
BattleArena.Gui/
├── ViewModels/
│   └── MainWindowViewModel.cs    # Modify: add world→combat phase transition
│
├── Views/
│   └── MainWindow.axaml          # Modify: wire world→combat→world flow
│
└── ViewModels/World/
    └── WorldViewModel.cs         # Modify: expose encounter trigger
```

### CombatBridge

```csharp
public class CombatBridge
{
    public CombatResult RunEncounter(Character playerCharacter, List<NpcEntity> enemies, ITerrainType terrain);
    public void ApplyCombatResult(CombatResult result, PlayerViewModel player, List<NpcEntity> enemies);
}
```

- Constructs `Party` objects from the world-state player character and enemy NPCs
- Calls `CombatSimulator.Simulate()` (existing engine)
- Returns the `CombatResult` for playback

### Encounter triggers

| Trigger Type | Where defined |
|-------------|---------------|
| Enemy NPC tile contact | NpcEntities with `IsHostile=true` — step adjacent → combat |
| Dungeon encounter zones | Map tile regions flagged as encounter zones |
| Quest-triggered fights | Future extension |

### Flow

1. Player steps on encounter tile / contacts hostile NPC
2. `CombatBridge` builds parties from world state
3. `MainWindowViewModel.Phase` switches from `"World"` to `"Combat"`
4. Combat runs and plays back normally (existing system)
5. Post-combat: results applied (HP changes, enemy removal)
6. Phase switches back to `"World"` with updated state

### What this task does NOT do

- Does not add tactical positioning on the isometric map during combat (future)
- Does not change `CombatSimulator`, `AttackResolver`, or any combat engine code
- Does not modify the combat phase UI
- Does not add positional range calculations during combat (distance-band model deferred)

### Constraints

- The existing combat system must work exactly as before when entered from the main menu (non-world path)
- The bridge is the only code that touches both world and combat systems
- All party/character construction uses existing `Character` entities from Core

---

## Acceptance Criteria

- [ ] Stepping on an encounter tile starts combat with the correct parties
- [ ] Combat uses the existing CombatSimulator (unmodified)
- [ ] Combat playback uses the existing UI (unmodified)
- [ ] After combat, player returns to the world map at the encounter position
- [ ] Defeated enemies are removed from the world map
- [ ] Player HP changes from combat persist in the world state
- [ ] Combat can still be started from the main menu independently
- [ ] No changes to combat engine, presenters, or playback code outside the bridge
