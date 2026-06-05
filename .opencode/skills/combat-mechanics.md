---
name: combat-mechanics
description: Use when implementing or modifying combat mechanics — attack resolution, damage formula, turn meter, status effects, resistance, dice rolls, or combat logging. Also use when writing combat-related tests.
self-update: true
self-update-trigger: Any change to files under BattleArena.Core/Entities/, BattleArena.Application/Services/, BattleArena.Application/Models/, or BattleArena.Application/Interfaces/
self-update-action: After modifying combat mechanic code, update this file to reflect the current behaviour.
---

# Combat Mechanics

## Key Classes

| Service | File | Responsibility |
|---------|------|----------------|
| `CombatSimulator` | `BattleArena.Application/Services/CombatSimulator.cs` | Tick-by-tick combat loop driver |
| `CombatService` | `BattleArena.Application/Services/CombatService.cs` | Pure math: attack & damage resolution |
| `CombatStatsService` | `BattleArena.Application/Services/CombatStatsService.cs` | Computes AttackPower / DefensePower |
| `StatusEffectService` | `BattleArena.Application/Services/StatusEffectService.cs` | Effect lifecycle: apply, tick, expire, resist |
| `TurnmeterService` | `BattleArena.Application/Services/TurnmeterService.cs` | TM gain per tick, cost after action |
| `DiceService` | `BattleArena.Application/Services/DiceService.cs` | Seeded RNG for deterministic replay |
| `AutoActionDecisionSource` | `BattleArena.Application/Services/AutoActionDecisionSource.cs` | Default AI — picks fixed weapon, random spell, or unarmed |
| `ConsoleActionDecisionSource` | `BattleArena.Demo/ConsoleActionDecisionSource.cs` | Interactive console menu — user picks melee, spell, or move |

## Modifier Pipeline

The `ICombatModifier` pipeline allows plugging in combat adjustments without modifying `CombatService`.

| Phase | When it runs | Fields affected | Example |
|-------|-------------|-----------------|---------|
| `AttackRoll` | Before opposed d20 roll | `AttackPowerDelta`, `DefensePowerDelta` | `RangeModifier`, `TerrainModifier` |
| `DamageCalculation` | Inside `ResolveDamage`, after base damage | `DamageDelta`, `DamageMultiplier` | `DamageModifier` (protective buffs) |
| `Healing` | Inside `ResolveHealing`, after base heal | `HealingPowerDelta`, `HealingMultiplier` | `HealingModifier` (caster buffs, group heal penalty) |

Priority bands: **10** = base/range, **20** = environmental, **30** = item/set/spell-buff.
To add a modifier: implement `ICombatModifier` → register in DI → done.

## Interfaces

| Interface | File | Purpose |
|-----------|------|---------|
| `IActionDecisionSource` | `BattleArena.Application/Interfaces/IActionDecisionSource.cs` | Decides which attack source to use at turn start. Injected per-party; auto uses AI, interactive uses console menu. Returns `null` for Move/Skip. |
| `ITargetSelector` | `BattleArena.Application/Interfaces/ITargetSelector.cs` | Decides which enemy to target. Separate from action decision. |

## Combat Loop

```
for tick = 1 to maxTicks:
  1. TurnMeterGain — every living combatant gains TM
  2. Find ready actors (meter >= 100, not CC'd), sort by meter descending
  3. SkippedTurn — ready but CC'd actors skip
  4. For each acting combatant:
     a. IActionDecisionSource.ChooseAttackAsync → weapon / spell / unarmed / null(Move)
     b. SelectTarget → hero or enemy selector (skipped if Move)
     c. TurnStart (SkippedTurn if Move was chosen)
     d. If healing spell → ResolveHealing (skip attack), else continue below
     e. DoTTick — DoT damage on actor
     f. TickAll — decrement effect durations, remove expired
     g. ResolveAttack — d20 + AP vs DP (runs AttackRoll-phase modifiers)
     h. Damage — if hit, reduce HP (runs DamageCalculation-phase modifiers)
     i. OnHitEffects — spell status effects
     j. SelfBuffs — apply buff-type OnHitEffects to caster
     k. SpellDisruption — 20% melee hit on caster
     l. CheckDefeat — Death / KnockedOut
     m. FumblePenalty — natural 1
     n. TurnEnd — deduct TM cost
```

## Attack Resolution

Attack resolution uses an **opposed d20 roll** with a 7-case priority matrix evaluated before the normal hit check:

```
attackRoll  = d20
defenseRoll = d20
```

