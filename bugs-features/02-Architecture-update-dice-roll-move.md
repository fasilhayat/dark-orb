# Architecture Update Task + Dice Roll Migration

Project: Dark Orb

---

# Task 1 - Architecture Correction: Game Engine Authority Enforcement

File: `architecture-update.md`

---

## Objective

Refactor system boundaries so that the **Game Core Engine becomes the sole authority for all game rule execution**, while the REST API is strictly limited to:

* persistence
* retrieval
* updates to persistent entities (XP, inventory, spells, etc.)

The API must NOT contain or execute any game logic.

---

## Current Problem

The system currently violates separation of concerns:

* Dice rolls exist in API layer
* Game logic partially exists in API orchestration layer
* Combat outcomes may be influenced outside the engine
* Determinism is not guaranteed at domain level

---

## Target Architecture

### Game Core Engine (AUTHORITY LAYER)

Responsible for:

* combat resolution
* dice rolling
* damage calculation
* turn order logic
* status effects
* resource systems (HP / Mana / TM)
* deterministic simulation via seeded RNG

Characteristics:

* pure domain logic
* no database access
* no HTTP awareness
* fully deterministic

---

### REST API (PERSISTENCE LAYER ONLY)

Responsible for:

* storing character state
* retrieving character data
* updating XP
* updating inventory
* updating spell acquisitions
* persisting combat results

Prohibited responsibilities:

* NO dice rolling
* NO combat logic
* NO rule evaluation
* NO randomness generation

---

## Enforcement Rules

* Any game mechanic in API is considered a **critical architecture violation**
* API must treat engine as black-box service
* All outcomes must originate from engine execution

---

## Required Changes

* Identify all game logic currently in API layer
* Move logic into Game Core Engine
* Replace API logic with engine calls
* Ensure no duplicated rule logic remains in API

---

## Validation Criteria

* [ ] API contains zero game logic
* [ ] Game Core Engine contains all rule execution
* [ ] No randomness exists outside engine
* [ ] API cannot influence combat outcomes
* [ ] Deterministic replay preserved

---

# Task 2 - Dice Roll Migration to Game Core Engine

File: `feature-dice-roll-migration.md`

---

## Objective

Move all dice rolling functionality from the REST API layer into the Game Core Engine.

All randomness must be generated inside the engine using a deterministic, seed-based RNG system.

---

## Scope of Change

### MUST MOVE

* d20 rolls
* d8 rolls
* d6 rolls
* d4 rolls
* any derived roll logic (crit, fumble, hit/miss resolution)

---

### MUST REMOVE FROM API

* any random number generation
* any dice roll utilities
* any combat resolution randomness
* any probability calculations affecting combat outcome

---

## Required Engine Behavior

### RNG System

The Game Core Engine must:

* use a seeded RNG per combat session
* ensure identical seed → identical result
* expose deterministic roll functions:

  * `RollD20()`
  * `RollD8()`
  * `RollD6()`
  * `RollD4()`

---

## Determinism Requirement

Given:

* identical seed
* identical input state

The engine MUST produce:

* identical dice rolls
* identical combat outcome
* identical combat log

---

## API Replacement Behavior

API must replace all dice logic with:

* `Engine.ExecuteCombat(request)`
* or equivalent orchestration call

API must NOT:

* compute any roll
* override roll results
* simulate partial combat logic

---

## Logging Requirement

Dice rolls must be:

* emitted by the engine
* included in combat log
* reproducible from replay JSON

Example log format remains:

```text
DICE  d20 → 17
DICE  d8 → 6
```

But the source must be engine-only.

---

## Migration Steps

1. Identify all API-level dice logic
2. Extract logic into Game Core Engine RNG module
3. Replace API calls with engine calls
4. Wire engine RNG into combat execution pipeline
5. Validate replay consistency using seed
6. Remove API randomness utilities completely

---

## Validation Criteria

* [ ] No dice logic exists in API layer
* [ ] All dice rolls originate in Game Core Engine
* [ ] Combat outcomes are deterministic via seed
* [ ] Replay system produces identical results
* [ ] Combat log shows engine-generated rolls only
* [ ] No fallback randomness paths exist

---

## Final Architectural Constraint

After this change:

> The API is incapable of generating or influencing randomness.

All randomness is:

* engine-owned
* seed-controlled
* replayable
* fully logged

---

# Combined Acceptance Criteria

* [ ] Game Core Engine is sole authority for all combat rules
* [ ] API is reduced to persistence + orchestration only
* [ ] Dice rolling fully migrated to engine
* [ ] Deterministic replay system remains intact
* [ ] No duplicated logic between API and engine
* [ ] No randomness exists outside engine boundary
