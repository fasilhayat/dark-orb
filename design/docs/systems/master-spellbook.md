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

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Magic Missile | Stormcraft | 1 | Common Core | Early | Mage 1 | Reliable force darts that strike true. | HP damage. | Mage | Force | No. | Single-Target Damage, Nuke |
| Armor | Aegis | 1 | Common Core | Early | Mage 1 | Magical armor that improves survivability. | Armor Class increase. | Mage | None | No. | Defensive, Buff |
| Shield | Aegis | 1 | Common Core | Early | Mage 1 | Magical shield against attacks and missiles. | Armor Class increase and projectile defense. | Mage | None | No. | Defensive |
| Burning Hands | Stormcraft | 1 | Common Core | Early | Mage 1 | Short cone of flame that scorches nearby enemies. | HP damage. | Mage | Fire | No clear persistent burn in baseline list. | Offensive, AoE |
| Grease | Mirage | 1 | Common Core | Early | Mage 1 | Slippery coating that causes falls and handling failure. | Movement disruption and TM loss from slips or recovery delay. | Mage | None/Control | Yes, persistent slippery zone. | CC, Slip, AoE |
| Sleep | Mirage | 1 | Common Core | Early | Mage 1 | Puts weaker targets into magical sleep. | TM freeze and action denial. | Mage | None/Control | Yes, duration disable. | CC, AoE |
| Color Spray | Mirage | 1 | Common Core | Early | Mage 1 | Cone of sensory overload that blinds, stuns, or drops weak targets. | TM loss and action denial. | Mage | Light/Control | No. | CC, AoE |
| Detect Magic | Aegis / Mirage | 1 | Common Core | Early | Mage 1 | Reveals magical auras and enchantments. | Utility and magical threat awareness. | Mage | None | No. | Utility |
| Invisibility | Mirage | 2 | Common Core | Early | Mage 2 | Makes a target unseen until broken. | Targeting denial and survivability increase. | Mage | None | Yes, duration stealth state. | Invisibility |
| Mirror Image | Mirage | 2 | Common Core | Early | Mage 2 | Creates illusory duplicates to absorb attacks. | Defensive miss chance and survivability increase. | Mage | None | Yes, images persist until removed. | Defensive, Image |
| Web | Mirage / Dominion | 2 | Common Core | Early | Mage 2 | Sticky strands trap and hinder enemies in an area. | Movement reduction or root; TM suppression through trapping. | Mage | None/Control | Yes, persistent sticky field while active. | CC, Root, AoE |
| Stinking Cloud | Umbramancy / Mirage | 2 | Common Core | Early | Mage 2 | Nauseating cloud that disrupts actions in its area. | TM suppression, action failure, and Movement denial by zone pressure. | Mage | Poison/Control | Yes, persistent cloud zone. | CC, AoE |

## Mage specialization

