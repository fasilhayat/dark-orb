namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;

public class CombatSimulatorTests
{
    [Fact]
    public void Simulate_CombatWithoutActions_EmitsRoundBoundaryEvents()
    {
        var (_, simulator) = CreateSimulator();
        var hero = CreateCharacter("Hero", maxHitPoints: 100, turnSpeed: 1);
        var enemy = CreateCharacter("Enemy", maxHitPoints: 100, turnSpeed: 1);

        var result = simulator.Simulate(Party.Solo(hero), Party.Solo(enemy), 20);

        var roundStarts = result.Log.Where(e => e.EventType == "RoundStart").ToList();
        var roundEnds = result.Log.Where(e => e.EventType == "RoundEnd").ToList();

        Assert.Equal([1, 11], roundStarts.Select(e => e.Tick));
        Assert.Equal([1, 2], roundStarts.Select(e => e.RoundNumber));
        Assert.Equal([10, 20], roundEnds.Select(e => e.Tick));
        Assert.Equal([1, 2], roundEnds.Select(e => e.RoundNumber));
    }

    [Fact]
    public void Simulate_SummonSpell_EmitsPetEventsAndExpiresAtRoundEnd()
    {
        var (_, simulator) = CreateSimulator(
            new FixedTargetSelector(new Dictionary<string, string> { ["Elara"] = "Training Dummy" }),
            new FixedTargetSelector(new Dictionary<string, string> { ["Training Dummy"] = "Elara" }));
        var summoner = CreateSummoner(CreateSummonSpell(durationRounds: 1));
        var enemy = CreateCharacter("Training Dummy", maxHitPoints: 200, turnSpeed: 1);

        var result = simulator.Simulate(Party.Solo(summoner), Party.Solo(enemy), 20);

        var petSummoned = Assert.Single(result.Log.Where(e => e.EventType == "PetSummoned"));
        Assert.Equal("Spirit Wolf", petSummoned.SummonedPetName);
        Assert.Equal(1, petSummoned.Tick);
        Assert.Equal(1, petSummoned.RoundNumber);

        var petExpired = Assert.Single(result.Log.Where(e => e.EventType == "PetExpired"));
        Assert.Equal("Spirit Wolf", petExpired.SummonedPetName);
        Assert.Equal(20, petExpired.Tick);
        Assert.Equal(2, petExpired.RoundNumber);
    }

    [Fact]
    public void Simulate_SummonedPet_PrefersLastAttackerOfMaster()
    {
        var (_, simulator) = CreateSimulator(
            new FixedTargetSelector(new Dictionary<string, string> { ["Elara"] = "Bystander" }),
            new FixedTargetSelector(new Dictionary<string, string> { ["Attacker"] = "Elara" }));
        var summoner = CreateSummoner(CreateSummonSpell(durationRounds: 3));
        var attacker = CreateEnemy("Attacker", turnSpeed: 100);
        var bystander = CreateEnemy("Bystander", turnSpeed: 1);
        var enemyParty = new Party
        {
            Name = "Enemies",
            Members =
            [
                new PartyMember { Character = attacker, AttackSource = CreateWeapon("Club", DieType.D8) },
                new PartyMember { Character = bystander, AttackSource = CreateWeapon("Knife", DieType.D4) }
            ]
        };

        var result = simulator.Simulate(Party.Solo(summoner), enemyParty, 2);

        var petTurn = Assert.Single(result.Log.Where(e => e.EventType == "TurnStart" && e.ActorName == "Spirit Wolf"));
        Assert.Equal("Attacker", petTurn.TargetName);
    }

    private static (IDiceService Dice, CombatSimulator Simulator) CreateSimulator(
        ITargetSelector? heroTargetSelector = null,
        ITargetSelector? enemyTargetSelector = null)
    {
        var dice = Substitute.For<IDiceService>();
        dice.Seed.Returns(123);
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        dice.Roll(DieType.D20).Returns(20);
        dice.Roll(DieType.D4).Returns(1);
        dice.Roll(DieType.D6).Returns(1);
        dice.Roll(DieType.D8).Returns(1);
        dice.Roll(DieType.D100).Returns(100);

        var simulator = new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice,
            heroTargetSelector,
            enemyTargetSelector);

        return (dice, simulator);
    }

    private static Character CreateSummoner(Spell summonSpell) => new()
    {
        Name = "Elara",
        ClassId = 7,
        Level = 1,
        Intelligence = 12,
        Dexterity = 10,
        Strength = 8,
        StrikeRating = 14,
        TurnSpeed = 100,
        MaxHitPoints = 100,
        CurrentHitPoints = 100,
        MaxMana = summonSpell.ManaCost,
        CurrentMana = summonSpell.ManaCost,
        MemorizedSpells = [summonSpell]
    };

    private static Character CreateEnemy(string name, int turnSpeed) => CreateCharacter(name, maxHitPoints: 100, turnSpeed: turnSpeed);

    private static Character CreateCharacter(string name, int maxHitPoints, int turnSpeed) => new()
    {
        Name = name,
        ClassId = 8,
        Level = 1,
        Strength = 10,
        Dexterity = 10,
        Intelligence = 10,
        StrikeRating = 14,
        TurnSpeed = turnSpeed,
        MaxHitPoints = maxHitPoints,
        CurrentHitPoints = maxHitPoints
    };

    private static Spell CreateSummonSpell(int durationRounds) => new()
    {
        Name = "Summon Spirit Wolf",
        School = SpellSchool.Conjuration,
        DamageDie = DieType.D4,
        DamageCount = 0,
        DamageType = DamageType.Bludgeoning,
        SpellLevel = 3,
        TurnMeterCost = 90,
        ManaCost = 35,
        SummonedPet = new Pet
        {
            Name = "Spirit Wolf",
            MaxHitPoints = 20,
            ArmorClass = 12,
            TurnSpeed = 100,
            Strength = 14,
            StrikeRating = 14,
            AttackBonus = 2,
            DamageCount = 1,
            DamageDie = DieType.D6,
            DamageType = DamageType.Slashing,
            SummonDurationRounds = durationRounds
        }
    };

    private static Weapon CreateWeapon(string name, DieType damageDie) => new()
    {
        Name = name,
        DamageDie = damageDie,
        DamageCount = 1,
        DamageType = DamageType.Bludgeoning,
        AttackType = AttackType.Melee
    };

    private sealed class FixedTargetSelector(Dictionary<string, string> preferredTargets) : ITargetSelector
    {
        public Task<Character> SelectTargetAsync(Character actor, IEnumerable<Character> candidates, CancellationToken ct = default)
        {
            var options = candidates.ToList();
            if (preferredTargets.TryGetValue(actor.Name, out var preferredTarget))
            {
                var match = options.FirstOrDefault(candidate => candidate.Name == preferredTarget);
                if (match is not null)
                    return Task.FromResult(match);
            }

            return Task.FromResult(options[0]);
        }
    }
}
