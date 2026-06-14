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

Deities are defined in [`../reference/deities.md`](../reference/deities.md). The authoritative list:

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
| **Ignaroth** | The burning destroyer | Fire, Destruction |
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
| Shield | Aegis | 1 | Common Core | Early | - | Mage | None | No. | Defensive |
| [Burning Hands](#burning-hands) | Stormcraft | 1 | Common Core | Early | - | Mage | Fire | No clear persistent burn in baseline list. | Offensive, AoE |
| Grease | Mirage | 1 | Common Core | Early | - | Mage | None/Control | Yes, persistent slippery zone. | CC, Slip, AoE |
| [Sleep](#sleep) | Mirage | 1 | Common Core | Early | - | Mage | None/Control | Yes, duration disable. | CC, AoE |
| Color Spray | Mirage | 1 | Common Core | Early | - | Mage | Light/Control | No. | CC, AoE |
| Detect Magic | Aegis / Mirage | 1 | Common Core | Early | - | Mage | None | No. | Utility |
| [Invisibility](#invisibility) | Mirage | 2 | Common Core | Early | - | Mage | None | Yes, duration stealth state. | Invisibility |
| Mirror Image | Mirage | 2 | Common Core | Early | - | Mage | None | Yes, images persist until removed. | Defensive, Image |
| Web | Mirage / Dominion | 2 | Common Core | Early | - | Mage | None/Control | Yes, persistent sticky field while active. | CC, Root, AoE |
| Stinking Cloud | Umbramancy / Mirage | 2 | Common Core | Early | - | Mage | Poison/Control | Yes, persistent cloud zone. | CC, AoE |

<span id="magic-missile"></span>
### Magic Missile

<hr>

<img src="spell-icons/01_magic_missile.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 1 | 5 | 1d4+1 per dart | Mage | Single-Target Damage, Nuke |

*The first incantation taught in every arcane academy — three flawless darts of pure force that never deviate from their mark. Three glowing darts of pure force that never miss — each deals 1d4+1 damage and strikes simultaneously with no attack roll required. Force damage bypasses most resistances and immunities. Base 3d4+3 at level 1, gaining +1 dart at levels 3, 5, and 7 (max 6d4+6). Guaranteed HP damage that cannot be dodged, parried, or blocked.*

<span id="armor"></span>
### Armor

<hr>

<img src="spell-icons/02_armor.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Aegis | 1 | 10 | None | Mage | Defensive, Buff |

*A shimmering field of magical force wraps the caster in invisible plate. Creates a protective field granting a significant Armor Class bonus that stacks with worn armor. AC +6 at level 1, scaling +1 per 3 caster levels (max +10). The field lasts until dispelled or the caster rests.*

<span id="shield"></span>
### Shield

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Aegis | 1 | - |  | Mage | Defensive |

*Mage 1 Mage.*

<span id="burning-hands"></span>
### Burning Hands

<hr>

<img src="spell-icons/03_burning_hands.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 1 | 5 | 1d4 per level Fire | Mage | Offensive, AoE |

*A fan of roaring flame erupts from the caster's fingertips. A cone-shaped burst hits all targets in short range for 1d4 fire damage per caster level (max 5d4). No save for half. Base 1d4 at level 1, scaling +1d4 per level up to 5d4.*

<span id="grease"></span>
### Grease

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 1 | - |  | Mage | CC, Slip, AoE |

*Mage 1 Mage Damage type: None/Control. Yes, persistent slippery zone.*

<span id="sleep"></span>
### Sleep

<hr>

<img src="spell-icons/04_sleep.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 1 | 5 | None | Mage | CC, AoE |

*A cloud of shimmering blue motes drifts across the battlefield. Puts low-HP targets into magical slumber, affecting up to 4 HD of creatures total. Slumber breaks on damage or when the duration expires. Non-lethal crowd control that freezes TM.*

<span id="color-spray"></span>
### Color Spray

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 1 | - |  | Mage | CC, AoE |

*Mage 1 Mage Damage type: Light/Control.*

<span id="detect-magic"></span>
### Detect Magic

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Aegis / Mirage | 1 | - |  | Mage | Utility |

*Mage 1 Mage.*

<span id="invisibility"></span>
### Invisibility

<hr>

<img src="spell-icons/05_invisibility.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 2 | 10 | None | Mage | Invisibility |

*The caster or a touched ally fades from sight, becoming a whisper of refracted light. Renders the target completely invisible — attacks against them suffer a severe miss chance. The spell ends when the target attacks or casts an offensive spell.*

<span id="mirror-image"></span>
### Mirror Image

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 2 | - |  | Mage | Defensive, Image |

*Mage 2 Mage Yes, images persist until removed.*

<span id="web"></span>
### Web

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage / Dominion | 2 | - |  | Mage | CC, Root, AoE |

*Mage 2 Mage Damage type: None/Control. Yes, persistent sticky field while active.*

<span id="stinking-cloud"></span>
### Stinking Cloud

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy / Mirage | 2 | - |  | Mage | CC, AoE |

*Mage 2 Mage Damage type: Poison/Control. Yes, persistent cloud zone.*


## Mage specialization

From the mid game onward, mage identity shifts toward school-defined picks, stronger battlefield roles, and rarer variants. These are still organized with the same access-rule framework.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| Lightning Bolt | Stormcraft | 3 | School Specialization | Mid | - | Mage | Lightning | Optional electric aftershock in variants. | Offensive, AoE, Nuke |
| [Fireball](#fireball) | Stormcraft | 3 | School Specialization | Mid | - | Mage | Fire | No in baseline effect text. | Offensive, AoE, Nuke |
| Blink | Mirage | 3 | School Specialization | Mid | - | Mage | None | Yes, duration displacement effect. | Blink, Defensive |
| [Slow](#slow) | Dominion / Mirage | 3 | School Specialization | Mid | - | Mage | None/Control | Yes, duration-based tempo suppression. | CC, Debuff, Turn-Meter Control |
| [Haste](#haste) | Dominion | 3 | School Specialization | Mid | - | Mage, Paladin, Knight, Bard | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| Mass Haste | Dominion | 5 | School Specialization | Late | - | Mage, Priest, Druid | None/Buff | Yes, duration-based speed buff. Caster suffers DefensePower debuff. | Buff, TM Uplift, Group |
| Vampiric Touch | Umbramancy | 3 | School Specialization | Mid | - | Mage | Necrotic/Drain-theme | Leech effect instead of burn. | Single-Target Damage, Leech |
| Fear | Umbramancy / Dominion | 4 | School Specialization | Mid | - | Mage | None/Control | No. | CC, Debuff |
| Ice Storm | Stormcraft | 4 | School Specialization | Mid | - | Mage | Cold/Physical | No. | Offensive, AoE |
| [Confusion](#confusion) | Mirage / Dominion | 4/7 | School Specialization | Late | - | Mage | None/Control | Yes, duration-based control effect. | CC, AoE |
| Cloudkill | Umbramancy | 5 | School Specialization | Late | - | Mage | Poison | Yes, persistent cloud hazard. | Offensive, AoE |
| Cone of Cold | Stormcraft | 5 | School Specialization | Late | - | Mage | Cold | No. | Offensive, AoE, Nuke |
| [Feeblemind](#feeblemind) | Umbramancy | 5 | School Specialization | Late | - | Mage | None/Anti-Mage | Yes, lasting debilitation. | CC, Anti-Mage |
| Delayed Blast Fireball | Stormcraft | 7 | School Specialization | Late | - | Mage | Fire | No baseline burn rider. | Offensive, AoE, Nuke |
| Maze | Mirage | 8 | School Specialization | Late | - | Mage | None/Control | Yes, exile duration. | CC |
| Mind Siphon Variant | Umbramancy | 4 | School Specialization | Mid | - | Mage, Dark Priest | Shadow/Drain | Yes, lingering mana suppression in variant design. | MP Leech, Variant |
| Arc Lash Variant | Stormcraft | 3 | School Specialization | Mid | - | Mage | Lightning | Yes, electric aftershock in variant design. | Single-Target Damage, TM Control, Variant |
| Mirror Guard Variant | Mirage / Aegis | 3 | School Specialization | Mid | - | Mage | Illusory/None | Yes, images persist until broken. | Defensive, Variant |
| Greasefire Variant | Stormcraft / Mirage | 2 | School Specialization | Mid | - | Mage | Fire | Yes, brief burning ground effect in variant design. | Offensive, AoE, Variant |
| Mind Game | Umbramancy | 2 | School Specialization | Mid | - | Mage | Shadow | Yes, Confused (gray) | CC, Debuff |
| Charm Person | Mirage | 2 | School Specialization | Mid | - | Mage | None | Yes, Charmed (pink) | CC, Charm |

<span id="lightning-bolt"></span>
### Lightning Bolt

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 3 | - |  | Mage | Offensive, AoE, Nuke |

*Mage 4 Mage Damage type: Lightning. Optional electric aftershock in variants.*

<span id="fireball"></span>
### Fireball

<hr>

<img src="spell-icons/06_fireball.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 3 | 15 | 1d6 per level Fire | Mage | Offensive, AoE, Nuke |

*A pea-sized bead of orange light streaks to the target point and erupts into a roaring sphere of flame. A wide-area explosion dealing 1d6 fire damage per caster level (cap 10d6) to all targets in a 20-foot radius. Cannot be shaped. Base 5d6 at level 5, scaling +1d6 per level to 10d6.*

<span id="blink"></span>
### Blink

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 3 | - |  | Mage | Blink, Defensive |

*Mage 4 Mage Yes, duration displacement effect.*

<span id="slow"></span>
### Slow

<hr>

<img src="spell-icons/08_slow.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion / Mirage | 3 | 15 | None | Mage | CC, Debuff, Turn-Meter Control |

*A cloying purple haze settles over the target, weighing down their limbs. Reduces turn meter gain by 50%, halves movement speed, and applies -2 DefensePower. Duration 1 round per caster level.*

<span id="haste"></span>
### Haste

<hr>

<img src="spell-icons/07_haste.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | 3 | 20 | None | Mage, Paladin, Knight, Bard | Buff, TM Uplift |

*Time warps around the target as golden energy suffuses their limbs. Massively accelerates turn meter gain by 50% and grants +2 AttackPower and +2 DefensePower. Lasts 1 round per caster level (max 10 rounds).*

<span id="mass-haste"></span>
### Mass Haste

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | 5 | - |  | Mage, Priest, Druid | Buff, TM Uplift, Group |

*Mage 7 Mage, Priest, Druid Damage type: None/Buff. Yes, duration-based speed buff. Caster suffers DefensePower debuff.*

<span id="vampiric-touch"></span>
### Vampiric Touch

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | 3 | - |  | Mage | Single-Target Damage, Leech |

*Mage 4 Mage Damage type: Necrotic/Drain-theme. Leech effect instead of burn.*

<span id="fear"></span>
### Fear

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy / Dominion | 4 | - |  | Mage | CC, Debuff |

*Mage 5 Mage Damage type: None/Control.*

<span id="ice-storm"></span>
### Ice Storm

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 4 | - |  | Mage | Offensive, AoE |

*Mage 5 Mage Damage type: Cold/Physical.*

<span id="confusion"></span>
### Confusion

<hr>

<img src="spell-icons/09_confusion.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage / Dominion | 4 | 15 | None | Mage | CC, AoE |

*Swirling ribbons of clashing colour erupt around the target. The target acts erratically — may attack allies, skip turns, or wander randomly each round. Lasts 1 round per caster level (max 6).*

<span id="cloudkill"></span>
### Cloudkill

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | 5 | - |  | Mage | Offensive, AoE |

*Mage 7 Mage Damage type: Poison. Yes, persistent cloud hazard.*

<span id="cone-of-cold"></span>
### Cone of Cold

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 5 | - |  | Mage | Offensive, AoE, Nuke |

*Mage 7 Mage Damage type: Cold.*

<span id="feeblemind"></span>
### Feeblemind

<hr>

<img src="spell-icons/10_feeblemind.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | 5 | 25 | None | Mage | CC, Anti-Mage |

*A lance of pure psychic corruption pierces the target's consciousness. Devastating Intelligence and Wisdom drain drops mental stats to 1, making spellcasting impossible. Deals severe MP damage (2d6 x caster level).*

<span id="delayed-blast-fireball"></span>
### Delayed Blast Fireball

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 7 | - |  | Mage | Offensive, AoE, Nuke |

*Mage 9 Mage Damage type: Fire. No baseline burn rider.*

<span id="maze"></span>
### Maze

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 8 | - |  | Mage | CC |

*Mage 10 Mage Damage type: None/Control. Yes, exile duration.*

<span id="mind-siphon-variant"></span>
### Mind Siphon Variant

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | 4 | - |  | Mage, Dark Priest | MP Leech, Variant |

*Mage 5 Mage, Dark Priest Damage type: Shadow/Drain. Yes, lingering mana suppression in variant design.*

<span id="arc-lash-variant"></span>
### Arc Lash Variant

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | 3 | - |  | Mage | Single-Target Damage, TM Control, Variant |

*Mage 4 Mage Damage type: Lightning. Yes, electric aftershock in variant design.*

<span id="mirror-guard-variant"></span>
### Mirror Guard Variant

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage / Aegis | 3 | - |  | Mage | Defensive, Variant |

*Mage 4 Mage Damage type: Illusory/None. Yes, images persist until broken.*

<span id="greasefire-variant"></span>
### Greasefire Variant

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft / Mirage | 2 | - |  | Mage | Offensive, AoE, Variant |

*Mage 3 Mage Damage type: Fire. Yes, brief burning ground effect in variant design.*

<span id="mind-game"></span>
### Mind Game

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | 2 | - |  | Mage | CC, Debuff |

*Mage 3 Mage Damage type: Shadow. Yes, Confused (gray).*

<span id="charm-person"></span>
### Charm Person

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | 2 | - |  | Mage | CC, Charm |

*Mage 4 Mage Yes, Charmed (pink).*


## Priest spellbook

Priests gain broad early identity through blessings, healing, commands, wards, and spiritual battlefield control. Their later spells expand into miracles, barriers, supreme restoration, and holy devastation rather than generic arcane offense.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| [Bless](#bless) | Deity | 1 | Class Core | Early | - | Priest, Paladin | None | Yes, duration buff. | Buff, AoE |
| Command | Deity | 1 | Class Core | Early | - | Priest, Paladin | None/Control | No. | CC |
| [Cure Light Wounds](#cure-light-wounds) | Deity | 1 | Class Core | Early | - | Priest, Druid, Paladin | Healing | No direct after-effect beyond restored HP. | Healing |
| [Protection from Evil](#protection-from-evil) | Deity | 1 | Class Core | Early | - | Priest, Paladin | None | No. | Defensive, Buff |
| Chasten | Deity | 1 | Core | Early | - | Priest | Radiant | No | Debuff |
| Sanctuary | Deity | 1 | Class Core | Early | - | Priest, Paladin | None | Yes, duration shield-state. | Defensive |
| Aid | Deity | 2 | Class Core | Early | - | Priest, Paladin | None | Yes, duration support buff. | Buff |
| Chant | Deity | 2 | Class Core | Early | - | Priest | None | Yes, duration aura. | Buff, Debuff |
| [Hold Person](#hold-person) | Deity | 2/3 | Class Core | Mid | - | Priest | None/Control | No. | CC |
| Prayer | Deity | 3 | Class Core | Mid | - | Priest | None | Yes, duration field effect. | Buff, Debuff |
| Remove Paralysis | Deity | 3 | Class Core | Mid | - | Priest, Paladin | Cleanse | No. | Healing, Cleanse |
| Cure Serious Wounds | Deity | 4 | Class Core | Mid | - | Priest, Druid, Paladin | Healing | No. | Healing |
| Free Action | Deity | 4 | Class Core | Mid | - | Priest, Paladin | None | Yes, duration buff. | Defensive |
| Cure Critical Wounds | Deity | 5 | School Specialization | Late | - | Priest, Druid, Paladin | Healing | No. | Healing |
| [Flame Strike](#flame-strike) | Deity | 5 | School Specialization | Late | - | Priest | Fire/Radiant | No explicit lingering burn. | Offensive, Nuke |
| [Heal](#heal) | Deity | 6 | School Specialization | Late | - | Priest | Healing | No. | Healing |
| [Blade Barrier](#blade-barrier) | Deity | 6 | School Specialization | Late | - | Priest | Physical/Magical | Yes, persistent hazard while active. | Offensive, Defensive, Barrier |
| Heroes' Feast | Deity | 6 | School Specialization | Late | - | Priest | Buff | Yes, prebuff duration benefits. | Buff, AoE |
| Restoration | Deity | 7 | School Specialization | Late | - | Priest | Healing | No. | Healing, Cleanse |

<span id="bless"></span>
### Bless

<hr>

<img src="spell-icons/11_bless.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 8 | None | Priest, Paladin | Buff, AoE |

*The priest raises a holy symbol as golden light descends upon their allies. Allies in range gain +1 AttackPower, +10% turn meter rate, and +1 to all saving throws. Affects up to 6 allies.*

<span id="command"></span>
### Command

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Priest, Paladin | CC |

*Priest 1 Priest, Paladin Damage type: None/Control.*

<span id="cure-light-wounds"></span>
### Cure Light Wounds

<hr>

<img src="spell-icons/12_cure_light_wounds.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 6 | 1d8+1 Healing | Priest, Druid, Paladin | Healing |

*A soft green glow radiates from the healer's palms as wounds knit and bruises fade. Restores 1d8+1 hit points to a single target, scaling +1d8+1 per caster level (cap 5d8+5 at level 5).*

<span id="protection-from-evil"></span>
### Protection from Evil

<hr>

<img src="spell-icons/13_protection_from_evil.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 10 | None | Priest, Paladin | Defensive, Buff |

*A shimmering golden ward encircles the target, deflecting the attentions of malevolent forces. Provides +2 AC and +2 saving throws against evil creatures. Grants immunity to mental control and possession.*

<span id="chasten"></span>
### Chasten

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Priest | Debuff |

*1 Priest Damage type: Radiant.*

<span id="sanctuary"></span>
### Sanctuary

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Priest, Paladin | Defensive |

*Priest 1 Priest, Paladin Yes, duration shield-state.*

<span id="aid"></span>
### Aid

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Priest, Paladin | Buff |

*Priest 3 Priest, Paladin Yes, duration support buff.*

<span id="chant"></span>
### Chant

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Priest | Buff, Debuff |

*Priest 3 Priest Yes, duration aura.*

<span id="hold-person"></span>
### Hold Person

<hr>

<img src="spell-icons/14_hold_person.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | 10 | None | Priest | CC |

*Golden bands of divine light wrap around the target, locking their limbs in place. Paralyzes a humanoid target completely — no movement, no actions, no defense. Save each round to break free.*

<span id="prayer"></span>
### Prayer

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Priest | Buff, Debuff |

*Priest 5 Priest Yes, duration field effect.*

<span id="remove-paralysis"></span>
### Remove Paralysis

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Priest, Paladin | Healing, Cleanse |

*Priest 5 Priest, Paladin Damage type: Cleanse.*

<span id="cure-serious-wounds"></span>
### Cure Serious Wounds

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Priest, Druid, Paladin | Healing |

*Priest 6 Priest, Druid, Paladin Damage type: Healing.*

<span id="free-action"></span>
### Free Action

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Priest, Paladin | Defensive |

*Priest 6 Priest, Paladin Yes, duration buff.*

<span id="cure-critical-wounds"></span>
### Cure Critical Wounds

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 5 | - |  | Priest, Druid, Paladin | Healing |

*Priest 7 Priest, Druid, Paladin Damage type: Healing.*

<span id="flame-strike"></span>
### Flame Strike

<hr>

<img src="spell-icons/15_flame_strike.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 5 | 20 | 1d6 per level Fire/Radiant | Priest | Offensive, Nuke |

*A pillar of divine fire descends from the heavens. A vertical column dealing 1d6 fire + 1d6 radiant damage per caster level (cap 15d6+15d6). Undead take double damage. Base 6d6+6d6 at level 6, scaling +1d6/+1d6 per level.*

<span id="heal"></span>
### Heal

<hr>

<img src="spell-icons/16_heal.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 6 | 30 | Cures all HP | Priest | Healing |

*The most powerful restorative miracle in the divine arsenal. Instantly restores the target to full health and cures blindness, deafness, paralysis, disease, and poison.*

<span id="blade-barrier"></span>
### Blade Barrier

<hr>

<img src="spell-icons/17_blade_barrier.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 6 | 25 | 1d6 per level Slashing | Priest | Offensive, Defensive, Barrier |

*A ring of spinning silver blades materializes, orbiting in a deadly dance. An immobile 20-foot ring dealing 1d6 slashing per caster level (cap 15d6) to any creature passing through. Lasts 1 round per level.*

<span id="heroes-feast"></span>
### Heroes' Feast

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 6 | - |  | Priest | Buff, AoE |

*Priest 8 Priest Damage type: Buff. Yes, prebuff duration benefits.*

<span id="restoration"></span>
### Restoration

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 7 | - |  | Priest | Healing, Cleanse |

*Priest 9 Priest Damage type: Healing.*


## Druid spellbook

Druids begin with natural control and utility, then scale into storms, swarms, primal damage, and guardian summoning. Their battlefield identity should feel environmental and living rather than doctrinal or purely holy.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| [Entangle](#entangle) | Deity | 1 | Class Core | Early | - | Druid, Priest | None/Control | Yes, persistent rooting zone while active. | CC, Root |
| Faerie Fire | Deity | 1 | Class Core | Early | - | Druid, Priest | None/Reveal | Yes, duration reveal. | Debuff |
| Shillelagh | Deity | 1 | Class Core | Early | - | Druid | Physical/Magical | No. | Buff |
| Barkskin | Deity | 2 | Class Core | Early | - | Druid, Priest | None | Yes, duration-based defensive skin. | Defensive |
| Goodberry | Deity | 2 | Class Core | Early | - | Druid, Priest | Healing | No. | Healing |
| Heat Metal | Deity | 2 | Class Core | Early | - | Druid, Priest | Fire | Yes, continuing heat damage or pressure. | Debuff |
| [Call Lightning](#call-lightning) | Deity | 3 | Class Core | Mid | - | Druid, Priest | Lightning | Yes in repeated-round use, though not burn. | Offensive |
| Hold Animal | Deity | 3 | Class Core | Mid | - | Druid, Priest | None/Control | Yes, duration root/paralysis. | CC |
| Call Woodland Beings | Deity | 4 | School Specialization | Mid | - | Druid | Variable | Yes, summoned allies persist for duration. | Summoning |
| Giant Insect | Deity | 4 | School Specialization | Mid | - | Druid, Priest | Physical | Yes, transformed creatures persist for duration. | Summoning-lite |
| Insect Plague | Deity | 5 | School Specialization | Late | - | Druid, Priest | Physical/Poison-theme | Yes, persistent swarm presence. | Offensive, CC |
| Anti-Plant Shell | Deity | 5 | School Specialization | Late | - | Druid, Priest | None | Yes, persistent shell. | Defensive |
| Fire Seeds | Deity | 6 | School Specialization | Late | - | Druid | Fire | Sometimes, depending on trap-style implementation. | Offensive |
| Liveoak | Deity | 6 | School Specialization | Late | - | Druid | Physical | Yes, awakened guardian persists. | Summoning |
| Creeping Doom | Deity | 7 | School Specialization | Late | - | Druid | Physical | Yes, persistent swarm pressure. | Offensive, CC |
| Earthquake | Deity | 7 | School Specialization | Late | - | Druid, Priest | Physical | Yes, persistent terrain disruption during effect. | Offensive, AoE |
| Turn Undead | Deity | 2 | Class Core | Early | - | Priest, Paladin, Knight | Holy | Yes, Fear (2 turns) | Offensive, CC |

<span id="entangle"></span>
### Entangle

<hr>

<img src="spell-icons/18_entangle.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 5 | None | Druid, Priest | CC, Root |

*The ground erupts with grasping vines and thick roots that snake around the legs of the unwary. Plants and roots grapple all creatures in a 40-foot radius — movement reduced to 0.*

<span id="faerie-fire"></span>
### Faerie Fire

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Druid, Priest | Debuff |

*Druid 1 Druid, Priest Damage type: None/Reveal. Yes, duration reveal.*

<span id="shillelagh"></span>
### Shillelagh

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Druid | Buff |

*Druid 1 Druid Damage type: Physical/Magical.*

<span id="barkskin"></span>
### Barkskin

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Druid, Priest | Defensive |

*Druid 3 Druid, Priest Yes, duration-based defensive skin.*

<span id="goodberry"></span>
### Goodberry

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Druid, Priest | Healing |

*Druid 3 Druid, Priest Damage type: Healing.*

<span id="heat-metal"></span>
### Heat Metal

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Druid, Priest | Debuff |

*Druid 3 Druid, Priest Damage type: Fire. Yes, continuing heat damage or pressure.*

<span id="call-lightning"></span>
### Call Lightning

<hr>

<img src="spell-icons/19_call_lightning.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | 12 | 1d6 per level Lightning | Druid, Priest | Offensive |

*The druid raises a hand to the sky, summoning a storm bolt from the heavens. A 5-foot wide lightning bolt strikes from above for 1d6 per caster level (cap 10d6). Can be called each round while the storm lasts.*

<span id="hold-animal"></span>
### Hold Animal

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Druid, Priest | CC |

*Druid 5 Druid, Priest Damage type: None/Control. Yes, duration root/paralysis.*

<span id="call-woodland-beings"></span>
### Call Woodland Beings

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Druid | Summoning |

*Druid 6 Druid Damage type: Variable. Yes, summoned allies persist for duration.*

<span id="giant-insect"></span>
### Giant Insect

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Druid, Priest | Summoning-lite |

*Druid 6 Druid, Priest Damage type: Physical. Yes, transformed creatures persist for duration.*

<span id="insect-plague"></span>
### Insect Plague

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 5 | - |  | Druid, Priest | Offensive, CC |

*Druid 7 Druid, Priest Damage type: Physical/Poison-theme. Yes, persistent swarm presence.*

<span id="anti-plant-shell"></span>
### Anti-Plant Shell

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 5 | - |  | Druid, Priest | Defensive |

*Druid 7 Druid, Priest Yes, persistent shell.*

<span id="fire-seeds"></span>
### Fire Seeds

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 6 | - |  | Druid | Offensive |

*Druid 8 Druid Damage type: Fire. Sometimes, depending on trap-style implementation.*

<span id="liveoak"></span>
### Liveoak

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 6 | - |  | Druid | Summoning |

*Druid 8 Druid Damage type: Physical. Yes, awakened guardian persists.*

<span id="creeping-doom"></span>
### Creeping Doom

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 7 | - |  | Druid | Offensive, CC |

*Druid 9 Druid Damage type: Physical. Yes, persistent swarm pressure.*

<span id="earthquake"></span>
### Earthquake

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 7 | - |  | Druid, Priest | Offensive, AoE |

*Druid 9 Druid, Priest Damage type: Physical. Yes, persistent terrain disruption during effect.*

<span id="turn-undead"></span>
### Turn Undead

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Priest, Paladin, Knight | Offensive, CC |

*Priest 3, Paladin 4, Knight 6 Priest, Paladin, Knight Damage type: Holy. Yes, Fear (2 turns).*


## Paladin spellbook

Paladins begin magical access around level 6 in Dark Orb and remain a narrow support caster with holy defenses, buffs, and companion protection. Their spell list intentionally avoids broad offensive identity and instead reinforces survivability, courage, and team stability.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| [Bless](#bless) | Deity | 1 | Class Core | Early | - | Improves ally morale and combat readiness. |  |  |  |

<span id="bless"></span>
### Bless

<hr>

<img src="spell-icons/11_bless.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 8 | None | Priest, Paladin | Buff, AoE |

*The priest raises a holy symbol as golden light descends upon their allies. Allies in range gain +1 AttackPower, +10% turn meter rate, and +1 to all saving throws. Affects up to 6 allies.*


## Knight spellbook

Knights begin spell-like command magic around level 9 and should feel like tactical leaders using morale, discipline, banner magic, and resistance support. Their list is deliberately distinct from paladins even when both support allies.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Class | Damage Type | Afterburn | Tags |
|------|--------|-------------|-------------|-----------|---------------|-------|------------|-----------|------|
| War Cry | Deity | 1 | Class Core | Early | - | Knight, Paladin | Sonic/Morale | Short-duration momentum effect. | CC or Buff, Variant |
| [Smite](#smite) | Deity | 1 | Class Core | Early | - | Knight | Radiant | No | Offensive |
| Rallying Cry | Deity | 1 | Class Core | Early | - | Knight | Sonic/Morale | Short aura duration. | Buff, Variant |
| Steadfast Line | Deity | 2 | Class Core | Early | - | Knight | None | Yes, short formation aura. | Buff, Variant |
| Banner of Resolve | Deity | 2 | Class Core | Early | - | Knight | None | Yes, aura duration. | Buff, Variant |
| Iron Will Litany | Deity | 3 | Class Core | Mid | - | Knight | None | Yes, chant duration. | Defensive, Variant |
| Advance Signal | Deity | 3 | Class Core | Mid | - | Knight | None | Short-duration surge. | Buff, Variant |
| [Haste](#haste) | Dominion | 3 | School Specialization | Mid | - | Knight | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| Shielding Cadence | Deity | 3 | Class Core | Mid | - | Knight | None | Yes, cadence duration. | Defensive, Variant |
| Battle Hymn of Defiance | Deity | 4 | School Specialization | Late | - | Knight | Sonic/Morale | Yes, anthem duration. | Buff, AoE, Variant |
| Arcane Defiance Banner | Deity | 4 | School Specialization | Late | - | Knight | None | Yes, banner aura. | Defensive, Variant |
| Lionheart Command | Deity | 4 | School Specialization | Late | - | Knight | Sonic/Morale | Yes, command duration. | Buff, Variant |

<span id="war-cry"></span>
### War Cry

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Knight, Paladin | CC or Buff, Variant |

*Knight 9 Knight, Paladin Damage type: Sonic/Morale. Short-duration momentum effect.*

<span id="smite"></span>
### Smite

<hr>

<img src="spell-icons/20_smite.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | 8 | 1d8+1 Radiant | Paladin, Knight | Offensive |

*The paladin's weapon blazes with holy radiance as they strike — a single, decisive blow empowered by divine will. Empowers the next melee attack with +1d8 radiant damage (+1 per level). Double damage to undead and demons.*

<span id="rallying-cry"></span>
### Rallying Cry

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 1 | - |  | Knight | Buff, Variant |

*Knight 9 Knight Damage type: Sonic/Morale. Short aura duration.*

<span id="steadfast-line"></span>
### Steadfast Line

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Knight | Buff, Variant |

*Knight 10 Knight Yes, short formation aura.*

<span id="banner-of-resolve"></span>
### Banner of Resolve

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 2 | - |  | Knight | Buff, Variant |

*Knight 10 Knight Yes, aura duration.*

<span id="iron-will-litany"></span>
### Iron Will Litany

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Knight | Defensive, Variant |

*Knight 11 Knight Yes, chant duration.*

<span id="advance-signal"></span>
### Advance Signal

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Knight | Buff, Variant |

*Knight 11 Knight Short-duration surge.*

<span id="haste"></span>
### Haste

<hr>

<img src="spell-icons/07_haste.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | 3 | 20 | None | Mage, Paladin, Knight, Bard | Buff, TM Uplift |

*Time warps around the target as golden energy suffuses their limbs. Massively accelerates turn meter gain by 50% and grants +2 AttackPower and +2 DefensePower. Lasts 1 round per caster level (max 10 rounds).*

<span id="shielding-cadence"></span>
### Shielding Cadence

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 3 | - |  | Knight | Defensive, Variant |

*Knight 12 Knight Yes, cadence duration.*

<span id="battle-hymn-of-defiance"></span>
### Battle Hymn of Defiance

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Knight | Buff, AoE, Variant |

*Knight 12 Knight Damage type: Sonic/Morale. Yes, anthem duration.*

<span id="arcane-defiance-banner"></span>
### Arcane Defiance Banner

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Knight | Defensive, Variant |

*Knight 13 Knight Yes, banner aura.*

<span id="lionheart-command"></span>
### Lionheart Command

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | 4 | - |  | Knight | Buff, Variant |

*Knight 13 Knight Damage type: Sonic/Morale. Yes, command duration.*


## Additional Common Spells

These spells are migrated from the quick-reference index. School, class, and progression metadata are preliminary — review during the next progression pass.

| Spell | School | Damage | Mana | Class | Tags |
|------|--------|--------|------|-------|------|
| [Haste](#haste) | Dominion | — | 20 | Mage, Paladin, Knight | Buff, TM Uplift |

<span id="haste"></span>
### Haste

<hr>

<img src="spell-icons/07_haste.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | - | 20 | — | Mage, Paladin, Knight | Buff, TM Uplift |

*Time warps around the target as golden energy suffuses their limbs. Massively accelerates turn meter gain by 50% and grants +2 AttackPower and +2 DefensePower. Lasts 1 round per caster level (max 10 rounds).*

| Fire Storm | Stormcraft | 1D10 Fire | 12 | Mage | Offensive, AoE, Nuke |

<span id="fire-storm"></span>
### Fire Storm

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 12 | 1D10 Fire | Mage | Offensive, AoE, Nuke |

*Stormcraft.*

| Acid Rain | Stormcraft | 1D6 Acid | 9 | Mage | Offensive, AoE |

<span id="acid-rain"></span>
### Acid Rain

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 9 | 1D6 Acid | Mage | Offensive, AoE |

*Stormcraft.*

| Lava Hail | Stormcraft | 1D12 Fire | 15 | Mage | Offensive, AoE, Nuke |

<span id="lava-hail"></span>
### Lava Hail

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 15 | 1D12 Fire | Mage | Offensive, AoE, Nuke |

*Stormcraft.*

| Lightning Strike | Stormcraft | 1D10 Lightning | 10 | Mage | Offensive, AoE |

<span id="lightning-strike"></span>
### Lightning Strike

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 10 | 1D10 Lightning | Mage | Offensive, AoE |

*Stormcraft.*

| Sand Storm | Verdancy | 1D6 Bludgeoning | 7 | Druid | Offensive, AoE |

<span id="sand-storm"></span>
### Sand Storm

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Verdancy | - | 7 | 1D6 Bludgeoning | Druid | Offensive, AoE |

*Verdancy.*

| Blinding Flash | Mirage | — | 6 | Mage, Priest | CC, AoE |

<span id="blinding-flash"></span>
### Blinding Flash

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | - | 6 | — | Mage, Priest | CC, AoE |

*Mirage.*

| Insect Swarm | Verdancy | 1D4 Piercing | 7 | Druid | Offensive, DoT |

<span id="insect-swarm"></span>
### Insect Swarm

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Verdancy | - | 7 | 1D4 Piercing | Druid | Offensive, DoT |

*Verdancy.*

| Fog of Despair | Umbramancy | — | 8 | Priest | CC, AoE |

<span id="fog-of-despair"></span>
### Fog of Despair

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Umbramancy | - | 8 | — | Priest | CC, AoE |

*Umbramancy.*

| Stun | Stormcraft | — | 5 | Mage | CC |

<span id="stun"></span>
### Stun

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 5 | — | Mage | CC |

*Stormcraft.*

| Charm Enemy | Mirage | — | 8 | Mage | CC |

<span id="charm-enemy"></span>
### Charm Enemy

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | - | 8 | — | Mage | CC |

*Mirage.*

| Taunt | Dominion | — | 4 | Knight | CC |

<span id="taunt"></span>
### Taunt

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | - | 4 | — | Knight | CC |

*Dominion.*

| Freeze | Stormcraft | — | 7 | Mage | CC |

<span id="freeze"></span>
### Freeze

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Stormcraft | - | 7 | — | Mage | CC |

*Stormcraft.*

| Confuse | Mirage | — | 6 | Mage | CC |

<span id="confuse"></span>
### Confuse

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | - | 6 | — | Mage | CC |

*Mirage.*

| Provoke | Dominion | — | 5 | Knight | CC, Debuff |

<span id="provoke"></span>
### Provoke

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Dominion | - | 5 | — | Knight | CC, Debuff |

*Dominion.*

| Sacrifice | Deity | — | 0 | Priest | Support |

<span id="sacrifice"></span>
### Sacrifice

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Deity | - | 0 | — | Priest | Support |

*Deity.*

| Blind | Mirage | — | 5 | Mage | CC |

<span id="blind"></span>
### Blind

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Mirage | - | 5 | — | Mage | CC |

*Mirage.*

| Root | Verdancy | — | 5 | Druid | CC |

<span id="root"></span>
### Root

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Verdancy | - | 5 | — | Druid | CC |

*Verdancy.*

| Summon Creature | Varied | — | 12 | Mage | Summon |

<span id="summon-creature"></span>
### Summon Creature

<hr>

<img src="spell-icons/_placeholder.png" width="120"/>

| School | Level | Mana | Damage | Class | Tags |
|--------|-------|------|--------|-------|------|
| Varied | - | 12 | — | Mage | Summon |

*Varied.*



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
