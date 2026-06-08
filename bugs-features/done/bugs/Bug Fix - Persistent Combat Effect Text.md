# Bug Fix - Persistent Combat Effect Text After Combat Termination

Project: Dark Orb

---

## Problem

When a combat session is stopped or ended, active combat UI elements—specifically floating spell effect text (e.g., “Stun”, “Damage”, “Perfect Parry”)—remain visible on screen outside of the combat context.

These UI artifacts persist across navigation to other screens (e.g., Main Menu, Character Screens) and are only cleared when a new combat starts and overwrites the display layer.

This indicates that combat-related UI overlays are not being properly disposed or reset on combat termination.

---

## Expected Behavior

When combat ends or is interrupted:

* All combat-specific UI overlays must be cleared immediately
* No floating combat text should persist outside the combat scene
* UI state must be fully reset when transitioning away from combat
* New screens must start with a clean UI canvas state

---

## Required Fix

On any of the following events:

* Combat ends naturally (win/lose)
* Combat is manually stopped
* User navigates away from combat screen
* Main menu is opened during combat
* New combat session is initiated

The system must:

### UI Cleanup Responsibilities

* Clear all floating combat text layers
* Clear all active status effect UI elements tied to combat
* Dispose or reset combat UI canvas/overlay layer
* Cancel any pending UI animations or delayed hide operations
* Reset any global/shared UI state used by combat rendering

### Architectural Requirement

Combat UI elements must be strictly scoped to the combat lifecycle.

No combat UI element may persist beyond the lifecycle of:

* CombatController / CombatSession
* Combat UI View / Scene

---

## Acceptance Criteria

### Scenario 1

**Given** a combat session is active

**When** the user ends combat or returns to main menu

**Then** all floating combat text is immediately removed

**And** no combat UI elements remain visible in other screens

---

### Scenario 2

**Given** combat has been stopped manually

**When** the game transitions to another screen

**Then** the UI is fully reset to a clean state

**And** no residual spell effect text is visible

---

### Scenario 3

**Given** no combat session is active

**When** the user navigates through menus or screens

**Then** no combat-related UI elements are rendered at any time

---

### Scenario 4

**Given** a new combat session starts

**When** combat UI initializes

**Then** only current combat events are displayed

**And** no previous combat UI artifacts appear

---

## Validation Checklist

* [x] Combat end clears all floating text immediately
* [x] UI does not leak between scenes
* [x] No persistent spell effect text exists outside combat
* [x] New combat starts with clean UI state
* [x] Navigation away from combat triggers full UI teardown
* [x] No delayed animations reintroduce old UI elements

---

## Notes

Likely root causes to investigate:

* Global/static UI overlay layers not scoped per combat session
* Missing disposal of floating text queue or animation manager
* Event listeners not unsubscribed on combat teardown
* UI canvas not being reset on scene transition

Ensure combat UI is fully lifecycle-bound and cannot survive beyond its owning session.
