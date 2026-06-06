# Bug Fix - Heal Gain Overlay Positioned Incorrectly on HP Bar

Project: Dark Orb

---

## Problem

The healing visualization is rendering incorrectly.

When a character receives healing, the temporary glowing white HP gain overlay appears somewhere in the middle of the HP bar rather than being attached to the current edge of the filled HP bar.

This makes the heal animation appear disconnected from the actual health increase and reduces visual clarity.

---

## Expected Behavior

The heal gain overlay should always originate from the current end position of the filled HP bar and visually extend outward to represent the incoming health gain.

Example:

Current HP:

```text
[████████░░░░░░░░░░]
```

Healing Incoming:

```text
[████████▒▒▒░░░░░░░]
```

Where:

* `████████` = current HP
* `▒▒▒` = glowing heal gain overlay
* `░░░░░░░` = remaining empty HP capacity

The heal overlay must always begin immediately after the currently filled HP segment.

---

## Current Incorrect Behavior

Example:

```text
[████▒▒▒████░░░░░░]
```

The heal overlay appears inside the existing HP region rather than at its edge.

This creates a visually incorrect representation of where the health increase is being applied.

---

## Required Fix

Review the HP bar rendering logic and verify that heal overlay positioning is calculated from:

```text
Current HP Percentage
```

rather than:

```text
Bar midpoint
Container midpoint
Previous cached position
Animation origin
```

The heal overlay should:

* Start at the current HP fill endpoint.
* Extend toward the future HP value.
* Never overlap existing filled HP.
* Never originate from the center of the bar.
* Recalculate correctly for every heal event.

---

## Acceptance Criteria

### Scenario 1

**Given** a character has 40% HP

**When** a heal is applied

**Then** the heal overlay begins exactly at the 40% fill position

**And** extends toward the new HP position

---

### Scenario 2

**Given** a character has 75% HP

**When** a heal is applied

**Then** the heal overlay begins at the current HP endpoint

**And** does not appear inside the existing HP region

---

### Scenario 3

**Given** a character receives multiple heals

**When** each heal animation plays

**Then** the overlay is positioned correctly every time

**And** no drifting or offset accumulates

---

### Scenario 4

**Given** a character is near maximum HP

**When** a small heal is applied

**Then** the heal overlay is rendered at the end of the filled HP bar

**And** remains visually attached to the HP edge

---

## Validation Checklist

* [ ] Heal overlay always starts at current HP endpoint
* [ ] Heal overlay never appears in the middle of the HP bar
* [ ] Heal overlay scales correctly with different HP percentages
* [ ] Consecutive heals remain correctly positioned
* [ ] Near-full HP scenarios render correctly
* [ ] No visual jitter or offset occurs during animation

---

## Investigation Notes

Likely areas to inspect:

* HP bar percentage-to-pixel conversion
* Heal animation origin calculation
* Cached fill width values
* UI scaling calculations
* Resolution-independent positioning logic
* HP bar fill width versus container width calculations

The heal overlay should be anchored to the actual rendered HP fill endpoint, not to any fixed position within the HP bar container.
