# Feature Exploration - Tactical Movement System

Project: Dark Orb

File: `feature-movement-system-exploration.md`

Status: Analysis / Design Phase

Priority: Future Core Gameplay Feature

---

# Objective

Explore and design a movement system for Dark Orb that introduces tactical positioning, movement costs, terrain interaction, range management, and movement-affecting abilities while remaining compatible with the existing combat engine.

This task is exploratory and architectural.

No implementation is authorized.

The purpose is to establish the design, mechanics, balancing considerations, data model impacts, combat flow implications, AI requirements, and future extensibility before development begins.

---

# Background

Dark Orb currently focuses on:

- Turn Meter combat
- Melee attacks
- Spell casting
- Status effects
- Equipment
- Character progression

Combat currently assumes all participants are always within range.

There is no concept of:

- Position
- Distance
- Movement
- Terrain
- Line of sight
- Engagement zones

Adding movement would fundamentally change combat and introduce a tactical layer to gameplay.

---

# Design Goal

Movement should create meaningful decisions.

The player should have to decide:

- Move or attack?
- Close distance?
- Retreat?
- Kite enemies?
- Hold a choke point?
- Protect a caster?
- Break enemy formation?
- Escape dangerous effects?

Movement must become a tactical resource.

---

# Core Concept

Introduce:

```text
Movement Points (MV)
```

Separate from:

```text
Mana Points (MP)
Turn Meter (TM)
Health Points (HP)
```

Movement Points represent how far a character can move during combat.

---

# Recommended Direction - Distance Band Movement System

After evaluating the existing Dark Orb architecture, the preferred implementation is a **Distance Band Movement System** rather than a full tactical grid.

This recommendation should be treated as the default implementation path unless later analysis proves otherwise.

---

## Why Distance Bands Are Recommended

Dark Orb already has:

- Turn Meter combat
- Combat logs
- Replay files
- AI decision making
- Status effects
- Spell systems

A full grid-based movement system would require introducing:

- Pathfinding
- Tile occupancy
- Terrain calculations
- Line of sight
- Area-of-effect positioning
- Collision handling
- Formation logic
- Significantly more replay data

This would substantially increase implementation complexity across nearly every combat subsystem.

---

## Distance Band Model

Instead of exact coordinates, combatants exist within abstract combat ranges.

Example:

```text
Engaged
Near
Medium
Far
Distant
```

---

### Engaged

Melee range.

Characteristics:

- Melee attacks possible
- Touch spells possible
- Opportunity attacks (future feature)

---

### Near

Close battlefield distance.

Characteristics:

- One movement action away from engagement
- Short-range spells effective
- Thrown weapons effective

---

### Medium

Mid-range combat.

Characteristics:

- Most offensive spells effective
- Bows effective
- Crossbows effective

---

### Far

Long-range combat.

Characteristics:

- Long-range spells effective
- Bows effective

---

### Distant

Extreme range.

Characteristics:

- Few abilities reach this range
- Long-range preparation zone
- Enables kiting and retreat strategies

---

# Movement Point Calculation

Movement is derived from multiple sources.

---

## Base Racial Affinity

Every race receives a base movement value.

Example values:

| Race | Base MV |
|--------|---------|
| Human | 5 |
| Elf | 6 |
| Halfling | 6 |
| Dwarf | 4 |
| Half-Orc | 5 |

Actual balancing values to be determined.

---

## Armor Influence

Armor directly affects mobility.

### Light Armor

Examples:

- Cloth
- Leather
- Padded

Modifier:

```text
+0 to +2 MV
```

---

### Medium Armor

Examples:

- Chain
- Scale

Modifier:

```text
0 MV
```

---

### Heavy Armor

Examples:

- Plate
- Full Plate

Modifier:

```text
-1 to -3 MV
```

---

## Class Affinity

Certain classes are naturally more mobile.

### Ranger

```text
+2 MV
```

Reason:

- Wilderness mobility
- Skirmisher role

---

### Rogue

```text
+2 MV
```

Reason:

- Agile combat style
- Hit-and-run tactics

---

### Fighter

No bonus.

---

### Knight

Potential movement penalty due to heavy armor specialization.

---

### Mage

No inherent bonus.

---

### Priest / Druid / Paladin

Subject to balancing review.

May gain movement-related deity blessings in future systems.

---

## Dexterity Influence

Dexterity should affect movement.

Potential model:

```text
Every X DEX grants +1 MV
```

Requires balancing analysis.

---

## Magical Equipment

Magic items may alter movement.

Examples:

### Boots of Swiftness

```text
+2 MV
```

### Boots of Levitation

```text
Ignore terrain penalties
```

### Cursed Boots

```text
-2 MV
```

### Wings

```text
Flight movement
```

Future feature.

---

## Spell Influence

Movement can be modified by magic.

### Haste

```text
+50% movement
```

---

### Slow

```text
-50% movement
```

---

### Root

```text
0 movement
```

---

### Stun

```text
0 movement
```

---

### Web

```text
Movement cost doubled
```

---

# Range Interaction

Movement introduces attack ranges.

---

## Melee Weapons

Require:

```text
Engaged
```

---

## Polearms

Can attack:

```text
Near
```

Potential future feature.

---

## Bows

Effective at:

```text
Near
Medium
Far
```

---

## Spells

Spell-specific range restrictions.

Examples:

```text
Touch
Near
Medium
Far
Global
```

---

# Additional Status Effects Enabled By Movement

Movement enables new tactical mechanics.

---

## Fear

Target attempts to increase distance from threat.

