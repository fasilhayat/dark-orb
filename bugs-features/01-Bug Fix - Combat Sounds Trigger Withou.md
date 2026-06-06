# Bug Fix - Combat Sounds Trigger Without Corresponding Visual Feedback

Project: Dark Orb

---

## Problem

A combat sound effect is sometimes played without any visible combat event being shown to the player.

Examples may include:

* Spell activations
* Buff applications
* Debuff applications
* Crowd-control effects
* Perfect Parry
* Dodge
* Critical Hit
* Resource gains
* Passive ability triggers

This creates a disconnect between audio feedback and visual feedback.

The player hears that something happened but cannot determine:

* What happened
* Who triggered it
* Who was affected
* Which ability or effect was involved

---

## Expected Behavior

Every combat event that produces an audio cue must also produce a visible combat event.

The player should never hear a combat sound without being able to identify the corresponding action on screen.

The visual representation should use the same combat event label system already used for:

* Spell names
* Status effects
* Perfect Parry
* Critical Hit
* Dodge
* Heal
* Crowd-control effects

---

# Global Spell Visibility Requirement

All spells in the game must produce visible combat feedback when activated.

This requirement applies to:

* Active abilities
* Basic attacks implemented as spells
* Passive-triggered spells
* Buff spells
* Debuff spells
* Healing spells
* Damage spells
* Crowd-control spells
* Resource manipulation spells
* Area-of-effect spells
* Future spells added to the game

Whenever a spell is executed:

* The spell name must be displayed as floating combat text.
* The text must appear on the affected character(s) or appropriate combat target.
* The text must use the existing combat label system.
* The text must be visible long enough for the player to identify the action.
* The spell label must appear regardless of whether the spell causes damage.
* The spell label must appear regardless of whether the spell succeeds or fails.

Examples:

* Fireball → "Fireball"
* Heal → "Heal"
* Stun → "Stun"
* Mana Burn → "Mana Burn"
* Frost Nova → "Frost Nova"
* Shield Wall → "Shield Wall"

The player should always be able to visually follow combat actions without relying solely on audio cues.

---

## Combat Feedback Rule

### New Global Rule

Any event that triggers a combat sound must also trigger a visible combat label.

Examples:

| Event            | Sound | Visual Label Required |
| ---------------- | ----- | --------------------- |
| Fireball         | Yes   | Yes                   |
| Heal             | Yes   | Yes                   |
| Perfect Parry    | Yes   | Yes                   |
| Stun             | Yes   | Yes                   |
| Dodge            | Yes   | Yes                   |
| Freeze           | Yes   | Yes                   |
| Passive Trigger  | Yes   | Yes                   |
| Mana Gain Effect | Yes   | Yes                   |
| Resource Drain   | Yes   | Yes                   |

No exceptions.

---

## Required Fix

Investigate all combat event paths that trigger audio playback.

For every sound-producing event:

1. Verify a combat label is created.
2. Verify the label is displayed on the correct character.
3. Verify the label remains visible long enough to be noticed.
4. Verify audio and visual feedback occur simultaneously.
5. Identify and fix any orphaned sound triggers.

Additionally:

6. Verify every spell in the game produces a visible spell label.
7. Verify spell labels use a consistent presentation format.
8. Verify future spells automatically inherit this behavior.
9. Remove any spell implementation that bypasses combat label generation.

---

## Architectural Requirement

Introduce a unified combat feedback pipeline.

Combat events should follow:

```text
Combat Event
    ↓
Combat Label Generation
    ↓
Visual Presentation
    ↓
Audio Playback
```

The combat label should become a mandatory part of spell execution.

Ideally:

* Spells cannot execute without generating a combat feedback event.
* Audio and visual systems consume the same event.
* Future spells automatically inherit this behavior.

This prevents future desynchronization between audio and visuals.

---

## Acceptance Criteria

### Scenario 1

**Given** a combat event occurs

**When** a sound effect is played

**Then** a matching combat label is displayed

**And** the player can identify the event source

---

### Scenario 2

**Given** any spell in the game is cast

**When** the spell executes

**Then** the spell name is displayed as floating combat text

**And** the sound effect and label appear together

---

### Scenario 3

**Given** a defensive reaction occurs

**When** the sound plays

**Then** the corresponding combat text is displayed

Examples:

* Perfect Parry
* Dodge
* Block

---

### Scenario 4

**Given** a status effect is applied

**When** the status effect sound plays

**Then** the status effect label is displayed

Examples:

* Stun
* Freeze
* Sleep
* Petrify
* Poison
* Burn

---

### Scenario 5

**Given** a new spell is added to the game

**When** the spell is executed

**Then** the spell automatically generates a combat label

**And** no additional implementation work is required

---

### Scenario 6

**Given** combat is running

**When** reviewing combat activity

**Then** every audible event has a corresponding visible event

**And** no audio-only events exist

---

## Validation Checklist

* [ ] Every sound-producing combat event displays a label
* [ ] Every spell displays its name when activated
* [ ] Audio and visual feedback occur together
* [ ] No orphaned audio events exist
* [ ] Spell sounds display spell names
* [ ] Status effect sounds display effect names
* [ ] Defensive reactions display labels
* [ ] Passive ability activations display labels
* [ ] Resource-related sounds display labels
* [ ] Newly added spells automatically inherit this behavior
* [ ] Existing combat labels continue functioning correctly

---

## Notes

The governing principle for combat feedback should be:

> If the player can hear it, the player must also be able to see it.

Additionally:

> Every spell cast in Dark Orb must generate visible combat feedback, regardless of spell type, outcome, or implementation path.

Audio and visual combat feedback should always remain synchronized to improve combat readability and player understanding.
