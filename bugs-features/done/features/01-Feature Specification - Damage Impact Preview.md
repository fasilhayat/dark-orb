# Feature Specification - Damage Impact Preview & Critical Hit Visual FX

Project: Dark Orb

File: `feature-damage-impact-visualization.md`

---

## Objective

Introduce a **pre-damage visual indicator system** for high-impact attacks.

When a **critical hit or high-damage event** occurs, the UI must visually represent the *incoming HP loss before it is applied* using a white “pre-subtraction glow” effect.

This mirrors and complements the existing heal visual system.

---

## Core Concept

Damage and healing must become **visually symmetrical systems**:

* Heal → visual expansion into HP bar
* Damage → visual contraction from HP bar

But critically:

> Damage must be previewed BEFORE HP is subtracted

---

# Trigger Conditions

The effect is triggered when ANY of the following occur:

## 1. Critical Hit

* Any attack flagged as CRIT in combat log:

  * `CRIT !!`
  * `[x2 CRIT]`
  * or equivalent engine flag

---

## 2. High Damage Event

A hit qualifies as “high damage” if:

* Damage ≥ 25% of target max HP
  OR
* Damage exceeds configured “devastation threshold”

(This threshold must be configurable, not hardcoded in UI)

---

# Visual Behavior

## Phase 1 - Damage Preview (Pre-Subtraction)

Before HP is reduced:

### UI Behavior:

* Target HP bar displays a **white overlay segment**
* Overlay represents the exact amount of HP to be lost
* Overlay originates from the **right edge of current HP**
* Overlay shrinks the HP bar visually but does NOT commit state change yet

### Animation:

* White glow / pulse effect
* Slight flicker or energy distortion
* Synchronized with impact sound (if present)

### Timing Rule:

* This must occur AFTER damage roll is finalized
* BUT BEFORE HP value is committed

---

## Phase 2 - Damage Application

Immediately after preview phase:

* HP is subtracted
* HP bar updates to final value
* White overlay is removed

---

## Phase 3 - Impact Confirmation (Optional Enhancement)

On completion:

* Brief pulse of the character frame border (damage color)
* Optional shake effect on character card
* Optional “impact flash” layer

---

# Symmetry Requirement (Critical)

This system must mirror heal behavior:

| Event  | Visual Direction                | Timing              |
| ------ | ------------------------------- | ------------------- |
| Heal   | Expand HP bar                   | After heal resolves |
| Damage | Contract HP bar (white preview) | Before HP changes   |

---

# Rules of Operation

## 1. No Logic Coupling

This feature MUST NOT:

* modify damage calculation
* modify crit rules
* modify HP system
* modify combat logic

It is strictly UI-layer rendering.

---

## 2. Source of Truth

Damage values come from:

* Combat log event: `Damage applied`
* Or equivalent engine event

UI does NOT compute damage.

---

## 3. Determinism Requirement

The preview must match exactly:

* final damage value
* mitigation already applied
* crit multipliers

No approximation allowed.

---

## 4. Multi-hit Support

If a spell or attack has multiple damage components:

* preview must aggregate total damage first
* then render single unified preview
* then apply final subtraction

---

# Edge Cases

## 1. Overkill Damage

If damage exceeds remaining HP:

* preview is capped at current HP
* full bar turns white
* HP goes to 0 or negative per engine rules

---

## 2. Simultaneous Effects

If heal and damage occur same tick:

* resolve ordering strictly by log sequence
* each effect must animate independently
* no visual merging allowed

---

## 3. Stacked Damage Events

If multiple hits occur in same tick:

* each hit must render sequentially OR
* combined preview (engine-defined mode)

Must remain deterministic.

---

# Configuration Parameters

Add configurable UI parameters:

* `devastationThresholdPercent`
* `damagePreviewDurationMs`
* `impactFlashIntensity`
* `enableCardShake`

---

# Acceptance Criteria

* [x] Critical hits trigger pre-damage white preview
* [x] High damage triggers preview effect
* [x] HP is NOT updated before preview completes
* [x] Final HP update occurs after preview
* [x] Heal and damage visuals are symmetrical
* [x] Multi-hit attacks handled correctly
* [x] No modification to combat logic layer
* [x] Deterministic rendering from log data
* [x] Overkill damage handled correctly
* [x] Works with existing combat log format
