namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for the StatusEffects.feature.
/// Exercises StatusEffectService directly to verify stacking rules and
/// duration ticking behaviour described in §10 and §11.
/// </summary>
[Binding]
public class StatusEffectSteps
{
    private readonly StatusEffectService _sut = new();
    private Character _target = new();

    [Given(@"a fresh character with no status effects")]
    public void GivenAFreshCharacterWithNoStatusEffects()
    {
        _target = new Character();
    }

    [Given(@"a fresh character with active effect ""([^""]+)"" lasting (\d+) turns?")]
    public void GivenAFreshCharacterWithActiveEffectLastingTurns(string name, int duration)
    {
        _target = new Character();
        _target.ActiveStatusEffects.Add(new StatusEffect { Name = name, Duration = duration });
    }

    [Given(@"the character also has active effect ""([^""]+)"" lasting (\d+) turns?")]
    public void GivenTheCharacterAlsoHasActiveEffectLastingTurns(string name, int duration)
    {
        _target.ActiveStatusEffects.Add(new StatusEffect { Name = name, Duration = duration });
    }

    [When(@"the effect ""([^""]+)"" with (NoStack|Stack|HighestWins) rule and magnitude (\d+) from ""([^""]+)"" is applied")]
    public void WhenTheEffectWithRuleAndMagnitudeFromIsApplied(string name, string rule, int magnitude, string source)
    {
        _sut.Apply(_target, new StatusEffect
        {
            Name = name,
            StackRule = Enum.Parse<StackRule>(rule),
            Magnitude = magnitude,
            Source = source
        });
    }

    [When(@"status effects tick once")]
    public void WhenStatusEffectsTickOnce()
    {
        _sut.TickAll(_target);
    }

    [Then(@"the character should have (\d+) active status effects?")]
    public void ThenTheCharacterShouldHaveActiveStatusEffects(int expected)
    {
        Assert.Equal(expected, _target.ActiveStatusEffects.Count);
    }

    [Then(@"the active effect ""([^""]+)"" should have magnitude (\d+)")]
    public void ThenTheActiveEffectShouldHaveMagnitude(string name, int expected)
    {
        var effect = _target.ActiveStatusEffects.FirstOrDefault(e => e.Name == name);
        Assert.NotNull(effect);
        Assert.Equal(expected, effect.Magnitude);
    }

    [Then(@"the active effect ""([^""]+)"" should have (\d+) turns? remaining")]
    public void ThenTheActiveEffectShouldHaveTurnsRemaining(string name, int expected)
    {
        var effect = _target.ActiveStatusEffects.FirstOrDefault(e => e.Name == name);
        Assert.NotNull(effect);
        Assert.Equal(expected, effect.Duration);
    }

    [Then(@"the active effect ""([^""]+)"" should have expired")]
    public void ThenTheActiveEffectShouldHaveExpired(string name)
    {
        Assert.DoesNotContain(_target.ActiveStatusEffects, e => e.Name == name);
    }
}
