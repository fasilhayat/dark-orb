namespace BattleArena.Demo;

using BattleArena.Presentation;

static partial class Demo
{
    private static double PacingMultiplier { get; set; } = 1.0;

    private static void Paced(int milliseconds) => Thread.Sleep((int)(milliseconds * PacingMultiplier));

    private static ConsoleCombatPresenter CreateCombatPresenter() =>
        new(DisplayConfig, MaxHp, DrawCombatScreen, Paced);

    internal static void PlayTurnBased()
    {
        var state = BuildDisplayStates();
        CombatPlaybackEngine.PlayTurnBased(Result, state, CreateCombatPresenter(), EnsureSummonedPetDisplayState);
    }

    internal static void PlayRealTime()
    {
        var state = BuildDisplayStates();
        CombatPlaybackEngine.PlayRealTime(Result, state, CreateCombatPresenter(), EnsureSummonedPetDisplayState);
    }
}
