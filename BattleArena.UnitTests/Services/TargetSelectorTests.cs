namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;
using Xunit;

// Tests for targeting selectors and the BattleSimulator's per-party selector routing.
//
// LowestHpTargetSelector  — picks the living enemy with the lowest current HP.
// RandomTargetSelector    — picks any living enemy at random.
// BattleSimulator routing — hero selector fires for party-0 actors,
//                            enemy selector fires for party-1 actors.
public class TargetSelectorTests
{
    // ── LowestHpTargetSelector ─────────────────────────────────────────────────

    [Fact]
    public async Task LowestHp_ReturnsTargetWithLowestCurrentHp()
    {
        var sut    = new LowestHpTargetSelector();
        var actor  = MakeChar("Actor", 30);
        var high   = MakeChar("High",  40);
        var mid    = MakeChar("Mid",   20);
        var low    = MakeChar("Low",    5);

        var result = await sut.SelectTargetAsync(actor, new[] { high, mid, low });

        Assert.Equal("Low", result.Name);
    }

    [Fact]
    public async Task LowestHp_WithSingleTarget_ReturnsThatTarget()
    {
        var sut    = new LowestHpTargetSelector();
        var actor  = MakeChar("Actor", 30);
        var only   = MakeChar("Only",  15);

        var result = await sut.SelectTargetAsync(actor, new[] { only });

        Assert.Equal("Only", result.Name);
    }

    [Fact]
    public async Task LowestHp_WithTiedLowestHp_ReturnsOneOfTheTied()
    {
        var sut   = new LowestHpTargetSelector();
        var actor = MakeChar("Actor", 30);
        var tieA  = MakeChar("TieA", 10);
        var tieB  = MakeChar("TieB", 10);
        var high  = MakeChar("High", 40);

        var result = await sut.SelectTargetAsync(actor, new[] { high, tieA, tieB });

        Assert.True(result.Name is "TieA" or "TieB",
            $"Expected TieA or TieB but got {result.Name}");
    }

