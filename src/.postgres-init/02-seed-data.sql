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
    ('Ice'), ('Lightning'), ('Shadow'), ('Holy'),     ('Acid'), ('Psychic'), ('Healing')
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


INSERT INTO arena_data.spell_school (name, description) VALUES
    ('Aegis', 'Wards, protection, armor reinforcement, resistance, sanctuaries, and anti-magic.'),
    ('Stormcraft', 'Raw elemental force — fire, lightning, frost, detonations, and destructive hazards.'),
    ('Verdancy', 'Nature, beasts, roots, wind, stone, insects, herbs, and primal elemental power.'),
    ('Umbramancy', 'Dark magic — death, undead, shadow, curses, fear, life-drain, and sinister control.'),
    ('Mirage', 'Illusion, invisibility, mirror images, deception, confusion, stealth, and perception warping.'),
    ('Dominion', 'Command, blessing, morale, discipline, fear resistance, divine authority, and battle momentum.'),
    ('Deity', 'Divine magic channeled through deities — used by priests, druids, paladins, and knights.')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- SEED: SPELLS
-- All spells from dark-orb-master-spellbook.md
-- ============================================================

INSERT INTO arena_data.spell (school_id, damage_die_id, damage_type_id, attack_type_id, name, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage, description)
SELECT ss.id, dd.id, dt.id, at.id, s.name, s.mana_cost, s.turn_meter_cost, s.spell_level, s.damage_count, s.attack_bonus, s.flat_damage_bonus, s.elemental_type, s.elemental_damage, s.description
FROM (VALUES
    -- Mage Common Core (level 1-2)
    ('Stormcraft', 'D4', 'Force', 'Spell', 'Magic Missile',    10, 60, 1, 3, 2, 0, 'Force',    0, 'Reliable force darts that strike true.  Tags: Single-Target Damage, Nuke'),
    ('Aegis', 'D4', 'None', 'Spell', 'Armor',                   5, 60, 1, 0, 0, 0, 'None',     0, 'Magical armor that improves survivability.  Tags: Defensive, Buff'),
    ('Aegis', 'D4', 'None', 'Spell', 'Shield',                  5, 60, 1, 0, 0, 0, 'None',     0, 'Magical shield against attacks and missiles.  Tags: Defensive'),
    ('Stormcraft', 'D4', 'Fire', 'Spell', 'Burning Hands',     10, 60, 1, 1, 2, 0, 'Fire',     3, 'Short cone of flame that scorches nearby enemies.  Tags: Offensive, AoE'),
    ('Mirage', 'D4', 'None', 'Spell', 'Grease',                 8, 60, 1, 0, 0, 0, 'None',     0, 'Slippery coating that causes falls and handling failure.  Tags: CC, Slip, AoE'),
    ('Mirage', 'D4', 'None', 'Spell', 'Sleep',                  8, 60, 1, 0, 0, 0, 'None',     0, 'Puts weaker targets into magical sleep.  Tags: CC, AoE'),
    ('Mirage', 'D4', 'None', 'Spell', 'Color Spray',            8, 60, 1, 0, 0, 0, 'Light',    0, 'Cone of sensory overload that blinds, stuns, or drops weak targets.  Tags: CC, AoE'),
    ('Mirage', 'D4', 'None', 'Spell', 'Detect Magic',           5, 60, 1, 0, 0, 0, 'None',     0, 'Reveals magical auras and enchantments.  Tags: Utility'),
    ('Mirage', 'D4', 'None', 'Spell', 'Invisibility',          12, 70, 2, 0, 0, 0, 'None',     0, 'Makes a target unseen until broken.  Tags: Invisibility'),
    ('Mirage', 'D4', 'None', 'Spell', 'Mirror Image',          12, 70, 2, 0, 0, 0, 'None',     0, 'Creates illusory duplicates to absorb attacks.  Tags: Defensive, Image'),
    ('Dominion', 'D4', 'None', 'Spell', 'Web',                 15, 70, 2, 0, 0, 0, 'None',     0, 'Sticky strands trap and hinder enemies.  Tags: CC, Root, AoE'),
    ('Mirage', 'D4', 'Poison', 'Spell', 'Stinking Cloud',      15, 70, 2, 0, 0, 0, 'Poison',   0, 'Nauseating cloud that disrupts actions.  Tags: CC, AoE'),

    -- Mage Specialization (level 3+)
    ('Stormcraft', 'D6', 'Lightning', 'Spell', 'Lightning Bolt',    25, 80, 3, 3, 2, 0, 'Lightning', 0, 'Straight-line lightning blast through enemies.  Tags: Offensive, AoE, Nuke'),
    ('Stormcraft', 'D6', 'Fire', 'Spell', 'Fireball',               30, 90, 3, 3, 2, 0, 'Fire',      0, 'Explosive ranged fire burst for clustered targets.  Tags: Offensive, AoE, Nuke'),
    ('Mirage', 'D4', 'None', 'Spell', 'Blink',                     20, 80, 3, 0, 0, 0, 'None',      0, 'Phasing displacement defense.  Tags: Blink, Defensive'),
    ('Dominion', 'D4', 'None', 'Spell', 'Slow',                    20, 80, 3, 0, 0, 0, 'None',      0, 'Reduces enemy tempo and action efficiency.  Tags: CC, Debuff, Turn-Meter Control'),
    ('Umbramancy', 'D8', 'Shadow', 'Spell', 'Vampiric Touch',      25, 80, 3, 2, 2, 0, 'Shadow',    0, 'Melee life-drain spell that steals vitality.  Tags: Single-Target Damage, Leech'),
    ('Umbramancy', 'D4', 'None', 'Spell', 'Fear',                  22, 80, 4, 0, 0, 0, 'Shadow',    0, 'Sends enemies fleeing in panic.  Tags: CC, Debuff'),
    ('Stormcraft', 'D6', 'Ice', 'Spell', 'Ice Storm',              35, 90, 4, 4, 2, 0, 'Ice',       5, 'Area storm of cold and impact force.  Tags: Offensive, AoE'),
    ('Mirage', 'D4', 'None', 'Spell', 'Confusion',                 30, 90, 4, 0, 0, 0, 'None',      0, 'Scrambles enemy behavior and target selection.  Tags: CC, AoE'),
    ('Umbramancy', 'D6', 'Poison', 'Spell', 'Cloudkill',           40, 100, 5, 4, 2, 0, 'Poison',    0, 'Expanding poisonous cloud.  Tags: Offensive, AoE'),
    ('Stormcraft', 'D6', 'Ice', 'Spell', 'Cone of Cold',           40, 100, 5, 5, 2, 0, 'Ice',       0, 'Heavy cone-shaped cold burst.  Tags: Offensive, AoE, Nuke'),
    ('Umbramancy', 'D4', 'None', 'Spell', 'Feeblemind',            35, 100, 5, 0, 0, 0, 'None',      0, 'Cripples caster or intellectual function.  Tags: CC, Anti-Mage'),
    ('Stormcraft', 'D6', 'Fire', 'Spell', 'Delayed Blast Fireball',50, 110, 7, 5, 3, 0, 'Fire',      10, 'Timed explosive fire spell.  Tags: Offensive, AoE, Nuke'),
    ('Mirage', 'D4', 'None', 'Spell', 'Maze',                      45, 120, 8, 0, 0, 0, 'None',      0, 'Temporarily removes a target from the battlefield.  Tags: CC'),

    -- Mage variants
    ('Umbramancy', 'D4', 'Shadow', 'Spell', 'Mind Siphon',         25, 80, 4, 0, 2, 0, 'Shadow',    0, 'Dark anti-mage variant that drains magical reserves.  Tags: MP Leech, Variant'),
    ('Stormcraft', 'D6', 'Lightning', 'Spell', 'Arc Lash',         25, 80, 3, 2, 2, 0, 'Lightning', 0, 'Focused lightning lash that shocks one target.  Tags: Single-Target Damage, TM Control, Variant'),
    ('Mirage', 'D4', 'None', 'Spell', 'Mirror Guard',              22, 80, 3, 0, 0, 0, 'None',      0, 'Advanced mirror-image ward with partial retaliation.  Tags: Defensive, Variant'),
    ('Stormcraft', 'D6', 'Fire', 'Spell', 'Greasefire',            20, 75, 2, 2, 2, 0, 'Fire',      3, 'Ignites a grease field into a burning slick.  Tags: Offensive, AoE, Variant'),

    -- Legacy roster spells (mapped to new schools)
    ('Stormcraft', 'D8', 'Ice', 'Spell', 'Ice Bolt',               35, 80, 2, 2, 2, 0, 'Ice',       0, 'A bolt of ice that freezes the target.  Tags: Single-Target Damage'),
    ('Stormcraft', 'D6', 'Lightning', 'Spell', 'Shock',            20, 75, 2, 2, 2, 0, 'Lightning', 0, 'A jolt of electrical energy.  Tags: Single-Target Damage'),
    ('Stormcraft', 'D6', 'Lightning', 'Spell', 'Static Shock',     30, 80, 2, 1, 2, 0, 'Lightning', 0, 'A charged static shock that leaves lasting effects.  Tags: Single-Target Damage, Debuff'),

    -- Priest spells (Deity school)
    ('Deity', 'D4', 'None', 'Spell', 'Bless',                     10, 60, 1, 0, 0, 0, 'None',     0, 'Improves ally morale and combat performance.  Tags: Buff, AoE'),
    ('Deity', 'D4', 'None', 'Spell', 'Command',                   10, 60, 1, 0, 0, 0, 'None',     0, 'One-word forced action disrupting the target.  Tags: CC'),
    ('Deity', 'D4', 'Healing', 'Spell', 'Cure Light Wounds',     15, 60, 1, 1, 0, 4, 'None',     0, 'Basic divine healing.  Tags: Healing'),
    ('Deity', 'D4', 'None', 'Spell', 'Protection from Evil',     10, 60, 1, 0, 0, 0, 'None',     0, 'Defensive ward against evil influence.  Tags: Defensive, Buff'),
    ('Deity', 'D4', 'None', 'Spell', 'Chasten',                  10, 60, 1, 0, 0, 0, 'None',     0, 'Weakens sinful and hostile targets.  Tags: Debuff'),
    ('Deity', 'D4', 'None', 'Spell', 'Sanctuary',                10, 60, 1, 0, 0, 0, 'None',     0, 'Makes hostile creatures less likely to attack.  Tags: Defensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Aid',                      15, 70, 2, 0, 0, 0, 'None',     0, 'Supportive blessing that improves staying power.  Tags: Buff'),
    ('Deity', 'D4', 'None', 'Spell', 'Chant',                    15, 70, 2, 0, 0, 0, 'None',     0, 'Battlefield prayer that aids allies and hinders enemies.  Tags: Buff, Debuff'),
    ('Deity', 'D4', 'None', 'Spell', 'Hold Person',              18, 70, 2, 0, 0, 0, 'None',     0, 'Paralyzes humanoid targets.  Tags: CC'),
    ('Deity', 'D4', 'None', 'Spell', 'Prayer',                   20, 80, 3, 0, 0, 0, 'None',     0, 'Broad ally buff plus enemy penalty effect.  Tags: Buff, Debuff'),
    ('Deity', 'D4', 'None', 'Spell', 'Remove Paralysis',         12, 70, 3, 0, 0, 0, 'None',     0, 'Frees allies from paralysis.  Tags: Healing, Cleanse'),
    ('Deity', 'D4', 'Healing', 'Spell', 'Cure Serious Wounds',   25, 80, 4, 2, 0, 8, 'None',     0, 'Stronger direct healing.  Tags: Healing'),
    ('Deity', 'D4', 'None', 'Spell', 'Free Action',              18, 80, 4, 0, 0, 0, 'None',     0, 'Prevents many movement-impairing effects.  Tags: Defensive'),
    ('Deity', 'D4', 'Healing', 'Spell', 'Cure Critical Wounds',  35, 90, 5, 2, 0, 12, 'None',    0, 'Large heal for severe injuries.  Tags: Healing'),
    ('Deity', 'D6', 'Fire', 'Spell', 'Flame Strike',             30, 90, 5, 3, 2, 0, 'Fire',      5, 'Vertical divine column of holy fire.  Tags: Offensive, Nuke'),
    ('Deity', 'D8', 'Healing', 'Spell', 'Heal',                  25, 80, 6, 2, 0, 12, 'None',     0, 'Major restorative miracle.  Tags: Healing'),
    ('Deity', 'D6', 'Physical', 'Spell', 'Blade Barrier',        35, 100, 6, 4, 2, 0, 'None',     0, 'Wall or ring of whirling blades.  Tags: Offensive, Defensive, Barrier'),
    ('Deity', 'D4', 'None', 'Spell', 'Heroes Feast',             40, 100, 6, 0, 0, 0, 'None',     0, 'Group pre-battle meal with strong support benefits.  Tags: Buff, AoE'),
    ('Deity', 'D4', 'None', 'Spell', 'Restoration',              30, 90, 7, 0, 0, 0, 'None',     0, 'Repairs severe spiritual or life-force harm.  Tags: Healing, Cleanse'),

    -- Druid spells
    ('Deity', 'D4', 'None', 'Spell', 'Entangle',                 10, 60, 1, 0, 0, 0, 'None',     0, 'Plants twist around creatures and restrain them.  Tags: CC, Root'),
    ('Deity', 'D4', 'None', 'Spell', 'Faerie Fire',              10, 60, 1, 0, 0, 0, 'None',     0, 'Outlines targets, countering stealth.  Tags: Debuff'),
    ('Deity', 'D4', 'Physical', 'Spell', 'Shillelagh',            5, 60, 1, 0, 2, 0, 'None',     0, 'Enchants a club or staff to hit harder.  Tags: Buff'),
    ('Deity', 'D4', 'None', 'Spell', 'Barkskin',                 15, 70, 2, 0, 0, 0, 'None',     0, 'Skin becomes as tough as bark.  Tags: Defensive'),
    ('Deity', 'D4', 'Healing', 'Spell', 'Goodberry',             12, 70, 2, 1, 0, 4, 'None',     0, 'Creates restorative berries.  Tags: Healing'),
    ('Deity', 'D4', 'Fire', 'Spell', 'Heat Metal',              15, 70, 2, 1, 2, 0, 'Fire',      3, 'Punishes armored enemies through heat.  Tags: Debuff'),
    ('Deity', 'D6', 'Lightning', 'Spell', 'Call Lightning',      25, 80, 3, 3, 2, 0, 'Lightning', 0, 'Repeated lightning strikes from a storm.  Tags: Offensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Hold Animal',              18, 70, 3, 0, 0, 0, 'None',     0, 'Immobilizes beasts.  Tags: CC'),
    ('Deity', 'D4', 'None', 'Spell', 'Call Woodland Beings',    25, 80, 4, 0, 0, 0, 'None',     0, 'Summons nature spirits or woodland allies.  Tags: Summoning'),
    ('Deity', 'D4', 'Physical', 'Spell', 'Giant Insect',         25, 80, 4, 2, 2, 0, 'None',     0, 'Enlarges vermin into combat-capable forms.  Tags: Summoning-lite'),
    ('Deity', 'D4', 'Poison', 'Spell', 'Insect Plague',          35, 90, 5, 3, 2, 0, 'Poison',    0, 'Swarming insects disrupt and overwhelm groups.  Tags: Offensive, CC'),
    ('Deity', 'D4', 'None', 'Spell', 'Anti-Plant Shell',        25, 80, 5, 0, 0, 0, 'None',     0, 'Prevents plant creatures from closing in.  Tags: Defensive'),
    ('Deity', 'D6', 'Fire', 'Spell', 'Fire Seeds',              35, 90, 6, 4, 2, 0, 'Fire',      5, 'Druid explosive seeds used as bombs or traps.  Tags: Offensive'),
    ('Deity', 'D4', 'Physical', 'Spell', 'Liveoak',              40, 100, 6, 0, 0, 0, 'None',     0, 'Awakens a great tree guardian.  Tags: Summoning'),
    ('Deity', 'D4', 'Physical', 'Spell', 'Creeping Doom',        45, 100, 7, 4, 2, 0, 'Poison',    0, 'Devastating moving swarm.  Tags: Offensive, CC'),
    ('Deity', 'D6', 'Physical', 'Spell', 'Earthquake',           45, 110, 7, 5, 2, 0, 'None',     0, 'Wide-area terrain disruption.  Tags: Offensive, AoE'),

    -- Paladin spells
    ('Deity', 'D4', 'None', 'Spell', 'Remove Fear',              10, 60, 1, 0, 0, 0, 'None',     0, 'Clears fear and bolsters courage.  Tags: Buff, Cleanse'),
    ('Deity', 'D8', 'Holy', 'Spell', 'Smite',                    35, 80, 2, 2, 2, 0, 'None',     0, 'Divine strike against enemies.  Tags: Offensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Resist Fire',              12, 70, 2, 0, 0, 0, 'Fire',     0, 'Grants fire resistance.  Tags: Defensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Resist Cold',              12, 70, 2, 0, 0, 0, 'Ice',      0, 'Grants cold resistance.  Tags: Defensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Magical Vestment',         18, 80, 3, 0, 0, 0, 'None',     0, 'Enhances armor or shield with divine power.  Tags: Buff, Defensive'),
    ('Deity', 'D4', 'None', 'Spell', 'Protection from Evil 10ft',20, 80, 4, 0, 0, 0, 'None',     0, 'Group protection aura against evil.  Tags: Defensive, AoE'),
    ('Deity', 'D4', 'None', 'Spell', 'Holy Bulwark',             25, 80, 4, 0, 0, 0, 'None',     0, 'Elite paladin ward for nearby allies.  Tags: Defensive, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Paladin Warcry',           20, 80, 3, 0, 0, 0, 'None',     0, 'Inspiring holy battle-cry that rallies allies.  Tags: Buff, AoE, Variant'),

    -- Knight spells
    ('Deity', 'D4', 'None', 'Spell', 'War Cry',                  15, 70, 1, 0, 0, 0, 'None',     0, 'Battle shout that shocks enemies or steels allies.  Tags: CC or Buff, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Rallying Cry',             15, 70, 1, 0, 0, 0, 'None',     0, 'Calls allies back into formation.  Tags: Buff, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Steadfast Line',           18, 75, 2, 0, 0, 0, 'None',     0, 'Reinforces discipline and formation stability.  Tags: Buff, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Banner of Resolve',        18, 75, 2, 0, 0, 0, 'None',     0, 'Banner magic that hardens allied will.  Tags: Buff, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Iron Will Litany',         22, 80, 3, 0, 0, 0, 'None',     0, 'Litany of discipline against hostile magic.  Tags: Defensive, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Advance Signal',           20, 80, 3, 0, 0, 0, 'None',     0, 'Tactical call to press the attack.  Tags: Buff, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Shielding Cadence',        22, 80, 3, 0, 0, 0, 'None',     0, 'Rhythmic command that improves survival.  Tags: Defensive, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Battle Hymn of Defiance', 30, 90, 4, 0, 0, 0, 'None',     0, 'Powerful morale chant for large engagements.  Tags: Buff, AoE, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Arcane Defiance Banner',  30, 90, 4, 0, 0, 0, 'None',     0, 'Elite banner ward against sorcery.  Tags: Defensive, Variant'),
    ('Deity', 'D4', 'None', 'Spell', 'Lionheart Command',        35, 100, 4, 0, 0, 0, 'None',     0, 'Supreme command that hardens allied resolve.  Tags: Buff, Variant'),

    -- Legacy roster spells (continued)
    ('Deity', 'D6', 'Healing', 'Spell', 'Mass Heal',             50, 100, 4, 3, 0, 6, 'None',     0, 'Powerful group healing spell.  Tags: Healing, AoE')
) AS s(school_name, die_name, dmg_type_name, atk_type_name, name, mana_cost, turn_meter_cost, spell_level, damage_count, attack_bonus, flat_damage_bonus, elemental_type, elemental_damage, description)
JOIN arena_data.spell_school ss ON ss.name = s.school_name
JOIN arena_data.die_type dd ON dd.name = s.die_name
JOIN arena_data.damage_type dt ON dt.name = s.dmg_type_name
JOIN arena_data.attack_type at ON at.name = s.atk_type_name
ON CONFLICT (name) DO NOTHING;


