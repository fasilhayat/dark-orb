# Dark Orb Master Spellbook

This master document merges the earlier Dark Orb spell roster with the rebuilt progression system so the setting has one complete spell reference instead of separate versions. It keeps AD&D 2e-inspired spell foundations while applying the custom Dark Orb access rules for mages, priests, druids, paladins, and knights.

## Structure

The system now uses one shared framework for every spell entry: **School**, **Spell Level**, **Access Layer**, **Access Tier**, **Minimum Level**, **Effect**, **Impact**, **Class**, **Damage Type**, and **Afterburn**. This lets the spellbook show both fantasy identity and progression logic in one place.

## Progression model

The spellbook is organized into three access layers: **Common Core**, **Class Core**, and **School Specialization**. Common Core stabilizes low-tier play, Class Core gives each class a clear role before rare magic appears, and School Specialization defines the stronger signature effects in the mid and late game.

| Layer | Purpose | Design use |
|---|---|---|
| Common Core | Shared baseline spells used by most members of a casting class. | Gives early characters a stable toolkit and prevents weak low-level identity. |
| Class Core | Spells tied to class fantasy before deep specialization. | Keeps priests, druids, paladins, and knights feeling different early. |
| School Specialization | Signature spells and variants tied to magical doctrine. | Defines rare effects, elite spell picks, and late-game mastery. |

## Class progression

This is the applied progression plan for Dark Orb, including your custom paladin and knight rules. The access model below is intentionally tuned for game feel rather than strict AD&D 2e canon in every case.

| Class | Early progression | Mid progression | Late progression |
|---|---|---|---|
| Mage | Levels 1-2 use a broad Common Core of widely taught arcane staples. | Levels 3-4 keep some shared spells but begin school-gated picks. | Level 5+ is driven mostly by school specialization, rare variants, and elite identity. |
| Priest | Early access to blessings, healing, command, protection, and curse tools. | Mid progression expands into restoration, stronger control, and battlefield support. | Late progression adds miracles, barriers, summons, and supreme healing. |
| Druid | Early access to roots, beasts, natural healing, and terrain magic. | Mid progression expands into storms, swarms, and primal battlefield control. | Late progression adds catastrophes, guardian summons, and great nature magic. |
| Paladin | Starts using magic around level 6 in Dark Orb, focused on self and companion buffs, wards, cleansing, and healing-lite support. | Mid progression adds auras, resistance, anti-fear, and stronger defensive support. | Late progression gains elite holy defense and small-area protection, but stays narrower than a priest. |
| Knight | Starts command-style magic around level 9 in Dark Orb, focused on warcries, morale boosts, TM uplift, formation discipline, and resistance support. | Mid progression adds stronger command auras and anti-panic tools. | Late progression gains elite banner magic, group Magic Resistance support, and morale supremacy rather than broad spellcasting. |

## Access rules

### Mage access rules

Mages begin with a broad **Common Core** so that early arcane play feels flexible and useful rather than prematurely specialized. School choice begins to matter at levels 3-4 and becomes dominant at level 5+, with elite school variants and rare off-school picks treated as mastery rewards.

### Priest access rules

Priests are **deity-aligned casters** — their magic originates from a patron deity, not a spell school. Schools remain secondary metadata for legacy grouping but do not drive progression or identity.

Priests gain early class identity through blessings, command effects, healing, warding, and curse interaction. Their progression should stay broad within divine identity, with late-game access opening stronger battlefield miracles and barriers rather than turning them into elemental specialists.

### Druid access rules

Druids are **deity-aligned casters** — their primal magic is tied to nature deities. Schools remain secondary metadata; identity and progression are deity-driven.

Druids gain early access to roots, nature utility, beast interaction, and natural healing before expanding into storms, swarms, terrain control, and primal summoning. Their list should remain distinct from priests by controlling the battlefield through natural force and environment.

### Paladin access rules

Paladins are **deity-aligned casters** — their magic is channelled through a patron deity. Schools are secondary.

In Dark Orb, paladins start spell use around level 6 as a custom rule, even though AD&D 2e canon places paladin priest spellcasting later. Their list is restricted to protection, self and companion buffs, cleansing, courage, healing-lite support, and defensive wards, with very little offensive magic.

### Knight access rules

Knights are **deity-aligned casters** — their command magic flows from divine authority. Schools are secondary.

Knights begin magical support around level 9 and focus on warcries, morale, formation integrity, TM uplift, fear control, and Magic Resistance support. They should feel like martial leaders using command magic, not priests with a renamed spell list.

## Six schools

Dark Orb uses six broad schools to organize common spells, specialized spells, and custom variants. These are broader and more game-facing than strict AD&D school labels, which makes them better for a large modular spellbook.

### Aegis

Aegis focuses on wards, protection, armor reinforcement, resistance, sanctuaries, anti-magic, and survival tools. It is the main school for Armor Class increases, Magic Resistance buffs, barriers, and anti-control defense.

### Stormcraft

Stormcraft governs raw elemental force such as fire, lightning, frost, detonations, and destructive battlefield hazards. It is the primary school for HP-damage nukes, electrocute variants, and high-pressure area spells.

### Verdancy

Verdancy is the school of nature, beasts, roots, wind, stone, insects, herbs, and primal elemental power. It excels at Movement control, terrain shaping, nature healing, storms, swarms, and guardian summons.

### Umbramancy

