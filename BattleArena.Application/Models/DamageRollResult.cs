using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Models;

public class DamageRollResult
{
    public DieType DieType { get; set; }
    public int Result { get; set; }
}
