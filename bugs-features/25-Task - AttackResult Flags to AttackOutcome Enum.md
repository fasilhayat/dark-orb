# Task — AttackResult Boolean Flags to AttackOutcome Enum

Project: Dark Orb

Priority: Medium

Type: Refactoring

Status: Draft

---

## Objective

Replace the 6 mutually-exclusive boolean flags on `AttackResult` (`IsCriticalHit`, `IsFumble`, `IsDevastatingStrike`, `IsClash`, `IsPerfectParry`, `IsTotalReversal`) with a single `AttackOutcome` enum. These flags are mutually exclusive by design — only one outcome per attack.

---

## Current State

`AttackResult.cs:12-23` — 6 independent boolean properties:

```csharp
public bool IsCriticalHit { get; set; }
public bool IsFumble { get; set; }
public bool IsDevastatingStrike { get; set; }
public bool IsClash { get; set; }
public bool IsPerfectParry { get; set; }
public bool IsTotalReversal { get; set; }
```

These are set in `CombatService.cs` and read in 10+ files totaling ~80+ usage sites. The flags are checked individually with `if (result.IsTotalReversal)`, `else if (result.IsDevastatingStrike)`, etc.

---

## Proposed Solution

Add an `AttackOutcome` enum in `BattleArena.Application.Models`:

```csharp
namespace BattleArena.Application.Models;

public enum AttackOutcome
{
    NormalHit,
    CriticalHit,
    Fumble,
    DevastatingStrike,
    Clash,
    PerfectParry,
    TotalReversal,
    Miss,
}
```

Replace the 6 bools with a single property:

```csharp
public AttackOutcome Outcome { get; set; } = AttackOutcome.NormalHit;
```

Update all consumers to use `result.Outcome switch { AttackOutcome.TotalReversal => ... }` or `if (result.Outcome == AttackOutcome.TotalReversal)`.

---

## Migration complexity: Large

This touches ~80+ sites across 12 production files and several test files:

| File | Impact |
|------|--------|
| `AttackResult.cs` | Replace 6 bools with 1 enum property |
| `CombatLogEntry.cs` | Replace 5 `bool?` mirror properties with `AttackOutcome?` |
| `CombatService.cs` | Replace `IsX = true` with `Outcome = AttackOutcome.X` |
| `CombatLogger.cs` | Replace flag checks with `switch` on `Outcome` |
| `CombatLogWriter.cs` | Replace flag checks with `switch` on `Outcome` |
| `AttackResolver.cs` | Replace `if (result.IsClash)` with `result.Outcome == AttackOutcome.Clash` |
| `StatusEffectProcessor.cs` | Replace `if (result.IsFumble)` / `IsTotalReversal` |
| `TurnMeterProcessor.cs` | Replace `result.IsPerfectParry` / `IsTotalReversal` |
| `LevelingService.cs` | Replace `e.IsCritical == true` |
| `AvaloniaCombatPresenter.cs` | Replace `e.IsFumble == true` |
| 5+ test files | Replace assertions on bools with assertions on enum |

---

## Acceptance Criteria

- [ ] `AttackOutcome` enum exists with all 9 values
- [ ] All 6 boolean flags removed from `AttackResult`
- [ ] All ~80 usage sites updated
- [ ] No behavioral change in combat resolution
- [ ] All 719 tests pass
