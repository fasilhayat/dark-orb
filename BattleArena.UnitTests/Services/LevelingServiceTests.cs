namespace BattleArena.UnitTests.Services;

using Application.Models;
using Application.Services;
using Core.Entities;

public class LevelingServiceTests
{
    private readonly LevelingService _sut = new();

    [Fact]
    public void ComputeCombatXp_BaseOnly_ReturnsSumOfEnemyLevelsTimesMultiplier()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[]
        {
            MakeChar("Goblin", 3),
            MakeChar("Orc", 5),
        };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal((3 + 5) * 12, xp);
    }

    [Fact]
    public void ComputeCombatXp_WithPartyCrits_AddsBonusPerCrit()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = new[]
        {
            MakeAttack("Hero", crit: true),
            MakeAttack("Hero", crit: true),
            MakeAttack("Hero", crit: false),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal(baseXp + 2 * 8, xp);
    }

    [Fact]
    public void ComputeCombatXp_WithPartyFumbles_SubtractsPenaltyPerFumble()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = new[]
        {
            MakeAttack("Hero", fumble: true),
            MakeAttack("Hero", fumble: true),
            MakeAttack("Hero", crit: false),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal(baseXp - 2 * 8, xp);
    }

    [Fact]
    public void ComputeCombatXp_CritsAndFumblesNetOut()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = new[]
        {
            MakeAttack("Hero", crit: true),
            MakeAttack("Hero", fumble: true),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal(3 * 12, xp);
    }

    [Fact]
    public void ComputeCombatXp_EnemyCritsDoNotAffectPartyBonus()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = new[]
        {
            MakeAttack("Hero", crit: true),
            MakeAttack("Goblin", crit: true),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal(3 * 12 + 8, xp);
    }

    [Fact]
    public void ComputeCombatXp_AtExpectedRounds_FactorIsOne()
    {
        var party = new[] { MakeChar("A", 1), MakeChar("B", 1) };
        var enemies = new[] { MakeChar("E1", 2), MakeChar("E2", 2) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal((2 + 2) * 12, xp);
    }

    [Fact]
    public void ComputeCombatXp_DoubleExpectedRounds_GivesThirtyPercentBonus()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();
        var expected = ExpectedRounds(party, enemies);
        var ticks = expected * 2;

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal((int)(baseXp * 1.3), xp);
    }

    [Fact]
    public void ComputeCombatXp_HalfExpectedRounds_GivesFifteenPercentBonus()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 4) };
        var log = Array.Empty<CombatLogEntry>();
        var expected = ExpectedRounds(party, enemies);
        var ticks = Math.Max(1, expected / 2);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);
        var baseXp = 4 * 12;

        Assert.Equal((int)(baseXp * 1.15), xp);
    }

    [Fact]
    public void ComputeCombatXp_ExtremeLongFight_ClampsAtMaxFactor()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = 9999;

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal(baseXp * 2, xp);
    }

    [Fact]
    public void ComputeCombatXp_ZeroEnemyLevels_ReturnsZeroPlusBonuses()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Rat", 0) };
        var log = new[]
        {
            MakeAttack("Hero", crit: true),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal(8, xp);
    }

    [Fact]
    public void ComputeCombatXp_EmptyEnemies_ReturnsZero()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = Array.Empty<Character>();
        var log = Array.Empty<CombatLogEntry>();
        var ticks = 10;

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal(0, xp);
    }

    [Fact]
    public void ComputeCombatXp_NegativeXpFromFumbles_ClampsAtMinimum()
    {
        var party = new[] { MakeChar("Hero", 1) };
        var enemies = new[] { MakeChar("Rat", 1) };
        var log = new[]
        {
            MakeAttack("Hero", fumble: true),
            MakeAttack("Hero", fumble: true),
            MakeAttack("Hero", fumble: true),
        };
        var ticks = ExpectedRounds(party, enemies);

        var xp = _sut.ComputeCombatXp(party, enemies, log, ticks);

        Assert.Equal(1, xp);
    }

    [Fact]
    public void AwardCombatXp_SplitsEvenlyAmongSurvivors()
    {
        var party = new[]
        {
            MakeChar("Theron", 1, hp: 100, alive: true),
            MakeChar("Lyra", 1, hp: 100, alive: true),
        };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party, enemies);

        var awards = _sut.AwardCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal(2, awards.Count);
        Assert.Equal(baseXp / 2, awards["Theron"]);
        Assert.Equal(baseXp / 2, awards["Lyra"]);
    }

    [Fact]
    public void AwardCombatXp_DeadCharacterReceivesNothing()
    {
        var party = new[]
        {
            MakeChar("Theron", 1, hp: 100, alive: true),
            MakeChar("Lyra", 1, hp: 0, alive: false),
        };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party.Where(c => c.CurrentHitPoints > 0), enemies);

        var awards = _sut.AwardCombatXp(party, enemies, log, ticks);

        Assert.Single(awards);
        Assert.True(awards.ContainsKey("Theron"));
        Assert.False(awards.ContainsKey("Lyra"));
    }

    [Fact]
    public void AwardCombatXp_AllDead_ReturnsEmpty()
    {
        var party = new[]
        {
            MakeChar("Theron", 1, hp: 0, alive: false),
        };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();

        var awards = _sut.AwardCombatXp(party, enemies, log, 10);

        Assert.Empty(awards);
    }

    [Fact]
    public void AwardCombatXp_UpdatesCharacterExperiencePoints()
    {
        var party = new[] { MakeChar("Theron", 1, hp: 100, alive: true) };
        var enemies = new[] { MakeChar("Goblin", 3) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party, enemies);

        _ = _sut.AwardCombatXp(party, enemies, log, ticks);
        var baseXp = 3 * 12;

        Assert.Equal(baseXp, party[0].ExperiencePoints);
    }

    [Fact]
    public void AwardCombatXp_CharacterLevelsUpWhenXpExceedsThreshold()
    {
        var party = new[] { MakeChar("Theron", 1, xp: 95, hp: 100, alive: true) };
        var enemies = new[] { MakeChar("Goblin", 5) };
        var log = Array.Empty<CombatLogEntry>();
        var ticks = ExpectedRounds(party, enemies);
        var gain = 5 * 12;

        _ = _sut.AwardCombatXp(party, enemies, log, ticks);

        Assert.Equal(95 + gain, party[0].ExperiencePoints);
        Assert.True(party[0].Level > 1);
    }

    [Fact]
    public void EffectiveStrikeRating_Martial_ReducesEveryTwoLevels()
    {
        var c = MakeChar("Fighter", 10, classId: 8);

        var sr = _sut.EffectiveStrikeRating(c);

        var expectedReduction = Math.Min((10 - 1) / 2, 6);
        Assert.Equal(c.StrikeRating - expectedReduction, sr);
    }

    [Fact]
    public void EffectiveStrikeRating_Caster_ReducesEveryFourLevels()
    {
        var c = MakeChar("Mage", 12, classId: 5);

        var sr = _sut.EffectiveStrikeRating(c);

        var expectedReduction = Math.Min((12 - 1) / 4, 6);
        Assert.Equal(c.StrikeRating - expectedReduction, sr);
    }

    [Fact]
    public void EffectiveStrikeRating_Hybrid_ReducesEveryThreeLevels()
    {
        var c = MakeChar("Rogue", 9, classId: 9);

        var sr = _sut.EffectiveStrikeRating(c);

        var expectedReduction = Math.Min((9 - 1) / 3, 6);
        Assert.Equal(c.StrikeRating - expectedReduction, sr);
    }

    [Fact]
    public void EffectiveStrikeRating_MinimumOfOne()
    {
        var c = MakeChar("Weakling", 12, classId: 8);
        c.StrikeRating = 1;

        var sr = _sut.EffectiveStrikeRating(c);

        Assert.Equal(1, sr);
    }

    [Fact]
    public void AccessorySlotCount_Martial_FollowsCommonProgression()
    {
        var c = MakeChar("Fighter", 9, classId: 8);

        var slots = _sut.AccessorySlotCount(c);

        Assert.Equal(3, slots);
    }

    [Fact]
    public void AccessorySlotCount_Caster_GetsBonusSlots()
    {
        var c = MakeChar("Mage", 11, classId: 5);

        var slots = _sut.AccessorySlotCount(c);

        Assert.Equal(5, slots); // 3 common + 2 caster
    }

    [Fact]
    public void AccessorySlotCount_CasterMaxLevel_SixSlots()
    {
        var c = MakeChar("Mage", 12, classId: 5);

        var slots = _sut.AccessorySlotCount(c);

        Assert.Equal(6, slots); // 4 common + 2 caster
    }

    [Fact]
    public void AccessorySlotCount_Hybrid_GetsBonusAtLevelTen()
    {
        var c = MakeChar("Bard", 12, classId: 6);

        var slots = _sut.AccessorySlotCount(c);

        Assert.Equal(5, slots); // 4 common + 1 hybrid
    }

    [Fact]
    public void CheckLevelUp_NoChange_ReturnsNull()
    {
        var before = MakeChar("Hero", 1);
        var after = MakeChar("Hero", 1);

        var result = _sut.CheckLevelUp(before, after);

        Assert.Null(result);
    }

    [Fact]
    public void CheckLevelUp_LevelUp_ReturnsDelta()
    {
        var before = MakeChar("Hero", 1, classId: 8);
        var after = MakeChar("Hero", 3, classId: 8);

        var result = _sut.CheckLevelUp(before, after);

        Assert.NotNull(result);
        Assert.Equal(1, result.OldLevel);
        Assert.Equal(3, result.NewLevel);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static int ExpectedRounds(IEnumerable<Character> party, IEnumerable<Character> enemies) =>
        Math.Max(1, (party.Count() + enemies.Count()) * 2);

    private static CombatLogEntry MakeAttack(string actor, bool crit = false, bool fumble = false) => new()
    {
        Tick = 1,
        ActorName = actor,
        EventType = "Attack",
        IsCritical = crit,
        IsFumble = fumble,
        IsHit = !fumble,
    };

    private static Character MakeChar(string name, int level, int classId = 8, int xp = 0, int hp = 100, bool alive = true) => new()
    {
        Name = name,
        Level = level,
        ClassId = classId,
        ExperiencePoints = xp,
        MaxHitPoints = hp,
        CurrentHitPoints = alive ? hp : 0,
        StrikeRating = 20,
    };
}
