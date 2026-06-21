namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class QuestRepository : IQuestRepository
{
    private readonly IDbContext _context;

    public QuestRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<Quest?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_quest(p_id := @p_id)",
            MapQuest,
            new NpgsqlParameter("p_id", id));
        return results.FirstOrDefault();
    }

    public async Task<List<Quest>> GetAllAsync(int? level = null)
    {
        if (level.HasValue)
            return await _context.ExecuteQueryAsync(
                "fn_get_quests_by_level(p_level := @p_level)",
                MapQuest,
                new NpgsqlParameter("p_level", level.Value));
        return await _context.ExecuteQueryAsync("fn_get_quests", MapQuest);
    }

    public async Task<int> CreateAsync(Quest quest)
    {
        return await _context.ExecuteScalarAsync<int>(
            "fn_create_quest(@p_name, @p_description, @p_quest_type, @p_level_requirement, @p_reward_xp, @p_prereq_quest_ids, @p_location_id)",
            new NpgsqlParameter("p_name", quest.Name),
            new NpgsqlParameter("p_description", quest.Description),
            new NpgsqlParameter("p_quest_type", quest.QuestType.ToString()),
            new NpgsqlParameter("p_level_requirement", quest.LevelRequirement),
            new NpgsqlParameter("p_reward_xp", quest.RewardXp),
            new NpgsqlParameter("p_prereq_quest_ids", quest.PrereqQuestIds),
            new NpgsqlParameter("p_location_id", (object?)quest.LocationId ?? DBNull.Value));
    }

    public async Task UpdateAsync(Quest quest)
    {
        await _context.ExecuteProcedureAsync(
            "sp_update_quest(@p_id, @p_name, @p_description, @p_quest_type, @p_level_requirement, @p_reward_xp, @p_prereq_quest_ids, @p_location_id)",
            new NpgsqlParameter("p_id", quest.Id),
            new NpgsqlParameter("p_name", quest.Name),
            new NpgsqlParameter("p_description", quest.Description),
            new NpgsqlParameter("p_quest_type", quest.QuestType.ToString()),
            new NpgsqlParameter("p_level_requirement", quest.LevelRequirement),
            new NpgsqlParameter("p_reward_xp", quest.RewardXp),
            new NpgsqlParameter("p_prereq_quest_ids", quest.PrereqQuestIds),
            new NpgsqlParameter("p_location_id", (object?)quest.LocationId ?? DBNull.Value));
    }

    public async Task DeleteAsync(int id)
    {
        await _context.ExecuteProcedureAsync(
            "sp_delete_quest(@p_id)",
            new NpgsqlParameter("p_id", id));
    }

    public async Task<CharacterQuest?> GetCharacterQuestAsync(int characterId, int questId)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_character_quest(p_character_id := @p_char, p_quest_id := @p_quest)",
            MapCharacterQuest,
            new NpgsqlParameter("p_char", characterId),
            new NpgsqlParameter("p_quest", questId));
        return results.FirstOrDefault();
    }

    public async Task<List<CharacterQuest>> GetCharacterQuestsAsync(int characterId, QuestStatus? status = null)
    {
        if (status.HasValue)
            return await _context.ExecuteQueryAsync(
                "fn_get_character_quests(p_character_id := @p_char, p_status := @p_status)",
                MapCharacterQuest,
                new NpgsqlParameter("p_char", characterId),
                new NpgsqlParameter("p_status", status.Value.ToString()));
        return await _context.ExecuteQueryAsync(
            "fn_get_character_quests(p_character_id := @p_char)",
            MapCharacterQuest,
            new NpgsqlParameter("p_char", characterId));
    }

    public async Task AcceptQuestAsync(int characterId, int questId)
    {
        await _context.ExecuteProcedureAsync(
            "sp_accept_quest(@p_character_id, @p_quest_id)",
            new NpgsqlParameter("p_character_id", characterId),
            new NpgsqlParameter("p_quest_id", questId));
    }

    public async Task UpdateQuestProgressAsync(int characterId, int questId, string progressJson)
    {
        await _context.ExecuteProcedureAsync(
            "sp_update_quest_progress(@p_character_id, @p_quest_id, @p_progress)",
            new NpgsqlParameter("p_character_id", characterId),
            new NpgsqlParameter("p_quest_id", questId),
            new NpgsqlParameter("p_progress", progressJson));
    }

    public async Task CompleteQuestAsync(int characterId, int questId)
    {
        await _context.ExecuteProcedureAsync(
            "sp_complete_quest(@p_character_id, @p_quest_id)",
            new NpgsqlParameter("p_character_id", characterId),
            new NpgsqlParameter("p_quest_id", questId));
    }

    private static Quest MapQuest(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = (string)reader["description"],
        QuestType = Enum.Parse<QuestType>((string)reader["quest_type"], ignoreCase: true),
        LevelRequirement = (int)reader["level_requirement"],
        RewardXp = (int)reader["reward_xp"],
        PrereqQuestIds = reader["prereq_quest_ids"] as int[] ?? [],
        LocationId = reader["location_id"] as string,
    };

    private static CharacterQuest MapCharacterQuest(NpgsqlDataReader reader) => new()
    {
        CharacterId = (int)reader["character_id"],
        QuestId = (int)reader["quest_id"],
        Status = Enum.Parse<QuestStatus>((string)reader["status"], ignoreCase: true),
        ProgressJson = reader["progress"] as string ?? "{}",
        CompletedAt = reader["completed_at"] as DateTime?,
    };
}
