-- ============================================================
-- BattleArena - PostgreSQL World and Reference Seed Data
-- Contains lookup/reference data, world content, items, NPC records,
-- spells, and other non-character seed data.
-- ============================================================

-- ============================================================
-- SEED: REFERENCE DATA
-- ============================================================

INSERT INTO arena_data.die_type (name, sides) VALUES
    ('D4', 4), ('D6', 6), ('D8', 8), ('D10', 10), ('D12', 12), ('D20', 20), ('D100', 100)
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.damage_type (name) VALUES
    ('Bludgeoning'), ('Piercing'), ('Slashing'), ('Poison'), ('Fire'),
    ('Ice'), ('Lightning'), ('Shadow'), ('Holy'), ('Acid'), ('Psychic')
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
    ('AoE'), ('CC'), ('Other'), ('Evocation'), ('Conjuration')
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.equipment_slot (name) VALUES
    ('Head'), ('Chest'), ('Hands'), ('Waist'), ('Foot'),
    ('Neck'), ('Back'), ('RightHand'), ('LeftHand'), ('Banner'),
    ('Ring1'), ('Ring2'), ('Ornament')
ON CONFLICT (name) DO NOTHING;


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


-- Non-playable races (Undead, Demon)
UPDATE arena_data.race SET is_playable = FALSE WHERE name IN ('Undead', 'Demon');


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


-- Seed: Feat Resistances
INSERT INTO arena_data.feat_resistance (feat_id, resistance_type, resistance_value)
SELECT rsa.id, 'Magic', 25
FROM arena_data.race_special_ability rsa
JOIN arena_data.race r ON r.id = rsa.race_id
WHERE r.name IN ('Elf', 'Dwarf', 'Kobold', 'Ogre') AND rsa.name = 'Magic Resistance'
AND NOT EXISTS (SELECT 1 FROM arena_data.feat_resistance fr WHERE fr.feat_id = rsa.id);


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


-- Class-race restrictions
INSERT INTO arena_data.class_race (class_id, race_id)
SELECT c.id, r.id
FROM (VALUES
    ('Barbarian', 'Human'), ('Barbarian', 'Orc'), ('Barbarian', 'Ogre'), ('Barbarian', 'Dwarf'),
    ('Knight',    'Human'), ('Knight',    'Elf'), ('Knight',    'Dwarf'), ('Knight',    'Orc'),
    ('Paladin',   'Human'), ('Paladin',   'Elf'), ('Paladin',   'Dwarf'),
    ('Priest',    'Human'), ('Priest',    'Elf'), ('Priest',    'Dwarf'), ('Priest',    'Lizard'),
    ('Priest',    'Kobold'), ('Priest',   'Halfling'), ('Priest',  'Orc'),
    ('Mage',      'Human'), ('Mage',      'Elf'), ('Mage',      'Kobold'),
    ('Bard',      'Human'), ('Bard',      'Elf'), ('Bard',      'Halfling'),
    ('Druid',     'Human'), ('Druid',     'Elf'), ('Druid',     'Halfling'), ('Druid',    'Lizard'),
    ('Fighter',   'Human'), ('Fighter',   'Elf'), ('Fighter',   'Dwarf'), ('Fighter',   'Lizard'),
    ('Fighter',   'Kobold'), ('Fighter',  'Orc'), ('Fighter',   'Ogre'), ('Fighter',   'Halfling'),
    ('Rogue',     'Human'), ('Rogue',     'Elf'), ('Rogue',     'Dwarf'), ('Rogue',     'Halfling'), ('Rogue', 'Kobold')
) AS src(class_name, race_name)
JOIN arena_data.class c ON c.name = src.class_name
JOIN arena_data.race r ON r.name = src.race_name;


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


-- ============================================================
-- CLASS-ITEM RESTRICTIONS
-- Mirrors ArchetypeWeaponExtensions in BattleArena.Core.
-- Class IDs (insertion order): Barbarian=1, Knight=2, Paladin=3, Priest=4,
--   Mage=5, Bard=6, Druid=7, Fighter=8, Rogue=9
-- ============================================================

