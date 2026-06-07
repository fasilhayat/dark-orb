namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class CharacterRepository : ICharacterRepository
{
    private readonly IDbContext _context;

    public CharacterRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<Character?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_character(p_id := @p_id)",
            MapCharacter,
            new NpgsqlParameter("p_id", id));
        return results.FirstOrDefault();
    }

    public async Task<List<Character>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_characters", MapCharacter);
    }

    public async Task<int> CreateAsync(Character character)
    {
        var result = await _context.ExecuteScalarAsync<int>(
            "fn_create_character(@p_name, @p_race_id, @p_class_id, @p_strength, @p_dexterity, @p_stamina, @p_intelligence, @p_wisdom, @p_charisma, @p_strength_percentile, @p_max_hit_points, @p_npc, @p_biography, @p_experience_points, @p_max_mana, @p_subrace_id)",
            new NpgsqlParameter("p_name", character.Name),
            new NpgsqlParameter("p_race_id", character.RaceId),
            new NpgsqlParameter("p_class_id", character.ClassId),
            new NpgsqlParameter("p_strength", character.Strength),
            new NpgsqlParameter("p_dexterity", character.Dexterity),
            new NpgsqlParameter("p_stamina", character.Stamina),
            new NpgsqlParameter("p_intelligence", character.Intelligence),
            new NpgsqlParameter("p_wisdom", character.Wisdom),
            new NpgsqlParameter("p_charisma", character.Charisma),
            new NpgsqlParameter("p_strength_percentile", character.StrengthPercentile),
            new NpgsqlParameter("p_max_hit_points", character.MaxHitPoints),
            new NpgsqlParameter("p_npc", character.Npc),
            new NpgsqlParameter("p_biography", character.Biography),
            new NpgsqlParameter("p_experience_points", character.ExperiencePoints),
            new NpgsqlParameter("p_max_mana", character.MaxMana),
            new NpgsqlParameter("p_subrace_id", (object?)character.SubraceId ?? DBNull.Value));
        return result;
    }

    public async Task UpdateAsync(Character character)
    {
        await _context.ExecuteProcedureAsync(
            "sp_update_character(@p_id, @p_name, @p_level, @p_strength, @p_dexterity, @p_stamina, @p_intelligence, @p_wisdom, @p_charisma, @p_strength_percentile, @p_current_hit_points, @p_npc, @p_biography, @p_experience_points, @p_max_mana)",
            new NpgsqlParameter("p_id", character.Id),
            new NpgsqlParameter("p_name", character.Name),
            new NpgsqlParameter("p_level", character.Level),
            new NpgsqlParameter("p_strength", character.Strength),
            new NpgsqlParameter("p_dexterity", character.Dexterity),
            new NpgsqlParameter("p_stamina", character.Stamina),
            new NpgsqlParameter("p_intelligence", character.Intelligence),
            new NpgsqlParameter("p_wisdom", character.Wisdom),
            new NpgsqlParameter("p_charisma", character.Charisma),
            new NpgsqlParameter("p_strength_percentile", character.StrengthPercentile),
            new NpgsqlParameter("p_current_hit_points", character.CurrentHitPoints),
            new NpgsqlParameter("p_npc", character.Npc),
            new NpgsqlParameter("p_biography", character.Biography),
            new NpgsqlParameter("p_experience_points", character.ExperiencePoints),
            new NpgsqlParameter("p_max_mana", character.MaxMana));
    }

    public async Task DeleteAsync(int id)
    {
        await _context.ExecuteProcedureAsync(
            "sp_delete_character(@p_id)",
            new NpgsqlParameter("p_id", id));
    }

    public async Task<List<Weapon>> GetCharacterWeaponsAsync(int characterId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_character_weapons(p_character_id := @p_id)",
            MapCharacterWeapon,
            new NpgsqlParameter("p_id", characterId));
    }

    public async Task<List<(Armor Armor, string SlotName)>> GetCharacterArmorAsync(int characterId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_character_armor(p_character_id := @p_id)",
            MapCharacterArmor,
            new NpgsqlParameter("p_id", characterId));
    }

    public async Task<List<Spell>> GetCharacterSpellsAsync(int characterId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_character_spells(p_character_id := @p_id)",
            MapCharacterSpell,
            new NpgsqlParameter("p_id", characterId));
    }

    private static Character MapCharacter(NpgsqlDataReader reader)
    {
        var c = new Character
        {
            Id = (int)reader["id"],
            Name = (string)reader["name"],
            Level = (int)reader["level"],
            RaceId = (int)reader["race_id"],
            ClassId = (int)reader["class_id"],
            SubraceId = reader["subrace_id"] as int?,
            ClassName = reader["class_name"] as string ?? string.Empty,
            Sex = reader["sex"] as string ?? "Unknown",
            Strength = (int)reader["strength"],
            Dexterity = (int)reader["dexterity"],
            Stamina = (int)reader["stamina"],
            Intelligence = (int)reader["intelligence"],
            Wisdom = (int)reader["wisdom"],
            Charisma = (int)reader["charisma"],
            MaxHitPoints = (int)reader["max_hit_points"],
            CurrentHitPoints = (int)reader["current_hit_points"],
            StrikeRating = reader["strike_rating"] as int? ?? 20,
            TurnSpeed = reader["turn_speed"] as int? ?? 0,
            StrengthPercentile = reader["strength_percentile"] as int? ?? 0,
            Npc = reader["npc"] as short? ?? 0,
            Biography = reader["biography"] as string ?? string.Empty,
            ExperiencePoints = reader["experience_points"] as int? ?? 0,
            MaxMana = reader["max_mana"] as int? ?? 0,
            CurrentMana = reader["max_mana"] as int? ?? 0
        };
        c.CurrentHitPoints = c.MaxHitPoints;
        return c;
    }

    private static Weapon MapCharacterWeapon(NpgsqlDataReader reader)
    {
        _ = reader["slot_name"]; // consume slot column but map weapon from remaining columns
        return new Weapon
        {
            Id = (int)reader["id"],
            Name = (string)reader["name"],
            Description = reader["description"] as string ?? string.Empty,
            Archetype = Enum.Parse<ArchetypeWeapon>((string)reader["weapon_type"]),
            DamageDie = Enum.Parse<DieType>((string)reader["damage_die"]),
            DamageType = Enum.Parse<DamageType>((string)reader["damage_type"]),
            AttackType = Enum.Parse<AttackType>((string)reader["attack_type"]),
            DamageCount = reader["damage_count"] as int? ?? 1,
            Hands = reader["hands"] as int? ?? 1,
            Quality = Enum.Parse<GearQuality>((string)reader["quality"]),
            AttackBonus = reader["attack_bonus"] as int? ?? 0
        };
    }

    private static (Armor, string) MapCharacterArmor(NpgsqlDataReader reader)
    {
        var slotName = (string)reader["slot_name"];
        var armor = new Armor
        {
            Id = (int)reader["id"],
            Name = (string)reader["name"],
            Description = reader["description"] as string ?? string.Empty,
            ArmorClass = (int)reader["armor_class"],
            Category = reader["category"] as string ?? string.Empty,
            MaxDexterityBonus = reader["max_dexterity_bonus"] as int? ?? 0,
            StealthDisadvantage = reader["stealth_disadvantage"] as bool? ?? false,
            StrengthRequirement = reader["strength_requirement"] as int? ?? 0,
            Quality = Enum.Parse<GearQuality>((string)reader["quality"]),
            ArmorClassBonus = reader["armor_class_bonus"] as int? ?? 0,
            Mitigation = reader["mitigation"] as int? ?? 0,
            TurnMeterPenalty = reader["turn_meter_penalty"] as int? ?? 0,
            TurnMeterCostReduction = reader["turn_meter_cost_reduction"] as int? ?? 0
        };
        return (armor, slotName);
    }

    private static Spell MapCharacterSpell(NpgsqlDataReader reader)
    {
        var schoolStr = (string)reader["school"];
        var school = schoolStr switch
        {
            "Aegis" => SpellSchool.Aegis,
            "Stormcraft" => SpellSchool.Stormcraft,
            "Verdancy" => SpellSchool.Verdancy,
            "Umbramancy" => SpellSchool.Umbramancy,
            "Mirage" => SpellSchool.Mirage,
            "Dominion" => SpellSchool.Dominion,
            _ => SpellSchool.Deity
        };

        return new Spell
        {
            Id = (int)reader["id"],
            Name = (string)reader["name"],
            Description = reader["description"] as string ?? string.Empty,
            School = school,
            ManaCost = (int)reader["mana_cost"],
            TurnMeterCost = reader["turn_meter_cost"] as int? ?? 100,
            SpellLevel = reader["spell_level"] as int? ?? 1,
            DamageCount = reader["damage_count"] as int? ?? 1,
            AttackBonus = reader["attack_bonus"] as int? ?? 0,
            FlatDamageBonus = reader["flat_damage_bonus"] as int? ?? 0,
            DamageDie = reader["damage_die"] is string dieStr ? Enum.Parse<DieType>(dieStr) : DieType.D4,
            DamageType = reader["damage_type"] is string dtStr ? Enum.Parse<DamageType>(dtStr) : DamageType.Bludgeoning,
            AttackType = reader["attack_type"] is string atStr ? Enum.Parse<AttackType>(atStr) : AttackType.Spell,
            ElementalType = reader["elemental_type"] is string etStr && Enum.TryParse<ElementalType>(etStr, true, out var et) ? et : ElementalType.None,
            ElementalDamage = reader["elemental_damage"] as int? ?? 0
        };
    }
}