From the mid game onward, mage identity shifts toward school-defined picks, stronger battlefield roles, and rarer variants. These are still organized with the same access-rule framework.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Lightning Bolt | Stormcraft | 3 | School Specialization | Mid | Mage 4 | Straight-line lightning blast through enemies. | HP damage; Electrocute variants can add TM loss or brief stun pressure. | Mage | Lightning | Optional electric aftershock in variants. | Offensive, AoE, Nuke |
| Fireball | Stormcraft | 3 | School Specialization | Mid | Mage 4 | Explosive ranged fire burst for clustered targets. | HP damage to all victims in blast radius. | Mage | Fire | No in baseline effect text. | Offensive, AoE, Nuke |
| Blink | Mirage | 3 | School Specialization | Mid | Mage 4 | Phasing displacement defense. | Strong hit avoidance and mobility defense. | Mage | None | Yes, duration displacement effect. | Blink, Defensive |
| Slow | Dominion / Mirage | 3 | School Specialization | Mid | Mage 4 | Reduces enemy tempo and action efficiency. | TM reduction and Movement reduction. | Mage | None/Control | Yes, duration-based tempo suppression. | CC, Debuff, Turn-Meter Control |
| Haste | Dominion | 3 | School Specialization | Mid | Mage 5 | Accelerates a target, massively increasing turn meter gain. | TM acceleration and action frequency increase. | Mage, Paladin, Knight, Bard | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| Mass Haste | Dominion | 5 | School Specialization | Late | Mage 7 | Accelerates all party members, boosting turn meter gain. Inflicts Haste Fatigue (DefensePower -2) on the caster. | Party-wide TM acceleration with a caster defense penalty. | Mage, Priest, Druid | None/Buff | Yes, duration-based speed buff. Caster suffers DefensePower debuff. | Buff, TM Uplift, Group |
| Vampiric Touch | Umbramancy | 3 | School Specialization | Mid | Mage 4 | Melee life-drain spell that steals vitality. | Victim loses HP; caster gains HP or sustain value in adaptation. | Mage | Necrotic/Drain-theme | Leech effect instead of burn. | Single-Target Damage, Leech |
| Fear | Umbramancy / Dominion | 4 | School Specialization | Mid | Mage 5 | Sends enemies fleeing in panic. | TM disorder and forced Movement away from threat source. | Mage | None/Control | No. | CC, Debuff |
| Ice Storm | Stormcraft | 4 | School Specialization | Mid | Mage 5 | Area storm of cold and impact force. | HP damage and possible Movement reduction in variant implementations. | Mage | Cold/Physical | No. | Offensive, AoE |
| Confusion | Mirage / Dominion | 4/7 | School Specialization | Late | Mage 6 | Scrambles enemy behavior and target selection. | TM unreliability, wasted turns, and positional chaos. | Mage | None/Control | Yes, duration-based control effect. | CC, AoE |
| Cloudkill | Umbramancy | 5 | School Specialization | Late | Mage 7 | Expanding poisonous cloud that fills space and kills or weakens creatures. | HP damage over time and Movement denial through zone pressure. | Mage | Poison | Yes, persistent cloud hazard. | Offensive, AoE |
| Cone of Cold | Stormcraft | 5 | School Specialization | Late | Mage 7 | Heavy cone-shaped cold burst. | HP damage; can support Movement slow in variant forms. | Mage | Cold | No. | Offensive, AoE, Nuke |
| Feeblemind | Umbramancy | 5 | School Specialization | Late | Mage 7 | Cripples caster or intellectual function. | MP pressure, anti-caster shutdown, and reduced magical threat output. | Mage | None/Anti-Mage | Yes, lasting debilitation. | CC, Anti-Mage |
| Delayed Blast Fireball | Stormcraft | 7 | School Specialization | Late | Mage 9 | Timed explosive fire spell for setup nuking. | Massive HP damage with delayed detonation pressure. | Mage | Fire | No baseline burn rider. | Offensive, AoE, Nuke |
| Maze | Mirage | 8 | School Specialization | Late | Mage 10 | Temporarily removes a target from the battlefield. | TM removal through temporary battlefield exile. | Mage | None/Control | Yes, exile duration. | CC |
| Mind Siphon Variant | Umbramancy | 4 | School Specialization | Mid | Mage 5 | Dark anti-mage variant that drains magical reserves. | MP damage or MP leech against spellcasters; can also reduce Magic Resistance in elite versions. | Mage, Dark Priest | Shadow/Drain | Yes, lingering mana suppression in variant design. | MP Leech, Variant |
| Arc Lash Variant | Stormcraft | 3 | School Specialization | Mid | Mage 4 | Focused lightning lash that shocks one target intensely. | HP damage plus Electrocute for TM loss or brief action delay. | Mage | Lightning | Yes, electric aftershock in variant design. | Single-Target Damage, TM Control, Variant |
| Mirror Guard Variant | Mirage / Aegis | 3 | School Specialization | Mid | Mage 4 | Advanced mirror-image ward with partial retaliation or reflect chance. | Defense through miss chance and possible Magic Resistance flavor in elite versions. | Mage | Illusory/None | Yes, images persist until broken. | Defensive, Variant |
| Greasefire Variant | Stormcraft / Mirage | 2 | School Specialization | Mid | Mage 3 | Custom variant that ignites a grease field into a burning slick. | HP damage plus Movement denial on the slicked area. | Mage | Fire | Yes, brief burning ground effect in variant design. | Offensive, AoE, Variant |
| Mind Game | Umbramancy | 2 | School Specialization | Mid | Mage 3 | Confuses the target, causing erratic behavior. | Random target selection, may skip turn or hit ally. | Mage | Shadow | Yes, Confused (gray) | CC, Debuff |
| Charm Person | Mirage | 2 | School Specialization | Mid | Mage 4 | Charms a humanoid to fight as an ally. | Target switches sides for the duration. | Mage | None | Yes, Charmed (pink) | CC, Charm |

