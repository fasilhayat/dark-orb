using BattleArena.Core.Entities;
using BattleArena.Core.Interfaces;
using BattleArena.Infrastructure.Data;
using Npgsql;

namespace BattleArena.Infrastructure.Repositories;

public class RaceRepository : IRaceRepository
{
    private readonly IDbContext _context;

    public RaceRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Race>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_races", MapRace);
    }

    public async Task<Race?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_races(p_id := @p_id)",
            MapRace,
            new NpgsqlParameter("p_id", id));

        if (results.Count == 0)
            return null;

        var race = results[0];
        race.Feats = await GetFeatsByRaceIdAsync(id);
        return race;
    }

    public async Task<List<Feat>> GetFeatsByRaceIdAsync(int raceId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_feats(p_race_id := @p_race_id)",
            MapFeat,
            new NpgsqlParameter("p_race_id", raceId));
    }

    private static Race MapRace(NpgsqlDataReader reader)
    {
        var race = new Race
        {
            Id = (int)reader["id"],
            Name = (string)reader["name"],
            Description = reader["description"] as string ?? string.Empty
        };

        if (reader["strength_bonus"] != DBNull.Value)
            race.AbilityBonuses["Strength"] = (int)reader["strength_bonus"];
        if (reader["dexterity_bonus"] != DBNull.Value)
            race.AbilityBonuses["Dexterity"] = (int)reader["dexterity_bonus"];
        if (reader["stamina_bonus"] != DBNull.Value)
            race.AbilityBonuses["Stamina"] = (int)reader["stamina_bonus"];
        if (reader["intelligence_bonus"] != DBNull.Value)
            race.AbilityBonuses["Intelligence"] = (int)reader["intelligence_bonus"];
        if (reader["wisdom_bonus"] != DBNull.Value)
            race.AbilityBonuses["Wisdom"] = (int)reader["wisdom_bonus"];
        if (reader["charisma_bonus"] != DBNull.Value)
            race.AbilityBonuses["Charisma"] = (int)reader["charisma_bonus"];

        return race;
    }

    private static Feat MapFeat(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        RaceId = reader["race_id"] as int?
    };
}
