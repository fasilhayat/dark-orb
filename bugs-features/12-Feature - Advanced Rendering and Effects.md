# Feature - Advanced Rendering and Effects

Project: Dark Orb

File: `feature-advanced-rendering.md`

Dependencies: 11 (Asset Pipeline), 09 (World Interaction)

---

## Objective

Upgrade rendering from Avalonia controls to `WriteableBitmap` (and optionally SkiaSharp) for higher performance and visual effects: fog of war, lighting, particles, weather.

---

## Scope

New files:

```
BattleAura.Gui/
├── Rendering/
│   ├── WriteableTileRenderer.cs      # High-perf renderer using WriteableBitmap
│   ├── FogOfWar.cs                   # Visibility mask, reveal areas
│   ├── LightingLayer.cs              # Dynamic lighting overlay
│   ├── ParticleSystem.cs             # Simple particle emitter
│   └── WeatherController.cs          # Rain, fog overlay effects
│
└── Models/World/
    └── VisibilityMap.cs              # Revealed / visible tile tracking
```

Modified files:

```
BattleArena.Gui/
├── ViewModels/World/
│   └── WorldViewModel.cs             # Modify: add visibility, weather state
│
└── Rendering/
    └── TileRenderer.cs               # Replace Canvas approach with WriteableBitmap
```

### WriteableTileRenderer

Replaces the `ItemsControl`-based tile rendering with a `WriteableBitmap` that is drawn to once and invalidated only when the camera moves or tiles change. Benchmark targets: 500+ visible tiles at 60fps.

The old `TileRenderer` stays as a fallback — the `WriteableTileRenderer` is swapped in via configuration or capability check.

### Fog of War

- `VisibilityMap` tracks which tiles the player has seen
- Unexplored tiles are rendered as black/dark
- Explored but not currently visible tiles are dimmed
- Currently visible tiles are full brightness
- Visibility range: tiles within N steps of the player (configurable)

### Lighting

- Dynamic light sources: torches, campfires, magic glow
- `LightingLayer` composites a multi-color alpha blend over the tile render
- Light sources defined per tile in the map or attached to entities

### Particles

- Simple `ParticleSystem`: emitters with position, velocity, lifetime, color
- Used for: campfire sparks, magic effects, rain splashes
- Straightforward list of structs updated each frame

### Weather

- `WeatherController` applies global overlay effects
- Modes: Clear, Rain, Fog
- Rain: diagonal line particles
- Fog: semi-transparent white overlay with slow drift

### Performance requirement

All effects combined must not drop below 30fps on a standard development machine with 500 visible tiles. Effects degrade gracefully (skip particle update if frame budget exceeded).

---

## Acceptance Criteria

- [ ] `WriteableTileRenderer` matches the visual output of `TileRenderer`
- [ ] Fog of war hides unexplored areas
- [ ] Explored tiles persist as dimmed after player moves away
- [ ] At least one dynamic light source renders (torch or campfire)
- [ ] Rain particle effect renders during weather
- [ ] Frame rate stays above 30fps with 500+ tiles
- [ ] Falling back to `TileRenderer` does not break any features
- [ ] No changes to combat code
