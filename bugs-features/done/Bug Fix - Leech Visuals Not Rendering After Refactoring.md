# Bug Fix - Leech Visuals Not Rendering After CombatSimulator Refactoring

Project: Dark Orb

Priority: High

Type: Bug

---

## Symptoms

After the CombatSimulator decomposition refactoring, leech effects (both HP and Mana) cause the GUI to stop rendering combat visuals. The combat engine continues running and the log file is written to disk, but no leech drain/gain animations, border flashes, overlay text, or log row updates appear in the GUI.

---

## Diagnosis

The refactored `StatusEffectProcessor.ProcessActorLeechAsync` introduced three defects that broke the visual rendering pipeline:

### Defect 1: Wrong EventType

| Code | Original | Refactored (broken) |
|------|----------|---------------------|
| HP Leech event | `"LeechTick"` | `"Leech"` |
| Mana Leech event | `"LeechTick"` | `"ManaLeech"` |

The GUI's `CombatPlaybackEngine.EmitVisualEvents` only has a switch case for `"LeechTick"`. Both `"Leech"` and `"ManaLeech"` fell through silently — no visual events were published.

### Defect 2: Missing CombatLogEntry Fields

The original code populates these fields on every leech `CombatLogEntry`:
- `LeechAmount` — amount drained
- `LeechCasterName` — who receives the resource
- `LeechResourceType` — `"HP"` or `"Mana"`
- `LeechTargetAfter` — target's resource after drain
- `LeechCasterAfter` — caster's resource after gain
- `StatusEffectName` — the effect name (e.g., `"Leech"`)

The refactored code populated none of these, so `CombatDisplayState.ApplyEvent` and the GUI rendering had no data to work with.

### Defect 3: Wrong Resource Type String

The refactored HP leech branch checked `resourceType == "Health"` instead of `resourceType == "HP"`, so HP leech never matched the HP branch.

---

## Affected File

`src/BattleArena.Application/Services/Combat/StatusEffectProcessor.cs` — `ProcessActorLeechAsync` method

---

## Fix

Rewrote `ProcessActorLeechAsync` to match the original `CombatSimulator`'s exact event type and field population pattern:

```csharp
public async Task ProcessActorLeechAsync(
    int tick, CombatantState actorState, CombatantState casterState, 
    StatusEffect effect, Func<CombatLogEntry, Task> notify)
{
    if (effect.LeechPerTurn <= 0) return;
    
    var casterName = casterState.Character.Name;
    var resourceType = effect.LeechResourceType ?? "HP";
    var leechAmount = effect.LeechPerTurn;

    if (resourceType == "HP")
    {
        // ... HP drain with proper field population
    }
    else if (resourceType == "Mana")
    {
        // ... Mana drain with proper field population
    }
}
```

Key changes:
- EventType is `"LeechTick"` for both HP and Mana paths
- `LeechAmount`, `LeechCasterName`, `LeechResourceType`, `LeechTargetAfter`, `LeechCasterAfter`, `StatusEffectName` all populated
- HP branch uses `"HP"` not `"Health"`

---

## Acceptance Criteria

- [x] Leech events produce `EventType = "LeechTick"` (not `"Leech"` or `"ManaLeech"`)
- [x] HP leech CombatLogEntry contains `LeechAmount`, `LeechCasterName`, `LeechResourceType`, `LeechTargetAfter`, `LeechCasterAfter`, `StatusEffectName`
- [x] Mana leech CombatLogEntry contains the same fields
- [x] GUI `EmitVisualEvents` switch case for `"LeechTick"` is reached and publishes visual events
- [x] Border flash appears on leech target and caster
- [x] Overlay text ("HP LEECH" / "MANA LEECH") appears
- [x] Mana drain/gain bar animations play
- [x] Combat log row for leech is rendered
- [x] All 705 existing tests pass
- [x] Build succeeds with 0 errors
