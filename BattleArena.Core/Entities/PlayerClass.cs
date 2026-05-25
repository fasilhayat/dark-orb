using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class PlayerClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DieType HitDie { get; set; }
    public int BaseStrikeRating { get; set; }
}
