# BattleArena Combat System Specification (Turn-Based + Turnmeter Model)

## 1. System Overview

This system defines a deterministic, turn-based tactical combat model inspired by classic CRPG mechanics (AD&D-like structure) but fully normalized for modern simulation consistency.

Core design goals:
- Deterministic outcomes from defined probability rules
- Fully data-driven combat resolution
- Single unified stat resolution model (no dual THAC0-style offsets)
- Scalable modifier system (items, spells, racial traits, buffs, curses)
- Turnmeter-based initiative system enabling variable action frequency
- Level must be a first-class scaling axis (attack, defense, damage, HP)

---

## 2. Core Combat Philosophy

All combat interactions resolve through:

```
d20 + AttackPower ≥ DefensePower
```

No mechanic should bypass this system unless explicitly defined as a rule exception.

---

## 3. Primary Combat Equation

```
Hit if: d20 + AttackPower ≥ DefensePower
Natural 1 → automatic miss + fumble
Natural 20 → automatic hit + critical
```

---

## 4. Derived Stats Model

### AttackPower

> **Modern D&D model — values are used directly. There is no `20 - X` subtraction.**

```
ClassAccuracyBase = StrikeRating           ← higher SR = better attacker
LevelScaling      = Level / 2
AttributeModifier = (STR or DEX - 10) / 2   (depends on weapon type; INT for spells)
WeaponAttackBonus = source.AttackBonus
SkillModifiers    = sum(feat.AttackBonus)
BuffModifiers     = stacked status effect bonuses
RacialModifiers   = sum(race.feat.AttackBonus)
ItemSetBonuses    = 0 (reserved)

AttackPower = ClassAccuracyBase + LevelScaling + AttributeModifier
            + WeaponAttackBonus + SkillModifiers + BuffModifiers
            + RacialModifiers + ItemSetBonuses
```

### DefensePower

> **Modern D&D model — ArmorClass value is used directly. There is no `20 - AC` subtraction.**

```
EffectiveAC       = TotalArmorClass        ← higher AC value = more defensive
DexterityModifier = min((DEX - 10) / 2, maxDexterityBonus)
ShieldBonus       = shield.DefenseBonus
DefensiveBuffs    = stacked status effect bonuses / penalties
RacialModifiers   = sum(race.feat.DefenseBonus) + sum(character.feat.DefenseBonus)
ItemSetBonuses    = 0 (reserved)
LevelDefenseBonus = Level

DefensePower = EffectiveAC + DexterityModifier + ShieldBonus
             + DefensiveBuffs + RacialModifiers + ItemSetBonuses
             + LevelDefenseBonus
```

### Key Property: LevelDefenseBonus = Level

`LevelDefenseBonus` was changed from `Level / 2` to `Level` to make level a meaningful defensive axis:

| Level | LevelDefenseBonus (old) | LevelDefenseBonus (new) |
|-------|------------------------|------------------------|
| 1     | 0                      | 1                      |
| 4     | 2                      | 4                      |
| 9     | 4                      | 9                      |

**Impact:** In a Level 9 vs Level 4 fight, the higher-level defender gains +5 net defense (L9 gets +9, L4 attacker gets +4 LevelScaling). Without this change, they only gained +2 net defense (L9 +4 vs L4 +4 = 0).

---

## 5. StrikeRating Integration

> **Higher StrikeRating = better attacker.** `ClassAccuracyBase = StrikeRating` (direct, no conversion).
> This is the modern system. The old THAC0 formula (`20 - StrikeRating`) has been retired.

| StrikeRating | ClassAccuracyBase | Typical Class |
|-------------|-------------------|---------------|
| 19          | 19                | Fighter       |
| 17          | 17                | Ranger        |
| 15          | 15                | Cleric        |
| 13          | 13                | Thief         |
| 10          | 10                | Mage          |

Higher StrikeRating = higher ClassAccuracyBase = more likely to hit. Fighters are the most accurate combat class; Mages are the least.