## Priest spellbook

Priests gain broad early identity through blessings, healing, commands, wards, and spiritual battlefield control. Their later spells expand into miracles, barriers, supreme restoration, and holy devastation rather than generic arcane offense.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Bless | Deity | 1 | Class Core | Early | Priest 1 | Improves ally morale and combat performance. | TM uplift in custom pacing systems and general support. | Priest, Paladin | None | Yes, duration buff. | Buff, AoE |
| Command | Deity | 1 | Class Core | Early | Priest 1 | One-word forced action disrupting the target briefly. | TM disruption and action loss for the victim. | Priest, Paladin | None/Control | No. | CC |
| Cure Light Wounds | Deity | 1 | Class Core | Early | Priest 1 | Basic divine healing. | HP restoration. | Priest, Druid, Paladin | Healing | No direct after-effect beyond restored HP. | Healing |
| Protection from Evil | Deity | 1 | Class Core | Early | Priest 1 | Defensive ward against evil influence and attacks. | Armor Class improvement and defensive resistance versus evil threats. | Priest, Paladin | None | No. | Defensive, Buff |
| Chasten | Deity | 1 | Core | Early | 1 | Weakens sinful/hostile targets | TM loss / debuff | Priest | Radiant | No | Debuff |
| Sanctuary | Deity | 1 | Class Core | Early | Priest 1 | Makes hostile creatures less likely or unable to attack the protected subject directly. | Defensive targeting denial and effective survivability increase. | Priest, Paladin | None | Yes, duration shield-state. | Defensive |
| Aid | Deity | 2 | Class Core | Early | Priest 3 | Supportive blessing that improves staying power. | Effective HP increase and morale support. | Priest, Paladin | None | Yes, duration support buff. | Buff |
| Chant | Deity | 2 | Class Core | Early | Priest 3 | Battlefield prayer that aids allies and hinders enemies. | Ally TM support, enemy TM drag, and battle momentum shift. | Priest | None | Yes, duration aura. | Buff, Debuff |
| Hold Person | Deity | 2/3 | Class Core | Mid | Priest 4 | Paralyzes humanoid targets. | TM freeze and Movement set to zero while held. | Priest | None/Control | No. | CC |
| Prayer | Deity | 3 | Class Core | Mid | Priest 5 | Broad ally buff plus enemy penalty effect. | Teamwide tempo advantage, including custom TM uplift for allies and drag for foes. | Priest | None | Yes, duration field effect. | Buff, Debuff |
| Remove Paralysis | Deity | 3 | Class Core | Mid | Priest 5 | Frees allies from paralysis. | Restores Movement and TM gain by ending paralysis. | Priest, Paladin | Cleanse | No. | Healing, Cleanse |
| Cure Serious Wounds | Deity | 4 | Class Core | Mid | Priest 6 | Stronger direct healing. | HP restoration. | Priest, Druid, Paladin | Healing | No. | Healing |
| Free Action | Deity | 4 | Class Core | Mid | Priest 6 | Prevents many movement-impairing effects. | Movement immunity to many roots, holds, or slows. | Priest, Paladin | None | Yes, duration buff. | Defensive |
| Cure Critical Wounds | Deity | 5 | School Specialization | Late | Priest 7 | Large heal for severe injuries. | HP restoration. | Priest, Druid, Paladin | Healing | No. | Healing |
| Flame Strike | Deity | 5 | School Specialization | Late | Priest 7 | Vertical divine column of holy fire. | HP damage and holy offensive pressure. | Priest | Fire/Radiant | No explicit lingering burn. | Offensive, Nuke |
| Heal | Deity | 6 | School Specialization | Late | Priest 8 | Major restorative miracle. | Major HP restoration and condition recovery. | Priest | Healing | No. | Healing |
| Blade Barrier | Deity | 6 | School Specialization | Late | Priest 8 | Immobile wall or ring of whirling blades around a point. | HP damage and Movement denial by forcing enemies to stop, reroute, or suffer repeated contact damage. | Priest | Physical/Magical | Yes, persistent hazard while active. | Offensive, Defensive, Barrier |
| Heroes' Feast | Deity | 6 | School Specialization | Late | Priest 8 | Group pre-battle meal with strong support benefits. | Teamwide survivability, morale, and resilience increase. | Priest | Buff | Yes, prebuff duration benefits. | Buff, AoE |
| Restoration | Deity | 7 | School Specialization | Late | Priest 9 | Repairs severe spiritual or life-force harm. | Restores magical stability and cleanses severe debuffs. | Priest | Healing | No. | Healing, Cleanse |

