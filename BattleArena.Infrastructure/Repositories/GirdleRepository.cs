using BattleArena.Core.Entities;
using BattleArena.Core.Interfaces;
using BattleArena.Infrastructure.Data;
using Npgsql;

namespace BattleArena.Infrastructure.Repositories;

public class GirdleRepository : IGirdleRepository
{
    private readonly IDbContext _context;

    public GirdleRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Girdle>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_girdles", MapGirdle);
    }

    private static Girdle MapGirdle(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        EffectType = (string)reader["effect_type"],
        EffectValue = (int)reader["effect_value"],
        Cursed = (bool)reader["cursed"],
        CurseEffect = reader["curse_effect"] as string ?? string.Empty
    };
}
