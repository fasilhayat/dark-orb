# Equipment Reference

> **Source of truth.** All weapons, armor, accessories, and item sets.
> Keep in sync with `src/.postgres-init/02-seed-data.sql` and `src/BattleArena.Demo/roster.json`.

---

## Table of Contents

1. [Common Weapons](#1-common-weapons)
2. [Epic Weapons](#2-epic-weapons)
3. [Legendary Weapons](#3-legendary-weapons)
4. [Cursed Weapons](#4-cursed-weapons)
5. [Rare / Heirloom Weapons](#5-rare--heirloom-weapons)
6. [Common Armor](#6-common-armor)
7. [Epic Armor](#7-epic-armor)
8. [Legendary Armor](#8-legendary-armor)
9. [Cursed Armor](#9-cursed-armor)
10. [Rare / Heirloom Armor](#10-rare--heirloom-armor)
11. [Rings](#11-rings)
12. [Amulets](#12-amulets)
13. [Girdles](#13-girdles)
14. [Item Sets](#14-item-sets)

---

## 1. Common Weapons

| Weapon | Type | Damage | Attack | Hands | Quality |
|--------|:----:|:------:|:------:|:-----:|:-------:|
| **Hand Axe** — *A weathered throwing axe passed down through generations of border scouts. The leather grip bears the brand of the Northern Watch.* | Axe | 1D6 Slashing | Melee | 1H | Common |
| **Battle Axe** — *Forged for the line-breakers of the Iron Company. Each swing carries the weight of a hundred battles fought in the mountain passes.* | Axe | 1D8 Slashing | Melee | 2H | Common |
| **Short Sword** — *The preferred blade of city guards and sellswords. Quick, reliable, and easy to maintain in the field.* | ShortSword | 1D6 Piercing | Melee | 1H | Common |
| **Long Sword** — *The knight's companion. Balanced for cut and thrust, this blade has been the weapon of warriors across every kingdom for a thousand years.* | Sword | 1D8 Slashing | Melee | 1H | Common |
| **Great Sword** — *A towering blade requiring both hands and the strength of three men. Favored by executioners and elite shock troops of the Crimson Legion.* | Sword | 1D10 Slashing | Melee | 2H | Common |
| **War Hammer** — *Its head forged in the shape of a ram's skull by the smiths of the Stonepeak clan. Each blow lands like a battering ram against fortifications.* | Hammer | 1D8 Bludgeoning | Melee | 1H | Common |
| **Maul** — *A weapon that does not cut or pierce — it simply destroys whatever it hits. The tool of temple guardians and ogre-killers.* | Hammer | 1D10 Bludgeoning | Melee | 2H | Common |
| **Mace** — *The simplest of weapons — a weighted head on a wooden shaft. It crushes armor where a blade would turn aside.* | Mace | 1D6 Bludgeoning | Melee | 1H | Common |
| **Great Mace** — *A massive two-handed mace requiring great strength. Its head is forged from solid steel, capable of caving in plate armor and shattering shields with a single blow.* | Mace | 1D10 Bludgeoning | Melee | 2H | Common |
| **Morning Star** — *A spiked iron ball on a short chain. Once carried by the cavalry of the fallen Kingdom of Ashvale before its fall to the demon horde.* | MorningStar | 1D8 Piercing | Melee | 1H | Common |
| **Lance** — *The thunder of a cavalry charge is the sound of lances lowering. Few things in battle match the terror of knights at full gallop.* | Lance | 1D10 Piercing | Melee | 2H | Common |
| **Spear** — *The oldest weapon of mortal kind. Easy to learn, hard to master, deadly in disciplined formation.* | Spear | 1D6 Piercing | Melee | 1H | Common |
| **Dagger** — *Small enough to hide in a boot or up a sleeve. Every adventurer carries one, and every assassin has used one.* | Dagger | 1D4 Piercing | Melee | 1H | Common |
| **Quarter Staff** — *A length of hardened ironwood favored by travelers, monks, and those who prefer discretion over steel.* | Staff | 1D6 Bludgeoning | Melee | 2H | Common |
| **Wand** — *A slender focus rod of enchanted elm. Its tip glows faintly when magic surges through it. Used by hedge wizards and court mages alike.* | Wand | 1D4 Piercing | Spell | 1H | Common |
| **Short Bow** — *A curved bow carved from yew and horn by the bowyers of the Green Valley. Hunters across the realm rely on its steady pull.* | Bow | 1D6 Piercing | Ranged | 2H | Common |
| **Long Bow** — *The signature weapon of the Eldergard Rangers. Its range and stopping power are whispered about in every tavern from here to the coast.* | Bow | 1D8 Piercing | Ranged | 2H | Common |
| **Light Crossbow** — *A mechanical bow that can be fired one-handed while prone. The favored tool of tunnel fighters and castle defenders.* | Crossbow | 1D6 Piercing | Ranged | 2H | Common |
| **Heavy Crossbow** — *A miniature siege engine. Its bolts punch through plate armor at two hundred paces. Requires a crank and steady nerves.* | Crossbow | 1D10 Piercing | Ranged | 2H | Common |

---

## 2. Epic Weapons

| Weapon | Type | Damage | Attack | Hands | Atk Bonus |
|--------|:----:|:------:|:------:|:-----:|:---------:|
| **Bone Crusher** — *A brutal mace carved from the femur of a hill giant by the shaman of the Thunder Ridge tribe. It shatters armor and bone into dust. The handle is wrapped in the hide of the shaman's first kill.* | Mace | 1D8 Bludgeoning | Melee | 1H | +1 |
| **Wind Cutter** — *A slender long sword balanced to perfection. Forged by the wind elf smith Aeloril, who spent a century shaping its edge. It sings as it cuts through air.* | Sword | 1D8 Slashing | Melee | 1H | +1 |
| **Viper Fang** — *A curved assassin's dagger coated in a venom that never dries. The fang of the great serpent Sythiss was hollowed and set into a hilt of obsidian.* | Dagger | 1D6 Poison | Melee | 1H | +1 |

---

## 3. Legendary Weapons

| Weapon | Type | Damage | Attack | Hands | Atk Bonus |
|--------|:----:|:------:|:------:|:-----:|:---------:|
| **Soul Reaver** — *A massive black blade forged in the Abyss from a dying star. It drinks the souls of the fallen and whispers their final screams to its wielder.* | Sword | 1D12 Slashing | Melee | 2H | +3 |
| **Stormbringer** — *A crackling lance charged with the fury of a primordial storm. When the wielder charges, thunder shakes the earth and lightning arcs from the tip.* | Lance | 1D12 Lightning | Melee | 2H | +2 |
| **Dragon's Fury** — *A flaming battle axe forged from the fang of the Great Wyrm Igneel. The blade burns eternally, and its wounds cauterize as they are made.* | Axe | 1D10 Fire | Melee | 2H | +2 |
| **Shadow Sting** — *A dagger that exists partly in the material plane and partly in the Shadowfell. It phases through armor to strike the soul directly.* | Dagger | 1D6 Shadow | Melee | 1H | +2 |
| **Frostbite** — *A short sword blessed by the Frost Queen of the Northern Wastes. Eternal ice coats the blade, slowing victims and leaving frozen wounds.* | ShortSword | 1D8 Ice | Melee | 1H | +2 |
| **Sun's Wrath** — *A morning star that glows with the light of dawn. Forged by the priests of Aethelion, it sears the undead and burns away darkness.* | MorningStar | 1D10 Holy | Melee | 1H | +2 |

---

## 4. Cursed Weapons

| Weapon | Type | Damage | Attack | Hands | Atk Bonus | Curse |
|--------|:----:|:------:|:------:|:-----:|:---------:|-------|
| **Blood Drinker** — *A long sword with a red vein running through the steel. It hungers for blood and grants its wielder unnatural strength, but it feeds on the wielder's life force with every swing.* | Sword | 1D10 Slashing | Melee | 1H | +2 | -1 HP per successful hit |
| **Witchwood Staff** — *A gnarled staff of living black wood that writhes in the hand. It amplifies dark magic but slowly poisons the user's mind with whispers from the void.* | Staff | 1D8 Shadow | Spell | 2H | +1 | -1 Wisdom per day held |
| **Soul Prison** — *A mace forged from the bars of a broken cage that held a hundred souls. It hits like an avalanche and traps a fragment of each victim's spirit, but the trapped souls scream constantly in the wielder's mind.* | Mace | 1D10 Bludgeoning | Melee | 1H | +3 | -1 Stamina per day, chance to be stunned by screams |
| **Serpent's Fang** — *A spear tipped with the fang of the World Serpent. Venom drips eternally from the tip. It is deadly to enemies — and occasionally to its wielder.* | Spear | 1D8 Piercing | Melee | 2H | +1 | 10% chance to poison self on critical miss |

---

## 5. Rare / Heirloom Weapons

| Weapon | Type | Damage | Attack | Hands | Atk Bonus |
|--------|:----:|:------:|:------:|:-----:|:---------:|
| **Father's Mercy** — *A well-worn long sword passed down through five generations of the Samek family. The leather grip is molded to the hand of the original owner, and the blade bears the scratches of a hundred battles. It may not be magical, but it has never let its wielder down.* | Sword | 1D8 Slashing | Melee | 1H | +0 |
| **The Last Argument** — *A morning star forged from the chains of a slave galley by a freed prisoner named Harvoth. Each of the six spikes represents a year of servitude. Harvoth vowed it would be his last argument in any dispute.* | MorningStar | 1D8 Piercing | Melee | 1H | +1 |
| **Wolf's Bane** — *A spear originally crafted by the ranger Aldric to hunt the dire wolves that terrorized his village. The shaft is wrapped in silver wire and the obsidian tip has never dulled. It has claimed the lives of seven alpha wolves.* | Spear | 1D6 Piercing | Melee | 1H | +0 |
| **Oathkeeper** — *A blade broken and reforged three times, each by a different smith across three generations. Its current form is simple, unbreakable, and sharp — much like the oath it was forged to represent.* | Sword | 1D8 Slashing | Melee | 1H | +0 |
| **Barrow Bow** — *A short bow carved from the root of a tree that grew through an ancient barrow. The wood remembers the dead and guides arrows toward the vital spots of the living.* | Bow | 1D6 Piercing | Ranged | 2H | +1 |
| **Final Toll** — *A hand axe carried by the bell-ringer of the Temple of Passing. He used it to defend the temple during the Sack of Eldergard. The axe still rings like a bell when it strikes.* | Axe | 1D6 Slashing | Melee | 1H | +0 |

---

## 6. Common Armor

| Armor | AC | Category | Max Dex | Stealth | Str Req | Mitigation |
|-------|:--:|:--------:|:-------:|:-------:|:-------:|:----------:|
| **Shield** — *A wooden shield branded with the crest of the City Watch. It has stopped arrows, blades, and a charging boar. The paint is chipped from a dozen battles.* | +2 | Shield | ∞ | OK | 0 | — |
| **Padded Armor** — *Quilted cloth stuffed with raw wool and straw. Better than nothing, but only barely. Worn by militia conscripts and desperate peasants.* | 11 | Light | ∞ | DIS | 0 | — |
| **Robes** — *Simple woven cloth robes worn by scholars, priests, and mages. No physical protection, but they allow complete freedom of movement for spellcasting.* | 10 | Caster | ∞ | OK | 0 | 0 |
| **Leather Armor** — *Treated leather boiled in wax and shaped to the body. Worn by scouts, highwaymen, and rangers who value mobility over raw protection.* | 11 | Light | ∞ | OK | 0 | 1 |
| **Studded Leather** — *Leather reinforced with hundreds of iron rivets. A favorite among city guards who patrol the dangerous dock districts at night.* | 12 | Light | ∞ | OK | 0 | 1 |
| **Hide Armor** — *The stripped hide of a cave bear, crudely cured over a campfire. Primitive but effective. Worn by the barbarian tribes of the Frozen Wastes.* | 12 | Medium | +2 | OK | 0 | 2 |
| **Chain Shirt** — *A shirt of interlocking rings that jingles with every step. The minimum standard for any professional soldier in the Eldergard army.* | 13 | Medium | +2 | OK | 0 | — |
| **Scale Mail** — *Overlapping iron plates sewn onto a leather backing resembling dragon scales. Provides excellent protection against slashing attacks. Worn by dragon-hunters of the Burning Plains.* | 14 | Medium | +2 | DIS | 0 | 2 |
| **Breastplate** — *A polished steel breastplate engraved with the wearer's family crest. Favored by officers who need protection without sacrificing mobility.* | 14 | Medium | +2 | OK | 0 | — |
| **Half Plate** — *Partial plate armor covering the vital areas while leaving joints exposed for mobility. A grim compromise between protection and speed.* | 15 | Medium | +2 | DIS | 0 | — |
| **Ring Mail** — *Leather armor with heavy iron rings sewn across the surface. An old design still used by frontier garrisons who cannot afford better.* | 14 | Heavy | +0 | DIS | 0 | — |
| **Chain Mail** — *A full hauberk of interlocking rings reaching to the knees. Heavy, noisy, but nearly impervious to slashing weapons. Standard issue for the Iron Company.* | 16 | Heavy | +0 | DIS | 13 | 3 |
| **Splint Armor** — *Vertical steel strips riveted to a sturdy leather backing. An affordable alternative to full plate, favored by veteran mercenaries of the Free Companies.* | 17 | Heavy | +0 | DIS | 15 | — |
| **Plate Armor** — *The pinnacle of mortal armor craft. Articulated steel plates covering every inch of the body. Only knights and wealthy lords can afford it.* | 18 | Heavy | +0 | DIS | 15 | 5 |

*(∞ = unlimited Dex bonus, DIS = Stealth Disadvantage, — = not yet seeded in game data)*

---

## 7. Epic Armor

| Armor | AC | Category | Max Dex | Stealth | Str Req | AC Bonus |
|-------|:--:|:--------:|:-------:|:-------:|:-------:|:--------:|
| **Knight's Honor** — *Ceremonial splint armor blessed by the priests of Aethelion. Its enameled surface depicts the Battle of the Silver Plains in exquisite detail.* | 17 | Heavy | +0 | DIS | 15 | +1 |
| **Mithril Chain** — *A shimmering chain shirt forged from mithril, the lightest metal known to dwarven craft. It flows like silk but protects like steel.* | 14 | Medium | ∞ | OK | 0 | +1 |

---

## 8. Legendary Armor

| Armor | AC | Category | Max Dex | Stealth | Str Req | AC Bonus |
|-------|:--:|:--------:|:-------:|:-------:|:-------:|:--------:|
| **Titan Plate** — *Colossal plate forged in the heart of Mount Kryx by ancient giant smiths. It could withstand a direct hit from a god's hammer.* | 18 | Heavy | +0 | DIS | 18 | +2 |
| **Dragon Scale Mail** — *Armor woven from hundreds of indestructible dragon scales from the Great Wyrm Igneel. The scales still retain their fire resistance.* | 15 | Medium | +2 | OK | 0 | +1 |
| **Shadow Cloak** — *A cloak woven from the fabric of twilight itself. It drifts and shifts of its own accord, causing enemy attacks to miss at the last instant.* | 12 | Light | ∞ | OK | 0 | +1 |

---

## 9. Cursed Armor

| Armor | AC | Category | Max Dex | Stealth | Str Req | AC Bonus | Curse |
|-------|:--:|:--------:|:-------:|:-------:|:-------:|:--------:|-------|
| **Binding Chains** — *Chain mail forged from the actual chains of a prison ship that sank with a hundred souls aboard. The damned still cling to it, dragging on the wearer. Grants protection but weighs on the spirit.* | 16 | Heavy | +0 | DIS | 13 | +2 | -2 Dexterity, cannot remove without Remove Curse |
| **Mask of the Betrayer** — *A full helm of black steel with no eyeholes — yet the wearer sees perfectly through it. Sometimes. The helm shows its bearer visions of their greatest betrayal at the worst possible moment.* | 1 | Light | ∞ | OK | 0 | +1 | Occasional hallucination (-2 on next save) |
| **Widow's Embrace** — *A beautiful silver breastplate that once belonged to a queen who watched her entire kingdom fall. It protects the body but fills the heart with grief.* | 14 | Medium | +2 | OK | 0 | +1 | -1 Charisma, wearer weeps during battle |

---

## 10. Rare / Heirloom Armor

| Armor | AC | Category | Max Dex | Stealth | Str Req | AC Bonus |
|-------|:--:|:--------:|:-------:|:-------:|:-------:|:--------:|
| **Forest Warden's Coat** — *Studded leather grown from a living treant sapling by the druids of the Deepwood. It breathes, repairs itself over time, and never impedes movement.* | 13 | Light | ∞ | OK | 0 | +1 |
| **Mariner's Plate** — *Rust-proof plate armor inlaid with coral from the Sunken Kingdoms. It grants the wearer the ability to breathe water and move freely underwater.* | 18 | Heavy | +0 | DIS | 15 | +1 |
| **Watchman's Shield** — *A well-worn shield with a fist-sized dent — the result of stopping a boulder during the Siege of Ironwall. The watchman who carried it saved a dozen lives that day.* | +2 | Shield | +0 | OK | 0 | +1 |

---

## 11. Rings

| Ring | Quality | Effect | Cursed? |
|------|:-------:|--------|:-------:|
| **Band of the Bull** — *A thick iron band etched with a charging bull. Grants +2 Strength.* | Rare | Strength +2 | No |
| **Serpent Ring** — *A coiled jade serpent that sharpens the mind. Grants +2 Intelligence.* | Rare | Intelligence +2 | No |
| **Ring of the Fox** — *A silver ring engraved with a running fox. Grants +2 Dexterity.* | Rare | Dexterity +2 | No |
| **Titan Ring** — *A massive stone ring worn by giants. Grants +3 Strength.* | Epic | Strength +3 | No |
| **Ring of Arcane Focus** — *A crystal ring pulsing with magical energy. Reduces spell mana cost.* | Rare | ManaCost -1 | No |
| **Ring of Shadows** — *A dark ring that drinks the light around it. +1 AC, +1 Stealth.* | Epic | ArmorClass +1 | No |
| **Cursed Ring of Greed** — *A glittering gold ring that feels warm to the touch. +2 Charisma but -2 Stamina from sleepless nights.* | Legendary | Charisma +2 | **Yes** — -2 Stamina, cannot be removed |

---

## 12. Amulets

| Amulet | Quality | Effect |
|--------|:-------:|--------|
| **Amulet of the Archon** — *A golden pendant bearing the crest of the celestial realm. +2 Wisdom, +1 Holy damage.* | Epic | Wisdom +2 |
| **Heartstone Pendant** — *A warm gem that pulses like a heartbeat. +20 Max HP, +1 Stamina.* | Rare | HitPoints +20 |
| **Dragon Tooth Amulet** — *A sharp fang from a young dragon, still humming with power. +1 Strength, +1 Fire Resist.* | Rare | Strength +1 |
| **Locket of Lost Souls** — *A black iron locket containing ash from the Shadowfell. +2 Intelligence, attracts undead.* | Epic | Intelligence +2 |
| **Silver Cross of Hope** — *A simple silver cross that glows faintly in darkness. +1 Wisdom, Fear Resistance.* | Uncommon | Wisdom +1 |

---

## 13. Girdles

| Girdle | Quality | Effect | Cursed? |
|--------|:-------:|--------|:-------:|
| **Girdle of Giant Strength** — *A thick leather belt woven from giant hair. Grants 18/00 Strength to any wearer.* | Legendary | Strength 18 | No |
| **Belt of the Ram** — *A bronze belt with a ram's head buckle. +2 Constitution, +1 Charge damage.* | Rare | Stamina +2 | No |
| **Sash of Shadows** — *A dark silk sash that blends into darkness. +1 Dexterity, +1 Stealth.* | Rare | Dexterity +1 | No |
| **Iron Buckle of Vigor** — *A simple iron buckle that fortifies the body. +1 Stamina, +5 Max HP.* | Uncommon | Stamina +1 | No |
| **Cursed Girdle of Weakness** — *An ornate golden belt that feels heavy. +3 Charisma but -3 Strength (drains your power).* | Legendary | Charisma +3 | **Yes** — -3 Strength, -1 max HP/day |

---

## 14. Item Sets

### Iron Sentinel
*A sturdy set of forged iron armor worn by the city watch of Eldergard. Grants unparalleled defense when worn together.*

| Pieces | Bonus |
|:------:|-------|
| 2 | +1 AC |
| 4 | +2 AC, +1 Strength |

**Items:** Knight's Honor (armor), Mariner's Plate (armor)

---

### Shadow Stalker
*Dark leather and chain worn by the Nightblades of the undercity. Enhances speed and stealth.*

| Pieces | Bonus |
|:------:|-------|
| 2 | +1 Dexterity |
| 3 | +2 Stealth, +1 Attack Bonus |

**Items:** Shadow Cloak (armor), Shadow Sting (weapon), Leather Armor (armor)

---

### Dragonborn Legacy
*Armor and weapons crafted from the remains of the Great Wyrm Igneel. Provides fire resistance and fury.*

| Pieces | Bonus |
|:------:|-------|
| 2 | Fire Resistance +10% |
| 3 | +2 Attack Bonus vs. Dragons |
| 5 | +3 Fire Damage on hit |

**Items:** Dragon Scale Mail (armor), Dragon's Fury (weapon)
