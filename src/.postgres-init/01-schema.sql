-- ============================================================
-- BattleArena - PostgreSQL Schema and Programmability
-- Contains schema DDL, ALTER TABLE changes, functions, procedures,
-- and database initialization setup that must run before seed data.
-- ============================================================
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
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT NOT NULL DEFAULT ''
);


CREATE TABLE IF NOT EXISTS arena_data.equipment_slot (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);


-- ============================================================
-- RACES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.race (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    base_movement_speed INTEGER NOT NULL DEFAULT 30,
    strength_bonus INTEGER NOT NULL DEFAULT 0,
    dexterity_bonus INTEGER NOT NULL DEFAULT 0,
    stamina_bonus INTEGER NOT NULL DEFAULT 0,
    intelligence_bonus INTEGER NOT NULL DEFAULT 0,
    wisdom_bonus INTEGER NOT NULL DEFAULT 0,
    charisma_bonus INTEGER NOT NULL DEFAULT 0,
    description TEXT DEFAULT '',
    hit_point_bonus INTEGER NOT NULL DEFAULT 0,
    is_playable BOOLEAN NOT NULL DEFAULT TRUE,
    strength_min INTEGER NOT NULL DEFAULT 3,
    dexterity_min INTEGER NOT NULL DEFAULT 3,
    stamina_min INTEGER NOT NULL DEFAULT 3,
    intelligence_min INTEGER NOT NULL DEFAULT 3,
    wisdom_min INTEGER NOT NULL DEFAULT 3,
    charisma_min INTEGER NOT NULL DEFAULT 3,
    strength_max INTEGER NOT NULL DEFAULT 18,
    dexterity_max INTEGER NOT NULL DEFAULT 18,
    stamina_max INTEGER NOT NULL DEFAULT 18,
    intelligence_max INTEGER NOT NULL DEFAULT 18,
    wisdom_max INTEGER NOT NULL DEFAULT 18,
    charisma_max INTEGER NOT NULL DEFAULT 18,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS arena_data.subrace (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    name VARCHAR(50) NOT NULL,
    description TEXT DEFAULT '',
    strength_bonus INTEGER NOT NULL DEFAULT 0,
    dexterity_bonus INTEGER NOT NULL DEFAULT 0,
    stamina_bonus INTEGER NOT NULL DEFAULT 0,
    intelligence_bonus INTEGER NOT NULL DEFAULT 0,
    wisdom_bonus INTEGER NOT NULL DEFAULT 0,
    charisma_bonus INTEGER NOT NULL DEFAULT 0,
    hit_point_bonus INTEGER NOT NULL DEFAULT 0
);


CREATE TABLE IF NOT EXISTS arena_data.race_special_ability (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    name VARCHAR(100) NOT NULL,
    description TEXT DEFAULT ''
);


-- ============================================================
-- FEAT RESISTANCE JUNCTION TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.feat_resistance (
    id SERIAL PRIMARY KEY,
    feat_id INTEGER NOT NULL REFERENCES arena_data.race_special_ability(id),
    resistance_type VARCHAR(50) NOT NULL,
    resistance_value INTEGER NOT NULL DEFAULT 0
);


-- ============================================================
-- SUBRACE SPECIAL ABILITIES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.subrace_special_ability (
    id SERIAL PRIMARY KEY,
    subrace_id INTEGER NOT NULL REFERENCES arena_data.subrace(id),
    name VARCHAR(100) NOT NULL,
    description TEXT DEFAULT '',
    attack_bonus INTEGER NOT NULL DEFAULT 0,
    defense_bonus INTEGER NOT NULL DEFAULT 0
);


CREATE TABLE IF NOT EXISTS arena_data.subrace_feat_resistance (
    id SERIAL PRIMARY KEY,
    feat_id INTEGER NOT NULL REFERENCES arena_data.subrace_special_ability(id),
    resistance_type VARCHAR(50) NOT NULL,
    resistance_value INTEGER NOT NULL DEFAULT 0
);


-- ============================================================
-- CLASSES
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.class (
    id SERIAL PRIMARY KEY,
    hit_die_id INTEGER NOT NULL REFERENCES arena_data.die_type(id),
    name VARCHAR(50) NOT NULL UNIQUE,
    base_strike_rating INTEGER NOT NULL DEFAULT 20,
    movement_bonus INTEGER NOT NULL DEFAULT 0,
    description TEXT DEFAULT '',
    attack_count INTEGER NOT NULL DEFAULT 1,
    bow_attack_count INTEGER NOT NULL DEFAULT 0,
    armor_restriction VARCHAR(20),
    can_dual_wield BOOLEAN NOT NULL DEFAULT FALSE,
    weapon_switch_cost NUMERIC(3,2) NOT NULL DEFAULT 1.0,
    two_handed_bonus INTEGER NOT NULL DEFAULT 0,
    shield_bonus_damage INTEGER NOT NULL DEFAULT 0,
    ranged_attack_bonus INTEGER NOT NULL DEFAULT 0
);


CREATE TABLE IF NOT EXISTS arena_data.class_race (
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id),
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    PRIMARY KEY (class_id, race_id)
);


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
    pet_id INTEGER NOT NULL REFERENCES arena_data.pet(id),
    class_id INTEGER NOT NULL REFERENCES arena_data.class(id),
    PRIMARY KEY (pet_id, class_id)
);