Umbramancy is the dark magic school, covering death, undead, shadow, curses, fear, life-drain, anti-caster pressure, and sinister battlefield control. It is the best school for necrotic damage, HP leech, MP drain variants, curse trees, and undead conjuration.

### Mirage

Mirage handles illusion, invisibility, mirror images, deception, confusion, stealth, and perception warping. It supports defense and control through miss chance, misdirection, concealment, and sensory denial.

### Dominion

Dominion governs command, blessing, morale, discipline, fear resistance, divine authority, and battle momentum. It is the natural school for warcries, prayer effects, teamwide TM uplift, panic control, and leadership magic.

## Deity system (divine casters)

A subset of classes derive their power from deities rather than spell schools. This system runs alongside the school system — it does not replace it.

### Divine caster classes

The following classes are **deity-aligned**:

| Class | Role | Magic access |
|-------|------|--------------|
| Priest | Full divine caster | Level 1 |
| Druid | Full divine caster | Level 1 |
| Paladin | Limited divine caster | Level 6 |
| Knight | Command magic caster | Level 9 |

These classes **do not** use spell schools as their primary identity system. Schools remain valid for legacy classification and mechanical grouping but do not drive progression or identity logic.

### Canonical deities

Deities are defined in [`../reference/deities.md`](#../reference/deities.md). The authoritative list:

#### Light deities (Sky / Heaven aligned)

| Deity | Title | Domain |
|-------|-------|--------|
| **Aethelion** | The radiant father of light | Heaven, Light |
| **Astrara** | The guiding star mother | Stars, Fate |
| **Celestara** | The weaver of destiny | Destiny, Time |
| **Lunara** | The silver moon goddess | Moon, Magic, Tides |

#### Twilight deity (Boundary / Time aligned)

| Deity | Title | Domain |
|-------|-------|--------|
| **Chronara** | The keeper of time | Time, Stars, Balance |

Chronara watches stars ignite and shadows burn out without ever taking a side. She is the twilight fulcrum between the celestial and the void. Her associations with stars and the night sky underpin future night-sky buff mechanics tied to constellations, moon phases, and star visibility.

#### Dark deities (Elemental / Shadow aligned)

| Deity | Title | Domain |
|-------|-------|--------|
| **Ignara** | The burning destroyer | Fire, Destruction |
| **Umbraex** | The void lord | Darkness, Secrets |
| **Veparix** | The deceptive mist | Deception, Illusion |
| **Noctivane** | The shadow assassin god | Shadow, Stealth |

### Deity spell metadata

Every divine spell includes the following conceptual fields:

| Field | Required | Description |
|-------|----------|-------------|
| `PrimaryDeity` | Yes | The deity granting the spell |
| `DeityAlignment` | Yes | Good, Evil, or Neutral |
| `DeitySource` | Yes | Power origin identifier |
| `FallbackDeity` | Yes | `DEITY_UNBOUND` (see below) |

### Placeholder: DEITY_UNBOUND

Used when no specific deity is assigned:

- Meaning: Generic divine power source, temporary fallback
- Used until explicit deity binding is defined
- Prevents system gaps during incomplete mappings

### Night-sky buffs (future implementation)

Extra buffs become available when constellations, the moon, or stars are visible in the night sky. These effects are:

- **Reserved for future implementation** — not yet active
- Associated with Chronara (time/stars), Astrara (guiding star), and Lunara (moon)
- May grant temporary bonuses to divine caster classes when fighting under visible celestial bodies
- Design intent: create dynamic power variance tied to in-game time-of-day and location visibility

### School / deity boundary

| System | Used by | Drives |
|--------|---------|--------|
| Spell schools | Arcane casters, fire/frost/shadow/nature users | Progression, identity, spell access |
| Deity system | Priest, Druid, Paladin, Knight | Progression, identity, spell access |

Both systems coexist. Schools remain fully intact and functional for non-divine casters.

### Smite

- **Deity-channelled divine attack spell**
- Restricted to: Paladin (Level 6+), Knight (Level 6+)
- Must be associated with a valid deity
- Cannot be used outside the listed divine caster archetypes

### Chasten

- Divine counterpart to Smite for non-martial casters
- Available to: Priest (Level 1+), Druid (Level 1+)
- Default bound to Light deities
- Supports `DEITY_UNBOUND` fallback
- Mirrors Smite progression structure

## Impact system

The spellbook uses a unified impact language so offensive and defensive spells can be compared cleanly. **HP** covers damage and healing, **TM** covers turn-meter acceleration, loss, or lock, **MP** covers mana drain or leech variants, **Magic Resistance** covers anti-magic protection or magical vulnerability, **Armor Class** covers physical survivability, and **Movement** covers roots, slows, displacement, teleporting, and terrain denial.

## Mage common core

These are the low-tier spells most mages should have access to before school specialization strongly limits choice. This solves the progression issue you identified by making early mages broad and competent before their schools fully define them.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| [Magic Missile](#magic-missile) | Stormcraft | 1 | Common Core | Early | - | Mage | Force | No. | Single-Target Damage, Nuke |
| [Armor](#armor) | Aegis | 1 | Common Core | Early | - | Mage | None | No. | Defensive, Buff |
| [Shield](#shield) | Aegis | 1 | Common Core | Early | - | Mage | None | No. | Defensive |
| [Burning Hands](#burning-hands) | Stormcraft | 1 | Common Core | Early | - | Mage | Fire | No clear persistent burn in baseline list. | Offensive, AoE |
| [Grease](#grease) | Mirage | 1 | Common Core | Early | - | Mage | None/Control | Yes, persistent slippery zone. | CC, Slip, AoE |
| [Sleep](#sleep) | Mirage | 1 | Common Core | Early | - | Mage | None/Control | Yes, duration disable. | CC, AoE |
| [Color Spray](#color-spray) | Mirage | 1 | Common Core | Early | - | Mage | Light/Control | No. | CC, AoE |
| [Detect Magic](#detect-magic) | Aegis / Mirage | 1 | Common Core | Early | - | Mage | None | No. | Utility |
| [Invisibility](#invisibility) | Mirage | 2 | Common Core | Early | - | Mage | None | Yes, duration stealth state. | Invisibility |
| [Mirror Image](#mirror-image) | Mirage | 2 | Common Core | Early | - | Mage | None | Yes, images persist until removed. | Defensive, Image |
| [Web](#web) | Mirage / Dominion | 2 | Common Core | Early | - | Mage | None/Control | Yes, persistent sticky field while active. | CC, Root, AoE |
| [Stinking Cloud](#stinking-cloud) | Umbramancy / Mirage | 2 | Common Core | Early | - | Mage | Poison/Control | Yes, persistent cloud zone. | CC, AoE |

<span id="magic-missile"></span>
### Magic Missile

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/01_magic_missile.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>1</td><td>5</td><td>1d4+1 per dart</td><td>Mage</td><td>Single-Target Damage, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="armor"></span>
### Armor

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/02_armor.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aegis</td><td>1</td><td>10</td><td>None</td><td>Mage</td><td>Defensive, Buff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="shield"></span>
### Shield

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aegis</td><td>1</td><td>-</td><td></td><td>Mage</td><td>Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="burning-hands"></span>
### Burning Hands

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/03_burning_hands.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>1</td><td>5</td><td>1d4 per level Fire</td><td>Mage</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="grease"></span>
### Grease

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>1</td><td>-</td><td></td><td>Mage</td><td>CC, Slip, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="sleep"></span>
### Sleep

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/04_sleep.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>1</td><td>5</td><td>None</td><td>Mage</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="color-spray"></span>
### Color Spray

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>1</td><td>-</td><td></td><td>Mage</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="detect-magic"></span>
### Detect Magic

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aegis / Mirage</td><td>1</td><td>-</td><td></td><td>Mage</td><td>Utility</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="invisibility"></span>
### Invisibility

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/05_invisibility.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>2</td><td>10</td><td>None</td><td>Mage</td><td>Invisibility</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="mirror-image"></span>
### Mirror Image

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>2</td><td>-</td><td></td><td>Mage</td><td>Defensive, Image</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="web"></span>
### Web

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage / Dominion</td><td>2</td><td>-</td><td></td><td>Mage</td><td>CC, Root, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="stinking-cloud"></span>
### Stinking Cloud

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy / Mirage</td><td>2</td><td>-</td><td></td><td>Mage</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




## Mage specialization

From the mid game onward, mage identity shifts toward school-defined picks, stronger battlefield roles, and rarer variants. These are still organized with the same access-rule framework.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| [Lightning Bolt](#lightning-bolt) | Stormcraft | 3 | School Specialization | Mid | - | Mage | Lightning | Optional electric aftershock in variants. | Offensive, AoE, Nuke |
| [Fireball](#fireball) | Stormcraft | 3 | School Specialization | Mid | - | Mage | Fire | No in baseline effect text. | Offensive, AoE, Nuke |
| [Blink](#blink) | Mirage | 3 | School Specialization | Mid | - | Mage | None | Yes, duration displacement effect. | Blink, Defensive |
| [Slow](#slow) | Dominion / Mirage | 3 | School Specialization | Mid | - | Mage | None/Control | Yes, duration-based tempo suppression. | CC, Debuff, Turn-Meter Control |
| [Haste](#haste) | Dominion | 3 | School Specialization | Mid | - | Mage, Paladin, Knight, Bard | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| [Mass Haste](#mass-haste) | Dominion | 5 | School Specialization | Late | - | Mage, Priest, Druid | None/Buff | Yes, duration-based speed buff. Caster suffers DefensePower debuff. | Buff, TM Uplift, Group |
| [Vampiric Touch](#vampiric-touch) | Umbramancy | 3 | School Specialization | Mid | - | Mage | Necrotic/Drain-theme | Leech effect instead of burn. | Single-Target Damage, Leech |
| [Fear](#fear) | Umbramancy / Dominion | 4 | School Specialization | Mid | - | Mage | None/Control | No. | CC, Debuff |
| [Ice Storm](#ice-storm) | Stormcraft | 4 | School Specialization | Mid | - | Mage | Cold/Physical | No. | Offensive, AoE |
| [Confusion](#confusion) | Mirage / Dominion | 4/7 | School Specialization | Late | - | Mage | None/Control | Yes, duration-based control effect. | CC, AoE |
| [Cloudkill](#cloudkill) | Umbramancy | 5 | School Specialization | Late | - | Mage | Poison | Yes, persistent cloud hazard. | Offensive, AoE |
| [Cone of Cold](#cone-of-cold) | Stormcraft | 5 | School Specialization | Late | - | Mage | Cold | No. | Offensive, AoE, Nuke |
| [Feeblemind](#feeblemind) | Umbramancy | 5 | School Specialization | Late | - | Mage | None/Anti-Mage | Yes, lasting debilitation. | CC, Anti-Mage |
| [Delayed Blast Fireball](#delayed-blast-fireball) | Stormcraft | 7 | School Specialization | Late | - | Mage | Fire | No baseline burn rider. | Offensive, AoE, Nuke |
| [Maze](#maze) | Mirage | 8 | School Specialization | Late | - | Mage | None/Control | Yes, exile duration. | CC |
| [Mind Siphon Variant](#mind-siphon-variant) | Umbramancy | 4 | School Specialization | Mid | - | Mage, Dark Priest | Shadow/Drain | Yes, lingering mana suppression in variant design. | MP Leech, Variant |
| [Arc Lash Variant](#arc-lash-variant) | Stormcraft | 3 | School Specialization | Mid | - | Mage | Lightning | Yes, electric aftershock in variant design. | Single-Target Damage, TM Control, Variant |
| [Mirror Guard Variant](#mirror-guard-variant) | Mirage / Aegis | 3 | School Specialization | Mid | - | Mage | Illusory/None | Yes, images persist until broken. | Defensive, Variant |
| [Greasefire Variant](#greasefire-variant) | Stormcraft / Mirage | 2 | School Specialization | Mid | - | Mage | Fire | Yes, brief burning ground effect in variant design. | Offensive, AoE, Variant |
| [Mind Game](#mind-game) | Umbramancy | 2 | School Specialization | Mid | - | Mage | Shadow | Yes, Confused (gray) | CC, Debuff |
| [Charm Person](#charm-person) | Mirage | 2 | School Specialization | Mid | - | Mage | None | Yes, Charmed (pink) | CC, Charm |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="lightning-bolt"></span>
### Lightning Bolt

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>3</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fireball"></span>
### Fireball

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/06_fireball.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>3</td><td>15</td><td>1d6 per level Fire</td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="blink"></span>
### Blink

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>3</td><td>-</td><td></td><td>Mage</td><td>Blink, Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="slow"></span>
### Slow

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/08_slow.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion / Mirage</td><td>3</td><td>15</td><td>None</td><td>Mage</td><td>CC, Debuff, Turn-Meter Control</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="haste"></span>
### Haste

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/07_haste.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion</td><td>3</td><td>20</td><td>None</td><td>Mage, Paladin, Knight, Bard</td><td>Buff, TM Uplift</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="mass-haste"></span>
### Mass Haste

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion</td><td>5</td><td>-</td><td></td><td>Mage, Priest, Druid</td><td>Buff, TM Uplift, Group</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="vampiric-touch"></span>
### Vampiric Touch

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>3</td><td>-</td><td></td><td>Mage</td><td>Single-Target Damage, Leech</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fear"></span>
### Fear

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy / Dominion</td><td>4</td><td>-</td><td></td><td>Mage</td><td>CC, Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="ice-storm"></span>
### Ice Storm

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>4</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="confusion"></span>
### Confusion

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/09_confusion.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage / Dominion</td><td>4</td><td>15</td><td>None</td><td>Mage</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="cloudkill"></span>
### Cloudkill

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>5</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="cone-of-cold"></span>
### Cone of Cold

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>5</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="feeblemind"></span>
### Feeblemind

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/10_feeblemind.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>5</td><td>25</td><td>None</td><td>Mage</td><td>CC, Anti-Mage</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="delayed-blast-fireball"></span>
### Delayed Blast Fireball

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>7</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="maze"></span>
### Maze

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>8</td><td>-</td><td></td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="mind-siphon-variant"></span>
### Mind Siphon Variant

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>4</td><td>-</td><td></td><td>Mage, Dark Priest</td><td>MP Leech, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="arc-lash-variant"></span>
### Arc Lash Variant

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>3</td><td>-</td><td></td><td>Mage</td><td>Single-Target Damage, TM Control, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="mirror-guard-variant"></span>
### Mirror Guard Variant

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage / Aegis</td><td>3</td><td>-</td><td></td><td>Mage</td><td>Defensive, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="greasefire-variant"></span>
### Greasefire Variant

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft / Mirage</td><td>2</td><td>-</td><td></td><td>Mage</td><td>Offensive, AoE, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="mind-game"></span>
### Mind Game

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>2</td><td>-</td><td></td><td>Mage</td><td>CC, Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="charm-person"></span>
### Charm Person

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>2</td><td>-</td><td></td><td>Mage</td><td>CC, Charm</td></tr>
</table>
</td>
</tr>
</table>




## Priest spellbook

Priests gain broad early identity through blessings, healing, commands, wards, and spiritual battlefield control. Their later spells expand into miracles, barriers, supreme restoration, and holy devastation rather than generic arcane offense.

| Spell | Deity | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
|[Bless](#bless)| Aethelion |1|Class Core|Early|-|Priest, Paladin|None|Yes, duration buff.|Buff, AoE|
|[Command](#command)| Umbraex |1|Class Core|Early|-|Priest, Paladin|None/Control|No.|CC|
|[Cure Light Wounds](#cure-light-wounds)| Aethelion |1|Class Core|Early|-|Priest, Druid, Paladin|Healing|No direct after-effect beyond restored HP.|Healing|
|[Protection from Evil](#protection-from-evil)| Aethelion |1|Class Core|Early|-|Priest, Paladin|None|No.|Defensive, Buff|
|[Chasten](#chasten)| Umbraex |1|Core|Early|-|Priest|Radiant|No|Debuff|
|[Sanctuary](#sanctuary)| Aethelion |1|Class Core|Early|-|Priest, Paladin|None|Yes, duration shield-state.|Defensive|
|[Aid](#aid)| Aethelion |2|Class Core|Early|-|Priest, Paladin|None|Yes, duration support buff.|Buff|
|[Chant](#chant)| Astrara |2|Class Core|Early|-|Priest|None|Yes, duration aura.|Buff, Debuff|
|[Hold Person](#hold-person)| Umbraex |2/3|Class Core|Mid|-|Priest|None/Control|No.|CC|
|[Prayer](#prayer)| Chronara |3|Class Core|Mid|-|Priest|None|Yes, duration field effect.|Buff, Debuff|
|[Remove Paralysis](#remove-paralysis)| Celestara |3|Class Core|Mid|-|Priest, Paladin|Cleanse|No.|Healing, Cleanse|
|[Cure Serious Wounds](#cure-serious-wounds)| Aethelion |4|Class Core|Mid|-|Priest, Druid, Paladin|Healing|No.|Healing|
|[Free Action](#free-action)| Astrara |4|Class Core|Mid|-|Priest, Paladin|None|Yes, duration buff.|Defensive|
|[Cure Critical Wounds](#cure-critical-wounds)| Aethelion |5|School Specialization|Late|-|Priest, Druid, Paladin|Healing|No.|Healing|
|[Flame Strike](#flame-strike)| Ignara |5|School Specialization|Late|-|Priest|Fire/Radiant|No explicit lingering burn.|Offensive, Nuke|
|[Heal](#heal)| Aethelion |6|School Specialization|Late|-|Priest|Healing|No.|Healing|
|[Blade Barrier](#blade-barrier)| Celestara |6|School Specialization|Late|-|Priest|Physical/Magical|Yes, persistent hazard while active.|Offensive, Defensive, Barrier|
|[Heroes' Feast](#heroes-feast)| Aethelion |6|School Specialization|Late|-|Priest|Buff|Yes, prebuff duration benefits.|Buff, AoE|
|[Restoration](#restoration)| Aethelion |7|School Specialization|Late|-|Priest|Healing|No.|Healing, Cleanse|

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="bless"></span>
### Bless

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/11_bless.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>8</td><td>None</td><td>Priest, Paladin</td><td>Buff, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="command"></span>
### Command

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>-</td><td></td><td>Priest, Paladin</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="cure-light-wounds"></span>
### Cure Light Wounds

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/12_cure_light_wounds.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>6</td><td>1d8+1 Healing</td><td>Priest, Druid, Paladin</td><td>Healing</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="protection-from-evil"></span>
### Protection from Evil

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/13_protection_from_evil.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>10</td><td>None</td><td>Priest, Paladin</td><td>Defensive, Buff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="chasten"></span>
### Chasten

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>-</td><td></td><td>Priest</td><td>Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="sanctuary"></span>
### Sanctuary

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>-</td><td></td><td>Priest, Paladin</td><td>Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="aid"></span>
### Aid

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>2</td><td>-</td><td></td><td>Priest, Paladin</td><td>Buff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="chant"></span>
### Chant

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Astrara</td><td>2</td><td>-</td><td></td><td>Priest</td><td>Buff, Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="hold-person"></span>
### Hold Person

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/14_hold_person.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbraex</td><td>2</td><td>10</td><td>None</td><td>Priest</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="prayer"></span>
### Prayer

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Astrara</td><td>3</td><td>-</td><td></td><td>Priest</td><td>Buff, Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="remove-paralysis"></span>
### Remove Paralysis

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>3</td><td>-</td><td></td><td>Priest, Paladin</td><td>Healing, Cleanse</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="cure-serious-wounds"></span>
### Cure Serious Wounds

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>4</td><td>-</td><td></td><td>Priest, Druid, Paladin</td><td>Healing</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="free-action"></span>
### Free Action

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Astrara</td><td>4</td><td>-</td><td></td><td>Priest, Paladin</td><td>Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="cure-critical-wounds"></span>
### Cure Critical Wounds

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>5</td><td>-</td><td></td><td>Priest, Druid, Paladin</td><td>Healing</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="flame-strike"></span>
### Flame Strike

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/15_flame_strike.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Ignara</td><td>5</td><td>20</td><td>1d6 per level Fire/Radiant</td><td>Priest</td><td>Offensive, Nuke</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="heal"></span>
### Heal

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/16_heal.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>6</td><td>30</td><td>Cures all HP</td><td>Priest</td><td>Healing</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="blade-barrier"></span>
### Blade Barrier

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/17_blade_barrier.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>6</td><td>25</td><td>1d6 per level Slashing</td><td>Priest</td><td>Offensive, Defensive, Barrier</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="heroes-feast"></span>
### Heroes' Feast

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>6</td><td>-</td><td></td><td>Priest</td><td>Buff, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="restoration"></span>
### Restoration

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>7</td><td>-</td><td></td><td>Priest</td><td>Healing, Cleanse</td></tr>
</table>
</td>
</tr>
</table>




## Druid spellbook

Druids begin with natural control and utility, then scale into storms, swarms, primal damage, and guardian summoning. Their battlefield identity should feel environmental and living rather than doctrinal or purely holy.

| Spell | Deity | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
|[Entangle](#entangle)| Chronara |1|Class Core|Early|-|Druid, Priest|None/Control|Yes, persistent rooting zone while active.|CC, Root|
|[Faerie Fire](#faerie-fire)| Veparix |1|Class Core|Early|-|Druid, Priest|None/Reveal|Yes, duration reveal.|Debuff|
|[Shillelagh](#shillelagh)| Chronara |1|Class Core|Early|-|Druid|Physical/Magical|No.|Buff|
|[Barkskin](#barkskin)| Celestara |2|Class Core|Early|-|Druid, Priest|None|Yes, duration-based defensive skin.|Defensive|
|[Goodberry](#goodberry)| Lunara |2|Class Core|Early|-|Druid, Priest|Healing|No.|Healing|
|[Heat Metal](#heat-metal)| Ignara |2|Class Core|Early|-|Druid, Priest|Fire|Yes, continuing heat damage or pressure.|Debuff|
|[Call Lightning](#call-lightning)| Chronara |3|Class Core|Mid|-|Druid, Priest|Lightning|Yes in repeated-round use, though not burn.|Offensive|
|[Hold Animal](#hold-animal)| Chronara |3|Class Core|Mid|-|Druid, Priest|None/Control|Yes, duration root/paralysis.|CC|
|[Call Woodland Beings](#call-woodland-beings)| Chronara |4|School Specialization|Mid|-|Druid|Variable|Yes, summoned allies persist for duration.|Summoning|
|[Giant Insect](#giant-insect)| Chronara |4|School Specialization|Mid|-|Druid, Priest|Physical|Yes, transformed creatures persist for duration.|Summoning-lite|
|[Insect Plague](#insect-plague)| Umbraex |5|School Specialization|Late|-|Druid, Priest|Physical/Poison-theme|Yes, persistent swarm presence.|Offensive, CC|
|[Anti-Plant Shell](#anti-plant-shell)| Chronara |5|School Specialization|Late|-|Druid, Priest|None|Yes, persistent shell.|Defensive|
|[Fire Seeds](#fire-seeds)| Ignara |6|School Specialization|Late|-|Druid|Fire|Sometimes, depending on trap-style implementation.|Offensive|
|[Liveoak](#liveoak)| Chronara |6|School Specialization|Late|-|Druid|Physical|Yes, awakened guardian persists.|Summoning|
|[Creeping Doom](#creeping-doom)| Umbraex |7|School Specialization|Late|-|Druid|Physical|Yes, persistent swarm pressure.|Offensive, CC|
|[Earthquake](#earthquake)| Chronara |7|School Specialization|Late|-|Druid, Priest|Physical|Yes, persistent terrain disruption during effect.|Offensive, AoE|
|[Turn Undead](#turn-undead)| Aethelion |2|Class Core|Early|-|Priest, Paladin, Knight|Holy|Yes, Fear (2 turns)|Offensive, CC|

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="entangle"></span>
### Entangle

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/18_entangle.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>1</td><td>5</td><td>None</td><td>Druid, Priest</td><td>CC, Root</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="faerie-fire"></span>
### Faerie Fire

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Veparix</td><td>1</td><td>-</td><td></td><td>Druid, Priest</td><td>Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="shillelagh"></span>
### Shillelagh

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>1</td><td>-</td><td></td><td>Druid</td><td>Buff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="barkskin"></span>
### Barkskin

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>2</td><td>-</td><td></td><td>Druid, Priest</td><td>Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="goodberry"></span>
### Goodberry

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Lunara</td><td>2</td><td>-</td><td></td><td>Druid, Priest</td><td>Healing</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="heat-metal"></span>
### Heat Metal

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Ignara</td><td>2</td><td>-</td><td></td><td>Druid, Priest</td><td>Debuff</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="call-lightning"></span>
### Call Lightning

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/19_call_lightning.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>3</td><td>12</td><td>1d6 per level Lightning</td><td>Druid, Priest</td><td>Offensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="hold-animal"></span>
### Hold Animal

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>3</td><td>-</td><td></td><td>Druid, Priest</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="call-woodland-beings"></span>
### Call Woodland Beings

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>4</td><td>-</td><td></td><td>Druid</td><td>Summoning</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="giant-insect"></span>
### Giant Insect

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>4</td><td>-</td><td></td><td>Druid, Priest</td><td>Summoning-lite</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="insect-plague"></span>
### Insect Plague

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbraex</td><td>5</td><td>-</td><td></td><td>Druid, Priest</td><td>Offensive, CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="anti-plant-shell"></span>
### Anti-Plant Shell

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>5</td><td>-</td><td></td><td>Druid, Priest</td><td>Defensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fire-seeds"></span>
### Fire Seeds

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Ignara</td><td>6</td><td>-</td><td></td><td>Druid</td><td>Offensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="liveoak"></span>
### Liveoak

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>6</td><td>-</td><td></td><td>Druid</td><td>Summoning</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="creeping-doom"></span>
### Creeping Doom

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbraex</td><td>7</td><td>-</td><td></td><td>Druid</td><td>Offensive, CC</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="earthquake"></span>
### Earthquake

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>7</td><td>-</td><td></td><td>Druid, Priest</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="turn-undead"></span>
### Turn Undead

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>2</td><td>-</td><td></td><td>Priest, Paladin, Knight</td><td>Offensive, CC</td></tr>
</table>
</td>
</tr>
</table>




## Paladin spellbook

Paladins begin magical access around level 6 in Dark Orb and remain a narrow support caster with holy defenses, buffs, and companion protection. Their spell list intentionally avoids broad offensive identity and instead reinforces survivability, courage, and team stability.

| Spell | Deity | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
|[Bless](#bless)| Aethelion |1|Class Core|Early|-|Improves ally morale and combat readiness.||||

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="bless"></span>
### Bless

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/11_bless.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>8</td><td>None</td><td>Priest, Paladin</td><td>Buff, AoE</td></tr>
</table>
</td>
</tr>
</table>




## Knight spellbook

Knights begin spell-like command magic around level 9 and should feel like tactical leaders using morale, discipline, banner magic, and resistance support. Their list is deliberately distinct from paladins even when both support allies.

| Spell | Deity | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
|[War Cry](#war-cry)| Ignara |1|Class Core|Early|-|Knight, Paladin|Sonic/Morale|Short-duration momentum effect.|CC or Buff, Variant|
|[Smite](#smite)| Aethelion |1|Class Core|Early|-|Knight|Radiant|No|Offensive|
| [Rallying Cry](#rallying-cry) | Aethelion | 1 | Class Core | Early | - | Knight | Sonic/Morale | Short aura duration. | Buff, Variant |
|[Steadfast Line](#steadfast-line)| Celestara |2|Class Core|Early|-|Knight|None|Yes, short formation aura.|Buff, Variant|
|[Banner of Resolve](#banner-of-resolve)| Celestara |2|Class Core|Early|-|Knight|None|Yes, aura duration.|Buff, Variant|
|[Iron Will Litany](#iron-will-litany)| Celestara |3|Class Core|Mid|-|Knight|None|Yes, chant duration.|Defensive, Variant|
|[Advance Signal](#advance-signal)| Chronara |3|Class Core|Mid|-|Knight|None|Short-duration surge.|Buff, Variant|
|[Haste](#haste)| Celestara |3|School Specialization|Mid|-|Knight|None/Buff|Yes, duration-based speed buff.|Buff, TM Uplift|
|[Shielding Cadence](#shielding-cadence)| Celestara |3|Class Core|Mid|-|Knight|None|Yes, cadence duration.|Defensive, Variant|
|[Battle Hymn of Defiance](#battle-hymn-of-defiance)| Chronara |4|School Specialization|Late|-|Knight|Sonic/Morale|Yes, anthem duration.|Buff, AoE, Variant|
|[Arcane Defiance Banner](#arcane-defiance-banner)| Lunara |4|School Specialization|Late|-|Knight|None|Yes, banner aura.|Defensive, Variant|
|[Lionheart Command](#lionheart-command)| Chronara |4|School Specialization|Late|-|Knight|Sonic/Morale|Yes, command duration.|Buff, Variant|

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="war-cry"></span>
### War Cry

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Ignara</td><td>1</td><td>-</td><td></td><td>Knight, Paladin</td><td>CC or Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="smite"></span>
### Smite

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/20_smite.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>8</td><td>1d8+1 Radiant</td><td>Paladin, Knight</td><td>Offensive</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="rallying-cry"></span>
### Rallying Cry

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>1</td><td>-</td><td></td><td>Knight</td><td>Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="steadfast-line"></span>
### Steadfast Line

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>2</td><td>-</td><td></td><td>Knight</td><td>Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="banner-of-resolve"></span>
### Banner of Resolve

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>2</td><td>-</td><td></td><td>Knight</td><td>Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="iron-will-litany"></span>
### Iron Will Litany

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>3</td><td>-</td><td></td><td>Knight</td><td>Defensive, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="advance-signal"></span>
### Advance Signal

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>3</td><td>-</td><td></td><td>Knight</td><td>Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="haste"></span>
### Haste

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/07_haste.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion</td><td>3</td><td>20</td><td>None</td><td>Mage, Paladin, Knight, Bard</td><td>Buff, TM Uplift</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="shielding-cadence"></span>
### Shielding Cadence

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Celestara</td><td>3</td><td>-</td><td></td><td>Knight</td><td>Defensive, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="battle-hymn-of-defiance"></span>
### Battle Hymn of Defiance

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Chronara</td><td>4</td><td>-</td><td></td><td>Knight</td><td>Buff, AoE, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="arcane-defiance-banner"></span>
### Arcane Defiance Banner

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Lunara</td><td>4</td><td>-</td><td></td><td>Knight</td><td>Defensive, Variant</td></tr>
</table>
</td>
</tr>
</table>




<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="lionheart-command"></span>
### Lionheart Command

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Aethelion</td><td>4</td><td>-</td><td></td><td>Knight</td><td>Buff, Variant</td></tr>
</table>
</td>
</tr>
</table>




## Additional Common Spells

These spells are migrated from the quick-reference index. School, class, and progression metadata are preliminary — review during the next progression pass.

| Spell | School | Damage | Mana | Class | Tags |
|------|--------|--------|------|-------|------|
| [Fire Storm](#fire-storm) | Stormcraft | 1D10 Fire | 12 | Mage | Offensive, AoE, Nuke |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fire-storm"></span>
### Fire Storm

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>12</td><td>1D10 Fire</td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




| [Acid Rain](#acid-rain) | Stormcraft | 1D6 Acid | 9 | Mage | Offensive, AoE |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="acid-rain"></span>
### Acid Rain

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>9</td><td>1D6 Acid</td><td>Mage</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




| [Lava Hail](#lava-hail) | Stormcraft | 1D12 Fire | 15 | Mage | Offensive, AoE, Nuke |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="lava-hail"></span>
### Lava Hail

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>15</td><td>1D12 Fire</td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




| [Lightning Strike](#lightning-strike) | Stormcraft | 1D10 Lightning | 10 | Mage | Offensive, AoE |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="lightning-strike"></span>
### Lightning Strike

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>10</td><td>1D10 Lightning</td><td>Mage</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




| [Sand Storm](#sand-storm) | Verdancy | 1D6 Bludgeoning | 7 | Druid | Offensive, AoE |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="sand-storm"></span>
### Sand Storm

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Verdancy</td><td>-</td><td>7</td><td>1D6 Bludgeoning</td><td>Druid</td><td>Offensive, AoE</td></tr>
</table>
</td>
</tr>
</table>




| [Blinding Flash](#blinding-flash) | Mirage | — | 6 | Mage, Priest | CC, AoE |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="blinding-flash"></span>
### Blinding Flash

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>-</td><td>6</td><td>—</td><td>Mage, Priest</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




| [Insect Swarm](#insect-swarm) | Verdancy | 1D4 Piercing | 7 | Druid | Offensive, DoT |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="insect-swarm"></span>
### Insect Swarm

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Verdancy</td><td>-</td><td>7</td><td>1D4 Piercing</td><td>Druid</td><td>Offensive, DoT</td></tr>
</table>
</td>
</tr>
</table>




| [Fog of Despair](#fog-of-despair) | Umbramancy | — | 8 | Priest | CC, AoE |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fog-of-despair"></span>
### Fog of Despair

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Umbramancy</td><td>-</td><td>8</td><td>—</td><td>Priest</td><td>CC, AoE</td></tr>
</table>
</td>
</tr>
</table>




| [Stun](#stun) | Stormcraft | — | 5 | Mage | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="stun"></span>
### Stun

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>5</td><td>—</td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Charm Enemy](#charm-enemy) | Mirage | — | 8 | Mage | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="charm-enemy"></span>
### Charm Enemy

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>-</td><td>8</td><td>—</td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Taunt](#taunt) | Dominion | — | 4 | Knight | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="taunt"></span>
### Taunt

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion</td><td>-</td><td>4</td><td>—</td><td>Knight</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Freeze](#freeze) | Stormcraft | — | 7 | Mage | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="freeze"></span>
### Freeze

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>7</td><td>—</td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Confuse](#confuse) | Mirage | — | 6 | Mage | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="confuse"></span>
### Confuse

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>-</td><td>6</td><td>—</td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Provoke](#provoke) | Dominion | — | 5 | Knight | CC, Debuff |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="provoke"></span>
### Provoke

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Dominion</td><td>-</td><td>5</td><td>—</td><td>Knight</td><td>CC, Debuff</td></tr>
</table>
</td>
</tr>
</table>




| [Sacrifice](#sacrifice) | Deity | — | 0 | Priest | Support |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="sacrifice"></span>
### Sacrifice

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Deity</td><td>-</td><td>0</td><td>—</td><td>Priest</td><td>Support</td></tr>
</table>
</td>
</tr>
</table>




| [Blind](#blind) | Mirage | — | 5 | Mage | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="blind"></span>
### Blind

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Mirage</td><td>-</td><td>5</td><td>—</td><td>Mage</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Root](#root) | Verdancy | — | 5 | Druid | CC |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="root"></span>
### Root

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Verdancy</td><td>-</td><td>5</td><td>—</td><td>Druid</td><td>CC</td></tr>
</table>
</td>
</tr>
</table>




| [Summon Creature](#summon-creature) | Varied | — | 12 | Mage | Summon |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="summon-creature"></span>
### Summon Creature

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Varied</td><td>-</td><td>12</td><td>—</td><td>Mage</td><td>Summon</td></tr>
</table>
</td>
</tr>
</table>




| [Fire Storm](#fire-storm) | Stormcraft | 1D10 Fire | 12 | Mage | Offensive, AoE, Nuke |

<hr style="border: none; border-top: 1px solid #444; margin: 24px 0;">

<span id="fire-storm"></span>
### Fire Storm

<hr>

<table>
<tr>
<td style="padding-right: 12px;"><img src="spell-icons/_placeholder.png" width="120"/></td>
<td>
<table>
<tr><th>School</th><th>Level</th><th>Mana</th><th>Damage</th><th>Class</th><th>Tags</th></tr>
<tr><td>Stormcraft</td><td>-</td><td>12</td><td>1D10 Fire</td><td>Mage</td><td>Offensive, AoE, Nuke</td></tr>
</table>
</td>
</tr>
</table>




## Variant design rules

The spellbook is intentionally broad, so baseline spells can branch into variants while still respecting their school and access rules. Good examples are Lightning Bolt into Electrocute or Arc Lash variants, Barkskin into more elite defensive skins, Web into poisonous or shadow-infused webs, and Umbramancy lines such as Mind Siphon and Mana Leak for MP drain against hostile casters.

## Expansion checklist

When adding new spells, apply the following order so progression remains coherent:

1. Decide the **class identity** first.
2. Assign the **access layer**: Common Core, Class Core, or School Specialization.
3. Assign the **access tier**: Early, Mid, or Late.
4. Set the **minimum level** by class track.
5. Assign the **school** that best matches the spell's doctrine.
6. Define the **impact** in terms of HP, TM, MP, Magic Resistance, Armor Class, or Movement.
7. Add **damage type** and **afterburn** only after the access and identity rules are locked.

## Design summary

The merged structure now preserves the earlier spell identity work while applying the new progression model and access restrictions across the document. Low-tier mages stay broad before specializing, paladins start earlier but remain protective, knights come online later as morale casters, and every spell entry now follows one common design grammar.
