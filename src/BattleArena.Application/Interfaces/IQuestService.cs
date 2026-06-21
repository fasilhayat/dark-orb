namespace BattleArena.Application.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

public interface IQuestService
{
    Task<Quest?> GetQuestAsync(int id);
    Task<List<Quest>> GetAllQuestsAsync(int? level = null);
    Task<int> CreateQuestAsync(Quest quest);
    Task UpdateQuestAsync(Quest quest);
    Task DeleteQuestAsync(int id);
    Task<List<CharacterQuest>> GetCharacterQuestsAsync(int characterId, QuestStatus? status = null);
    Task AcceptQuestAsync(int characterId, int questId);
    Task UpdateProgressAsync(int characterId, int questId, string progressJson);
    Task<(bool Completed, string? Message)> TryCompleteQuestAsync(int characterId, int questId);
}
