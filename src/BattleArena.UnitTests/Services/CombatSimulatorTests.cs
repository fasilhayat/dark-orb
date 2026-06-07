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
        var enemy = CreateCharacter("Training Dummy", maxHitPoints: 500, turnSpeed: 1);

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

    private static (IDiceService Dice, ICombatSimulator Simulator) CreateSimulator(
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
        School = SpellSchool.Verdancy,
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

// ── Spellcaster combat tests ───────────────────────────────────────────────────

public class CombatSimulatorSpellcasterTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static (IDiceService Dice, ICombatSimulator Simulator) CreateSimulator()
    {
        var dice = Substitute.For<IDiceService>();
        dice.Seed.Returns(99);
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        dice.Roll(DieType.D20).Returns(15);   // hits most defenses
        dice.Roll(DieType.D4).Returns(2);
        dice.Roll(DieType.D6).Returns(3);
        dice.Roll(DieType.D8).Returns(4);
        dice.Roll(DieType.D10).Returns(5);
        dice.Roll(DieType.D100).Returns(100); // no resistance triggers

        return (dice, new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice));
    }

    private static Character CreateSpellcaster(string name, Spell spell, int manaCost = 0) => new()
    {
        Name             = name,
        ClassId          = 5,
        Level            = 5,
        Strength         = 8,
        Dexterity        = 14,
        Intelligence     = 18,
        StrikeRating     = 13,
        TurnSpeed        = 100,
        MaxHitPoints     = 30,
        CurrentHitPoints = 30,
        MaxMana          = 300,
        CurrentMana      = 300,
        MemorizedSpells  = [MakeSpell(spell.Name, manaCost)]
    };

    private static Character CreateToughTarget(string name) => new()
    {
        Name             = name,
        ClassId          = 8,
        Level            = 1,
        Strength         = 10,
        Dexterity        = 10,
        Intelligence     = 10,
        StrikeRating     = 14,
        TurnSpeed        = 1,
        MaxHitPoints     = 300,
        CurrentHitPoints = 300
    };

    private static Spell MakeSpell(string name, int manaCost = 0) => new()
    {
        Name          = name,
            School        = SpellSchool.Stormcraft,
            DamageDie     = DieType.D8,
            DamageCount   = 2,
            DamageType    = DamageType.Fire,
        AttackBonus   = 2,
        SpellLevel    = 2,
        TurnMeterCost = 80,
        ManaCost      = manaCost
    };

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Simulate_SpellcasterWithNullAttackSource_CastsMemorizedSpells()
    {
        var (_, simulator) = CreateSimulator();
        var caster = CreateSpellcaster("Lyra", MakeSpell("Fireball"));

        // Party.Solo leaves AttackSource = null → ResolveAttackSource picks spells
        var result = simulator.Simulate(Party.Solo(caster), Party.Solo(CreateToughTarget("Dummy")), 30);

        Assert.Contains(result.Log,
            e => e.EventType == "Attack" && e.ActorName == "Lyra" && e.AttackSourceName == "Fireball");
    }

    [Fact]
    public void Simulate_SpellcasterCastingSpells_EmitsManaDeductEvents()
    {
        var (_, simulator) = CreateSimulator();
        var caster = CreateSpellcaster("Mira", MakeSpell("Fireball"), manaCost: 20);

        var result = simulator.Simulate(Party.Solo(caster), Party.Solo(CreateToughTarget("Dummy")), 30);

        var deducts = result.Log.Where(e => e.EventType == "ManaDeduct" && e.ActorName == "Mira").ToList();
        Assert.NotEmpty(deducts);
        Assert.All(deducts, e =>
        {
            Assert.False(string.IsNullOrEmpty(e.ActorName));
            Assert.True(e.ManaCost is > 0,    $"ManaCost must be > 0 on ManaDeduct (tick {e.Tick})");
            Assert.True(e.ManaAfter.HasValue,  $"ManaAfter must be set on ManaDeduct (tick {e.Tick})");
        });
    }

    [Fact]
    public void Simulate_SpellcasterWithNullAttackSource_DoesNotUseUnarmedWhileManaAvailable()
    {
        var (_, simulator) = CreateSimulator();
        var caster = CreateSpellcaster("Lyra", MakeSpell("Fireball"), manaCost: 5);

        var result = simulator.Simulate(Party.Solo(caster), Party.Solo(CreateToughTarget("Dummy")), 20);

        var unarmedAttacks = result.Log
            .Where(e => e.EventType == "Attack" && e.ActorName == "Lyra"
                     && (e.AttackSourceName == "Unarmed" || e.AttackSourceName == UnarmedStrike.Default.Name))
            .ToList();

        // With 300 mana and manaCost 5, caster can cast 60 times — no fallback to unarmed in 20 ticks
        Assert.Empty(unarmedAttacks);
    }
}