| Priority | Condition | Outcome |
|----------|-----------|---------|
| 1 | atk=1 AND def=20 | **TotalReversal** — miss, −4 AP penalty, defender +30 TM |
| 2 | atk=20 AND def=1 | **DevastatingStrike** — auto-hit, triple base damage |
| 3 | atk=20 AND def=20 | **Clash** — both take 50% of each other's base damage |
| 4 | atk=1, def≠20 | **Fumble** — miss, −2 AP penalty, defender +20 TM |
| 5 | atk=20, def≠1,20 | **Critical Hit** — auto-hit, double base damage |
| 6 | def=20, atk≠1 | **PerfectParry** — miss, defender +20 TM |
| 7 | both 2–19 | **Normal opposed roll** — hit iff `attackRoll + AP ≥ defenseRoll + DP` |

## Damage Formula

Before damage calculation, **`CombatPhase.DamageCalculation` modifiers** run.
They can set `DamageDelta` (flat) and `DamageMultiplier` (multiplicative).

```
BaseDamage   = diceRoll + abilityModifier + source.FlatDamageBonus + Level / 2
scaledBase   = isCritical ? BaseDamage × 2 : BaseDamage
scaledBase   = (int)(scaledBase × DamageMultiplier)            ← modifier pipeline
FinalDamage  = max(0, (int)(scaledBase × typeMultiplier) - mitigation + elementalDamage + DamageDelta)
```
DevastatingStrike uses `BaseDamage × 3` instead of `× 2`.

## Healing Formula

Before healing, **`CombatPhase.Healing` modifiers** run.
They can set `HealingPowerDelta` (flat) and `HealingMultiplier` (multiplicative).

```
BaseHeal   = diceRoll + INTmod + source.FlatDamageBonus + HealingPowerDelta
FinalHeal  = max(1, (int)(BaseHeal × HealingMultiplier))
```

Group heals (name contains "Mass") apply at 0.6× potency per target.

## Turn Meter

```
GainPerTick = max(1, TurnSpeed + DEXmod + levelBonus + buffs - armorPenalty)
AfterActionCost: meter -= 100 (weapons) or spell cost
```

`levelBonus` is `LevelProgression.TurnMeterLevelBonus(level, archetype)` — typically +0 to +3 depending on class archetype.

## Status Effect Two-Phase Roll

```
Phase 1 (ApplicationChance): Roll D100 > chance → quiet miss
Phase 2 (Resistance):        Roll D100 <= resistance → EffectResisted
                             Otherwise → EffectApplied
```

Resistance is capped at 95 (always ≥5% chance to land).

## Terrain System

`TerrainType` enum: `Plains`, `Desert`, `Mountain`, `Rocky`, `Icy`, `Forest`, `Jungle`, `Swamp`.

The `TerrainModifier` (band 20, AttackRoll phase) applies racial AP/DP adjustments:

| Race | Bonuses | Penalties |
|------|---------|-----------|
| Human | — (adaptable) | — |
| Elf | Forest +2 AP | Desert −1 AP, Swamp −1 AP |
| Dwarf | Mountain +2 DP, Rocky +1 DP | Swamp −1 DP |
| Lizard | Desert +1 AP/+1 DP, Swamp +1 AP/+1 DP | Icy −1 AP/−1 DP |
| Orc | Desert +1 AP, Mountain +1 AP | Forest −1 AP, Swamp −1 AP |
| Ogre | Mountain +2 AP, Rocky +1 DP | Forest −1 AP |
| Kobold | Desert +1 AP, Rocky +1 AP | Forest −1 AP |
| Gladefolk | Forest +1 AP/+1 DP, Jungle +1 AP | Desert −1 AP |
| Undead | Icy +1 AP | — |
| Demon | Desert +1 AP | — |

`SimulateAsync` accepts a `TerrainType` parameter (defaults to `Plains`).

## Event Types

| EventType | Meaning |
|-----------|---------|
| `TurnMeterGain` | TM increased |
| `TurnStart` | Actor begins turn |
| `Attack` | Hit/miss resolved |
| `Damage` | HP reduced |
| `DoTTick` | DoT damage applied |
| `SkippedTurn` | CC'd or voluntarily skipped turn |
| `Move` | Actor used Move action (stub — no range effect yet) |
| `EffectApplied` | Effect landed |
| `EffectResisted` | Resistance blocked |
| `EffectExpired` | Duration reached zero |
| `FumblePenalty` | Natural-1 penalty applied |
| `SpellDisrupted` | Melee hit on caster |
| `SpellQueued` | Began charging a spell |
| `SpellCharging` | Still charging (TM accumulating) |
| `SpellLost` | Concentration broken or CC'd while charging |
| `ConcentrationPass` | Maintained concentration after hit |
| `ManaDeduct` | Mana spent on spell |
| `ManaRegen` | Mana regenerated per tick |
| `InsufficientMana` | Not enough mana to cast |
| `Death` | HP ≤ -10 |
| `KnockedOut` | HP -9 to 0 |
| `TurnEnd` | Action complete |
| `PetSummoned` | Pet entered combat |
| `PetExpired` | Summon duration ended |
| `Healed` | HP restored by healing spell |
| `Clash` | Both combatants exchange glancing blows |

