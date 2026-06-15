# Task — EventType to Typed Constants

Project: Dark Orb

Priority: High

Type: Refactoring / Technical Debt

Status: Draft

---

## Objective

Replace the plain `string` `EventType` property on `CombatLogEntry` with a typed constant pattern (`static class EventTypes`) to eliminate 287 string-literal references, provide compile-time safety, and serve as a single source of truth for all 33+ event type values.

---

## Current State

`EventType` is declared as `string` in `CombatLogEntry.cs:9`:

```csharp
public string EventType { get; set; } = string.Empty;
```

The only documentation is a stale comment on line 4 listing 7 values, while the codebase actually uses 33+ distinct event type strings across 4+ projects.

### Reference map (all 33+ values in use)

| Value | Used in |
|-------|---------|
| `"TurnStart"` | PlaybackEngine, Presenter, CombatLogWriter |
| `"TurnEnd"` | PlaybackEngine |
| `"TurnMeterGain"` | PlaybackEngine, Presenter, CombatLogWriter |
| `"Attack"` | PlaybackEngine, Presenter, CombatLogWriter, AttackResolver |
| `"Damage"` | PlaybackEngine, Presenter, CombatLogWriter |
| `"DamagePreview"` | PlaybackEngine, Presenter |
| `"HealPreview"` | PlaybackEngine, Presenter |
| `"Healed"` | PlaybackEngine, Presenter, SpellProcessor |
| `"DoTTick"` | Presenter, StatusEffectProcessor, CombatLogWriter |
| `"HoTTick"` | Presenter, StatusEffectProcessor |
| `"LeechTick"` | Presenter, StatusEffectProcessor |
| `"EffectApplied"` | Presenter, StatusEffectProcessor, CombatLogWriter |
| `"EffectResisted"` | Presenter, StatusEffectProcessor, CombatLogWriter |
| `"EffectExpired"` | Presenter, StatusEffectProcessor |
| `"EffectReflected"` | StatusEffectProcessor |
| `"FumblePenalty"` | Presenter, StatusEffectProcessor |
| `"PerfectParry"` | PlaybackEngine, Presenter, AttackResolver |
| `"DevastatingStrike"` | PlaybackEngine, Presenter, AttackResolver |
| `"TotalReversal"` | PlaybackEngine, Presenter |
| `"Clash"` | AttackResolver |
| `"Death"` | PlaybackEngine, Presenter |
| `"KnockedOut"` | PlaybackEngine, Presenter |
| `"Resurrection"` | PlaybackEngine |
| `"PetSummoned"` | PlaybackEngine, Presenter, SpellProcessor |
| `"PetExpired"` | PlaybackEngine, Presenter, StatusEffectProcessor |
| `"SkippedTurn"` | PlaybackEngine, Presenter |
| `"RoundStart"` | PlaybackEngine, Presenter, CombatSimulator |
| `"RoundEnd"` | PlaybackEngine, Presenter |
| `"ManaRegen"` | Presenter |
| `"ManaDeduct"` | Presenter, SpellProcessor |
| `"ApiCall"` | PlaybackEngine, Presenter |
| `"SpellQueued"` | SpellProcessor |
| `"SpellDisrupted"` | SpellProcessor |
| `"SpellLost"` | SpellProcessor |
| `"ConcentrationPass"` | SpellProcessor |
| `"SummonFailed"` | SpellProcessor |
| `"SummonPet"` | SpellProcessor |
| `"ClearPersistent"` | PlaybackEngine |
| `"DoTDamage"` | PlaybackEngine |
| `"IncredibleEvent"` | PlaybackEngine |
| `"Move"` | MainWindow |
| `"PerfectDodge"` | CombatSoundRegistry |

### Risks of current approach

- **No compile-time safety** — a typo like `"KnockOut"` vs `"KnockedOut"` creates a dead code path that's only detectable at runtime
- **No Intellisense** — developers must grep to discover valid values
- **No single source of truth** — 42 values with no canonical list
- **Fragile refactoring** — renaming an event type requires a project-wide text search
- **Hidden coupling** — consumers in 4 different projects all depend on the same string contracts

---

## Proposed Solution

Introduce a `static class EventTypes` (or equivalent `readonly record struct`) in `BattleArena.Application.Models`:

```csharp
namespace BattleArena.Application.Models;

public static class EventTypes
{
    public const string TurnStart = "TurnStart";
    public const string TurnEnd = "TurnEnd";
    public const string Attack = "Attack";
    // ... all 42 values
}
```

Then update `CombatLogEntry.EventType` to remain `string` (for serialization compatibility) but all comparisons use `EventTypes.X`:

```csharp
// Before
if (entry.EventType == "TurnStart")

// After
if (entry.EventType == EventTypes.TurnStart)
```

This approach:
- Keeps JSON serialization unchanged (no migration needed for replay files / DB data)
- Gives compile-time safety against typos
- Provides Intellisense discovery
- Centralizes the contract
- Requires zero changes to the serializer or any persisted data

### Alternative considered: enum

An enum would require a JSON serialization converter (e.g., `JsonStringEnumConverter`) and break persisted replay/DB data if values change. The const-string approach avoids all migration risk while still getting compile-time safety.

---

## Files to Modify

| File | Change |
|------|--------|
| `BattleArena.Application/Models/EventTypes.cs` | **New file** — define all 42+ constants |
| `BattleArena.Application/Models/CombatLogEntry.cs` | No change needed (stays `string`) |
| `BattleArena.Application/Services/Combat/*.cs` | Replace string literals with `EventTypes.X` (7 files) |
| `BattleArena.Application/Services/CombatLogger.cs` | Replace string literals with `EventTypes.X` |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Replace all string literals (~30 refs) |
| `BattleArena.Presentation/CombatSoundRegistry.cs` | Replace string literals (2 refs) |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | Replace all string literals (~30 refs) |
| `BattleArena.Gui/Views/MainWindow.axaml.cs` | Replace string literals (1 ref) |
| All unit test files | Replace string literals (~89 refs) |
| All acceptance test step files | Replace string literals |

---

## Acceptance Criteria

- [ ] `EventTypes` static class exists with all documented values
- [ ] Zero string-literals remain in switch/match/== comparisons for EventType (use `EventTypes.X`)
- [ ] All 719 tests pass
- [ ] All existing replay/log serialization remains compatible (no JSON format change)
- [ ] No behavioral change in combat simulation or playback
