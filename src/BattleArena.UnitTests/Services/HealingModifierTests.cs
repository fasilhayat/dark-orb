namespace BattleArena.UnitTests.Services;

using Application.Modifiers;
using Core.Entities;
using Core.Entities.Enums;
using Core.Models;

public class HealingModifierTests
{
    private readonly HealingModifier _sut = new();

    private static CombatModifierContext MakeCtx(
        List<StatusEffect>? casterBuffs = null,
        List<StatusEffect>? targetDebuffs = null,
        bool isGroupHeal = false)
    {
        var school   = isGroupHeal ? SpellSchool.Deity : SpellSchool.Deity;
        var spellName = isGroupHeal ? "Mass Heal" : "Heal";
        var spell = new Spell
        {
            Name = spellName,
            School = school,
            DamageType = DamageType.Healing,
            ManaCost = 6,
            DamageDie = DieType.D8,
            DamageCount = 2,
            FlatDamageBonus = 4
        };
        var healer = new Character { Name = "Healer", Intelligence = 14, Strength = 10 };
        if (casterBuffs is not null)
            healer.ActiveStatusEffects.AddRange(casterBuffs);

        var target = new Character { Name = "Target", Strength = 10 };
        if (targetDebuffs is not null)
            target.ActiveStatusEffects.AddRange(targetDebuffs);

        return new CombatModifierContext
        {
            Attacker         = healer,
            Defender         = target,
            Source           = spell,
            Range            = EngagementRange.Melee,
            BaseAttackPower  = 10,
            BaseDefensePower = 8
        };
    }

    [Fact]
    public void NoEffects_DefaultHealing()
    {
        var ctx = MakeCtx();
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.HealingPowerDelta);
        Assert.Equal(1.0, ctx.HealingMultiplier);
    }

    [Fact]
    public void CasterBuff_BoostsHealing()
    {
        var ctx = MakeCtx(casterBuffs: new List<StatusEffect>
        {
            new()
            {
                Name = "Empowered Healing", Type = StatusEffectType.Buff,
                AttackPowerModifier = 6, Duration = 3, StackRule = StackRule.NoStack, Source = "Divine Favour"
            }
        });
        _sut.Apply(ctx);

        // AttackPowerModifier 6 / 2 = 3 extra healing
        Assert.Equal(3, ctx.HealingPowerDelta);
    }

    [Fact]
    public void TargetDebuff_ReducesHealing()
    {
        var ctx = MakeCtx(targetDebuffs: new List<StatusEffect>
        {
            new()
            {
                Name = "Wounded", Type = StatusEffectType.Debuff,
                DefensePowerModifier = -3, Duration = 3, StackRule = StackRule.NoStack, Source = "Curse"
            }
        });
        _sut.Apply(ctx);

        // 0.8 multiplier from target debuff
        Assert.Equal(0.8, ctx.HealingMultiplier);
    }

    [Fact]
    public void GroupHeal_HasReducedPotency()
    {
        var ctx = MakeCtx(isGroupHeal: true);
        _sut.Apply(ctx);

        // 0.6 multiplier from group heal
        Assert.Equal(0.6, ctx.HealingMultiplier);
    }

    [Fact]
    public void GroupHealWithBuffAndDebuff_AllMultipliersStack()
    {
        var ctx = MakeCtx(
            casterBuffs: new List<StatusEffect>
            {
                new()
                {
                    Name = "Empowered Healing", Type = StatusEffectType.Buff,
                    AttackPowerModifier = 6, Duration = 3, StackRule = StackRule.NoStack, Source = "Divine Favour"
                }
            },
            targetDebuffs: new List<StatusEffect>
            {
                new()
                {
                    Name = "Wounded", Type = StatusEffectType.Debuff,
                    DefensePowerModifier = -3, Duration = 3, StackRule = StackRule.NoStack, Source = "Curse"
                }
            },
            isGroupHeal: true);

        _sut.Apply(ctx);

        Assert.Equal(3, ctx.HealingPowerDelta);
        Assert.Equal(0.48, ctx.HealingMultiplier); // 0.8 * 0.6 = 0.48
    }

    [Fact]
    public void Priority_IsBaseBand()
    {
        Assert.Equal(10, _sut.Priority);
    }
}