INSERT INTO arena_data.equipment_slot (name) VALUES
    ('Head'), ('Chest'), ('Hands'), ('Waist'), ('Foot'),
    ('Neck'), ('Back'), ('RightHand'), ('LeftHand'), ('Banner'),
    ('Ring1'), ('Ring2'), ('Ornament')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- SEED: RACES
-- ============================================================

INSERT INTO arena_data.race (name, description, base_movement_speed, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, hit_point_bonus) VALUES
    ('Human',    'The children of the All-Father, humans are the most adaptable of the mortal races. From the barbarian hordes of the Frozen Wastes to the merchant princes of Eldergard, humanity''s ambition knows no bounds. Their settlements dot every corner of the realm, and their short lives burn twice as bright as the long-lived elves. No other race can match their versatility — a human may rise from peasant to king in a single lifetime.', 30, 1, 1, 1, 1, 1, 1, 0),
    ('Elf',      'Born from the tears of the Moon goddess, the elves are the eldest of the mortal races. Their connection to magic runs in their blood, granting them innate resistance to spells and a grace that other races find unsettling. High Elves study the arcane arts in crystal towers, Dark Elves weave shadows in the underdark, and Forest Elves move as whispers through the ancient woods. Elves measure time in centuries and rarely hurry.', 35, 0, 2, 0, 2, 0, 1, 0),
    ('Dwarf',    'Forged from the bones of the earth itself, dwarves are as stubborn as the mountains they call home. Their kingdoms stretch deep beneath the peaks, where they mine mithril and carve halls of breathtaking beauty. Dwarven smiths are unmatched in the mortal realm, and their resistance to magic makes them feared opponents. A dwarf''s word is their bond, and their grudges are recorded in stone to last ten generations.', 25, 2, 0, 2, 0, 1, 0, 2),
    ('Lizard',   'Scales shimmering like gemstones, the lizardfolk are the Silent Children of the Sun. They are descendants of the lesser dragons, evolved from those ancient bloodlines when the world was young. The draconic heritage runs deep in their veins — their scales, their resilience, their cold patience all echo the great wyrms. To outsiders they seem emotionless, but among their own kind they share deep bonds of loyalty. Swamp Lizards glide through poisonous marshes, Desert Lizards endure the searing heat, and Forest Lizards strike from the canopy with terrifying precision.', 30, 2, 0, 1, 0, 0, 0, 1),
    ('Undead',   'Not a race but a condition — souls that refused the call of the afterlife. Undead cannot be played as characters; they exist only as NPCs and monsters encountered in the world. Bound to their rotting bodies by sheer will or necromantic curse, they walk the mortal plane seeking purpose, vengeance, or redemption. Immune to fear and pain, they feel only the cold hunger of their existence. Some serve dark masters; others wander as lone penitents, searching for a peace that will not come.', 25, 1, 0, 0, 1, 0, 0, 0),
    ('Kobold',   'Small, scaly, and underestimated by every other race, kobolds are survivors. They dwell in the cracks of the world — forgotten mines, sewer networks, and the underbellies of great cities. Their natural cunning and magic resistance have kept them alive against larger, stronger foes. A kobold''s greatest weapon is not their claw or fang, but their cleverness. They build traps that would impress dwarves and tunnels that baffle even elves.', 25, 0, 2, 0, 1, 0, 0, 0),
    ('Demon',    'Hailing from the infernal planes beyond the mortal veil, demons are creatures of pure elemental chaos. Each demon embodies a primal force — fire demons burn with endless rage, shadow demons hunger for fear and despair. They enter the mortal world through rifts and summonings, bringing destruction in their wake. Yet some demons reject their nature, seeking redemption in a world that fears and despises them.', 30, 2, 0, 1, 1, 0, 1, 1),
    ('Orc',      'The Chosen of the War God, orcs were created to fight. Their muscles bulge with unnatural strength, and their bones knit faster than any other race. Orc society is built around the concept of ''Ushog'' — the eternal struggle that gives life meaning. They value strength above all else and respect only those who can defeat them in battle. Despite their savage reputation, orcish honor is absolute; an orc who gives their word will die before breaking it.', 30, 3, 0, 1, 0, 0, 0, 2),
    ('Ogre',     'Titans reduced by ages of separation from their divine ancestors, ogres are the largest of the mortal races. Standing twelve feet tall and built of solid muscle and thick bone, they are living battering rams. Mountain Ogres possess residual magic resistance from their giant bloodline, Hill Ogres throw boulders with deadly accuracy, Desert Ogres endure the harshest climates, and Forest Ogres can regenerate wounds at an alarming rate.', 25, 3, 0, 2, 0, 0, 0, 3),
    ('Gladefolk', 'The smallest of the civilized races, gladefolk possess a spirit that belies their stature. They believe in the power of luck, good food, and a warm hearth, yet they are among the bravest souls in battle. Gladefolk feel fear but refuse to show it, using their natural agility and sharp tongues to mock and taunt enemies into reckless charges. Forest Gladefolk move through woodland without a trace, while Hill Gladefolk are renowned for their hospitality and uncanny good fortune.', 25, 0, 2, 1, 0, 1, 1, 0),
    ('Half-Elf', 'Bridging two worlds, half-elves carry the grace of their elven heritage and the adaptability of their human blood. They are charismatic diplomats, natural leaders, and gifted magic-users. Welcomed in both human cities and elven enclaves yet belonging fully to neither, their dual heritage grants them insight and empathy beyond their years. Their pointed ears and ageless features hint at elven ancestry, while their drive and ambition are purely human.', 30, 0, 1, 0, 1, 0, 2, 0);


