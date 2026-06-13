# Feature - D&D Style Location UI

Project: Dark Orb

File: `feature-dnd-location-ui.md`

Reference: `design/assets/misc/c998f3bc-0265-4099-8905-aebdf43fe3e4.png`

---

## Objective

Replace the bare Location phase with a D&D-themed UI layout containing character portrait panels, the isometric view, and a bottom action bar. Dark theme with ornate style.

---

## Layout

```
┌──────────────────────────────────────────────────────┐
│  ╔══ LOCATION NAME ═══════════════════════════ ╗    │
│  ║  [—] [—] [—]                             ═══╝    │
│  ╚══════════════════════════════════════════════╝    │
│                                                       │
│  ┌──────┐  ┌──────────────────────────────┐ ┌──────┐ │
│  │PORTRAIT│  │                              │ │ INFO │ │
│  │  NAME  │  │     ISOMETRIC VIEW           │ │ PANEL│ │
│  │ HP ███ │  │                              │ │      │ │
│  │ MP ███ │  │     (WorldView control)      │ │ NPCs │ │
│  │        │  │                              │ │ Zone │ │
│  │PORTRAIT│  │                              │ │      │ │
│  │  NAME  │  │                              │ │      │ │
│  │ HP ███ │  │                              │ │      │ │
│  │ MP ███ │  │                              │ │      │ │
│  └──────┘  └──────────────────────────────┘ └──────┘ │
│                                                       │
│  ┌──────────────────────────────────────────────────┐ │
│  │ ⚔ Attack  🛡 Defend  ✦ Ability  ◆ Item  📖 Log │ │
│  └──────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────┘
```

### 1. Top Bar

- Zone/location name on the left
- Mini control buttons on the right
- Ornate border styling

### 2. Left Panel — Party Member Cards

Each card contains:
- **Portrait** (64×64, loaded from Assets/Portraits/{name}.png, fallback silhouette)
- **Character name**
- **HP bar** (red) with numeric (current/max)
- **Mana bar** (blue) with numeric (current/max)
- **Status effects** (colored dots or small icons)
- **Level/Class** text

The panel scrolls if > 4 members. Uses existing `PortraitResolver.GetPortrait()`.

### 3. Center — Isometric View

The existing `WorldView` control, unchanged. Fills available space.

### 4. Right Panel — Location Info

- **Zone name** + terrain type
- **Weather** indicator (future)
- **Nearby NPCs / enemies** list from WorldViewModel

### 5. Bottom Bar

- Stub action buttons: Attack, Defend, Ability, Item, Log
- D&D style border treatment
- Each button has an icon character (⚔ 🛡 ✦ ◆ 📖)

---

## Implementation

All changes in `MainWindow.axaml` (Location phase grid) and `MainWindowViewModel.cs`. No changes to `WorldView` or isometric code.

The existing `PortraitResolver` in `BattleArena.Gui/PortraitResolver.cs` already handles portrait loading. Party member data comes from the roster (party composition).

---

## Acceptance Criteria

- [ ] Location phase has 5-region layout matching the design
- [ ] Party panel shows portraits with HP/Mana bars
- [ ] Portraits load from Assets/Portraits/ with fallback
- [ ] Isometric view fills the center area
- [ ] Right info panel shows zone name and terrain
- [ ] Bottom bar has styled action buttons
- [ ] Dark D&D theme styling throughout
- [ ] World map and combat phases unaffected