CREATE TABLE IF NOT EXISTS arena_data.pet_race_restriction (
    pet_id INTEGER NOT NULL REFERENCES arena_data.pet(id),
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    PRIMARY KEY (pet_id, race_id)
);


-- ============================================================
-- WEAPONS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.weapon_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT DEFAULT ''
);


-- ============================================================
-- CLASS-ITEM RESTRICTIONS
-- Mirrors ArchetypeWeaponExtensions in BattleArena.Core.
-- Each row is an ALLOWED pair: a character of class_id may equip weapons of weapon_type_id.
-- Enforced via fn_weapon_allowed_for_class + CHECK on character_equipment.
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.class_item_restriction (
    class_id       INTEGER NOT NULL REFERENCES arena_data.class(id),
    weapon_type_id INTEGER NOT NULL REFERENCES arena_data.weapon_type(id),
    PRIMARY KEY (class_id, weapon_type_id)
);


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
    minimum_strength INTEGER NOT NULL DEFAULT 0,
    attack_bonus INTEGER NOT NULL DEFAULT 0,
    cursed BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT DEFAULT '',
    curse_effect TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);


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
    mitigation INTEGER NOT NULL DEFAULT 0,
    turn_meter_penalty INTEGER NOT NULL DEFAULT 0,
    turn_meter_cost_reduction INTEGER NOT NULL DEFAULT 0,
    movement_penalty INTEGER NOT NULL DEFAULT 0,
    cursed BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT DEFAULT '',
    curse_effect TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);


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
    set_id INTEGER NOT NULL REFERENCES arena_data.item_set(id),
    pieces_required INTEGER NOT NULL CHECK (pieces_required >= 2),
    effect_description TEXT NOT NULL DEFAULT ''
);


-- Armor resistance junction table (created here so seed data can use it)
CREATE TABLE IF NOT EXISTS arena_data.armor_resistance (
    id SERIAL PRIMARY KEY,
    armor_id INTEGER NOT NULL REFERENCES arena_data.armor(id),
    resistance_type VARCHAR(50) NOT NULL,
    resistance_value INTEGER NOT NULL DEFAULT 0
);


-- ============================================================
-- ACCESSORIES (Rings, Amulets, Girdles)
-- Normalised: one reference table for type, one data table for all entries.
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.accessory_type (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);


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


-- ============================================================
-- SPELLS
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.spell (
    id SERIAL PRIMARY KEY,
    school_id INTEGER NOT NULL REFERENCES arena_data.spell_school(id),
    damage_die_id INTEGER REFERENCES arena_data.die_type(id),
    damage_type_id INTEGER REFERENCES arena_data.damage_type(id),
    attack_type_id INTEGER REFERENCES arena_data.attack_type(id),
    name VARCHAR(100) NOT NULL UNIQUE,
    mana_cost INTEGER NOT NULL DEFAULT 5,
    turn_meter_cost INTEGER NOT NULL DEFAULT 100,
    spell_level INTEGER NOT NULL DEFAULT 1,
    damage_count INTEGER NOT NULL DEFAULT 1,
    attack_bonus INTEGER NOT NULL DEFAULT 0,
    flat_damage_bonus INTEGER NOT NULL DEFAULT 0,
    elemental_type VARCHAR(50) DEFAULT 'None',
    elemental_damage INTEGER NOT NULL DEFAULT 0,
    description TEXT DEFAULT ''
);


