-- ============================================================
-- BattleArena - PostgreSQL Initialization Script
-- World: Homebrew AD&D-inspired fantasy
-- Schema: arena_data
-- Naming: snake_case tables/columns, fn_ functions, sp_ procs, p_ params
-- ============================================================

CREATE SCHEMA IF NOT EXISTS arena_data;

-- ============================================================
-- REFERENCE TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.die_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(10) NOT NULL UNIQUE,
    sides INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS arena_data.damage_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.attack_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.armor_category (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.affinity (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.gear_quality (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    sort_order INTEGER NOT NULL DEFAULT 5
);

CREATE TABLE IF NOT EXISTS arena_data.gear_slot (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.deity_alignment (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.spell_school (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS arena_data.equipment_slot (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- ============================================================
-- SEED: REFERENCE DATA
-- ============================================================

INSERT INTO arena_data.die_type (name, sides) VALUES
    ('D4', 4), ('D6', 6), ('D8', 8), ('D10', 10), ('D12', 12), ('D20', 20), ('D100', 100)
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.damage_type (name) VALUES
    ('Bludgeoning'), ('Piercing'), ('Slashing'), ('Poison'), ('Fire'),
    ('Ice'), ('Lightning'), ('Shadow'), ('Holy'), ('Acid')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.attack_type (name) VALUES
    ('Melee'), ('Ranged'), ('Spell')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.armor_category (name) VALUES
    ('Light'), ('Medium'), ('Heavy'), ('Shield')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.affinity (name) VALUES
    ('Spiritual'), ('Magical'), ('Forceful'), ('Chaos')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.gear_quality (name, sort_order) VALUES
    ('Legendary', 1), ('Epic', 2), ('Rare', 3), ('Uncommon', 4), ('Common', 5)
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.gear_slot (name) VALUES
    ('Helmet'), ('Chest'), ('Gauntlets'), ('Belt'), ('Ornament'),
    ('Foot'), ('RingLeft'), ('RingRight'), ('Amulet'), ('Banner'), ('Back')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.deity_alignment (name) VALUES
    ('Light'), ('Dark')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.spell_school (name) VALUES
    ('AoE'), ('CC'), ('Other')
ON CONFLICT (name) DO NOTHING;

INSERT INTO arena_data.equipment_slot (name) VALUES
    ('Head'), ('Chest'), ('Hands'), ('Waist'), ('Foot'),
    ('Neck'), ('Back'), ('RightHand'), ('LeftHand'), ('Banner'),
    ('Ring1'), ('Ring2'), ('Ornament')
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- RACES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.race (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    strength_bonus INTEGER NOT NULL DEFAULT 0,
    dexterity_bonus INTEGER NOT NULL DEFAULT 0,
    stamina_bonus INTEGER NOT NULL DEFAULT 0,
    intelligence_bonus INTEGER NOT NULL DEFAULT 0,
    wisdom_bonus INTEGER NOT NULL DEFAULT 0,
    charisma_bonus INTEGER NOT NULL DEFAULT 0,
    description TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS arena_data.subrace (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    description TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS arena_data.race_special_ability (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT DEFAULT ''
);

-- ============================================================
-- SEED: RACES
-- ============================================================

INSERT INTO arena_data.race (name, description, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus) VALUES
    ('Human',    'The children of the All-Father, humans are the most adaptable of the mortal races. From the barbarian hordes of the Frozen Wastes to the merchant princes of Eldergard, humanity''s ambition knows no bounds. Their settlements dot every corner of the realm, and their short lives burn twice as bright as the long-lived elves. No other race can match their versatility — a human may rise from peasant to king in a single lifetime.', 1, 1, 1, 1, 1, 1),
    ('Elf',      'Born from the tears of the Moon goddess, the elves are the eldest of the mortal races. Their connection to magic runs in their blood, granting them innate resistance to spells and a grace that other races find unsettling. High Elves study the arcane arts in crystal towers, Dark Elves weave shadows in the underdark, and Forest Elves move as whispers through the ancient woods. Elves measure time in centuries and rarely hurry.', 0, 2, 0, 2, 0, 1),
    ('Dwarf',    'Forged from the bones of the earth itself, dwarves are as stubborn as the mountains they call home. Their kingdoms stretch deep beneath the peaks, where they mine mithril and carve halls of breathtaking beauty. Dwarven smiths are unmatched in the mortal realm, and their resistance to magic makes them feared opponents. A dwarf''s word is their bond, and their grudges are recorded in stone to last ten generations.', 2, 0, 2, 0, 1, 0),
    ('Lizard',   'Scales shimmering like gemstones, the lizardfolk are the Silent Children of the Sun. They are descendants of the lesser dragons, evolved from those ancient bloodlines when the world was young. The draconic heritage runs deep in their veins — their scales, their resilience, their cold patience all echo the great wyrms. To outsiders they seem emotionless, but among their own kind they share deep bonds of loyalty. Swamp Lizards glide through poisonous marshes, Desert Lizards endure the searing heat, and Forest Lizards strike from the canopy with terrifying precision.', 2, 0, 1, 0, 0, 0),
    ('Undead',   'Not a race but a condition — souls that refused the call of the afterlife. Undead cannot be played as characters; they exist only as NPCs and monsters encountered in the world. Bound to their rotting bodies by sheer will or necromantic curse, they walk the mortal plane seeking purpose, vengeance, or redemption. Immune to fear and pain, they feel only the cold hunger of their existence. Some serve dark masters; others wander as lone penitents, searching for a peace that will not come.', 1, 0, 0, 1, 0, 0),
    ('Kobold',   'Small, scaly, and underestimated by every other race, kobolds are survivors. They dwell in the cracks of the world — forgotten mines, sewer networks, and the underbellies of great cities. Their natural cunning and magic resistance have kept them alive against larger, stronger foes. A kobold''s greatest weapon is not their claw or fang, but their cleverness. They build traps that would impress dwarves and tunnels that baffle even elves.', 0, 2, 0, 1, 0, 0),
    ('Demon',    'Hailing from the infernal planes beyond the mortal veil, demons are creatures of pure elemental chaos. Each demon embodies a primal force — fire demons burn with endless rage, shadow demons hunger for fear and despair. They enter the mortal world through rifts and summonings, bringing destruction in their wake. Yet some demons reject their nature, seeking redemption in a world that fears and despises them.', 2, 0, 1, 1, 0, 1),
    ('Orc',      'The Chosen of the War God, orcs were created to fight. Their muscles bulge with unnatural strength, and their bones knit faster than any other race. Orc society is built around the concept of ''Ushog'' — the eternal struggle that gives life meaning. They value strength above all else and respect only those who can defeat them in battle. Despite their savage reputation, orcish honor is absolute; an orc who gives their word will die before breaking it.', 3, 0, 1, 0, 0, 0),
    ('Ogre',     'Titans reduced by ages of separation from their divine ancestors, ogres are the largest of the mortal races. Standing twelve feet tall and built of solid muscle and thick bone, they are living battering rams. Mountain Ogres possess residual magic resistance from their giant bloodline, Hill Ogres throw boulders with deadly accuracy, Desert Ogres endure the harshest climates, and Forest Ogres can regenerate wounds at an alarming rate.', 3, 0, 2, 0, 0, 0),
    ('Halfling', 'The smallest of the civilized races, halflings possess a spirit that belies their stature. They believe in the power of luck, good food, and a warm hearth, yet they are among the bravest souls in battle. Halflings feel fear but refuse to show it, using their natural agility and sharp tongues to mock and taunt enemies into reckless charges. Forest Halflings move through woodland without a trace, while Hill Halflings are renowned for their hospitality and uncanny good fortune.', 0, 2, 1, 0, 1, 1);

-- Subraces
INSERT INTO arena_data.subrace (race_id, name, description)
SELECT r.id, s.name, s.descr
FROM (VALUES
    ('Elf', 'High Elf',    'Elves with innate spellcasting and keen intellect.'),
    ('Elf', 'Dark Elf',    'Drow who dwell underground with superior darkvision.'),
    ('Elf', 'Forest Elf',  'Wood elves who move unseen through natural terrain.'),
    ('Dwarf', 'Mountain Dwarf', 'Stout dwarves from the high peaks, expert metalworkers.'),
    ('Dwarf', 'Hill Dwarf',     'Dwarves of the rolling hills, known for endurance.'),
    ('Lizard', 'Swamp Lizard',  'Scaled hunters of the marshlands, immune to toxins.'),
    ('Lizard', 'Desert Lizard', 'Sun-scorched reptiles resistant to heat and sand.'),
    ('Lizard', 'Forest Lizard', 'Jungle-dwelling ambush predators with keen senses.'),
    ('Demon', 'Fire Demon', 'Infernals wreathed in hellflame, dealing fire damage.'),
    ('Demon', 'Shadow Demon', 'Dark stalkers who move through shadows and inflict fear.'),
    ('Orc', 'Green Orc',  'Jungle orcs with poisoned weapons and stealth.'),
    ('Orc', 'Blue Orc',   'Coastal raiders with unnatural strength.'),
    ('Orc', 'Red Orc',    'Mountain berserkers who fight in blood fury.'),
    ('Ogre', 'Mountain Ogre', 'Hill giants with natural magic resistance.'),
    ('Ogre', 'Hill Ogre',      'Boulder-throwing brutes of the lowlands.'),
    ('Ogre', 'Desert Ogre',    'Sun-hardened giants resistant to heat.'),
    ('Ogre', 'Forest Ogre',    'Troll-kin with regenerative properties.'),
    ('Halfling', 'Forest Halfling', 'Wood-wise halflings who disappear into foliage.'),
    ('Halfling', 'Hill Halfling',   'Pastoral folk known for luck and hospitality.')
) AS s(race_name, name, descr)
JOIN arena_data.race r ON r.name = s.race_name;

-- Race Special Abilities (SP)
INSERT INTO arena_data.race_special_ability (race_id, name, description)
SELECT r.id, s.name, s.descr
FROM (VALUES
    ('Elf', 'Magic Resistance',    'Advantage on saving throws against magical effects.'),
    ('Dwarf', 'Magic Resistance',  'Advantage on saving throws against magical effects.'),
    ('Lizard', 'Poison Immunity',  'Immune to poison damage and the poisoned condition.'),
    ('Undead', 'Fear Immunity',    'Immune to being frightened.'),
    ('Undead', 'Cause Fear',       'Attacks can cause fear in living opponents.'),
    ('Undead', 'Stun',             'Attacks have a chance to stun living targets.'),
    ('Kobold', 'Magic Resistance', 'Advantage on saving throws against magical effects.'),
    ('Demon', 'Cause Fear',        'Presence instills fear in weaker enemies.'),
    ('Demon', 'Stun',              'Infernal strikes can stun opponents.'),
    ('Orc', 'Extra Strength',      '+2 bonus to melee damage rolls.'),
    ('Ogre', 'Magic Resistance',   'Advantage on saving throws against magical effects.'),
    ('Ogre', 'Extra Strength',     '+2 bonus to melee damage rolls.'),
    ('Halfling', 'Taunt',          'Can force enemies to target them instead of allies.'),
    ('Halfling', 'Fear Immunity',  'Immune to being frightened.')
) AS s(race_name, name, descr)
JOIN arena_data.race r ON r.name = s.race_name;

-- ============================================================
-- CLASSES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.class (
    id SERIAL PRIMARY KEY,
    hit_die_id INTEGER NOT NULL REFERENCES arena_data.die_type(id),
    name VARCHAR(50) NOT NULL UNIQUE,
    base_strike_rating INTEGER NOT NULL DEFAULT 20,
    description TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS arena_data.class_race (
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id) ON DELETE CASCADE,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id) ON DELETE CASCADE,
    PRIMARY KEY (class_id, race_id)
);

-- ============================================================
-- SEED: CLASSES
-- ============================================================

INSERT INTO arena_data.class (name, description, hit_die_id, base_strike_rating)
	SELECT src.name, src.description, d.id, src.strike_rating
	FROM (VALUES
    ('Barbarian', 'Fierce warriors who channel rage into devastating attacks.',       'D12', 19),
    ('Knight',    'Armored cavaliers and champions of noble causes.',                 'D10', 18),
    ('Paladin',   'Holy warriors blessed by the gods with divine power.',             'D10', 18),
    ('Priest',    'Devoted servants who channel divine magic to heal and protect.',    'D8',  19),
    ('Mage',      'Masters of the arcane who wield devastating spells.',              'D4',  20),
    ('Bard',      'Musicians and storytellers who weave magic through performance.',   'D6',  19),
    ('Druid',     'Guardians of nature who command the elements and beasts.',         'D8',  19),
    ('Fighter',   'Weapons masters trained in all forms of combat.',                   'D10', 18),
    ('Rogue',     'Cunning infiltrators who strike from the shadows.',                'D6',  19)
) AS src(name, description, die_name, strike_rating)
JOIN arena_data.die_type d ON d.name = src.die_name;

-- Class-race restrictions per ideas.txt
INSERT INTO arena_data.class_race (class_id, race_id)
SELECT c.id, r.id
FROM (VALUES
    ('Barbarian', 'Human'),
    ('Knight', 'Human'),
    ('Paladin', 'Human'),
    ('Priest', 'Human'), ('Priest', 'Elf'), ('Priest', 'Dwarf'), ('Priest', 'Lizard'),
    ('Priest', 'Kobold'), ('Priest', 'Halfling'),
    ('Mage', 'Human'), ('Mage', 'Elf'),
    ('Bard', 'Human'),
    ('Druid', 'Elf'),
    ('Fighter', 'Elf'), ('Fighter', 'Dwarf'), ('Fighter', 'Lizard'),
    ('Fighter', 'Kobold'), ('Fighter', 'Orc'), ('Fighter', 'Ogre'), ('Fighter', 'Halfling'),
    ('Rogue', 'Elf'), ('Rogue', 'Halfling'), ('Rogue', 'Kobold')
) AS src(class_name, race_name)
JOIN arena_data.class c ON c.name = src.class_name
JOIN arena_data.race r ON r.name = src.race_name;

-- ============================================================
-- DEITIES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.deity (
    id SERIAL PRIMARY KEY,
    alignment_id INTEGER NOT NULL REFERENCES arena_data.deity_alignment(id),
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT DEFAULT '',
    domain VARCHAR(100) DEFAULT ''
);

INSERT INTO arena_data.deity (name, alignment_id, description, domain)
SELECT src.name, a.id, src.description, src.domain
FROM (VALUES
    ('Heaven',       'Light', 'The celestial realm of pure light and order.',       'Heaven, Light'),
    ('Star',         'Light', 'The guiding stars that illuminate fate.',            'Stars, Fate'),
    ('Constellations', 'Light', 'The woven patterns of destiny in the night sky.',  'Destiny, Time'),
    ('Moon',         'Light', 'The silver orb that governs tides and magic.',       'Moon, Magic, Tides'),
    ('Fire',         'Dark',  'The consuming flame of destruction and rebirth.',     'Fire, Destruction'),
    ('Darkness',     'Dark',  'The void from which all shadows are born.',           'Darkness, Secrets'),
    ('Smoke',        'Dark',  'The veil of deception and obscured truths.',          'Deception, Illusion'),
    ('Shadow',       'Dark',  'The realm between light and dark, home to assassins.','Shadow, Stealth')
) AS src(name, alignment_name, description, domain)
JOIN arena_data.deity_alignment a ON a.name = src.alignment_name;

-- ============================================================
-- PETS
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.pet (
    id SERIAL PRIMARY KEY,
    damage_die_id INTEGER REFERENCES arena_data.die_type(id),
    name VARCHAR(50) NOT NULL UNIQUE,
    armor_class INTEGER NOT NULL DEFAULT 10,
    hit_points INTEGER NOT NULL DEFAULT 10,
    description TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS arena_data.pet_class_restriction (
    pet_id INTEGER NOT NULL REFERENCES arena_data.pet(id) ON DELETE CASCADE,
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id) ON DELETE CASCADE,
    PRIMARY KEY (pet_id, class_id)
);

CREATE TABLE IF NOT EXISTS arena_data.pet_race_restriction (
    pet_id INTEGER NOT NULL REFERENCES arena_data.pet(id) ON DELETE CASCADE,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id) ON DELETE CASCADE,
    PRIMARY KEY (pet_id, race_id)
);

INSERT INTO arena_data.pet (name, description, damage_die_id, armor_class, hit_points)
SELECT src.name, src.description, d.id, src.ac, src.hp
FROM (VALUES
    ('Wolf',    'A loyal pack hunter with sharp fangs.',     'D6',  13, 18),
    ('Falcon',  'A swift bird of prey that strikes from above.', 'D4', 12, 8),
    ('Eagle',   'A majestic raptor with powerful talons.',   'D6',  13, 14),
    ('Hound',   'A trained war dog with a keen nose.',       'D6',  14, 22),
    ('Panther', 'A sleek black predator that hunts in darkness.', 'D8', 14, 26),
    ('Boar',    'A tusked beast with thick hide and fury.',  'D8',  15, 30),
    ('Dragon',  'A young dragon bound to its master.',       'D10', 17, 50),
    ('Bat',     'A swarm of cave bats that confuse enemies.','D4',  10, 6),
    ('Spider',  'A venomous arachnid that ensnares prey.',   'D6',  12, 12)
) AS src(name, description, die_name, ac, hp)
JOIN arena_data.die_type d ON d.name = src.die_name;

-- Pet class restrictions
INSERT INTO arena_data.pet_class_restriction (pet_id, class_id)
SELECT p.id, c.id
FROM (VALUES
    ('Wolf', 'Paladin'), ('Wolf', 'Fighter'), ('Wolf', 'Barbarian'),
    ('Falcon', 'Paladin'), ('Falcon', 'Fighter'), ('Falcon', 'Barbarian'),
    ('Eagle', 'Paladin'), ('Eagle', 'Fighter'), ('Eagle', 'Barbarian'),
    ('Hound', 'Paladin'), ('Hound', 'Fighter'), ('Hound', 'Barbarian'),
    ('Panther', 'Rogue'),
    ('Boar', 'Fighter'), ('Boar', 'Barbarian'),
    ('Dragon', 'Mage'), ('Dragon', 'Paladin'), ('Dragon', 'Fighter'), ('Dragon', 'Barbarian'),
    ('Bat', 'Priest'), ('Bat', 'Mage'),
    ('Spider', 'Priest'), ('Spider', 'Mage')
) AS src(pet_name, class_name)
JOIN arena_data.pet p ON p.name = src.pet_name
JOIN arena_data.class c ON c.name = src.class_name;

-- Pet race restrictions (Undead get bats/spiders; Dragon only for Elf/Human)
INSERT INTO arena_data.pet_race_restriction (pet_id, race_id)
SELECT p.id, r.id
FROM (VALUES
    ('Bat', 'Undead'),
    ('Spider', 'Undead'),
    ('Dragon', 'Elf'),
    ('Dragon', 'Human')
) AS src(pet_name, race_name)
JOIN arena_data.pet p ON p.name = src.pet_name
JOIN arena_data.race r ON r.name = src.race_name;

-- ============================================================
-- WEAPONS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.weapon_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT DEFAULT ''
);

INSERT INTO arena_data.weapon_type (name, description) VALUES
    ('Hammer',     'One-handed or two-handed crushing weapon.'),
    ('Axe',        'One-handed, two-handed, or dual-wield slashing weapon.'),
    ('Sword',      'One-handed, two-handed, or dual-wield blade.'),
    ('Bow',        'Ranged weapon firing arrows over distance.'),
    ('Crossbow',   'Mechanical ranged weapon with high penetration.'),
    ('Staff',      'Two-handed wooden pole, often used by spellcasters.'),
    ('Wand',       'A short magical conduit for spell focus.'),
    ('Dagger',     'Small concealed blade for close-quarters stabbing.'),
    ('ShortSword', 'A quick blade shorter than a full sword.'),
    ('Mace',       'A blunt one-handed club with a heavy head.'),
    ('MorningStar','A spiked ball on a chain attached to a handle.'),
    ('Lance',      'A long spear used from horseback.'),
    ('Spear',      'A versatile polearm for thrusting or throwing.')
ON CONFLICT (name) DO NOTHING;

CREATE TABLE IF NOT EXISTS arena_data.weapon (
    id SERIAL PRIMARY KEY,
    weapon_type_id INTEGER NOT NULL REFERENCES arena_data.weapon_type(id),
    damage_die_id INTEGER NOT NULL REFERENCES arena_data.die_type(id),
    damage_type_id INTEGER NOT NULL REFERENCES arena_data.damage_type(id),
    attack_type_id INTEGER NOT NULL REFERENCES arena_data.attack_type(id),
    gear_quality_id INTEGER NOT NULL DEFAULT 5 REFERENCES arena_data.gear_quality(id),
    set_id INTEGER DEFAULT NULL,
    name VARCHAR(100) NOT NULL,
    damage_count INTEGER NOT NULL DEFAULT 1,
    hands INTEGER NOT NULL DEFAULT 1,
    attack_bonus INTEGER NOT NULL DEFAULT 0,
    cursed BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT DEFAULT '',
    curse_effect TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id
FROM (VALUES
    ('Hand Axe',         'A weathered throwing axe passed down through generations of border scouts. The leather grip bears the brand of the Northern Watch.',   'Axe',        'D6', 'Slashing',    'Melee',  1, 1, 'Common'),
    ('Battle Axe',       'Forged for the line-breakers of the Iron Company. Each swing carries the weight of a hundred battles fought in the mountain passes.',    'Axe',        'D8', 'Slashing',    'Melee',  1, 2, 'Common'),
    ('Short Sword',      'The preferred blade of city guards and sellswords. Quick, reliable, and easy to maintain in the field.',                                'ShortSword', 'D6', 'Piercing',    'Melee',  1, 1, 'Common'),
    ('Long Sword',       'The knight''s companion. Balanced for cut and thrust, this blade has been the weapon of warriors across every kingdom for a thousand years.','Sword', 'D8', 'Slashing', 'Melee',  1, 1, 'Common'),
    ('Great Sword',      'A towering blade requiring both hands and the strength of three men. Favored by executioners and elite shock troops of the Crimson Legion.','Sword','D10','Slashing', 'Melee',  1, 2, 'Common'),
    ('War Hammer',       'Its head forged in the shape of a ram''s skull by the smiths of the Stonepeak clan. Each blow lands like a battering ram against fortifications.','Hammer','D8','Bludgeoning','Melee',1,1,'Common'),
    ('Maul',             'A weapon that does not cut or pierce — it simply destroys whatever it hits. The tool of temple guardians and ogre-killers.',               'Hammer',     'D10','Bludgeoning','Melee',1,2,'Common'),
    ('Dagger',           'Small enough to hide in a boot or up a sleeve. Every adventurer carries one, and every assassin has used one.',                            'Dagger',     'D4', 'Piercing',   'Melee', 1, 1, 'Common'),
    ('Mace',             'The simplest of weapons — a weighted head on a wooden shaft. It crushes armor where a blade would turn aside.',                             'Mace',       'D6', 'Bludgeoning','Melee',1,1,'Common'),
    ('Morning Star',     'A spiked iron ball on a short chain. Once carried by the cavalry of the fallen Kingdom of Ashvale before its fall to the demon horde.',   'MorningStar','D8', 'Piercing',   'Melee',1,1,'Common'),
    ('Lance',            'The thunder of a cavalry charge is the sound of lances lowering. Few things in battle match the terror of knights at full gallop.',       'Lance',      'D10','Piercing',   'Melee',1,2,'Common'),
    ('Spear',            'The oldest weapon of mortal kind. Easy to learn, hard to master, deadly in disciplined formation.',                                       'Spear',      'D6', 'Piercing',   'Melee',1,1,'Common'),
    ('Quarter Staff',    'A length of hardened ironwood favored by travelers, monks, and those who prefer discretion over steel.',                                   'Staff',      'D6', 'Bludgeoning','Melee',1,2,'Common'),
    ('Wand',             'A slender focus rod of enchanted elm. Its tip glows faintly when magic surges through it. Used by hedge wizards and court mages alike.',  'Wand',       'D4', 'Piercing',   'Spell',1,1,'Common'),
    ('Short Bow',        'A curved bow carved from yew and horn by the bowyers of the Green Valley. Hunters across the realm rely on its steady pull.',             'Bow',        'D6', 'Piercing',   'Ranged',1,2,'Common'),
    ('Long Bow',         'The signature weapon of the Eldergard Rangers. Its range and stopping power are whispered about in every tavern from here to the coast.',  'Bow',        'D8', 'Piercing',   'Ranged',1,2,'Common'),
    ('Light Crossbow',   'A mechanical bow that can be fired one-handed while prone. The favored tool of tunnel fighters and castle defenders.',                     'Crossbow',   'D6', 'Piercing',   'Ranged',1,2,'Common'),
    ('Heavy Crossbow',   'A miniature siege engine. Its bolts punch through plate armor at two hundred paces. Requires a crank and steady nerves.',                'Crossbow',   'D10','Piercing',   'Ranged',1,2,'Common'),
    -- Epic weapons
    ('Bone Crusher',     'A brutal mace carved from the femur of a hill giant by the shaman of the Thunder Ridge tribe. It shatters armor and bone into dust. The handle is wrapped in the hide of the shaman''s first kill.',
                                                                                                                        'Mace',       'D8', 'Bludgeoning','Melee',1,1,'Epic'),
    ('Wind Cutter',      'A slender long sword balanced to perfection. Forged by the wind elf smith Aeloril, who spent a century shaping its edge. It sings as it cuts through air.',
                                                                                                                        'Sword',      'D8', 'Slashing',   'Melee',1,1,'Epic'),
    ('Viper Fang',       'A curved assassin''s dagger coated in a venom that never dries. The fang of the great serpent Sythiss was hollowed and set into a hilt of obsidian.',
                                                                                                                        'Dagger',     'D6', 'Poison',     'Melee',1,1,'Epic'),
    -- Legendary weapons
    ('Soul Reaver',      'A massive black blade forged in the Abyss from a dying star. It drinks the souls of the fallen and whispers their final screams to its wielder. +3 attack bonus.',
                                                                                                                        'Sword',      'D12','Slashing',   'Melee',1,2,'Legendary'),
    ('Stormbringer',     'A crackling lance charged with the fury of a primordial storm. When the wielder charges, thunder shakes the earth and lightning arcs from the tip. +2 attack bonus.',
                                                                                                                        'Lance',      'D12','Lightning',  'Melee',1,2,'Legendary'),
    ('Dragon''s Fury',   'A flaming battle axe forged from the fang of the Great Wyrm Igneel. The blade burns eternally, and its wounds cauterize as they are made. +2 attack bonus.',
                                                                                                                        'Axe',        'D10','Fire',       'Melee',1,2,'Legendary'),
    ('Shadow Sting',     'A dagger that exists partly in the material plane and partly in the Shadowfell. It phases through armor to strike the soul directly. +2 attack bonus.',
                                                                                                                        'Dagger',     'D6', 'Shadow',     'Melee',1,1,'Legendary'),
    ('Frostbite',        'A short sword blessed by the Frost Queen of the Northern Wastes. Eternal ice coats the blade, slowing victims and leaving frozen wounds. +2 attack bonus.',
                                                                                                                        'ShortSword', 'D8', 'Ice',        'Melee',1,1,'Legendary'),
    ('Sun''s Wrath',     'A morning star that glows with the light of dawn. Forged by the priests of the Sun God, it sears the undead and burns away darkness. +2 attack bonus.',
                                                                                                                        'MorningStar','D10','Holy',       'Melee',1,1,'Legendary')
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Set attack bonuses
UPDATE arena_data.weapon SET attack_bonus = 3 WHERE name = 'Soul Reaver';
UPDATE arena_data.weapon SET attack_bonus = 2 WHERE name IN ('Stormbringer', 'Dragon''s Fury', 'Shadow Sting', 'Frostbite', 'Sun''s Wrath');
UPDATE arena_data.weapon SET attack_bonus = 1 WHERE name IN ('Bone Crusher', 'Wind Cutter', 'Viper Fang');

-- ============================================================
-- CURSED WEAPONS
-- ============================================================

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus, cursed, curse_effect)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus, TRUE, src.curse
FROM (VALUES
    ('Blood Drinker',    'A long sword with a red vein running through the steel. It hungers for blood and grants its wielder unnatural strength, but it feeds on the wielder''s life force with every swing.',
                                                                                                    'Sword', 'D10', 'Slashing', 'Melee', 1, 1, 'Epic', 2, '-1 HP per successful hit'),
    ('Witchwood Staff',  'A gnarled staff of living black wood that writhes in the hand. It amplifies dark magic but slowly poisons the user''s mind with whispers from the void.',
                                                                                                    'Staff', 'D8',  'Shadow',   'Spell', 1, 2, 'Epic', 1, '-1 Wisdom per day held'),
    ('Soul Prison',      'A mace forged from the bars of a broken cage that held a hundred souls. It hits like an avalanche and traps a fragment of each victim''s spirit, but the trapped souls scream constantly in the wielder''s mind.',
                                                                                                    'Mace',  'D10', 'Bludgeoning','Melee', 1, 1, 'Legendary', 3, '-1 Stamina per day, chance to be stunned by screams'),
    ('Serpent''s Fang',  'A spear tipped with the fang of the World Serpent. Venom drips eternally from the tip. It is deadly to enemies — and occasionally to its wielder.',
                                                                                                    'Spear', 'D8',  'Piercing', 'Melee', 1, 2, 'Rare', 1, '10% chance to poison self on critical miss')
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus, curse)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- RARE / HEIRLOOM WEAPONS
-- ============================================================

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus
FROM (VALUES
    ('Father''s Mercy',   'A well-worn long sword passed down through five generations of the Samek family. The leather grip is molded to the hand of the original owner, and the blade bears the scratches of a hundred battles. It may not be magical, but it has never let its wielder down.',
                                                                                        'Sword', 'D8', 'Slashing', 'Melee', 1, 1, 'Uncommon', 0),
    ('The Last Argument', 'A morning star forged from the chains of a slave galley by a freed prisoner named Harvoth. Each of the six spikes represents a year of servitude. Harvoth vowed it would be his last argument in any dispute.',
                                                                                        'MorningStar', 'D8', 'Piercing', 'Melee', 1, 1, 'Rare', 1),
    ('Wolf''s Bane',      'A spear originally crafted by the ranger Aldric to hunt the dire wolves that terrorized his village. The shaft is wrapped in silver wire and the obsidian tip has never dulled. It has claimed the lives of seven alpha wolves.',
                                                                                        'Spear', 'D6', 'Piercing', 'Melee', 1, 1, 'Rare', 0),
    ('Oathkeeper',        'A blade broken and reforged three times, each by a different smith across three generations. Its current form is simple, unbreakable, and sharp — much like the oath it was forged to represent.',
                                                                                        'Sword', 'D8', 'Slashing', 'Melee', 1, 1, 'Rare', 0),
    ('Barrow Bow',        'A short bow carved from the root of a tree that grew through an ancient barrow. The wood remembers the dead and guides arrows toward the vital spots of the living.',
                                                                                        'Bow',   'D6', 'Piercing', 'Ranged', 1, 2, 'Rare', 1),
    ('Final Toll',        'A hand axe carried by the bell-ringer of the Temple of Passing. He used it to defend the temple during the Sack of Eldergard. The axe still rings like a bell when it strikes.',
                                                                                        'Axe',   'D6', 'Slashing', 'Melee', 1, 1, 'Uncommon', 0)
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- ARMOR
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.armor (
    id SERIAL PRIMARY KEY,
    armor_category_id INTEGER NOT NULL REFERENCES arena_data.armor_category(id),
    gear_quality_id INTEGER NOT NULL DEFAULT 5 REFERENCES arena_data.gear_quality(id),
    set_id INTEGER DEFAULT NULL,
    name VARCHAR(100) NOT NULL,
    armor_class INTEGER NOT NULL,
    max_dexterity_bonus INTEGER NOT NULL DEFAULT 0,
    stealth_disadvantage BOOLEAN NOT NULL DEFAULT FALSE,
    strength_requirement INTEGER NOT NULL DEFAULT 0,
    armor_class_bonus INTEGER NOT NULL DEFAULT 0,
    cursed BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT DEFAULT '',
    curse_effect TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id
FROM (VALUES
    ('Shield',           'A wooden shield branded with the crest of the City Watch. It has stopped arrows, blades, and a charging boar. The paint is chipped from a dozen battles.',
                                                                                                   2,  'Shield', 0,  FALSE, 0, 'Common'),
    ('Padded Armor',     'Quilted cloth stuffed with raw wool and straw. Better than nothing, but only barely. Worn by militia conscripts and desperate peasants.',
                                                                                                   11, 'Light',  99, TRUE,  0, 'Common'),
    ('Leather Armor',    'Treated leather boiled in wax and shaped to the body. Worn by scouts, highwaymen, and rangers who value mobility over raw protection.',
                                                                                                   11, 'Light',  99, FALSE, 0, 'Common'),
    ('Studded Leather',  'Leather reinforced with hundreds of iron rivets. A favorite among city guards who patrol the dangerous dock districts at night.',
                                                                                                   12, 'Light',  99, FALSE, 0, 'Common'),
    ('Hide Armor',       'The stripped hide of a cave bear, crudely cured over a campfire. Primitive but effective. Worn by the barbarian tribes of the Frozen Wastes.',
                                                                                                   12, 'Medium', 2,  FALSE, 0, 'Common'),
    ('Chain Shirt',      'A shirt of interlocking rings that jingles with every step. The minimum standard for any professional soldier in the Eldergard army.',
                                                                                                   13, 'Medium', 2,  FALSE, 0, 'Common'),
    ('Scale Mail',       'Overlapping iron plates sewn onto a leather backing resembling dragon scales. Provides excellent protection against slashing attacks. Worn by dragon-hunters of the Burning Plains.',
                                                                                                   14, 'Medium', 2,  TRUE,  0, 'Common'),
    ('Breastplate',      'A polished steel breastplate engraved with the wearer''s family crest. Favored by officers who need protection without sacrificing mobility.',
                                                                                                   14, 'Medium', 2,  FALSE, 0, 'Common'),
    ('Half Plate',       'Partial plate armor covering the vital areas while leaving joints exposed for mobility. A grim compromise between protection and speed.',
                                                                                                   15, 'Medium', 2,  TRUE,  0, 'Common'),
    ('Ring Mail',        'Leather armor with heavy iron rings sewn across the surface. An old design still used by frontier garrisons who cannot afford better.',
                                                                                                   14, 'Heavy',  0,  TRUE,  0, 'Common'),
    ('Chain Mail',       'A full hauberk of interlocking rings reaching to the knees. Heavy, noisy, but nearly impervious to slashing weapons. Standard issue for the Iron Company.',
                                                                                                   16, 'Heavy',  0,  TRUE,  13, 'Common'),
    ('Splint Armor',     'Vertical steel strips riveted to a sturdy leather backing. An affordable alternative to full plate, favored by veteran mercenaries of the Free Companies.',
                                                                                                   17, 'Heavy',  0,  TRUE,  15, 'Common'),
    ('Plate Armor',      'The pinnacle of mortal armor craft. Articulated steel plates covering every inch of the body. Only knights and wealthy lords can afford it.',
                                                                                                   18, 'Heavy',  0,  TRUE,  15, 'Common'),
    -- Epic armor
    ('Knight''s Honor',  'Ceremonial splint armor blessed by the priests of Heaven. Its enameled surface depicts the Battle of the Silver Plains in exquisite detail.',
                                                                                                   17, 'Heavy',  0, TRUE,  15, 'Epic'),
    ('Mithril Chain',    'A shimmering chain shirt forged from mithril, the lightest metal known to dwarven craft. It flows like silk but protects like steel.',
                                                                                                   14, 'Medium', 99, FALSE, 0,  'Epic'),
    -- Legendary armor
    ('Titan Plate',      'Colossal plate forged in the heart of Mount Kryx by ancient giant smiths. It could withstand a direct hit from a god''s hammer. +2 AC.',
                                                                                                   18, 'Heavy',  0, TRUE,  18, 'Legendary'),
    ('Dragon Scale Mail','Armor woven from hundreds of indestructible dragon scales from the Great Wyrm Igneel. The scales still retain their fire resistance.',
                                                                                                   15, 'Medium', 2, FALSE, 0, 'Legendary'),
    ('Shadow Cloak',     'A cloak woven from the fabric of twilight itself. It drifts and shifts of its own accord, causing enemy attacks to miss at the last instant.',
                                                                                                   12, 'Light',  99, FALSE, 0, 'Legendary')
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Set armor_class_bonus for quality items
UPDATE arena_data.armor SET armor_class_bonus = 2 WHERE name = 'Titan Plate';
UPDATE arena_data.armor SET armor_class_bonus = 1 WHERE name IN ('Dragon Scale Mail', 'Shadow Cloak');
UPDATE arena_data.armor SET armor_class_bonus = 1 WHERE name IN ('Knight''s Honor', 'Mithril Chain');

-- ============================================================
-- CURSED ARMOR
-- ============================================================

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus, cursed, curse_effect)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus, TRUE, src.curse
FROM (VALUES
    ('Binding Chains',   'Chain mail forged from the actual chains of a prison ship that sank with a hundred souls aboard. The damned still cling to it, dragging on the wearer. Grants protection but weighs on the spirit.',
                                                                                   16, 'Heavy', 0, TRUE, 13, 'Epic', 2, '-2 Dexterity, cannot remove without Remove Curse spell'),
    ('Mask of the Betrayer','A full helm of black steel with no eyeholes — yet the wearer sees perfectly through it. Sometimes. The helm shows its bearer visions of their greatest betrayal at the worst possible moment.',
                                                                                   1,  'Light', 99, FALSE, 0, 'Rare', 1, 'Occasional hallucination of betrayal (-2 on next save)'),
    ('Widow''s Embrace',  'A beautiful silver breastplate that once belonged to a queen who watched her entire kingdom fall. It protects the body but fills the heart with grief.',
                                                                                   14, 'Medium', 2, FALSE, 0, 'Rare', 1, '-1 Charisma, wearer weeps uncontrollably during battle')
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus, curse)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- RARE / HEIRLOOM ARMOR
-- ============================================================

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus
FROM (VALUES
    ('Forest Warden''s Coat','Studded leather grown from a living treant sapling by the druids of the Deepwood. It breathes, repairs itself over time, and never impedes movement.',
                                                                                   13, 'Light', 99, FALSE, 0, 'Rare', 1),
    ('Mariner''s Plate',  'Rust-proof plate armor inlaid with coral from the Sunken Kingdoms. It grants the wearer the ability to breathe water and move freely underwater.',
                                                                                   18, 'Heavy', 0, TRUE, 15, 'Rare', 1),
    ('Watchman''s Shield','A well-worn shield with a fist-sized dent — the result of stopping a boulder during the Siege of Ironwall. The watchman who carried it saved a dozen lives that day.',
                                                                                   2,  'Shield', 0, FALSE, 0, 'Rare', 1)
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Set item set associations (Deity alignments already seeded above)
-- Iron Sentinel: Watchman's Shield, Knight's Honor, Mariner's Plate
-- Shadow Stalker: Shadow Cloak, Shadow Sting, Leather Armor
-- Dragonborn Legacy: Dragon Scale Mail, Dragon's Fury

CREATE TABLE IF NOT EXISTS arena_data.item_set (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS arena_data.set_bonus (
    id SERIAL PRIMARY KEY,
    set_id INTEGER NOT NULL REFERENCES arena_data.item_set(id) ON DELETE CASCADE,
    pieces_required INTEGER NOT NULL CHECK (pieces_required >= 2),
    effect_description TEXT NOT NULL DEFAULT ''
);

-- Seed: Item Sets
INSERT INTO arena_data.item_set (name, description) VALUES
    ('Iron Sentinel', 'A sturdy set of forged iron armor worn by the city watch of Eldergard. Grants unparalleled defense when worn together.'),
    ('Shadow Stalker', 'Dark leather and chain worn by the Nightblades of the undercity. Enhances speed and stealth.'),
    ('Dragonborn Legacy', 'Armor and weapons crafted from the remains of the Great Wyrm Igneel. Provides fire resistance and fury.')
ON CONFLICT (name) DO NOTHING;

-- Set bonuses
INSERT INTO arena_data.set_bonus (set_id, pieces_required, effect_description)
SELECT s.id, src.pieces, src.effect
FROM (VALUES
    ('Iron Sentinel', 2, '+1 AC bonus'),
    ('Iron Sentinel', 4, '+2 AC bonus, +1 Strength'),
    ('Shadow Stalker', 2, '+1 Dexterity'),
    ('Shadow Stalker', 3, '+2 Stealth, +1 Attack Bonus'),
    ('Dragonborn Legacy', 2, 'Fire Resistance +10%'),
    ('Dragonborn Legacy', 3, '+2 Attack Bonus vs. Dragons'),
    ('Dragonborn Legacy', 5, '+3 Fire Damage on hit')
) AS src(set_name, pieces, effect)
JOIN arena_data.item_set s ON s.name = src.set_name;

-- Link weapons & armor to item sets
UPDATE arena_data.armor SET set_id = (SELECT id FROM arena_data.item_set WHERE name = 'Iron Sentinel') WHERE name IN ('Knight''s Honor', 'Mariner''s Plate');
UPDATE arena_data.armor SET set_id = (SELECT id FROM arena_data.item_set WHERE name = 'Shadow Stalker') WHERE name IN ('Shadow Cloak', 'Leather Armor');
UPDATE arena_data.armor SET set_id = (SELECT id FROM arena_data.item_set WHERE name = 'Dragonborn Legacy') WHERE name IN ('Dragon Scale Mail');
UPDATE arena_data.weapon SET set_id = (SELECT id FROM arena_data.item_set WHERE name = 'Shadow Stalker') WHERE name IN ('Shadow Sting');
UPDATE arena_data.weapon SET set_id = (SELECT id FROM arena_data.item_set WHERE name = 'Dragonborn Legacy') WHERE name IN ('Dragon''s Fury');

-- ============================================================
-- ACCESSORIES (Rings, Amulets, Girdles)
-- Normalised: one reference table for type, one data table for all entries.
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.accessory_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO arena_data.accessory_type (name) VALUES
    ('Ring'), ('Amulet'), ('Girdle')
ON CONFLICT (name) DO NOTHING;

CREATE TABLE IF NOT EXISTS arena_data.accessory (
    id SERIAL PRIMARY KEY,
    accessory_type_id INTEGER NOT NULL REFERENCES arena_data.accessory_type(id),
    gear_quality_id   INTEGER NOT NULL DEFAULT 5 REFERENCES arena_data.gear_quality(id),
    name              VARCHAR(100) NOT NULL UNIQUE,
    effect_type       VARCHAR(50)  NOT NULL DEFAULT 'none',
    effect_value      INTEGER      NOT NULL DEFAULT 0,
    cursed            BOOLEAN      NOT NULL DEFAULT FALSE,
    description       TEXT DEFAULT '',
    curse_effect      TEXT DEFAULT ''
);

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    -- Rings
    ('Band of the Bull',          'A thick iron band etched with a charging bull. Grants +2 Strength.',                                                          'Ring',   'Rare',      'Strength',      2, FALSE, ''),
    ('Serpent Ring',              'A coiled jade serpent that sharpens the mind. Grants +2 Intelligence.',                                                        'Ring',   'Rare',      'Intelligence',  2, FALSE, ''),
    ('Ring of the Fox',           'A silver ring engraved with a running fox. Grants +2 Dexterity.',                                                              'Ring',   'Rare',      'Dexterity',     2, FALSE, ''),
    ('Titan Ring',                'A massive stone ring worn by giants. Grants +3 Strength.',                                                                     'Ring',   'Epic',      'Strength',      3, FALSE, ''),
    ('Ring of Arcane Focus',      'A crystal ring pulsing with magical energy. Reduces spell mana cost.',                                                         'Ring',   'Rare',      'ManaCost',     -1, FALSE, ''),
    ('Ring of Shadows',           'A dark ring that drinks the light around it. +1 AC, +1 Stealth.',                                                              'Ring',   'Epic',      'ArmorClass',    1, FALSE, ''),
    ('Cursed Ring of Greed',      'A glittering gold ring that feels warm to the touch. +2 Charisma but -2 Stamina from sleepless nights.',                       'Ring',   'Legendary', 'Charisma',      2, TRUE,  '-2 Stamina, cannot be removed'),
    -- Amulets
    ('Amulet of the Archon',      'A golden pendant bearing the crest of the celestial realm. +2 Wisdom, +1 Holy damage.',                                       'Amulet', 'Epic',      'Wisdom',        2, FALSE, ''),
    ('Heartstone Pendant',        'A warm gem that pulses like a heartbeat. +20 Max HP, +1 Stamina.',                                                             'Amulet', 'Rare',      'HitPoints',    20, FALSE, ''),
    ('Dragon Tooth Amulet',       'A sharp fang from a young dragon, still humming with power. +1 Strength, +1 Fire Resist.',                                    'Amulet', 'Rare',      'Strength',      1, FALSE, ''),
    ('Locket of Lost Souls',      'A black iron locket containing ash from the Shadowfell. +2 Intelligence, attracts undead.',                                    'Amulet', 'Epic',      'Intelligence',  2, FALSE, ''),
    ('Silver Cross of Hope',      'A simple silver cross that glows faintly in darkness. +1 Wisdom, Fear Resistance.',                                            'Amulet', 'Uncommon',  'Wisdom',        1, FALSE, ''),
    -- Girdles
    ('Girdle of Giant Strength',  'A thick leather belt woven from giant hair. Grants 18/00 Strength to any wearer.',                                             'Girdle', 'Legendary', 'Strength',     18, FALSE, ''),
    ('Belt of the Ram',           'A bronze belt with a ram''s head buckle. +2 Constitution, +1 Charge damage.',                                                 'Girdle', 'Rare',      'Stamina',       2, FALSE, ''),
    ('Sash of Shadows',           'A dark silk sash that blends into darkness. +1 Dexterity, +1 Stealth.',                                                        'Girdle', 'Rare',      'Dexterity',     1, FALSE, ''),
    ('Iron Buckle of Vigor',      'A simple iron buckle that fortifies the body. +1 Stamina, +5 Max HP.',                                                         'Girdle', 'Uncommon',  'Stamina',       1, FALSE, ''),
    ('Cursed Girdle of Weakness', 'An ornate golden belt that feels heavy. +3 Charisma but -3 Strength (drains your power).',                                    'Girdle', 'Legendary', 'Charisma',      3, TRUE,  '-3 Strength, -1 max HP per day worn')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality    gq    ON gq.name    = src.quality_name;

-- ============================================================
-- NPC CHARACTERS
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.npc (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id),
    name VARCHAR(100) NOT NULL,
    level INTEGER NOT NULL DEFAULT 1,
    strength INTEGER NOT NULL DEFAULT 10,
    dexterity INTEGER NOT NULL DEFAULT 10,
    stamina INTEGER NOT NULL DEFAULT 10,
    intelligence INTEGER NOT NULL DEFAULT 10,
    wisdom INTEGER NOT NULL DEFAULT 10,
    charisma INTEGER NOT NULL DEFAULT 10,
    is_merchant BOOLEAN NOT NULL DEFAULT FALSE,
    is_quest_giver BOOLEAN NOT NULL DEFAULT FALSE,
    is_hostile BOOLEAN NOT NULL DEFAULT FALSE,
    biography TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Old Man Kael',    r.id, c.id, 8, 8, 10, 12, 16, 18, 14, FALSE, TRUE, FALSE,
       'A blind seer who speaks in riddles. He knows the location of the Sun''s Wrath and will trade the knowledge for a vial of dragon''s blood.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Human' AND c.name = 'Priest';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Greta Ironhand',  r.id, c.id, 12, 16, 12, 18, 10, 12, 10, TRUE, FALSE, FALSE,
       'A dwarf smith who forged weapons for three kings. She keeps the Soul Reaver hidden beneath her forge, waiting for a worthy champion. She buys and sells all weapons and armor.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Dwarf' AND c.name = 'Fighter';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Shadowmere',      r.id, c.id, 10, 14, 20, 12, 14, 16, 18, FALSE, TRUE, FALSE,
       'The leader of the Nightblades guild. She offers membership to those who prove their worth by retrieving the Shadow Sting from the Crypt of Whispers.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Elf' AND c.name = 'Rogue';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Korg Stonefist',  r.id, c.id, 15, 20, 8, 20, 6, 8, 7, FALSE, FALSE, TRUE,
       'A wandering orc berserker who challenges all who cross his path. Wields a massive maul and wears cursed plate that feeds on his pain.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Orc' AND c.name = 'Barbarian';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Merchant Vex',    r.id, c.id, 6, 10, 14, 10, 16, 12, 16, TRUE, FALSE, FALSE,
       'A kobold trader with a cart full of "authentic" artifacts. Most are fakes, but occasionally she comes across a real treasure. She sells rings, amulets, and girdles.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Kobold' AND c.name = 'Rogue';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'High Priestess Luna', r.id, c.id, 14, 10, 12, 14, 16, 20, 18, FALSE, TRUE, FALSE,
       'The head of the Moon temple. She bestows the Amulet of the Archon upon those who complete the pilgrimage to the Moonlit Peak.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Human' AND c.name = 'Priest';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Graveworm',       r.id, c.id, 9, 14, 14, 16, 10, 10, 6, FALSE, FALSE, TRUE,
       'An undead warlord who commands a legion of skeletons in the Bone Fields. He carries Frostbite, the blade that killed him centuries ago.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Undead' AND c.name = 'Fighter';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Lysander the Bard', r.id, c.id, 7, 10, 14, 12, 14, 12, 20, FALSE, TRUE, FALSE,
       'A halfling bard who knows every legend, song, and secret in the realm. He can reveal the location of any legendary item for a price.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Halfling' AND c.name = 'Bard';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Infernal Commander Zoth', r.id, c.id, 18, 22, 14, 20, 16, 14, 16, FALSE, FALSE, TRUE,
       'A demon lord commanding the legions of the Fire Pits. He wields the Stormbringer lance and rides a nightmare steed. The final boss of the Burning Plains.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Demon' AND c.name = 'Knight';

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Elder Treant',    r.id, c.id, 20, 20, 8, 22, 14, 20, 14, FALSE, TRUE, FALSE,
       'An ancient treant awakened by the druids of the Deepwood. He grants the Dragon Scale Mail to those who prove they can protect the forest.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Elf' AND c.name = 'Druid';

-- ============================================================
-- SPELLS
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.spell (
    id SERIAL PRIMARY KEY,
    school_id INTEGER NOT NULL REFERENCES arena_data.spell_school(id),
    damage_die_id INTEGER REFERENCES arena_data.die_type(id),
    damage_type_id INTEGER REFERENCES arena_data.damage_type(id),
    name VARCHAR(100) NOT NULL UNIQUE,
    mana_cost INTEGER NOT NULL DEFAULT 5,
    description TEXT DEFAULT ''
);

INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, mana_cost)
SELECT src.name, src.description, s.id, d.id, dt.id, src.mana
FROM (VALUES
    ('Blade Barrier',    'A wall of spinning blades.',                 'AoE', 'D8',  'Slashing',    10),
    ('Ice Storm',        'Hail and ice pummel the area.',              'AoE', 'D8',  'Ice',         8),
    ('Fire Storm',       'A conflagration engulfs the area.',          'AoE', 'D10', 'Fire',        12),
    ('Acid Rain',        'Corrosive rain burns all in the area.',      'AoE', 'D6',  'Acid',        9),
    ('Lava Hail',        'Molten rock rains from the sky.',            'AoE', 'D12', 'Fire',        15),
    ('Lightning Strike', 'A bolt of lightning strikes from above.',    'AoE', 'D10', 'Lightning',   10),
    ('Sand Storm',       'Blinding sand scours the battlefield.',      'AoE', 'D6',  'Bludgeoning', 7),
    ('Blizzard',         'Freezing winds and snow pelt the area.',     'AoE', 'D8',  'Ice',         10),
    ('Blinding Flash',   'A brilliant flash blinds all who see it.',   'AoE', NULL,  NULL,          6),
    ('Earthquake',       'The ground shakes violently.',               'AoE', 'D12', 'Bludgeoning', 14),
    ('Insect Swarm',     'A cloud of biting insects descends.',        'AoE', 'D4',  'Piercing',    7),
    ('Fog of Despair',   'A choking fog that saps morale.',            'AoE', NULL,  NULL,          8),
    ('Stun',             'A concussive force that stuns the target.',  'CC',  NULL,  NULL,          5),
    ('Sleep',            'Puts the target into a magical slumber.',    'CC',  NULL,  NULL,          6),
    ('Charm Enemy',      'Bends an enemy to your will.',               'CC',  NULL,  NULL,          8),
    ('Fear',             'Instills overwhelming terror.',              'CC',  NULL,  NULL,          7),
    ('Taunt',            'Forces an enemy to attack you.',             'CC',  NULL,  NULL,          4),
    ('Freeze',           'Encases the target in ice.',                 'CC',  NULL,  NULL,          7),
    ('Confuse',          'Makes the target act erratically.',          'CC',  NULL,  NULL,          6),
    ('Provoke',          'Enrages the target, reducing its defenses.', 'CC',  NULL,  NULL,          5),
    ('Sacrifice',        'Sacrifice own HP to empower an ally.',       'CC',  NULL,  NULL,          0),
    ('Blind',            'Robs the target of sight.',                  'CC',  NULL,  NULL,          5),
    ('Root',             'Anchors the target to the ground.',          'CC',  NULL,  NULL,          5),
    ('Summon Creature',  'Calls a creature to fight for you.',         'Other', NULL, NULL,         12)
) AS src(name, description, school_name, die_name, dmg_name, mana)
JOIN arena_data.spell_school s ON s.name = src.school_name
LEFT JOIN arena_data.die_type d ON d.name = src.die_name
LEFT JOIN arena_data.damage_type dt ON dt.name = src.dmg_name;

-- ============================================================
-- CHARACTERS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.character (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id),
    name VARCHAR(100) NOT NULL,
    level INTEGER NOT NULL DEFAULT 1,
    strength INTEGER NOT NULL DEFAULT 10,
    dexterity INTEGER NOT NULL DEFAULT 10,
    stamina INTEGER NOT NULL DEFAULT 10,
    intelligence INTEGER NOT NULL DEFAULT 10,
    wisdom INTEGER NOT NULL DEFAULT 10,
    charisma INTEGER NOT NULL DEFAULT 10,
    strength_percentile INTEGER DEFAULT 0,
    max_hit_points INTEGER NOT NULL DEFAULT 10,
    current_hit_points INTEGER NOT NULL DEFAULT 10,
    experience_points INTEGER NOT NULL DEFAULT 0,
    strike_rating INTEGER NOT NULL DEFAULT 20,
    turn_speed INTEGER NOT NULL DEFAULT 10,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS arena_data.character_equipment (
    id SERIAL PRIMARY KEY,
    character_id INTEGER NOT NULL REFERENCES arena_data.character(id) ON DELETE CASCADE,
    slot_id INTEGER NOT NULL REFERENCES arena_data.equipment_slot(id),
    item_type VARCHAR(10) NOT NULL,
    item_id INTEGER NOT NULL,
    UNIQUE (character_id, slot_id)
);

CREATE TABLE IF NOT EXISTS arena_data.character_inventory (
    id SERIAL PRIMARY KEY,
    character_id INTEGER NOT NULL REFERENCES arena_data.character(id) ON DELETE CASCADE,
    item_type VARCHAR(10) NOT NULL,
    item_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1
);

-- ============================================================
-- SEED: SAMPLE CHARACTERS
-- ============================================================

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, strength_percentile, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Bruenor Battlehammer', r.id, c.id, 5, 18, 12, 18, 9, 13, 11, 76, 55, 55, 14, 12
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Fighter';

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Tanis Half-Elven', r.id, c.id, 5, 14, 16, 12, 14, 14, 16, 38, 38, 14, 18
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Rogue';

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, strength_percentile, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Karg Bloodfang', r.id, c.id, 6, 18, 10, 16, 7, 8, 9, 99, 72, 72, 13, 14
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Orc' AND c.name = 'Barbarian';

-- Additional playable characters for full 6-hero party demos
INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Elara Swiftwind', r.id, c.id, 5, 8, 14, 10, 18, 16, 14, 28, 28, 13, 10
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Elara Swiftwind');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Sir Aldric Vane', r.id, c.id, 6, 17, 10, 18, 11, 13, 14, 62, 62, 14, 8
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Knight'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sir Aldric Vane');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Mira Brightholm', r.id, c.id, 4, 10, 14, 12, 15, 17, 16, 34, 34, 14, 12
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Priest'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Mira Brightholm');

-- ============================================================
-- NPC FLAG + BIOGRAPHY ON CHARACTER TABLE
-- ============================================================

ALTER TABLE arena_data.character ADD COLUMN IF NOT EXISTS npc SMALLINT NOT NULL DEFAULT 0 CHECK (npc IN (0, 1));
ALTER TABLE arena_data.character ADD COLUMN IF NOT EXISTS biography TEXT DEFAULT '';

-- ============================================================
-- SEED: ADDITIONAL CHARACTERS (both heroes and NPCs)
-- ============================================================

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Brorn Ironarm', r.id, c.id, 6, 18, 10, 18, 8, 10, 9, 68, 68, 13, 10, 0
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Barbarian'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Brorn Ironarm');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sylas Moonshadow', r.id, c.id, 5, 10, 16, 10, 17, 14, 15, 30, 30, 14, 16, 0
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Rogue'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sylas Moonshadow');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Captain Aldric', r.id, c.id, 7, 16, 12, 15, 10, 12, 13, 58, 58, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Captain Aldric');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sister Marigold', r.id, c.id, 9, 10, 10, 12, 14, 18, 16, 48, 48, 14, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Priest'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sister Marigold');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Rorik the Wanderer', r.id, c.id, 8, 18, 10, 18, 8, 10, 9, 90, 90, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Barbarian'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Rorik the Wanderer');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Selene Nightwhisper', r.id, c.id, 10, 8, 14, 10, 18, 14, 16, 32, 32, 14, 14, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Selene Nightwhisper');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Grommash Ironhide', r.id, c.id, 12, 20, 10, 18, 7, 8, 10, 112, 112, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Orc' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Grommash Ironhide');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Finn Swift', r.id, c.id, 6, 8, 18, 10, 12, 10, 16, 28, 28, 14, 18, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Halfling' AND c.name = 'Rogue'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Finn Swift');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'The Collector', r.id, c.id, 15, 10, 12, 10, 20, 16, 14, 42, 42, 15, 12, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'The Collector');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Morgath the Pale', r.id, c.id, 14, 18, 8, 16, 10, 12, 8, 126, 126, 14, 8, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Undead' AND c.name = 'Knight'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Morgath the Pale');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sizzle', r.id, c.id, 5, 6, 14, 8, 16, 10, 12, 18, 18, 15, 14, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Kobold' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sizzle');

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Ivy Thornwood', r.id, c.id, 8, 10, 14, 12, 16, 18, 14, 52, 52, 14, 14, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Druid'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Ivy Thornwood');

