namespace BattleArena.Core.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

public interface IQuestRepository
{
    Task<Quest?> GetByIdAsync(int id);
    Task<List<Quest>> GetAllAsync(int? level = null);
    Task<int> CreateAsync(Quest quest);
    Task UpdateAsync(Quest quest);
    Task DeleteAsync(int id);

    Task<CharacterQuest?> GetCharacterQuestAsync(int characterId, int questId);
    Task<List<CharacterQuest>> GetCharacterQuestsAsync(int characterId, QuestStatus? status = null);
    Task AcceptQuestAsync(int characterId, int questId);
    Task UpdateQuestProgressAsync(int characterId, int questId, string progressJson);
    Task CompleteQuestAsync(int characterId, int questId);
}
