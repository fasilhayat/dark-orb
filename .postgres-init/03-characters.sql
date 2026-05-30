-- ============================================================
-- BattleArena - PostgreSQL Character Seed Data
-- Contains playable and combat character seeds, their biography and
-- max_mana updates, plus character equipment/spell/inventory data.
-- ============================================================

-- ============================================================
-- SEED: SAMPLE CHARACTERS
-- ============================================================

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, strength_percentile, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Bruenor Battlehammer', r.id, c.id, 5, 18, 12, 18, 9, 13, 11, 76, 52, 52, 14, 12
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Fighter';


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Tanis Half-Elven', r.id, c.id, 5, 14, 16, 12, 14, 14, 16, 25, 25, 14, 18
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Rogue';


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, strength_percentile, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Karg Bloodfang', r.id, c.id, 6, 18, 10, 16, 7, 8, 9, 99, 62, 62, 13, 14
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Orc' AND c.name = 'Barbarian';


-- Additional playable characters for full 6-hero party demos
INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Elara Swiftwind', r.id, c.id, 5, 8, 14, 10, 18, 16, 14, 14, 14, 13, 10
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Elara Swiftwind');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Sir Aldric Vane', r.id, c.id, 6, 17, 10, 18, 11, 13, 14, 61, 61, 14, 8
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Knight'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sir Aldric Vane');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed)
SELECT 'Mira Brightholm', r.id, c.id, 4, 10, 14, 12, 15, 17, 16, 25, 25, 14, 12
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Priest'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Mira Brightholm');


-- ============================================================
-- SEED: ADDITIONAL CHARACTERS (both heroes and NPCs)
-- ============================================================

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Brorn Ironarm', r.id, c.id, 6, 18, 10, 18, 8, 10, 9, 63, 63, 13, 10, 0
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Barbarian'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Brorn Ironarm');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sylas Moonshadow', r.id, c.id, 5, 10, 16, 10, 17, 14, 15, 20, 20, 14, 16, 0
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Rogue'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sylas Moonshadow');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Captain Aldric', r.id, c.id, 7, 16, 12, 15, 10, 12, 13, 57, 57, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Captain Aldric');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sister Marigold', r.id, c.id, 9, 10, 10, 12, 14, 18, 16, 53, 53, 14, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Priest'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sister Marigold');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Rorik the Wanderer', r.id, c.id, 8, 18, 10, 18, 8, 10, 9, 82, 82, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Dwarf' AND c.name = 'Barbarian'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Rorik the Wanderer');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Selene Nightwhisper', r.id, c.id, 10, 8, 14, 10, 18, 14, 16, 26, 26, 14, 14, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Selene Nightwhisper');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Grommash Ironhide', r.id, c.id, 12, 20, 10, 18, 7, 8, 10, 118, 118, 13, 10, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Orc' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Grommash Ironhide');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Finn Swift', r.id, c.id, 6, 8, 18, 10, 12, 10, 16, 23, 23, 14, 18, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Halfling' AND c.name = 'Rogue'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Finn Swift');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'The Collector', r.id, c.id, 15, 10, 12, 10, 20, 16, 14, 39, 39, 15, 12, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'The Collector');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Morgath the Pale', r.id, c.id, 14, 18, 8, 16, 10, 12, 8, 123, 123, 14, 8, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Undead' AND c.name = 'Knight'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Morgath the Pale');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Sizzle', r.id, c.id, 5, 6, 14, 8, 16, 10, 12, 9, 9, 15, 14, 1
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Kobold' AND c.name = 'Mage'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Sizzle');


INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc)
SELECT 'Ivy Thornwood', r.id, c.id, 8, 10, 14, 12, 16, 18, 14, 47, 47, 14, 14, 1
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


UPDATE arena_data.character SET max_mana = 155 WHERE name = 'Elara Swiftwind';

UPDATE arena_data.character SET max_mana = 100 WHERE name = 'Mira Brightholm';

UPDATE arena_data.character SET max_mana = 200 WHERE name = 'Selene Nightwhisper';

UPDATE arena_data.character SET max_mana = 275 WHERE name = 'The Collector';

UPDATE arena_data.character SET max_mana = 135 WHERE name = 'Sizzle';

UPDATE arena_data.character SET max_mana = 105 WHERE name = 'Sister Marigold';

UPDATE arena_data.character SET max_mana = 145 WHERE name = 'Ivy Thornwood';


-- ============================================================
-- SEED: CHARACTER EQUIPMENT
-- ============================================================

