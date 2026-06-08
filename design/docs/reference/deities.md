# Deities — Canonical Reference

> **Source of truth.** This file is the single canonical list of deities for the BattleArena world.
> Keep in sync with `src/.postgres-init/02-seed-data.sql`.

| # | Deity Name | Alignment | Court | Domain | Symbol |
|---|-----------|:---------:|:-----:|--------|--------|
| 1 | **Aethelion** | Light | Sky (Heaven) | Heaven, Light — celestial realm of pure light and order | Radiant celestial crown with halo and light rays |
| 2 | **Astrara** | Light | Sky (Heaven) | Stars, Fate — guiding stars illuminating fate | Five-pointed guiding star sigil |
| 3 | **Celestara** | Light | Sky (Heaven) | Destiny, Time — woven destiny patterns in night sky | Connected star-map with lines |
| 4 | **Lunara** | Light | Sky (Heaven) | Moon, Magic, Tides — silver orb governing tides and magic | Silver crescent with arcane curves |
| 5 | **Ignaroth** | Dark | Shadow (Elemental) | Fire, Destruction — consuming flame of destruction and rebirth | Consuming flame with rebirth spiral |
| 6 | **Umbraex** | Dark | Shadow (Shadow) | Darkness, Secrets — void from which shadows are born | Eclipse with empty eye and shadow rings |
| 7 | **Veparix** | Dark | Shadow (Shadow) | Deception, Illusion — veil of deception and obscured truths | Layered drifting wisps forming veil |
| 8 | **Noctivane** | Dark | Shadow (Shadow) | Shadow, Stealth — realm between light/dark, home to assassins | Stealth crescent with concealed blade |
| 9 | **Chronara** | Twilight | Boundary (Time) | Time, Stars, Balance — keeper of time who watches without taking sides | Hourglass with star-filled sands |

> **Alignment aliases:** Light = Good, Dark = Evil, Twilight = Neutral

## Light Deities (Sky Court)

| Deity | Title | Domain | Worshipped By |
|-------|-------|--------|---------------|
| **Aethelion** | The radiant father of light | Heaven, Light | Paladins, priests of order, judges, kings |
| **Astrara** | The guiding star mother | Stars, Fate | Navigators, oracles, fortune-tellers |
| **Celestara** | The weaver of destiny | Destiny, Time | Historians, prophets, time-keepers |
| **Lunara** | The silver moon goddess | Moon, Magic, Tides | Mages, seers, priests of the moon |

## Dark Deities (Shadow Court)

| Deity | Title | Domain | Worshipped By |
|-------|-------|--------|---------------|
| **Ignaroth** | The burning destroyer | Fire, Destruction | Berserkers, pyromancers, nihilists |
| **Umbraex** | The void lord | Darkness, Secrets | Spies, occultists, keepers of forbidden knowledge |
| **Veparix** | The deceptive mist | Deception, Illusion | Tricksters, spies, illusionists |
| **Noctivane** | The shadow assassin god | Shadow, Stealth | Assassins, thieves, shadow mages |

## Twilight Deities (Boundary)

| Deity | Title | Domain | Worshipped By |
|-------|-------|--------|---------------|
| **Chronara** | The keeper of time | Time, Stars, Balance | Historians, prophets, time-keepers, those who seek balance |

## Deity Spell Metadata

Spells granted by deities use the following metadata fields:

| Field | Required | Notes |
|-------|:--------:|-------|
| `PrimaryDeity` | Yes | The deity granting the spell |
| `DeityAlignment` | Yes | Good, Evil, or Neutral |
| `DeitySource` | Yes | Power origin identifier |
| `FallbackDeity` | Yes | `DEITY_UNBOUND` (generic divine magic, no patron assigned) |

For the full list of deity-aligned spells, see [`master-spellbook.md`](../systems/master-spellbook.md).