// ── Healing combat tests ───────────────────────────────────────────────────────

public class CombatSimulatorHealingTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static (IDiceService Dice, ICombatSimulator Simulator) CreateSimulator()
    {
        var dice = Substitute.For<IDiceService>();
        dice.Seed.Returns(42);
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        dice.Roll(DieType.D20).Returns(15);
        dice.Roll(DieType.D4).Returns(2);
        dice.Roll(DieType.D6).Returns(3);
        dice.Roll(DieType.D8).Returns(4);
        dice.Roll(DieType.D10).Returns(5);
        dice.Roll(DieType.D100).Returns(100);

        return (dice, new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice));
    }

    private static Character MakeHealer(string name, int hp, int maxHp, int mana, params Spell[] spells) =>
        new()
        {
            Name             = name,
            ClassId          = 6,
            Level            = 4,
            Strength         = 10,
            Dexterity        = 12,
            Intelligence     = 16,
            StrikeRating     = 13,
            TurnSpeed        = 100,
            MaxHitPoints     = maxHp,
            CurrentHitPoints = hp,
            MaxMana          = mana,
            CurrentMana      = mana,
            MemorizedSpells  = spells.ToList()
        };

    private static Character MakeTarget(string name, int hp, int maxHp) =>
        new()
        {
            Name             = name,
            ClassId          = 8,
            Level            = 1,
            Strength         = 10,
            Dexterity        = 10,
            Intelligence     = 10,
            StrikeRating     = 10,
            TurnSpeed        = 1,
            MaxHitPoints     = maxHp,
            CurrentHitPoints = hp
        };

    private static Spell MakeHeal(string name = "Heal", int manaCost = 10) =>
        new()
        {
            Name          = name,
            School        = SpellSchool.Deity,
            DamageDie     = DieType.D8,
            DamageCount   = 2,
            DamageType    = DamageType.Healing,
            ManaCost      = manaCost,
            TurnMeterCost = 80,
            SpellLevel    = 2
        };

    private static Spell MakeDamage(string name = "Magic Missile", int manaCost = 15) =>
        new()
        {
            Name          = name,
            School        = SpellSchool.Stormcraft,
            DamageDie     = DieType.D8,
            DamageCount   = 2,
            DamageType    = DamageType.Holy,
            ManaCost      = manaCost,
            TurnMeterCost = 80,
            SpellLevel    = 2,
            AttackBonus   = 2
        };

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Simulate_HealerWithInjuredAlly_TurnStartTargetsAllyNotEnemy()
    {
        var (_, simulator) = CreateSimulator();
        var healer = MakeHealer("Sera", hp: 30, maxHp: 50, mana: 200,
            MakeHeal(),
            MakeDamage("Fireball"));
        var ally = MakeTarget("Ally", hp: 10, maxHp: 50);
        var enemy = MakeTarget("Gruk", hp: 50, maxHp: 50);

        var heroParty = new Party
        {
            Name = "Heroes",
            Members = new List<PartyMember>
            {
                new() { Character = healer, AttackSource = null },
                new() { Character = ally, AttackSource = new Weapon { Name = "Fists", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee } }
            }
        };
        var enemyParty = Party.Solo(enemy, new Weapon { Name = "Club", DamageDie = DieType.D6, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee });

        var result = simulator.Simulate(heroParty, enemyParty, maxTicks: 50);

        // Find the healer's TurnStart events
        var healerTurns = result.Log
            .Where(e => e.EventType == "TurnStart" && e.ActorName == "Sera")
            .ToList();

        Assert.NotEmpty(healerTurns);

        // Every turn where Sera casts Heal must target an ally (Ally or Sera herself)
        foreach (var turn in healerTurns)
        {
            if (turn.AttackSourceName == "Heal")
            {
                Assert.True(turn.TargetName == "Ally" || turn.TargetName == "Sera",
                    $"Heal targeted '{turn.TargetName}' — expected an ally (Ally or Sera)");
            }
        }
    }

    [Fact]
    public void Simulate_HealerWithFullHpParty_DoesNotWasteTurnsOnHeal()
    {
        var (_, simulator) = CreateSimulator();
        // Both healer and ally at full HP → AI should prefer Smite over Heal
        var healer = MakeHealer("Sera", hp: 50, maxHp: 50, mana: 200,
            MakeHeal(),
            MakeDamage("Fireball"));
        var ally = MakeTarget("Ally", hp: 50, maxHp: 50);
        var enemy = MakeTarget("Gruk", hp: 50, maxHp: 50);

        var heroParty = new Party
        {
            Name = "Heroes",
            Members = new List<PartyMember>
            {
                new() { Character = healer, AttackSource = null },
                new() { Character = ally, AttackSource = new Weapon { Name = "Fists", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee } }
            }
        };
        var enemyParty = Party.Solo(enemy, new Weapon { Name = "Club", DamageDie = DieType.D6, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee });

        var result = simulator.Simulate(heroParty, enemyParty, maxTicks: 50);

        var healTurns = result.Log
            .Where(e => e.EventType == "TurnStart" && e.ActorName == "Sera" && e.AttackSourceName == "Heal")
            .ToList();

        // With all allies at full HP, AI should not pick Heal at all
        Assert.Empty(healTurns);
    }

    [Fact]
    public void Simulate_HealerWithBothHealAndDamage_AtFullHp_PrefersDamage()
    {
        // Regression test: when a healer has both heal and damage spells
        // and all allies are at full HP, the AI should prefer Smite over Heal.
        var (_, simulator) = CreateSimulator();
        var healer = MakeHealer("Sera", hp: 50, maxHp: 50, mana: 200,
            MakeHeal(),
            MakeDamage("Fireball"));
        var enemy = MakeTarget("Gruk", hp: 50, maxHp: 50);

        var result = simulator.Simulate(Party.Solo(healer), Party.Solo(enemy), maxTicks: 30);

        var healTurns = result.Log
            .Where(e => e.EventType == "TurnStart" && e.ActorName == "Sera" && e.AttackSourceName == "Heal")
            .ToList();

        // With self at full HP and Smite available, AI should never pick Heal
        Assert.Empty(healTurns);
    }
}