-- Non-playable races (Undead, Demon)
UPDATE arena_data.race SET is_playable = FALSE WHERE name IN ('Undead', 'Demon');


-- Racial stat minimums and maximums
UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 18, dexterity_max = 18, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Human';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 6,  stamina_min = 3,  intelligence_min = 8,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 18, dexterity_max = 19, stamina_max = 18, intelligence_max = 19, wisdom_max = 18, charisma_max = 18
WHERE name = 'Elf';

UPDATE arena_data.race SET
    strength_min = 6,  dexterity_min = 3,  stamina_min = 8,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 19, dexterity_max = 18, stamina_max = 19, intelligence_max = 18, wisdom_max = 18, charisma_max = 17
WHERE name = 'Dwarf';

UPDATE arena_data.race SET
    strength_min = 6,  dexterity_min = 3,  stamina_min = 6,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 19, dexterity_max = 18, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Lizard';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 6,  stamina_min = 3,  intelligence_min = 6,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 17, dexterity_max = 19, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Kobold';

UPDATE arena_data.race SET
    strength_min = 8,  dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 20, dexterity_max = 18, stamina_max = 19, intelligence_max = 17, wisdom_max = 17, charisma_max = 17
WHERE name = 'Orc';

UPDATE arena_data.race SET
    strength_min = 10, dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 20, dexterity_max = 17, stamina_max = 20, intelligence_max = 15, wisdom_max = 16, charisma_max = 15
