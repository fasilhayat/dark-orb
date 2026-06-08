# Pets Reference

> Keep in sync with `src/.postgres-init/02-seed-data.sql`.

---

| Pet | Description | Damage | AC | HP | Special Ability | Allowed Classes | Allowed Races |
|-----|-------------|:------:|:--:|:--:|----------------|----------------|---------------|
| **Wolf** | A loyal pack hunter with sharp fangs. | 1D6 | 13 | 18 | **Pack Hunter** — +1 to hit when an ally is adjacent to the target. | Barbarian, Druid, Fighter, Paladin | — |
| **Falcon** | A swift bird of prey that strikes from above. | 1D4 | 12 | 8 | **Dive Strike** — first attack each combat deals +1 damage die. | Barbarian, Druid, Fighter, Paladin | — |
| **Eagle** | A majestic raptor with powerful talons. | 1D6 | 13 | 14 | **Keen Sight** — master gains +1 to Perception and ranged attack rolls. | Barbarian, Fighter, Paladin | — |
| **Hound** | A trained war dog with a keen nose. | 1D6 | 14 | 22 | **Tracker** — +1 to hit bleeding or wounded targets. | Barbarian, Fighter, Paladin | — |
| **Panther** | A sleek black predator that hunts in darkness. | 1D8 | 14 | 26 | **Shadow Prowl** — first attack from stealth deals double damage. | Rogue | — |
| **Boar** | A tusked beast with thick hide and fury. | 1D8 | 15 | 30 | **Fury** — deals +1D4 damage while below 50% HP. | Barbarian, Fighter | — |
| **Dragon** | A young dragon bound to its master. See Dragon — Class-Bound Special Abilities below. | 1D10 | 17 | 50 | **Fire Breath** (all) — 1D10 fire AoE, 1/rest. + class ability per table below. | Paladin, Knight, Mage, Priest | Human, Elf |
| **Bat** | A swarm of cave bats that confuse enemies. | 1D4 | 10 | 6 | **Sonic Screech** — chance to confuse one enemy on attack. | Priest, Mage | Undead |
| **Spider** | A venomous arachnid that ensnares prey. | 1D6 | 12 | 12 | **Venom** — bite poisons the target for 1D4 poison/turn for 3 turns. | Priest, Mage | Undead |

> **Note on Dragon race restriction:** Only Human and Elf characters may bond with a Dragon pet, regardless of class. Dwarf and Gladefolk Priests (who cannot take a Dragon) instead gain access to two pet slots — see *Leveling Plan §6*.

## Dragon — Class-Bound Special Abilities

All Dragons share **Fire Breath** (1D10 fire AoE, once per rest). Additionally, a Dragon's special ability depends on its master's class:

| Master Class | Dragon Ability | Effect |
|:------------:|----------------|--------|
| **Paladin / Knight** | **Dragon Fear** | The dragon lets out a deafening roar. All enemies within 15 ft. make a Wisdom save or become **Frightened** for 2 rounds (disadvantage on attacks, cannot move toward the dragon). Usable 1/rest. |
| **Mage** | **Fireball** | The dragon spits a concentrated fireball at a single target: 2D10 fire damage, save for half. Usable 1/rest. |
| **Priest** | **Diamond Scales** | The dragon's scales harden to an adamantine sheen. Permanent +2 AC and +10 Max HP. Damage reduction 2 vs. all sources. |

## Dwarf & Gladefolk Priest — Two-Pet Exception

Dwarf and Gladefolk Priests cannot bond with a Dragon (Human/Elf only). Instead, they may have **up to two active pets simultaneously**, chosen from the standard pet list (Wolf, Falcon, Eagle, Hound, Bat, Spider — both pets must be of different types). Both pets act independently on the turnmeter and share the master's pet leveling bonuses (split equally: each pet receives half the HP bonus per tier).
