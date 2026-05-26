namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class NpcRepository : INpcRepository
{
    private readonly IDbContext _context;

    public NpcRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Npc>> GetAllAsync(bool? merchant = null, bool? hostile = null)
    {
        var parameters = new List<NpgsqlParameter>();
        if (merchant.HasValue)
            parameters.Add(new NpgsqlParameter("p_merchant", merchant.Value));
        if (hostile.HasValue)
            parameters.Add(new NpgsqlParameter("p_hostile", hostile.Value));

        return await _context.ExecuteQueryAsync("fn_get_npcs", MapNpc, parameters.ToArray());
    }

    private static Npc MapNpc(NpgsqlDataReader reader) => new()
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
        IsMerchant = (bool)reader["is_merchant"],
        IsQuestGiver = (bool)reader["is_quest_giver"],
        IsHostile = (bool)reader["is_hostile"],
        Biography = reader["biography"] as string ?? string.Empty
    };
}
