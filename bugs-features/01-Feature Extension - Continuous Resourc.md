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

* [ ] Leech creates persistent visual link between caster and target
* [ ] Mana transfer is continuously animated during effect duration
* [ ] Per-tick delta is visually correct and deterministic
* [ ] No HP damage FX system is reused for Leech
* [ ] No desync between caster gain and target loss visuals
* [ ] Smooth interpolation between ticks implemented
* [ ] Effect terminates cleanly when Leech ends
