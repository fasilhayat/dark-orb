# Task — Phase Strings to Static Constants

Project: Dark Orb

Priority: Medium

Type: Refactoring

Status: Draft

---

## Objective

Replace all 9 hardcoded UI phase string literals (`"MainMenu"`, `"Setup"`, `"Combat"`, `"SpellPreview"`, `"ApiMenu"`, `"CharCreation"`, `"Location"`, `"World"`, `"WorldMap"`) with a `static class Phases` to provide compile-time safety and a single source of truth.

---

## Current State

Phase strings are used in 30 assignment/comparison sites across 2 files:

- `MainWindowViewModel.cs:118` — `private string _phase = "MainMenu";`
- `MainWindowViewModel.cs:138-146` — 9 `IsXPhase => Phase == "X"` properties
- `MainWindow.axaml.cs:48` — `_previousPhase = "MainMenu";`
- `MainWindow.axaml.cs:18 assignment sites` — `_vm.Phase = "X"` or `_vm.Phase == "X"`

---

## Proposed Solution

Add a `static class Phases` in `BattleArena.Gui`:

```csharp
namespace BattleArena.Gui;

public static class Phases
{
    public const string MainMenu = "MainMenu";
    public const string Setup = "Setup";
    public const string Combat = "Combat";
    public const string SpellPreview = "SpellPreview";
    public const string ApiMenu = "ApiMenu";
    public const string CharCreation = "CharCreation";
    public const string Location = "Location";
    public const string World = "World";
    public const string WorldMap = "WorldMap";
}
```

Then update all 30 sites to reference `Phases.X` instead of string literals.

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Gui/Phases.cs` | **New file** — static constants class |
| `BattleArena.Gui/ViewModels/MainWindowViewModel.cs` | Replace 10 string literals (line 118 default + 9 `IsXPhase` properties) |
| `BattleArena.Gui/Views/MainWindow.axaml.cs` | Replace 19 string literals (18 assignments + 1 default) |

---

## Acceptance Criteria

- [ ] `Phases` static class exists with all 9 phase constants
- [ ] All `== "PhaseName"` and `= "PhaseName"` in the 2 affected files use `Phases.PhaseName`
- [ ] No change to runtime behavior (string values remain identical)
- [ ] All 719 tests pass
- [ ] No other files affected (phase strings are UI-local)
