namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class ClassRepository : IClassRepository
{
    private readonly IDbContext _context;

    public ClassRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlayerClass>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_classes", MapClass);
    }

    public async Task<PlayerClass?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_classes()",
            MapClass);

        return results.FirstOrDefault(c => c.Id == id);
    }

    private static PlayerClass MapClass(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        MovementBonus = reader["movement_bonus"] as int? ?? 0,
        HitDie = ParseDieType(reader["hit_die"] as string),
        BaseStrikeRating = (int)reader["base_strike_rating"]
    };

    private static DieType ParseDieType(string? value) => value switch
    {
        "D4" => DieType.D4,
        "D6" => DieType.D6,
        "D8" => DieType.D8,
        "D10" => DieType.D10,
        "D12" => DieType.D12,
        "D20" => DieType.D20,
        "D100" => DieType.D100,
        _ => DieType.D6
    };
}