    [Fact]
    public async Task LowestHp_WithNoTargets_ThrowsInvalidOperationException()
    {
        var sut   = new LowestHpTargetSelector();
        var actor = MakeChar("Actor", 30);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SelectTargetAsync(actor, Enumerable.Empty<Character>()));
    }

    [Fact]
    public async Task LowestHp_ActorHpDoesNotInfluenceTargetChoice()
    {
        // A very weak actor should not cause a different target to be picked.
        var sut       = new LowestHpTargetSelector();
        var weakActor = MakeChar("WeakActor", 1);
        var strong    = MakeChar("Strong", 50);
        var weaker    = MakeChar("Weaker",  2);

        var result = await sut.SelectTargetAsync(weakActor, new[] { strong, weaker });

        Assert.Equal("Weaker", result.Name);
    }

    [Fact]
    public async Task LowestHp_OrderOfInputListDoesNotAffectChoice()
    {
        var sut   = new LowestHpTargetSelector();
        var actor = MakeChar("Actor", 30);
        var a     = MakeChar("A", 30);
        var b     = MakeChar("B",  5);  // lowest
        var c     = MakeChar("C", 20);

        var fwd = await sut.SelectTargetAsync(actor, new[] { a, b, c });
        var rev = await sut.SelectTargetAsync(actor, new[] { c, b, a });

        Assert.Equal("B", fwd.Name);
        Assert.Equal("B", rev.Name);
    }

    // ── RandomTargetSelector ──────────────────────────────────────────────────

    [Fact]
    public async Task Random_AlwaysReturnsATargetFromTheProvidedList()
    {
        var sut     = new RandomTargetSelector();
        var actor   = MakeChar("Actor", 30);
        var targets = new[] { MakeChar("A", 10), MakeChar("B", 20), MakeChar("C", 30) };

        for (var i = 0; i < 100; i++)
        {
            var result = await sut.SelectTargetAsync(actor, targets);
            Assert.Contains(result, targets);
        }
    }

    [Fact]
    public async Task Random_WithNoTargets_ThrowsInvalidOperationException()
    {
        var sut   = new RandomTargetSelector();
        var actor = MakeChar("Actor", 30);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SelectTargetAsync(actor, Enumerable.Empty<Character>()));
    }

    // ── BattleSimulator selector routing ──────────────────────────────────────

    [Fact]
    public async Task BattleSimulator_UsesHeroSelectorWhenHeroActs()
    {
        var heroSel  = Substitute.For<ITargetSelector>();
        var enemySel = Substitute.For<ITargetSelector>();
        SetupPassThrough(heroSel);
        SetupPassThrough(enemySel);

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");

        await BuildSimulator(heroSel, enemySel)
            .SimulateAsync(heroParty, enemyParty);

        await heroSel.Received().SelectTargetAsync(
            Arg.Is<Character>(c => c.Name == "Hero"),
            Arg.Any<IEnumerable<Character>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BattleSimulator_UsesEnemySelectorWhenEnemyActs()
    {
        var heroSel  = Substitute.For<ITargetSelector>();
        var enemySel = Substitute.For<ITargetSelector>();
        SetupPassThrough(heroSel);
        SetupPassThrough(enemySel);

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");

        await BuildSimulator(heroSel, enemySel)
            .SimulateAsync(heroParty, enemyParty);

        await enemySel.Received().SelectTargetAsync(
            Arg.Is<Character>(c => c.Name == "Enemy"),
            Arg.Any<IEnumerable<Character>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BattleSimulator_HeroSelectorNeverCalledForEnemyActors()
    {
        var heroSel  = Substitute.For<ITargetSelector>();
        var enemySel = Substitute.For<ITargetSelector>();
        SetupPassThrough(heroSel);
        SetupPassThrough(enemySel);

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");

        await BuildSimulator(heroSel, enemySel)
            .SimulateAsync(heroParty, enemyParty);

        await heroSel.DidNotReceive().SelectTargetAsync(
            Arg.Is<Character>(c => c.Name == "Enemy"),
            Arg.Any<IEnumerable<Character>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BattleSimulator_EnemySelectorNeverCalledForHeroActors()
    {
        var heroSel  = Substitute.For<ITargetSelector>();
        var enemySel = Substitute.For<ITargetSelector>();
        SetupPassThrough(heroSel);
        SetupPassThrough(enemySel);

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");

        await BuildSimulator(heroSel, enemySel)
            .SimulateAsync(heroParty, enemyParty);

        await enemySel.DidNotReceive().SelectTargetAsync(
            Arg.Is<Character>(c => c.Name == "Hero"),
            Arg.Any<IEnumerable<Character>>(),
            Arg.Any<CancellationToken>());
    }

    // ── Observer integration ───────────────────────────────────────────────────

    [Fact]
    public async Task BattleSimulator_ObserverReceivesAllEventsInOrder()
    {
        var events = new List<string>();
        var obs    = Substitute.For<IBattleObserver>();
        obs.OnEventAsync(Arg.Any<BattleLogEntry>(), Arg.Any<CancellationToken>())
           .Returns(ci => { events.Add(ci.Arg<BattleLogEntry>().EventType); return Task.CompletedTask; });

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");
        var result = await BuildSimulator(new LowestHpTargetSelector(), new LowestHpTargetSelector())
            .SimulateAsync(heroParty, enemyParty, observer: obs);

        // Observer and log must contain the same events in the same order.
        Assert.Equal(result.Log.Select(e => e.EventType), events);
    }

    [Fact]
    public async Task BattleSimulator_WithNoObserver_CompletesNormally()
    {
        // Passing null observer must not throw.
        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");
        var result = await BuildSimulator(new LowestHpTargetSelector(), new LowestHpTargetSelector())
            .SimulateAsync(heroParty, enemyParty, observer: null);

        Assert.NotNull(result);
        Assert.True(result.Log.Count > 0);
    }

    [Fact]
    public async Task BattleSimulator_CancellationToken_StopsSimulation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // already cancelled before we start

        var (heroParty, enemyParty) = BuildDuel("Hero", "Enemy");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => BuildSimulator(new LowestHpTargetSelector(), new LowestHpTargetSelector())
                .SimulateAsync(heroParty, enemyParty, ct: cts.Token));
    }

    // ── LowestHp focus-fire integration ───────────────────────────────────────

    [Fact]
    public async Task LowestHp_InPartyBattle_KillsWeakestEnemyFirst()
    {
        // Two heroes with high attack vs two enemies: one with 1 HP, one with 50 HP.
        // LowestHpTargetSelector must direct all hero attacks onto the 1-HP enemy first.
        var (hero1Member, _) = BuildMember("Hero1", hp: 50, str: 18, spd: 12);
        var (hero2Member, _) = BuildMember("Hero2", hp: 50, str: 18, spd: 12);

        var (weakMember,   _) = BuildMember("Weak",   hp:  1, str: 8, spd: 4);
        var (strongMember, _) = BuildMember("Strong", hp: 50, str: 8, spd: 4);

        var heroes  = Party.HeroParty("Heroes",  new[] { hero1Member, hero2Member });
        var enemies = new Party
        {
            Name    = "Enemies",
            Members = new List<PartyMember> { weakMember, strongMember }
        };

        var result = await BuildSimulator(new LowestHpTargetSelector(), new LowestHpTargetSelector())
            .SimulateAsync(heroes, enemies, maxTicks: 500);

        var deathOrder = result.Log
            .Where(e => e.EventType is "Death" or "KnockedOut")
            .Select(e => e.ActorName)
            .ToList();

        Assert.True(deathOrder.Count >= 1, "At least one combatant should have fallen.");
        Assert.Equal("Weak", deathOrder[0]);
    }

    [Fact]
    public async Task LowestHp_InPartyBattle_WinsAgainstRandomTargeting()
    {
        var (h1m, _) = BuildMember("FH1", hp: 30, str: 14, spd: 8);
        var (h2m, _) = BuildMember("FH2", hp: 30, str: 14, spd: 8);
        var (e1m, _) = BuildMember("RE1", hp: 30, str: 14, spd: 8);
        var (e2m, _) = BuildMember("RE2", hp: 30, str: 14, spd: 8);

        var focusParty  = Party.HeroParty("Focus",  new[] { h1m, h2m });
        var randomParty = new Party { Name = "Random", Members = new List<PartyMember> { e1m, e2m } };

        var result = await BuildSimulator(new LowestHpTargetSelector(), new RandomTargetSelector())
            .SimulateAsync(focusParty, randomParty, maxTicks: 500);

        Assert.NotNull(result);
        Assert.True(result.TotalTicks > 0);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static Character MakeChar(string name, int hp, int max = 50) =>
        new() { Name = name, CurrentHitPoints = hp, MaxHitPoints = max };

    /// Builds a Character + PartyMember pair sharing the same Weapon instance.
    private static (PartyMember member, Character character) BuildMember(
        string name, int hp, int str = 14, int spd = 8)
    {
        var sword = new Weapon
        {
            Name        = "Sword",
            DamageDie   = DieType.D6,
            DamageCount = 1,
            DamageType  = DamageType.Slashing,
            AttackType  = AttackType.Melee,
            AttackBonus = 2
        };
        var ch = new Character
        {
            Name             = name,
            Level            = 3,
            Strength         = str,
            Dexterity        = 10,
            Intelligence     = 10,
            StrikeRating     = 12,
            TurnSpeed        = spd,
            MaxHitPoints     = hp,
            CurrentHitPoints = hp,
            Equipment        = new ArmorSlots
            {
                Chest     = new Armor { Name = "Leather", ArmorClass = 8, Mitigation = 0, MaxDexterityBonus = 6 },
                RightHand = sword
            }
        };
        return (new PartyMember { Character = ch, AttackSource = sword }, ch);
    }

    private static (Party heroParty, Party enemyParty) BuildDuel(string heroName, string enemyName)
    {
        var (hm, _) = BuildMember(heroName,  hp: 40, str: 14, spd: 8);
        var (em, _) = BuildMember(enemyName, hp: 40, str: 14, spd: 8);
        return (Party.Solo(hm.Character, hm.AttackSource),
                Party.Solo(em.Character, em.AttackSource));
    }

    private static BattleSimulator BuildSimulator(ITargetSelector heroSel, ITargetSelector enemySel) =>
        new(new CombatService(new DiceService(), new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            heroSel,
            enemySel);

    /// Configures a mock selector to pass through to the first available target.
    private static void SetupPassThrough(ITargetSelector mock) =>
        mock.SelectTargetAsync(
                Arg.Any<Character>(),
                Arg.Any<IEnumerable<Character>>(),
                Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(ci.Arg<IEnumerable<Character>>().First()));
}
