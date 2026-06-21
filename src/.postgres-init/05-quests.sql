-- Quest system tables and programmability (independent module)

CREATE TABLE IF NOT EXISTS arena_data.quest (
    id                 SERIAL PRIMARY KEY,
    name               VARCHAR(100) NOT NULL,
    description        TEXT,
    quest_type         VARCHAR(20) NOT NULL CHECK (quest_type IN ('Main', 'Side', 'Class')),
    level_requirement  INTEGER NOT NULL DEFAULT 1,
    reward_xp          INTEGER NOT NULL DEFAULT 0,
    prereq_quest_ids   INTEGER[] DEFAULT '{}',
    location_id        VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS arena_data.character_quest (
    character_id   INTEGER NOT NULL REFERENCES arena_data.character(id) ON DELETE CASCADE,
    quest_id       INTEGER NOT NULL REFERENCES arena_data.quest(id) ON DELETE CASCADE,
    status         VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Completed', 'Failed')),
    progress       JSONB NOT NULL DEFAULT '{}',
    completed_at   TIMESTAMP,
    PRIMARY KEY (character_id, quest_id)
);


-- ── Functions ─────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION arena_data.fn_get_quests()
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT, quest_type VARCHAR,
    level_requirement INTEGER, reward_xp INTEGER,
    prereq_quest_ids INTEGER[], location_id VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT q.id, q.name::VARCHAR, q.description::TEXT, q.quest_type::VARCHAR,
           q.level_requirement, q.reward_xp,
           q.prereq_quest_ids, q.location_id::VARCHAR
    FROM arena_data.quest q
    ORDER BY q.level_requirement, q.id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_quests_by_level(p_level INTEGER)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT, quest_type VARCHAR,
    level_requirement INTEGER, reward_xp INTEGER,
    prereq_quest_ids INTEGER[], location_id VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT q.id, q.name::VARCHAR, q.description::TEXT, q.quest_type::VARCHAR,
           q.level_requirement, q.reward_xp,
           q.prereq_quest_ids, q.location_id::VARCHAR
    FROM arena_data.quest q
    WHERE q.level_requirement <= p_level
    ORDER BY q.level_requirement, q.id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_quest(p_id INTEGER)
RETURNS TABLE(
    id INTEGER, name VARCHAR, description TEXT, quest_type VARCHAR,
    level_requirement INTEGER, reward_xp INTEGER,
    prereq_quest_ids INTEGER[], location_id VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT q.id, q.name::VARCHAR, q.description::TEXT, q.quest_type::VARCHAR,
           q.level_requirement, q.reward_xp,
           q.prereq_quest_ids, q.location_id::VARCHAR
    FROM arena_data.quest q
    WHERE q.id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_create_quest(
    p_name VARCHAR,
    p_description TEXT DEFAULT '',
    p_quest_type VARCHAR DEFAULT 'Side',
    p_level_requirement INTEGER DEFAULT 1,
    p_reward_xp INTEGER DEFAULT 0,
    p_prereq_quest_ids INTEGER[] DEFAULT '{}',
    p_location_id VARCHAR DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.quest (name, description, quest_type, level_requirement, reward_xp, prereq_quest_ids, location_id)
    VALUES (p_name, p_description, p_quest_type, p_level_requirement, p_reward_xp, p_prereq_quest_ids, p_location_id)
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_character_quests(p_character_id INTEGER, p_status VARCHAR DEFAULT NULL)
RETURNS TABLE(
    character_id INTEGER, quest_id INTEGER, status VARCHAR,
    progress JSONB, completed_at TIMESTAMP
) AS $$
BEGIN
    IF p_status IS NOT NULL THEN
        RETURN QUERY
        SELECT cq.character_id, cq.quest_id, cq.status::VARCHAR, cq.progress, cq.completed_at
        FROM arena_data.character_quest cq
        WHERE cq.character_id = p_character_id AND cq.status = p_status
        ORDER BY cq.quest_id;
    ELSE
        RETURN QUERY
        SELECT cq.character_id, cq.quest_id, cq.status::VARCHAR, cq.progress, cq.completed_at
        FROM arena_data.character_quest cq
        WHERE cq.character_id = p_character_id
        ORDER BY cq.quest_id;
    END IF;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION arena_data.fn_get_character_quest(p_character_id INTEGER, p_quest_id INTEGER)
RETURNS TABLE(
    character_id INTEGER, quest_id INTEGER, status VARCHAR,
    progress JSONB, completed_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT cq.character_id, cq.quest_id, cq.status::VARCHAR, cq.progress, cq.completed_at
    FROM arena_data.character_quest cq
    WHERE cq.character_id = p_character_id AND cq.quest_id = p_quest_id;
END;
$$ LANGUAGE plpgsql;


-- ── Procedures ────────────────────────────────────────────────────

CREATE OR REPLACE PROCEDURE arena_data.sp_update_quest(
    p_id INTEGER,
    p_name VARCHAR,
    p_description TEXT DEFAULT NULL,
    p_quest_type VARCHAR DEFAULT NULL,
    p_level_requirement INTEGER DEFAULT NULL,
    p_reward_xp INTEGER DEFAULT NULL,
    p_prereq_quest_ids INTEGER[] DEFAULT NULL,
    p_location_id VARCHAR DEFAULT NULL
)
AS $$
BEGIN
    UPDATE arena_data.quest
    SET name = p_name,
        description = COALESCE(p_description, description),
        quest_type = COALESCE(p_quest_type, quest_type),
        level_requirement = COALESCE(p_level_requirement, level_requirement),
        reward_xp = COALESCE(p_reward_xp, reward_xp),
        prereq_quest_ids = COALESCE(p_prereq_quest_ids, prereq_quest_ids),
        location_id = COALESCE(p_location_id, location_id)
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE arena_data.sp_delete_quest(p_id INTEGER)
AS $$
BEGIN
    DELETE FROM arena_data.character_quest WHERE quest_id = p_id;
    DELETE FROM arena_data.quest           WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE arena_data.sp_accept_quest(p_character_id INTEGER, p_quest_id INTEGER)
AS $$
BEGIN
    INSERT INTO arena_data.character_quest (character_id, quest_id, status, progress)
    VALUES (p_character_id, p_quest_id, 'Active', '{}');
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE arena_data.sp_update_quest_progress(
    p_character_id INTEGER,
    p_quest_id INTEGER,
    p_progress TEXT
)
AS $$
BEGIN
    UPDATE arena_data.character_quest
    SET progress = p_progress::JSONB
    WHERE character_id = p_character_id AND quest_id = p_quest_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE arena_data.sp_complete_quest(p_character_id INTEGER, p_quest_id INTEGER)
AS $$
BEGIN
    UPDATE arena_data.character_quest
    SET status = 'Completed', completed_at = NOW()
    WHERE character_id = p_character_id AND quest_id = p_quest_id;
END;
$$ LANGUAGE plpgsql;