WHERE name = 'Ogre';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 6,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 17, dexterity_max = 19, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Gladefolk';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 18, dexterity_max = 18, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Half-Elf';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 18, dexterity_max = 18, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Undead';

UPDATE arena_data.race SET
    strength_min = 3,  dexterity_min = 3,  stamina_min = 3,  intelligence_min = 3,  wisdom_min = 3,  charisma_min = 3,
    strength_max = 18, dexterity_max = 18, stamina_max = 18, intelligence_max = 18, wisdom_max = 18, charisma_max = 18
WHERE name = 'Demon';


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
    ('Gladefolk', 'Forest Gladefolk', 'Wood-wise gladefolk who disappear into foliage.'),
    ('Gladefolk', 'Hill Gladefolk',   'Pastoral folk known for luck and hospitality.'),
    ('Half-Elf', 'Half-High-Elf', 'Half-elves with high-elven heritage, inheriting keen intellect and innate magic.'),
    ('Half-Elf', 'Half-Wood-Elf', 'Half-elves with wood-elven heritage, gaining enhanced agility and woodland instincts.')
) AS s(race_name, name, descr)
JOIN arena_data.race r ON r.name = s.race_name;


-- Kobold subraces
INSERT INTO arena_data.subrace (race_id, name, description, strength_bonus, dexterity_bonus, stamina_bonus, intelligence_bonus, wisdom_bonus, charisma_bonus, hit_point_bonus)
SELECT r.id, s.name, s.descr, s.str, s.dex, s.sta, s.int, s.wis, s.cha, s.hp
FROM (VALUES
    ('Kobold', 'Cave Kobold',  'Cave kobolds are master trap-makers who dwell in the dark warrens beneath the world. Their keen intellect and natural cunning make them deadly in prepared positions.',   0, 1, 0, 2, 0, 0, 0),
    ('Kobold', 'Desert Kobold','Desert kobolds survive in the searing wastes by burrowing beneath the dunes. Their agility and heat-hardened scales let them strike and vanish like mirages.',          0, 2, 1, 0, 0, 0, 0),
    ('Kobold', 'Swamp Kobold', 'Swamp kobolds thrive in the poisonous marshes where even orcs fear to tread. Their hardy constitutions shrug off toxins that would fell larger creatures.',          0, 0, 2, 0, 1, 0, 1),
    ('Kobold', 'Forest Kobold','Forest kobolds are the unseen hunters of the deep woods, reading the language of leaves and shadows. They set snares that would shame a ranger.',                       0, 1, 0, 0, 2, 0, 0)
) AS s(race_name, name, descr, str, dex, sta, int, wis, cha, hp)
JOIN arena_data.race r ON r.name = s.race_name
WHERE NOT EXISTS (SELECT 1 FROM arena_data.subrace sb WHERE sb.race_id = r.id AND sb.name = s.name);


-- Subrace ability bonuses (stacked on top of race bonuses)
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 1, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'High Elf';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 1, hit_point_bonus = 0 WHERE name = 'Dark Elf';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Forest Elf';
UPDATE arena_data.subrace SET strength_bonus = 1, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Mountain Dwarf';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 1, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Hill Dwarf';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Swamp Lizard';
UPDATE arena_data.subrace SET strength_bonus = 1, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Desert Lizard';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Forest Lizard';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Green Orc';
UPDATE arena_data.subrace SET strength_bonus = 1, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Blue Orc';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Red Orc';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 2 WHERE name = 'Mountain Ogre';
UPDATE arena_data.subrace SET strength_bonus = 1, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Hill Ogre';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Desert Ogre';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 1, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 1 WHERE name = 'Forest Ogre';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Forest Gladefolk';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 1, hit_point_bonus = 0 WHERE name = 'Hill Gladefolk';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 0, stamina_bonus = 0, intelligence_bonus = 1, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Half-High-Elf';
UPDATE arena_data.subrace SET strength_bonus = 0, dexterity_bonus = 1, stamina_bonus = 0, intelligence_bonus = 0, wisdom_bonus = 0, charisma_bonus = 0, hit_point_bonus = 0 WHERE name = 'Half-Wood-Elf';