## Druid spellbook

Druids begin with natural control and utility, then scale into storms, swarms, primal damage, and guardian summoning. Their battlefield identity should feel environmental and living rather than doctrinal or purely holy.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Entangle | Deity | 1 | Class Core | Early | Druid 1 | Plants twist around creatures in the area and restrain them. | Movement reduction or full root; TM loss in variant implementations. | Druid, Priest | None/Control | Yes, persistent rooting zone while active. | CC, Root |
| Faerie Fire | Deity | 1 | Class Core | Early | Druid 1 | Outlines targets, countering stealth and concealment. | Reduced evasiveness and easier targeting; lowers effective defensive concealment. | Druid, Priest | None/Reveal | Yes, duration reveal. | Debuff |
| Shillelagh | Deity | 1 | Class Core | Early | Druid 1 | Enchants a club or staff to hit harder. | Raises weapon HP damage output. | Druid | Physical/Magical | No. | Buff |
| Barkskin | Deity | 2 | Class Core | Early | Druid 3 | Skin becomes as tough as bark, improving base Armor Class. | Armor Class increase and slight defensive resilience increase. | Druid, Priest | None | Yes, duration-based defensive skin. | Defensive |
| Goodberry | Deity | 2 | Class Core | Early | Druid 3 | Creates restorative berries. | HP restoration and sustain support. | Druid, Priest | Healing | No. | Healing |
| Heat Metal | Deity | 2 | Class Core | Early | Druid 3 | Punishes armored enemies through escalating heat. | HP damage over time, pain pressure, and possible Movement disruption. | Druid, Priest | Fire | Yes, continuing heat damage or pressure. | Debuff |
| Call Lightning | Deity | 3 | Class Core | Mid | Druid 5 | Repeated lightning strikes called from a storm. | HP damage; ideal for variants with Electrocute, TM loss, or anti-metal bonus damage. | Druid, Priest | Lightning | Yes in repeated-round use, though not burn. | Offensive |
| Hold Animal | Deity | 3 | Class Core | Mid | Druid 5 | Immobilizes beasts. | TM freeze and Movement set to zero. | Druid, Priest | None/Control | Yes, duration root/paralysis. | CC |
| Call Woodland Beings | Deity | 4 | School Specialization | Mid | Druid 6 | Brings nature spirits or woodland allies. | HP pressure, support utility, or CC depending on ally type. | Druid | Variable | Yes, summoned allies persist for duration. | Summoning |
| Giant Insect | Deity | 4 | School Specialization | Mid | Druid 6 | Enlarges vermin into combat-capable forms. | HP pressure and Movement denial through large bodies. | Druid, Priest | Physical | Yes, transformed creatures persist for duration. | Summoning-lite |
| Insect Plague | Deity | 5 | School Specialization | Late | Druid 7 | Swarming insects disrupt and overwhelm groups. | HP chip damage, Movement hindrance, and TM pressure through disruption. | Druid, Priest | Physical/Poison-theme | Yes, persistent swarm presence. | Offensive, CC |
| Anti-Plant Shell | Deity | 5 | School Specialization | Late | Druid 7 | Prevents plant creatures from closing in. | Personal safety zone and Movement denial against plant attackers. | Druid, Priest | None | Yes, persistent shell. | Defensive |
| Fire Seeds | Deity | 6 | School Specialization | Late | Druid 8 | Druid explosive seeds used as bombs or traps. | HP damage and trap-style zone denial. | Druid | Fire | Sometimes, depending on trap-style implementation. | Offensive |
| Liveoak | Deity | 6 | School Specialization | Late | Druid 8 | Awakens or empowers a great tree guardian. | HP pressure, tank presence, and Movement blocking. | Druid | Physical | Yes, awakened guardian persists. | Summoning |
| Creeping Doom | Deity | 7 | School Specialization | Late | Druid 9 | Devastating moving swarm that overwhelms enemies. | HP damage over time plus Movement denial by panic and pursuit pressure. | Druid | Physical | Yes, persistent swarm pressure. | Offensive, CC |
| Earthquake | Deity | 7 | School Specialization | Late | Druid 9 | Wide-area terrain disruption and collapse threat. | HP damage, Movement disruption, and TM loss from knockdown or instability in variants. | Druid, Priest | Physical | Yes, persistent terrain disruption during effect. | Offensive, AoE |
| Turn Undead | Deity | 2 | Class Core | Early | Priest 3, Paladin 4, Knight 6 | Drives undead enemies away in fear. | Undead must resist or flee; holy damage to undead. | Priest, Paladin, Knight | Holy | Yes, Fear (2 turns) | Offensive, CC |

