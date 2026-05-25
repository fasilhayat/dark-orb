namespace BattleArena.Core.Entities;

public class Npc
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Stamina { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    public bool IsMerchant { get; set; }
    public bool IsQuestGiver { get; set; }
    public bool IsHostile { get; set; }
    public string Biography { get; set; } = string.Empty;
}
