using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Interfaces;

public interface IDiceService
{
    int Roll(DieType dieType);
    int Roll(int count, int sides);
    int RollWithAdvantage(DieType dieType);
    int RollWithDisadvantage(DieType dieType);
}
