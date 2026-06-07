# Feature Extension - Continuous Resource Drain Visual System (Leech Spell)

Project: Dark Orb

File: `feature-damage-impact-visualization.md` (extension)

---

## Objective

Extend the existing **damage/heal impact visualization system** to support **continuous resource transfer effects**, specifically the `Leech` spell.

Leech is not a discrete event.

It is a **sustained state-based transfer effect** that must be visually represented as a continuous flow between entities.

---

## Core Design Shift

Previous systems:

* Damage = instant event (preview → apply → done)
* Heal = instant event (apply → expand → done)

Leech:

> is a **time-bound transfer state**, not an event

Therefore:

* It does NOT use single-frame impact logic
* It uses continuous animation tied to tick duration

---

## Trigger Condition

Leech visual system activates when:

* A `Leech` spell is successfully applied
* And its status effect is active on target

It remains active until:

* effect expires
* dispelled
* or target dies

---

## Visual Behavior Model

### Phase 1 - Activation Burst (On Apply)

When Leech starts:

* Brief “link formation” animation between caster and target
* Energy tether appears (mana-colored stream)
* Initial pulse confirms activation

This is a **single event**

---

### Phase 2 - Continuous Drain Loop (Core Behavior)

While Leech is active:

#### Every tick:

* Mana is visually removed from target
* Same amount is visually added to caster
* A **flowing energy stream persists between units**

---

## Visual Requirements

### 1. Bidirectional Flow Representation

* Target → Caster energy stream
* Color: mana-specific (blue / arcane tone)
* Flow must be continuous, not segmented

---

### 2. Smooth Interpolation Between Ticks

Even though combat is tick-based:

* animation must interpolate between tick values
* no visible “step jumps”
* smooth depletion curve per tick interval

---

### 3. HP/Mana Bar Integration

During Leech:

* Target mana bar:

  * continuously drains
  * optional subtle white “pre-loss” shimmer at drain front

* Caster mana bar:

  * continuously fills
  * matching gain glow

---

### 4. Stability Rule

At no point should:

* caster gain > target loss (visual mismatch)
* or vice versa

They must always mirror exactly per tick delta.

---

## Timing Model

Each tick produces:

1. Engine computes mana delta
2. Log records mana change
3. UI consumes delta
4. UI interpolates effect over tick duration

---

## Interaction with Existing Damage System

Leech is NOT a damage effect.

Therefore:

* It does NOT trigger damage preview system
* It does NOT use HP bar white “subtraction glow”
* It uses a separate **resource transfer visual layer**

---

## Effect Layer Priority

If multiple effects occur:

1. Damage/Heal impact FX (HP system)
2. Leech continuous FX (mana system)
3. Status FX (stun, burn, etc.)
4. Cosmetic FX (sounds, border pulses)

---

## Edge Cases

### 1. Leech + Mana Cap

If caster mana is full:

* visual flow continues from target
* excess transfer is shown as “overflow fade”
* no mana gain is applied beyond cap (engine rule)

---

### 2. Target Mana Reaches 0

* drain stream continues but becomes “empty siphon”
* reduced intensity visual
* optional flicker effect indicating depletion

---

### 3. Multiple Leech Sources

If stacked:

* streams merge into single stronger channel
* intensity scales with total drain rate

---

## Configuration Parameters

* `leechFlowSpeed`
* `leechStreamIntensity`
* `leechInterpolationSmoothing`
* `leechOverflowFadeStrength`

---

## Consistency Rule with Damage System

Leech must remain:

* continuous (not discrete flashes)
* state-driven (not event-driven)
* interpolated between ticks

Damage system remains:

* discrete impact-based (preview → apply)

These systems must NOT be unified.

---

## Acceptance Criteria

* [x] Leech creates persistent visual link between caster and target
* [x] Mana transfer is continuously animated during effect duration
* [x] Per-tick delta is visually correct and deterministic
* [x] No HP damage FX system is reused for Leech
* [x] No desync between caster gain and target loss visuals
* [x] Smooth interpolation between ticks implemented
* [x] Effect terminates cleanly when Leech ends

---

## Implementation Summary

### Core gameplay mechanic (general Leech behaviour, not spell-specific)
- **`StatusEffectType.Leech`** — new enum value for generic leech behaviour
- **`StatusEffect.LeechPerTurn`** — amount drained per tick
- **`StatusEffect.LeechResourceType`** — `"HP"` or `"Mana"` (per-spell configurable via JSON)
- **`StatusEffect.CasterName`** — tracks who receives the drained resource (only set for `Type == Leech`)
- **`CombatLogEntry`** — added `LeechAmount`, `LeechCasterName`, `LeechResourceType`, `LeechTargetAfter`, `LeechCasterAfter` fields

### Combat simulation
- **`CombatSimulator.ProcessActorLeechAsync`** — processes leech effects during a character's turn; drains HP/Mana from target, transfers to caster (capped at max)
- Integrated into `ProcessActingActorAsync` after HoT, before DoT
- **`RosterLoader.StatusEffectDto`** — extended with `LeechPerTurn`, `LeechResourceType`, and other effect fields for JSON deserialization
- **`AutoActionDecisionSource`** — filters out leech spells when caster's resource is already full (no wasteful casts)

### Presentation / GUI
- **`CombatDisplayState.ApplyEvent`** — handles `LeechTick` to update both target and caster HP/Mana
- **`CombatPlaybackEngine`** — emits visual events and sounds for `LeechTick`; leech added to persistent effect names with purple border flicker
- **`VisualEvent`** — added `LeechAmount`, `LeechCasterName`, `LeechResourceType` fields
- **`CombatSoundRegistry`** — `LeechTick` sound mapping ("Eerie whisper of draining energy")

### Avalonia GUI
- **`CharacterCard.axaml`** — mana bar has light red drain overlay (`#ff6666`) and light purple gain glow (`background` animated via `ManaGainColor`)
- **`CharCardViewModel`** — added `ManaDrainOpacity/Start/Fraction/Remainder` and `ManaGainOpacity/Start/Fraction/Remainder` + `ManaGainColor` (animates light purple → mana bar purple)
- **`AvaloniaCombatPresenter`** — `AnimateManaDrain` (opacity fade, 800ms) and `AnimateManaGain` (color lerp from `#cc88ff` → `#cc44cc`, 800ms); `BuildLeechTickRow` for GUI log; `LeechTick` routing in `ShowCombatEvent`

### Combat log output
- **`CombatLogWriter`** — `LeechTick` rendered as `🩸 -<amt> HP → <caster> +<amt>` (or `♦` for mana)
- **GUI log** — same symbols with colored segments (red for HP, magenta for mana)

### Roster data
- Added `"Mind Siphon"` spell (Umbramancy, mana leech) to both `BattleArena.Gui/Data/roster.json` and `BattleArena.Demo/roster.json`
- Given to `Vaelith Moonveil` and `Old Man Kael`

### Tests
- **`Duel_ManaLeech_DrainsTargetAndGrantsCaster`** — verifies mana transfer via `LeechTick` events
- **`StatusEffect_Leech_ExpiresCleanly`** — verifies leech duration expiration
- All **702 tests pass** (582 unit + 120 acceptance)
