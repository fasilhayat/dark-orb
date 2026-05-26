namespace BattleArena.Application.Interfaces;

using Core.Entities.Enums;

public interface IDiceService
{
    int Roll(DieType dieType);
    int Roll(int count, int sides);
    int RollWithAdvantage(DieType dieType);
    int RollWithDisadvantage(DieType dieType);
}
