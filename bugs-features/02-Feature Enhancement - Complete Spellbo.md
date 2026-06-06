# Feature Enhancement - Complete Spellbook Seeding and Spell Progression Alignment

Project: Dark Orb

---

## Objective

Complete the implementation of the Dark Orb spell system by aligning the database seed data with the master design specification located at:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

The master spellbook is the authoritative source of truth for:

* Spell definitions
* Spell schools
* Spell progression
* Access restrictions
* Damage types
* Resource effects
* Turn Meter effects
* Health effects
* Mana effects
* Status effects
* Afterburn effects

The AI agent must treat the spellbook as the only valid source of truth. Any assumptions not explicitly defined in it are invalid.

All remaining spells defined in the spellbook must be seeded into the database.

---

# Phase 1 - Spellbook Review and Correction

## Required Update Before Any Database Work

Before implementing any database changes:

1. Read:

   ```text
   dark-orb/design/dark-orb-master-spellbook.md
   ```

2. Update the spellbook itself where required.

### Fireball Revision Requirement

Fireball must be revised:

* Entry-level Fireball must NOT include afterburn effects.
* Afterburn is introduced only at higher caster progression levels.
* Afterburn potency scales with progression tier.
* Base Fireball is direct damage only.

---

### Ice Storm Revision Requirement

Ice Storm must be revised:

* Entry-level Ice Storm must NOT include lingering secondary effects.
* Secondary effects are unlocked at higher progression tiers.
* Base Ice Storm is direct frost damage only.

---

# Phase 2 - Spell School Source of Truth (MANDATORY)

Spell schools are defined in:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

### Critical Rule

Do NOT:

* Invent school names
* Rename schools
* Hardcode school lists
* Assume canonical fantasy archetypes

### Required Behavior

Before seeding or modifying anything:

1. Extract all spell schools directly from the spellbook.
2. Use exact naming and identifiers from the spellbook.
3. Use exact relationships defined in the spellbook.
4. Use exact access rules defined in the spellbook.

### Enforcement Rule

If any mismatch exists between:

* Database
* Existing seeds
* Character assignments
* Spell assignments

and the spellbook:

➡ The spellbook overrides everything.

---

## School Validation Requirements

Verify:

* All schools defined in the spellbook exist in database.
* No extra or missing schools exist.
* All spells belong to valid spellbook-defined schools.
* All progression paths stay within their defined school rules.
* No legacy or placeholder schools exist.

---

## Validation Output Required

Produce:

* List of schools discovered in spellbook
* Spell count per school
* Missing spells per school
* Incorrect assignments
* Fixes applied

---

# Phase 3 - Complete Spell Seeding

## Requirement

Seed ALL remaining spells defined in:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

Only missing entries should be inserted.

---

## Seeding Rules (STRICT)

* Use only `INSERT` statements.
* Do NOT use `ALTER` statements.
* Do NOT update schema.
* Do NOT create migrations.
* This must be a clean initialization dataset.

---

# Phase 4 - Character Spell Assignment Alignment

Review all existing seeded characters.

Update spell assignments so they strictly conform to spellbook rules.

### Exception - DO NOT MODIFY

The following characters are frozen:

* Training Dummy
* Golem

No changes are allowed to these entities.

---

# Phase 5 - Smite / Chasten

## Smite Restriction

Smite is restricted to:

* Paladins
* Knights

No other classes may use Smite.

---

## Chasten Introduction

Introduce a new spell:

* Name: Chasten
* Role: Divine counterpart to Smite for non-martial divine casters

Allowed classes:

* Priests
* Druids

Requirements:

* Must match Smite tier structure
* Must match Smite progression logic
* Must remain balanced against Smite
* Must follow Divine school rules from spellbook

---

# Phase 6 - Progression Validation

Verify all spell progression chains:

* Entry tier exists
* Intermediate tiers exist
* Advanced tiers exist
* Scaling is correct
* Resource costs are consistent
* Effects scale correctly
* Afterburn rules are respected
* School rules remain intact

---

# Phase 7 - Database Integrity Validation

Ensure:

* No duplicate spells
* No orphaned references
* No invalid school assignments
* No invalid character assignments
* No broken progression chains

---

# Deliverables

## 1. Spellbook Updates

Update:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

Must include:

* Fireball afterburn revision
* Ice Storm revision
* Smite restriction rules
* Chasten definition
* Any inconsistencies discovered during execution

---

## 2. Seed Data Output

Generate:

* Spell seeds (INSERT only)
* Spell progression seeds (INSERT only)
* Character spell assignments (INSERT only)
* School mapping seeds (INSERT only)

---

## 3. Validation Report

Include:

* List of schools discovered from spellbook
* Verification that database matches spellbook schools exactly
* List of seeded spells
* List of updated character assignments
* Smite vs Chasten verification
* Confirmation that Training Dummy was not modified
* Confirmation that Golem was not modified
* Confirmation of INSERT-only rule compliance

---

# Acceptance Criteria

* [ ] Spellbook is read first and treated as source of truth
* [ ] Fireball revised to remove early afterburn
* [ ] Ice Storm revised to remove early secondary effects
* [ ] All spell schools are derived from spellbook only
* [ ] No hardcoded or assumed school names exist
* [ ] All remaining spells seeded
* [ ] Only INSERT statements used
* [ ] Character assignments updated correctly
* [ ] Training Dummy untouched
* [ ] Golem untouched
* [ ] Smite restricted correctly
* [ ] Chasten implemented correctly
* [ ] Database matches spellbook exactly
* [ ] No orphaned or invalid references remain
