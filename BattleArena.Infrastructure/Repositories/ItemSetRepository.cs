using BattleArena.Core.Entities;
using BattleArena.Core.Interfaces;
using BattleArena.Infrastructure.Data;
using Npgsql;

namespace BattleArena.Infrastructure.Repositories;

public class ItemSetRepository : IItemSetRepository
{
    private readonly IDbContext _context;

    public ItemSetRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemSet>> GetAllSetsAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_item_sets", MapSet);
    }

    public async Task<List<SetBonus>> GetSetBonusesAsync(int setId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_set_bonuses(p_set_id := @p_set_id)",
            MapBonus,
            new NpgsqlParameter("p_set_id", setId));
    }

    private static ItemSet MapSet(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty
    };

    private static SetBonus MapBonus(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        SetId = (int)reader["set_id"],
        PiecesRequired = (int)reader["pieces_required"],
        EffectDescription = (string)reader["effect_description"]
    };
}
