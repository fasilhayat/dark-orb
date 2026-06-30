namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

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

    public async Task<List<Race>> GetPlayableAsync()
    {
        return (await _context.ExecuteQueryAsync("fn_get_races", MapRace))
            .Where(r => r.IsPlayable)
            .ToList();
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
        var feats = await _context.ExecuteQueryAsync(
            "fn_get_feats(p_race_id := @p_race_id)",
            MapFeat,
            new NpgsqlParameter("p_race_id", raceId));

        // Load resistances for each feat
        foreach (var feat in feats)
            feat.Resistances = await GetFeatResistancesAsync(feat.Id);

        return feats;
    }

    public async Task<List<ResistanceBonus>> GetFeatResistancesAsync(int featId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_feat_resistances(p_feat_id := @p_feat_id)",
            MapResistance,
            new NpgsqlParameter("p_feat_id", featId));
    }

    public async Task<List<Subrace>> GetSubracesByRaceIdAsync(int raceId)
    {
        var subraces = await _context.ExecuteQueryAsync(
            "fn_get_subraces(p_race_id := @p_race_id)",
            MapSubrace,
            new NpgsqlParameter("p_race_id", raceId));

        foreach (var subrace in subraces)
            subrace.Feats = await GetSubraceAbilitiesAsync(subrace.Id);

        return subraces;
    }

    public async Task<List<Subrace>> GetAllSubracesAsync()
    {
        var subraces = await _context.ExecuteQueryAsync("fn_get_subraces", MapSubrace);

        foreach (var subrace in subraces)
            subrace.Feats = await GetSubraceAbilitiesAsync(subrace.Id);

        return subraces;
    }

    public async Task<Subrace?> GetSubraceByIdAsync(int subraceId)
    {
        var subraces = await _context.ExecuteQueryAsync("fn_get_subraces", MapSubrace);
        var subrace = subraces.FirstOrDefault(s => s.Id == subraceId);
        if (subrace is not null)
            subrace.Feats = await GetSubraceAbilitiesAsync(subrace.Id);
        return subrace;
    }

    public async Task<List<Feat>> GetSubraceAbilitiesAsync(int subraceId)
    {
        var feats = await _context.ExecuteQueryAsync(
            "fn_get_subrace_abilities(p_subrace_id := @p_subrace_id)",
            MapSubraceFeat,
            new NpgsqlParameter("p_subrace_id", subraceId));

        foreach (var feat in feats)
            feat.Resistances = await GetSubraceFeatResistancesAsync(feat.Id);

        return feats;
    }

    public async Task<List<ResistanceBonus>> GetSubraceFeatResistancesAsync(int featId)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_subrace_feat_resistances(p_feat_id := @p_feat_id)",
            MapResistance,
            new NpgsqlParameter("p_feat_id", featId));
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

        if (reader["strength_min"] != DBNull.Value) race.StrengthMin = (int)reader["strength_min"];
        if (reader["dexterity_min"] != DBNull.Value) race.DexterityMin = (int)reader["dexterity_min"];
        if (reader["stamina_min"] != DBNull.Value) race.StaminaMin = (int)reader["stamina_min"];
        if (reader["intelligence_min"] != DBNull.Value) race.IntelligenceMin = (int)reader["intelligence_min"];
        if (reader["wisdom_min"] != DBNull.Value) race.WisdomMin = (int)reader["wisdom_min"];
        if (reader["charisma_min"] != DBNull.Value) race.CharismaMin = (int)reader["charisma_min"];
        if (reader["strength_max"] != DBNull.Value) race.StrengthMax = (int)reader["strength_max"];
        if (reader["dexterity_max"] != DBNull.Value) race.DexterityMax = (int)reader["dexterity_max"];
        if (reader["stamina_max"] != DBNull.Value) race.StaminaMax = (int)reader["stamina_max"];
        if (reader["intelligence_max"] != DBNull.Value) race.IntelligenceMax = (int)reader["intelligence_max"];
        if (reader["wisdom_max"] != DBNull.Value) race.WisdomMax = (int)reader["wisdom_max"];
        if (reader["charisma_max"] != DBNull.Value) race.CharismaMax = (int)reader["charisma_max"];

        if (reader["is_playable"] != DBNull.Value)
            race.IsPlayable = (bool)reader["is_playable"];

        return race;
    }

    private static Feat MapFeat(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        RaceId = reader["race_id"] as int?
    };

    private static ResistanceBonus MapResistance(NpgsqlDataReader reader)
    {
        var type = Enum.TryParse<ResistanceType>((string)reader["resistance_type"], true, out var parsed)
            ? parsed : ResistanceType.Magic;
        return new ResistanceBonus(type, (int)reader["resistance_value"]);
    }

    private static Subrace MapSubrace(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        RaceId = (int)reader["race_id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        StrengthBonus = reader["strength_bonus"] as int? ?? 0,
        DexterityBonus = reader["dexterity_bonus"] as int? ?? 0,
        StaminaBonus = reader["stamina_bonus"] as int? ?? 0,
        IntelligenceBonus = reader["intelligence_bonus"] as int? ?? 0,
        WisdomBonus = reader["wisdom_bonus"] as int? ?? 0,
        CharismaBonus = reader["charisma_bonus"] as int? ?? 0,
        HitPointBonus = reader["hit_point_bonus"] as int? ?? 0
    };

    private static Feat MapSubraceFeat(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        AttackBonus = reader["attack_bonus"] as int? ?? 0,
        DefenseBonus = reader["defense_bonus"] as int? ?? 0
    };
}
