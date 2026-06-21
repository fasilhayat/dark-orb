namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;

public class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;
    private readonly IReadOnlyList<IQuestValidator> _validators;

    public QuestService(IQuestRepository questRepository, IEnumerable<IQuestValidator> validators)
    {
        _questRepository = questRepository;
        _validators = validators.ToList();
    }

    public async Task<Quest?> GetQuestAsync(int id) =>
        await _questRepository.GetByIdAsync(id);

    public async Task<List<Quest>> GetAllQuestsAsync(int? level = null) =>
        await _questRepository.GetAllAsync(level);

    public async Task<int> CreateQuestAsync(Quest quest) =>
        await _questRepository.CreateAsync(quest);

    public async Task UpdateQuestAsync(Quest quest) =>
        await _questRepository.UpdateAsync(quest);

    public async Task DeleteQuestAsync(int id) =>
        await _questRepository.DeleteAsync(id);

    public async Task<List<CharacterQuest>> GetCharacterQuestsAsync(int characterId, QuestStatus? status = null) =>
        await _questRepository.GetCharacterQuestsAsync(characterId, status);

    public async Task AcceptQuestAsync(int characterId, int questId) =>
        await _questRepository.AcceptQuestAsync(characterId, questId);

    public async Task UpdateProgressAsync(int characterId, int questId, string progressJson) =>
        await _questRepository.UpdateQuestProgressAsync(characterId, questId, progressJson);

    public async Task<(bool Completed, string? Message)> TryCompleteQuestAsync(int characterId, int questId)
    {
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null)
            return (false, "Quest not found.");

        var characterQuest = await _questRepository.GetCharacterQuestAsync(characterId, questId);
        if (characterQuest is null)
            return (false, "Character has not accepted this quest.");
        if (characterQuest.Status != QuestStatus.Active)
            return (false, $"Quest is already {characterQuest.Status.ToString().ToLowerInvariant()}.");

        var validator = _validators.FirstOrDefault(v => v.Handles is null || v.Handles == quest.QuestType);
        if (validator is null)
            return (false, $"No validator registered for quest type {quest.QuestType}.");

        if (!validator.CanComplete(characterQuest, quest))
            return (false, "Completion conditions not yet met.");

        await _questRepository.CompleteQuestAsync(characterId, questId);
        return (true, null);
    }
}