## Attack Outcome Distributions

Because attack resolution uses an **opposed d20 roll** (attacker d20 + AP vs defender d20 + DP), with special-rule overrides for natural 1s and 20s, the theoretical hit rate depends on both the AP/DP difference and the priority-ordered special outcomes.

### Special-outcome rates (fixed, independent of AP/DP)

| Outcome | Dice condition | Probability | Notes |
|---------|---------------|-------------|-------|
| TotalReversal | atk=1, def=20 | 1/400 (0.25 %) | Auto-miss, −4 AP penalty, +30 TM to defender |
| DevastatingStrike | atk=20, def=1 | 1/400 (0.25 %) | Auto-hit, triple base damage |
| PerfectParry (both-20) | atk=20, def=20 | 1/400 (0.25 %) | Auto-miss, +20 TM to defender |
| Fumble | atk=1, def≠20 | 19/400 (4.75 %) | Auto-miss, −2 AP penalty |
| Critical hit | atk=20, def∉{1,20} | 18/400 (4.50 %) | Auto-hit, double base damage |
| PerfectParry (def-20) | atk∉{1,20}, def=20 | 18/400 (4.50 %) | Auto-miss, +20 TM to defender |
| Normal opposed roll | atk∈[2,19], def∈[1,19] | 342/400 (85.50 %) | Hit iff d20 + AP ≥ d20 + DP |

### Overall hit probability (all outcomes)

```
P(hit) = (18 cases Critical + 1 case Devastating + NormalHits) / 400
```

Where `NormalHits` = number of (atk∈[2,19], def∈[1,19]) pairs where `atk + AP ≥ def + DP`.

### Measured distributions (verified by `CombatDistribution.feature`)

Each scenario runs **2000 attack resolutions** with live seeded dice and asserts ≥3σ statistical bounds.

The normal-distribution confidence intervals are computed as μ ± z·σ where σ = √(N·p·(1-p)) and z = 1 (68 %), 2 (95 %), 3 (99.7 %). The special-outcome rates (crit 4.5 %, fumble 4.75 %, PP 4.75 %) are constant across all AP/DP configurations and verified in the balanced scenario.

<img src="../../design/diagrams/combat-distribution-comparison.svg" alt="Hit distribution comparison across 4 combat scenarios" width="100%"/>
<img src="../../design/diagrams/combat-distribution-bellcurve.svg" alt="Balanced scenario bell curve" width="100%"/>
<img src="../../design/diagrams/combat-distribution-defensive.svg" alt="Defensive advantage bell curve" width="100%"/>
<img src="../../design/diagrams/combat-distribution-attacker.svg" alt="Attacker advantage bell curve" width="100%"/>
<img src="../../design/diagrams/combat-distribution-highlevel.svg" alt="High level bell curve" width="100%"/>

Each SVG embeds the full raw dataset (hit, z-score, PDF value for every x) in XML comments, making it machine-readable for automated re-analysis.

## Self-update instructions

When you modify any of the following files, **immediately** update this skill to match the new behaviour:

- `BattleArena.Core/Entities/` (Character, StatusEffect, Party, etc.)
- `BattleArena.Application/Services/` (CombatSimulator, CombatService, StatusEffectService, TurnmeterService, DiceService)
- `BattleArena.Application/Modifiers/` (any `ICombatModifier` implementation)
- `BattleArena.Application/Models/` (CombatLogEntry, CombatResult)
- `BattleArena.Application/Interfaces/` (new interfaces added)

**What to update**:

| Section | Source of truth | Key things to verify |
|---------|----------------|---------------------|
| Attack Resolution | `CombatService.ResolveAttack` | Priority matrix conditions & outcomes, opposed-roll formula |
| Damage Formula | `CombatService.ResolveDamage` | `BaseDamage` components (especially `Level * 2`), crit multiplier placement, DevastatingStrike ×3 |
| Turn Meter | `TurnmeterService.ComputeGainPerTick` | `max(1, TurnSpeed + DEXmod + buffs - armorPenalty)` |
| Status Effect Two-Phase Roll | `StatusEffectService.TryApply` | Phase 1 (app chance) before Phase 2 (resistance) |
| Event Types | All services above | Every `EventType` string emitted by the code must be listed |
| Healing Formula | `CombatService.ResolveHealing` | `ResolveHealing` dice + INT mod + flat bonus + modifier pipeline |
| Modifier Pipeline | `CombatService` modifier-loop in each phase | All `CombatPhase` values wired, priority ordering |
| Terrain System | `TerrainModifier.Apply` | Race-terrain lookup table, AttackRoll-phase wiring |

