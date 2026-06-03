namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class PetRepository : IPetRepository
{
    private readonly IDbContext _context;

    public PetRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pet>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_pets", MapPet);
    }

    public async Task<List<Pet>> GetByClassAndRaceAsync(int? classId, int? raceId)
    {
        var parameters = new List<NpgsqlParameter>();
        if (classId.HasValue)
            parameters.Add(new NpgsqlParameter("p_class_id", classId.Value));
        if (raceId.HasValue)
            parameters.Add(new NpgsqlParameter("p_race_id", raceId.Value));

        var sql = "fn_get_pets(";
        if (classId.HasValue) sql += "p_class_id := @p_class_id";
        if (classId.HasValue && raceId.HasValue) sql += ", ";
        if (raceId.HasValue) sql += "p_race_id := @p_race_id";
        sql += ")";

        return await _context.ExecuteQueryAsync(sql, MapPet, [.. parameters]);
    }

    private static Pet MapPet(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        DamageDie = ParseDieType(reader["damage_die"] as string),
        ArmorClass = reader["armor_class"] as int? ?? 10,
        MaxHitPoints = (int)reader["hit_points"]
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
        _ => DieType.D4
    };
}