-- NPC biographies

UPDATE arena_data.character SET biography = 'A retired captain of the City Watch who now runs a small weapons shop in the market district. He lost his left eye to a goblin arrow during the Goblin Wars and claims it gave him better judgment of character.' WHERE name = 'Captain Aldric';

UPDATE arena_data.character SET biography = 'A soft-spoken priestess of the Temple of Light who has healed everything from battlefield wounds to broken hearts. She never turns away the sick or poor, and the temple gardens she tends are the most beautiful in the city.' WHERE name = 'Sister Marigold';

UPDATE arena_data.character SET biography = 'A dwarf who has outlived three clans and drank every tavern dry from the Iron Mountains to the coast. He wanders the realm seeking worthy drinking partners and fights worth remembering. Despite his gruff exterior, he has saved more than one village from bandits.' WHERE name = 'Rorik the Wanderer';

UPDATE arena_data.character SET biography = 'A half-elf enchantress who runs an apothecary and curio shop. Her true specialty lies in identifying magical items and brokering deals between those who have them and those who seek them. She speaks four languages and is never caught off guard.' WHERE name = 'Selene Nightwhisper';

UPDATE arena_data.character SET biography = 'An orc of few words and many kills. He wanders the realm seeking worthy opponents to test his steel against. Despite his fearsome reputation, he has a strict code of honor and has been known to spare foes who yield with dignity.' WHERE name = 'Grommash Ironhide';