## Paladin spellbook

Paladins begin magical access around level 6 in Dark Orb and remain a narrow support caster with holy defenses, buffs, and companion protection. Their spell list intentionally avoids broad offensive identity and instead reinforces survivability, courage, and team stability.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Bless | Deity | 1 | Class Core | Early | Paladin 6 | Improves ally morale and combat readiness. | TM uplift and combat support.
 | Paladin | None | Yes, duration buff. | Buff, AoE |
| Smite | Deity | 1 | Class Core | Early | Paladin 6 | Divine strike vs enemies | HP dmg | Paladin | Radiant | No | Offensive |
| Cure Light Wounds | Deity | 1 | Class Core | Early | Paladin 6 | Basic holy healing. | HP restoration. | Paladin | Healing | No. | Healing |
| Remove Fear | Deity | 1 | Class Core | Early | Paladin 6 | Clears fear and bolsters courage. | TM stabilization and panic protection. | Paladin | None | No major after-effect beyond morale protection. | Buff, Cleanse |
| Protection from Evil | Deity | 1 | Class Core | Early | Paladin 6 | Defensive ward against evil influence and attacks. | Armor Class support and defensive resistance. | Paladin | None | No. | Defensive |
| Aid | Deity | 2 | Class Core | Early | Paladin 7 | Supportive blessing with extra staying power. | Effective HP increase and morale support. | Paladin | None | Yes, duration support buff. | Buff |
| Barkskin | Deity | 2 | Class Core | Early | Paladin 7 | Toughens skin like bark in Dark Orb's holy-nature support blend. | Armor Class increase and resilience increase. | Paladin | None | Yes, duration-based defensive skin. | Defensive, Variant |
| Resist Fire/Resist Cold | Deity | 2 | Class Core | Mid | Paladin 7 | Grants elemental resistance. | Effective HP increase versus selected damage type. | Paladin | None | Yes, duration buff. | Defensive |
| Chant | Deity | 2 | Class Core | Mid | Paladin 8 | Holy chant that steadies allies and pressures foes. | Ally TM support and enemy tempo drag. | Paladin | None | Yes, duration aura. | Buff, Debuff, Variant |
| Remove Paralysis | Deity | 3 | Class Core | Mid | Paladin 8 | Frees allies from paralysis. | Restores Movement and TM gain by ending paralysis. | Paladin | Cleanse | No. | Cleanse |
| Haste | Dominion | 3 | School Specialization | Mid | Paladin 9 | Accelerates a target, massively increasing turn meter gain. | TM acceleration. | Paladin | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| Magical Vestment | Deity | 3 | Class Core | Mid | Paladin 8 | Enhances armor or shield quality with divine power. | Armor Class increase. | Paladin | None | Yes, duration buff. | Buff, Defensive |
| Free Action | Deity | 4 | School Specialization | Late | Paladin 9 | Prevents roots, holds, and slows. | Movement immunity to control effects. | Paladin | None | Yes, duration buff. | Defensive |
| Protection from Evil, 10' Radius | Deity | 4 | School Specialization | Late | Paladin 9 | Group protection aura against evil. | Group defense, Armor Class support, and anti-control protection. | Paladin | None | Yes, persistent aura duration. | Defensive, AoE |
| Holy Bulwark Variant | Deity | 4 | School Specialization | Late | Paladin 10 | Elite paladin ward for nearby allies. | Armor Class increase, Magic Resistance support, and brief TM stabilization. | Paladin | Radiant/None | Yes, aura duration. | Defensive, Variant |
| Paladin's Warcry Variant | Deity | 3 | School Specialization | Late | Paladin 9 | Inspiring holy battle-cry that rallies nearby allies. | Ally TM increase, fear resistance, and minor attack uplift in custom design. | Paladin | Sonic/Morale | Short-duration momentum buff. | Buff, AoE, Variant |

