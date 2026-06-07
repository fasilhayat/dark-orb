# Refactoring Task - Magic String Discovery and Enum Consolidation Plan

Project: Dark Orb

Priority: Medium-High

Type: Refactoring / Technical Debt

Status: Analysis and Planning Only (No Code Changes Authorized)

---

## Objective

Identify and analyze all significant magic strings, hardcoded literals, and string-based domain values throughout the codebase.

The goal is to produce a structured refactoring plan that replaces appropriate string literals with strongly typed enums and centralized constants where applicable.

No implementation is permitted during this task.

Implementation will only occur after explicit approval from the user.

---

## Background

The codebase has evolved over time and likely contains many hardcoded string values such as:

```csharp
"Holy"
"Fire"
"Ice"
"Burn"
"Stun"
"Heal"
"Smite"
"Critical"
"Miss"
"Human"
"Elf"
"Knight"
"Druid"
```

These values create several risks:

- Typographical errors
- Duplicate definitions
- Inconsistent naming
- Difficult refactoring
- Weak compile-time validation
- Reduced discoverability
- Hidden coupling between systems

---

## Important Rule

This task is strictly:

```text
Analysis
Planning
Validation
Risk Assessment
```

This task must NOT:

- Change production code
- Introduce enums
- Remove literals
- Rename values
- Modify tests

No code changes are allowed.

---

# Phase 1 - Discovery

## Repository Scan

Perform a repository-wide analysis to identify:

- string literals
- switch statement string comparisons
- if/else string comparisons
- hardcoded identifiers
- serialization values
- combat event names
- spell names
- status effect names
- race names
- class names
- equipment categories
- damage types
- sound identifiers
- animation identifiers
- UI event identifiers

---

## Exclusions

Do NOT include:

- log messages
- user-facing text
- narrative descriptions
- combat flavor text
- exception messages
- markdown content
- documentation

Unless those values are also used for logic.

---

# Phase 2 - Classification

Group discovered literals into logical categories.

Potential categories include:

---

## Damage Types

Examples:

```text
Fire
Ice
Holy
Shadow
Arcane
Poison
Bludgeoning
Piercing
Slashing
```

Potential Candidate:

```csharp
DamageType
```

---

## Status Effects

Examples:

```text
Burn
Freeze
Stun
Leech
Poison
Silence
```

Potential Candidate:

```csharp
StatusEffectType
```

---

## Character Classes

Examples:

```text
Knight
Paladin
Druid
Priest
Mage
Fighter
```

Potential Candidate:

```csharp
CharacterClass
```

---

## Races

Examples:

```text
Human
Elf
Dwarf
Halfling
```

Potential Candidate:

```csharp
RaceType
```

---

## Spell Schools

Examples:

Values discovered from:

```text
dark-orb/design/dark-orb-master-spellbook.md
```

Potential Candidate:

```csharp
SpellSchool
```

---

## Combat Results

Examples:

```text
Hit
Miss
Critical
Fumble
```

Potential Candidate:

```csharp
CombatResultType
```

---

## Additional Categories

Identify any additional categories that logically qualify for enum conversion.

---

# Phase 3 - Suitability Analysis

For each category determine:

### Safe For Enum

Strongly typed values that:

- have finite valid values
- are domain concepts
- rarely change
- are used in logic

---

### Should Remain String

Values that:

- come from database content
- are user editable
- are localization targets
- are externally controlled
- are serialization contracts

---

### Should Become Constants

Values that:

- are shared identifiers
- are not true domain types
- should not be enums

Example:

```csharp
public static class SoundIds
```

---

# Phase 4 - Risk Assessment

Identify areas where enum conversion may introduce risk.

Examples:

- Database persistence
- Existing seed data
- JSON serialization
- Replay system
- Save game compatibility
- API contracts
- Configuration files

---

## Required Output

For each candidate category provide:

### Current Usage Count

Example:

```text
Damage Types
Occurrences: 143
Files: 22
```

---

### Recommended Refactor

Example:

```text
Introduce DamageType enum
```

---

### Risk Level

```text
Low
Medium
High
```

---

### Migration Complexity

```text
Small
Medium
Large
```

---

# Phase 5 - Proposed Execution Plan

Generate a future implementation plan.

The plan must include:

---

## Stage 1

Lowest risk enum migrations.

---

## Stage 2

Medium complexity migrations.

---

## Stage 3

High impact migrations.

---

## Stage 4

Serialization and persistence validation.

---

## Stage 5

Cleanup and dead code removal.

---

# Testing Strategy (Planning Only)

The implementation plan must define required tests.

---

## Unit Tests

Verify:

- Enum mapping
- Parsing
- Conversion logic

---

## Integration Tests

Verify:

- Database persistence
- API compatibility
- Combat execution

---

## Replay Validation Tests

Verify:

- Existing replay JSON files remain valid
- Existing combat logs remain valid
- Deterministic outcomes remain unchanged

---

## Regression Tests

Verify:

- Spell casting
- Combat resolution
- Status effects
- Equipment handling
- Character loading

---

# Deliverables

## Discovery Report

List:

- all candidate magic strings
- occurrence counts
- categories

---

## Refactoring Proposal

List:

- recommended enums
- recommended constants
- values that should remain strings

---

## Risk Assessment

List:

- migration risks
- serialization concerns
- persistence concerns

---

## Execution Plan

Produce a phased implementation plan.

No code changes allowed.

---

# Explicit Constraint

At the conclusion of this task:

```text
NO CODE SHALL BE MODIFIED
```

The output must be:

```text
Analysis
Recommendations
Migration Plan
Testing Plan
Risk Assessment
```

Only.

Implementation requires a separate user-approved task.

---

## Acceptance Criteria

- [ ] Repository scanned for magic strings
- [ ] Logical categories identified
- [ ] Enum candidates identified
- [ ] Constant candidates identified
- [ ] Unsafe conversions documented
- [ ] Risk assessment completed
- [ ] Testing strategy defined
- [ ] Migration phases proposed
- [ ] No code modified
- [ ] Awaiting explicit user approval before execution