`EffectiveStrikeRating` (leveling service) adds a class-archetype gain on level-up:
- Martial: +1 per 2 levels (cap +6)
- Hybrid: +1 per 3 levels (cap +6)
- Caster: +1 per 4 levels (cap +6)

"SR improved" means `EffectiveStrikeRating` went **up** on level-up.

---

## 6. Armor Class Model

> **Higher ArmorClass value = more defensive.** `EffectiveAC = TotalArmorClass` (direct, no conversion).
> This is the modern system. The old THAC0 formula (`20 - AC` where lower was better) has been retired.

```
EffectiveAC = TotalArmorClass
```

A character wearing Plate Armor (AC 18) has `EffectiveAC 18` — the highest physical defense value in the game. Robes (AC 10) give `EffectiveAC 10`. Higher is always better.

Dexterity bonus is capped by the sum of `MaxDexterityBonus` across all worn armor pieces — heavy armor limits how much DEX helps.

### Canonical Armor Values (from `02-seed-data.sql`)

| Name | AC | Category | MaxDexBonus | Mitigation |
|------|----|----------|-------------|------------|
| Robes | 10 | Caster | 99 | 0 |
| Leather Armor | 11 | Light | 99 | 1 |
| Studded Leather | 12 | Light | 99 | 1 |
| Hide Armor | 12 | Medium | 2 | 2 |
| Scale Mail | 14 | Medium | 2 | 2 |
| Chain Mail | 16 | Heavy | 0 | 3 |
| Plate Armor | 18 | Heavy | 0 | 5 |

These values are the single source of truth. Tests must use `BattleArena.UnitTests.TestData.ArmorCatalog` to reference them — do not hard-code armor stats in test code.

---

## 7. Hit Points

### Base HP

`MaxHitPoints` is the base value stored in the database or seed data. It represents the character's HP at Level 1.

### EffectiveMaxHitPoints

```
EffectiveMaxHitPoints = MaxHitPoints + max(0, Level - 1) × HitPointsPerLevel
```

`HitPointsPerLevel` varies by class (Mage d6, Rogue/Bard d8, Priest/Druid d10, Fighter/Knight/Paladin d10, Barbarian d12). The average across all classes is roughly 6 + Stamina modifier per level.

### HP Scaling Examples

| Character | Class | Level | MaxHitPoints | HitPointsPerLevel | EffectiveMaxHP |
|-----------|-------|-------|-------------|-------------------|----------------|
| Marigold  | Priest | 9     | 48          | 6                 | 96             |
| Mira      | Priest | 4     | 34          | 6                 | 52             |
| Theron    | Thief | 5     | 50          | 6                 | 74             |
| Krag      | Orc   | 4     | 45          | 6                 | 63             |

### CurrentHitPoints

On DB load, `CurrentHitPoints` is initialized to `EffectiveMaxHitPoints` (characters start at full health). During combat, `CurrentHitPoints` decreases from damage and increases from healing.

Death thresholds:
- `CurrentHitPoints ≤ -10` → Death
- `-9 ≤ CurrentHitPoints ≤ 0` → KnockedOut

### Level-Up HP Gain

When a character gains a level, `CurrentHitPoints` increases by `HitPointsPerLevel × levelsGained`, matching the D&D rule that you gain HP when you level up. `EffectiveMaxHitPoints` increases automatically since it's computed from `Level`.

---

## 8. Damage Formula

```
BaseDamage   = WeaponDiceRoll + AttributeModifier + FlatDamageBonus + Level / 2
FinalDamage  = max(0, (int)(BaseDamage × TypeMultiplier) - ArmorMitigation + ElementalDamage)
```

Where:
- `WeaponDiceRoll` = sum of `DamageCount` rolls of `DamageDie`
- `AttributeModifier` = (STR or INT - 10) / 2
- `FlatDamageBonus` = source.FlatDamageBonus
- `Level / 2` = level-based damage scaling
- `TypeMultiplier` = 1.5 if defender is vulnerable to this damage type, else 1.0
- `ArmorMitigation` = sum of all equipped armor `Mitigation` values
- `ElementalDamage` = flat elemental bonus from source