## Knight spellbook

Knights begin spell-like command magic around level 9 and should feel like tactical leaders using morale, discipline, banner magic, and resistance support. Their list is deliberately distinct from paladins even when both support allies.

| Spell | School | Spell Level | Access Layer | Access Tier | Minimum Level | Effect | Impact | Class | Damage Type | Afterburn | Tags |
|---|---|---|---|---|---|---|---|---|---|---|---|
| War Cry | Deity | 1 | Class Core | Early | Knight 9 | Battle shout that shocks enemies or steels allies. | Offensive version causes TM disruption and panic in enemies; support version grants TM gain and fear resistance to allies. | Knight, Paladin | Sonic/Morale | Short-duration momentum effect. | CC or Buff, Variant |
| Smite | Deity | 1 | Class Core | Early | Knight 6 | Divine strike vs enemies | HP dmg | Knight | Radiant | No | Offensive |
| Rallying Cry | Deity | 1 | Class Core | Early | Knight 9 | Calls allies back into formation. | TM increase and morale restoration for companions. | Knight | Sonic/Morale | Short aura duration. | Buff, Variant |
| Steadfast Line | Deity | 2 | Class Core | Early | Knight 10 | Reinforces discipline and formation stability. | Movement resistance to forced displacement and TM stabilization. | Knight | None | Yes, short formation aura. | Buff, Variant |
| Banner of Resolve | Deity | 2 | Class Core | Early | Knight 10 | Banner magic that hardens allied will. | Fear resistance, TM uplift, and morale support. | Knight | None | Yes, aura duration. | Buff, Variant |
| Iron Will Litany | Deity | 3 | Class Core | Mid | Knight 11 | Litany of discipline against hostile magic. | Magic Resistance increase and anti-panic support. | Knight | None | Yes, chant duration. | Defensive, Variant |
| Advance Signal | Deity | 3 | Class Core | Mid | Knight 11 | Tactical call to press the attack. | Ally TM increase and Movement boost for an advance. | Knight | None | Short-duration surge. | Buff, Variant |
| Haste | Dominion | 3 | School Specialization | Mid | Knight 12 | Accelerates a target, massively increasing turn meter gain. | TM acceleration. | Knight | None/Buff | Yes, duration-based speed buff. | Buff, TM Uplift |
| Shielding Cadence | Deity | 3 | Class Core | Mid | Knight 12 | Rhythmic command that improves survival in formation. | Armor Class increase and partial Magic Resistance support. | Knight | None | Yes, cadence duration. | Defensive, Variant |
| Battle Hymn of Defiance | Deity | 4 | School Specialization | Late | Knight 12 | Powerful morale chant for large engagements. | Teamwide TM uplift, panic immunity, and combat resilience. | Knight | Sonic/Morale | Yes, anthem duration. | Buff, AoE, Variant |
| Arcane Defiance Banner | Deity | 4 | School Specialization | Late | Knight 13 | Elite banner ward against sorcery. | Group Magic Resistance increase and magical pressure reduction. | Knight | None | Yes, banner aura. | Defensive, Variant |
| Lionheart Command | Deity | 4 | School Specialization | Late | Knight 13 | Supreme command that hardens allied resolve. | Large TM uplift, fear immunity, and offense confidence boost. | Knight | Sonic/Morale | Yes, command duration. | Buff, Variant |

