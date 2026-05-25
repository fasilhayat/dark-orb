# Instructions

## What you already did right

Your core combat identity is structurally coherent:

- You chose a consistent resolution model: `d20 + strMod + attackBonus >= StrikeRating - AC`
- You established that:
  - Lower AC (Armor Class) is better
  - StrikeRating is the attacker difficulty baseline
  - Equipment modifies both offense and defense
  - Status effects exist across races, spells, and gear

This is compatible with a Gold Box-style deterministic system.

You also solved one of the biggest problems many systems fail at:

- You unified weapons, armor, spells, pets, and races under the same numeric ecosystem.

That is a strong foundation because it makes the system data-driven.

## The first major problem

You do **not** yet define the execution semantics.

You have stats, but not strict rules for:

- When modifiers apply: pre-roll or post-roll
- Whether effects stack or override
- Whether bonuses are bounded or unbounded
- Whether "Advantage" exists or is converted into numeric form
- Whether crits or fumbles exist
- Whether damage types interact with AC or resistances
- Whether StrikeRating is static per class or dynamic per level

Right now your system is effectively:

- *"the human interprets intent at runtime"*

That is fine for lore, but it is fatal for a combat engine.

## Second major problem: inconsistent modeling

You are mixing three incompatible design paradigms:

1. **AD&D-style abstractions**
   - THAC0-like StrikeRating
   - Classes with Hit Dice
   - Equipment tiers

2. **Modern RPG modifiers**
   - Advantage on saving throws
   - Conditional immunities
   - Status effects with stacking penalties

3. **MMO-style item systems**
   - Set bonuses
   - Percentage resistances
   - Multi-layer passive effects

These systems do **not** naturally compose.

You are currently assuming they do, but they do not.

### Example failure case

- Elf has "Magic Resistance"
- Ring gives "Spell cost reduction"
- Spell applies "Fear"
- Demon applies "Cause Fear immunity"

You have not defined:

- Precedence rules
- Resolution order
- Conflict resolution behavior

That means the system will eventually produce ambiguous outcomes.

## Third major problem: missing combat engine core

You have content, but not the engine loop.

A working system requires this minimum contract:

1. **Turn model**
   - Initiative formula
   - Tie-breaking rules
   - Turn phases
   - Reaction actions

2. **Event system**
   - `AttackAttempt`
   - `HitResolved`
   - `DamageApplied`
   - `StatusApplied`
   - `DeathEvent`

3. **Effect lifecycle rules**
   - Duration model: turn-based or tick-based
   - Refresh rules
   - Stacking rules
   - Dispel rules

Without this, the implementation will turn into spaghetti logic in C#.

## Fourth major problem: data is not normalized for code

Your Markdown is readable for humans, but not safely compilable into a system.

### Example

"Cause Fear" is used as:

- A spell
- A demon ability
- An undead ability

But there is no single canonical `EffectDefinition`.

That guarantees duplication bugs in code.

You need to normalize this into:

- Ability definitions
- Effect definitions
- Entity modifiers
- Item modifiers

Right now these are all mixed together.

## What your system actually is

You are not building only a game design.

You are building a **rules database** that still needs a deterministic simulation engine.

That engine is the real project.

Everything you wrote is currently just input data.

## What must be designed next

Before writing more lore or adding content, you must define:

1. **Combat resolution pipeline**
   A deterministic pipeline such as:

   - Initiative phase
   - Action selection
   - Target validation
   - Hit calculation
   - Effect resolution
   - Damage application
   - Death/state transition
   - Triggered events

   If you cannot write this as a deterministic pipeline, the system is not buildable yet.

2. **Single source of truth for modifiers**

   Every modifier must be one of:

   - Additive stat modifier
   - Multiplicative modifier
   - Boolean state
   - Triggered effect

   Right now those semantics are mixed freely, which will break your C# model.

3. **Unified effect system**

   Everything should collapse into:

   - `EffectId`
   - `TriggerCondition`
   - `Duration`
   - `StackRule`
   - `ResolutionOrder`

   If you do not unify this, your "cool items" will become unmaintainable logic branches.

## Hard verdict

Your design is strong as a world framework.

But as a combat system, it is incomplete in exactly the way that causes late-stage failure:

- You have content richness early
- You do not yet have execution rules
- You have mixed paradigms without normalization
- You have no formal simulation contract

This is the point where ambitious RPGs either:

- become engine rewrites every few months
- or get stabilized into a real system

---

## Combat maths

### Recommended model

Use:

- `SR = Attack baseline`
- `AC = Defense component`

Keep both concepts, but position them on different sides of the system.

#### Hit equation

`(d20 + SR + AttackModifiers) >= (AC + DefenseModifiers)`

#### Meaning of SR and AC

- `SR` = attacker capability
  - class + proficiency baseline
- `AC` = defender capability
  - armor + agility baseline

This is the cleanest mapping to your current design because:

- `SR` already exists as `StrikeRating`
- `AC` already exists as armor class

Nothing is lost; it is just normalized.

### What SR and AC mean in this model

#### StrikeRating (`SR`)

Becomes:

- `SR = ClassAccuracy + LevelScaling`

So:

- Fighter → low SR (better accuracy)
- Mage → high SR (worse accuracy)

> Note: in this model, lower SR is better if you invert it, or you convert SR into a bonus instead.

You therefore have two consistent interpretations:

- **Option A:** `SR` is a bonus
  - `AttackScore = BaseAttack + SR`
- **Option B:** `SR` is a difficulty value
  - This is old-school THAC0 style and **not recommended**

#### Armor Class (`AC`)

`DefenseScore = AC + DexModifier + Shield + Buffs`

So `AC` is not a second system; it is the core defensive stat.

### The key correction

You are currently trying to do this:

`d20 + SR + attack >= SR - AC`

That is structurally broken because:

- `SR` appears on both sides
- `AC` is subtractive instead of additive
- Scaling becomes nonlinear and unpredictable

### The unified interpretation

Think of `SR` and `AC` as labels, not mechanics:

- `SR` = how good you are at hitting
- `AC` = how hard you are to hit

Then collapse them into a single comparison:

- `Hit if: d20 + HitPower >= DefensePower`

Where:

- `HitPower = SR + weapon + stats + buffs`
- `DefensePower = AC + dex + armor + buffs`

### Why this is the correct model

If you insist on keeping `SR` and `AC` as separate interacting variables in subtraction form, you lock yourself into:

- confusing stacking rules
- hard-to-balance scaling curves
- inconsistent item power growth
- unpredictable PvE difficulty spikes

It may work for a prototype, but it will collapse under content growth.

### Hard truth

`SR` and `AC` are not two equations.

They are two contributors to a single inequality system.

If you want a system that survives:

- 100+ items
- 20+ status effects
- multi-target AoE
- pets
- cursed gear
- scaling enemies

then you must stop treating `SR` and `AC` as competing formulas.

They are just inputs into:

- `AttackPower`
- `DefensePower`
