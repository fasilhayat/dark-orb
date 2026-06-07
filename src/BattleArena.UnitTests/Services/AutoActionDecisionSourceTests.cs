namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;
using Xunit;

public class AutoActionDecisionSourceTests
{
    private static AutoActionDecisionSource CreateSut(IDiceService dice) => new(dice);

    private static Character MakeActor(string name, int hp = 50, int maxHp = 50, params Spell[] spells) =>
        new()
        {
            Name = name,
            MaxHitPoints = maxHp,
            CurrentHitPoints = hp,
            MaxMana = 100,
            CurrentMana = 100,
            MemorizedSpells = spells.ToList()
        };

    private static Character MakeAlly(string name, int hp, int maxHp = 50) =>
        new() { Name = name, MaxHitPoints = maxHp, CurrentHitPoints = hp };

    private static Spell MakeHeal(string name = "Heal", int manaCost = 10) =>
        new()
        {
            Name = name,
            School = SpellSchool.Deity,
            DamageDie = DieType.D8,
            DamageCount = 2,
            DamageType = DamageType.Healing,
            ManaCost = manaCost,
            TurnMeterCost = 80
        };

    private static Spell MakeDamage(string name = "Magic Missile", int manaCost = 15) =>
        new()
        {
            Name = name,
            School = SpellSchool.Stormcraft,
            DamageDie = DieType.D8,
            DamageCount = 2,
            ManaCost = manaCost,
            TurnMeterCost = 80
        };

    [Fact]
    public async Task ChooseAttack_AllAlliesFullHp_ExcludesHealingSpells()
    {
        var dice = Substitute.For<IDiceService>();
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        var sut = CreateSut(dice);

        var actor = MakeActor("Sera", hp: 50, maxHp: 50,
            MakeHeal(),
            MakeDamage());

        var allies = new[] { MakeAlly("Sera", hp: 50, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        // Run 20 trials — with all allies at full HP, Heal should never be picked
        for (var i = 0; i < 20; i++)
        {
            var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);
            Assert.NotNull(result);
            Assert.False(result is Spell s && s.IsHealing,
                $"Healing spell was chosen on trial {i} when all allies were at full HP");
        }
    }

    [Fact]
    public async Task ChooseAttack_AllyBelowHalfHp_PrefersHealing()
    {
        var dice = Substitute.For<IDiceService>();
        // RollIndex(10) returns 0 → takes heal branch (0 < 7)
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        var sut = CreateSut(dice);

        var actor = MakeActor("Sera", hp: 20, maxHp: 50,
            MakeHeal(),
            MakeDamage());

        var allies = new[] { MakeAlly("Sera", hp: 20, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);

        Assert.NotNull(result);
        Assert.True(result is Spell s && s.IsHealing,
            $"Expected a healing spell when ally is below 50% HP, got {result.Name}");
    }

    [Fact]
    public async Task ChooseAttack_NoSpells_ReturnsDefaultAttack()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        var actor = MakeActor("Gruk", hp: 50);
        var weapon = new Weapon
        {
            Name = "Axe",
            DamageDie = DieType.D8,
            DamageCount = 1,
            DamageType = DamageType.Slashing,
            AttackType = AttackType.Melee
        };

        var result = await sut.ChooseAttackAsync(actor, weapon,
            Array.Empty<Character>(), Array.Empty<Character>(), 0, default);

        Assert.NotNull(result);
        Assert.Equal("Axe", result.Name);
    }

    [Fact]
    public async Task ChooseAttack_NoSpellsAndNoDefault_ReturnsUnarmed()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        var actor = MakeActor("Gruk", hp: 50);

        var result = await sut.ChooseAttackAsync(actor, null,
            Array.Empty<Character>(), Array.Empty<Character>(), 0, default);

        Assert.NotNull(result);
        Assert.Equal(UnarmedStrike.Default.Name, result.Name);
    }

    [Fact]
    public async Task ChooseAttack_InsufficientMana_FallsBackToUnarmed()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        // When the character has spells but lacks mana, the original behaviour
        // was to return UnarmedStrike (not defaultAttack). The health-aware logic
        // preserves this: no affordable spells → UnarmedStrike.
        var actor = MakeActor("Sera", hp: 50, maxHp: 50,
            MakeHeal(manaCost: 50),
            MakeDamage(manaCost: 50));
        actor.CurrentMana = 0;

        var weapon = new Weapon
        {
            Name = "Mace",
            DamageDie = DieType.D6,
            DamageCount = 1,
            DamageType = DamageType.Bludgeoning,
            AttackType = AttackType.Melee
        };

        var result = await sut.ChooseAttackAsync(actor, weapon,
            Array.Empty<Character>(), Array.Empty<Character>(), 0, default);

        Assert.NotNull(result);
        Assert.Equal(UnarmedStrike.Default.Name, result.Name);
    }

    [Fact]
    public async Task ChooseAttack_SomeInjured_MixesHealAndDamage()
    {
        var dice = Substitute.For<IDiceService>();
        // First call RollIndex to pick from the mixed pool (both spells)
        dice.RollIndex(Arg.Any<int>()).Returns(0, 1);
        var sut = CreateSut(dice);

        var actor = MakeActor("Sera", hp: 40, maxHp: 50,
            MakeHeal(),
            MakeDamage());

        var allies = new[] { MakeAlly("Sera", hp: 40, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        var first = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);
        var second = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);

        Assert.NotNull(first);
        Assert.NotNull(second);
        // RollIndex(2) returned 0 then 1 → should be two different spells
        Assert.NotEqual(first.Name, second.Name);
    }
}