## Additional Common Spells

These spells are migrated from the quick-reference index. School, class, and progression metadata are preliminary — review during the next progression pass.

| Spell | School | Description | Damage | Mana | Impact | Class | Tags |
|-------|--------|-------------|:------:|:----:|--------|-------|------|
| Haste | Dominion | Accelerates a target, doubling turn meter gain for a short duration. | — | 20 | TM acceleration | Mage, Paladin, Knight | Buff, TM Uplift |
| Fire Storm | Stormcraft | A conflagration engulfs the area. | 1D10 Fire | 12 | HP damage | Mage | Offensive, AoE, Nuke |
| Acid Rain | Stormcraft | Corrosive rain burns all in the area. | 1D6 Acid | 9 | HP damage | Mage | Offensive, AoE |
| Lava Hail | Stormcraft | Molten rock rains from the sky. | 1D12 Fire | 15 | HP damage | Mage | Offensive, AoE, Nuke |
| Lightning Strike | Stormcraft | A bolt of lightning strikes from above. | 1D10 Lightning | 10 | HP damage | Mage | Offensive, AoE |
| Sand Storm | Verdancy | Blinding sand scours the battlefield. | 1D6 Bludgeoning | 7 | HP damage | Druid | Offensive, AoE |
| Blinding Flash | Mirage | A brilliant flash blinds all who see it. | — | 6 | TM disruption | Mage, Priest | CC, AoE |
| Insect Swarm | Verdancy | A cloud of biting insects descends. | 1D4 Piercing | 7 | HP damage, DoT | Druid | Offensive, DoT |
| Fog of Despair | Umbramancy | A choking fog that saps morale. | — | 8 | TM disruption | Priest | CC, AoE |
| Stun | Stormcraft | A concussive force that stuns the target. | — | 5 | TM freeze | Mage | CC |
| Charm Enemy | Mirage | Bends an enemy to your will. | — | 8 | TM control | Mage | CC |
| Taunt | Dominion | Forces an enemy to attack you. | — | 4 | TM disruption | Knight | CC |
| Freeze | Stormcraft | Encases the target in ice. | — | 7 | TM freeze | Mage | CC |
| Confuse | Mirage | Makes the target act erratically. | — | 6 | TM disruption | Mage | CC |
| Provoke | Dominion | Enrages the target, reducing its defenses. | — | 5 | Debuff | Knight | CC, Debuff |
| Sacrifice | Deity | Sacrifice own HP to empower an ally. | — | 0 | HP transfer | Priest | Support |
| Blind | Mirage | Robs the target of sight. | — | 5 | Debuff | Mage | CC |
| Root | Verdancy | Anchors the target to the ground. | — | 5 | Movement denial | Druid | CC |
| Summon Creature | Varied | Calls a creature to fight for you. | — | 12 | Summoning | Mage | Summon |

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
