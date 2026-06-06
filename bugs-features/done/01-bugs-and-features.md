# Bug Fix and Feature Improvement Task

Project: Dark Orb

## Objective

Analyze the existing combat engine, turn meter system, combat lifecycle management, status effect processing, and UI state transitions.

Implement the following bug fixes and feature improvements.

---

# Bug 1 - Combat Session Not Properly Terminated

## Problem

When a combat has started and is actively running, the combat process continues in the background if the user:

* Clicks Main Menu
* Starts a New Combat
* Navigates away from the Combat Screen

This causes the previous combat session to continue processing while a new combat session is created.

The result is multiple combat sessions existing simultaneously and stale combat state leaking into newly started battles.

## Required Fix

Whenever the user leaves the combat screen or initiates a new combat:

* Stop all running combat timers.
* Stop all combat loops.
* Stop all combat background tasks.
* Stop all turn meter processing.
* Remove event subscriptions related to the combat.
* Dispose active combat resources.
* Clear current combat references.
* Reset combat state.

There must never be more than one active combat session.

## Acceptance Criteria

### Scenario 1

**Given** a combat is running

**When** the user clicks Main Menu

**Then** the active combat session is completely terminated

**And** no combat processing continues in the background

### Scenario 2

**Given** a combat is running

**When** the user clicks New Combat

**Then** the current combat session is terminated

**And** a fresh combat session is created

**And** no state from the previous combat remains

### Scenario 3

**Given** multiple combats have been started sequentially

**When** inspecting runtime state

**Then** only one combat session exists

**And** only one combat loop is active

---

# Bug 2 - Stun Turn Meter Lock Timing Issue

## Problem

The stun visual effect activates immediately.

However:

* The turn meter lock is applied later.
* The victim sometimes gains turn meter for several ticks after being stunned.
* After the stun expires, turn meter generation may remain frozen indefinitely.

The visual state and mechanical state are out of sync.

## Required Fix

When stun is applied:

* Turn meter lock must activate immediately.
* Turn meter gain must stop immediately.
* UI indicators must activate immediately.

When stun expires:

* Turn meter gain must resume immediately.
* All temporary lock states must be removed.
* Normal turn meter progression must continue.

The stun effect must have identical start and end timing for:

* Mechanics
* Visuals
* Turn meter updates

## Acceptance Criteria

### Scenario 1

**Given** a character receives Stun

**When** the stun is applied

**Then** turn meter gain stops immediately

**And** no additional turn meter is gained during the stun duration

### Scenario 2

**Given** a stunned character

**When** stun expires

**Then** turn meter gain resumes immediately

**And** the character can continue progressing toward a turn

### Scenario 3

**Given** repeated stun applications

**When** combat runs for multiple rounds

**Then** no permanent turn meter lock occurs

**And** turn meter recovery functions correctly every time

---

# Feature Improvement - Unified Turn Meter Lock Visual System

## Goal

Create a consistent visual language for all effects that prevent turn meter progression.

## Requirement

Any effect that blocks, freezes, disables, pauses, or locks turn meter progression must use the same visual indicators.

### Required Visual Behaviour

When a turn meter lock effect is active:

* Turn meter border blinks.
* Blink color matches the character card border effect.
* Turn meter pipe markers become white.
* Turn meter bar enters a locked visual state.
* Visual state begins immediately when the lock begins.

When the lock effect ends:

* Turn meter border returns to normal.
* Pipe markers return to normal.
* Turn meter resumes normal updates.
* All lock visuals are removed immediately.

### Effect Label Synchronization

When a turn meter lock effect is active:

* The effect label text color must match the character card border blink color.
* The effect label text color must match the turn meter lock indicator color.
* The effect label, card border, and turn meter lock visuals must always use a shared color source.

Examples:

* Stun → Yellow border, yellow effect label, yellow turn meter lock visuals.
* Freeze → Blue border, blue effect label, blue turn meter lock visuals.
* Sleep → Purple border, purple effect label, purple turn meter lock visuals.
* Petrify → Gray border, gray effect label, gray turn meter lock visuals.

The purpose is to create a clear visual association between:

* Character card border effect
* Turn meter lock indicator
* Effect label text

All three visual elements should communicate the same status effect using a consistent color scheme.

### Architectural Requirement

Implement a centralized effect visual configuration.

All visual properties for crowd-control effects must be derived from a single source of truth, including:

* Character card border color
* Character card border animation
* Effect label color
* Turn meter lock color
* Future crowd-control visual indicators

Future crowd-control effects should be able to reuse this configuration without introducing duplicated visual logic.

## Scope

Apply this behaviour to:

* Stun
* Freeze
* Sleep
* Petrify
* Future turn meter lock effects

Any future effect that disables turn meter progression should automatically inherit the same visual behaviour.

## Acceptance Criteria

### Scenario 1

**Given** a turn meter lock effect is applied

**When** the effect becomes active

**Then** the turn meter border begins blinking immediately

**And** the pipe markers become white

### Scenario 2

**Given** a character is affected by Stun

**When** the stun effect becomes active

**Then** the character card border blinks using the stun color

**And** the turn meter lock indicator uses the stun color

**And** the effect label text displays using the same stun color

### Scenario 3

**Given** different crowd-control effects

**When** they lock turn meter progression

**Then** they all use the same visual lock indicators

**And** they all use matching effect label colors

**And** the effect can be identified visually without reading additional information

### Scenario 4

**Given** a turn meter lock effect expires

**When** the effect ends

**Then** all lock visuals are removed

**And** the effect label returns to its default styling

**And** the character card border returns to its normal state

**And** turn meter progression resumes normally

---

# Validation Requirements

Perform the following verification:

* [ ] Start combat and exit to Main Menu.
* [ ] Start combat and immediately start another combat.
* [ ] Verify no background combat sessions survive.
* [ ] Verify only a single combat session can exist at a time.
* [ ] Verify stun locks turn meter immediately.
* [ ] Verify stun expiration restores turn meter immediately.
* [ ] Verify multiple stun applications do not permanently freeze turn meter.
* [ ] Verify all turn meter locking effects use identical visual indicators.
* [ ] Verify effect labels use the same color as their associated crowd-control effect.
* [ ] Verify no memory leaks or orphaned timers remain after combat termination.

## Deliverables

1. Root cause analysis for each bug.
2. Description of the implemented fix.
3. List of affected files.
4. Regression testing results.
5. Confirmation that all acceptance criteria pass.
6. Confirmation that only a single combat session can exist at any given time.
7. Confirmation that all turn meter lock effects use the centralized visual configuration.