### Critical Hits

On a natural 20: `BaseDamage` is doubled before type multiplier and mitigation.
```
CriticalFinalDamage = max(0, (int)((BaseDamage × 2) × TypeMultiplier) - ArmorMitigation + ElementalDamage)
```

### Damage Scaling by Level

| Level | Level / 2 | Example: 1d8+2 weapon, STR mod 0 |
|-------|-----------|-----------------------------------|
| 1     | 0         | avg 4.5 + 0 = 4.5                |
| 4     | 2         | avg 4.5 + 2 = 6.5                |
| 9     | 4         | avg 4.5 + 4 = 8.5                |
| 15    | 7         | avg 4.5 + 7 = 11.5               |

---

## 9. Turnmeter System

```
TurnMeter range: 0–100
TurnMeter gain per tick: max(1, TurnSpeed + dexMod + buffMod - armorPenalty)
At TurnMeter ≥ 100 → take turn
After turn → subtract 100
At TurnMeter ≥ 200 → dual action possible
```

### Current Status

Level does **not** affect turnmeter gain. This is a deliberate design choice: Dexterity and equipment are the primary axes for action frequency. A low-level character with high DEX can outpace a higher-level character with low DEX, but will lose due to lower damage, lower hit rate, and fewer HP.

### Turnmeter Gap Example: L9 Priest vs L4 Priest

| Character | Level | DEX | dexMod | TurnSpeed | TM/tick | Ticks per action |
|-----------|-------|-----|--------|-----------|---------|------------------|
| Marigold  | 9     | 10  | 0      | 10        | 10      | 10.0             |
| Mira      | 4     | 16  | 3      | 12        | 15      | 6.7              |

Mira gains 50% more TM per tick, giving her ~3 actions for every 2 of Marigold's. Despite this speed advantage, Marigold wins consistently due to the compounding level advantages in attack, defense, damage, and HP.

---

## 10. Action Economy

Each turn:
- 1 primary action (attack, cast spell, use item)
- Optional bonus action (if available)
- Movement phase (not yet simulated)

Spells may consume `TurnMeterCost` in addition to `ManaCost`, reducing the caster's remaining TM after casting.

---

## 11. Status Effects

Unified structure:
```
TriggerCondition
Duration
StackRule (Stack | HighestWins | NoStack)
Magnitude
ResolutionPriority
```

### Resistance System (Two-Phase Infliction)

1. Phase 1: `D100 > ApplicationChance` → quiet miss (no log event)
2. Phase 2: `D100 ≤ defender.ComputeResistance(effect.ResistanceType)` → `EffectResisted`
3. Otherwise: `Apply()` → `EffectApplied`

Resistance is capped at 95 (always at least 5% infliction chance).

### On-Hit Effect Targeting

Every on-hit status effect carries an `EffectTarget` enum (`Target` or `Caster`):
- `EffectTarget.Target` (default) → applied to the **defender** via `ProcessOnHitEffectsAsync`. Used for debuffs, stuns, DoTs.
- `EffectTarget.Caster` → applied to the **spell caster** via `ProcessSelfBuffsAsync`. Used for self-buffs, shields, wards.

This explicit targeting replaced an earlier buggy approach that blindly applied all on-hit effects to the caster. The two processing methods filter by target — no overlap, no ambiguity.

### Reflection

A defender with an active status effect that has `ReflectChance > 0` may redirect incoming on-hit effects (debuffs, damage-over-time) back to the original caster.

1. On a hit that carries on-hit effects, before applying each effect, the simulator checks `defender.ActiveStatusEffects` for any buff with `ReflectChance > 0`. The highest `ReflectChance` value is used.
2. Roll `D100` ≤ `ReflectChance` → the effect is redirected to the attacker and an `EffectReflected` event is logged.
3. Otherwise → the effect is applied to the defender normally.
4. Elemental DoTs (Burning, Chilled, Shocked, Poisoned) are **never** reflected — they always land on the original target.