-- Subrace special abilities
INSERT INTO arena_data.subrace_special_ability (subrace_id, name, description, attack_bonus, defense_bonus)
SELECT s.id, a.name, a.descr, a.atk, a.def
FROM (VALUES
    ('High Elf',      'Arcane Affinity',   'Innate spellcasting: knows one cantrip. +1 Intelligence for spell attacks.', 1, 0),
    ('Dark Elf',      'Drow Magic',        'Can cast darkness and faerie fire once per day. +1 Charisma for spell DCs.', 0, 0),
    ('Forest Elf',    'Mask of the Wild',  'Can Hide in natural terrain even when lightly observed.', 0, 0),
    ('Mountain Dwarf', 'Stonecraft',       'Expert armourers: +1 AC when wearing heavy armour.', 0, 1),
    ('Hill Dwarf',    'Dwarven Toughness', 'Stubborn endurance: +2 HP per level.', 0, 0),
    ('Swamp Lizard',  'Toxin Resistance',  'Advantage on saves vs. poison; natural poison damage +1.', 1, 0),
    ('Desert Lizard', 'Heat Adaptation',   'Immune to extreme heat; fire resistance +10.', 0, 1),
    ('Forest Lizard', 'Camouflage Scales', 'Advantage on Stealth checks in forest terrain; +1 AC when ambushing.', 0, 1),
    ('Green Orc',     'Jungle Stalker',    'Move silently through undergrowth; poisoned weapon attacks deal +1 damage.', 1, 0),
    ('Blue Orc',      'Tides of War',      '+2 to damage rolls when below half HP.', 2, 0),
    ('Red Orc',       'Blood Frenzy',      '+1 attack bonus for each consecutive hit on the same target (max +3).', 1, 0),
    ('Mountain Ogre', 'Giant Blood',       'Residual giant lineage grants magic resistance +5 and +1 AC.', 0, 1),
    ('Hill Ogre',     'Boulder Toss',      'Can throw improvised projectiles dealing +1 damage die.', 1, 0),
    ('Desert Ogre',   'Heat Endurance',    'Fire resistance +15; ignore difficult terrain from sand.', 0, 1),
    ('Forest Ogre',   'Regeneration',      'Regain 2 HP per tick while below half HP.', 0, 0),
    ('Forest Gladefolk', 'Woodland Stride','Moving through non-magical terrain costs no extra movement.', 0, 0),
    ('Hill Gladefolk',   'Fortunate',       'May re-roll a single D20 once per combat.', 0, 0),
    ('Half-High-Elf',    'Cantrip Adept',   'Knows one wizard cantrip; +1 Intelligence for spell attacks.', 1, 0),
    ('Half-Wood-Elf',    'Fleet of Foot',   '+5 movement speed; can Dash as a bonus action once per combat.', 0, 0),
    ('Cave Kobold',      'Trap Master',     'Gains +2 to hit against targets that have already acted this round.', 2, 0),
    ('Desert Kobold',    'Sand Veil',       'Burrowing speed 10 ft; +2 AC when in sandy terrain.', 0, 2),
    ('Swamp Kobold',     'Marsh Dweller',    'Ignore poison damage; ignores movement penalties from swamp terrain.', 0, 1),
    ('Forest Kobold',    'Ambush Tactics',   '+1 attack bonus and +1 damage when attacking from hiding.', 1, 1)
) AS a(subrace_name, name, descr, atk, def)
JOIN arena_data.subrace s ON s.name = a.subrace_name;


-- Subrace feat resistances
INSERT INTO arena_data.subrace_feat_resistance (feat_id, resistance_type, resistance_value)
SELECT ssa.id, 'Fire', 10
FROM arena_data.subrace_special_ability ssa
JOIN arena_data.subrace s ON s.id = ssa.subrace_id
WHERE s.name = 'Desert Lizard' AND ssa.name = 'Heat Adaptation'
AND NOT EXISTS (SELECT 1 FROM arena_data.subrace_feat_resistance fr WHERE fr.feat_id = ssa.id);

INSERT INTO arena_data.subrace_feat_resistance (feat_id, resistance_type, resistance_value)
SELECT ssa.id, 'Fire', 15
FROM arena_data.subrace_special_ability ssa
JOIN arena_data.subrace s ON s.id = ssa.subrace_id
WHERE s.name = 'Desert Ogre' AND ssa.name = 'Heat Endurance'
AND NOT EXISTS (SELECT 1 FROM arena_data.subrace_feat_resistance fr WHERE fr.feat_id = ssa.id);

INSERT INTO arena_data.subrace_feat_resistance (feat_id, resistance_type, resistance_value)
SELECT ssa.id, 'Magic', 5
FROM arena_data.subrace_special_ability ssa
JOIN arena_data.subrace s ON s.id = ssa.subrace_id
WHERE s.name = 'Mountain Ogre' AND ssa.name = 'Giant Blood'
AND NOT EXISTS (SELECT 1 FROM arena_data.subrace_feat_resistance fr WHERE fr.feat_id = ssa.id);

INSERT INTO arena_data.subrace_feat_resistance (feat_id, resistance_type, resistance_value)
SELECT ssa.id, 'Poison', 25
FROM arena_data.subrace_special_ability ssa
JOIN arena_data.subrace s ON s.id = ssa.subrace_id
WHERE s.name = 'Swamp Kobold' AND ssa.name = 'Marsh Dweller'
AND NOT EXISTS (SELECT 1 FROM arena_data.subrace_feat_resistance fr WHERE fr.feat_id = ssa.id);


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
    ('Gladefolk', 'Taunt',          'Can force enemies to target them instead of allies.'),
    ('Gladefolk', 'Fear Immunity',  'Immune to being frightened.'),
    ('Half-Elf', 'Magic Resistance', 'Advantage on saving throws against magical effects.'),
    ('Half-Elf', 'Fey Ancestry',     'Advantage against being charmed; immune to magical sleep.')
) AS s(race_name, name, descr)
JOIN arena_data.race r ON r.name = s.race_name;


-- Seed: Feat Resistances
INSERT INTO arena_data.feat_resistance (feat_id, resistance_type, resistance_value)
SELECT rsa.id, 'Magic', 25
FROM arena_data.race_special_ability rsa
JOIN arena_data.race r ON r.id = rsa.race_id
WHERE r.name IN ('Elf', 'Dwarf', 'Kobold', 'Ogre') AND rsa.name = 'Magic Resistance'
AND NOT EXISTS (SELECT 1 FROM arena_data.feat_resistance fr WHERE fr.feat_id = rsa.id);

INSERT INTO arena_data.feat_resistance (feat_id, resistance_type, resistance_value)
SELECT rsa.id, 'Magic', 25
FROM arena_data.race_special_ability rsa
JOIN arena_data.race r ON r.id = rsa.race_id
WHERE r.name = 'Half-Elf' AND rsa.name = 'Magic Resistance'
AND NOT EXISTS (SELECT 1 FROM arena_data.feat_resistance fr WHERE fr.feat_id = rsa.id);


-- ============================================================
-- SEED: CLASSES
-- ============================================================

INSERT INTO arena_data.class (name, description, movement_bonus, hit_die_id, base_strike_rating,
    attack_count, bow_attack_count, armor_restriction, can_dual_wield, weapon_switch_cost,
    two_handed_bonus, shield_bonus_damage, ranged_attack_bonus)
	SELECT src.name, src.description, src.movement, d.id, src.strike_rating,
        src.attacks, src.bow_attacks, src.armor, src.dual_wield, src.switch_cost,
        src.th_bonus, src.shield_bonus, src.ranged_bonus
	FROM (VALUES
    ('Barbarian', 'Fierce warriors who channel rage into devastating attacks.',       5, 'D12', 12, 3, 0, 'Light', FALSE, 0.0, 2, 0, 0),
    ('Knight',    'Armored cavaliers and champions of noble causes.',                 0, 'D10', 11, 2, 0, NULL,    FALSE, 0.5, 0, 2, 0),
    ('Paladin',   'Holy warriors blessed by the gods with divine power.',             0, 'D10',  9, 2, 0, NULL,    FALSE, 0.5, 2, 0, 0),
    ('Priest',    'Devoted servants who channel divine magic to heal and protect.',    5, 'D8',   6, 1, 0, NULL,    FALSE, 1.0, 0, 0, 0),
    ('Mage',      'Masters of the arcane who wield devastating spells.',              0, 'D4',   4, 1, 0, NULL,    FALSE, 1.0, 0, 0, 0),
    ('Bard',      'Musicians and storytellers who weave magic through performance.',   5, 'D6',   6, 1, 0, NULL,    FALSE, 1.0, 0, 0, 0),
    ('Druid',     'Guardians of nature who command the elements and beasts.',         5, 'D8',   7, 1, 0, NULL,    FALSE, 1.0, 0, 0, 0),
    ('Fighter',   'Weapons masters trained in all forms of combat.',                   0, 'D10', 12, 2, 0, NULL,    TRUE,  0.5, 0, 0, 0),
    ('Rogue',     'Cunning infiltrators who strike from the shadows.',               10, 'D6',   8, 1, 0, NULL,    TRUE,  1.0, 0, 0, 0),
    ('Ranger',    'Skilled trackers and woodsmen who tame the wilds.',                5, 'D10', 10, 2, 3, NULL,    TRUE,  0.0, 0, 0, 1)
) AS src(name, description, movement, die_name, strike_rating,
    attacks, bow_attacks, armor, dual_wield, switch_cost,
    th_bonus, shield_bonus, ranged_bonus)