// ── Queued-spell TM cost regression ────────────────────────────────────────────

public class CombatSimulatorQueuedSpellTests
{
    [Fact]
    public void Simulate_QueuedSpell_DeductsCorrectTmCostAfterTurn()
    {
        var dice = Substitute.For<IDiceService>();
        dice.Seed.Returns(42);
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        dice.Roll(DieType.D20).Returns(15);
        dice.Roll(DieType.D4).Returns(2);
        dice.Roll(DieType.D6).Returns(3);
        dice.Roll(DieType.D8).Returns(4);
        dice.Roll(DieType.D100).Returns(100);

        var simulator = new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);

        // Spell costing 130% of a turn — more than the character can pay immediately
        var slowSpell = new Spell
        {
            Name = "SlowCast",
            School = SpellSchool.Stormcraft,
            DamageDie = DieType.D8,
            DamageCount = 1,
            DamageType = DamageType.Fire,
            SpellLevel = 1,
            TurnMeterCost = 130,
            ManaCost = 10
        };

        // INT 10 → mod 0; Level 1 → 1% reduction; TM cost = max(10, 130 - 0 - 1) = 129
        var caster = new Character
        {
            Name = "Caster",
            ClassId = 5,
            Level = 1,
            Intelligence = 10,
            Dexterity = 10,
            TurnSpeed = 10,
            StrikeRating = 10,
            MaxHitPoints = 100,
            CurrentHitPoints = 100,
            MaxMana = 100,
            CurrentMana = 100,
            MemorizedSpells = [slowSpell]
        };

        var target = new Character
        {
            Name = "Dummy",
            ClassId = 8,
            Level = 1,
            Dexterity = 10,
            TurnSpeed = 1,
            StrikeRating = 10,
            MaxHitPoints = 500,
            CurrentHitPoints = 500
        };

        var result = simulator.Simulate(Party.Solo(caster), Party.Solo(target), 30);

        // Confirm the spell was queued (couldn't pay full cost immediately)
        var queued = result.Log.Where(e => e.EventType == "SpellQueued" && e.ActorName == "Caster").ToList();
        Assert.NotEmpty(queued);

        // Confirms charging occurred over multiple ticks
        var charging = result.Log.Where(e => e.EventType == "SpellCharging" && e.ActorName == "Caster").ToList();
        Assert.NotEmpty(charging);

        // The TurnEnd event shows the TM after deduction
        var turnEnd = result.Log
            .Where(e => e.EventType == "TurnEnd" && e.ActorName == "Caster")
            .OrderByDescending(e => e.Tick)
            .FirstOrDefault();

        Assert.NotNull(turnEnd);