Reflection is a property of `StatusEffect.ReflectChance` (int, 0–100, default 0). Any buff can grant it. This makes reflection composable — it can come from spells, racial traits, or equipment without changes to the modifier pipeline.

### Status Effect Categories

| Type | Example | Behavior |
|------|---------|----------|
| Buff | Bless | Positive stat modifier, fixed duration |
| Debuff | Weakened | Negative stat modifier, fixed duration |
| DamageOverTime | Burning | Deals DoTDamage each tick |
| Root | Rooted | Prevents movement, may cause SkippedTurn |

---

## 12. Stacking Rules

- Flat bonuses stack additively
- Percentage modifiers apply last (not yet implemented)
- Same-source effects do not stack unless `StackRule.Stack`
- `HighestWins`: only the highest magnitude applies per category
- `NoStack`: only the first application applies
- Negative effects sum regardless of rule (debuff stacking)

---

## 13. Level Gap Analysis

This section provides the mathematical derivation of why higher-level characters dominate lower-level ones.

### Scaling Factors Summary

| Factor | Formula | L9 vs L4 delta | Delta direction |
|--------|---------|----------------|-----------------|
| Attack | +Level | +5             | Higher-level hits more often |
| Defense | +Level | +5             | Higher-level is harder to hit |
| Damage | +Level / 2 | +2              | Higher-level hits harder |
| HP | +(Level-1) × 6 | +30      | Higher-level has more HP buffer |

All four factors compound **in the same direction** (higher-level benefits in all four).

---

### 13.1 Same-Level Mirror Match

Two identical characters at the same level:

```
Attacker LevelScaling  = N
Defender LevelDefense  = N
Net accuracy modifier  = 0   (Attack - Defense from level cancels out)
Damage per hit         = weapon + mods + N × 2
HP                     = MaxHP + (N-1) × 6
```

Hit rate is purely determined by gear, stats, class (StrikeRating), and d20. Level cancels out.

<table><tr>
<td width="60%"><img width="100%" alt="Balanced combat bell curve" src="diagrams/combat-distribution-bellcurve.svg"></td>
<td width="40%" valign="top">

**How to read this bell curve:** The x-axis is hit count out of 2000 attacks; the y-axis is probability density (area under the curve sums to 1). The neon green curve is a normal distribution centred at **mu=1205** (60.25% × 2000) with **sigma=21.9**. The gold dashed line marks the mean. The shaded bands show the 68%/95%/99.7% confidence intervals (±1/2/3 sigma). In a balanced mirror match, most runs land between 1183 and 1227 hits (±1 sigma, 68% of the time).

</td>
</tr></table>

---

### 13.2 Delta-Level Match (L9 vs L4)

#### Hit Rate

**Marigold (L9) attacks Mira (L4):**
```
AttackPower  = ClassAccuracyBase(6) + LevelScaling(9) + 0 + 2(weapon) = 17
DefensePower = EffectiveAC(15) + dexMod(0) + LevelDefense(4) = 19
d20 + 17 ≥ 19 → miss on 1 (nat 1 only; 1+17=18 < 19)
Hit chance: 95% (19/20, only natural 1 misses)
```

**Mira (L4) attacks Marigold (L9):**
```
AttackPower  = ClassAccuracyBase(4) + LevelScaling(4) + (-1) + 0(dagger) = 7
DefensePower = EffectiveAC(15) + dexMod(1) + LevelDefense(9) = 25
d20 + 7 ≥ 25 → need d20 ≥ 18
Hit chance: 15% (3/20: 18, 19, 20 + natural 20 always hits)
```

In this example, Marigold hits 95% of the time while Mira hits only 15% — a **6.3× hit rate advantage** from level alone.

#### Damage Per Hit

Using the test character stats:
```
Marigold:  1d8+2 longsword + Level/2(4) = avg 4.5 + 2 + 4 = avg 10.5
Mira:      1d4 dagger + Level/2(2) = avg 2.5 + 2 = avg 4.5
```

