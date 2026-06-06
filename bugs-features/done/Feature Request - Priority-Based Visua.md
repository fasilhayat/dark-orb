# Feature Request - Priority-Based Visual Effect Resolution for Stacked Status Effects

Project: Dark Orb

---

## Objective

Improve the visual handling of multiple simultaneous status effects on a character.

When several status effects are active at the same time, the character card border should visually represent the most recently applied effect rather than attempting to display multiple competing visual states.

This creates a clear and predictable visual experience for the player.

---

## Current Behavior

Status effects can stack on a character.

Examples:

* Stun
* Freeze
* Sleep
* Petrify
* Bleed
* Burn
* Poison

When multiple effects are active simultaneously, visual indicators may compete for control of the character card border.

This can lead to:

* Unclear visual state
* Flickering between effects
* Incorrect color representation
* Difficulty determining which effect was applied most recently

---

## Desired Behavior

Status effects should continue stacking mechanically.

However, only one status effect should control the character card border visuals at any given time.

### Visual Ownership Rule

The most recently applied effect becomes the visual owner.

Examples:

#### Example 1

Character receives:

1. Burn
2. Poison
3. Stun

Result:

* All three effects remain active.
* All three effects continue functioning mechanically.
* Stun controls the border visuals because it was applied last.

#### Example 2

Character receives:

1. Stun
2. Freeze

Result:

* Stun remains active.
* Freeze remains active.
* Freeze controls the border visuals because it was applied last.

---

## Visual Resolution Logic

Whenever a new status effect is applied:

1. Add the effect to the active effect list.
2. Preserve all existing effects.
3. Update visual ownership.
4. Assign border visuals to the newest active effect.

Whenever the visual owner expires:

1. Remove the expired effect.
2. Determine the newest remaining active effect.
3. Transfer visual ownership to that effect.
4. Update the border immediately.

---

## Scope

The visual ownership system should govern:

* Character card border color
* Character card border blinking
* Character card border animation
* Future border-based status effect visuals

This feature does not affect:

* Damage calculations
* Status effect duration
* Turn meter behavior
* Combat mechanics

Only visual presentation is changing.

---

## Architectural Requirement

Introduce a dedicated visual-effect resolver.

Responsibilities:

* Track active status effects
* Track effect application order
* Determine current visual owner
* Update border visuals consistently
* Handle expiration and ownership transfer

The resolver should become the single source of truth for character card visual state.

---

## Acceptance Criteria

### Scenario 1

**Given** a character has Burn active

**When** Stun is applied afterward

**Then** Burn remains active

**And** Stun remains active

**And** the character card border displays the Stun visual style

---

### Scenario 2

**Given** a character has Burn, Poison, and Stun active

**When** Freeze is applied

**Then** all effects remain active

**And** Freeze becomes the visual owner

**And** the border updates immediately to Freeze visuals

---

### Scenario 3

**Given** Freeze currently owns the visuals

**When** Freeze expires

**Then** the next most recently applied active effect becomes the visual owner

**And** the border updates immediately

---

### Scenario 4

**Given** a character has no active visual effects

**When** all status effects expire

**Then** the border returns to its default state

---

## Validation Checklist

* [ ] Status effects continue stacking mechanically
* [ ] Only one effect controls border visuals at a time
* [ ] Most recently applied effect always wins visual ownership
* [ ] Expired visual owners correctly transfer ownership
* [ ] Border updates immediately on ownership changes
* [ ] No visual flickering occurs
* [ ] Default border is restored when no effects remain

---

## Notes

This feature establishes a predictable visual hierarchy:

**Latest Applied Effect Wins**

Mechanical stacking remains unchanged.

Only the visual ownership of the character card border is affected.
