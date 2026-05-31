namespace BattleArena.UnitTests.Services;

using Application.Modifiers;
using Core.Entities;
using Core.Entities.Enums;
using Core.Models;

public class RangeModifierTests
{
    private readonly RangeModifier _sut = new();

    private static CombatModifierContext MakeCtx(AttackType attackType, EngagementRange range)
    {
        var source = new Weapon { Name = "Test", AttackType = attackType, DamageDie = DieType.D6 };
        var character = new Character { Strength = 10 };
        return new CombatModifierContext
        {
            Attacker         = character,
            Defender         = character,
            Source           = source,
            Range            = range,
            BaseAttackPower  = 10,
            BaseDefensePower = 8
        };
    }

    [Fact]
    public void Apply_RangedAtMeleeRange_ReducesAttackPowerByTwo()
    {
        var ctx = MakeCtx(AttackType.Ranged, EngagementRange.Melee);
        _sut.Apply(ctx);
        Assert.Equal(-2, ctx.AttackPowerDelta);
        Assert.Equal(0,  ctx.DefensePowerDelta);
    }

    [Theory]
    [InlineData(EngagementRange.Short)]
    [InlineData(EngagementRange.Long)]
    public void Apply_RangedAtDistance_ReducesDefensePowerByOne(EngagementRange range)
    {
        var ctx = MakeCtx(AttackType.Ranged, range);
        _sut.Apply(ctx);
        Assert.Equal(0,  ctx.AttackPowerDelta);
        Assert.Equal(-1, ctx.DefensePowerDelta);
    }

    [Theory]
    [InlineData(EngagementRange.Melee)]
    [InlineData(EngagementRange.Short)]
    [InlineData(EngagementRange.Long)]
    public void Apply_MeleeWeapon_NoModifiers(EngagementRange range)
    {
        var ctx = MakeCtx(AttackType.Melee, range);
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.AttackPowerDelta);
        Assert.Equal(0, ctx.DefensePowerDelta);
    }

    [Theory]
    [InlineData(EngagementRange.Melee)]
    [InlineData(EngagementRange.Short)]
    [InlineData(EngagementRange.Long)]
    public void Apply_SpellAttack_NoModifiers(EngagementRange range)
    {
        var ctx = MakeCtx(AttackType.Spell, range);
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.AttackPowerDelta);
        Assert.Equal(0, ctx.DefensePowerDelta);
    }

    [Fact]
    public void Apply_MultipleModifiersAccumulate_InPriorityOrder()
    {
        // Simulate two modifiers both adjusting AP — a future-proof integration test
        var second = new RangeModifier(); // same modifier applied twice (contrived but tests accumulation)
        var ctx = MakeCtx(AttackType.Ranged, EngagementRange.Melee);
        _sut.Apply(ctx);
        second.Apply(ctx);
        Assert.Equal(-4, ctx.AttackPowerDelta); // -2 + -2
    }

    [Fact]
    public void Priority_IsLowerThanEnvironmentalBand()
    {
        Assert.Equal(10, _sut.Priority); // positional band = 10; environmental = 20
    }
}
