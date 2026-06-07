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
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        var sut = CreateSut(dice);
        // Ally is at 38% HP (19/50), below the 40% heal threshold → heals
        var actor = MakeActor("Sera", hp: 20, maxHp: 50,
            MakeHeal(),
            MakeDamage());
        var allies = new[] { MakeAlly("Sera", hp: 19, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };
        var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);
        Assert.NotNull(result);
        Assert.True(result is Spell s && s.IsHealing,
            $"Expected a healing spell when ally is below 40% HP, got {result.Name}");
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
    public async Task ChooseAttack_InsufficientMana_FallsBackToDefaultAttack()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        // When the character has spells but lacks mana, fall back to the weapon
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
        Assert.Equal("Mace", result.Name);
    }

    [Fact]
    public async Task ChooseAttack_AllyBelowThreshold_AlwaysHeals()
    {
        var dice = Substitute.For<IDiceService>();
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        var sut = CreateSut(dice);

        // Ally is at 30% HP (15/50), below the 40% heal threshold → always heals
        var actor = MakeActor("Sera", hp: 30, maxHp: 50,
            MakeHeal(),
            MakeDamage());
        var allies = new[] { MakeAlly("Sera", hp: 15, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };
        var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);
        Assert.NotNull(result);
        Assert.True(result is Spell s && s.IsHealing,
            $"Expected a healing spell when ally is below 40% HP, got {result.Name}");
    }

    [Fact]
    public async Task ChooseAttack_AllyAboveThreshold_UsesDamageSpells()
    {
        var dice = Substitute.For<IDiceService>();
        dice.RollIndex(Arg.Any<int>()).Returns(0);
        var sut = CreateSut(dice);

        var actor = MakeActor("Sera", hp: 45, maxHp: 50,
            MakeHeal(),
            MakeDamage());

        var allies = new[] { MakeAlly("Sera", hp: 45, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        // Ally is at 90% HP, above 70% threshold → uses damage spells
        var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);

        Assert.NotNull(result);
        Assert.True(result is Spell s && !s.IsHealing,
            $"Expected a damage spell when ally is above 70% HP, got {result.Name}");
    }

    [Fact]
    public async Task ChooseAttack_OnlyHealSpellsAndFullHp_UsesWeapon()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        var actor = MakeActor("Sera", hp: 50, maxHp: 50,
            MakeHeal());

        var allies = new[] { MakeAlly("Sera", hp: 50, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        var weapon = new Weapon
        {
            Name = "Mace",
            DamageDie = DieType.D6,
            DamageCount = 1,
            DamageType = DamageType.Bludgeoning,
            AttackType = AttackType.Melee
        };

        // Only healing spells, allies at full HP → should use weapon
        var result = await sut.ChooseAttackAsync(actor, weapon, enemies, allies, 0, default);

        Assert.NotNull(result);
        Assert.Equal("Mace", result.Name);
    }

    [Fact]
    public async Task ChooseAttack_NullDefaultWithEquippedWeapon_UsesWeapon()
    {
        var dice = Substitute.For<IDiceService>();
        var sut = CreateSut(dice);

        var actor = MakeActor("Elira", hp: 50, maxHp: 50,
            MakeHeal("Cure Light Wounds"),
            MakeHeal("Cure Serious Wounds"),
            MakeHeal("Heal"),
            MakeHeal("Mass Heal"));

        actor.Equipment.RightHand = new Weapon
        {
            Name = "Mace",
            DamageDie = DieType.D6,
            DamageCount = 1,
            DamageType = DamageType.Bludgeoning,
            AttackType = AttackType.Melee
        };

        var allies = new[] { MakeAlly("Elira", hp: 50, maxHp: 50) };
        var enemies = new[] { MakeAlly("Gruk", hp: 50) };

        // defaultAttack is null (spellcaster), only healing spells, allies are full HP
        // should fall back to equipped weapon, not unarmed
        var result = await sut.ChooseAttackAsync(actor, null, enemies, allies, 0, default);

        Assert.NotNull(result);
        Assert.Equal("Mace", result.Name);
        Assert.IsNotType<UnarmedStrike>(result);
    }
}
