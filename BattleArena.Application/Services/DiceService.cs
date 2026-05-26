namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities.Enums;

public class DiceService : IDiceService
{
    private readonly Random _random = new();

    public int Roll(DieType dieType)
    {
        var sides = dieType switch
        {
            DieType.D4 => 4,
            DieType.D6 => 6,
            DieType.D8 => 8,
            DieType.D10 => 10,
            DieType.D12 => 12,
            DieType.D20 => 20,
            DieType.D100 => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(dieType))
        };
        return _random.Next(1, sides + 1);
    }

    public int Roll(int count, int sides)
    {
        var total = 0;
        for (var i = 0; i < count; i++)
            total += _random.Next(1, sides + 1);
        return total;
    }

    public int RollWithAdvantage(DieType dieType)
    {
        var roll1 = Roll(dieType);
        var roll2 = Roll(dieType);
        return Math.Max(roll1, roll2);
    }

    public int RollWithDisadvantage(DieType dieType)
    {
        var roll1 = Roll(dieType);
        var roll2 = Roll(dieType);
        return Math.Min(roll1, roll2);
    }
}
