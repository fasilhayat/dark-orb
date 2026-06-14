# Feature Exploration - Isometric World View and Tactical Movement Layer (Avalonia)

Project: Dark Orb

File: `feature-isometric-world-view.md`

Status: Analysis / Architecture / Future Implementation

Priority: High

---

# Objective

Introduce a future isometric world view into the existing Avalonia GUI that enables:

- World exploration
- Character movement
- Towns and settlements
- Dungeons
- Quest locations
- NPC interactions
- Tactical positioning
- Future combat movement integration

This feature establishes the architectural foundation for evolving Dark Orb from a combat simulator into a fully explorable RPG experience.

---

# Important Scope Limitation

This task is:

```text
Analysis
Architecture
Planning
Prototype Design
```

Only.

No implementation is authorized.

The purpose is to establish the long-term architecture before development begins.

---

# Existing GUI Requirement

The solution already contains an established Avalonia GUI.

The isometric view must be integrated into the existing GUI architecture.

The feature must not:

- Replace existing combat screens
- Replace character management screens
- Replace inventory screens

Instead it becomes a new major screen within the application.

---

# Vision

Dark Orb should eventually support:

```text
Main Menu
    ↓
World Map
    ↓
Isometric Exploration
    ↓
Dungeon Exploration
    ↓
Combat Encounters
    ↓
Loot / Progression
```

The isometric system becomes the foundation for exploration gameplay.

---

# Recommended Technology Stack

Use existing Avalonia UI framework.

Primary rendering options:

### Phase 1

Avalonia controls

Suitable for:

- Prototyping
- Small maps
- Debugging

---

### Phase 2

WriteableBitmap rendering

Suitable for:

- Larger maps
- Many entities
- Better performance

---

### Phase 3

SkiaSharp acceleration

Suitable for:

- High-fidelity rendering
- Particle systems
- Advanced effects

---

# Architectural Overview

Recommended structure:

```text
DarkOrb.Gui
│
├── Views
│   ├── WorldView.axaml
│   ├── DungeonView.axaml
│   └── TacticalMapView.axaml
│
├── ViewModels
│   ├── WorldViewModel
│   ├── DungeonViewModel
│   └── CharacterViewModel
│
├── Rendering
│   ├── IsometricRenderer
│   ├── TileRenderer
│   ├── SpriteRenderer
│   └── CameraController
│
└── Models
    ├── Tile
    ├── MapChunk
    ├── WorldLocation
    └── MapEntity
```

---

# Rendering Model

The recommended model is:

```text
Tile Layer
    ↓
Object Layer
    ↓
Character Layer
    ↓
Effects Layer
    ↓
UI Overlay Layer
```

This allows future support for:

- Fog of war
- Weather
- Spell effects
- Floating damage text
- Area effects

---

# Isometric Coordinate System

The engine must support logical coordinates:

```text
TileX
TileY
```

and convert them into screen coordinates.

---

## Recommended Formula

```csharp
screenX = (tileX - tileY) * (tileWidth / 2);
screenY = (tileX + tileY) * (tileHeight / 2);
```

This formula should become part of a dedicated rendering service.

Example:

```text
IsometricCoordinateTranslator
```

The conversion logic must never be duplicated.

---

# Map System

Maps should be tile based.

Potential structure:

```text
Grass
Road
Forest
Water
Mountain
Dungeon Floor
Dungeon Wall
Bridge
```

Each tile should contain:

```text
Tile Type
Movement Cost
Passable
Texture
Lighting Modifier
```

---

# Character Movement System

Movement must use MVVM.

The UI should never directly control character state.

---

## Character View Model

Example:

```text
Tile Position
Screen Position
Movement State
Facing Direction
Current Animation
```

---

## Movement Modes

### Grid Movement

Character moves tile-by-tile.

---

### Animated Movement

Character smoothly interpolates between tiles.

Preferred approach.

---

# Camera System

Future camera support:

