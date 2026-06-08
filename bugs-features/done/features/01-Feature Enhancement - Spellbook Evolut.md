# Spellbook Update Task - Deity Alignment System Integration

Project: Dark Orb

File Target:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

---

## Objective

Update the master spellbook to introduce a **Deity-Based Power System** for divine casters.

This is a **documentation-only update task**.

No database changes, no seeding, and no implementation logic are part of this step.

---

# Core System Change

A subset of classes no longer derive their power from spell schools.

Instead, their magic originates from **deities**.

---

# Affected Classes

The following classes are now classified as **deity-aligned casters**:

* Priest
* Paladin
* Knight
* Druid

These classes:

* Do NOT use spell schools as their primary identity system
* DO use deities as their primary source of magical power
* MAY still reference schools for legacy classification or mechanical grouping, but not for progression or identity logic

---

# Deity Source of Truth

Deities are defined in:

```text
dark-orb/design/assets/deities_names-alignment.md
```

---

## Canonical Deity List (Authoritative)

### Good Deities (Sky / Heaven Aligned)

* **Aethelion** — The radiant father of light
* **Astrara** — The guiding star mother
* **Celestara** — The weaver of destiny
* **Lunara** — The silver moon goddess

---

### Evil Deities (Elemental / Shadow Aligned)

* **Ignaroth** — The burning destroyer
* **Umbraex** — The void lord
* **Veparix** — The deceptive mist
* **Noctivane** — The shadow assassin god

---

# System Change: Spell Schools vs Deities

## Existing System (Still Valid for Non-Divine Casters)

Spell schools remain valid for:

* Arcane casters
* Fire-based magic users
* Frost-based magic users
* Shadow casters
* Nature casters
* Any non-divine progression systems

Schools remain:

* Fully intact
* Fully functional
* Unchanged for non-divine systems

---

## New System (Divine Casters Only)

For the following classes:

* Priest
* Paladin
* Knight
* Druid

### Primary Classification Rule

Divine spells must now include:

* Primary Deity Source (mandatory)
* Deity Alignment (Good / Evil)
* Optional fallback: `DEITY_UNBOUND`

Schools are no longer the primary classification system for these classes.

---

# Placeholder System Requirement

To support incomplete or transitional mappings:

## Required Placeholder

```text
DEITY_UNBOUND
```

### Meaning:

* No specific deity assigned
* Generic divine power source
* Temporary fallback state
* Used until explicit deity binding is defined

---

# Smite / Chasten System Update

## Smite

Smite is now defined as:

* A **Deity-Channelled Divine Attack Spell**
* Available only to:

  * Paladins (Level 6+)
  * Knights (Level 6+)

### Smite Rules:

* Must be associated with a valid deity (Good or Evil depending on subclass definition)
* Must support future scaling via deity alignment mechanics
* Cannot be used outside divine caster archetypes listed above

---

## Chasten

Chasten is defined as:

* The **divine equivalent of Smite for non-martial casters**

### Available to:

* Priests (Level 1+)
* Druids (Level 1+)

### Chasten Rules:

* Default bound to Good Deities
* Must support `DEITY_UNBOUND` fallback state
* Must mirror Smite progression structure
* Must remain mechanically balanced relative to Smite

---

# Spell Metadata Extension (Documentation Requirement)

All divine spells in the spellbook must now support the following conceptual fields:

* `PrimaryDeity`
* `DeityAlignment`
* `DeitySource`
* `FallbackDeity = DEITY_UNBOUND`

These fields define:

* Power origin
* Alignment behavior
* Progression dependency (future system hook)
* Fallback behavior for incomplete assignments

---

# Progression Model Update (Divine Casters)

For divine casters, progression is no longer strictly school-driven.

Instead, progression considers:

* Deity alignment compatibility
* Class–deity synergy
* Divine tier progression (to be defined in later expansions)

Schools remain secondary metadata only.

---

# Compatibility Rules

* Non-divine casters remain unchanged
* Existing spell schools remain valid outside divine system
* No removal of current school system
* No breaking changes to arcane/frost/fire systems

---

# Required Updates to Spellbook

Update:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

to include:

## 1. Deity System Section

* Full definition of deity-based magic
* Canonical deity list (from assets file)
* Alignment categorization (Good / Evil)
* Placeholder system (`DEITY_UNBOUND`)

---

## 2. Divine Class Override Rule

Explicitly define:

* Priest, Paladin, Knight, Druid use deity system
* Schools are not primary for these classes
* Deity system overrides school system for divine logic only

---

## 3. Smite / Chasten Integration

* Smite restricted to Paladin + Knight (Level 6+)
* Chasten available to Priest + Druid (Level 1+)
* Both are deity-based spells

---

## 4. System Boundary Definition

Clearly separate:

* School-based magic system (Arcane/Fire/Frost/etc.)
* Deity-based magic system (Divine casters only)

---

# Acceptance Criteria

* [x] Spellbook updated with deity system section
* [x] Canonical deity list included from asset file
* [x] Priest/Paladin/Knight/Druid marked as deity-based casters
* [x] Schools remain intact for non-divine systems
* [x] DEITY_UNBOUND placeholder defined
* [x] Smite updated with level 6 restriction (Paladin 6, Knight 6)
* [x] Chasten added with level 1 access (Priest 1, Druid 1)
* [x] No gameplay or database assumptions introduced
* [x] No removal of existing systems

## Fix Applied

Updated `design/dark-orb-master-spellbook.md`:
- Added Deity System section with canonical deity list (Aethelion, Astrara, Celestara, Lunara for Good; Ignaroth, Umbraex, Veparix, Noctivane for Evil)
- Added DEITY_UNBOUND placeholder rules for incomplete mappings
- Marked Priest, Druid, Paladin, Knight as deity-aligned casters in class access rules
- Added school/deity boundary table separating the two systems
- Added Chasten to Priest spell table (Deity school, level 1, TM loss/debuff)
- Updated Smite entries: Paladin 6+ in Paladin table, Knight 6+ in Knight table
- Changed school column from "Dominion" to "Deity" for divine spells (Smite, Chasten)
- All existing school systems left intact for non-divine casters
