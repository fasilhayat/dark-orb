# Feature - Location UI Layout

Project: Dark Orb

File: `feature-location-ui-layout.md`

Reference: `design/assets/misc/c998f3bc-0265-4099-8905-aebdf43fe3e4.png`

Dependencies: 13 (World Map Navigation), all isometric tasks

---

## Objective

Replace the minimal Location phase (full-screen isometric view) with a structured UI layout containing character portrait panels, action buttons, status bars, and the isometric view as the main area.

---

## Current state

The Location phase is a bare `Grid` with a title bar and the `WorldView` control filling the entire space. No character info, no party display, no action bar.

---

## Regions (from reference design)

The screen should be divided into these regions:

```
┌─────────────────────────────────────────────────┐
│  TOP BAR (zone name, location name, controls)    │
├──────────┬──────────────────────────┬───────────┤
│  PARTY   │                          │  LOCATION │
│  PANEL   │    ISOMETRIC VIEW        │  INFO     │
│ (left)   │    (main area)           │  (right)  │
│          │                          │           │
│ Portrait │                          │ Terrain   │
│  HP bar  │                          │ Enemies   │
│  Mana    │                          │  nearby   │
│  Status  │                          │           │
├──────────┴──────────────────────────┴───────────┤
│  BOTTOM BAR (actions, quick slots, chat log)    │
└─────────────────────────────────────────────────┘
```

### 1. Top Bar

- Zone/location name (e.g., "Village of Ashwood")
- Current time or weather indicator
- Minimap toggle button
- Settings / menu button

### 2. Party Panel (left side, ~200px wide)

One row per party member containing:
- **Portrait image** — loaded from `Assets/Portraits/{name}.png`, falls back to colored silhouette
- **Name** — character name
- **HP bar** — current / max HP
- **Mana bar** — current / max Mana
- **Status effects** — small icons for active buffs/debuffs
- **Level / Class** — small text

The panel scrolls if more than 4 party members.

### 3. Isometric View (center, fills remaining space)

The existing `WorldView` control, unchanged.

### 4. Location Info Panel (right side, ~180px wide)

- **Terrain type** (from current zone)
- **Nearby enemies** / NPCs in the area
- **Points of interest** on the current map
- **Weather** indicator (future)

### 5. Bottom Bar

- **Action buttons**: Inventory, Party Management, Abilities
- **Quick slots**: 4-6 configurable ability/item slots
- **Interaction log**: brief event feed (picked up item, entered area, etc.)

---

## Portrait system

Portraits are already loaded by `PortraitResolver.GetPortrait(name)` which looks up PNGs in `Assets/Portraits/`. Existing portraits for 19 characters.

Files:
- `PortraitResolver.cs` — already exists, provides `GetPortrait(characterName) -> Bitmap?`
- `Assets/Portraits/*.png` — already exists for all named heroes and enemies

---

## Implementation plan

### Phase 1: Layout shell
- Restructure Location phase Grid into the 5-region layout
- Add empty placeholder panels with correct sizes
- Verify WorldView still fills the center correctly

### Phase 2: Party panel
- Read party composition from WorldViewModel or game state
- Render portrait + HP bar + Mana bar for each member
- Style to match reference design

### Phase 3: Location info panel
- Show current zone/terrain
- List nearby NPCs from WorldViewModel

### Phase 4: Bottom bar
- Action buttons (stubs)
- Interaction log feed

---

## Files to modify

```
BattleArena.Gui/
├── Views/
│   └── MainWindow.axaml        # Restructure Location phase layout
├── ViewModels/
│   ├── MainWindowViewModel.cs  # Add panel visibility/state props
│   └── World/
│       └── WorldViewModel.cs   # Expose party members, nearby NPCs
└── Views/World/
    └── WorldView.axaml         # Keep unchanged (fills center)
```

## New files

None — all changes are to existing files.

---

## Acceptance Criteria

- [ ] Location phase shows 5-region layout matching reference
- [ ] Party panel displays portraits for each party member
- [ ] HP and Mana bars update in real time
- [ ] Location info shows zone name and terrain
- [ ] Bottom bar has stub action buttons
- [ ] Isometric view fills the center area without overlap
- [ ] World map and combat phases are unaffected
