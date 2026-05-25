# BattleArena Combat System Specification (Turn-Based + Turnmeter Model)

## 1. System Overview

This system defines a deterministic, turn-based tactical combat model inspired by classic CRPG mechanics (AD&D-like structure) but fully normalized for modern simulation consistency.

Core design goals:
- Deterministic outcomes from defined probability rules
- Fully data-driven combat resolution
- Single unified stat resolution model (no dual THAC0-style offsets)
- Scalable modifier system (items, spells, racial traits, buffs, curses)
- Turnmeter-based initiative system enabling variable action frequency
- Tactical grid-based combat (assumed, but not required for math layer)

---

## 2. Core Combat Philosophy

All combat interactions resolve through:

Attack Power vs Defense Power + d20 randomness

No mechanic should bypass this system unless explicitly defined as a rule exception.

---

## 3. Primary Combat Equation

Hit occurs if:

d20 + AttackPower ≥ DefensePower

---

## 4. Derived Stats Model

AttackPower:
ClassAccuracyBase
+ LevelScaling
+ AttributeModifier (STR or DEX depending on weapon type)
+ WeaponAttackBonus
+ SkillModifiers
+ Buffs
+ RacialModifiers
+ ItemSetBonuses

DefensePower:
ArmorClass
+ DexterityModifier
+ ShieldBonus
+ DefensiveBuffs
+ RacialModifiers
+ ItemSetBonuses

---

## 5. StrikeRating Integration

ClassAccuracyBase = inverse mapping of StrikeRating

Fighter/Knight: high accuracy
Mage: low accuracy

---

## 6. Armor Class Model

EffectiveAC = 20 - AC

DefensePower uses EffectiveAC + modifiers

---

## 7. Turnmeter System

TurnMeter range: 0–100

TurnMeter gain per tick:
BaseSpeed + Dex scaling + buffs - armor penalties

At TurnMeter ≥ 100 → take turn
After turn → subtract 100

At TurnMeter ≥ 200 → dual action possible

---

## 8. Action Economy

Each turn:
- 1 primary action
- optional bonus action
- movement phase

---

## 9. Damage System

Damage:
WeaponDice + AttributeModifier + FlatBonuses

FinalDamage:
BaseDamage × TypeMultiplier - ArmorMitigation + ElementalModifiers

---

## 10. Status Effects

Unified structure:
TriggerCondition
Duration
StackRule
Magnitude
ResolutionPriority

---

## 11. Stacking Rules

- Flat bonuses stack
- Percentage modifiers apply last
- Same-source effects do not stack unless specified
- Highest defensive buff wins per category

---

## 12. Example Combat

Fighter vs Orc:
AttackPower = 7
DefensePower = 13
d20 + 7 ≥ 13

---

## 13. Items Examples

Dragon’s Fury:
+AttackPower, +Fire damage, Burning effect

Binding Chains:
+DefensePower, -Dexterity, TurnMeter penalty

Ring of Shadows:
+DefensePower, stealth bonus

---

## 14. Pets

Pets are independent actors:
- Own TurnMeter
- Own actions
- Can be targeted separately

---

## 15. Constraints

- No hidden formulas
- No dual subtraction systems
- Deterministic resolution ordering
- All randomness limited to d20 + damage rolls

---

## 16. System Output

Each combat resolution must produce:
Hit/Miss
Damage breakdown
Status effects
Turnmeter changes
Triggered effects
State transitions

---

## 17. Design Intent

### Supported goals

- Tactical CRPG combat
- Item-heavy RPG systems
- Deterministic simulation
- Expandable content systems

### Missing part

#### 1. Critical Hit Logic

You need a deterministic rule layer on top of the hit check.

#### Recommended model

**Option A (clean and stable)**

- If natural `d20` roll == `20` → Critical Hit
- If natural `d20` roll == `1` → Critical Fumble

This is the simplest and most robust approach.

**Option B (scaling model for your system)**

If you want item/spell interaction:

- `CriticalScore = d20 + AttackPower - DefensePower`

Then:

- Crit if `≥ +10` over threshold
- Fumble if `≤ -10` under threshold

This is harder to balance and easier to break with modifiers.

#### 2. What a Critical Hit should do

You must define the effect, otherwise it is only cosmetic.

**Common CRPG rules**

Recommended outcomes:

- Damage × 2
- Max damage roll + bonus modifier
- Apply an extra status effect

**Example**

- `BaseDamage × 2`
- `OR +1 extra status effect roll`

Keep it deterministic.

#### 3. Fumble Logic

This is where most systems either become frustrating or interesting.

**Recommended simple version**

- Fumble occurs on natural roll = `1`

**Effects**

- Attack misses automatically
- Apply a self-debuff
- Lose TurnMeter bonus
- Small self-stun chance (`10–25%`)

**Example**

- Skip next minor action
- `-2 AttackPower` next turn

#### 4. Important critique

You are currently designing a system that is:

- mathematically structured (**good**)
- incomplete in failure states (**bad**)

Critical hits and fumbles are not flavor systems.

They are:

- variance injection mechanisms

Without them:

- combat becomes flat and predictable
- item progression loses emotional spikes
- boss fights feel statistically linear

With them:

- you introduce controlled chaos
- this is essential for RPG engagement curves

#### 5. Hard truth

Your current combat spec is functionally incomplete for production use without:

- crit logic
- fumble logic
- status resolution priority ordering
- resistance interaction rules

You’re at about `70–75%` of a full combat engine spec, not `95%`.

If you want, the next step is to upgrade this document into a production-grade deterministic combat spec, including:

- full event ordering
- crit/fumble integration into the Turnmeter system
- status stacking priority rules
- resistance + immunity resolution hierarchy
- edge-case handling for multi-hit AoE and simultaneous death triggers

That is the point where it becomes implementable without interpretation ambiguity.

Du har nået grænsen for chats med dataana