Example:

```text
Engaged → Near
Near → Medium
Medium → Far
```

Restrictions:

- Cannot willingly move closer to feared source.

Combat Log Example:

```text
FEAR  Luna flees from Umbraex Cultist.
MOVE  Luna retreats from Near to Medium.
```

---

## Terror

Advanced version of Fear.

Behavior:

- Forced retreat
- Potential spell failure
- Reduced combat effectiveness

---

## Flee

Voluntary combat action.

Requirements:

```text
Reach Distant range
+
Pass escape check
```

Result:

```text
Combat ends
```

Future use cases:

- Dungeon escape
- Tactical retreat
- Encounter withdrawal

---

## Panic

Loss of control.

Behavior:

- Random movement decisions
- May retreat
- May approach danger
- May lose actions

---

## Root

Movement becomes:

```text
0
```

Target cannot move.

---

## Entangle

Movement cost increases.

Example:

```text
1 movement step = 2 movement points
```

---

## Slow

Movement reduced.

Examples:

```text
-50%
-2 MV
```

---

## Haste

Movement increased.

Examples:

```text
+50%
+2 MV
```

---

## Knockback

Forced movement away from attacker.

Examples:

```text
Engaged → Near
Near → Medium
```

---

## Pull

Forced movement toward attacker.

Examples:

```text
Far → Medium
Medium → Near
```

---

## Charge

Special movement attack.

Behavior:

- Move multiple bands
- Attack immediately
- Bonus damage possible

Ideal for:

- Knights
- Paladins
- Fighters

---

## Leap

Movement ignores restrictions.

Examples:

```text
Near → Engaged
Far → Near
```

Without intermediate movement.

---

## Dash

Consumes action for additional movement.

Ideal for:

- Rangers
- Rogues
- Scouts

---

# AI Impact Analysis

Movement significantly affects AI.

AI must learn:

- Pursuit
- Retreat
- Kiting
- Flanking
- Position management
- Threat avoidance
- Range optimization
- Escape evaluation

---

# Terrain System (Future)

Movement opens future terrain mechanics.

Examples:

### Road

Reduced movement cost.

---

### Forest

Increased movement cost.

---

### Swamp

Heavy movement penalty.

---

### Lava

Damage while moving.

---

### Ice

Sliding mechanics.

---

# Turn Meter Interaction

Movement must integrate with Turn Meter.

Investigate:

### Option A

Movement is free during turn.

---

### Option B

Movement consumes Turn Meter.

---

### Option C

Movement and actions consume Action Points.

Requires balancing analysis.

---

# Equipment Impact Analysis

Review all equipment categories.

Potential movement modifiers:

- Armor
- Boots
- Shields
- Weapons
- Rings
- Cloaks
- Artifacts

---

# Deity Interaction (Future)

Movement can be influenced by deity blessings.

Examples:

### Aethelion

Bonus movement when pursuing evil enemies.

---

### Lunara

Night-time movement bonuses.

---

### Celestara

Movement prediction and initiative bonuses.

---

### Astrara

Exploration and travel bonuses.

---

### Ignaroth

Aggressive charge bonuses.

---

### Umbraex

Shadow-step movement abilities.

---

### Veparix

Illusionary movement effects.

---

### Noctivane

Assassin mobility bonuses.

---

# Data Model Investigation

Potential additions:

### Character

```text
BaseMovement
CurrentMovement
MovementModifiers
```

---

### Equipment

```text
MovementModifier
```

---

### Status Effects

```text
MovementModifier
MovementMultiplier
MovementLock
ForcedMovement
```

---

### Spells

```text
RangeCategory
MovementEffects
ForcedMovementEffects
```

---

# Combat Log Requirements

Movement must be fully visible.

Examples:

```text
MOVE      Ranger advances from Far to Medium
MOVE      Knight charges from Near to Engaged
FEAR      Cultist flees from Paladin
PULL      Necromancer drags Rogue closer
KNOCKBACK Orc is forced from Engaged to Near
FLEE      Priest successfully escapes combat
```

---

# Replay System Requirements

Movement events must be serialized.

Required event types:

```text
Move
Fear
Flee
Charge
Leap
Pull
Knockback
Root
Slow
Haste
Dash
```

Replay determinism must be preserved.

---

# Balancing Investigation

Evaluate:

- Melee viability
- Ranged dominance
- Spell range balance
- Armor penalties
- Race mobility advantages
- Class mobility advantages
- Status effect impact
- Escape mechanics

Movement should add tactical depth without making melee classes obsolete.

---

# Deliverables

Produce:

1. Recommended movement model
2. Combat impact assessment
3. AI impact assessment
4. Data model proposal
5. Replay system impact analysis
6. Balance considerations
7. UI/UX proposal
8. Phased implementation roadmap

---

# Explicit Constraint

This task is:

```text
Analysis Only
```

No implementation.

No database changes.

No combat engine changes.

No UI changes.

---

# Acceptance Criteria

- [ ] Movement system options analyzed
- [ ] Distance-band approach evaluated and recommended
- [ ] Race mobility model proposed
- [ ] Armor mobility model proposed
- [ ] Class mobility model proposed
- [ ] Spell movement modifiers analyzed
- [ ] Equipment modifiers analyzed
- [ ] Deity influence opportunities documented
- [ ] Fear and flee systems documented
- [ ] Range system implications documented
- [ ] AI impact documented
- [ ] Replay impact documented
- [ ] Data model proposal created
- [ ] Recommended implementation path identified
- [ ] Awaiting user approval before development