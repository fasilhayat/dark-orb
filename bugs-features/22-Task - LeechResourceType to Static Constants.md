# Task — LeechResourceType to Static Constants

Project: Dark Orb

Priority: Low

Type: Refactoring

Status: Draft

---

## Objective

Replace the hardcoded `"HP"` and `"Mana"` string comparisons for `LeechResourceType` with typed constants across all 20+ usage sites.

---

## Current State

`LeechResourceType` is a `string?` property on `StatusEffect` (default `"HP"`) and `CombatLogEntry`. It is compared using `== "HP"` and `== "Mana"` across 7 files:

| File | Lines | Usage |
|------|-------|-------|
| `StatusEffectProcessor.cs` | 39, 42, 59, 68, 86, 106, 109, 126, 135 | Default and comparison |
| `CombatLogWriter.cs` | 275, 276 | Display formatting |
| `AutoActionDecisionSource.cs` | 123, 125 | AI scoring |
| `TurnMeterProcessor.cs` | 41 | Turn meter logic |
| `RosterLoader.cs` | 352 | Default value |
| `CombatPlaybackEngine.cs` | 712, 714 | Visual event emission |
| `AvaloniaCombatPresenter.cs` | 1037 | Effect visual config lookup |
| `CombatDisplayState.cs` | 139, 141, 147, 149 | State tracking |

---

## Proposed Solution

Add constants near the `LeechResourceType` property definition, or in a shared location:

```csharp
public static class LeechResources
{
    public const string Hp = "HP";
    public const string Mana = "Mana";
}
```

Then replace all `== "HP"` and `== "Mana"` as well as default value `"HP"` with `LeechResources.Hp` / `LeechResources.Mana`.

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Core/Entities/LeechResources.cs` | **New file** — static constants |
| `BattleArena.Application/Services/Combat/StatusEffectProcessor.cs` | Replace 9 string literals |
| `BattleArena.Application/Services/CombatLogWriter.cs` | Replace 2 string literals |
| `BattleArena.Application/Services/AutoActionDecisionSource.cs` | Replace 2 string literals |
| `BattleArena.Application/Services/Combat/TurnMeterProcessor.cs` | Replace 1 string literal |
| `BattleArena.Application/Services/RosterLoader.cs` | Replace 1 default value |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Replace 2 string literals |
| `BattleArena.Presentation/CombatDisplayState.cs` | Replace 4 string literals |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | Replace 1 string literal |

---

## Acceptance Criteria

- [ ] `LeechResources` static class exists with `Hp` and `Mana` constants
- [ ] All `== "HP"` and `== "Mana"` comparisons use `LeechResources.Hp` / `LeechResources.Mana`
- [ ] Default value `= "HP"` uses `LeechResources.Hp`
- [ ] No change to runtime behavior
- [ ] All 719 tests pass
