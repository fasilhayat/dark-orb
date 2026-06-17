# Task — Race/Class Name Comparisons to Enum-Driven

Project: Dark Orb

Priority: Low

Type: Refactoring

Status: Done

---

## Objective

Replace the 3 hardcoded race/class name string comparisons with the existing `RaceType` enum, eliminating brittle string matching.

---

## Current State

**`Character.cs:277-278`** — Race name comparisons in `AttackPower` calculation:

```csharp
var isElf = Race?.Name == "Elf";
var isHalfElf = Race?.Name == "Half-Elf";
```

Both `"Elf"` and `"Half-Elf"` are valid `RaceType` enum values (`RaceType.Elf`, `RaceType.HalfElf`). The `Race` entity already has a `RaceType` property — use that instead of string-comparing the display name.

**`CombatBalanceSteps.cs:78`** (acceptance test) — Class name comparison:

```csharp
var school = _character.ClassName == "Mage" ? SpellSchool.Stormcraft : SpellSchool.Deity;
```

---

## Proposed Solution

In `Character.cs`, replace string comparison with `RaceType`:

```csharp
// Before
var isElf = Race?.Name == "Elf";
var isHalfElf = Race?.Name == "Half-Elf";

// After
var isElf = Race?.Type == RaceType.Elf;
var isHalfElf = Race?.Type == RaceType.HalfElf;
```

This requires `Race.Type` to be exposed (verify it exists — `RaceType` enum is already defined in `Core.Entities.Enums`).

In the acceptance test, determine if the comparison can use a class-to-school mapping (or leave it since it's test code).

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Core/Entities/Character.cs` | Replace 2 `Race?.Name ==` with `Race?.Type == RaceType.X` |

---

## Acceptance Criteria

- [x] `RaceType` flat enum replaced by 10 subsectioned enums matching bestiary categories: `HumanoidType`, `BeastType`, `MonstrosityType`, `UndeadType`, `SpiritType`, `DemonType`, `ConstructType`, `DragonType`, `CelestialType`, `FeyType`
- [x] `Race` entity does not yet expose a typed property — `Character.cs` string comparisons deferred until `Race` is extended
- [x] All 719 tests pass
