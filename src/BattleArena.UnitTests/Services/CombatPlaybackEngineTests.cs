namespace BattleArena.UnitTests.Services;

using BattleArena.Application.Models;
using BattleArena.Presentation;

public class CombatPlaybackEngineTests
{
    private sealed class SpyPresenter : ICombatPresenter
    {
        public VisualEventBus VisualEventBus { get; } = new();

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

        public void ShowCombatEventOverlay(string actor, string? target, string type) =>
            Calls.Add($"ShowOverlay:{actor}:{type}");

        public int GetEventDelayMs(string eventType) => 0;
        public void Wait(int ms) { }
        public void ClearAllPersistentEffects() => Calls.Add("ClearAllPersistentEffects");
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
            IsAlive = true,
            Race = "Human"
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
            new CharDisplayState { Name = "Hero", MaxHp = 100, Hp = 100, IsAlive = true, Race = "Human" },
            new CharDisplayState { Name = "Enemy", MaxHp = 80, Hp = 80, IsAlive = true, Race = "Orc" }
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

    // ── PlayTurnBased — TurnEnd ───────────────────────────────────────────────────

    [Fact]
    public void PlayTurnBased_TurnEnd_NeverRenderedAsEvent()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "TurnStart"),
            E(1, "Attack"),
            E(1, "TurnEnd")
        ]);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        Assert.DoesNotContain("ShowCombatEvent:TurnEnd", spy.Calls);
    }

    // ── PlayTurnBased — pending messages ─────────────────────────────────────────

    [Fact]
    public void PlayTurnBased_RoundStart_OutsideTurn_RenderedAtStartOfFirstTurn()
    {
        var state = MakeState("Hero");
        var spy = new SpyPresenter();
        var result = MakeResult([
            E(1, "RoundStart"),
            E(2, "TurnStart"),
            E(2, "Attack")
        ]);
        result.Log[0].RoundNumber = 1;

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        var rendered = spy.RenderedEvents.Select(e => e.EventType).ToList();
        Assert.Contains("RoundStart", rendered);
        Assert.True(rendered.IndexOf("RoundStart") < rendered.IndexOf("Attack"),
            "RoundStart must be rendered before Attack");
    }

    // ── PlayTurnBased — TM snapshot regression ───────────────────────────────────
    //
    // These tests guard the specific bug where TurnMeterGain was applied eagerly
    // in the main loop, so fast characters always showed TM=100 at RefreshScreen
    // because their TM had already re-accumulated by the time FlushTurn fired.
    // The fix: TM is only ever set via the TurnMeterSnapshot on TurnStart.

    [Fact]
    public void PlayTurnBased_TmGainWithinTurn_NeverAppliedToState()
    {
        // A TurnMeterGain that fires inside a turn must not dirty the display state.
        var heroSt = new CharDisplayState { Name = "Hero", MaxHp = 100, Hp = 100, IsAlive = true, Race = "Human" };
        var state  = new CombatDisplayState([heroSt], CombatLayout.From(["Hero"], ["Enemy"], false));

        var presenter = new RefreshCapturingPresenter();
        var log = new List<CombatLogEntry>
        {
            new() { Tick = 1, EventType = "TurnStart", ActorName = "Hero", TargetName = "Enemy",
                    TurnMeterSnapshot = new Dictionary<string, int> { ["Hero"] = 100 } },
            // TM re-accumulates mid-turn in the simulator — must be ignored by the display
            new() { Tick = 1, EventType = "TurnMeterGain", ActorName = "Hero", TurnMeterAfter = 15 },
        };

        CombatPlaybackEngine.PlayTurnBased(MakeResult(log), state, presenter);

        // Snapshot said 100; the TurnMeterGain of 15 must NOT have been applied
        Assert.Equal(100, state.TryGet("Hero")!.Tm);
    }

    [Fact]
    public void PlayTurnBased_RefreshScreen_ShowsSnapshotTm_NotGainedAfterSnapshot()
    {
        // Core regression: before the fix, Goblin would show TM=80 at RefreshScreen
        // because the TurnMeterGain (→80) was applied eagerly before FlushTurn.
        // Correct behaviour: RefreshScreen must see the snapshot value of 40.

        var heroSt = new CharDisplayState { Name = "Hero",   MaxHp = 100, Hp = 100, IsAlive = true, Race = "Human" };
        var goblin = new CharDisplayState { Name = "Goblin", MaxHp =  80, Hp =  80, IsAlive = true, Race = "Orc" };
        var state  = new CombatDisplayState([heroSt, goblin],
                         CombatLayout.From(["Hero"], ["Goblin"], false));

        var presenter = new RefreshCapturingPresenter();
        var log = new List<CombatLogEntry>
        {
            new() { Tick = 1, EventType = "TurnStart", ActorName = "Hero", TargetName = "Goblin",
                    TurnMeterSnapshot = new Dictionary<string, int> { ["Hero"] = 100, ["Goblin"] = 40 } },
            // Goblin's TM re-accumulates within the turn — must not reach RefreshScreen
            new() { Tick = 1, EventType = "TurnMeterGain", ActorName = "Goblin", TurnMeterAfter = 80 },
            new() { Tick = 1, EventType = "Attack", ActorName = "Hero", TargetName = "Goblin" }
        };

        CombatPlaybackEngine.PlayTurnBased(MakeResult(log), state, presenter);

        Assert.Single(presenter.TmSnapshotsAtRefresh);
        Assert.Equal(40,  presenter.TmSnapshotsAtRefresh[0]["Goblin"]);
        Assert.Equal(100, presenter.TmSnapshotsAtRefresh[0]["Hero"]);
    }

    [Fact]
    public void PlayTurnBased_SecondTurn_RefreshScreenShowsSecondTurnSnapshot()
    {
        // Each turn's RefreshScreen must use that turn's own TurnMeterSnapshot,
        // completely independent of the previous turn's values.

        var heroSt = new CharDisplayState { Name = "Hero",   MaxHp = 100, Hp = 100, IsAlive = true, Race = "Human" };
        var goblin = new CharDisplayState { Name = "Goblin", MaxHp =  80, Hp =  80, IsAlive = true, Race = "Orc" };
        var state  = new CombatDisplayState([heroSt, goblin],
                         CombatLayout.From(["Hero"], ["Goblin"], false));

        var presenter = new RefreshCapturingPresenter();
        var log = new List<CombatLogEntry>
        {
            new() { Tick = 1, EventType = "TurnStart", ActorName = "Hero",   TargetName = "Goblin",
                    TurnMeterSnapshot = new Dictionary<string, int> { ["Hero"] = 100, ["Goblin"] = 30 } },
            new() { Tick = 1, EventType = "Attack", ActorName = "Hero", TargetName = "Goblin" },
            new() { Tick = 2, EventType = "TurnStart", ActorName = "Goblin", TargetName = "Hero",
                    TurnMeterSnapshot = new Dictionary<string, int> { ["Hero"] = 18, ["Goblin"] = 100 } },
            new() { Tick = 2, EventType = "Attack", ActorName = "Goblin", TargetName = "Hero" }
        };

        CombatPlaybackEngine.PlayTurnBased(MakeResult(log), state, presenter);

        Assert.Equal(2, presenter.TmSnapshotsAtRefresh.Count);
        // Turn 1
        Assert.Equal(100, presenter.TmSnapshotsAtRefresh[0]["Hero"]);
        Assert.Equal(30,  presenter.TmSnapshotsAtRefresh[0]["Goblin"]);
        // Turn 2
        Assert.Equal(18,  presenter.TmSnapshotsAtRefresh[1]["Hero"]);
        Assert.Equal(100, presenter.TmSnapshotsAtRefresh[1]["Goblin"]);
    }

    // ── Persistent effect visual-event regression ───────────────────────────────
    //
    // Bug: EffectExpired entries have no TargetName, only ActorName. The ClearPersistent
    // handler in AvaloniaCombatPresenter now falls back to ActorName. This test verifies
    // the visual event is emitted at all — the consumer-side fix is in the presenter.
    [Fact]
    public void PlayTurnBased_PersistentEffectExpired_EmitClearPersistentVisualEvent()
    {
        var state = MakeState("Hero", "Enemy");
        var spy = new SpyPresenter();
        var visualEvents = new List<VisualEvent>();
        spy.VisualEventBus.NormalEventPublished += visualEvents.Add;

        var log = new List<CombatLogEntry>
        {
            new() { Tick = 1, EventType = "TurnStart", ActorName = "Hero", TargetName = "Enemy" },
            new() { Tick = 1, EventType = "EffectApplied", ActorName = "Hero",
                    StatusEffectName = "Burning" },
            new() { Tick = 1, EventType = "Attack", ActorName = "Hero", TargetName = "Enemy" },
            new() { Tick = 1, EventType = "EffectExpired", ActorName = "Hero",
                    StatusEffectName = "Burning" },
        };
        var result = MakeResult(log);

        CombatPlaybackEngine.PlayTurnBased(result, state, spy);

        var clear = visualEvents.FirstOrDefault(v => v.EffectName == "ClearPersistent");
        Assert.NotNull(clear);
        Assert.Equal("Hero", clear.ActorName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private sealed class CapturingPresenter : ICombatPresenter
    {
        public VisualEventBus VisualEventBus { get; } = new();
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
        public void ShowCombatEventOverlay(string actor, string? target, string type) { }
        public void ClearAllPersistentEffects() { }
    }

    /// <summary>
    /// Captures a snapshot of every character's TM value (int, not a reference)
    /// each time <see cref="RefreshScreen"/> is called.  GUI-agnostic — no Avalonia
    /// or Unity dependency, tests only the Presentation contract.
    /// </summary>
    private sealed class RefreshCapturingPresenter : ICombatPresenter
    {
        public VisualEventBus VisualEventBus { get; } = new();
        public List<Dictionary<string, int>> TmSnapshotsAtRefresh { get; } = [];

        public void RefreshScreen(CombatDisplayState state, int tick, string? active) =>
            TmSnapshotsAtRefresh.Add(
                state.All.ToDictionary(kv => kv.Key, kv => kv.Value.Tm));

        public void ShowInitialScreen(CombatDisplayState state, int tick) { }
        public void WaitForCombatStart() { }
        public void ShowTurnHeader(int turn, string actor, string? target, bool isHero) { }
        public void WaitForNextTurn(bool over) { }
        public void ShowQuietTicksSummary(int from, int to) { }
        public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state) { }
        public int GetEventDelayMs(string eventType) => 0;
        public void Wait(int ms) { }
        public void ShowCombatEventOverlay(string actor, string? target, string type) { }
        public void ClearAllPersistentEffects() { }
    }
}
