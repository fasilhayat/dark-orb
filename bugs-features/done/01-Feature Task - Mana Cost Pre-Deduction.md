# Feature Task - Mana Cost Pre-Deduction Glow Effect

Project: Dark Orb

---

## Objective

Enhance the mana (MP) UI system so that spell casting shows a **pre-deduction visual preview** of mana cost before it is applied.

When a spell is cast, the MP bar must visually indicate the exact amount of mana that will be consumed using a glowing overlay effect, before the mana value is deducted.

This brings consistency with:
- HP damage preview system (white subtraction glow)
- Critical hit damage visualization system

---

## Scope

This feature applies to:

- All spell casts that consume mana
- All mana-consuming abilities (current and future)
- NPC and player-controlled characters

---

## Core Behavior

### Step 1 - Spell Cast Initiation

When a spell cast is triggered:

- Spell is validated (cost, cooldown, etc.)
- Mana cost is determined

---

### Step 2 - Pre-Deduction Mana Preview (NEW)

Before mana is deducted:

- MP bar displays a **glowing “reserved mana segment”**
- The glow represents the exact mana cost
- The segment originates from the current MP value and extends toward the deducted amount

#### Visual Requirements:

- Color: Mana-themed glow (arcane blue / violet depending on system theme)
- Effect: Pulsing / soft shimmer
- Position: Inside MP bar (not external UI overlay)
- Behavior: Matches exact mana cost value

---

### Step 3 - Mana Deduction

After preview animation:

- Mana is deducted from current MP value
- MP bar updates to final value
- Glow effect is removed

---

## Timing Rules

- Preview MUST occur before any state mutation
- MP deduction MUST occur only after preview completes (or is frame-synced in deterministic systems)
- No immediate “snap deduction” allowed

---

## Consistency Requirement

This system must match existing visual systems:

| System | Preview Type | Timing |
|--------|--------------|--------|
| HP Damage | White subtraction overlay | Before HP update |
| Mana Cost | Glowing reserved segment | Before MP update |

---

## Multi-Cost Support

If a spell has multiple mana components:

- All costs must be summed first
- A single unified preview is shown
- No per-component flashing allowed

---

## Edge Cases

### 1. Insufficient Mana

If caster lacks mana:

- Preview still shows full cost
- MP bar visually empties with glow preview
- Cast is canceled or fails after preview (engine-defined behavior)

---

### 2. Zero Cost Spells

If mana cost is 0:

- No preview effect is shown
- No glow is triggered

---

### 3. Rapid Sequential Casting

If multiple spells are cast quickly:

- Each spell must queue its own preview
- No overlapping or merging of glow states unless explicitly stacked

---

## Performance Constraints

- Must not trigger additional redraw loops per frame
- Must be driven by combat event stream (not polling)
- Must remain deterministic in replay mode

---

## Configuration Parameters

Add tunable UI parameters:

- `manaPreviewGlowIntensity`
- `manaPreviewAnimationDurationMs`
- `manaPreviewColorScheme`
- `manaPreviewSmoothingFactor`

---

## Integration Rules

- Mana cost preview is driven strictly by combat log / engine event
- UI must not compute mana costs independently
- UI must not simulate or predict spell cost logic

---

## Acceptance Criteria

- [x] Mana cost is visually previewed before deduction
- [x] Glow effect accurately reflects exact mana cost
- [x] MP bar updates only after preview completes
- [x] No desync between engine state and UI display
- [x] Works for all spell types and abilities
- [x] Handles insufficient mana correctly
- [x] Fully deterministic in replay system
- [x] No direct UI-side mana calculations

---

## Implementation Summary

### Core combat simulation
- **`CombatSimulator.DeductManaCostAsync`** — emits a `ManaPreview` event (with cost and current mana) BEFORE mutating `CurrentMana`, then emits the existing `ManaDeduct` after deduction
- Zero-cost spells (`ManaCost <= 0`) skip preview entirely

### Presentation layer
- **`CombatDisplayState.ApplyEvent`** — `ManaPreview` passes through without state mutation (mana unchanged until `ManaDeduct`)
- **`CombatPlaybackEngine.EmitVisualEvents`** — emits a `VisualEvent` with `ManaCost` and `ManaBefore` fields, violet arcane overlay text `"MANA -{cost}"`
- **`VisualEvent`** — added `ManaCost` and `ManaBefore` fields

### GUI (Avalonia)
- **`CharacterCard.axaml`** — mana bar now has a violet arcane glow overlay (`#aa66ff`) bound to `ManaCostPreviewOpacity/Start/Fraction/Remainder`
- **`CharCardViewModel`** — added mana cost preview overlay properties
- **`AvaloniaCombatPresenter`** — `AnimateManaCostPreview` (opacity fade 0.8→0, 600ms); `ManaPreview` handled in `OnNormalVisualEvent` with `ManaCost`; `BuildManaPreviewRow` for GUI log; 500ms delay configured in `_delays`

### Combat log output
- **`CombatLogWriter`** — `ManaPreview` rendered as `[tick] MANAPREVIEW {message}`
- **GUI log** — `BuildManaPreviewRow` shows ◆-dimmed "prepares [spell] (reserving {cost} mana)"

### Tests
- All **702 tests pass** (582 unit + 120 acceptance)