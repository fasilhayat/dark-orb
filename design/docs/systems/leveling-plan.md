# BattleArena — Leveling Plan

> **Level cap, XP, class features, racial unlocks, and spell progression for levels 1–20.**
> Canonical source for all leveling data.

---

## Table of Contents

1. [XP & Leveling](#1-xp--leveling)
2. [Hit Points & Hit Dice](#2-hit-points--hit-dice)
3. [Strike Rating Progression](#3-strike-rating-progression)
4. [Turnmeter Level Bonus](#4-turnmeter-level-bonus)
5. [Accessory Slot Unlocks](#5-accessory-slot-unlocks)
6. [Spell Memorization](#6-spell-memorization)
7. [XP from Battles](#7-xp-from-battles)
8. [Class-by-Class Progression](#8-class-by-class-progression)
9. [Spell Progression Tables](#9-spell-progression-tables)
10. [Racial Benefits by Level](#10-racial-benefits-by-level)
11. [Pet Unlock Summary](#11-pet-unlock-summary)
12. [Level-Up Checklist](#12-level-up-checklist)

---

## 1. XP & Leveling

**Max level: 20** (D&D 5e standard).

Every level-up grants:

| Benefit | Detail |
|---------|--------|
| Hit Points | Roll HitDie + Stamina mod (max HD at L1) |
| Strike Rating | Per archetype table (§3) |
| Turnmeter Bonus | Per archetype table (§4) |
| Accessory Slots | Per archetype table (§5) |
| Class feature | See §8 below |
| Racial benefit | See §10 below (unlock levels only) |

### XP Thresholds

| Level | Total XP Required | XP to Next Level |
|:-----:|:-----------------:|:----------------:|
| 1 | 0 | 300 |
| 2 | 300 | 600 |
| 3 | 900 | 1,800 |
| 4 | 2,700 | 3,800 |
| 5 | 6,500 | 7,500 |
| 6 | 14,000 | 9,000 |
| 7 | 23,000 | 11,000 |
| 8 | 34,000 | 14,000 |
| 9 | 48,000 | 16,000 |
| 10 | 64,000 | 21,000 |
| 11 | 85,000 | 15,000 |
| 12 | 100,000 | 20,000 |
| 13 | 120,000 | 20,000 |
| 14 | 140,000 | 25,000 |
| 15 | 165,000 | 30,000 |
| 16 | 195,000 | 30,000 |
| 17 | 225,000 | 40,000 |
| 18 | 265,000 | 40,000 |
| 19 | 305,000 | 50,000 |
| 20 | 355,000 | — |

---

## 2. Hit Points & Hit Dice

Each class uses a specific **Hit Die** that determines how many HP a character gains per level.

| Class | Archetype | Hit Die | Sides | Avg per level |
|-------|:---------:|:-------:|:-----:|:-------------:|
| Barbarian | Martial | D12 | 12 | 6.5 |
| Fighter | Martial | D10 | 10 | 5.5 |
| Knight | Martial | D10 | 10 | 5.5 |
| Paladin | Martial | D10 | 10 | 5.5 |
| Mage | Caster | D6 | 6 | 3.5 |
| Priest | Caster | D10 | 10 | 5.5 |
| Druid | Caster | D10 | 10 | 5.5 |
| Rogue | Hybrid | D8 | 8 | 4.5 |
| Bard | Hybrid | D8 | 8 | 4.5 |

### Level 1 HP

At Level 1, a character receives the **maximum** value of their hit die plus their Stamina modifier:

```
Level 1 HP = max(HitDie) + StaminaModifier
```

Where `StaminaModifier = (Stamina - 10) / 2`.

### HP per level

On each subsequent level-up, the character **rolls** their hit die and adds their Stamina modifier:

```
HP Gain = Roll(HitDie) + StaminaModifier (minimum 1)
```

This gain is added to **both** MaxHitPoints and CurrentHitPoints.

Stamina (Constitution) is the primary defensive stat — a Fighter with 18 Stamina (+4 modifier) gains an average of 9.5 HP per level, compared to a Mage with 10 Stamina (+0) gaining 3.5.

### HP Examples

| Character | Class | Level | Stamina | Hit Die | Expected HP |
|-----------|-------|:-----:|:-------:|:-------:|:-----------:|
| Priest (avg) | D10 | 1 | 12 | D10 | 11 |
| Priest (avg) | D10 | 4 | 12 | D10 | 29 |
| Priest (avg) | D10 | 9 | 12 | D10 | 59 |
| Fighter (tough) | D10 | 5 | 18 | D10 | 52 |
| Mage (frail) | D6 | 10 | 10 | D6 | 33 |
| Barbarian (sturdy) | D12 | 8 | 18 | D12 | 86 |

### Stamina Modifier Table

| Stamina | Modifier |
|:-------:|:--------:|
| 3 | –3 |
| 4–5 | –2 |
| 6–8 | –1 |
| 9–12 | +0 |
| 13–15 | +1 |
| 16–17 | +2 |
| 18–19 | +3 |
| 20 | +4 |

---

## 3. Strike Rating Progression

A higher Strike Rating is better. The table below shows the cumulative bonus added to the class base SR.

| Level | Martial | Hybrid | Caster |
|:-----:|:-------:|:------:|:------:|
| 1 | +0 | +0 | +0 |
| 2 | +0 | +0 | +0 |
| 3 | +1 | +0 | +0 |
| 4 | +1 | +1 | +0 |
| 5 | +2 | +1 | +1 |
| 6 | +2 | +1 | +1 |
| 7 | +3 | +2 | +1 |
| 8 | +3 | +2 | +2 |
| 9 | +4 | +2 | +2 |
| 10 | +4 | +3 | +2 |
| 11 | +5 | +3 | +3 |
| 12 | +5 | +3 | +3 |

**Example:** A Level 10 Fighter (base SR 21) → effective SR = 21 + 4 = **25**.  
A Level 10 Mage (base SR 17) → effective SR = 17 + 2 = **19**.

---

## 4. Turnmeter Level Bonus

Level provides a small bonus to turnmeter gain per tick, scaled by archetype:

| Archetype | TM Bonus |
|:---------:|:--------:|
| Martial | +Level/3 |
| Hybrid | +Level/4 |
| Caster | +Level/5 |

This ensures higher-level characters act slightly more often, but Dexterity and TurnSpeed remain the primary axes for action frequency.

**Example:** A Level 9 Priest (Caster) gets +9/5 = +1 TM/tick, while a Level 4 Priest gets +4/5 = +0 TM/tick. The level gap provides a minor action frequency advantage.

---

## 5. Accessory Slot Unlocks

Accessory slots determine how many rings, amulets, and girdles a character can equip simultaneously.

| Level | Martial | Hybrid | Caster |
|:-----:|:-------:|:------:|:------:|
| 1 | 0 | 0 | 0 |
| 2 | 0 | 0 | 1 |
| 3 | 1 | 1 | 1 |
| 4 | 1 | 1 | 2 |
| 5 | 1 | 2 | 2 |
| 6 | 2 | 2 | 3 |
| 7 | 2 | 2 | 3 |
| 8 | 2 | 3 | 4 |
| 9 | 3 | 3 | 4 |
| 10 | 3 | 4 | 5 |
| 11 | 3 | 4 | 5 |
| 12 | 4 | 5 | 6 |

Casters attune to magical items more readily and unlock accessory slots earlier and in greater number. Martials gain fewer slots but compensate with superior weapon training and armor proficiency.

---

## 6. Spell Memorization

The number of spells a character can **memorize** (prepare) per day depends on their primary casting stat and class level.

| Class | Casting Stat | Formula | Bonus per level |
|-------|:------------:|---------|:---------------:|
| Mage | Intelligence | 2 + (Int − 10) / 2 + level / 3 | +1 every 3 levels |
| Priest | Wisdom | 2 + (Wis − 10) / 2 + level / 3 | +1 every 3 levels |
| Druid | Wisdom | 2 + (Wis − 10) / 2 + level / 3 | +1 every 3 levels |
| Bard | Charisma | 2 + (Cha − 10) / 2 + level / 3 | +1 every 3 levels |
| Paladin | Charisma | 1 + (Cha − 10) / 2 + level / 4 | +1 every 4 levels |

Minimum 1 slot at any level.

**Example:** A Priest with 16 Wisdom (+2 mod) at level 6:
- Base = 2 + 2 = 4
- Level bonus = 6 / 3 = 2
- Total = **6 prepared spells**

| Intelligence | INT Mod | Base Slots |
|:-----------:|:-------:|:----------:|
| 3 | –3 | 1 |
| 6–8 | –1 | 1 |
| 9–12 | +0 | 2 |
| 13–15 | +1 | 3 |
| 16–17 | +2 | 4 |
| 18–19 | +3 | 5 |
| 20 | +4 | 6 |

Equipment bonuses (e.g., Mage Robes, Arcane Circlets, Amulets of Wisdom) can add additional spell slots.

---

## 7. XP from Battles

After each battle, experience is awarded using the following formula:

```
Base XP   = sum(enemy levels) × 12
Net bonus = (party crits - party fumbles) × 8

Expected rounds = (party size + enemy size) × 2
Round ratio     = actual rounds ÷ expected rounds
Round factor    = 1.0 + |round ratio - 1.0| × 0.3   clamped to [0.5, 2.0]

Total XP = floor(base XP × round factor) + net bonus
XP per survivor = Total XP ÷ number of surviving party members (rounded down)
```

The **round factor** rewards both ends of the bell curve:
- Fights resolved much faster than expected (high efficiency) earn a bonus.
- Fights that drag on much longer than expected (grueling endurance) also earn a bonus.
- Standard-length fights earn base XP with no modifier.

Each **critical hit** landed by the party adds +8 XP to the pool before splitting.
Each **fumble** by the party subtracts -8 XP.

Only characters who are **alive** at the end of the battle receive XP. Unconscious or dead characters gain nothing.

**Example 1 (efficient):** A party of 3 heroes defeats 3 enemies of levels 5, 4, and 3 in 5 rounds.  
Base XP = (5 + 4 + 3) × 12 = 144  
Expected rounds = (3 + 3) × 2 = 12  
Round ratio = 5 ÷ 12 = 0.42 → factor = 1 + |0.42 - 1| × 0.3 = 1.17  
Total XP (no crits or fumbles) = 144 × 1.17 = 168  
All 3 survive → 168 ÷ 3 = **56 XP each** (efficiency bonus).

**Example 2 (grueling):** Same enemies but the fight takes 20 rounds with 2 party crits and 1 fumble.  
Base XP = 144  
Round ratio = 20 ÷ 12 = 1.67 → factor = 1 + |1.67 - 1| × 0.3 = 1.20  
Net bonus = (2 - 1) × 8 = +8  
Total XP = 144 × 1.20 + 8 = 180  
All 3 survive → 180 ÷ 3 = **60 XP each** (endurance bonus + net crit benefit).

---

## 8. Class-by-Class Progression

### 8.1 Barbarian (Martial, D12)

| Lvl | Features |
|:---:|----------|
| 1 | **Rage** (1/rest)* — +2 melee damage, take half damage from physical attacks. Lasts 3 + Stamina mod rounds.<br>**Unarmored Defense** — AC = 10 + Dex mod + Stamina mod when not wearing armor. |
| 2 | **Reckless Attack** — Attack with advantage; enemies get advantage against you until next turn.<br>**Danger Sense** — Advantage on reflex saves vs. traps/area effects. |
| 3 | **Rage** (2/rest). **Primal Path** — choose Berserker (bonus attack when raging) or Totem Warrior (spirit animal grants utility). |
| 4 | **Pet Companion** — gain a **Wolf** or **Hound** pet (see Pet Unlock §11).<br>**Rage** (3/rest). |
| 5 | **Extra Attack** — attack twice per action.<br>**Fast Movement** — +10 ft. base move. |
| 6 | **Rage** (4/rest). **Feral Instinct** — advantage on initiative; can enter Rage at the start of combat even when surprised. |
| 7 | **Brutal Critical** — add one extra weapon die on critical hits. |
| 8 | **Rage** (5/rest). **Primal Champion** — Strength and Stamina increase by +1 (max 20). |
| 9 | **Relentless Rage** — if reduced to 0 HP while raging, make DC 10 Stamina save to drop to 1 HP instead. DC increases by +5 each subsequent use. |
| 10 | **Rage** (6/rest). |
| 11 | **Brutal Critical** (2 dice) — two extra weapon dice on criticals. |
| 12 | **Eternal Warrior** — while raging, gain immunity to fear & charm. Rage has unlimited duration until combat ends. |

*\* "Rage" number of uses per rest is shown as a guide; actual balance may be tuned.*

---

### 8.2 Fighter (Martial, D10)

| Lvl | Features |
|:---:|----------|
| 1 | **Fighting Style** — choose from: Archery (+2 ranged attack), Two-Weapon, Great Weapon, Defense (+1 AC), or Dueling (+2 damage with 1H).<br>**Second Wind** (1/rest) — regain 1D10 + level HP as a free action. |
| 2 | **Action Surge** (1/rest) — take an extra action this turn. |
| 3 | **Martial Archetype** — choose Champion (improved critical), Battle Master (maneuvers), or Weapon Master (specialisation). |
| 4 | **Action Surge** (2/rest). **Shield Training** — can bash with shield as bonus action (1D4 + Str mod bludgeoning). |
| 5 | **Extra Attack** — attack twice per action. |
| 6 | **Weapon Expertise** — choose one weapon type; gain +1 attack and +1 damage with it.<br>**Ability Score Improvement** — +1 to any ability (max 20). |
| 7 | **Indomitable** (1/rest) — reroll a failed save. |
| 8 | **Action Surge** (3/rest). **Martial Archetype feature**. |
| 9 | **Indomitable** (2/rest). **Weapon Expertise** for a second weapon type. |
| 10 | **Ability Score Improvement** — +1 to any ability (max 20). |
| 11 | **Extra Attack** (2) — attack three times per action. |
| 12 | **Weapons Master** — all weapon attacks gain +1 to hit. Once per combat, make an attack of opportunity without using your reaction. |

---

### 8.3 Knight (Martial, D10)

| Lvl | Features |
|:---:|----------|
| 1 | **Heavy Armor Proficiency** — no movement penalty in heavy armor.<br>**Shield Bash** — bonus-action bash (1D4 + Str mod bludgeoning; on hit, target is pushed 5 ft.).<br>**Pet Companion** — gain a **Wolf** or **Falcon** pet (see Pet Unlock §11). |
| 2 | **Mounted Combat** — when mounted, mount gains +2 AC and shares half the damage you take.<br>**Charging Strike** — after moving 10+ ft. in a straight line, next melee attack deals +1D8 extra damage. |
| 3 | **Commanding Presence** — allies within 15 ft. gain +1 to hit while you are conscious. |
| 4 | **Improved Shield Bash** — shield bash now deals 1D6 and stuns for 1 round on a critical. |
| 5 | **Extra Attack** — attack twice per action. |
| 6 | **Mount Improvement** — mount gains +10 ft. move speed and +5 Max HP per Knight level. |
| 7 | **Banner of Courage** — allies within 15 ft. are immune to fear while you are conscious. |
| 8 | **Heavy Slam** — as an action, slam the ground; all adjacent enemies make Stamina save or fall prone. |
| 9 | **Unbreakable Will** — once per rest, if you would be stunned or charmed, ignore that effect. |
| 10 | **Improved Mount** — mount gains an additional attack when you take the Attack action. |
| 11 | **Battlefield Commander** — as a bonus action, grant one ally an immediate attack. |
| 12 | **Iron Bulwark** — reduce all incoming damage by 3 while wearing heavy armor and a shield. Allies behind you gain half-cover (+2 AC).<br>**Dragon Bond** (Human/Elf only) — unlock Dragon pet (see §11). |

---

### 8.4 Paladin (Martial, D10 — Half-Caster)

| Lvl | Features |
|:---:|----------|
| 1 | **Divine Sense** — detect celestials, fiends, and undead within 60 ft. (number of uses = 1 + Cha mod / rest).<br>**Lay on Hands** — pool of 5 × level HP to heal allies (can also cure one disease/poison per 5 points spent).<br>**Pet Companion** — gain a **Wolf** or **Falcon** pet (see Pet Unlock §11). |
| 2 | **Divine Smite** — spend a spell slot to add 1D8 + 1D8 per spell level (max +5D8) to a melee hit.<br>**Spellcasting** — L1 spells. Access to half-caster spell progression (see §9.2). |
| 3 | **Divine Health** — immune to disease.<br>**Sacred Oath** — choose Oath of Devotion, Oath of Vengeance, or Oath of the Ancients. |
| 4 | **L2 spells**.<br>**Ability Score Improvement** — +1 to any ability (max 20). |
| 5 | **Extra Attack** — attack twice per action. |
| 6 | **Aura of Protection** — you and allies within 10 ft. add your Cha mod to all saving throws. |
| 7 | **L3 spells**.<br>**Oath feature**. |
| 8 | **Aura of Resolve** — you and allies within 10 ft. are immune to charm. |
| 9 | **L4 spells**. |
| 10 | **Aura of Courage** — you and allies within 10 ft. are immune to fear. |
| 11 | **Improved Divine Smite** — all melee hits deal an extra 1D8 radiant damage. |
| 12 | **L5 spells**.<br>**Holy Champion** — once per rest, transform for 1 minute: gain flying speed 30 ft., aura radius doubles, and smite dice max at no cost.<br>**Dragon Bond** (Human/Elf only) — unlock Dragon pet (see §11). |

---

### 8.5 Rogue (Hybrid, D6)

| Lvl | Features |
|:---:|----------|
| 1 | **Sneak Attack** (1D6) — +1D6 damage on attacks with advantage or when ally is within 5 ft. of target. Once per turn.<br>**Thieves' Cant** — a secret language of signs, symbols, and coded phrases. |
| 2 | **Cunning Action** — Dash, Disengage, or Hide as a bonus action. |
| 3 | **Sneak Attack** (2D6).<br>**Roguish Archetype** — choose Thief (climb speed, faster item use), Assassin (auto-crit on surprised targets), or Shadow Dancer (shadow-step). |
| 4 | **Trap Detection** — automatically spot traps within 15 ft. while searching. Detect traps as a free action once per round. |
| 5 | **Sneak Attack** (3D6).<br>**Uncanny Dodge** — when hit by an attack you can see, halve the damage as a reaction. |
| 6 | **Expertise** — double proficiency bonus for two skills (or one skill and thieves' tools). |
| 7 | **Sneak Attack** (4D6).<br>**Evasion** — area effects deal half damage on a failed save or zero on a successful one. |
| 8 | **Trap Crafting** — can create traps during rest (choose from: caltrops, snare, poison needle). Crafted traps use your Sneak Attack dice for damage. |
| 9 | **Sneak Attack** (5D6).<br>**Cloak** — once per rest, vanish into shadows for 1 minute; your next attack from this state automatically crits. |
| 10 | **Ability Score Improvement** — +1 to any ability (max 20).<br>**Roguish Archetype feature**. |
| 11 | **Sneak Attack** (6D6).<br>**Blindsense** — sense invisible/hidden creatures within 15 ft. |
| 12 | **Master Assassin** — Sneak Attack dice increase to D8 (instead of D6). Once per rest, declare a target; you have advantage on all attacks against that target for 1 minute. |

---

### 8.6 Bard (Hybrid, D6 — Full Caster)

| Lvl | Features |
|:---:|----------|
| 1 | **Spellcasting** — L1 spells (full-caster progression, see §9.1).<br>**Bardic Inspiration** (D6) — 3×/rest, grant an ally a D6 die to add to one roll. |
| 2 | **Jack of All Trades** — add half proficiency to all untrained ability checks.<br>**Song of Rest** — during a short rest, allies regain +1D6 extra HP. |
| 3 | **L2 spells**.<br>**Bard College** — choose College of Lore (extra skills, Cutting Words) or College of Valor (armor proficiency, Combat Inspiration).<br>**Expertise** — double proficiency for two skills. |
| 4 | **Bardic Inspiration** (D8). **Ability Score Improvement** — +1 to any ability (max 20). |
| 5 | **L3 spells**.<br>**Font of Inspiration** — regain all Bardic Inspiration uses on a short rest. |
| 6 | **Countercharm** — as an action, grant nearby allies advantage on charm/fear saves. |
| 7 | **L4 spells**. |
| 8 | **Bardic Inspiration** (D10). |
| 9 | **L5 spells**.<br>**Song of Rest** (D8). |
| 10 | **Expertise** — double proficiency for two more skills.<br>**Magical Secrets** — learn two spells from any class's spell list (at a level you can cast). |
| 11 | **L6 spells**. |
| 12 | **Bardic Inspiration** (D12).<br>**Master of Lore** — learn one more Magical Secret. Friendly creatures add your Cha mod to initiative rolls while you are conscious. |

---

### 8.7 Mage (Caster, D4 — Full Caster)

| Lvl | Features |
|:---:|----------|
| 1 | **Spellcasting** — L1 spells (full-caster progression, see §9.1).<br>**Arcane Recovery** (1/rest) — regain half your level in total spell levels during a short rest (rounded up). |
| 2 | **Arcane Tradition** — choose Evocation (sculpt spells), Conjuration (extended summons), or Illusion (heightened DCs). Gain tradition's L2 feature. |
| 3 | **L2 spells**. |
| 4 | **Ability Score Improvement** — +1 to any ability (max 20). |
| 5 | **L3 spells**. |
| 6 | **Arcane Recovery** (2/rest).<br>**Arcane Tradition feature**. |
| 7 | **L4 spells**. |
| 8 | **Arcane Tradition feature**. |
| 9 | **L5 spells**. |
| 10 | **Arcane Recovery** (3/rest). |
| 11 | **L6 spells**. |
| 12 | **Archmage** — choose one L1 and one L2 spell; you may cast each at its base level for 0 Mana once per rest. Arcane Recovery restores all spent Mana (instead of half).<br>**Dragon Bond** (Human/Elf only) — unlock Dragon pet (see §11). |

---

### 8.8 Priest (Caster, D8 — Full Caster)

| Lvl | Features |
|:---:|----------|
| 1 | **Spellcasting** — L1 spells (full-caster progression, see §9.1).<br>**Turn Undead** (1/rest) — undead within 30 ft. make a Wisdom save or flee for 1 minute. |
| 2 | **Channel Divinity** (1/rest) — use one of: Turn Undead (doesn't count against rest) **or** a domain-specific channel.<br>**Divine Domain** — choose Light (bonus fire/holy spells), Healing (bonus heal dice), or War (weapon/armor prof). |
| 3 | **L2 spells**. |
| 4 | **Turn Undead** (2/rest). **Ability Score Improvement** — +1 to any ability (max 20). |
| 5 | **L3 spells**. |
| 6 | **Channel Divinity** (2/rest).<br>**Divine Domain feature**. |
| 7 | **L4 spells**. |
| 8 | **Divine Intervention** — once per 7 days, you may call upon your deity. The GM chooses an appropriate effect (e.g., heal full party, resurrect ally, banish a demon). |
| 9 | **L5 spells**. |
| 10 | **Turn Undead** (3/rest). |
| 11 | **L6 spells**. |
| 12 | **High Priest** — your Turn Undead now destroys undead of CR 2 or lower on a failed save. Divine Intervention cooldown reduces to 3 days.<br>**Dragon Bond** (Human/Elf only) — unlock Dragon pet (see §11). Dwarf/Gladefolk Priests instead gain Two-Pet ability (see §11b). |

---

### 8.9 Druid (Caster, D8 — Full Caster)

| Lvl | Features |
|:---:|----------|
| 1 | **Spellcasting** — L1 spells (full-caster progression, see §9.1).<br>**Druidic** — a secret language of druids.<br>**Pet Companion** — gain a **Wolf** or **Falcon** pet (see Pet Unlock §11). |
| 2 | **Wild Shape** — transform into a beast of CR 1/4 or lower. Duration = Druid level × 2 rounds. Use 2/rest.<br>**Druid Circle** — choose Circle of the Land (bonus spell slots in natural terrain) or Circle of the Moon (higher CR wild shapes, combat forms). |
| 3 | **L2 spells**. |
| 4 | **Wild Shape** improvement — can now take forms of CR 1/2.<br>**Ability Score Improvement** — +1 to any ability (max 20). |
| 5 | **L3 spells**.<br>**Pet Companion** — your pet gains +10 Max HP and +1 damage die. |
| 6 | **Druid Circle feature**.<br>**Wild Shape** — 3/rest. |
| 7 | **L4 spells**.<br>**Wild Shape** improvement — CR 1 forms. |
| 8 | **Wild Shape** — duration doubles.<br>**Ability Score Improvement** — +1 to any ability (max 20). |
| 9 | **L5 spells**.<br>**Pet Companion** — your pet gains another +10 Max HP. |
| 10 | **Druid Circle feature**.<br>**Wild Shape** — 4/rest; CR 2 forms. |
| 11 | **L6 spells**. |
| 12 | **Archdruid** — unlimited Wild Shape uses (duration still capped). Immune to poison/disease. Your pet gains +5 to all saves and +2 AC. |

---

## 9. Spell Progression Tables

### 9.1 Full Casters (Mage, Priest, Druid, Bard)

**Spell Slots per Level:**

| Char Lvl | L1 | L2 | L3 | L4 | L5 | L6 |
|:--------:|:--:|:--:|:--:|:--:|:--:|:--:|
| 1 | 2 | — | — | — | — | — |
| 2 | 3 | — | — | — | — | — |
| 3 | 4 | 2 | — | — | — | — |
| 4 | 4 | 3 | — | — | — | — |
| 5 | 4 | 3 | 2 | — | — | — |
| 6 | 4 | 3 | 3 | — | — | — |
| 7 | 4 | 3 | 3 | 1 | — | — |
| 8 | 4 | 3 | 3 | 2 | — | — |
| 9 | 4 | 3 | 3 | 3 | 1 | — |
| 10 | 4 | 3 | 3 | 3 | 2 | — |
| 11 | 4 | 3 | 3 | 3 | 2 | 1 |
| 12 | 4 | 3 | 3 | 3 | 2 | 2 |

**Spells Known** (Mage learns from spellbook; Priest/Druid prepare from full list):

| Char Lvl | Spells Known* |
|:--------:|:-------------:|
| 1 | 4 |
| 2 | 5 |
| 3 | 6 |
| 4 | 7 |
| 5 | 8 |
| 6 | 10 |
| 7 | 12 |
| 8 | 13 |
| 9 | 14 |
| 10 | 15 |
| 11 | 16 |
| 12 | 18 |

*\* Mages learn spells into their spellbook (can copy more from scrolls). Priests and Druids prepare spells from their full class list — the "Spells Known" column represents how many they can prepare each day.*

### 9.2 Half-Casters (Paladin)

**Spell Slots per Level:**

| Char Lvl | L1 | L2 | L3 | L4 | L5 |
|:--------:|:--:|:--:|:--:|:--:|:--:|
| 1 | — | — | — | — | — |
| 2 | 2 | — | — | — | — |
| 3 | 3 | — | — | — | — |
| 4 | 3 | — | — | — | — |
| 5 | 4 | 2 | — | — | — |
| 6 | 4 | 2 | — | — | — |
| 7 | 4 | 3 | — | — | — |
| 8 | 4 | 3 | — | — | — |
| 9 | 4 | 3 | 2 | — | — |
| 10 | 4 | 3 | 2 | — | — |
| 11 | 4 | 3 | 3 | — | — |
| 12 | 4 | 3 | 3 | 1 | — |

**Spells Prepared** = Charisma modifier + half Paladin level (minimum 1).

Half-casters round spell levels down — Paladins gain L4 spells at L12, and never reach L5.

---

## 10. Racial Benefits by Level

Each race unlocks new abilities as the character gains experience. Racial benefits are gated by **character level**, not class level.

### Human

| Lvl | Benefit |
|:---:|---------|
| 1 | **Skilled** — proficiency in one extra skill of your choice. |
| 3 | **Adaptable** — gain one bonus feat (weapon proficiency, shield training, or a social skill). |
| 5 | **Inspiring Leader** — during a short rest, one ally regains +1D6 extra HP. |
| 7 | **Human Spirit** — once per rest, regain HP equal to your level as a free action. |
| 9 | **Versatile** — gain proficiency in one extra weapon type of your choice. |
| 12 | **Paragon** — all ability scores increase by +1 (this may raise a score above 20). |

### Elf

| Lvl | Benefit |
|:---:|---------|
| 1 | **Magic Resistance** (25%). **Keen Senses** — proficiency in Perception. |
| 3 | **Trance** — meditate for 4 hours instead of 8 to gain full rest benefits. |
| 5 | **Elven Accuracy** — when you have advantage on an attack roll, you may reroll one die once per turn. |
| 7 | **Fey Ancestry** — immune to magical sleep; advantage on saves vs. charm. |
| 9 | **Greater Magic Resistance** — resistance increases to 35%. |
| 12 | **Fey Ascension** — once per rest, teleport up to 30 ft. as a reaction. Magic Resistance increases to 40%. |

### Dwarf

| Lvl | Benefit |
|:---:|---------|
| 1 | **Magic Resistance** (25%). **Stonecunning** — double proficiency on history checks regarding stonework. |
| 3 | **Dwarven Toughness** — +1 Max HP per character level (retroactive). |
| 5 | **Dwarven Resilience** — advantage on saves vs. poison; resistance to poison damage. |
| 7 | **Stone Armor** — +1 AC while wearing medium or heavy armor. |
| 9 | **Greater Magic Resistance** — resistance increases to 35%. |
| 12 | **Mountain's Endurance** — once per rest, when reduced to 0 HP, instead drop to 1 HP. Resistance to all physical damage for 1 round. |

### Lizard (Descendants of Lesser Dragons)

| Lvl | Benefit |
|:---:|---------|
| 1 | **Poison Immunity** — immune to poison damage and the poisoned condition. **Natural Armor** — base AC 13 + Dex mod when not wearing armor. |
| 3 | **Cold-Blooded Resolve** — gain +1 AC for 1 round after taking fire damage. |
| 5 | **Primal Senses** — darkvision 60 ft.; proficiency in Survival. |
| 7 | **Scale Hardening** — +1 AC (permanent). |
| 9 | **Venom Strike** — once per rest, your next successful melee attack deals an extra 1D6 poison damage. |
| 12 | **Ancient Lizard** — damage reduction 3 vs. all physical sources. Natural Armor AC increases to 14 + Dex mod. |

### Kobold

| Lvl | Benefit |
|:---:|---------|
| 1 | **Magic Resistance** (25%). **Darkvision** 60 ft. |
| 3 | **Pack Tactics** — when an ally is within 5 ft. of your target, you have advantage on melee attack rolls. |
| 5 | **Trap Sense** — advantage on saving throws vs. traps. |
| 7 | **Cunning Escape** — as a reaction when attacked, you may move 10 ft. without provoking opportunity attacks. |
| 9 | **Greater Magic Resistance** — resistance increases to 35%. |
| 12 | **Dragon's Blessing** — once per rest, gain resistance to one damage type of your choice for 1 hour. Magic Resistance increases to 40%. |

### Orc

| Lvl | Benefit |
|:---:|---------|
| 1 | **Extra Strength** — +2 melee damage. **Intimidating** — proficiency in Intimidation. |
| 3 | **Relentless Endurance** — once per rest, when reduced to 0 HP, instead drop to 1 HP. |
| 5 | **Savage Attacks** — when you score a critical hit with a melee weapon, you may add one extra damage die. |
| 7 | **Blood Rage** — while below 50% HP, you deal +1D4 bonus damage on melee attacks. |
| 9 | **Powerful Build** — count as one size category larger for carrying capacity and grappling. |
| 12 | **War Chief** — once per rest, let out a warcry; all allies within 30 ft. gain +1 to hit for 3 rounds. |

### Ogre

| Lvl | Benefit |
|:---:|---------|
| 1 | **Magic Resistance** (25%). **Extra Strength** — +2 melee damage. **Intimidating** — proficiency in Intimidation. |
| 3 | **Ogre Toughness** — +2 Max HP per character level (retroactive). |
| 5 | **Massive Blows** — on a critical hit with a two-handed weapon, target is stunned for 1 round. |
| 7 | **Iron Hide** — +1 AC (permanent). |
| 9 | **Greater Magic Resistance** — resistance increases to 35%. |
| 12 | **Giant's Strength** — Strength score increases by +2 (max 22). You may wield a two-handed weapon in one hand. |

### Gladefolk

| Lvl | Benefit |
|:---:|---------|
| 1 | **Taunt** — force an enemy to attack you (see Status Effects). **Fear Immunity** — immune to the Frightened condition. **Lucky** — once per rest, reroll a natural 1 on an attack, save, or ability check. |
| 3 | **Naturally Stealthy** — you can attempt to hide behind a creature one size larger than you. |
| 5 | **Gladefolk Nimbleness** — +10 ft. move speed; you may move through spaces of larger creatures. |
| 7 | **Brave** — advantage on saves vs. fear (overlaps with immunity for complete protection vs. magical fear). |
| 9 | **Keen Senses** — proficiency in Perception. |
| 12 | **Greater Luck** — Lucky improves: you may reroll any D20 roll (not just natural 1s) once per rest. Additionally, you may grant your Lucky re-roll to an ally. |

---

## 11. Pet Unlock Summary

Reference: see [`../reference/pets.md`](../reference/pets.md) for full pet stats and special abilities.

### Pet Special Abilities

Every pet has a unique **Special Ability** that activates in combat (see `pets.md` for the full table). Abilities range from Pack Hunter (Wolf) to Venom (Spider) and are tied to the pet, not the master's class.

### Dragon — Extraordinary Pet

The **Dragon** is the rarest and most powerful pet. It is gated by both class and race:

| Requirement | Detail |
|-------------|--------|
| **Classes** | Paladin, Knight, Mage, Priest |
| **Races** | Human, Elf |
| **Level** | L12 |

All Dragons share **Fire Breath** (1D10 fire AoE, 1/rest). Additionally, a Dragon's special ability depends on its master's class:

| Master Class | Dragon Ability | Effect |
|:------------:|----------------|--------|
| **Paladin / Knight** | Dragon Fear | Enemies within 15 ft. make a Wisdom save or become Frightened for 2 rounds. 1/rest. |
| **Mage** | Fireball | Single-target 2D10 fire damage, save for half. 1/rest. |
| **Priest** | Diamond Scales | Permanent +2 AC, +10 Max HP, damage reduction 2. |

> **Important:** Non-Human/Elf characters cannot bond with a Dragon even if their class qualifies. See §11b below for the Dwarf/Gladefolk Priest alternative.

### Standard Pet Unlock Levels

| Class | Pets Available | Unlock Level |
|-------|----------------|:------------:|
| **Barbarian** | Wolf, Hound, Boar | L4 |
| **Druid** | Wolf, Falcon, Eagle, Boar | L1 (Wolf or Falcon), L5 (Eagle, Boar) |
| **Fighter** | Wolf, Falcon, Eagle, Hound, Boar | L1, L5 (additional choice) |
| **Knight** | Wolf, Falcon, Eagle, Hound | L1 (Wolf or Falcon), L5 (Eagle, Hound) |
| **Paladin** | Wolf, Falcon, Eagle, Hound | L1 (Wolf or Falcon), L5 (Eagle, Hound) |
| **Rogue** | Panther | L4 |

### Dragon Unlock by Class

| Class | Dragon Unlock | Notes |
|-------|:-------------:|-------|
| **Paladin** | L12 | Human or Elf only |
| **Knight** | L12 | Human or Elf only |
| **Mage** | L12 | Human or Elf only |
| **Priest** | L12 | Human or Elf only |

### 11b. Dwarf & Gladefolk Priest — Two-Pet Exception

Dwarf and Gladefolk Priests cannot bond with a Dragon (Human/Elf only). Instead, they gain the **Two-Pet** ability at L12:

- May have **up to two active pets** simultaneously.
- Choose from: Wolf, Falcon, Eagle, Hound, Bat, Spider.
- Both pets must be **different types** (no duplicate pets).
- Each pet acts independently on the turnmeter.
- Pet leveling bonuses are **split equally**: each pet receives half the HP bonus per tier.

| Character Level | Per-Pet Bonus (Two-Pet Priest) |
|:---------------:|--------------------------------|
| 1–4 | Base stats |
| 5–8 | +5 Max HP, +1 damage die |
| 9–11 | +5 Max HP (total +10), +1 AC |
| 12 | +3 to all saves, +1 AC |

### Pet Leveling (Single-Pet)

When a character levels up, their pet also improves:

| Character Level | Pet Bonus |
|:---------------:|-----------|
| 1–4 | Base stats |
| 5–8 | +10 Max HP, +1 damage die |
| 9–11 | +10 Max HP (total +20), +1 AC |
| 12 | +5 to all saves, +2 AC |

---

## 12. Level-Up Checklist

When a character gains a level, the player should:

1. **Roll Hit Points** — Roll HitDie + Stamina modifier (min 1). Add to both MaxHP and CurrentHP.
2. **Check Strike Rating** — Apply cumulative bonus from the archetype table (§3).
3. **Check Turnmeter Bonus** — Recalculate +Level/N from §4.
4. **Check Accessory Slots** — Unlock any new slots per archetype table (§5).
5. **Apply Class Feature** — Add the feature(s) listed in §8 for this level.
6. **Apply Racial Benefit** — If this is a racial-unlock level (1, 3, 5, 7, 9, or 12), add the benefit from §10.
7. **Update Spells** — If this level grants new spell slots (see §9), select/prepare new spells.
8. **Update Pet** — If this level grants a pet improvement (see §11), apply the bonus.
9. **Save** — Update the character sheet or database record.

---

*Last updated: June 2026.*