-- ============================================================
-- CHARACTERS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.character (
    id SERIAL PRIMARY KEY,
    race_id INTEGER NOT NULL REFERENCES arena_data.race(id),
    subrace_id INTEGER REFERENCES arena_data.subrace(id),
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
    sex VARCHAR(1) NOT NULL DEFAULT 'X',
    biography TEXT DEFAULT '',
    npc SMALLINT NOT NULL DEFAULT 0 CHECK (npc IN (0, 1)),
    max_mana INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);


-- ============================================================
-- CHARACTER SPELLS
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.character_spell (
    id SERIAL PRIMARY KEY,
    character_id INTEGER NOT NULL REFERENCES arena_data.character(id),
    spell_id INTEGER NOT NULL REFERENCES arena_data.spell(id),
    UNIQUE (character_id, spell_id)
);


-- ============================================================
-- CHARACTER EQUIPMENT
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.character_equipment (
    id SERIAL PRIMARY KEY,
    character_id INTEGER NOT NULL REFERENCES arena_data.character(id),
    slot_id INTEGER NOT NULL REFERENCES arena_data.equipment_slot(id),
    item_type VARCHAR(10) NOT NULL,
    item_id INTEGER NOT NULL,
    UNIQUE (character_id, slot_id)
);


CREATE TABLE IF NOT EXISTS arena_data.character_inventory (
    id SERIAL PRIMARY KEY,
    character_id INTEGER NOT NULL REFERENCES arena_data.character(id),
    item_type VARCHAR(10) NOT NULL,
    item_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1
);