UPDATE arena_data.character SET biography = 'A halfling with an infectious grin and a talent for being where he should not be. He runs an information network that spans every tavern and market stall in the city. For a few gold coins, Finn can tell you anything about anyone.' WHERE name = 'Finn Swift';

UPDATE arena_data.character SET biography = 'A mysterious figure cloaked in grey who appears at auctions, estate sales, and archaeological digs across the realm. The Collector buys rare and unusual items — never sells. His vault is rumoured to contain artifacts from the Age of Gods.' WHERE name = 'The Collector';

UPDATE arena_data.character SET biography = 'An undead knight cursed to guard the Tomb of the First King for eternity. He was once a valiant paladin who broke his oath and was sentenced to unending vigilance. He speaks in a hollow whisper and his sword has never rusted.' WHERE name = 'Morgath the Pale';

UPDATE arena_data.character SET biography = 'A kobold with an unhealthy obsession with fire and explosions. Sizzle sells "perfectly safe" fireworks and alchemical mixtures from a stall that has burned down four times. He insists the fires were not his fault.' WHERE name = 'Sizzle';

UPDATE arena_data.character SET biography = 'A forest guardian who protects the ancient groves of the Singing Woods. She trades rare herbs, seeds, and components to those who prove they respect nature. She has not spoken a word in three years — she claims the trees speak enough for her.' WHERE name = 'Ivy Thornwood';