Against armor mitigation:
```
Marigold deals: avg 10.5 - 2 = avg 8.5 per hit
Mira deals:     avg 4.5 - 1 = avg 3.5 per hit
```

Marigold hits **2.4× harder** per landed hit.

#### HP Buffer

```
Marigold: 80 (base) + 8×6 (level) = 128 EffectiveMaxHP
Mira:     45 (base) + 3×6 (level) = 63 EffectiveMaxHP
Marigold has 2.0× more HP.
```

#### Expected Rounds to Kill

Accounting for both hit rate and damage:

| Attacker | Hit chance | Avg damage/hit | Avg dmg per attack | Opponent HP | Expected attacks to kill |
|----------|-----------|---------------|-------------------|-------------|------------------------|
| Marigold | 95% | 22.5 | 21.4 | 63 | 63/21.4 ≈ **2.9 attacks** |
| Mira | 15% | 9.5 | 1.4 | 128 | 128/1.4 ≈ **91.4 attacks** |

Even accounting for Mira's turnmeter advantage (50% more actions), the L9 dominates. The level gap is insurmountable — as intended.

---

### 13.3 Generic Delta Formula

For a Level `H` vs Level `L` (H > L), delta `d = H - L`:

| Metric | Higher-level advantage |
|--------|----------------------|
| AttackPower advantage | `+d` (higher hits more) |
| DefensePower advantage | `+d` (higher is harder to hit) |
| Net to-hit advantage | `+2d` (attack + defense compound) |
| Damage advantage per hit | `+2d` |
| HP advantage | `+d × HitPointsPerLevel` |

### 13.4 Level Gap Table

Assuming identical gear, stats, and class (delta from level only):

| Delta | Attack advantage | Defense advantage | Net to-hit shift | Damage bonus | HP gap |
|-------|-----------------|-------------------|-----------------|-------------|--------|
| 1     | +1              | +1                | +2              | +2          | +6     |
| 2     | +2              | +2                | +4              | +4          | +12    |
| 3     | +3              | +3                | +6              | +6          | +18    |
| 5     | +5              | +5                | +10             | +10         | +30    |
| 10    | +10             | +10               | +20             | +20         | +60    |

A delta of 5 shifts the hit probability by the equivalent of ±10 on the d20 roll. Combined with +10 damage and +30 HP, a 5-level gap is effectively unwinnable for the lower-level party under equal gear conditions.

Hit distribution for the four representative combat scenarios, showing hit-count probability density over 2000 attacks:

| Scenario | Parameters | AP | DP | P(hit) | mu | sigma |
|----------|-----------|----|----|--------|----|-------|
| Defensive (L1) | STR10 SR8 vs AC14 | 8 | 14 | 27.50% | 550 | 19.97 |
| Balanced (L2) | STR12 SR8 vs AC8 | 10 | 8 | 60.25% | 1205 | 21.9 |
| Attacker (L1) | STR14 SR10 vs AC5 | 12 | 5 | 76.50% | 1530 | 17.72 |
| High Level (L5) | STR18 SR17 vs AC10 | 23 | 10 | 87.75% | 1755 | 9.97 |

<table><tr>
<td width="60%"><img width="100%" alt="Hit distribution comparison" src="diagrams/combat-distribution-comparison.svg"></td>
<td width="40%" valign="top">

**How to read the comparison chart:** Each horizontal bar spans the full 99.7% range (mu ± 3 sigma). Darker inner bands = 68% CI, medium = 95% CI, lightest outer = 99.7% CI. The gold line marks the mean. As hit rate increases (top to bottom), the CI bands tighten — higher AP/DP ratios produce more predictable outcomes. The defensive scenario (27.5% hit rate) has the widest spread; the high-level scenario (87.75%) is the tightest.

</td>
</tr></table>

<br>

<table><tr>
<td width="60%"><img width="100%" alt="Defensive advantage bell curve" src="diagrams/combat-distribution-defensive.svg"></td>
<td width="40%" valign="top">

