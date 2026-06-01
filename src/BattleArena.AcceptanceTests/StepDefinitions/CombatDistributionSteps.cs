namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

[Binding]
public class CombatDistributionSteps
{
    private readonly ICombatService _combat;
    private Character _attacker = null!;
    private Character _defender = null!;
    private int _hitCount;
    private int _criticalCount;
    private int _fumbleCount;
    private int _perfectParryCount;
    private int _devastatingCount;
    private int _totalReversalCount;
    private int _totalAttacks;

    // Reused attack source (unarmed = no weapon bonuses)
    private static readonly IAttackSource Unarmed = UnarmedStrike.Default;

    public CombatDistributionSteps()
    {
        var dice = new DiceService();
        var stats = new CombatStatsService();
        _combat = new CombatService(dice, stats);
    }

    [Given(@"a distribution attacker at level (\d+) with strength (\d+) and strike rating (\d+)")]
    public void GivenAttacker(int level, int strength, int strikeRating)
    {
        _attacker = new Character
        {
            Level = level,
            Strength = strength,
            StrikeRating = strikeRating,
            Dexterity = 10,
            MaxHitPoints = 100,
            CurrentHitPoints = 100
        };
    }

    [Given(@"the distribution attacker wields an unarmed strike")]
    public void GivenAttackerWieldsUnarmed()
    {
        // UnarmedStrike.Default is used in When step — nothing to configure here.
    }

    [Given(@"a distribution defender with armor class (\d+) and dexterity (\d+)")]
    public void GivenDefender(int armorClass, int dexterity)
    {
        _defender = new Character
        {
            Level = 1,
            Dexterity = dexterity,
            StrikeRating = 8,
            MaxHitPoints = 100,
            CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    Name = "Test Armor",
                    ArmorClass = armorClass,
                    MaxDexterityBonus = 10
                }
            }
        };
    }

    [When(@"(\d+) attacks are resolved")]
    public void WhenAttacksAreResolved(int count)
    {
        _totalAttacks = count;
        _hitCount = 0;
        _criticalCount = 0;
        _fumbleCount = 0;
        _perfectParryCount = 0;
        _devastatingCount = 0;
        _totalReversalCount = 0;

        for (var i = 0; i < count; i++)
        {
            var result = _combat.ResolveAttack(_attacker, _defender, Unarmed);

            if (result.IsHit)
                _hitCount++;
            if (result.IsCriticalHit)
                _criticalCount++;
            if (result.IsFumble)
                _fumbleCount++;
            if (result.IsPerfectParry)
                _perfectParryCount++;
            if (result.IsDevastatingStrike)
                _devastatingCount++;
            if (result.IsTotalReversal)
                _totalReversalCount++;
        }
    }

    [Then(@"the total hit count should be between (\d+) and (\d+)")]
    public void ThenHitCountBetween(int lower, int upper)
    {
        Assert.True(_hitCount >= lower && _hitCount <= upper,
            $"Hit count {_hitCount} outside expected range [{lower}, {upper}]");
    }

    [Then(@"the critical hit rate should be between (\d+)% and (\d+)%")]
    public void ThenCriticalRateBetween(int lowerPct, int upperPct)
    {
        var actual = (double)_criticalCount / _totalAttacks * 100;
        Assert.True(actual >= lowerPct && actual <= upperPct,
            $"Critical rate {actual:F2}% ({_criticalCount}/{_totalAttacks}) outside expected range [{lowerPct}%, {upperPct}%]");
    }

    [Then(@"the fumble rate should be between (\d+)% and (\d+)%")]
    public void ThenFumbleRateBetween(int lowerPct, int upperPct)
    {
        var actual = (double)_fumbleCount / _totalAttacks * 100;
        Assert.True(actual >= lowerPct && actual <= upperPct,
            $"Fumble rate {actual:F2}% ({_fumbleCount}/{_totalAttacks}) outside expected range [{lowerPct}%, {upperPct}%]");
    }

    [Then(@"the perfect parry rate should be between (\d+)% and (\d+)%")]
    public void ThenPerfectParryRateBetween(int lowerPct, int upperPct)
    {
        var actual = (double)_perfectParryCount / _totalAttacks * 100;
        Assert.True(actual >= lowerPct && actual <= upperPct,
            $"Perfect parry rate {actual:F2}% ({_perfectParryCount}/{_totalAttacks}) outside expected range [{lowerPct}%, {upperPct}%]");
    }
}
