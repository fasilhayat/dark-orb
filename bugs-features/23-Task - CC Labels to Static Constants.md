# Task — CC Label Strings to Static Constants

Project: Dark Orb

Priority: Low

Type: Refactoring

Status: Draft

---

## Objective

Replace the 7 hardcoded CC (Crowd Control) label strings (`"stunned"`, `"rooted"`, `"feared"`, `"crowd-controlled"`) in `CharacterExtensions.cs` with typed constants.

---

## Current State

`CharacterExtensions.cs:39-55` returns lowercase CC labels as strings:

```csharp
if (character.IsStunned()) return "stunned";
if (character.IsRooted()) return "rooted";
if (character.IsFeared()) return "feared";
// ...
StatusEffectType.Stun => "stunned",
StatusEffectType.Root => "rooted",
StatusEffectType.Fear => "feared",
_ => "crowd-controlled",
```

These labels are placed on `CombatLogEntry.CcLabel` and used elsewhere for display. Since they're already mostly centralized (only in one file), this is a straightforward extraction.

---

## Proposed Solution

Add a static constants class:

```csharp
namespace BattleArena.Application.Services.Combat;

public static class CcLabels
{
    public const string Stunned = "stunned";
    public const string Rooted = "rooted";
    public const string Feared = "feared";
    public const string Default = "crowd-controlled";
}
```

Replace the 7 return statements in `CharacterExtensions.cs` with `CcLabels.X`.

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Application/Services/Combat/CcLabels.cs` | **New file** — static constants class |
| `BattleArena.Application/Services/Combat/CharacterExtensions.cs` | Replace 7 return literal strings with `CcLabels.X` |

---

## Acceptance Criteria

- [ ] `CcLabels` static class exists with all 4 CC label constants
- [ ] All return statements in `CharacterExtensions.GetCcLabel` use `CcLabels.X`
- [ ] No change to runtime behavior
- [ ] All 719 tests pass