**Defensive:** Low AP (8) vs high DP (14) yields P(hit)=27.5%. The distribution is wide (sigma=19.97) — few hits land, with high variance. The left tail barely approaches zero; the right tail extends well past the mean.

</td>
</tr></table>

<br>

<table><tr>
<td width="60%"><img width="100%" alt="Attacker advantage bell curve" src="diagrams/combat-distribution-attacker.svg"></td>
<td width="40%" valign="top">

**Attacker:** High AP (12) vs low DP (5) yields P(hit)=76.5%. The curve shifts right and narrows (sigma=17.72). Most runs land 1512–1548 hits. The left tail is cut off earlier — fewer low-outlier runs.

</td>
</tr></table>

<br>

<table><tr>
<td width="60%"><img width="100%" alt="High level scaling bell curve" src="diagrams/combat-distribution-highlevel.svg"></td>
<td width="40%" valign="top">

**High level:** L5 stats (AP=23, DP=10) produce P(hit)=87.75% and the tightest spread (sigma=9.97). Higher levels compress the distribution around the mean — outcomes become deterministic. The 68% CI spans just 1745–1765 hits.

</td>
</tr></table>

---

## 14. Turnmeter Math

### Level vs Turnmeter

Level does not contribute to turnmeter gain. The TM system is purely a function of:
- `TurnSpeed` (base class/character attribute)
- `DexterityModifier` (DEX investment)
- `BuffModifiers` (status effects)
- `ArmorPenalty` (equipment weight)

### Implication

A Level 1 character with maximum DEX (19, mod +4) and high TurnSpeed (12) can outpace a Level 20 character with minimum DEX (3, mod -3) and low TurnSpeed (6):

```
L1 speedster: 12 + 4 = 16 TM/tick → 6.25 ticks/action
L20 tank:      6 - 3 =  3 TM/tick → 33.3 ticks/action
```

However, the L20 tank:
- Hits 19 more on attack (LevelScaling)
- Defends with +20 more defense (LevelDefenseBonus)
- Deals +10 more damage per hit (Level / 2)
- Has +114 more HP (19 × 6)

The level gap overwhelms any turnmeter advantage. This is by design.

---

## 15. Combat Simulation

### Simulator Architecture

```
CombatSimulator
├── ICombatService        (attack resolution, damage calculation)
├── ITurnmeterService     (TM gain per tick)
├── IStatusEffectService  (application, tick, expiry)
├── IDiceService          (random rolls)
```

All combat-log events use `CombatLogEntry` with an `EventType` string:

| EventType | Meaning |
|-----------|---------|
| TurnMeterGain | TM increased this tick |
| TurnStart | Actor begins their turn |
| Attack | Hit or miss resolved |
| Damage | HP reduced |
| SkippedTurn | CC'd actor cannot act |
| EffectApplied | Status effect landed |
| EffectResisted | Resistance roll blocked the effect |
| EffectExpired | Duration reached zero |
| EffectReflected | On-hit effect redirected back to attacker by reflective shield |
| DoTDamage | Damage-over-time tick |
| FumblePenalty | Fumble side-effect applied |
| Death | HP ≤ -10 |
| KnockedOut | HP in range -9 to 0 |
| PerfectParry | Defender deflects attack; gains TM bonus |
| Clash | Mutual weapon collision; both take reduced damage |
| DevastatingStrike | Triple-damage critical hit |
| TotalReversal | Fumble flipped; defender gains TM, attacker penalised harder |

### API Endpoint

`POST /v1/combat/simulate`
Accepts: `{ heroParty, enemyParty, maxTicks, heroTargetStrategy, enemyTargetStrategy }`
Returns: `CombatResult` with full event log and final state.

---

## 16. Defense Roll System

Every attack resolves one d20 for the attacker (`NatAttack`) and one d20 for the defender (`NatDefense`). The combination of these two rolls is evaluated against a 7-case priority matrix before the regular hit/miss check:

