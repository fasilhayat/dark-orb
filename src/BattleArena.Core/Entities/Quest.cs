namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Quest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType QuestType { get; set; }
    public int LevelRequirement { get; set; } = 1;
    public int RewardXp { get; set; }
    public int[] PrereqQuestIds { get; set; } = [];
    public string? LocationId { get; set; }
}
