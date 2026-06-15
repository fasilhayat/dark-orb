# Task — Hardcoded Hex Colors to Centralized Config

Project: Dark Orb

Priority: Low

Type: Refactoring

Status: Draft

---

## Objective

Centralize the ~100 hardcoded hex color strings scattered across 6+ presentation/GUI files into a single design-token config to eliminate duplication and enable consistent theming.

---

## Current State

Hex colors are duplicated across multiple files with overlapping values:

| File | Approx. count | Examples |
|------|---------------|----------|
| `CombatPlaybackEngine.cs` | 32 | `"#ff6600"`, `"#44ccff"`, `"#ffff44"`, overlay/effect colors |
| `AvaloniaCombatPresenter.cs` | 28 | Static brushes (`"#888"`, `"#fff"`, `"#00bfff"`), effect colors |
| `EffectVisualConfig.cs` | 10 | Status effect overlay colors |
| `MainWindowViewModel.cs` | 8 | Effect bar colors |
| `CcVisualConfig.cs` | 6 | CC label colors |
| `TransferEffectRegistry.cs` | 6 | Leech transfer colors |

Many values are duplicated: `"#ff6600"` (Burning) appears in at least 3 files, `"#44ccff"` (Ice) appears in at least 3 files.

---

## Proposed Solution

Create a `DesignColors` static class in `BattleArena.Presentation` that serves as the single source for all color values:

```csharp
namespace BattleArena.Presentation;

public static class DesignColors
{
    // Elemental colors
    public const string Fire = "#ff6600";
    public const string Ice = "#44ccff";
    public const string Lightning = "#ffff44";
    public const string Poison = "#44ff44";
    public const string Holy = "#ffffaa";
    public const string Shadow = "#cc44cc";
    public const string Acid = "#44ff44";
    public const string Arcane = "#cc88ff";

    // Status effect colors
    public const string Burning = "#ff6600";
    public const string Frozen = "#44ccff";
    public const string Shocked = "#ffff44";
    public const string Electrified = "#88ddff";
    public const string Poisoned = "#44ff44";
    public const string Bleeding = "#ff4444";
    public const string Confused = "#aaaaaa";
    public const string Charmed = "#ff88aa";

    // UI colors
    public const string White = "#fff";
    public const string Gray = "#888";
    public const string DarkGray = "#666";
    public const string Cyan = "#00bfff";
    public const string Yellow = "#d4a017";
    public const string Red = "#ff4444";
    public const string Green = "#44cc44";
    public const string Magenta = "#cc44cc";
    public const string Dim = "#555";

    // Combat overlay colors
    public const string CriticalHit = "#ffff00";
    public const string DevastatingStrike = "#ff00ff";
    public const string PerfectParry = "#00bfff";
    public const string TotalReversal = "#ff0000";
    public const string Fumble = "#ff6600";
    public const string Healed = "#44cc44";
    public const string Resurrection = "#ffffaa";
}
```

Then update all files to reference `DesignColors.X` instead of raw hex strings.

This task can be done incrementally — update one file at a time — but all files should eventually use the centralized config.

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Presentation/DesignColors.cs` | **New file** — all hex color constants |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Replace all hex strings with `DesignColors.X` |
| `BattleArena.Presentation/EffectVisualConfig.cs` | Replace hex strings with `DesignColors.X` |
| `BattleArena.Presentation/CcVisualConfig.cs` | Replace hex strings with `DesignColors.X` |
| `BattleArena.Presentation/TransferEffectRegistry.cs` | Replace hex strings with `DesignColors.X` |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | Replace hex strings with `DesignColors.X` |
| `BattleArena.Gui/ViewModels/MainWindowViewModel.cs` | Replace hex strings with `DesignColors.X` |

---

## Acceptance Criteria

- [ ] `DesignColors` class exists with all commonly-used hex colors
- [ ] All 6+ files reference `DesignColors.X` instead of raw hex strings
- [ ] No visual change in any overlay, effect bar, or UI element
- [ ] All 719 tests pass
