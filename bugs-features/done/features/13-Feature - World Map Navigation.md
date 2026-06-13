# Feature - World Map Navigation

Project: Dark Orb

File: `feature-world-map-navigation.md`

Dependencies: None (new UI phase — does not modify existing isometric code)

---

## Objective

Replace the current "WORLD" button behavior with a top-down 2D world map (Aelthoria). The party moves around this map by clicking location markers. Entering a town, dungeon, or encounter zone transitions to the existing isometric view.

---

## Architecture

The current "World" phase becomes **two sub-phases**:

```
World Map (2D image) ──click marker──→ Location (isometric)
     ↑                                       │
     └──────── "Back to World Map" ──────────┘
```

- **WorldMap** — shows `aelthoria-world-map.png`, party icon, location markers
- **Location** — existing isometric system (tasks 01–12), triggered per marker

---

## New files

```
BattleArena.Gui/
├── Assets/WorldMap/
│   └── aelthoria-world-map.png          # Copied from design/assets/misc/
│
├── Models/WorldMap/
│   ├── MapLocation.cs                   # Name, screen position, type, target map
│   └── LocationType.cs                  # Enum: Village, City, Dungeon, Encounter, Cave
│
├── ViewModels/WorldMap/
│   └── WorldMapViewModel.cs             # Holds locations, party position, phase state
│
└── Views/WorldMap/
    ├── WorldMapView.axaml               # Image + location markers + party icon
    └── WorldMapView.axaml.cs            # Click handling, transitions
```

## Modified files

```
BattleArena.Gui/
├── ViewModels/
│   └── MainWindowViewModel.cs           # Add IsWorldMapPhase, IsLocationPhase
│
├── Views/
│   ├── MainWindow.axaml                 # Add WorldMap phase + Location phase grids
│   └── MainWindow.axaml.cs              # Wire phase transitions
│
└── BattleArena.Gui.csproj               # Copy Assets/WorldMap/
```

---

## MapLocation

```csharp
public class MapLocation
{
    public string Name { get; set; }         // "Village of Ashwood"
    public LocationType Type { get; set; }
    public double ScreenX { get; set; }      // Position on the map image (pixels)
    public double ScreenY { get; set; }
    public string? TargetMapId { get; set; } // "ashwood" — maps to isometric zone
    public string? Description { get; set; } // Shown on hover
}
```

## LocationType

```csharp
public enum LocationType { Village, City, Dungeon, Encounter, Cave }
```

---

## Locations (initial set)

Placed on the 1536×1024 world map image:

| Name | Type | Position (x,y) | Target isometric map |
|------|------|----------------|---------------------|
| Village of Ashwood | Village | (680, 590) | Existing 40×30 world map (task 02) |
| Mountain Cave | Cave | (990, 420) | Existing dungeon map (task 08) |
| Duel Encounter | Encounter | (780, 540) | Triggers combat (task 10) |

Additional locations can be added as future content.

---

## Flow

1. User clicks **WORLD** on main menu → WorldMap phase
2. User sees the Aelthoria map with labeled markers
3. User clicks a marker → party icon moves there → transitions to Location phase
4. Location phase shows the existing isometric view (WorldView control)
5. User walks to exit/transition tile or clicks "Back to World Map"
6. Returns to WorldMap phase

---

## World Map controls

- **Click a marker** → move party there (animated 500ms slide) → enter location
- **Hover a marker** → show name + description tooltip
- Clicking empty map space does nothing

---

## Integration with existing code

- `MainWindowViewModel.Phase` values: `"WorldMap"`, `"Location"`, `"Combat"`, etc.
- The existing `WorldView` control is reused in the Location phase
- `WorldViewModel` is reused per location (its `MapManager` selects the correct map)
- Combat bridge (task 10) already handles world→combat→world transitions
- The encounter tile on the world map (20,10) is replaced by the Duel Encounter marker

---

## Acceptance Criteria

- [ ] World map displays the Aelthoria image at correct aspect ratio
- [ ] Location markers render as colored circles with labels
- [ ] Clicking a marker highlights it, moves party icon, transitions to location
- [ ] Location phase shows isometric view for that location
- [ ] "Back to World Map" returns from location to world map
- [ ] Hovering a marker shows name + description
- [ ] Existing isometric code is unchanged by the new phase
- [ ] Combat from world encounter still works
- [ ] Demo mode from main menu still works independently