| Priority | Condition | Outcome |
|----------|-----------|---------|
| 1 | atk=1 AND def=20 | **TotalReversal** — attacker fumbles hard; defender gains 30 TM (melee: +10% if ranged) |
| 2 | atk=20 AND def=1 | **DevastatingStrike** — triple-damage critical hit |
| 3 | atk=20 AND def=20 | **Clash** — mutual weapon lock; both take 50% of each other's base damage |
| 4 | atk=1, def≠20 | **Fumble** — miss + fumble penalty; defender gains 20 TM |
| 5 | atk=20, def≠1,20 | **Critical Hit** — double base damage |
| 6 | def=20, atk=2–19 | **PerfectParry** — automatic miss; defender gains 20 TM |
| 7 | both 2–19 | **Normal Roll** — `d20 + AttackPower ≥ DefensePower` |

TM boost for PerfectParry / TotalReversal: base 20 (or 30 for TotalReversal) − 10 if range is `Ranged`, applied via `ComputeDefenderTmBoost`.

---

## 17. Combat Modifier Pipeline

Combat modifiers are pluggable `ICombatModifier` implementations registered at DI startup and executed by `CombatService.ResolveAttack` in `Priority` order before the hit check.

### Interface

```csharp
public interface ICombatModifier
{
    string      Name     { get; }   // for logs / diagnostics
    int         Priority { get; }   // lower = runs first
    CombatPhase Phase    { get; }   // which phase this participates in
    void Apply(CombatModifierContext ctx);
}
```

### Context

`CombatModifierContext` carries read-only inputs (`Attacker`, `Defender`, `Source`, `Range`, `BaseAttackPower`, `BaseDefensePower`) and mutable output deltas (`AttackPowerDelta`, `DefensePowerDelta`). Modifiers accumulate deltas; the caller applies them to the base stats.

### Priority Bands

| Band | Range | Purpose |
|------|-------|---------|
| Positional | 10 | Range penalties, flanking, elevation |
| Environmental | 20 | Weather, terrain, darkness |
| Item / Set | 30 | Set-bonus effects, unique item effects |

### Adding a New Modifier

1. Implement `ICombatModifier` in `BattleArena.Application/Modifiers/`.
2. Register via DI in `AddServices.cs` (or equivalent).
3. No changes to `CombatService` are needed.

---

## 18. Pets

Pets are independent actors:
- Own TurnMeter (separate track)
- Own actions (independent attack resolution)
- Can be targeted separately
- Use `MaxHitPoints` from pet definition (no level-based HP scaling — pets have fixed stats)

---

## 19. Constraints

- No hidden formulas
- No dual subtraction systems
- Deterministic resolution ordering
- All randomness limited to d20 + damage dice
- `BattleArena.Demo` must never compute combat outcomes (display only)
- `BattleArena.Core` must have no dependencies on other projects
- **Cyclomatic complexity ≤ 10 per method** (modified McCabe — each `&&`/`||` counts as +1). Extract private helpers rather than letting any method exceed this limit. Values of 11–12 are acceptable only where splitting would add parameters without reducing real complexity.

---

## 20. Design Intent

### Supported goals

- Tactical CRPG combat where level matters
- Item-heavy RPG systems with meaningful gear progression
- Deterministic simulation for debugging and balance tuning
- Expandable content systems (new races, classes, spells)

### Missing (future work)

1. **Multi-target and AoE** — area-of-effect damage, simultaneous death handling
2. **Healing** — healing spells and effects are not yet implemented in the simulator
3. **Status effect resolution priority** — ordering rules for simultaneous effect application
4. **Turnmeter level scaling** — currently Level has no effect on TM; if testing shows low-level speedsters dominate, add `Level / 4` to TM gain

### Balance Target

The system should produce these expected outcomes, verified by in-memory diagnostic tests:

| Matchup | Expected win rate (higher-level) | Observed hit rates |
|---------|----------------------------------|--------------------|
| Same level, same gear | 50% | 60–70% hit rate (both sides symmetric) |
| +1 level, same gear | ~65% | — |
| +2 levels, same gear | ~80% | — |
| +3 levels, same gear | ~90% | — |
| +5 levels, same gear | ≥95% | — |
| +10 levels, same gear | ≥99% | — |

