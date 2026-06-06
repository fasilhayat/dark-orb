# Feature Improvement - Turn Meter Lock Pipe Visual Refinement

Project: Dark Orb

## Objective

Refine the visual behavior of the Turn Meter (TM) lock system to improve clarity, reduce visual noise, and ensure consistent interpretation of combat state.

---

# Current Problem

When a Turn Meter lock effect (e.g., Stun, Freeze, Sleep, Petrify) is active, the entire TM pipe structure is currently turned white.

This creates two issues:

1. **Loss of progress readability**

   * Fully unfilled pipes and filled pipes become visually indistinguishable.
   * Players lose understanding of how close a character is to their turn.

2. **Over-saturated visual feedback**

   * The entire meter is overridden, making it harder to interpret combat state at a glance.

---

# Required Improvement

The Turn Meter lock visual system must be refined as follows:

## Option A (Preferred - Recommended Design)

Only **filled Turn Meter pipes** should be affected by the lock visual effect.

### Behavior

When a TM lock effect is active:

* Only the **filled TM segments/pipes** are affected.
* Filled pipes should **blink using the same color as the character card border effect** (e.g., Stun = yellow).
* Unfilled pipes remain visually unchanged.
* The border effect of the TM meter continues to blink in the same effect color.
* The blinking of filled pipes must be synchronized with the border animation.

### Result

* Progress remains readable.
* Lock state is still highly visible.
* Visual identity remains consistent across UI elements.

---

## Option B (Fallback Design if A is not feasible)

If per-segment control is not technically feasible:

* The TM bar border color becomes the primary visual driver.
* Only the **active filled region overlay** (not the full pipe grid) pulses/blinks.
* The blink color must match the character card border effect color.
* No part of the empty/unfilled TM region should be visually altered.

---

# Required Consistency Rule

All Turn Meter lock effects must follow:

* Character card border color
* TM bar border color
* TM lock animation color
* Effect label color (existing requirement)
* Filled TM pipe animation color (this feature)

All must derive from a single effect visual definition.

---

# Acceptance Criteria

## Scenario 1

**Given** a character has an active TM lock effect

**When** the effect is applied

**Then** only filled TM pipes are visually affected

**And** unfilled TM pipes remain unchanged

---

## Scenario 2

**Given** a TM lock effect is active (e.g., Stun)

**When** the TM bar is displayed

**Then** the filled TM pipes blink using the same color as the character card border

**And** the TM border uses the same color

**And** the effect label uses the same color

---

## Scenario 3

**Given** a character is close to their turn (high TM fill)

**When** a lock effect is applied

**Then** progress remains visually readable

**And** the lock state is still clearly identifiable

---

## Scenario 4

**Given** the TM lock effect expires

**When** the effect ends

**Then** all TM pipe visuals return to normal immediately

**And** TM progression continues without visual artifacts

---

# Non-Functional Requirements

* Maintain real-time performance (no per-frame heavy UI recomputation).
* Avoid full re-render of TM UI on each tick if possible.
* Ensure animation synchronization between:

  * Character card border
  * TM border
  * Filled pipe animation

---

# Notes for Implementation

* Introduce or extend a shared `EffectVisualDefinition` model.
* Separate:

  * Fill-state rendering (progress)
  * Lock-state rendering (overlay animation)
* Ensure lock overlay does not destroy underlying progression state.
