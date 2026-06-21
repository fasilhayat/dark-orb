-- Seed quest data

INSERT INTO arena_data.quest (name, description, quest_type, level_requirement, reward_xp)
VALUES
    ('The Road to Aeltharion', 'Reach the city of Aeltharion and report to the guildmaster.', 'Main', 1, 200),
    ('Clearing the Warrens', 'Clear the vermin from the old trade route tunnels.', 'Side', 3, 150),
    ('Path of the Blade', 'Prove your combat prowess by defeating 5 enemies in tactical combat.', 'Class', 2, 300);
