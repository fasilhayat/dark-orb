# Feature - Tactical Movement Implementation

Project: Dark Orb

Status: Ready for Implementation (awaiting approval)

---

## Codebase Analysis — What Already Exists

The movement foundation is already partially wired:

| Component | Status | Location |
|-----------|--------|----------|
| `EngagementRange` enum | Exists: `Melee`, `Short`, `Long` | `Core/Entities/Enums/EngagementRange.cs` |
| `MoveIntent` class | Exists: `IAttackSource` with zero damage | `Application/Services/MoveIntent.cs` |
| `CombatantState.EngagementRange` | Field exists (defaults to `Melee`) | `Models/Combat/CombatantState.cs` |
| `Character.EffectiveMovementSpeed` | Exists: race + class + armor + buffs | `Core/Entities/Character.cs:65` |
| `Race.BaseMovementSpeed` | Exists (default 30) | `Core/Entities/Race.cs:8` |
| `RangeModifier` | Exists: -2 AP at melee for ranged, -1 DP otherwise | `Application/Modifiers/RangeModifier.cs` |
| Move handling in `TurnProcessor` | Exists: cycles Melee→Short→Long | `Services/Combat/TurnProcessor.cs:167` |

### Current Movement Flow

When a character chooses "Move" (via `MoveIntent`), the `TurnProcessor` cycles engagement:
```
Melee → Short → Long → Short → Melee → ...
```

Each move costs a full turn (consumes TM). Speed is computed but only displayed — not used for range calculation. After the move, the actor's `EngagementRange` is updated for subsequent attacks.

### Existing Data Model (Character)

```
EffectiveMovementSpeed = BaseMovementSpeed (race, default 30)
                       + Class.MovementBonus
                       - Equipment.TotalMovementPenalty
                       + ActiveStatusEffects.MovementModifier
```

---

## Gap Analysis — What's Missing for Full Tactical Movement

| Gap | Description | Impact |
|-----|-------------|--------|
| No MV resource | Movement consumes full turn (100 TM). No separate movement pool. | Can't move + attack in same turn |
| Single-band move | Move always cycles exactly one band | Can't move multiple bands per action |
| No range on weapons | `Weapon` has no `EffectiveRange` property | Any weapon can attack any range |
| No range on spells | `Spell` has no `RangeCategory` | All spells work at all ranges |
| No charge/leap/pull | Only basic move exists | No tactical depth |
| No terrain | No terrain system | Environment is flat/meaningless |
| Player movement not in AI | AI `ConsoleActionDecisionSource` offers Move, `AutoActionDecisionSource` doesn't | AI never repositions |

---

## Proposed Implementation Phases

### Phase 1 — Movement as Resource (instead of full-turn cost)

Replace the full-TM-cost move with a TM-based movement cost. A move consumes ~30-50 TM instead of 100, leaving the actor able to act afterwards.

**Changes:**
- `HandleNewAttackSetupAsync` in `TurnProcessor`: reduce move TM cost from 100 to `MoveTmCost` (e.g. 30)
- Add `MoveTmCost` constant or configurable value
- No new data model changes needed

**Risk:** Low. The flow exists, just changing the cost.

### Phase 2 — Multi-Band Movement

Allow moving more than one band per action by spending more TM.

**Changes:**
- Replace the single-band switch with distance-based movement
- A character with speed 30 moves 1 band, speed 60 moves 2 bands, etc.
- Or: cost to move scales by number of bands (e.g. 30 TM per band)

### Phase 3 — Weapon Range

Add an `EffectiveRange` property to `Weapon` and enforce it in `ResolveAttack`.

**Changes:**
- Add `EffectiveRange` enum to `Weapon` (Melee, Short, Medium, Far)
- Extend `EngagementRange` from 3 values to 5: `Melee`, `Short`, `Medium`, `Far`, `Distant`
- In `CombatService.ResolveAttack`, check if attacker's weapon can reach the current engagement range
- If out of range: attack automatically misses with a log entry
- Update `RangeModifier` to handle all 5 bands

### Phase 4 — Spell Range

Add a `RangeCategory` to `Spell` and enforce it during spell casting.

**Changes:**
- Add `RangeCategory` property to `Spell` (Touch, Near, Medium, Far, Global)
- Before resolving a spell attack, check `RangeCategory >= EngagementRange`
- Out-of-range spells fail with a log entry

### Phase 5 — Tactical Actions

Add charge, leap, pull, knockback, and dash as `IAttackSource` implementations.

**Changes:**
- New `ChargeIntent`, `LeapIntent`, `DashIntent` classes (like `MoveIntent`)
- Each has different TM costs and range effects
- Charge: move toward target + bonus damage
- Dash: double movement, costs more TM
- Pull/Knockback: forced movement on target

### Phase 6 — AI Movement

Update `AutoActionDecisionSource` to consider engagement range when choosing attacks.

**Changes:**
- If no enemies in weapon range, AI auto-selects Move first
- Add pursuit/retreat logic based on HP thresholds
- Avoid always-attacking behavior

---

## Files That Would Change (by Phase)

### Phase 1
- `Application/Services/Combat/TurnProcessor.cs` — reduce move TM cost
- `Application/Services/CombatSimulator.cs` — no change (thin orchestrator)

### Phase 3-4
- `Core/Entities/Enums/EngagementRange.cs` — add Medium, Far, Distant
- `Core/Entities/Weapon.cs` — add `EffectiveRange` property
- `Core/Entities/Spell.cs` — add `RangeCategory` property
- `Application/Modifiers/RangeModifier.cs` — handle 5 bands
- `Application/Services/CombatService.cs` — range validation in `ResolveAttack`
- Seed data / SQL for weapon ranges

### Phase 5
- `Application/Services/ChargeIntent.cs` (new)
- `Application/Services/LeapIntent.cs` (new)
- `Application/Services/DashIntent.cs` (new)
- `Application/Services/Combat/TurnProcessor.cs` — handle new intents

---

## What Would Stay the Same

- `ICombatService` interface — `ResolveAttack` already takes `EngagementRange`
- `CombatSimulator` — still a thin orchestrator, no movement logic
- `CombatLogEntry` — already has `EventType = "Move"`, add "Charge"/"Leap"/"Dash" as needed
- `Character` entity — `EffectiveMovementSpeed` already computed from race/class/armor/buffs
- `CombatantState` — already has `EngagementRange` field

---

## Acceptance Criteria

- [x] Movement costs TM (not full turn) — Phase 1
- [x] Multi-band movement possible — Phase 2
- [x] Root / immobilize prevents movement
- [ ] Weapon range enforced — Phase 3
- [ ] Spell range enforced — Phase 4
- [ ] Charge / Dash / Leap implemented — Phase 5
- [ ] AI repositions itself — Phase 6
- [x] All existing tests pass (regression)
- [x] Movement events appear in combat log
