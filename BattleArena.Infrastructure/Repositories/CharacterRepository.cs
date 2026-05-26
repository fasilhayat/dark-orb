namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
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
            "fn_create_character(@p_name, @p_race_id, @p_class_id, @p_strength, @p_dexterity, @p_stamina, @p_intelligence, @p_wisdom, @p_charisma, @p_strength_percentile, @p_max_hit_points)",
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
            new NpgsqlParameter("p_max_hit_points", character.MaxHitPoints));
        return result;
    }

    public async Task UpdateAsync(Character character)
    {
        await _context.ExecuteProcedureAsync(
            "sp_update_character(@p_id, @p_name, @p_level, @p_strength, @p_dexterity, @p_stamina, @p_intelligence, @p_wisdom, @p_charisma, @p_strength_percentile, @p_current_hit_points)",
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
            new NpgsqlParameter("p_current_hit_points", character.CurrentHitPoints));
    }

    public async Task DeleteAsync(int id)
    {
        await _context.ExecuteProcedureAsync(
            "sp_delete_character(@p_id)",
            new NpgsqlParameter("p_id", id));
    }

    private static Character MapCharacter(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Level = (int)reader["level"],
        RaceId = (int)reader["race_id"],
        ClassId = (int)reader["class_id"],
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
        StrengthPercentile = reader["strength_percentile"] as int? ?? 0
    };
}
