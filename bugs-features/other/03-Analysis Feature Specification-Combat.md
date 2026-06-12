# Analysis Feature Specification - Combat Benchmark System (Log + Replay Hybrid)

Project: Dark Orb

File: `analysis-feature.md`

---

## Objective

Build a deterministic combat analysis system that reconciles:

* Human-readable combat log (event stream)
* Replay JSON file (authoritative state definition)
* Combat engine output (runtime truth)

The goal is to detect:

* balance issues
* mechanical inconsistencies
* scaling problems
* desync between expected vs actual behavior

---

## Core Principle

There are 3 layers of truth:

### 1. Replay JSON (STATE TRUTH)

Defines:

* parties
* characters
* stats
* spells
* gear
* seed

This is the **intended simulation input state**.

---

### 2. Combat Log (EVENT TRUTH)

Defines:

* tick-by-tick execution
* dice rolls
* damage application
* healing
* TM updates
* mana changes
* narrative events

This is the **observed runtime behavior**.

---

### 3. Engine Reconstruction (DERIVED TRUTH)

Reconstructed simulation state derived from:

* JSON + deterministic rules + log execution

This is the **computed model used for validation**.

---

# Input Specification

Each analysis run consumes:

---

## 1. Replay JSON (PRIMARY STATE INPUT)

Example structure:

* seed
* label
* timestamp
* party1 / party2
* character stats
* armor
* weapons
* spells

### Role:

Defines the **initial conditions of combat**

Key responsibilities:

* base stats validation
* spell availability validation
* gear validation
* level scaling baseline

---

## 2. Combat Log (PRIMARY EVENT TRACE)

Example:

```
TICK LOG
MANACOST
DICE
DAMAGE
TM updates
HEAL
FUMBLE
KNOCKOUT
```

### Role:

Defines **what actually happened**

Must be treated as:

* authoritative execution trace
* tick-sequenced event stream

---

## 3. Derived Engine Ruleset (IMPLICIT)

Includes:

* damage formulas
* TM formula
* crit rules
* mitigation rules
* spell scaling logic

---

# Phase 1 - JSON State Validation Layer

## Purpose

Validate replay JSON integrity BEFORE analyzing combat.

---

## Checks

### 1. Character Validation

For each party:

* stats present
* HP > 0
* level valid
* turnSpeed valid
* classId valid

---

### 2. Equipment Validation

* armor exists
* weapon exists
* mitigation values valid
* attack types valid

---

### 3. Spell Validation

Each spell must contain:

* damageDie
* damageCount
* attackBonus
* spellLevel
* school
* onHitEffects

---

### 4. Structural Validation

* both parties exist
* members exist
* names consistent with log header label

---

## Output

* VALID / INVALID state snapshot
* validation error list

---

# Phase 2 - Log Parsing Layer

## Purpose

Convert TXT log into structured event stream.

---

## Parsing Targets

### Tick Events

```
[    1]  TM
[    1]  MANACOST
[    1]  ══ TURN
[    1]  DICE
[    1]  DAMAGE
[    1]  HEALED
```

---

## Event Normalization

Convert into:

* TURN_START
* DICE_ROLL
* DAMAGE_EVENT
* HEAL_EVENT
* MANA_EVENT
* TM_EVENT
* STATUS_EVENT
* KNOCKOUT_EVENT
* NARRATIVE_EVENT

---

## Rule

* ignore formatting characters
* preserve tick ordering
* preserve actor identity

---

# Phase 3 - State Reconstruction Engine

## Purpose

Rebuild combat state over time.

---

## Inputs

* JSON initial state
* normalized event stream

---

## Output State Per Tick

Each tick must include:

* HP values
* Mana values
* TM values
* status effects
* active spell effects

---

## Critical Rule

If reconstructed state deviates from log OR JSON:

➡ FLAG DESYNC

---

# Phase 4 - Combat Analysis Engine

## Purpose

Extract balance metrics.

---

## Core Metrics

### 1. Damage Analysis

* total damage per unit
* DPS curve
* crit contribution
* fumble impact

---

### 2. Turn Meter Analysis

* TM gain per tick
* turn frequency
* stun/freeze disruption impact

---

### 3. Resource Analysis

* mana usage efficiency
* regen vs cost ratio
* starvation points

---

### 4. Spell Effectiveness

* Smite vs Heal efficiency
* damage per mana
* scaling per level

---

### 5. Survivability Curve

* HP decay over time
* burst detection
* sustain classification

---

# Phase 5 - Benchmark Modes

---

## 1. Baseline Mode

* uses JSON-defined characters
* runs single combat
* produces baseline metrics

---

## 2. Level Scaling Mode (FUTURE HOOK)

* simulate level variations
* adjust stats dynamically
* measure scaling curves

---

## 3. Gear Swap Mode (FUTURE HOOK)

* replace armor/weapon sets
* measure impact deltas

---

## 4. Spell Progression Mode (FUTURE HOOK)

* enable/disable spells
* measure marginal value

---

# Phase 6 - Desync Detection System

---

## Hard Rule

If any of the following mismatch:

* JSON says HP = X
* Log implies HP ≠ X
* Reconstruction differs

➡ DESYNC ERROR

---

## Types of Desync

* DAMAGE mismatch
* MANA mismatch
* TM mismatch
* SPELL mismatch
* ORDER mismatch

---

# Phase 7 - Output Format (NO NEW JSON REQUIRED)

System outputs:

---

## 1. Validation Report

* JSON validity
* log integrity
* desync check result

---

## 2. Combat Timeline Reconstruction

* tick-by-tick resolved state

---

## 3. Metrics Report

* DPS
* TTK
* hit/crit/fumble rates
* resource efficiency

---

## 4. Balance Report

* overpowered/underpowered detection
* scaling anomalies
* spell efficiency ranking

---

## 5. Replay Consistency Report

* MATCH or DESYNC
* diff explanation if mismatch

---

# Critical Design Insight

You do NOT need a new JSON schema.

You already have:

* JSON = static truth
* log = dynamic truth
* engine = reconciliation layer

The system you are building is fundamentally a:

> **Combat reality validator**

not a simulator generator.

---

# Non-Negotiable Constraints

* JSON is immutable input state
* Log is immutable execution trace
* No inferred hidden mechanics allowed
* No guesswork on missing values
* Determinism required

---

# Acceptance Criteria

* [ ] JSON fully parsed and validated
* [ ] Log fully parsed into event stream
* [ ] State reconstruction matches both sources
* [ ] Desync detection implemented
* [ ] Damage / TM / Mana consistency validated
* [ ] Replay correctness verified
* [ ] No schema changes required
* [ ] Works on existing combat logs immediately