The `design/combat-design.md` file is the human-facing design spec — it should also be updated when you change formulas here, but this skill file is the **AI's source of truth** and must always match the code exactly.

---

## Test Dummy Reference Characters

Two dedicated test-dummy NPCs are defined in both `BattleArena.Demo/roster.json` and `BattleArena.Gui/Data/roster.json` with portraits at `Assets/Portraits/target-golem.png` and `Assets/Portraits/practice-dummy.png`.

### Target Golem — Equipped, self-sufficient combatant

Purpose: validates perfect parry, fumble, devastating strike, and the full spell/afterburn pipeline against a capable opponent that fights back.

| Stat | Value | Notes |
|------|-------|-------|
| Level | 10 | |
| Class | Fighter (id=8) | 2 attacks/turn, can dual-wield |
| Race | Human | No innate magic resistance |
| STR / DEX / STA / INT / WIS / CHA | 16 / 10 / 18 / 14 / 10 / 8 | |
| HP | 300 | Survives ~8–10 rounds vs typical attackers |
| Mana | 100 | Can cast 5 spells |
| StrikeRating | 14 | Moderate hit chance → enables parry/crit/fumble test coverage |
| TurnSpeed | 6 | Acts after most heroes (tests delayed responses) |
| Chest | Plate Armor | AC 18, Mitigation 5 |
| RightHand | Long Sword | 1d8 Slashing, AttackBonus +1, 1H |
| **Memorized spells** | | |
| Fireball | 3d6 Fire, L3 | ElementalType Fire → afterburn **Burning** (1d6/turn, 3 turns, 60 % app, resisted by Fire) |
| Ice Bolt | 2d8 Ice, L2 | ElementalType Ice → afterburn **Chilled** (1d4/turn, 2 turns, 50 % app, resisted by Cold) |
| Shock | 2d6 Lightning, L2 | ElementalType Lightning → afterburn **Shocked** (1d8/turn, 2 turns, 40 % app, resisted by Lightning) |
| Static Shock | 1d6 Lightning, L2 | onHit **Stun** (100 % app, 2 turns, resisted by Magic) + afterburn **Shocked** |
| Smite | 2d8 Holy, L2 | ElementalType Holy → no afterburn (Holy has no DoT) |

**What to test against this target:**
- Perfect Parry (both roll 20) — possible because golem attacks back with d20 + 14 + 1 + (STRmod) vs defender's d20 + DP
- Devastating Strike / TotalReversal / Fumble / Critical hit / Clash — all special outcomes are reachable
- Afterburn (Burning, Chilled, Shocked) application chance & tick damage
- Stun from Static Shock — resisted by Magic resistance
- Multiple resistance types in play: Magic (Stun), Fire (Burning), Cold (Chilled), Lightning (Shocked)

### Practice Dummy — Pure damage sponge

Purpose: absorbs any incoming damage (melee, ranged, spell) without fighting back. Use to measure raw DPS, status-effect application rates, and afterburn tick accumulation over a long combat.

| Stat | Value | Notes |
|------|-------|-------|
| Level | 10 | |
| Class | Fighter (id=8) | |
| Race | Human | No innate magic resistance |
| All stats | 10 | Flat, no ability modifiers |
| HP | 500 | Lasts 10+ rounds vs typical attackers |
| Mana | 0 | Cannot cast spells |
| StrikeRating | 1 | Cannot meaningfully attack (no weapon either) |
| TurnSpeed | 1 | Barely gains TM — mostly a stationary target |
| Chest | Studded Leather | AC 12, Mitigation 1 — low, so most damage gets through |
| RightHand | *none* | No melee weapon → cannot attack back |
| MemorizedSpells | *none* | No spellcasting |

**What to test against this target:**
- Raw damage output per round (melee, ranged, spell)
- Afterburn accumulation (stacking multiple Burning/Chilled/Shocked from different casters)
- Status-effect application rates (Stun, Fear, Root, Silence) over many trials
- Damage-over-time tick damage total over a full combat
- Knockout threshold (HP -9 to 0) and Death threshold (HP ≤ -10)
- Heal-over-time and external healing throughput
- Mana-based constraints (dummy has 0 mana — verifies no mana-drain edge case)

### GUI & demo availability

Both characters are registered in `BattleArena.Gui/PortraitResolver.cs` with existing portraits:

| Character | Portrait file | Selectable in |
|-----------|---------------|--------------|
| Target Golem | `Assets/Portraits/target-golem.png` | Demo offline PickFighter (duel), demo party combat (auto-enemy), GUI API-mode roster |
| Practice Dummy | `Assets/Portraits/practice-dummy.png` | Same as above |

In the GUI, `ToDisplayItems` filters to `PortraitResolver.HasPortrait(name)`, so registering them is sufficient for roster visibility. The demo shows all characters from the loaded roster (both heroes and enemies) with no portrait filter.
