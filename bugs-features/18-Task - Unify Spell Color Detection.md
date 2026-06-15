# Task — Unify Spell Color Detection

Project: Dark Orb

Priority: Medium

Type: Refactoring

Status: Draft

---

## Objective

Replace the duplicated, brittle string-matching spell/effect color detection logic with a single data-driven source.

---

## Current State

There are **two separate** spell-to-color mappings using completely different approaches:

### 1. `CombatPlaybackEngine.cs:306-326` — `SpellOverlayColor(string spellName)`

Uses `spellName.ToLowerInvariant().Contains(...)` chains:

```csharp
if (lower.Contains("fire") || lower.Contains("flame") || lower.Contains("burn") || lower.Contains("inferno"))
    return "#ff6600";
if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("freeze") || lower.Contains("cold"))
    return "#44ccff";
if (lower.Contains("shock") || lower.Contains("lightning") || lower.Contains("thunder") || lower.Contains("spark"))
    return "#ffff44";
// ... 5 more blocks
return "#ffffff";
```

This is called during playback to determine the overlay color for spell damage events.

### 2. `AvaloniaCombatPresenter.cs:846-859` — `EffectColor(string? effectName)`

Uses a switch on exact effect names:

```csharp
effectName switch
{
    "Burning" => "#ff6600",
    "Ignite" => "#ff4400",
    "Frozen" or "Freeze" => "#44ccff",
    "Shocked" => "#ffff44",
    "Electrified" => "#88ddff",
    "Poisoned" => "#44ff44",
    "Bleeding" => "#ff4444",
    "Confused" => "#aaaaaa",
    "Charmed" => "#ff88aa",
    _ => "#88ccff",
};
```

This is called during playback for `DoTTick`, `EffectApplied`, `EffectExpired` overlay rendering.

### Problems

1. **Duplicated logic** — changing a color requires editing two files
2. **Different matching strategies** — `Contains` substring match vs exact name switch — inconsistent behavior
3. **Gaps and overlaps** — `"fire"` in `SpellOverlayColor` maps to `#ff6600`, but `"Burning"` in `EffectColor` also maps to `#ff6600` — the duplicate is fragile
4. **Brittle** — adding a new spell name like `"Infernal Strike"` would match `"inferno"` via substring (accidental match); a new effect like `"Corroded"` would fall through to `#88ccff` default
5. **Not data-driven** — colors should come from elemental type or a config, not hardcoded spell name heuristics

---

## Proposed Solution

### Option A: Elemental-type driven (preferred)

Use the existing `ElementalType` enum on `Spell` to determine color:

```csharp
public static string ElementColor(ElementalType type) => type switch
{
    ElementalType.Fire => "#ff6600",
    ElementalType.Ice => "#44ccff",
    ElementalType.Lightning => "#ffff44",
    ElementalType.Poison => "#44ff44",
    ElementalType.Holy => "#ffffaa",
    ElementalType.Shadow => "#aa44aa",
    ElementalType.Acid => "#44ff44",
    _ => "#ffffff",
};
```

This replaces `SpellOverlayColor` entirely.

### Option B: Centralized config file

Add a `spell-colors.json` or extend `EffectVisualConfig` to map spell names / effect names / elemental types to colors. Load at startup, reference from one place.

### Option C: StatusEffect data-driven

Use the `StatusEffect.Name` as the key into a single `Dictionary<string, string>` in `EffectVisualConfig`. This is what `EffectColor` duplicates — move it out of the presenter and into a shared config class.

---

## Recommended approach

Combine A + C:
1. `SpellOverlayColor` → use `ElementalType` mapping (single method in `EffectVisualConfig` or a new `ColorConfig` helper)
2. `EffectColor` → move the name-to-color dictionary into `EffectVisualConfig` (which already exists in `BattleArena.Presentation`)
3. Both `CombatPlaybackEngine` and `AvaloniaCombatPresenter` reference the same config class

---

## Files to Modify

| File | Change |
|------|--------|
| `BattleArena.Presentation/EffectVisualConfig.cs` | Add `GetSpellOverlayColor(ElementalType)` method. Add `GetEffectColor(string effectName)` with existing mapping moved here |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Replace `SpellOverlayColor()` body with call to `EffectVisualConfig.GetSpellOverlayColor()` |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | Replace `EffectColor()` body with call to `EffectVisualConfig.GetEffectColor()`. Remove local method |
| `BattleArena.Gui/Views/MainWindow.axaml.cs` | If `SpellOverlayColor` is referenced here too, update to shared method |

---

## Acceptance Criteria

- [ ] Only one canonical source of spell/effect color mapping exists
- [ ] `SpellOverlayColor` and `EffectColor` both delegate to the shared config
- [ ] Color output is identical to current behavior for all existing spell names and effect names
- [ ] Adding a new effect requires updating only the config class, not the playback engine or presenter
- [ ] All 719 tests pass
- [ ] No visual regression in combat playback overlays