JOIN arena_data.die_type d ON d.name = src.die_name;


-- Class-race restrictions
INSERT INTO arena_data.class_race (class_id, race_id)
SELECT c.id, r.id
FROM (VALUES
    ('Barbarian', 'Human'), ('Barbarian', 'Orc'), ('Barbarian', 'Ogre'), ('Barbarian', 'Dwarf'),
    ('Knight',    'Human'), ('Knight',    'Elf'), ('Knight',    'Dwarf'), ('Knight',    'Orc'),
    ('Paladin',   'Human'), ('Paladin',   'Elf'), ('Paladin',   'Dwarf'),
    ('Priest',    'Human'), ('Priest',    'Elf'), ('Priest',    'Dwarf'), ('Priest',    'Lizard'),
    ('Priest',    'Kobold'), ('Priest',   'Gladefolk'), ('Priest',  'Orc'),
    ('Mage',      'Human'), ('Mage',      'Elf'), ('Mage',      'Kobold'),
    ('Bard',      'Human'), ('Bard',      'Elf'), ('Bard',      'Gladefolk'),
    ('Druid',     'Human'), ('Druid',     'Elf'), ('Druid',     'Gladefolk'), ('Druid',    'Lizard'),
    ('Fighter',   'Human'), ('Fighter',   'Elf'), ('Fighter',   'Dwarf'), ('Fighter',   'Lizard'),
    ('Fighter',   'Kobold'), ('Fighter',  'Orc'), ('Fighter',   'Ogre'), ('Fighter',   'Gladefolk'),
    ('Rogue',     'Human'), ('Rogue',     'Elf'), ('Rogue',     'Dwarf'), ('Rogue',     'Gladefolk'), ('Rogue', 'Kobold'),
    ('Ranger',    'Human'), ('Ranger',    'Elf'), ('Ranger',    'Dwarf'), ('Ranger',    'Gladefolk'),
    -- Half-Elf gets all classes (versatile like Humans)
    ('Barbarian', 'Half-Elf'), ('Knight',  'Half-Elf'), ('Paladin',  'Half-Elf'), ('Priest',   'Half-Elf'),
    ('Mage',      'Half-Elf'), ('Bard',    'Half-Elf'), ('Druid',    'Half-Elf'), ('Fighter',  'Half-Elf'),
    ('Rogue',     'Half-Elf'), ('Ranger',  'Half-Elf')
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
    ('Spear',      'A versatile polearm for thrusting or throwing.'),
    ('TwoHandedSword',     'A massive blade requiring both hands and immense strength.'),
    ('TwoHandedBattleAxe', 'A devastating two-handed axe that cleaves through armor.'),
    ('TwoHandedWarhammer', 'A colossal hammer wielded in both hands, crushing all before it.'),
    ('TwoHandedMace',      'A massive two-handed mace that crushes armor and bone with equal ease.')
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
    ('Bard','Sling'),      ('Druid','Sling'),  ('Fighter','Sling'), ('Rogue','Sling'),
    -- Two-handed swords: warrior classes only
    ('Barbarian','TwoHandedSword'), ('Knight','TwoHandedSword'), ('Paladin','TwoHandedSword'), ('Fighter','TwoHandedSword'),
    -- Two-handed battle-axes: warrior classes only
    ('Barbarian','TwoHandedBattleAxe'), ('Knight','TwoHandedBattleAxe'), ('Paladin','TwoHandedBattleAxe'), ('Fighter','TwoHandedBattleAxe'),
    -- Two-handed warhammers: warriors + Priest
    ('Barbarian','TwoHandedWarhammer'), ('Knight','TwoHandedWarhammer'), ('Paladin','TwoHandedWarhammer'), ('Priest','TwoHandedWarhammer'), ('Fighter','TwoHandedWarhammer'),
    -- Ranger: bows, crossbows, short swords, daggers, spears
    ('Ranger','Bow'), ('Ranger','Crossbow'), ('Ranger','ShortSword'), ('Ranger','Dagger'), ('Ranger','Spear')
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
-- Set minimum strength for two-handed weapons (STR 16 required)
UPDATE arena_data.weapon SET minimum_strength = 16 WHERE name IN (
    'Great Sword', 'Battle Axe', 'Maul',
    'Great Sword of the North', 'Barbarian''s Cleaver', 'Judgment Hammer',
    'Winter Oath', 'Dragon Tooth', 'Templar''s Verdict',
    'Soul Reaver', 'Dragon''s Fury', 'Stormbringer'
);

UPDATE arena_data.weapon SET attack_bonus = 3 WHERE name = 'Soul Reaver';

UPDATE arena_data.weapon SET attack_bonus = 2 WHERE name IN ('Stormbringer', 'Dragon''s Fury', 'Shadow Sting', 'Frostbite', 'Sun''s Wrath', 'Winter Oath', 'Dragon Tooth', 'Templar''s Verdict');

UPDATE arena_data.weapon SET attack_bonus = 1 WHERE name IN ('Bone Crusher', 'Wind Cutter', 'Viper Fang', 'Longbow of the Wilds');

-- Migrate two-handed weapons to new archetype types
UPDATE arena_data.weapon SET weapon_type_id = (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedSword')
WHERE name IN ('Great Sword', 'Great Sword of the North', 'Winter Oath', 'Soul Reaver')
AND weapon_type_id != (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedSword');

UPDATE arena_data.weapon SET weapon_type_id = (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedBattleAxe')
WHERE name IN ('Battle Axe', 'Barbarian''s Cleaver', 'Dragon Tooth', 'Dragon''s Fury')
AND weapon_type_id != (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedBattleAxe');

UPDATE arena_data.weapon SET weapon_type_id = (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedWarhammer')
WHERE name IN ('Maul', 'Judgment Hammer', 'Templar''s Verdict')
AND weapon_type_id != (SELECT id FROM arena_data.weapon_type WHERE name = 'TwoHandedWarhammer');


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
                                                                                        'Axe',   'D6', 'Slashing', 'Melee', 1, 1, 'Uncommon', 0),
    -- Two-handed weapons
    ('Great Sword of the North', 'A massive blade forged in the permafrost of the Frozen Wastes. Its edge is honed to split both shield and shield-bearer.',
                                                                                        'TwoHandedSword', 'D10', 'Slashing', 'Melee', 1, 2, 'Common', 0),
    ('Barbarian''s Cleaver',     'A crude but terrifying two-handed axe that has tasted blood in a hundred tribal skirmishes. The haft is wrapped in the hide of the first beast its owner slew.',
                                                                                        'TwoHandedBattleAxe', 'D10', 'Slashing', 'Melee', 1, 2, 'Common', 0),
    ('Judgment Hammer',          'A towering warhammer of black iron, etched with holy scripture. Its head is shaped like a fist, and it falls with the weight of divine judgment.',
                                                                                        'TwoHandedWarhammer', 'D10', 'Bludgeoning', 'Melee', 1, 2, 'Common', 0),
    ('Great Mace',                'A massive two-handed mace crafted from solid steel. Its flanged head can crush plate armor like tin, and its weight demands strength few can muster.',
                                                                                        'TwoHandedMace', 'D10', 'Bludgeoning', 'Melee', 1, 2, 'Common', 1),
    ('Winter Oath',              'A legendary greatsword of ice-blue steel that never dulls. Bound to the oath of the knight who swore to defend the realm from the frozen north.',
                                                                                        'TwoHandedSword', 'D12', 'Ice', 'Melee', 1, 2, 'Legendary', 2),
    ('Dragon Tooth',             'A colossal axe carved from the fang of a primordial dragon. It hums with draconic fury and sets the air ablaze with every swing.',
                                                                                        'TwoHandedBattleAxe', 'D12', 'Fire', 'Melee', 1, 2, 'Legendary', 2),
    ('Templar''s Verdict',       'A sacred warhammer that once belonged to the High Templar of the Silver City. It glows with holy light when undead draw near.',
                                                                                        'TwoHandedWarhammer', 'D12', 'Holy', 'Melee', 1, 2, 'Legendary', 2),
    -- Ranger weapons
    ('Ranger''s Short Bow',      'A compact yew bow reinforced with horn and sinew. Crafted for quick shots from forest cover.',
                                                                                        'Bow', 'D6', 'Piercing', 'Ranged', 1, 2, 'Common', 0),
    ('Longbow of the Wilds',     'A massive longbow strung with the sinew of a forest giant. Only the strongest rangers can draw it to full extension.',
                                                                                        'Bow', 'D8', 'Piercing', 'Ranged', 1, 2, 'Rare', 1),
    ('Twin Fangs',               'Matched short swords balanced for dual-wielding. The pair together are lighter than a single long sword.',
                                                                                        'ShortSword', 'D6', 'Piercing', 'Melee', 1, 1, 'Uncommon', 0)
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

UPDATE arena_data.armor SET turn_meter_penalty = 1 WHERE name IN ('Scale Mail', 'Half Plate', 'Ring Mail');

UPDATE arena_data.armor SET turn_meter_penalty = 2 WHERE name IN ('Chain Mail', 'Splint Armor');

UPDATE arena_data.armor SET turn_meter_penalty = 3 WHERE name IN ('Plate Armor');

UPDATE arena_data.armor SET turn_meter_penalty = 1 WHERE name IN ('Shield');

-- Quality armor overrides
UPDATE arena_data.armor SET turn_meter_penalty = 2 WHERE name IN ('Knight''s Honor', 'Battlesworn Plate');

UPDATE arena_data.armor SET turn_meter_penalty = 4 WHERE name IN ('Titan Plate', 'Aegis of the Fallen King');

UPDATE arena_data.armor SET turn_meter_penalty = 1 WHERE name IN ('Dragon Scale Mail');


-- Set turn_meter_cost_reduction (robe-type armor for spellcasters)
UPDATE arena_data.armor SET turn_meter_cost_reduction = 5 WHERE name IN ('Leather Armor');


-- Set movement_penalty for armor (heavier armor slows movement speed)
UPDATE arena_data.armor SET movement_penalty = 0 WHERE armor_category_id = (SELECT id FROM arena_data.armor_category WHERE name = 'Light');

UPDATE arena_data.armor SET movement_penalty = 5 WHERE armor_category_id = (SELECT id FROM arena_data.armor_category WHERE name = 'Medium');

UPDATE arena_data.armor SET movement_penalty = 10 WHERE armor_category_id = (SELECT id FROM arena_data.armor_category WHERE name = 'Heavy');

UPDATE arena_data.armor SET movement_penalty = 0 WHERE armor_category_id = (SELECT id FROM arena_data.armor_category WHERE name = 'Shield');


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
JOIN arena_data.gear_quality gq ON gq.name = src.quality_name;


-- (duplicate character seed block removed -- all 9 characters are already
-- seeded in the HEROES & ENEMIES section above ~lines 777-1029)


-- NPC records in the npc table (combat combatants — also tracked as world NPCs)

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Kaela Vornskald', 10, 19, 15, 17, 9, 11, 13, FALSE, FALSE, FALSE,
       'A wandering barbarian woman. She wields a great sword and fears nothing.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Barbarian';

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Ser Garrick Dawnshield', 12, 18, 11, 15, 11, 13, 17, FALSE, FALSE, FALSE,
       'A paladin of the Silver Basilica who survived the Fall of Black Hollow. Seeks infernal corruption to purge.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Paladin';

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Vaelith Moonveil', 9, 14, 18, 10, 19, 10, 13, FALSE, FALSE, FALSE,
       'An arcane duelist who combines blade mastery with devastating magical precision. Seeks forgotten magical artifacts.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Fighter';

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Sister Elira Vane', 7, 11, 13, 15, 13, 18, 15, FALSE, FALSE, FALSE,
       'A healer and exorcist of the Moon Temple. Tended plague victims during the Ash Fever outbreak.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Human' AND c.name = 'Priest';

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Lord Aethor Valeborn', 11, 16, 16, 14, 14, 10, 15, FALSE, FALSE, FALSE,
       'An exiled elven knight who wields the greatsword Winter Oath. Fights with measured precision.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Elf' AND c.name = 'Knight';

INSERT INTO arena_data.npc (race_id, class_id, name, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT r.id, c.id, 'Finnick Bramblefoot', 8, 8, 20, 13, 14, 11, 16, FALSE, FALSE, FALSE,
       'A notorious Gladefolk thief. Charming, sarcastic, and impossible to pin down.'
FROM arena_data.race r, arena_data.class c
WHERE r.name = 'Gladefolk' AND c.name = 'Rogue';


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
-- SEED: COMBAT CHARACTERS (for API roster — demo heroes & enemies)
-- Npc=0 → hero-side, Npc=1 → enemy-side
-- ============================================================

-- Heroes (Npc=0)

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Kaela Vornskald', 10, 'F',
        19, 15, 17, 9, 11, 13,
        100, 100, 15, 6, 0, 0,
        'A wandering barbarian woman who has felled beasts and bandits across the realm.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Human' AND c.name = 'Barbarian'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Great Sword';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Hide Armor';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Ser Garrick Dawnshield', 12, 'M',
        18, 11, 15, 11, 13, 17,
        96, 96, 13, 8, 0, 60,
        'A towering paladin clad in polished silver plate bearing the sigil of Heaven. He wields the warhammer Judicator.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Human' AND c.name = 'Paladin'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'War Hammer';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Plate Armor';

    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Smite';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Remove Fear';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Resist Fire';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Resist Cold';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Magical Vestment';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Protection from Evil 10ft';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Heal';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Holy Bulwark';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Heroes Feast';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Vaelith Moonveil', 9, 'F',
        14, 18, 10, 19, 10, 13,
        45, 45, 15, 8, 0, 90,
        'An arcane duelist who blends blade mastery with devastating magic. Seeks lost magical artifacts.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Elf' AND c.name = 'Fighter'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Long Sword';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Mithril Chain';

    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Fireball';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Ice Bolt';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Shock';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Magic Missile';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Shield';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Mirror Image';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Blink';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Lightning Bolt';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Invisibility';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Sister Elira Vane', 7, 'F',
        11, 13, 15, 13, 18, 15,
        49, 49, 17, 8, 0, 70,
        'A soft-spoken cleric of the Moon Temple. Healer and exorcist who tends to the sick and battles shadow spirits.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Human' AND c.name = 'Priest'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Mace';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Padded Armor';

    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Heal';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Mass Heal';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Bless';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Cure Light Wounds';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Cure Serious Wounds';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Command';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Chasten';
    INSERT INTO arena_data.character_spell (character_id, spell_id)
    SELECT v_id, s.id FROM arena_data.spell s WHERE s.name = 'Prayer';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Lord Aethor Valeborn', 11, 'M',
        16, 16, 14, 14, 10, 15,
        88, 88, 13, 7, 0, 0,
        'An exiled elven knight who wields the greatsword Winter Oath. He fights with measured precision.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Elf' AND c.name = 'Knight'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Great Sword';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Plate Armor';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Finnick Bramblefoot', 8, 'M',
        8, 20, 13, 14, 11, 16,
        40, 40, 17, 12, 0, 0,
        'A notorious Gladefolk thief known throughout taverns and criminal circles. Charmingly rogue.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Gladefolk' AND c.name = 'Rogue'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Dagger';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Studded Leather';
END;
$$;


-- Ranger Hero
DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Lyra Swiftarrow', 8, 'F',
        12, 18, 14, 11, 15, 13,
        60, 60, 16, 10, 0, 0,
        'An elven ranger of the Greenwood who has never missed a shot she intended to take. Her twin short swords are as quick as her bow.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Elf' AND c.name = 'Ranger'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Ranger''s Short Bow';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Studded Leather';
END;
$$;

-- Update existing characters for the new weapon system
-- Kaela Vornskald: Barbarian stays with Great Sword (two-handed) and Hide Armor (light only)
-- Lord Aethor Valeborn: Knight gets a shield for shield bonus
DO $$
DECLARE
    v_shield_id INTEGER;
    v_knight_id INTEGER;
BEGIN
    SELECT id INTO v_shield_id FROM arena_data.armor WHERE name = 'Shield';
    SELECT id INTO v_knight_id FROM arena_data.character WHERE name = 'Lord Aethor Valeborn';

    IF v_knight_id IS NOT NULL AND v_shield_id IS NOT NULL THEN
        INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
        SELECT v_knight_id, es.id, 'armor', v_shield_id
        FROM arena_data.equipment_slot es
        WHERE es.name = 'LeftHand'
        AND NOT EXISTS (
            SELECT 1 FROM arena_data.character_equipment ce
            WHERE ce.character_id = v_knight_id AND ce.slot_id = es.id
        );
    END IF;
END;
$$;

-- Update Ser Garrick Dawnshield to use Judgment Hammer (two-handed warhammer)
DO $$
DECLARE
    v_paladin_id INTEGER;
    v_hammer_id INTEGER;
BEGIN
    SELECT id INTO v_paladin_id FROM arena_data.character WHERE name = 'Ser Garrick Dawnshield';
    SELECT id INTO v_hammer_id FROM arena_data.weapon WHERE name = 'Judgment Hammer';

    IF v_paladin_id IS NOT NULL AND v_hammer_id IS NOT NULL THEN
        UPDATE arena_data.character_equipment
        SET item_id = v_hammer_id
        WHERE character_id = v_paladin_id AND slot_id = (SELECT id FROM arena_data.equipment_slot WHERE name = 'RightHand');
    END IF;
END;
$$;

-- Enemies (Npc=1)

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Korg Stonefist', 15, 'M',
        21, 10, 18, 8, 8, 8,
        165, 165, 14, 6, 1, 0,
        'A wandering orc berserker who challenges all who cross his path. Wields a massive maul.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Orc' AND c.name = 'Barbarian'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Maul';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Chain Mail';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Graveworm', 9, 'M',
        17, 10, 14, 11, 8, 6,
        72, 72, 14, 7, 1, 0,
        'An undead warlord who commands a legion of skeletons in the Bone Fields.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Undead' AND c.name = 'Fighter'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Short Sword';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Chain Mail';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Shadowmere', 10, 'F',
        10, 19, 10, 16, 10, 15,
        40, 40, 16, 12, 1, 0,
        'A striking female elf rogue and the leader of the Nightblades guild. Deadly with twin daggers.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Elf' AND c.name = 'Rogue'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Dagger';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Studded Leather';
END;
$$;


-- Ranger Enemy
DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Sylas Thornwood', 10, 'M',
        14, 20, 14, 12, 16, 12,
        75, 75, 15, 10, 1, 0,
        'An elven ranger who patrols the Deepwood. His arrows never miss their mark.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Elf' AND c.name = 'Ranger'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Longbow of the Wilds';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Studded Leather';
END;
$$;

-- Test Dummies (Npc=0, hero-side)
DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Target Golem', 10, 'N',
        16, 10, 18, 14, 10, 8,
        300, 300, 14, 6, 0, 100,
        'A hulking construct of stone and metal used for combat training. Withstands tremendous punishment.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Human' AND c.name = 'Fighter'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'weapon', w.id
    FROM arena_data.equipment_slot es, arena_data.weapon w
    WHERE es.name = 'RightHand' AND w.name = 'Long Sword';

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Plate Armor';
END;
$$;

DO $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO arena_data.character (race_id, class_id, name, level, sex,
        strength, dexterity, stamina, intelligence, wisdom, charisma,
        max_hit_points, current_hit_points, strike_rating, turn_speed, npc, max_mana, biography)
    SELECT r.id, c.id, 'Practice Dummy', 10, 'N',
        10, 10, 10, 10, 10, 10,
        500, 500, 1, 1, 0, 0,
        'A simple training target made of straw and cloth. Does not fight back.'
    FROM arena_data.race r, arena_data.class c
    WHERE r.name = 'Human' AND c.name = 'Fighter'
    RETURNING id INTO v_id;

    INSERT INTO arena_data.character_equipment (character_id, slot_id, item_type, item_id)
    SELECT v_id, es.id, 'armor', a.id
    FROM arena_data.equipment_slot es, arena_data.armor a
    WHERE es.name = 'Chest' AND a.name = 'Studded Leather';
END;
$$;

-- Missing NPC records in the npc table (quest givers / merchants)

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Kaela Vornskald', r.id, c.id, 10, 19, 15, 17, 9, 11, 13, FALSE, FALSE, FALSE,
       'A wandering barbarian woman. She wields a great sword and fears nothing.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Human' AND c.name = 'Barbarian'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Kaela Vornskald');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Ser Garrick Dawnshield', r.id, c.id, 12, 18, 11, 15, 11, 13, 17, FALSE, FALSE, FALSE,
       'A paladin of the Silver Basilica who survived the Fall of Black Hollow. Seeks infernal corruption to purge.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Human' AND c.name = 'Paladin'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Ser Garrick Dawnshield');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Vaelith Moonveil', r.id, c.id, 9, 14, 18, 10, 19, 10, 13, FALSE, FALSE, FALSE,
       'An arcane duelist who combines blade mastery with devastating magical precision. Seeks forgotten magical artifacts.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Elf' AND c.name = 'Fighter'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Vaelith Moonveil');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Sister Elira Vane', r.id, c.id, 7, 11, 13, 15, 13, 18, 15, FALSE, FALSE, FALSE,
       'A healer and exorcist of the Moon Temple. Tended plague victims during the Ash Fever outbreak.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Human' AND c.name = 'Priest'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Sister Elira Vane');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Lord Aethor Valeborn', r.id, c.id, 11, 16, 16, 14, 14, 10, 15, FALSE, FALSE, FALSE,
       'An exiled elven knight who wields the greatsword Winter Oath. Fights with measured precision.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Elf' AND c.name = 'Knight'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Lord Aethor Valeborn');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Finnick Bramblefoot', r.id, c.id, 8, 8, 20, 13, 14, 11, 16, FALSE, FALSE, FALSE,
       'A notorious Gladefolk thief. Charming, sarcastic, and impossible to pin down.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Gladefolk' AND c.name = 'Rogue'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Finnick Bramblefoot');

INSERT INTO arena_data.npc (name, race_id, class_id, level, strength, dexterity, stamina, intelligence, wisdom, charisma, is_merchant, is_quest_giver, is_hostile, biography)
SELECT 'Lyra Swiftarrow', r.id, c.id, 8, 12, 18, 14, 11, 15, 13, FALSE, FALSE, FALSE,
       'An elven ranger of the Greenwood. Master archer and twin-blade duelist.'
FROM arena_data.race r, arena_data.class c WHERE r.name = 'Elf' AND c.name = 'Ranger'
AND NOT EXISTS (SELECT 1 FROM arena_data.npc n WHERE n.name = 'Lyra Swiftarrow');