INSERT INTO arena_data.class_item_restriction (class_id, weapon_type_id)
SELECT c.id, wt.id
FROM (VALUES
    -- Dagger: all classes (Priests carry it as a ritual implement)
    ('Barbarian','Dagger'), ('Knight','Dagger'), ('Paladin','Dagger'), ('Priest','Dagger'),
    ('Mage','Dagger'),      ('Bard','Dagger'),   ('Druid','Dagger'),   ('Fighter','Dagger'),
    ('Rogue','Dagger'),
    -- ShortSword: warriors, bard, rogue
    ('Barbarian','ShortSword'), ('Knight','ShortSword'), ('Paladin','ShortSword'),
    ('Bard','ShortSword'),      ('Fighter','ShortSword'), ('Rogue','ShortSword'),
    -- Sword: warriors, bard, druid, rogue
    ('Barbarian','Sword'), ('Knight','Sword'), ('Paladin','Sword'),
    ('Bard','Sword'),      ('Druid','Sword'),  ('Fighter','Sword'), ('Rogue','Sword'),
    -- Axe: warriors only
    ('Barbarian','Axe'), ('Knight','Axe'), ('Paladin','Axe'), ('Fighter','Axe'),
    -- Mace: warriors + divine casters
    ('Barbarian','Mace'), ('Knight','Mace'), ('Paladin','Mace'),
    ('Priest','Mace'),    ('Druid','Mace'),  ('Fighter','Mace'),
    -- Hammer: same as Mace
    ('Barbarian','Hammer'), ('Knight','Hammer'), ('Paladin','Hammer'),
    ('Priest','Hammer'),    ('Druid','Hammer'),  ('Fighter','Hammer'),
    -- MorningStar: warriors + Priest (not Druid)
    ('Barbarian','MorningStar'), ('Knight','MorningStar'), ('Paladin','MorningStar'),
    ('Priest','MorningStar'),    ('Fighter','MorningStar'),
    -- Lance: mounted warriors only
    ('Barbarian','Lance'), ('Knight','Lance'), ('Paladin','Lance'), ('Fighter','Lance'),
    -- Spear: warriors, bard, druid
    ('Barbarian','Spear'), ('Knight','Spear'), ('Paladin','Spear'),
    ('Bard','Spear'),      ('Druid','Spear'),  ('Fighter','Spear'),
    -- Staff: all classes (universal)
    ('Barbarian','Staff'), ('Knight','Staff'), ('Paladin','Staff'), ('Priest','Staff'),
    ('Mage','Staff'),      ('Bard','Staff'),   ('Druid','Staff'),   ('Fighter','Staff'),
    ('Rogue','Staff'),
    -- Wand: mage only
    ('Mage','Wand'),
    -- Bow: warriors, bard, rogue
    ('Barbarian','Bow'), ('Knight','Bow'), ('Paladin','Bow'),
    ('Bard','Bow'),      ('Fighter','Bow'), ('Rogue','Bow'),
    -- Crossbow: same as Bow
    ('Barbarian','Crossbow'), ('Knight','Crossbow'), ('Paladin','Crossbow'),
    ('Bard','Crossbow'),      ('Fighter','Crossbow'), ('Rogue','Crossbow'),
    -- Sling: all except Mage
    ('Barbarian','Sling'), ('Knight','Sling'), ('Paladin','Sling'), ('Priest','Sling'),
    ('Bard','Sling'),      ('Druid','Sling'),  ('Fighter','Sling'), ('Rogue','Sling')
) AS src(class_name, wt_name)
JOIN arena_data.class       c  ON c.name  = src.class_name
JOIN arena_data.weapon_type wt ON wt.name = src.wt_name
ON CONFLICT (class_id, weapon_type_id) DO NOTHING;


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


-- Set mitigation values for armor
UPDATE arena_data.armor SET mitigation = 1 WHERE name IN ('Padded Armor', 'Leather Armor', 'Studded Leather');

UPDATE arena_data.armor SET mitigation = 2 WHERE name IN ('Hide Armor', 'Chain Shirt', 'Scale Mail', 'Breastplate');

UPDATE arena_data.armor SET mitigation = 3 WHERE name IN ('Half Plate', 'Ring Mail', 'Chain Mail');

UPDATE arena_data.armor SET mitigation = 4 WHERE name IN ('Splint Armor');

UPDATE arena_data.armor SET mitigation = 5 WHERE name IN ('Plate Armor');

UPDATE arena_data.armor SET mitigation = 0 WHERE name = 'Shield';

