-- ============================================================
-- BattleArena - PostgreSQL Character Seed Data
-- Single test character for API smoke-testing.
-- Main roster lives in BattleArena.Gui/Data/roster.json.
-- ============================================================

INSERT INTO arena_data.character (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, max_hit_points, current_hit_points, strike_rating, turn_speed, npc, sex, biography, max_mana)
SELECT 'Captain Torvin', r.id, c.id, 7, 16, 12, 15, 10, 12, 13, 57, 57, 15, 10, 1, 'M',
       'A retired captain of the City Watch who now runs a small weapons shop in the market district. He lost his left eye to a goblin arrow during the Goblin Wars and claims it gave him better judgment of character.',
       0
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.character WHERE name = 'Captain Torvin');


-- Equipment
WITH weapon_map (char_name, slot_name, item_name) AS (VALUES
    ('Captain Torvin', 'RightHand', 'Long Sword')
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

WITH armor_map (char_name, slot_name, item_name) AS (VALUES
    ('Captain Torvin', 'Chest', 'Chain Mail')
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