**Verified (diagnostic tests, `CombatDiagnosticTests.cs`):** same-level mirror matches produce 60–70% hit rates with canonical seed armor, which is within the healthy range.

These targets assume no extreme gear/stat disparities. A Level 1 in plate armor with a legendary weapon may defeat a higher-level unarmed opponent — gear matters within the system.

---

## 21. Visual Consistency Rules

### 21.1 Effect Color Mapping

All visual representations of a status effect **must use the same color** across all channels:

| Channel | Description |
|---------|-------------|
| **Persistent Border** | Flashing border on the character card while the effect is active |
| **Inline Label** | Effect name text next to the character name |
| **Overlay Message** | Floating combat text (e.g. "BURNING", "LEECH") |

### 21.2 Canonical Color Sources

| Effect | Color | Hex | Source |
|--------|-------|:---:|--------|
| **Burning** | Orange | `#ff6600` | `GetPersistentColor()` switch |
| **Ignite** | Bright red-orange | `#ff4400` | `GetPersistentColor()` switch |
| **Shocked** | Yellow | `#ffff44` | `GetPersistentColor()` switch |
| **Frozen / Freeze** | Light blue | `#44ccff` | `GetPersistentColor()` switch |
| **Poisoned** | Green | `#44ff44` | `GetPersistentColor()` switch |
| **Bleeding** | Red | `#ff4444` | `GetPersistentColor()` switch |
| **Leech** | Red-orange | `#ff6644` | `TransferEffectRegistry.TransferColor` |
| **LeechMana** | Purple | `#cc44ff` | `TransferEffectRegistry.TransferColor` |

CC effects (Stun, Sleep, Fear, Petrify, Root) use `CcVisualConfig` as their single source of truth.

### 21.3 Source-of-Truth Hierarchy

1. **`CcVisualConfig`** — CC effects (Stun, Freeze, Sleep, etc.)
2. **`TransferEffectRegistry`** — Transfer effects (Leech, LeechMana)
3. **`GetPersistentColor()` switch** — Standard DoTs/debuffs (Burning, Shocked, Poisoned, etc.)

GUI label color (`GetEffectColor()`) **must delegate** to these sources rather than hardcoding independent values. This ensures border, label, and overlay messages are always consistent.

### 21.4 Persistent Effect Lifecycle

| Phase | Event | Visual Action | Presenter Method |
|-------|-------|---------------|------------------|
| **Start** | `EffectApplied` | Start border flicker or mana bar blink | `StartPersistentEffect()` |
| **Tick** | `DoTTick` / `LeechTick` | Flash border + overlay message | `FlashBorder()` + `AddOverlayMessage()` |
| **End** | `EffectExpired` | Stop flicker/blink, reset visual state | `RemovePersistentEffect()` |

The lifecycle completes properly only when both `EffectApplied` and `EffectExpired` visual events carry `IsPersistent = true`.

---

## 22. Quick Reference: Formula Cheat Sheet

```
To-hit:             d20 + AttackPower ≥ d20 + DefensePower   (both sides roll — modern opposed-roll model)
AttackPower:        StrikeRating + Level/2 + (STR/DEX/INT-10)/2 + weapon.AttackBonus + feats + buffs + race
DefensePower:       TotalArmorClass + min((DEX-10)/2, maxDex) + shield + buffs + race + Level
BaseDamage:         WeaponDiceRoll + (STR/INT-10)/2 + FlatBonus + Level / 2
FinalDamage:        max(0, BaseDamage × (1.5 if vulnerable) - Mitigation + Elemental)
CriticalDamage:     max(0, (BaseDamage × 2) × (1.5 if vulnerable) - Mitigation + Elemental)
EffectiveMaxHP:     MaxHitPoints + max(0, Level - 1) × HitPointsPerLevel
TM gain/tick:       max(1, TurnSpeed + (DEX-10)/2 + buffs - armorPenalty)
```
