namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class BestiaryRepository : IBestiaryRepository
{
    private readonly IDbContext _context;

    public BestiaryRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<BestiaryEntry>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_bestiary", MapBestiary);
    }

    public async Task<List<BestiaryEntry>> GetByCategoryAndLevelAsync(string? category, int? level)
    {
        var parameters = new List<NpgsqlParameter>();
        if (!string.IsNullOrWhiteSpace(category))
            parameters.Add(new NpgsqlParameter("p_category", category));
        if (level.HasValue)
            parameters.Add(new NpgsqlParameter("p_level", level.Value));

        var sql = "fn_get_bestiary(";
        var clauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(category)) clauses.Add("p_category := @p_category");
        if (level.HasValue) clauses.Add("p_level := @p_level");
        sql += string.Join(", ", clauses);
        sql += ")";

        return await _context.ExecuteQueryAsync(sql, MapBestiary, [.. parameters]);
    }

    private static BestiaryEntry MapBestiary(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Category = (string)reader["category"],
        Name = (string)reader["name"],
        Level = (int)reader["level"],
        StrengthBonus = reader["strength_bonus"] as int? ?? 0,
        DexterityBonus = reader["dexterity_bonus"] as int? ?? 0,
        StaminaBonus = reader["stamina_bonus"] as int? ?? 0,
        IntelligenceBonus = reader["intelligence_bonus"] as int? ?? 0,
        WisdomBonus = reader["wisdom_bonus"] as int? ?? 0,
        CharismaBonus = reader["charisma_bonus"] as int? ?? 0,
        MaxHitPoints = (int)reader["max_hit_points"],
        ArmorClass = (int)reader["armor_class"],
        AttackDescription = reader["attack_description"] as string ?? string.Empty,
        SpecialAbilities = reader["special_abilities"] as string ?? string.Empty,
        Description = reader["description"] as string ?? string.Empty
    };
}
