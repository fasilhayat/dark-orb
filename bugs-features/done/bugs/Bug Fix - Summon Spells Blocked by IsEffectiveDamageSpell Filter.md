# Bug Fix - Summon Spells Blocked by IsEffectiveDamageSpell Filter

Project: Dark Orb

Priority: High

Type: Bug

---

## Symptoms

Two `CombatSimulatorTests` failed with `Assert.Single() Failure: The collection was empty`:
- `Simulate_SummonSpell_EmitsPetEventsAndExpiresAtRoundEnd`
- `Simulate_SummonedPet_PrefersLastAttackerOfMaster`

No `PetSummoned` or pet `TurnStart` events appeared in the combat log — the pet was never summoned.

---

## Root Cause

The `IsEffectiveDamageSpell` filter was added as part of the AI tactics improvement. It filters out spells with `DamageCount <= 0` and no `OnHitEffects`. However, **summon spells** have `DamageCount = 0` (they don't deal direct damage) and typically don't have `OnHitEffects` — the summoning is handled via the `SummonedPet` property instead.

The filter incorrectly blocked all summon spells from the AI's spell selection, so the AI never cast them.

## Fix

**File:** `src/BattleArena.Application/Services/AutoActionDecisionSource.cs`

Added a check for `spell.SummonedPet is not null` before the zero-damage filter:

```csharp
private static bool IsEffectiveDamageSpell(Spell spell)
{
    if (spell.SummonedPet is not null)
        return true;
    if (spell.DamageCount <= 0 && spell.OnHitEffects.Count == 0)
        return false;
    return true;
}
```

## Acceptance Criteria

- [x] Summon spells pass through `IsEffectiveDamageSpell` filter
- [x] `Simulate_SummonSpell_EmitsPetEventsAndExpiresAtRoundEnd` passes
- [x] `Simulate_SummonedPet_PrefersLastAttackerOfMaster` passes
- [x] All 4 summon tests pass (previously 2 failed)
- [x] All 705 tests pass
- [x] Build succeeds with 0 errors
