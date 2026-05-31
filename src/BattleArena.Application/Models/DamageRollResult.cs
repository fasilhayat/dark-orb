namespace BattleArena.Application.Models;

using Core.Entities.Enums;

public class DamageRollResult
{
    public DieType DieType { get; set; }
    public int Result { get; set; }
}
