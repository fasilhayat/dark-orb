namespace BattleArena.UnitTests.Services;

using BattleArena.Application.Models;
using BattleArena.Presentation;

public class CombatPlaybackEngineTests
{
    private sealed class SpyPresenter : ICombatPresenter
    {
        public List<string> Calls { get; } = [];
        public List<CombatLogEntry> RenderedEvents { get; } = [];

        public void ShowInitialScreen(CombatDisplayState state, int tick) => Calls.Add("ShowInitialScreen");
        public void WaitForCombatStart() => Calls.Add("WaitForCombatStart");
        public void RefreshScreen(CombatDisplayState state, int tick, string? active) => Calls.Add("RefreshScreen");
        public void ShowTurnHeader(int turn, string actor, string? target, bool isHero) => Calls.Add($"ShowTurnHeader:{actor}");
        public void WaitForNextTurn(bool over) => Calls.Add($"WaitForNextTurn:{over}");
        public void ShowQuietTicksSummary(int from, int to) => Calls.Add("ShowQuietTicksSummary");

        public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state)
        {
            Calls.Add($"ShowCombatEvent:{entry.EventType}");
            RenderedEvents.Add(entry);
        }

        public int GetEventDelayMs(string eventType) => 0;
        public void Wait(int ms) { }
    }

    private static CombatResult MakeResult(IEnumerable<CombatLogEntry> log) => new()
    {
        Log = log.ToList()
    };

    private static CombatDisplayState MakeState(params string[] heroNames) =>
        new(heroNames.Select(n => new CharDisplayState
        {
            Name = n,
            MaxHp = 100,
            Hp = 100,
            IsHero = true,
            IsAlive = true
        }),
        CombatLayout.From(heroNames, ["Enemy"], false));

    private static CombatLogEntry E(int tick, string type, string actor = "Hero") =>
        new()
        {
            Tick = tick,
            EventType = type,
            ActorName = actor,
            ActiveActorName = actor,
            TargetName = "Enemy"
        };

    [Fact]
    public void PreSeedTurnMeters_AppliesTmGainsBeforeFirstTurnStart()
    {
        var state = MakeState("Hero");
        var result = MakeResult([
            E(1, "TurnMeterGain"),
            E(1, "TurnMeterGain"),
            E(2, "TurnStart")
        ]);

        result.Log[0].TurnMeterAfter = 50;
        result.Log[1].TurnMeterAfter = 100;

        CombatPlaybackEngine.PreSeedTurnMeters(result, state);

        Assert.Equal(100, state.TryGet("Hero")!.Tm);
    }

    [Fact]
    public void PreSeedTurnMeters_StopsAtFirstTurnStart()
    {
        var state = MakeState("Hero");
        var result = MakeResult([
            E(1, "TurnMeterGain"),
            E(2, "TurnStart"),
            E(3, "TurnMeterGain")
        ]);
        result.Log[0].TurnMeterAfter = 40;
        result.Log[2].TurnMeterAfter = 99;

        CombatPlaybackEngine.PreSeedTurnMeters(result, state);

        Assert.Equal(40, state.TryGet("Hero")!.Tm);
    }

    [Fact]
    public void PlayTurnBased_ApiCall_OnlyRenderedWhenInTurn()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "ApiCall"),
            E(2, "TurnStart"),
            E(2, "Attack"),
            E(2, "ApiCall"),
            E(2, "TurnEnd")
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        var renderedTypes = spy.RenderedEvents.Select(e => e.EventType).ToList();
        Assert.Equal(1, renderedTypes.Count(t => t == "ApiCall"));
        var apiIdx = renderedTypes.IndexOf("ApiCall");
        var attackIdx = renderedTypes.IndexOf("Attack");
        Assert.True(apiIdx > attackIdx, "ApiCall must appear after Attack in rendered events");
    }

    [Fact]
    public void PlayTurnBased_TurnMeterGain_NeverRenderedAsEvent()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnMeterGain"),
            E(2, "TurnStart"),
            E(2, "TurnMeterGain"),
            E(2, "Attack")
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        Assert.DoesNotContain("ShowCombatEvent:TurnMeterGain", spy.Calls);
    }

    [Fact]
    public void PlayTurnBased_ShowInitialScreenCalledBeforeAnyTurn()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnStart"),
            E(1, "Attack")
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        var showInit = spy.Calls.IndexOf("ShowInitialScreen");
        var showTurn = spy.Calls.IndexOf("ShowTurnHeader:Hero");
        Assert.True(showInit < showTurn);
    }

    [Fact]
    public void PlayTurnBased_RefreshScreenCalledBeforeEachTurnHeader()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnStart"),
            E(1, "Attack"),
            E(2, "TurnStart"),
            E(2, "Attack")
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        var count = 0;
        for (int i = 1; i < spy.Calls.Count; i++)
        {
            if (spy.Calls[i].StartsWith("ShowTurnHeader", StringComparison.Ordinal))
            {
                Assert.Equal("RefreshScreen", spy.Calls[i - 1]);
                count++;
            }
        }

        Assert.Equal(2, count);
    }

    [Fact]
    public void PlayTurnBased_StateIsAppliedBeforeEventIsRendered()
    {
        var state = new CombatDisplayState(
        [
            new CharDisplayState { Name = "Hero", MaxHp = 100, Hp = 100, IsHero = true, IsAlive = true },
            new CharDisplayState { Name = "Enemy", MaxHp = 80, Hp = 80, IsHero = false, IsAlive = true }
        ],
        CombatLayout.From(["Hero"], ["Enemy"], false));

        int? hpSeenByPresenter = null;
        var presenter = new CapturingPresenter(entry =>
        {
            if (entry.EventType == "Damage")
                hpSeenByPresenter = state.TryGet("Enemy")!.Hp;
        });

        var result = MakeResult([
            E(1, "TurnStart"),
            new CombatLogEntry
            {
                Tick = 1,
                EventType = "Damage",
                ActorName = "Enemy",
                TargetHpAfter = 55,
                TargetName = "Enemy",
                ActiveActorName = "Hero"
            }
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, presenter);

        Assert.Equal(55, hpSeenByPresenter);
    }

    [Fact]
    public void PlayRealTime_QuietTickSummary_NotShownForSingleQuietTick()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnMeterGain"),
            E(2, "TurnStart"),
            E(2, "Attack")
        ]);
        result.Log[0].TurnMeterAfter = 40;

        CombatPlaybackEngine.PlayRealTime(result, state, spy);

        Assert.DoesNotContain("ShowQuietTicksSummary", spy.Calls);
    }

    [Fact]
    public void PlayRealTime_QuietTickSummary_ShownForMultipleQuietTicks()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnMeterGain"),
            E(2, "TurnMeterGain"),
            E(3, "TurnMeterGain"),
            E(4, "TurnStart"),
            E(4, "Attack")
        ]);

        foreach (var entry in result.Log)
            entry.TurnMeterAfter = 30;

        CombatPlaybackEngine.PlayRealTime(result, state, spy);

        Assert.Contains("ShowQuietTicksSummary", spy.Calls);
    }

    private sealed class CapturingPresenter : ICombatPresenter
    {
        private readonly Action<CombatLogEntry> _onEvent;

        public CapturingPresenter(Action<CombatLogEntry> onEvent) => _onEvent = onEvent;

        public void ShowInitialScreen(CombatDisplayState state, int tick) { }
        public void WaitForCombatStart() { }
        public void RefreshScreen(CombatDisplayState state, int tick, string? activeActorName) { }
        public void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero) { }
        public void WaitForNextTurn(bool combatOver) { }
        public void ShowQuietTicksSummary(int fromTick, int toTick) { }
        public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state) => _onEvent(entry);
        public int GetEventDelayMs(string eventType) => 0;
        public void Wait(int milliseconds) { }
    }
}