-- Validation function used by the CHECK constraint on character_equipment.
-- Returns TRUE for non-weapon rows and for weapons whose type exists in class_item_restriction.
CREATE OR REPLACE FUNCTION arena_data.fn_weapon_allowed_for_class(
    p_character_id INTEGER,
    p_item_type VARCHAR,
    p_item_id INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    v_class_id INTEGER;
    v_count INTEGER;
BEGIN
    IF p_item_type != 'weapon' THEN
        RETURN TRUE;
    END IF;

    SELECT class_id INTO v_class_id
    FROM arena_data.character
    WHERE id = p_character_id;

    SELECT COUNT(*) INTO v_count
    FROM arena_data.class_item_restriction cir
    JOIN arena_data.weapon w ON w.weapon_type_id = cir.weapon_type_id
    WHERE cir.class_id = v_class_id
      AND w.id = p_item_id;

    RETURN v_count > 0;
END;
$$ LANGUAGE plpgsql;


ALTER TABLE arena_data.character_equipment
    DROP CONSTRAINT IF EXISTS chk_weapon_class_allowed;

ALTER TABLE arena_data.character_equipment
    ADD CONSTRAINT chk_weapon_class_allowed
    CHECK (arena_data.fn_weapon_allowed_for_class(character_id, item_type, item_id));


-- ============================================================
-- FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_races(
    p_id INTEGER DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT,
    base_movement_speed INTEGER,
    strength_bonus INTEGER, dexterity_bonus INTEGER,
    stamina_bonus INTEGER, intelligence_bonus INTEGER,
    wisdom_bonus INTEGER, charisma_bonus INTEGER,
    is_playable BOOLEAN,
    strength_min INTEGER, dexterity_min INTEGER,
    stamina_min INTEGER, intelligence_min INTEGER,
    wisdom_min INTEGER, charisma_min INTEGER,
    strength_max INTEGER, dexterity_max INTEGER,
    stamina_max INTEGER, intelligence_max INTEGER,
    wisdom_max INTEGER, charisma_max INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.name::VARCHAR, r.description::TEXT,
           r.base_movement_speed,
           r.strength_bonus, r.dexterity_bonus, r.stamina_bonus,
           r.intelligence_bonus, r.wisdom_bonus, r.charisma_bonus,
           r.is_playable,
           r.strength_min, r.dexterity_min, r.stamina_min,
           r.intelligence_min, r.wisdom_min, r.charisma_min,
           r.strength_max, r.dexterity_max, r.stamina_max,
           r.intelligence_max, r.wisdom_max, r.charisma_max
    FROM arena_data.race r
    WHERE (p_id IS NULL OR r.id = p_id)
    ORDER BY r.name;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_subraces(
    p_race_id INTEGER DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, race_id INTEGER, name VARCHAR, description TEXT,
    strength_bonus INTEGER, dexterity_bonus INTEGER,
    stamina_bonus INTEGER, intelligence_bonus INTEGER,
    wisdom_bonus INTEGER, charisma_bonus INTEGER,
    hit_point_bonus INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.race_id, s.name::VARCHAR, s.description::TEXT,
           s.strength_bonus, s.dexterity_bonus, s.stamina_bonus,
           s.intelligence_bonus, s.wisdom_bonus, s.charisma_bonus,
           s.hit_point_bonus
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


CREATE OR REPLACE FUNCTION arena_data.fn_get_spell_schools()
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT ss.id, ss.name::VARCHAR, ss.description::TEXT
    FROM arena_data.spell_school ss
    ORDER BY ss.id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_classes()
RETURNS TABLE(id INTEGER, name VARCHAR, description TEXT, movement_bonus INTEGER, hit_die VARCHAR, base_strike_rating INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.description::TEXT, c.movement_bonus, d.name::VARCHAR AS hit_die, c.base_strike_rating
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
    quality VARCHAR, armor_class_bonus INTEGER,
    mitigation INTEGER, turn_meter_penalty INTEGER, turn_meter_cost_reduction INTEGER,
    movement_penalty INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.name::VARCHAR, a.description::TEXT,
           a.armor_class, ac.name::VARCHAR AS category,
           a.max_dexterity_bonus, a.stealth_disadvantage, a.strength_requirement,
           gq.name::VARCHAR AS quality,
           a.armor_class_bonus,
           a.mitigation, a.turn_meter_penalty, a.turn_meter_cost_reduction,
           a.movement_penalty
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
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT, school VARCHAR,
    mana_cost INTEGER, turn_meter_cost INTEGER, spell_level INTEGER,
    damage_count INTEGER, attack_bonus INTEGER, flat_damage_bonus INTEGER,
    elemental_type VARCHAR, elemental_damage INTEGER,
    damage_die VARCHAR, damage_type VARCHAR, attack_type VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.name::VARCHAR, s.description::TEXT, ss.name::VARCHAR AS school,
           s.mana_cost, s.turn_meter_cost, s.spell_level,
           s.damage_count, s.attack_bonus, s.flat_damage_bonus,
           s.elemental_type, s.elemental_damage,
           d.name::VARCHAR AS damage_die,
           dt.name::VARCHAR AS damage_type,
           at.name::VARCHAR AS attack_type
    FROM arena_data.spell s
    JOIN arena_data.spell_school ss ON ss.id = s.school_id
    LEFT JOIN arena_data.die_type d ON d.id = s.damage_die_id
    LEFT JOIN arena_data.damage_type dt ON dt.id = s.damage_type_id
    LEFT JOIN arena_data.attack_type at ON at.id = s.attack_type_id
    WHERE (p_school IS NULL OR ss.name = p_school)
    ORDER BY s.name;
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- CHARACTER EQUIPMENT FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_character_weapons(p_character_id INTEGER)
RETURNS TABLE(
    slot_name VARCHAR,
    id INTEGER, name VARCHAR, description TEXT,
    weapon_type VARCHAR, damage_die VARCHAR, damage_type VARCHAR,
    attack_type VARCHAR, damage_count INTEGER, hands INTEGER,
    quality VARCHAR, attack_bonus INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT es.name::VARCHAR AS slot_name,
           w.id, w.name::VARCHAR, w.description::TEXT,
           wt.name::VARCHAR AS weapon_type,
           d.name::VARCHAR AS damage_die,
           dt.name::VARCHAR AS damage_type,
           at.name::VARCHAR AS attack_type,
           w.damage_count, w.hands,
           gq.name::VARCHAR AS quality,
           w.attack_bonus
    FROM arena_data.character_equipment ce
    JOIN arena_data.equipment_slot es ON es.id = ce.slot_id
    JOIN arena_data.weapon w ON w.id = ce.item_id AND ce.item_type = 'weapon'
    JOIN arena_data.weapon_type wt ON wt.id = w.weapon_type_id
    JOIN arena_data.die_type d ON d.id = w.damage_die_id
    JOIN arena_data.damage_type dt ON dt.id = w.damage_type_id
    JOIN arena_data.attack_type at ON at.id = w.attack_type_id
    JOIN arena_data.gear_quality gq ON gq.id = w.gear_quality_id
    WHERE ce.character_id = p_character_id
    ORDER BY es.name;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_character_armor(p_character_id INTEGER)
RETURNS TABLE(
    slot_name VARCHAR,
    id INTEGER, name VARCHAR, description TEXT,
    armor_class INTEGER, category VARCHAR,
    max_dexterity_bonus INTEGER, stealth_disadvantage BOOLEAN,
    strength_requirement INTEGER, quality VARCHAR, armor_class_bonus INTEGER,
    mitigation INTEGER, turn_meter_penalty INTEGER, turn_meter_cost_reduction INTEGER,
    movement_penalty INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT es.name::VARCHAR AS slot_name,
           a.id, a.name::VARCHAR, a.description::TEXT,
           a.armor_class, ac.name::VARCHAR AS category,
           a.max_dexterity_bonus, a.stealth_disadvantage, a.strength_requirement,
           gq.name::VARCHAR AS quality,
           a.armor_class_bonus,
           a.mitigation, a.turn_meter_penalty, a.turn_meter_cost_reduction,
           a.movement_penalty
    FROM arena_data.character_equipment ce
    JOIN arena_data.equipment_slot es ON es.id = ce.slot_id
    JOIN arena_data.armor a ON a.id = ce.item_id AND ce.item_type = 'armor'
    JOIN arena_data.armor_category ac ON ac.id = a.armor_category_id
    JOIN arena_data.gear_quality gq ON gq.id = a.gear_quality_id
    WHERE ce.character_id = p_character_id
    ORDER BY es.name;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_character_spells(p_character_id INTEGER)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT, school VARCHAR,
    mana_cost INTEGER, turn_meter_cost INTEGER, spell_level INTEGER,
    damage_count INTEGER, attack_bonus INTEGER, flat_damage_bonus INTEGER,
    elemental_type VARCHAR, elemental_damage INTEGER,
    damage_die VARCHAR, damage_type VARCHAR, attack_type VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.name::VARCHAR, s.description::TEXT, ss.name::VARCHAR AS school,
           s.mana_cost, s.turn_meter_cost, s.spell_level,
           s.damage_count, s.attack_bonus, s.flat_damage_bonus,
           s.elemental_type, s.elemental_damage,
           d.name::VARCHAR AS damage_die,
           dt.name::VARCHAR AS damage_type,
           at.name::VARCHAR AS attack_type
    FROM arena_data.character_spell cs
    JOIN arena_data.spell s ON s.id = cs.spell_id
    JOIN arena_data.spell_school ss ON ss.id = s.school_id
    LEFT JOIN arena_data.die_type d ON d.id = s.damage_die_id
    LEFT JOIN arena_data.damage_type dt ON dt.id = s.damage_type_id
    LEFT JOIN arena_data.attack_type at ON at.id = s.attack_type_id
    WHERE cs.character_id = p_character_id
    ORDER BY s.name;
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- RESISTANCE FUNCTIONS
-- ============================================================

CREATE OR REPLACE FUNCTION arena_data.fn_get_armor_resistances(p_armor_id INTEGER DEFAULT NULL)
RETURNS TABLE(id INTEGER, armor_id INTEGER, resistance_type VARCHAR, resistance_value INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT ar.id, ar.armor_id, ar.resistance_type, ar.resistance_value
    FROM arena_data.armor_resistance ar
    WHERE (p_armor_id IS NULL OR ar.armor_id = p_armor_id)
    ORDER BY ar.resistance_type;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_feat_resistances(p_feat_id INTEGER DEFAULT NULL)
RETURNS TABLE(id INTEGER, feat_id INTEGER, resistance_type VARCHAR, resistance_value INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT fr.id, fr.feat_id, fr.resistance_type, fr.resistance_value
    FROM arena_data.feat_resistance fr
    WHERE (p_feat_id IS NULL OR fr.feat_id = p_feat_id)
    ORDER BY fr.resistance_type;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_subrace_abilities(
    p_subrace_id INTEGER DEFAULT NULL
)
RETURNS TABLE(id INTEGER, subrace_id INTEGER, name VARCHAR, description TEXT, attack_bonus INTEGER, defense_bonus INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT sa.id, sa.subrace_id, sa.name::VARCHAR, sa.description::TEXT, sa.attack_bonus, sa.defense_bonus
    FROM arena_data.subrace_special_ability sa
    WHERE (p_subrace_id IS NULL OR sa.subrace_id = p_subrace_id)
    ORDER BY sa.name;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_subrace_feat_resistances(p_feat_id INTEGER DEFAULT NULL)
RETURNS TABLE(id INTEGER, feat_id INTEGER, resistance_type VARCHAR, resistance_value INTEGER) AS $$
BEGIN
    RETURN QUERY
    SELECT fr.id, fr.feat_id, fr.resistance_type, fr.resistance_value
    FROM arena_data.subrace_feat_resistance fr
    WHERE (p_feat_id IS NULL OR fr.feat_id = p_feat_id)
    ORDER BY fr.resistance_type;
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- DEITIES
-- ============================================================

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
    id INTEGER, name VARCHAR, level INTEGER, race_id INTEGER, subrace_id INTEGER,
    class_id INTEGER, class_name VARCHAR,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    strength_percentile INTEGER, max_hit_points INTEGER, current_hit_points INTEGER,
    strike_rating INTEGER, turn_speed INTEGER,
    npc SMALLINT, biography TEXT,
    experience_points INTEGER, max_mana INTEGER, sex VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.level, c.race_id, c.subrace_id, c.class_id, cl.name::VARCHAR,
           c.strength, c.dexterity, c.stamina,
           c.intelligence, c.wisdom, c.charisma,
           c.strength_percentile, c.max_hit_points, c.current_hit_points,
           c.strike_rating, c.turn_speed,
           c.npc, c.biography::TEXT,
           c.experience_points, c.max_mana, c.sex::VARCHAR
    FROM arena_data.character c
    JOIN arena_data.class cl ON cl.id = c.class_id
    ORDER BY c.name;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_character(p_id INTEGER)
RETURNS TABLE(
    id INTEGER, name VARCHAR, level INTEGER, race_id INTEGER, subrace_id INTEGER,
    class_id INTEGER, class_name VARCHAR,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    strength_percentile INTEGER, max_hit_points INTEGER, current_hit_points INTEGER,
    strike_rating INTEGER, turn_speed INTEGER,
    npc SMALLINT, biography TEXT,
    experience_points INTEGER, max_mana INTEGER, sex VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.name::VARCHAR, c.level, c.race_id, c.subrace_id, c.class_id, cl.name::VARCHAR,
           c.strength, c.dexterity, c.stamina,
           c.intelligence, c.wisdom, c.charisma,
           c.strength_percentile, c.max_hit_points, c.current_hit_points,
           c.strike_rating, c.turn_speed,
           c.npc, c.biography::TEXT,
           c.experience_points, c.max_mana, c.sex::VARCHAR
    FROM arena_data.character c
    JOIN arena_data.class cl ON cl.id = c.class_id
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
    p_max_hit_points INTEGER DEFAULT 10,
    p_npc SMALLINT DEFAULT 0,
    p_biography TEXT DEFAULT '',
    p_experience_points INTEGER DEFAULT 0,
    p_max_mana INTEGER DEFAULT 0,
    p_subrace_id INTEGER DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
    v_strike_rating INTEGER;
BEGIN
    SELECT base_strike_rating INTO v_strike_rating FROM arena_data.class WHERE id = p_class_id;

    INSERT INTO arena_data.character (
        name, race_id, subrace_id, class_id, level,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        strength_percentile, max_hit_points, current_hit_points, strike_rating,
        npc, biography, experience_points, max_mana
    ) VALUES (
        p_name, p_race_id, p_subrace_id, p_class_id, 1,
        p_strength, p_dexterity, p_stamina, p_intelligence, p_wisdom, p_charisma,
        p_strength_percentile, p_max_hit_points, p_max_hit_points, v_strike_rating,
        p_npc, p_biography, p_experience_points, p_max_mana
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
    p_current_hit_points INTEGER DEFAULT 10,
    p_npc SMALLINT DEFAULT NULL,
    p_biography TEXT DEFAULT NULL,
    p_experience_points INTEGER DEFAULT NULL,
    p_max_mana INTEGER DEFAULT NULL
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
        npc = COALESCE(p_npc, npc),
        biography = COALESCE(p_biography, biography),
        experience_points = COALESCE(p_experience_points, experience_points),
        max_mana = COALESCE(p_max_mana, max_mana),
        updated_at = NOW()
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE arena_data.sp_delete_character(p_id INTEGER)
AS $$
BEGIN
    -- Delete child rows in dependency order before removing the character.
    DELETE FROM arena_data.character_spell     WHERE character_id = p_id;
    DELETE FROM arena_data.character_inventory WHERE character_id = p_id;
    DELETE FROM arena_data.character_equipment WHERE character_id = p_id;
    DELETE FROM arena_data.character           WHERE id           = p_id;
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
-- ITEM SETS
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
RETURNS TABLE(id INTEGER, name VARCHAR, race_id INTEGER, class_id INTEGER,
    race VARCHAR, class VARCHAR, level INTEGER,
    strength INTEGER, dexterity INTEGER, stamina INTEGER,
    intelligence INTEGER, wisdom INTEGER, charisma INTEGER,
    is_merchant BOOLEAN, is_quest_giver BOOLEAN, is_hostile BOOLEAN, biography TEXT) AS $$
BEGIN
    RETURN QUERY
    SELECT n.id, n.name::VARCHAR, n.race_id, n.class_id,
           r.name::VARCHAR AS race, c.name::VARCHAR AS class,
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
-- BESTIARY (Monster / Creature Stat Blocks)
-- ============================================================

CREATE TABLE IF NOT EXISTS arena_data.bestiary (
    id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL UNIQUE,
    level INTEGER NOT NULL,
    strength_bonus INTEGER NOT NULL DEFAULT 0,
    dexterity_bonus INTEGER NOT NULL DEFAULT 0,
    stamina_bonus INTEGER NOT NULL DEFAULT 0,
    intelligence_bonus INTEGER NOT NULL DEFAULT 0,
    wisdom_bonus INTEGER NOT NULL DEFAULT 0,
    charisma_bonus INTEGER NOT NULL DEFAULT 0,
    max_hit_points INTEGER NOT NULL,
    armor_class INTEGER NOT NULL,
    attack_description TEXT DEFAULT '',
    special_abilities TEXT DEFAULT '',
    description TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);


CREATE OR REPLACE FUNCTION arena_data.fn_get_bestiary(
    p_category VARCHAR(50) DEFAULT NULL,
    p_level INTEGER DEFAULT NULL
)
RETURNS TABLE(
    id INTEGER, category VARCHAR, name VARCHAR,
    level INTEGER,
    strength_bonus INTEGER, dexterity_bonus INTEGER,
    stamina_bonus INTEGER, intelligence_bonus INTEGER,
    wisdom_bonus INTEGER, charisma_bonus INTEGER,
    max_hit_points INTEGER, armor_class INTEGER,
    attack_description TEXT, special_abilities TEXT, description TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT b.id, b.category::VARCHAR, b.name::VARCHAR,
           b.level,
           b.strength_bonus, b.dexterity_bonus, b.stamina_bonus,
           b.intelligence_bonus, b.wisdom_bonus, b.charisma_bonus,
           b.max_hit_points, b.armor_class,
           b.attack_description::TEXT, b.special_abilities::TEXT, b.description::TEXT
    FROM arena_data.bestiary b
    WHERE (p_category IS NULL OR b.category = p_category)
      AND (p_level IS NULL OR b.level = p_level)
    ORDER BY b.level, b.name;
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

