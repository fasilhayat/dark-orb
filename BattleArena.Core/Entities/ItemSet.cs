namespace BattleArena.Core.Entities;

public class ItemSet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SetBonus
{
    public int Id { get; set; }
    public int SetId { get; set; }
    public int PiecesRequired { get; set; }
    public string EffectDescription { get; set; } = string.Empty;
}
