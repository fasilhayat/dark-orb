namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class ArmorRepository : IArmorRepository
{
    private readonly IDbContext _context;

    public ArmorRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Armor>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_armor", MapArmor);
    }

    public async Task<Armor?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_armor(p_id := @p_id)",
            MapArmor,
            new NpgsqlParameter("p_id", id));
        return results.FirstOrDefault();
    }

    private static Armor MapArmor(NpgsqlDataReader reader) => new()
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
}
