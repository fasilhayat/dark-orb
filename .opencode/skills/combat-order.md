# BattleArena Turn Meter & Combat Order

This document describes the **actual** turn meter system implemented in BattleArena.
It is *not* a design doc — it reflects what the source code does.

---

## 1. Turn Meter Fundamentals

Every combatant has a **turn meter** (TM), an integer that starts at **0** and
accumulates each tick. Once TM ≥ 100, the combatant may act.

### 1.1 TM gain per tick

```
gain = TurnSpeed + DEX_mod + buffMod - armorPenalty
gain = Math.Max(1, gain)
```

| Component | Source | Formula |
|-----------|--------|---------|
| `TurnSpeed` | `Character.TurnSpeed` | Base stat on the character |
| `DEX_mod` | `Character.Dexterity` | `(Dexterity - 10) / 2` (standard D&D mod) |
| `buffMod` | Active status effects | Sum of `StatusEffect.TurnMeterModifier` |
| `armorPenalty` | Equipped armor | Sum of `Armor.TurnMeterPenalty` across: Head, Chest, Hands, Waist, Boots, Neck, Back |
| Minimum | — | Always at least **1** per tick |

#### Test examples

| TurnSpeed | DEX | Armor penalty | Buff mod | Result | Source |
|-----------|-----|--------------|----------|--------|--------|
| 10 | 14 (+2) | 3 (Chest 2 + Head 1) | +4 (Haste) | **13** | `ComputeGainPerTick` test |
| 1 | 6 (-2) | 10 (Chest) | 0 | **1** (min) | `ComputeGainPerTick` test |
| 10 | 14 (+2) | 0 | +5 | **17** | `Tick` test |

### 1.2 Action threshold

A combatant is **Ready** when `CurrentValue >= 100`:

```csharp
public bool IsReady => CurrentValue >= 100;
public bool HasDualAction => CurrentValue >= 200;
```

`HasDualAction` is not currently used for multi-action turns — the default
**TM cost per turn is 100** (see §2.4).

---

## 2. Tick Loop (CombatSimulator)

Each iteration of the main loop (one "tick") runs these phases:

### 2.0 Pre: Update seed tracking

```csharp
_dice.CurrentTick = tick;  // stamps API dice-log entries
```

### 2.1 Turn meter gain

Every living combatant gets `ComputeGainPerTick(character)` added to their TM.

```csharp
// TurnmeterService.Tick() — simplified
state.CurrentValue += ComputeGainPerTick(character);
```

A `TurnMeterGain` log entry is emitted for every combatant, but only entries
where `IsReady == true` or `IsActive == true` are shown in the combat log
(others are noise‑filtered).

### 2.2 Acting order

All **alive**, **Ready** (TM ≥ 100), **not crowd‑controlled** combatants are
sorted by TM descending:

```
var acting = states
    .Where(s => s.Character.IsAlive && s.Meter.IsReady && !IsCrowdControlled(s.Character))
    .OrderByDescending(s => s.Meter.CurrentValue)
    .ToList();
```

If no one is ready, the tick ends and the loop advances to the next tick
(TM continues to accumulate).

### 2.3 Crowd control (skipped turns)

If a combatant is **Ready** but **crowd‑controlled** (e.g., stunned, frozen,
feared), they get a `SkippedTurn` event instead of acting. Their status
effects are ticked (durations decremented).

### 2.4 Full turn sequence (per actor)

For each actor in the `acting` list:

1. **Resolve attack source** — weapon, spell, or unarmed strike.
   Spells are picked randomly from memorized spells.

2. **Set `IsActive = true`** — marks the character as currently acting.

3. **Compute TM cost** — `100` for weapon/unarmed attacks. For spells:
   ```
   reduction = INT_mod * 3 + Level * 1 + Equipment.TotalTurnMeterCostReduction
   cost = Math.Max(10, spell.TurnMeterCost - reduction)
   ```

4. **Select target** — via `ITargetSelector` (heroes use hero selector;
   enemies use enemy selector).

5. **Emit `TurnStart` log entry** — includes `AttackSourceName`, `TargetName`,
   `IsSpell`, `IsActive=true`.

6. **Process DoT tick** — damage‑over‑time effects apply now. If the actor
   dies from their own DoT, their turn ends immediately.

7. **Tick all status effects** — durations decremented; expired effects emit
   `EffectExpired` events.

8. **Resolve attack** — `CombatService.ResolveAttack()`:
   - Rolls 1d20 + AttackPower vs target's DefensePower
   - Hit/miss/critical/fumble determination
   - Damage calculation with mitigation
   - Emits `Attack` and `Damage` log entries

9. **On‑hit effects** — if the attack source is a spell, on‑hit status effects
   are applied (`TryApply` with resistance roll).

10. **Spell disruption** — melee hits on spellcasters have a **20% chance**
    to reduce the target's TM by up to 25.

11. **Death/KO check** — if HP ≤ -10 → Death; if -9 to 0 → KnockedOut.

12. **Fumble penalty** — on a fumble (natural 1), apply "Fumble Penalty"
    status effect: `AttackPowerModifier = -2` for 1 turn.

13. **End turn** — `IsActive = false`, apply TM cost:
    ```
    state.CurrentValue = Math.Max(0, state.CurrentValue - tmCost)
    ```
    Emit `TurnEnd` log entry.

---

## 3. Combat End

Combat ends when one party has no living members.

| Condition | Result |
|-----------|--------|
| All enemies killed/KO'd | Victory (hero party wins) |
| All heroes killed/KO'd | Defeat (enemy party wins) |
| `maxTicks` exhausted | Timeout — no winner declared, `MaxTicksReached = true` |

---

## 4. Initialization

- `Party.Solo(character, attackSource)` — 1v1 duel
- `Party.HeroParty("name", members[])` — NvN party combat
- All combatants start with **TM = 0**
- No special first‑turn ordering — everyone accumulates TM from tick 1
- First actor to reach 100 acts first

### Pre‑seed (demo only)

The demo calls `PreSeedTurnMeters(states)` before playback starts, which
pre‑fills the display state so the first tick's TM bar animation shows
proper initial values rather than all zeros.

---

## 5. Logged Event Types (TM‑related)

| EventType | When emitted |
|-----------|-------------|
| `TurnMeterGain` | Every tick, every living combatant |
| `TurnStart` | Actor begins their turn |
| `TurnEnd` | Actor finishes their turn |
| `SkippedTurn` | Ready but CC'd — cannot act |
| `SpellDisrupted` | Melee hit reduces caster's TM (20% chance, up to −25) |

---

## 6. Summary

```
   ┌─────────────────────────────────────────────────────┐
   │  TICK LOOP (repeat until combat ends)               │
   │                                                     │
   │  1. All alive → TM += gain (TurnSpeed + DEX mod     │
   │     + buffs - armor penalty, min 1)                 │
   │                                                     │
   │  2. Ready actors (TM ≥ 100, not CC) sorted by TM    │
   │     ↓                                               │
   │  3. For each:                                       │
   │     a. Resolve weapon/spell                         │
   │     b. Select target                                │
   │     c. Roll 1d20 + AP vs DP                         │
   │     d. Apply damage + on-hit effects                │
   │     e. Check death/KO                               │
   │     f. TM -= cost (default 100)                     │
   │                                                     │
   │  4. CC'd ready actors → SkippedTurn                 │
   │                                                     │
   │  5. Check party wipe → end                          │
   └─────────────────────────────────────────────────────┘
```
