# Feature Specification - Spell SFX Replacement (Beep Audio Migration)

Project: Dark Orb

File: `feature-sfx-replacement.md`

---

## Objective

Replace all existing placeholder “beep” and legacy spell effect sounds in the GUI system with finalized production sound assets located in the `[SPECIFIC]` audio asset directory.

These new assets must fully overwrite existing files in:

```text
Assets/Sounds/
```

GUI project only.

---

## Scope

This change affects:

* Spell casting sounds
* Hit confirmation sounds
* Critical hit / impact sounds (if currently using placeholder beeps)
* UI feedback beeps tied to combat events

It does NOT affect:

* Music / background ambience
* Non-combat UI sounds unless explicitly mapped to beep placeholders

---

## Source of Truth

### New Audio Assets Location

```text
[SPECIFIC]/
```

This folder is the **authoritative source of replacement audio files**.

---

### Target Location (Overwrite Destination)

```text
GUI Project:
Assets/Sounds/
```

---

## Replacement Rule (STRICT)

This is a **full overwrite migration**, not a merge.

Rules:

* Every matching file in `Assets/Sounds/` must be replaced
* File names must remain identical OR be explicitly remapped (see mapping section)
* No legacy beep sounds may remain in active combat playback paths
* No fallback to old audio assets is allowed

---

## Mapping Requirement

Before overwrite, a mapping must be established:

### Required Mapping Table

For each sound:

* Original sound name (beep placeholder)
* New sound file from `[SPECIFIC]`
* Usage context (spell cast / hit / crit / UI / etc.)

Example structure:

* `beep_cast.wav` → `smite_cast_final.wav`
* `beep_hit.wav` → `impact_holy_01.wav`
* `beep_crit.wav` → `crit_blast_heavy_02.wav`

---

## Validation Requirement

After replacement:

### 1. File Integrity Check

* No missing audio files in `Assets/Sounds/`
* No orphan references in GUI code
* No broken playback calls

---

### 2. Playback Validation

Verify in combat replay:

* Spell cast triggers correct sound
* Hit events trigger correct sound
* Critical hits use correct impact sound
* No fallback beep audio remains anywhere

---

### 3. Regression Check

Ensure:

* No increase in audio latency
* No double playback events
* No silent failures in sound dispatcher

---

## Engine Integration Rule

Audio system must:

* Load only from `Assets/Sounds/`
* Not reference `[SPECIFIC]` at runtime
* Treat `[SPECIFIC]` as build-time source only

---

## Risk Considerations

### High Risk Areas

* Hardcoded beep references in GUI event handlers
* Cached audio clips in memory
* Legacy fallback sound hooks

These must be audited and removed if found.

---

## Deployment Behavior

This is a **destructive replacement operation**:

* Old beep assets are permanently removed from active use
* Replacement is immediate upon build refresh
* No dual-sound fallback system is permitted

---

## Acceptance Criteria

* [ ] All beep placeholder sounds replaced with production assets
* [ ] All replacements sourced from `[SPECIFIC]` directory
* [ ] No legacy beep audio used in combat playback
* [ ] Sound mapping validated and documented
* [ ] No missing or broken audio references
* [ ] Combat replay produces correct audio feedback
* [ ] No duplicate or overlapping sound triggers
