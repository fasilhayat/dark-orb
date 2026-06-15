namespace BattleArena.UnitTests.Services;

using System.Text.RegularExpressions;

/// <summary>
/// Guards two invariants that are easy to accidentally break:
///   1. All required combat event types are wired up in AvaloniaCombatPresenter.BuildRows,
///      so no event is silently dropped during playback.
///   2. ApiCall events are ordered BEFORE the Attack event they produced,
///      so dice rolls appear before the resolved outcome on screen.
/// </summary>
public class DisplayContractTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.GetFiles("BattleArena.sln").Any())
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Cannot locate BattleArena.sln");
    }

    // ── 1. All required event types are handled ────────────────────────────────

    [Theory]
    [InlineData("RoundStart")]
    [InlineData("RoundEnd")]
    [InlineData("TurnStart")]
    [InlineData("TurnEnd")]
    [InlineData("Attack")]
    [InlineData("Damage")]
    [InlineData("SkippedTurn")]
    [InlineData("EffectApplied")]
    [InlineData("EffectResisted")]
    [InlineData("EffectExpired")]
    [InlineData("DoTTick")]
    [InlineData("FumblePenalty")]
    [InlineData("Death")]
    [InlineData("KnockedOut")]
    [InlineData("ApiCall")]     // dice-roll API calls — must appear BEFORE Attack
    [InlineData("ManaRegen")]
    [InlineData("ManaDeduct")]
    [InlineData("PetSummoned")]
    [InlineData("PetExpired")]
    public void AvaloniaCombatPresenter_RegistersHandler_For(string eventType)
    {
        var presenterFile = Path.Combine(
            FindRepoRoot(), "BattleArena.Gui", "Presenters", "AvaloniaCombatPresenter.cs");

        var source = File.ReadAllText(presenterFile);
        var pattern = new Regex($@"""{eventType}""\s*=>");

        Assert.True(pattern.IsMatch(source),
            $"No handler registered for '{eventType}' in AvaloniaCombatPresenter.BuildRows. " +
            $"Add \"{eventType}\" => to the switch expression.");
    }

    // ── 2. ApiCall events are ordered before Attack in the merged log ──────────

    [Fact]
    public void CombatLogMerger_ApiCall_PrecedesAttack_InSameTick()
    {
        var log = new List<BattleArena.Application.Models.CombatLogEntry>
        {
            new() { Tick = 5, EventType = "TurnStart" },
            new() { Tick = 5, EventType = "Attack" },
            new() { Tick = 5, EventType = "Damage" },
            new() { Tick = 5, EventType = "KnockedOut" }
        };
        var diceLog = new List<BattleArena.Application.Models.CombatLogEntry>
        {
            new() { Tick = 5, EventType = "ApiCall", Message = "GET /v1/roll/D20 → 17" },
            new() { Tick = 5, EventType = "ApiCall", Message = "GET /v1/roll/D6  →  5" }
        };

        var merged = BattleArena.Presentation.CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var apiIdx    = types.IndexOf("ApiCall");
        var attackIdx = types.IndexOf("Attack");
        var koIdx     = types.IndexOf("KnockedOut");

        Assert.True(apiIdx < attackIdx,
            $"ApiCall (pos {apiIdx}) must appear BEFORE Attack (pos {attackIdx})");
        Assert.True(attackIdx < koIdx,
            $"Attack (pos {attackIdx}) must appear BEFORE KnockedOut (pos {koIdx})");
    }
}