-- Quality armor
UPDATE arena_data.armor SET mitigation = 2 WHERE name IN ('Mithril Chain');

UPDATE arena_data.armor SET mitigation = 4 WHERE name IN ('Knight''s Honor');

UPDATE arena_data.armor SET mitigation = 6 WHERE name IN ('Titan Plate');

UPDATE arena_data.armor SET mitigation = 3 WHERE name IN ('Dragon Scale Mail');

UPDATE arena_data.armor SET mitigation = 5 WHERE name IN ('Phoenix Carapace', 'Battlesworn Plate');

UPDATE arena_data.armor SET mitigation = 4 WHERE name IN ('Aegis of the Fallen King');

UPDATE arena_data.armor SET mitigation = 0 WHERE name IN ('Shroud of the Whispering Wind');


-- Set turn_meter_penalty for armor (heavier armor slows TM gain)
UPDATE arena_data.armor SET turn_meter_penalty = 0 WHERE armor_category_id = (SELECT id FROM arena_data.armor_category WHERE name = 'Light');

UPDATE arena_data.armor SET turn_meter_penalty = -2 WHERE name IN ('Scale Mail', 'Half Plate');

UPDATE arena_data.armor SET turn_meter_penalty = -3 WHERE name IN ('Ring Mail');

UPDATE arena_data.armor SET turn_meter_penalty = -5 WHERE name IN ('Chain Mail', 'Splint Armor');

UPDATE arena_data.armor SET turn_meter_penalty = -8 WHERE name IN ('Plate Armor');

UPDATE arena_data.armor SET turn_meter_penalty = -2 WHERE name IN ('Shield');

-- Quality armor overrides
UPDATE arena_data.armor SET turn_meter_penalty = -5 WHERE name IN ('Knight''s Honor', 'Battlesworn Plate');

UPDATE arena_data.armor SET turn_meter_penalty = -10 WHERE name IN ('Titan Plate', 'Aegis of the Fallen King');

UPDATE arena_data.armor SET turn_meter_penalty = -2 WHERE name IN ('Dragon Scale Mail');


-- Set turn_meter_cost_reduction (robe-type armor for spellcasters)
UPDATE arena_data.armor SET turn_meter_cost_reduction = 5 WHERE name IN ('Leather Armor');


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


-- Seed: Armor Resistances
INSERT INTO arena_data.armor_resistance (armor_id, resistance_type, resistance_value)
SELECT a.id, 'Fire', 10
FROM arena_data.armor a WHERE a.name = 'Dragon Scale Mail'
AND NOT EXISTS (SELECT 1 FROM arena_data.armor_resistance ar WHERE ar.armor_id = a.id AND ar.resistance_type = 'Fire');


INSERT INTO arena_data.accessory_type (name) VALUES
    ('Ring'), ('Amulet'), ('Girdle')