-- Weapon assignments
WITH weapon_map (char_name, slot_name, item_name) AS (VALUES
    ('Bruenor Battlehammer', 'RightHand', 'Long Sword'),
    ('Tanis Half-Elven',     'RightHand', 'Short Sword'),
    ('Karg Bloodfang',       'RightHand', 'Battle Axe'),
    ('Sir Aldric Vane',      'RightHand', 'Long Sword'),
    ('Mira Brightholm',      'RightHand', 'Dagger'),
    ('Brorn Ironarm',        'RightHand', 'Maul'),
    ('Sylas Moonshadow',     'RightHand', 'Short Sword'),
    ('Captain Aldric',       'RightHand', 'Long Sword'),
    ('Sister Marigold',      'RightHand', 'Mace'),
    ('Rorik the Wanderer',   'RightHand', 'Battle Axe'),
    ('Grommash Ironhide',    'RightHand', 'Great Sword'),
    ('Finn Swift',           'RightHand', 'Dagger'),
    ('Sizzle',               'RightHand', 'Dagger'),
    ('Ivy Thornwood',        'RightHand', 'Quarter Staff')
)
INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
SELECT c.id, es.id, 'weapon', w.id
FROM weapon_map m
JOIN arena_data.character c ON c.name = m.char_name
JOIN arena_data.equipment_slot es ON es.name = m.slot_name
JOIN arena_data.weapon w ON w.name = m.item_name
WHERE NOT EXISTS (
    SELECT 1 FROM arena_data.character_equipment ce
    WHERE ce.character_id = c.id AND ce.slot_id = es.id
);


-- Chest armor assignments
WITH armor_map (char_name, slot_name, item_name) AS (VALUES
    ('Bruenor Battlehammer', 'Chest', 'Chain Mail'),
    ('Tanis Half-Elven',     'Chest', 'Studded Leather'),
    ('Karg Bloodfang',       'Chest', 'Hide Armor'),
    ('Elara Swiftwind',      'Chest', 'Leather Armor'),
    ('Sir Aldric Vane',      'Chest', 'Plate Armor'),
    ('Mira Brightholm',      'Chest', 'Chain Shirt'),
    ('Brorn Ironarm',        'Chest', 'Hide Armor'),
    ('Sylas Moonshadow',     'Chest', 'Studded Leather'),
    ('Captain Aldric',       'Chest', 'Chain Mail'),
    ('Sister Marigold',      'Chest', 'Chain Shirt'),
    ('Rorik the Wanderer',   'Chest', 'Hide Armor'),
    ('Selene Nightwhisper',  'Chest', 'Leather Armor'),
    ('Grommash Ironhide',    'Chest', 'Splint Armor'),
    ('Finn Swift',           'Chest', 'Leather Armor'),
    ('The Collector',        'Chest', 'Leather Armor'),
    ('Morgath the Pale',     'Chest', 'Plate Armor'),
    ('Sizzle',               'Chest', 'Leather Armor'),
    ('Ivy Thornwood',        'Chest', 'Leather Armor')
)
INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
SELECT c.id, es.id, 'armor', a.id
FROM armor_map m
JOIN arena_data.character c ON c.name = m.char_name
JOIN arena_data.equipment_slot es ON es.name = m.slot_name
JOIN arena_data.armor a ON a.name = m.item_name
WHERE NOT EXISTS (
    SELECT 1 FROM arena_data.character_equipment ce
    WHERE ce.character_id = c.id AND ce.slot_id = es.id
);


-- Shield assignments (left hand)
WITH shield_map (char_name, slot_name, item_name) AS (VALUES
    ('Bruenor Battlehammer', 'LeftHand', 'Shield'),
    ('Sir Aldric Vane',      'LeftHand', 'Shield')
)
INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
SELECT c.id, es.id, 'armor', a.id
FROM shield_map m
JOIN arena_data.character c ON c.name = m.char_name
JOIN arena_data.equipment_slot es ON es.name = m.slot_name
JOIN arena_data.armor a ON a.name = m.item_name
WHERE NOT EXISTS (
    SELECT 1 FROM arena_data.character_equipment ce
    WHERE ce.character_id = c.id AND ce.slot_id = es.id
);


-- ============================================================
-- SEED: CHARACTER SPELLS
-- ============================================================

-- Elara Swiftwind (Elf Mage)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Elara Swiftwind'
  AND s.name IN ('Blade Barrier', 'Ice Storm', 'Lightning Strike', 'Fireball')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- Selene Nightwhisper (Elf Mage NPC)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Selene Nightwhisper'
  AND s.name IN ('Fireball', 'Ice Storm', 'Lightning Strike')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- The Collector (Human Mage NPC)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'The Collector'
  AND s.name IN ('Blade Barrier', 'Ice Storm', 'Fire Storm', 'Acid Rain', 'Lightning Strike', 'Earthquake', 'Blizzard')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- Sizzle (Kobold Mage NPC)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Sizzle'
  AND s.name IN ('Fire Storm', 'Lava Hail')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- Ivy Thornwood (Elf Druid NPC)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Ivy Thornwood'
  AND s.name IN ('Insect Swarm', 'Root', 'Blinding Flash')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- Mira Brightholm (Human Priest)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Mira Brightholm'
  AND s.name IN ('Smite', 'Moonfire')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);


-- Sister Marigold (Human Priest NPC)
INSERT INTO arena_data.character_spell (character_id, spell_id)
SELECT c.id, s.id
FROM arena_data.character c, arena_data.spell s
WHERE c.name = 'Sister Marigold'
  AND s.name IN ('Smite', 'Sleep')
  AND NOT EXISTS (SELECT 1 FROM arena_data.character_spell cs WHERE cs.character_id = c.id AND cs.spell_id = s.id);

