using BattleArena.Application.Services;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.UnitTests.Services;

public class DiceServiceTests
{
    private readonly DiceService _sut = new();

    [Fact]
    public void Roll_WithD6_ReturnsBetween1And6()
    {
        var results = Enumerable.Range(0, 100).Select(_ => _sut.Roll(DieType.D6)).ToList();

        Assert.All(results, r => Assert.InRange(r, 1, 6));
    }

    [Fact]
    public void Roll_WithD20_ReturnsBetween1And20()
    {
        var results = Enumerable.Range(0, 100).Select(_ => _sut.Roll(DieType.D20)).ToList();

        Assert.All(results, r => Assert.InRange(r, 1, 20));
    }

    [Fact]
    public void Roll_WithD100_ReturnsBetween1And100()
    {
        var results = Enumerable.Range(0, 50).Select(_ => _sut.Roll(DieType.D100)).ToList();

        Assert.All(results, r => Assert.InRange(r, 1, 100));
    }

    [Fact]
    public void Roll_WithCountAndSides_ReturnsCorrectRange()
    {
        var result = _sut.Roll(2, 6);

        Assert.InRange(result, 2, 12);
    }

    [Fact]
    public void Roll_ZeroCount_ReturnsZero()
    {
        var result = _sut.Roll(0, 6);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Roll_InvalidDieType_ThrowsArgumentOutOfRangeException()
    {
        var invalid = (DieType)999;

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Roll(invalid));
        Assert.Contains("dieType", ex.ParamName);
    }

    [Fact]
    public void RollWithAdvantage_ReturnsHigherOfTwoRolls()
    {
        var (normalTotal, advTotal, disadvantageTotal) = (0, 0, 0);
        var iterations = 100;

        for (var i = 0; i < iterations; i++)
        {
            normalTotal += _sut.Roll(DieType.D20);
            advTotal += _sut.RollWithAdvantage(DieType.D20);
            disadvantageTotal += _sut.RollWithDisadvantage(DieType.D20);
        }

        var normalAvg = normalTotal / (double)iterations;
        var advAvg = advTotal / (double)iterations;
        var disAvg = disadvantageTotal / (double)iterations;

        Assert.True(advAvg > normalAvg, $"Advantage average ({advAvg}) should be higher than normal ({normalAvg})");
        Assert.True(disAvg < normalAvg, $"Disadvantage average ({disAvg}) should be lower than normal ({normalAvg})");
    }

    [Theory]
    [InlineData(DieType.D4, 4)]
    [InlineData(DieType.D6, 6)]
    [InlineData(DieType.D8, 8)]
    [InlineData(DieType.D10, 10)]
    [InlineData(DieType.D12, 12)]
    [InlineData(DieType.D20, 20)]
    [InlineData(DieType.D100, 100)]
    public void Roll_AllDieTypes_RespectsUpperBound(DieType dieType, int maxValue)
    {
        var results = Enumerable.Range(0, 50).Select(_ => _sut.Roll(dieType)).ToList();

        Assert.All(results, r => Assert.InRange(r, 1, maxValue));
    }
}