-- ============================================================
-- SEED: ADDITIONAL WEAPONS
-- ============================================================

-- Legendary weapons

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus
FROM (VALUES
    ('Doomwhisper',
     'Fashioned from the heartwood of a tree that grew on a battlefield where ten thousand fell. Each arrow sings a different death knell as it flies, and those struck by its shafts feel the cold of the grave seep into their bones.',
     'Bow', 'D10', 'Piercing', 'Ranged', 1, 2, 'Legendary', 2),
    ('Worldsplitter',
     'The hammer of a forgotten earth god, shattered into seven pieces and reforged by mortal hands over seven generations. It remembers the weight of mountains, and when it strikes, the ground trembles in sympathy.',
     'Hammer', 'D12', 'Bludgeoning', 'Melee', 1, 2, 'Legendary', 2),
    ('Soulpiercer',
     'A spear that has tasted the blood of a hundred warlords across three continents. Its tip glows crimson when enemies draw near, and those wounded by it feel their life force drain into the ancient weapon.',
     'Spear', 'D10', 'Piercing', 'Melee', 1, 2, 'Legendary', 2)
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Epic weapons

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus
FROM (VALUES
    ('Thunderstrike',
     'A war hammer forged from a meteorite that struck the Temple of Storms during a thunderstorm. It crackles with residual sky-energy, and sparks dance along its head when raised in battle.',
     'Hammer', 'D10', 'Lightning', 'Melee', 1, 1, 'Epic', 1),
    ('Moonblade',
     'A short sword tempered under the light of three full moons by elven smiths who whisper to the stars during the forging. The blade gleams with an ethereal silver light that casts no shadow.',
     'ShortSword', 'D8', 'Slashing', 'Melee', 1, 1, 'Epic', 1),
    ('Hellspine',
     'A morning star assembled from chains pulled from the depths of the Abyss. Its spikes are still warm to the touch, and the handle is wrapped in the cured hide of a fiend that screamed for a century.',
     'MorningStar', 'D10', 'Fire', 'Melee', 1, 1, 'Epic', 1)
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Rare weapons

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus
FROM (VALUES
    ('Glimmer',
     'A short bow strung with a strand of siren hair, traded for at great cost in the port city of Tidehold. Arrows loosed from it hum softly and curve slightly in flight toward their target.',
     'Bow', 'D6', 'Piercing', 'Ranged', 1, 2, 'Rare', 0),
    ('Stonefang',
     'The jawbone of a basalt giant from the Cinder Peaks, shaped into a mace by dwarven shamans. It never chips, never dulls, and leaves crater-like dents in whatever it strikes.',
     'Mace', 'D8', 'Bludgeoning', 'Melee', 1, 1, 'Rare', 1),
    ('Widow''s Kiss',
     'A slender dagger with a groove carved along the spine for delivering toxins. The assassin who first carried it was never caught, and her mark was always found with a peaceful smile.',
     'Dagger', 'D6', 'Poison', 'Melee', 1, 1, 'Rare', 0)
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Uncommon weapons

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id, attack_bonus)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id, src.atk_bonus
FROM (VALUES
    ('River''s Edge',
     'A solid blade forged by the river-smiths of the Telmar Crossing. Nothing remarkable, but it has never broken in battle, which is more than many swords can claim.',
     'Sword', 'D8', 'Slashing', 'Melee', 1, 1, 'Uncommon', 0),
    ('Brawler''s Friend',
     'A weighted club favored by tavern enforcers and city watchmen across the realm. The leather grip is dark with years of use and the head is chipped from countless brawls.',
     'Mace', 'D6', 'Bludgeoning', 'Melee', 1, 1, 'Uncommon', 1),
    ('Trailblazer',
     'A practical hand axe carried by frontier scouts and border rangers. It clears brush equally as well as it discourages wild animals and highwaymen.',
     'Axe', 'D6', 'Slashing', 'Melee', 1, 1, 'Uncommon', 0)
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name, atk_bonus)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Common weapons

INSERT INTO arena_data.weapon (name, description, weapon_type_id, damage_die_id, damage_type_id, attack_type_id, damage_count, hands, gear_quality_id)
SELECT src.name, src.description, wt.id, d.id, dt.id, at.id, src.dmg_count, src.hands, gq.id
FROM (VALUES
    ('Woodcutter''s Cleaver',
     'A heavy blade meant for splitting firewood. It can split bone just as easily.',
     'Axe', 'D6', 'Slashing', 'Melee', 1, 1, 'Common'),
    ('Practice Sword',
     'A blunted training blade worn smooth by countless sparring sessions in the barracks yard.',
     'Sword', 'D6', 'Bludgeoning', 'Melee', 1, 1, 'Common'),
    ('Cudgel',
     'A sturdy oak branch wrapped in fraying cloth. The poor man''s weapon, but effective enough in a pinch.',
     'Mace', 'D4', 'Bludgeoning', 'Melee', 1, 1, 'Common')
) AS src(name, description, type_name, die_name, dmg_name, atk_name, dmg_count, hands, quality_name)
JOIN arena_data.weapon_type wt ON wt.name = src.type_name
JOIN arena_data.die_type d ON d.name = src.die_name
JOIN arena_data.damage_type dt ON dt.name = src.dmg_name
JOIN arena_data.attack_type at ON at.name = src.atk_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- SEED: ADDITIONAL ARMOR
-- ============================================================

-- Legendary armor

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus
FROM (VALUES
    ('Aegis of the Fallen King',
     'The armor of the last king of Ashvale, who stood alone at the bridge of Mareth while his people fled the demon horde. It bears a hundred scars in the metal and still gleams with defiance.',
     19, 'Heavy', 0, TRUE, 18, 'Legendary', 2),
    ('Shroud of the Whispering Wind',
     'Woven from the breath of a dying goddess by the silent monks of the Mountain of Silence. It weighs nothing, makes no sound when the wearer moves, and feels like standing in a gentle breeze.',
     13, 'Light', 99, FALSE, 0, 'Legendary', 1)
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Epic armor

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus
FROM (VALUES
    ('Phoenix Carapace',
     'Scale mail fashioned from the shed carapace of a phoenix-fire elemental that was tamed by the Sun Monks. It is unnaturally light, warm to the touch, and gleams like embers in firelight.',
     15, 'Medium', 3, FALSE, 0, 'Epic', 1),
    ('Battlesworn Plate',
     'Splint armor that was carried through the entirety of the Hundred Years War. Each dent and scratch on its surface tells the story of a battle survived, a comrade lost, or a foe defeated.',
     17, 'Heavy', 0, TRUE, 15, 'Epic', 0),
    ('Kithbound Leather',
     'Leather armor infused with the essence of a bonded animal companion through a druidic ritual. It shifts and flexes with the wearer''s movements as if it were alive, and a low growl emanates from it when danger nears.',
     12, 'Light', 99, FALSE, 0, 'Epic', 0)
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Rare armor

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus
FROM (VALUES
    ('Ironbark Vest',
     'A vest made from the bark of the ironbark tree, which grows only in the Singing Woods where the trees remember the First Age. Arrows and crossbow bolts bounce off it like rain off stone.',
     12, 'Light', 99, FALSE, 0, 'Rare', 0),
    ('Rune-etched Shield',
     'A shield carved with ancient dwarven warding runes that glow faintly when enemies approach. The runes tell the story of the first dwarven king who stood against the Shadow.',
     3, 'Shield', 0, FALSE, 0, 'Rare', 1),
    ('Stalker''s Coat',
     'A long coat of waxed leather and fine chainmail favored by bounty hunters who operate in the lawless borderlands. The interior is lined with concealed pockets designed for throwing knives, lockpicks, and escape tools.',
     14, 'Medium', 3, FALSE, 0, 'Rare', 0)
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Uncommon armor

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id, armor_class_bonus)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id, src.ac_bonus
FROM (VALUES
    ('Patrol Helm',
     'A standard-issue steel helm with a visor and the faded crest of the City Watch. It has seen its share of riots, alley fights, and night patrols through the poor quarters.',
     15, 'Heavy', 0, TRUE, 13, 'Uncommon', 0),
    ('Traveler''s Cloak',
     'A waxed wool cloak worn by merchants and couriers who travel the King''s Road. It turns light rain and provides just enough protection to matter in a roadside scuffle.',
     11, 'Light', 99, FALSE, 0, 'Uncommon', 0),
    ('Scout''s Leathers',
     'Soft, quiet leather armor worn by army scouts and mounted messengers. It has carried its wearer through enemy territory and back again more times than the owner can remember.',
     12, 'Light', 99, FALSE, 0, 'Uncommon', 0)
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name, ac_bonus)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Common armor

INSERT INTO arena_data.armor (name, description, armor_class, armor_category_id, max_dexterity_bonus, stealth_disadvantage, strength_requirement, gear_quality_id)
SELECT src.name, src.description, src.ac, acat.id, src.max_dex, src.stealth, src.str_req, gq.id
FROM (VALUES
    ('Boiled Leather Vest',
     'Leather hardened in hot wax and shaped over a wooden form. Better than nothing, and that is about all that can be said for it.',
     11, 'Light', 99, FALSE, 0, 'Common'),
    ('Iron Cap',
     'A simple iron skullcap that covers the top of the head and offers a false sense of security. Worn by militia and caravan guards who cannot afford a proper helm.',
     14, 'Heavy', 0, TRUE, 11, 'Common'),
    ('Patched Gambeson',
     'A padded cloth jacket that has been repaired so many times the patches have patches. It smells faintly of its previous owners, none of whom died rich.',
     10, 'Light', 99, FALSE, 0, 'Common')
) AS src(name, description, ac, category_name, max_dex, stealth, str_req, quality_name)
JOIN arena_data.armor_category acat ON acat.name = src.category_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- SEED: ADDITIONAL ACCESSORIES
-- ============================================================

-- Legendary accessories

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    ('Eye of the Void',
     'A black opal the size of a thumb, set in a silver cage. It seems to contain an endless darkness that moves when observed. The wearer glimpses fragments of the future in their dreams — not always pleasant, never wrong.',
     'Amulet', 'Legendary', 'Intelligence', 3, FALSE, ''),
    ('Ring of Kings',
     'A golden band worn by every sovereign of Eldergard since the founding of the realm a thousand years ago. It pulses with a warm golden light when the wearer speaks a truth that will shape history.',
     'Ring', 'Legendary', 'Charisma', 3, FALSE, '')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Epic accessories

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    ('Ember Pendant',
     'A pendant containing a single ember plucked from the heart of Mount Kryx by the Fire Walkers of the Smoldering Sect. It keeps the wearer warm even in the frozen wastes and glows brighter when danger is near.',
     'Amulet', 'Epic', 'Stamina', 2, FALSE, ''),
    ('Trickster''s Band',
     'A silver ring engraved with a fox''s face that seems to wink at different angles. It rotates freely on the finger, never resting in the same position. Favored by gamblers, diplomats, and those who live by their wits.',
     'Ring', 'Epic', 'Dexterity', 2, FALSE, '')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Rare accessories

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    ('Seer''s Lens',
     'A crystal lens on a silver chain, ground by the blind seers of the Azure Monastery. Looking through it reveals invisible magical auras and hidden enchantments.',
     'Amulet', 'Rare', 'Intelligence', 1, FALSE, ''),
    ('Guardian''s Seal',
     'A signet ring bearing the crest of the Iron Company — a gauntlet gripping a tower shield. It was awarded to veterans of the defense of Ironwall and grants courage to those who wear it.',
     'Ring', 'Rare', 'Stamina', 1, FALSE, ''),
    ('Windwalker''s Sash',
     'A silk sash woven from the thread of sky-spiders that live among the peaks of the Cloudreach Mountains. It flutters even when there is no breeze and lightens the step of the one who wears it.',
     'Girdle', 'Rare', 'Dexterity', 1, FALSE, ''),
    ('Merchant''s Weight',
     'A heavy bronze buckle said to have been used by the Master of Scales in the Grand Bazaar of Eldergard. The wearer always knows the true value of any item they hold and can sense hidden compartments.',
     'Girdle', 'Rare', 'Charisma', 1, FALSE, '')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Uncommon accessories

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    ('Copper Band',
     'A simple copper wedding band that has long since lost its shine. It belonged to someone''s grandmother and carries the warmth of a life well lived.',
     'Ring', 'Uncommon', 'Stamina', 1, FALSE, ''),
    ('Fang Necklace',
     'A necklace of wolf fangs strung on sinew. The hunter who made it claimed it kept him from getting lost in the woods — though the fangs themselves are more likely to intimidate than to guide.',
     'Amulet', 'Uncommon', 'Strength', 1, FALSE, ''),
    ('Traveler''s Belt',
     'A wide leather belt lined with small pouches and loops. It distributes weight perfectly across the hips, allowing the wearer to carry more without tiring as fast.',
     'Girdle', 'Uncommon', 'Stamina', 1, FALSE, '')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- Common accessories

INSERT INTO arena_data.accessory (name, description, accessory_type_id, gear_quality_id, effect_type, effect_value, cursed, curse_effect)
SELECT src.name, src.description, atype.id, gq.id, src.effect, src.value, src.cursed, src.curse
FROM (VALUES
    ('Tarnished Ring',
     'An old brass ring, green with age, found in a barrel of second-hand goods. Worth a few copper pieces and likely to turn your finger green.',
     'Ring', 'Common', 'none', 0, FALSE, ''),
    ('Rabbit''s Foot',
     'A dried rabbit''s foot on a frayed piece of string. It probably does nothing, but the soldier who carried it through three campaigns swore by its luck.',
     'Amulet', 'Common', 'none', 0, FALSE, ''),
    ('Rope Belt',
     'A length of braided hemp that serves as a belt. Practical, cheap, and easy to replace. Commonly worn by laborers and prisoners alike.',
     'Girdle', 'Common', 'none', 0, FALSE, '')
) AS src(name, description, type_name, quality_name, effect, value, cursed, curse)
JOIN arena_data.accessory_type atype ON atype.name = src.type_name
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;

-- ============================================================
-- FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_races(
    p_id INTEGER DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT,
    strength_bonus INTEGER, dexterity_bonus INTEGER,
    stamina_bonus INTEGER, intelligence_bonus INTEGER,
    wisdom_bonus INTEGER, charisma_bonus INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.name::VARCHAR, r.description::TEXT,
           r.strength_bonus, r.dexterity_bonus, r.stamina_bonus,
           r.intelligence_bonus, r.wisdom_bonus, r.charisma_bonus
    FROM arena_data.race r
    WHERE (p_id IS NULL OR r.id = p_id)
    ORDER BY r.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_subraces(
    p_race_id INTEGER DEFAULT NULL
)
RETURNS TABLE(id INTEGER, race_id INTEGER, name VARCHAR, description TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.race_id, s.name::VARCHAR, s.description::TEXT
    FROM arena_data.subrace s
    WHERE (p_race_id IS NULL OR s.race_id = p_race_id)
    ORDER BY s.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_race_abilities(
    p_race_id INTEGER DEFAULT NULL
)
RETURNS TABLE(id INTEGER, race_id INTEGER, name VARCHAR, description TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT sa.id, sa.race_id, sa.name::VARCHAR, sa.description::TEXT
    FROM arena_data.race_special_ability sa
    WHERE (p_race_id IS NULL OR sa.race_id = p_race_id)
    ORDER BY sa.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_classes()
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT, hit_die VARCHAR, base_strike_rating INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.description::TEXT, d.name::VARCHAR AS hit_die, c.base_strike_rating
    FROM arena_data.class c
    JOIN arena_data.die_type d ON d.id = c.hit_die_id
    ORDER BY c.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_weapons(
    p_id INTEGER DEFAULT NULL,
    p_type VARCHAR(50) DEFAULT NULL,
    p_quality VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT,
    weapon_type VARCHAR, damage_die VARCHAR, damage_type VARCHAR,
    attack_type VARCHAR, damage_count INTEGER, hands INTEGER,
    quality VARCHAR, attack_bonus INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT w.id, w.name::VARCHAR, w.description::TEXT,
           wt.name::VARCHAR AS weapon_type,
           d.name::VARCHAR AS damage_die,
           dt.name::VARCHAR AS damage_type,
           at.name::VARCHAR AS attack_type,
           w.damage_count, w.hands,
           gq.name::VARCHAR AS quality,
           w.attack_bonus
    FROM arena_data.weapon w
    JOIN arena_data.weapon_type wt ON wt.id = w.weapon_type_id
    JOIN arena_data.die_type d ON d.id = w.damage_die_id
    JOIN arena_data.damage_type dt ON dt.id = w.damage_type_id
    JOIN arena_data.attack_type at ON at.id = w.attack_type_id
    JOIN arena_data.gear_quality gq ON gq.id = w.gear_quality_id
    WHERE (p_id IS NULL OR w.id = p_id)
      AND (p_type IS NULL OR wt.name = p_type)
      AND (p_quality IS NULL OR gq.name = p_quality)
    ORDER BY gq.sort_order, w.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_armor(
    p_id INTEGER DEFAULT NULL,
    p_quality VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT,
    armor_class INTEGER, category VARCHAR,
    max_dexterity_bonus INTEGER, stealth_disadvantage BOOLEAN,
    strength_requirement INTEGER,
    quality VARCHAR, armor_class_bonus INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.name::VARCHAR, a.description::TEXT,
           a.armor_class, ac.name::VARCHAR AS category,
           a.max_dexterity_bonus, a.stealth_disadvantage, a.strength_requirement,
           gq.name::VARCHAR AS quality,
           a.armor_class_bonus
    FROM arena_data.armor a
    JOIN arena_data.armor_category ac ON ac.id = a.armor_category_id
    JOIN arena_data.gear_quality gq ON gq.id = a.gear_quality_id
    WHERE (p_id IS NULL OR a.id = p_id)
      AND (p_quality IS NULL OR gq.name = p_quality)
    ORDER BY gq.sort_order, a.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_spells(
    p_school VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT, school VARCHAR, mana_cost INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.name::VARCHAR, s.description::TEXT, ss.name::VARCHAR AS school, s.mana_cost
    FROM arena_data.spell s
    JOIN arena_data.spell_school ss ON ss.id = s.school_id
    WHERE (p_school IS NULL OR ss.name = p_school)
    ORDER BY s.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_deities(
    p_alignment VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(id INTEGER, name VARCHAR, alignment VARCHAR, description TEXT, domain VARCHAR) AS $$
BEGIN
    RETURN QUERY
    SELECT d.id, d.name::VARCHAR, da.name::VARCHAR AS alignment,
           d.description::TEXT, d.domain::VARCHAR
    FROM arena_data.deity d
    JOIN arena_data.deity_alignment da ON da.id = d.alignment_id
    WHERE (p_alignment IS NULL OR da.name = p_alignment)
    ORDER BY d.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_pets(
    p_class_id INTEGER DEFAULT NULL,
    p_race_id INTEGER DEFAULT NULL
)
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT, damage_die VARCHAR, armor_class INTEGER, hit_points INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT p.id, p.name::VARCHAR, p.description::TEXT,
           d.name::VARCHAR AS damage_die, p.armor_class, p.hit_points
    FROM arena_data.pet p
    JOIN arena_data.die_type d ON d.id = p.damage_die_id
    LEFT JOIN arena_data.pet_class_restriction pcr ON pcr.pet_id = p.id
    LEFT JOIN arena_data.pet_race_restriction prr ON prr.pet_id = p.id
    WHERE (p_class_id IS NULL OR pcr.class_id = p_class_id)
      AND (p_race_id IS NULL OR prr.race_id = p_race_id)
    ORDER BY p.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_characters()
RETURNS TABLE(
    id INTEGER, name VARCHAR, level INTEGER, race_id INTEGER, class_id INTEGER,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    strength_percentile INTEGER, max_hit_points INTEGER, current_hit_points INTEGER,
    strike_rating INTEGER, turn_speed INTEGER,
    npc SMALLINT, biography TEXT,
    experience_points INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.level, c.race_id, c.class_id,
           c.strength, c.dexterity, c.stamina,
           c.intelligence, c.wisdom, c.charisma,
           c.strength_percentile, c.max_hit_points, c.current_hit_points,
           c.strike_rating, c.turn_speed,
           c.npc, c.biography::TEXT,
           c.experience_points
    FROM arena_data.character c
    ORDER BY c.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_character(p_id INTEGER)
RETURNS TABLE(
    id INTEGER, name VARCHAR, level INTEGER, race_id INTEGER, class_id INTEGER,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    strength_percentile INTEGER, max_hit_points INTEGER, current_hit_points INTEGER,
    strike_rating INTEGER, turn_speed INTEGER,
    npc SMALLINT, biography TEXT,
    experience_points INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.level, c.race_id, c.class_id,
           c.strength, c.dexterity, c.stamina,
           c.intelligence, c.wisdom, c.charisma,
           c.strength_percentile, c.max_hit_points, c.current_hit_points,
           c.strike_rating, c.turn_speed,
           c.npc, c.biography::TEXT,
           c.experience_points
    FROM arena_data.character c
    WHERE c.id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_create_character(
    p_name VARCHAR,
    p_race_id INTEGER,
    p_class_id INTEGER,
    p_strength INTEGER,
    p_dexterity INTEGER,
    p_stamina INTEGER,
    p_intelligence INTEGER,
    p_wisdom INTEGER,
    p_charisma INTEGER,
    p_strength_percentile INTEGER DEFAULT 0,
    p_max_hit_points INTEGER DEFAULT 10
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
    v_strike_rating INTEGER;
BEGIN
    SELECT base_strike_rating INTO v_strike_rating FROM arena_data.class WHERE id = p_class_id;

    INSERT INTO arena_data.character (
        name, race_id, class_id, level,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        strength_percentile, max_hit_points, current_hit_points, strike_rating
    ) VALUES (
        p_name, p_race_id, p_class_id, 1,
        p_strength, p_dexterity, p_stamina, p_intelligence, p_wisdom, p_charisma,
        p_strength_percentile, p_max_hit_points, p_max_hit_points, v_strike_rating
    ) RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

CREATE OR REPLACE PROCEDURE arena_data.sp_update_character(
    p_id INTEGER,
    p_name VARCHAR,
    p_level INTEGER,
    p_strength INTEGER,
    p_dexterity INTEGER,
    p_stamina INTEGER,
    p_intelligence INTEGER,
    p_wisdom INTEGER,
    p_charisma INTEGER,
    p_strength_percentile INTEGER DEFAULT 0,
    p_current_hit_points INTEGER DEFAULT 10
)
AS $$
BEGIN
    UPDATE arena_data.character
    SET name = p_name,
        level = p_level,
        strength = p_strength,
        dexterity = p_dexterity,
        stamina = p_stamina,
        intelligence = p_intelligence,
        wisdom = p_wisdom,
        charisma = p_charisma,
        strength_percentile = p_strength_percentile,
        current_hit_points = p_current_hit_points,
        updated_at = NOW()
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE arena_data.sp_delete_character(p_id INTEGER)
AS $$
BEGIN
    DELETE FROM arena_data.character WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- FEAT FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_feats(p_race_id INTEGER DEFAULT NULL)
RETURNS TABLE(
    id INTEGER, race_id INTEGER, name VARCHAR, description TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT rsa.id, rsa.race_id, rsa.name::VARCHAR, rsa.description::TEXT
    FROM arena_data.race_special_ability rsa
    WHERE (p_race_id IS NULL OR rsa.race_id = p_race_id)
    ORDER BY rsa.name;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- ITEM SET FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_item_sets()
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.name::VARCHAR, s.description::TEXT
    FROM arena_data.item_set s
    ORDER BY s.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_set_bonuses(p_set_id INTEGER DEFAULT NULL)
RETURNS TABLE(id INTEGER, set_id INTEGER, pieces_required INTEGER, effect_description TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT sb.id, sb.set_id, sb.pieces_required, sb.effect_description::TEXT
    FROM arena_data.set_bonus sb
    WHERE (p_set_id IS NULL OR sb.set_id = p_set_id)
    ORDER BY sb.pieces_required;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- ACCESSORY FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_accessories(
    p_type VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT,
    accessory_type VARCHAR, quality VARCHAR,
    effect_type VARCHAR, effect_value INTEGER,
    cursed BOOLEAN, curse_effect TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.name::VARCHAR, a.description::TEXT,
           atype.name::VARCHAR AS accessory_type,
           gq.name::VARCHAR    AS quality,
           a.effect_type, a.effect_value,
           a.cursed, a.curse_effect::TEXT
    FROM arena_data.accessory a
    JOIN arena_data.accessory_type atype ON atype.id = a.accessory_type_id
    JOIN arena_data.gear_quality   gq    ON gq.id    = a.gear_quality_id
    WHERE (p_type IS NULL OR atype.name = p_type)
    ORDER BY atype.name, gq.sort_order, a.name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION arena_data.fn_get_npcs(
    p_merchant BOOLEAN DEFAULT NULL,
    p_hostile BOOLEAN DEFAULT NULL
)
RETURNS TABLE(id INTEGER, name VARCHAR, race VARCHAR, class VARCHAR, level INTEGER,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    is_merchant BOOLEAN, is_quest_giver BOOLEAN, is_hostile BOOLEAN, biography TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT n.id, n.name::VARCHAR, r.name::VARCHAR AS race, c.name::VARCHAR AS class,
           n.level, n.strength, n.dexterity, n.stamina,
           n.intelligence, n.wisdom, n.charisma,
           n.is_merchant, n.is_quest_giver, n.is_hostile, n.biography::TEXT
    FROM arena_data.npc n
    JOIN arena_data.race r ON r.id = n.race_id
    JOIN arena_data.class c ON c.id = n.class_id
    WHERE (p_merchant IS NULL OR n.is_merchant = p_merchant)
      AND (p_hostile IS NULL OR n.is_hostile = p_hostile)
    ORDER BY n.name;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- pg_cron SETUP
-- pg_cron is configured via cron.database_name=battle-arena_data,
-- so all jobs run directly in this database — no dblink needed.
-- ============================================================

CREATE EXTENSION IF NOT EXISTS pg_cron;

-- Vacuum arena tables weekly (Sunday at 2am)
SELECT cron.schedule('vacuum_weapon',    '0 2 * * 0', 'VACUUM ANALYZE arena_data.weapon');
SELECT cron.schedule('vacuum_armor',     '0 2 * * 0', 'VACUUM ANALYZE arena_data.armor');
SELECT cron.schedule('vacuum_race',      '0 2 * * 0', 'VACUUM ANALYZE arena_data.race');
SELECT cron.schedule('vacuum_character', '0 2 * * 0', 'VACUUM ANALYZE arena_data.character');

-- Clean old cron logs daily (1am)
SELECT cron.schedule('clean_cron_logs', '0 1 * * *',
    $$DELETE FROM cron.job_run_details WHERE end_time < NOW() - INTERVAL '5 days'$$);
