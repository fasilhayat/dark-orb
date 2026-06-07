# Bug Fix - Leech Events Dropped After CombatSimulator Refactoring

Project: Dark Orb

Priority: High

Type: Bug

---

## Symptoms

Leech effects (both HP and Mana) cause the GUI to stop rendering combat visuals after the first leech tick. The combat engine continues running in the background, the log file is written to disk, but no leech drain/gain animations, border flashes, overlay text, or log row updates appear. The user perceives the combat as frozen.

---

## Root Causes

Two independent defects were introduced during the CombatSimulator decomposition refactoring:

### Defect 1: Wrong EventType and Missing Fields in `StatusEffectProcessor.ProcessActorLeechAsync`

#### Original (correct):
- EventType: `"LeechTick"` for both HP and Mana paths
- All fields populated: `LeechAmount`, `LeechCasterName`, `LeechResourceType`, `LeechTargetAfter`, `LeechCasterAfter`, `StatusEffectName`

#### Refactored (broken):
- HP leech used `EventType = "Leech"` 
- Mana leech used `EventType = "ManaLeech"`
- Critical fields missing from CombatLogEntry

The GUI's `CombatPlaybackEngine.EmitVisualEvents` switch only has a case for `"LeechTick"`, so both event types silently fell through.

#### Fix:
Rewrote `ProcessActorLeechAsync` to match the original's exact event type and field population.

---

### Defect 2: TurnStart Order in `CombatSimulatorRefactored.ProcessActingActorAsync`

#### Original (correct):
```csharp
// 1. Log TurnStart
notify(EventType = "TurnStart");
// 2. Process leech, DoT, HoT, TickAll
ProcessActorLeechAsync(...);
ProcessActorDoTAsync(...);
```

#### Refactored (broken):
```csharp
// 1. Process leech, DoT, HoT first
ProcessActorStatusEffectsAsync(...);
// 2. THEN log TurnStart
notify(EventType = "TurnStart");
```

Because leech events were logged BEFORE TurnStart, they landed outside the turn boundary. `PlayTurnBased` processes events in a `default` switch case that only captures events when `inTurn == true`. LeechTick events between turns were silently dropped, never reaching `EmitVisualEvents`, `EmitCombatSounds`, or `BuildLeechTickRow`.

This is the primary reason the visuals appeared to "stop" — the first leech event was never rendered, and subsequent events in the same position were also lost.

#### Fix:
Swapped the order so `TurnStart` is logged first, then status effects:

```csharp
// 1. Log TurnStart (must come first for PlayTurnBased to capture subsequent events)
notify(EventType = "TurnStart");
// 2. Process status effects (now captured inside the turn)
ProcessActorStatusEffectsAsync(...);
```

---

## Affected Files

| File | Defect |
|------|--------|
| `src/BattleArena.Application/Services/Combat/StatusEffectProcessor.cs` | Defect 1 (event type + fields) |
| `src/BattleArena.Application/Services/CombatSimulatorRefactored.cs` | Defect 2 (TurnStart order) |

---

## Verification

- Build: 0 errors
- Unit tests: 585/585 passed
- Acceptance tests: 120/120 passed
- Combat log shows leech events properly ordered after TurnStart
- Mana leech animations, border flash, overlay text, and log rows all render

---

## Note on PlayTurnBased default case

The `PlayTurnBased` engine intentionally drops events that occur outside a turn (`inTurn == false`). This is by design — events like `TurnMeterGain` and `ManaRegen` happen every tick and would flood the UI. With Defect 2 fixed, leech/DoT/HoT events now land inside the turn and are properly rendered.

---

## Acceptance Criteria

- [x] Leech events produce `EventType = "LeechTick"` for both HP and Mana
- [x] Leech CombatLogEntry contains `LeechAmount`, `LeechCasterName`, `LeechResourceType`, `LeechTargetAfter`, `LeechCasterAfter`, `StatusEffectName`
- [x] Leech events are logged after `TurnStart` in the combat log
- [x] PlayTurnBased captures leech events inside the turn (`inTurn == true`)
- [x] GUI renders leech border flash, overlay text, and log row
- [x] Mana leech drain/gain animations play
- [x] Build succeeds, all 705 tests pass