ON CONFLICT (name) DO NOTHING;


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


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Blade Barrier', 'A wall of spinning blades.', ss.id, d.id, dt.id, at.id, 10, 90, 3, 3, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D8' AND dt.name = 'Slashing' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Ice Storm', 'Hail and ice pummel the area.', ss.id, d.id, dt.id, at.id, 8, 85, 3, 3, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D8' AND dt.name = 'Ice' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Fire Storm', 'A conflagration engulfs the area.', ss.id, d.id, dt.id, at.id, 12, 95, 4, 4, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D10' AND dt.name = 'Fire' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Acid Rain', 'Corrosive rain burns all in the area.', ss.id, d.id, dt.id, at.id, 9, 80, 3, 3, 1, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D6' AND dt.name = 'Acid' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Lava Hail', 'Molten rock rains from the sky.', ss.id, d.id, dt.id, at.id, 15, 100, 5, 4, 3, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D12' AND dt.name = 'Fire' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Lightning Strike', 'A bolt of lightning strikes from above.', ss.id, d.id, dt.id, at.id, 10, 90, 4, 3, 3, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D10' AND dt.name = 'Lightning' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Sand Storm', 'Blinding sand scours the battlefield.', ss.id, d.id, dt.id, at.id, 7, 75, 2, 2, 1, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D6' AND dt.name = 'Bludgeoning' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Blizzard', 'Freezing winds and snow pelt the area.', ss.id, d.id, dt.id, at.id, 10, 85, 4, 3, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D8' AND dt.name = 'Ice' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Earthquake', 'The ground shakes violently.', ss.id, d.id, dt.id, at.id, 14, 95, 5, 4, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D12' AND dt.name = 'Bludgeoning' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Insect Swarm', 'A cloud of biting insects descends.', ss.id, d.id, dt.id, at.id, 7, 75, 2, 2, 1, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'AoE' AND d.name = 'D4' AND dt.name = 'Piercing' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Blinding Flash', 'A brilliant flash blinds all who see it.', ss.id, NULL, NULL, at.id, 6, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'AoE' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Fog of Despair', 'A choking fog that saps morale.', ss.id, NULL, NULL, at.id, 8, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'AoE' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Stun', 'A concussive force that stuns the target.', ss.id, NULL, NULL, at.id, 5, 65, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Sleep', 'Puts the target into a magical slumber.', ss.id, dd.id, dt.id, at.id, 6, 60, 1, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type dd, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'CC' AND dd.name = 'D4' AND dt.name = 'Psychic' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Charm Enemy', 'Bends an enemy to your will.', ss.id, NULL, NULL, at.id, 8, 75, 3, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Fear', 'Instills overwhelming terror.', ss.id, NULL, NULL, at.id, 7, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Taunt', 'Forces an enemy to attack you.', ss.id, NULL, NULL, at.id, 4, 55, 1, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Freeze', 'Encases the target in ice.', ss.id, NULL, NULL, at.id, 7, 75, 3, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Confuse', 'Makes the target act erratically.', ss.id, NULL, NULL, at.id, 6, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Provoke', 'Enrages the target, reducing its defenses.', ss.id, NULL, NULL, at.id, 5, 60, 1, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Sacrifice', 'Sacrifice own HP to empower an ally.', ss.id, NULL, NULL, at.id, 0, 50, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Blind', 'Robs the target of sight.', ss.id, NULL, NULL, at.id, 5, 65, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Root', 'Anchors the target to the ground.', ss.id, NULL, NULL, at.id, 5, 65, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Entangle', 'Calls roots from the ground to hold enemies in place.', ss.id, NULL, NULL, at.id, 5, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Curse', 'Lays a dark curse on the target, weakening their resolve.', ss.id, NULL, NULL, at.id, 6, 70, 2, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'CC' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Summon Creature', 'Calls a creature to fight for you.', ss.id, NULL, NULL, at.id, 12, 85, 4, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'Other' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Summon: Spirit Wolf', 'Summons a spirit wolf to protect and fight alongside its master.', ss.id, NULL, NULL, at.id, 12, 100, 4, 1, 0, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.attack_type at
WHERE ss.name = 'Conjuration' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Fireball', 'A blazing orb of fire that explodes on impact.', ss.id, d.id, dt.id, at.id, 8, 90, 3, 3, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D6' AND dt.name = 'Fire' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Ice Bolt', 'A shard of enchanted ice that pierces and slows.', ss.id, d.id, dt.id, at.id, 6, 80, 2, 2, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D8' AND dt.name = 'Ice' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Shadow Bolt', 'A bolt of shadow energy that drains vitality.', ss.id, d.id, dt.id, at.id, 5, 70, 2, 2, 1, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D6' AND dt.name = 'Shadow' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Smite', 'A powerful holy strike channelled through the caster.', ss.id, d.id, dt.id, at.id, 8, 85, 3, 2, 3, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D8' AND dt.name = 'Holy' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Moonfire', 'Sacred moonlight burns unholy enemies and heals allies.', ss.id, d.id, dt.id, at.id, 6, 75, 2, 2, 2, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D6' AND dt.name = 'Holy' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.spell (name, description, school_id, damage_die_id, damage_type_id, attack_type_id, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage)
SELECT 'Soul Drain', 'Drains the life force of an enemy to restore the caster.', ss.id, d.id, dt.id, at.id, 7, 80, 3, 2, 1, 0, 'None', 0
FROM arena_data.spell_school ss, arena_data.die_type d, arena_data.damage_type dt, arena_data.attack_type at
WHERE ss.name = 'Evocation' AND d.name = 'D6' AND dt.name = 'Shadow' AND at.name = 'Spell'
ON CONFLICT (name) DO NOTHING;


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

