namespace BattleArena.Core.Entities;

public class Feat
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? RaceId { get; set; }
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public List<ResistanceBonus> Resistances { get; set; } = new();
}