        // The turn-end TM must reflect the actual spell cost, not the old hardcoded 100.
        // TM cost = max(10, 130 - (0 + 1 + 0)) = 129.
        // old AfterTurn(TM, 100) would give TM after = TM - 100 ≈ 30+.
        // correct AfterTurn(TM, 129) gives TM after = TM - 129 ≈ 1-2.
        var cost = turnEnd.TurnMeterBefore - turnEnd.TurnMeterAfter;
        Assert.Equal(129, cost);
    }
}

// ── Elemental afterburn DoT tests ───────────────────────────────────────────────

public class CombatSimulatorElementalDoTTests
{
    [Fact]
    public void Simulate_FireSpell_AppliesBurningDoTAndTickDamage()
    {
        var (dice, sim) = CreateSim();
        var caster = MakeCaster("Lyra", MakeFireSpell());
        var target = MakeTarget("Dummy", turnSpeed: 50);

        var result = sim.Simulate(Party.Solo(caster), Party.Solo(target), 50);

        // Burning must have been applied at least once
        var applied = result.Log
            .Where(e => e.EventType == "EffectApplied" && e.StatusEffectName == "Burning")
            .ToList();
        Assert.NotEmpty(applied);

        // Burning must tick damage on subsequent turns
        var ticks = result.Log
            .Where(e => e.EventType == "DoTTick" && e.StatusEffectName == "Burning")
            .ToList();
        Assert.NotEmpty(ticks);
        Assert.All(ticks, t => Assert.True(t.DamageDealt > 0,
            $"Burning DoT tick on tick {t.Tick} should deal > 0 damage, got {t.DamageDealt}"));

        // Messages must read e.g. "Dummy suffers 3 Burning damage."
        Assert.All(ticks, t => Assert.Matches(@"^\w+ suffers \d+ Burning damage\.$", t.Message));
    }

    [Fact]
    public void Simulate_NonElementalSpell_DoesNotApplyAfterburn()
    {
        var (dice, sim) = CreateSim();
        var caster = MakeCaster("Lyra", MakePlainSpell());
        var target = MakeTarget("Dummy");

        var result = sim.Simulate(Party.Solo(caster), Party.Solo(target), 30);

        var applied = result.Log
            .Where(e => e.EventType == "EffectApplied" && e.StatusEffectName is "Burning" or "Chilled" or "Shocked" or "Poisoned")
            .ToList();
        Assert.Empty(applied);
    }

    // ── helpers ─────────────────────────────────────────────────────────────────

    private static (IDiceService, ICombatSimulator) CreateSim()
    {
        var dice = Substitute.For<IDiceService>();
        dice.Seed.Returns(42);
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        dice.Roll(DieType.D20).Returns(15);    // hits
        dice.Roll(DieType.D4).Returns(2);
        dice.Roll(DieType.D6).Returns(3);
        dice.Roll(DieType.D8).Returns(4);
        dice.Roll(DieType.D100).Returns(50);   // passes 60% application chance, no resistance

        return (dice, new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice));
    }

    private static Character MakeCaster(string name, Spell spell) => new()
    {
        Name = name,
        ClassId = 5,
        Level = 5,
        Strength = 8,
        Dexterity = 14,
        Intelligence = 18,
        StrikeRating = 13,
        TurnSpeed = 100,
        MaxHitPoints = 30,
        CurrentHitPoints = 30,
        MaxMana = 300,
        CurrentMana = 300,
        MemorizedSpells = [spell]
    };

    private static Character MakeTarget(string name, int turnSpeed = 1) => new()
    {
        Name = name,
        ClassId = 8,
        Level = 1,
        Strength = 10,
        Dexterity = 10,
        Intelligence = 10,
        StrikeRating = 10,
        TurnSpeed = turnSpeed,
        MaxHitPoints = 100,
        CurrentHitPoints = 100
    };

    private static Spell MakeFireSpell() => new()
    {
        Name = "Fireball",
        School = SpellSchool.Stormcraft,
        DamageDie = DieType.D6,
        DamageCount = 3,
        DamageType = DamageType.Fire,
        AttackBonus = 2,
        SpellLevel = 3,
        TurnMeterCost = 90,
        ManaCost = 10,
        ElementalType = ElementalType.Fire
    };

    private static Spell MakePlainSpell() => new()
    {
        Name = "Magic Dart",
        School = SpellSchool.Stormcraft,
        DamageDie = DieType.D4,
        DamageCount = 2,
        DamageType = DamageType.Psychic,
        AttackBonus = 2,
        SpellLevel = 1,
        TurnMeterCost = 60,
        ManaCost = 5,
        ElementalType = ElementalType.None
    };
}






