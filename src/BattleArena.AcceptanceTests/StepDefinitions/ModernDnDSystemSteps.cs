namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Models;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for ModernDnDSystem.feature.
/// Guards permanently against regression to the old AD&D THAC0 system.
/// Tests the SR level-gain formula (additive = modern D&D) and archetype ordering.
/// Statistical combat scenarios reuse CombatDistributionSteps — no extra wiring needed.
/// </summary>
[Binding]
public class ModernDnDSystemSteps
{
    private int _lastGain;
    private int _martialGain;
    private int _casterGain;

    [When(@"the SR gain for a (martial|caster|hybrid) character at level (\d+) is computed")]
    public void WhenSrGainForArchetypeAtLevelIsComputed(string archetype, int level)
    {
        var arch = archetype switch
        {
            "martial" => LevelProgression.ClassArchetype.Martial,
            "caster"  => LevelProgression.ClassArchetype.Caster,
            _         => LevelProgression.ClassArchetype.Hybrid
        };
        var gain = LevelProgression.SrLevelGain(level, arch);
        _lastGain = gain;
        if (arch == LevelProgression.ClassArchetype.Martial) _martialGain = gain;
        if (arch == LevelProgression.ClassArchetype.Caster)  _casterGain  = gain;
    }

    [Then(@"the SR gain should be (\d+)")]
    public void ThenTheSrGainShouldBe(int expected)
    {
        Assert.Equal(expected, _lastGain);
    }

    [Then(@"the martial SR gain should exceed the caster SR gain")]
    public void ThenMartialSrGainShouldExceedCasterSrGain()
    {
        Assert.True(
            _martialGain > _casterGain,
            $"Martial SR gain ({_martialGain}) must exceed Caster SR gain ({_casterGain}). " +
            "In the modern D&D 5e system, martial classes are stronger attackers than casters. " +
            "A THAC0-style regression likely reversed the class SR ordering.");
    }
}