### Follow Mode

Camera follows player.

---

### Free Camera

Mouse drag navigation.

---

### Zoom

Zoom in/out.

---

### Center On Character

Automatic recentering.

---

# Input System

Investigate support for:

### Keyboard

```text
Arrow Keys
WASD
```

---

### Mouse

```text
Click to Move
```

---

### Controller

Future support.

---

# Movement Integration With Combat

Movement system should be designed with future combat integration in mind.

Combat movement system must be able to reuse:

```text
Position
Distance
Movement Cost
Pathfinding
```

without redesign.

---

# Tactical Combat Compatibility

Future tactical combat may use:

### Option A

Distance-band combat

Current recommendation.

---

### Option B

Full tile combat

Future evaluation.

---

# Performance Requirements

The architecture must support:

```text
500+
Visible Tiles
```

without UI degradation.

---

## Initial Implementation

Acceptable:

```text
ItemsControl
Canvas
DataTemplates
```

---

## Long-Term Implementation

Preferred:

```text
WriteableBitmap
```

for rendering.

---

## Advanced Rendering

Investigate:

```text
SkiaSharp
```

for:

- particles
- lighting
- shadows
- weather effects

---

# Animation System

Future animations:

### Walk

---

### Run

---

### Attack

---

### Cast Spell

---

### Hit Reaction

---

### Death

---

### Knockback

---

### Fear Movement

---

### Flee Movement

---

# World Features

Future exploration content:

### Settlements

---

### Villages

---

### Cities

---

### Temples

---

### Wilderness

---

### Dungeons

---

### Hidden Locations

---

### Quest Areas

---

# NPC System

Future support:

```text
Shopkeepers
Quest Givers
Guards
Wandering NPCs
Followers
```

---

# Pathfinding Investigation

Future evaluation required.

Potential approaches:

### A*

Recommended.

---

### Dijkstra

Alternative.

---

### Navigation Graphs

For large maps.

---

# Save Game Impact

Future save files may need:

```text
Current Map
Current Position
Visited Locations
Active Quests
```

---

# Replay System Impact

Movement should eventually become replayable.

Potential replay events:

```text
Move
Rotate
Interact
Enter Location
Leave Location
```

---

# Asset Pipeline

Investigate support for:

### Isometric Tiles

### Character Sprites

### Animated Sprites

### Environmental Objects

### Lighting Assets

### Weather Effects

---

# User Interface Requirements

The exploration view should coexist with existing systems.

Potential layout:

```text
┌──────────────────────────────┐
│ Character Portraits          │
├──────────────────────────────┤
│                              │
│     Isometric World View     │
│                              │
├──────────────────────────────┤
│ Action Bar / Status Bar      │
└──────────────────────────────┘
```

---

# Deliverables

Produce:

1. Technical architecture proposal
2. Rendering architecture proposal
3. MVVM design proposal
4. Coordinate system design
5. Camera system proposal
6. Input system proposal
7. Performance analysis
8. Tactical combat integration analysis
9. Asset pipeline proposal
10. Phased implementation roadmap

---

# Recommended Implementation Roadmap

## Phase 1

Static isometric map rendering.

---

## Phase 2

Player movement.

---

## Phase 3

NPC movement.

---

## Phase 4

World interaction.

---

## Phase 5

Dungeon exploration.

---

## Phase 6

Combat integration.

---

## Phase 7

Advanced rendering and effects.

---

# Acceptance Criteria

- [ ] Avalonia integration strategy documented
- [ ] Isometric coordinate system defined
- [ ] MVVM architecture proposed
- [ ] Rendering architecture proposed
- [ ] Camera system proposed
- [ ] Input system proposed
- [ ] Performance strategy documented
- [ ] Pathfinding options analyzed
- [ ] Tactical combat compatibility analyzed
- [ ] Asset pipeline proposed
- [ ] Future RPG exploration architecture defined
- [ ] Awaiting approval before implementation