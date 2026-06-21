namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class CharacterQuest
{
    public int CharacterId { get; set; }
    public int QuestId { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Active;
    public string ProgressJson { get; set; } = "{}";
    public DateTime? CompletedAt { get; set; }
}
