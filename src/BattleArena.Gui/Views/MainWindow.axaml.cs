using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Modifiers;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Gui.Data;
using BattleArena.Gui.Presenters;
using BattleArena.Gui.ViewModels;
using BattleArena.Presentation;

namespace BattleArena.Gui.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm = new();
    private readonly GuiDisplayConfig _displayConfig;

    private CancellationTokenSource? _cts;
    private ManualResetEventSlim? _waitForNext;
    private AvaloniaCombatPresenter? _presenter;
    private Character? _fighter1;
    private Character? _fighter2;
    private bool _useApi;
    private BattleArenaApiClient? _apiClient;
    private List<Character> _apiRoster = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _displayConfig = GuiDisplayConfig.Load();
        _vm.CombatLog.CollectionChanged += (_, _) =>
        {
            if (CombatLogListBox.ItemCount > 0)
                CombatLogListBox.ScrollIntoView(CombatLogListBox.Items[^1]!);
        };
        HeroListBox.ItemsSource = Roster.AllHeroes;
        DuelButton.IsEnabled = false;
        SelectionHint.Text = "Select Fighter 1";

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
                var apiSection = json.RootElement.GetProperty("BattleArenaApi");
                var url = apiSection.GetProperty("Url").GetString() ?? "http://localhost:5000";
                var key = apiSection.GetProperty("ApiKey").GetString() ?? "";
                _apiClient = new BattleArenaApiClient(url, key);
            }
        }
        catch
        {
            _apiClient = null;
        }

        if (_apiClient is not null)
            _ = CheckApiReachabilityAsync();
    }

    private async Task CheckApiReachabilityAsync()
    {
        var reachable = await _apiClient!.HealthCheckAsync();
        _vm.IsApiReachable = reachable;
    }

    private void OnDuelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Scenario = "Duel";
        DuelButton.IsEnabled = false;
        ClashButton.IsEnabled = true;
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        SelectionHint.Text = "Select Fighter 1";
    }

    private void OnClashClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Scenario = "Party";
        DuelButton.IsEnabled = true;
        ClashButton.IsEnabled = false;
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        SelectionHint.Text = "Clash mode — pick two fighters";
    }

    private void OnHeroSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (HeroListBox.SelectedItem is not Character hero) return;

        if (_fighter1?.Name == hero.Name)
        {
            _fighter1 = null;
            _vm.Fighter1Name = "";
            SelectionHint.Text = "Select Fighter 1";
        }
        else if (_fighter2?.Name == hero.Name)
        {
            _fighter2 = null;
            _vm.Fighter2Name = "";
            SelectionHint.Text = "Select Fighter 2";
        }
        else if (_fighter1 is null)
        {
            _fighter1 = hero;
            _vm.Fighter1Name = hero.Name;
            SelectionHint.Text = _fighter2 is null ? "Select Fighter 2" : "Both fighters selected!";
        }
        else if (_fighter2 is null)
        {
            _fighter2 = hero;
            _vm.Fighter2Name = hero.Name;
            SelectionHint.Text = "Both fighters selected!";
        }
        else
        {
            _fighter2 = hero;
            _vm.Fighter2Name = hero.Name;
        }

        HeroListBox.SelectedItem = null;
    }

    private async void OnFightClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_fighter1 is null || _fighter2 is null) return;
        await StartCombat();
    }

    private async void OnNewCombatClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NewCombatButton.IsVisible = false;
        if (_fighter1 is null || _fighter2 is null) return;
        await StartCombat();
    }

    private async Task StartCombat()
    {
        _vm.Phase = "Combat";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.ActiveActorName = "";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;

        ResetCombatant(_fighter1!);
        ResetCombatant(_fighter2!);

        var party1 = Party.Solo(_fighter1!, Roster.GetAttackSource(_fighter1!));
        var party2 = Party.Solo(_fighter2!, Roster.GetAttackSource(_fighter2!));

        await RunCombat(party1, party2);
    }

    private async void OnStartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // No longer used — replaced by OnFightClick
    }

    private void OnBackToSetupClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _waitForNext?.Dispose();
        _cts = null;
        _waitForNext = null;

        _vm.Phase = "Setup";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.IsRunning = false;
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        NextButton.IsEnabled = false;
        AutoButton.IsEnabled = false;
        NewCombatButton.IsVisible = false;
    }

    private void OnDemoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "Setup";
        _vm.Mode = "TurnBased";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        _fighter1 = null;
        _fighter2 = null;
        DuelButton.IsEnabled = false;
        ClashButton.IsEnabled = true;
        TurnBasedButton.IsEnabled = false;
        AutoModeButton.IsEnabled = true;
        SelectionHint.Text = "Select Fighter 1";
        HeroListBox.SelectedItem = null;
    }

    private void OnApiModeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "ApiMenu";
    }

    private void OnCreateCharClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "CharCreation";
        _vm.CharName = "";
        _vm.SelectedRaceIndex = -1;
        _vm.SelectedClassIndex = -1;
        _vm.CharStr = 0;
        _vm.CharStrExceptional = 0;
        _vm.CharDex = 0;
        _vm.CharSta = 0;
        _vm.CharInt = 0;
        _vm.CharWis = 0;
        _vm.CharCha = 0;
        ExcStrLabel.Text = "";
        UpdateCreateButton();
    }


    private void OnBackToMainClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "MainMenu";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
    }

    private async void OnVsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_apiClient is null) return;
        try
        {
            _apiRoster = await _apiClient.GetCharactersAsync();
            if (_apiRoster.Count == 0) return;
        }
        catch
        {
            return;
        }
        HeroListBox.ItemsSource = _apiRoster;
        _vm.IsApiMode = true;
        _useApi = true;
        _vm.Scenario = "Duel";
        _vm.Phase = "Setup";
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        DuelButton.IsEnabled = false;
        ClashButton.IsEnabled = true;
        SelectionHint.Text = "Select Fighter 1";
    }

    private async void OnPartyVsPartyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_apiClient is null) return;
        try
        {
            _apiRoster = await _apiClient.GetCharactersAsync();
            if (_apiRoster.Count == 0) return;
        }
        catch
        {
            return;
        }
        HeroListBox.ItemsSource = _apiRoster;
        _vm.IsApiMode = true;
        _useApi = true;
        _vm.Scenario = "Party";
        _vm.Phase = "Setup";
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        DuelButton.IsEnabled = true;
        ClashButton.IsEnabled = false;
        SelectionHint.Text = "Clash mode — pick two fighters";
    }

    // ── Character Creation Handlers ───────────────────────────

    private static readonly Random _charRng = new();
    private static readonly string[] _warriorClasses = ["Barbarian", "Fighter", "Paladin", "Knight", "Ranger"];

    // Race: (strength bonus, max strength after bonus, can have exceptional 18/xx)
    private static readonly Dictionary<string, (int Bonus, int Max, bool Exceptional)> RaceStrData = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Human"]    = ( 1, 18, true),
        ["Half-Elf"] = ( 0, 18, true),
        ["Elf"]      = ( 0, 18, false),
        ["Dwarf"]    = ( 2, 19, false),
        ["Lizard"]   = ( 2, 19, false),
        ["Kobold"]   = ( 0, 18, false),
        ["Orc"]      = ( 3, 20, false),
        ["Ogre"]     = ( 3, 20, false),
        ["Gladefolk"]= ( 0, 18, false),
    };

    private void OnRollStatsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var rd = RaceStrData.GetValueOrDefault(_vm.SelectedRace, (0, 18, false));
        var (strBonus, strMax, canExceptional) = rd;

        var baseStr = Roll4d6DropLowest();
        _vm.CharStr = Math.Min(baseStr + strBonus, strMax);
        _vm.CharDex = Roll4d6DropLowest();
        _vm.CharSta = Roll4d6DropLowest();
        _vm.CharInt = Roll4d6DropLowest();
        _vm.CharWis = Roll4d6DropLowest();
        _vm.CharCha = Roll4d6DropLowest();
        RollExceptionalStrength(_vm.CharStr, canExceptional);
        UpdateCreateButton();
    }

    private void RollExceptionalStrength(int finalStr, bool raceCanExceptional)
    {
        var cls = _vm.SelectedClass;
        if (finalStr == 18 && raceCanExceptional && _warriorClasses.Contains(cls))
        {
            _vm.CharStrExceptional = _charRng.Next(1, 101);
            ExcStrLabel.Text = $"Exceptional strength: 18/{FormatExceptional(_vm.CharStrExceptional)}";
        }
        else
        {
            _vm.CharStrExceptional = 0;
            ExcStrLabel.Text = "";
        }
    }

    private static string FormatExceptional(int pct) => pct == 100 ? "00" : pct.ToString("00");

    private static int Roll4d6DropLowest()
    {
        var rolls = new int[4];
        for (var i = 0; i < 4; i++)
            rolls[i] = _charRng.Next(1, 7);
        Array.Sort(rolls);
        return rolls[1] + rolls[2] + rolls[3];
    }

    private void OnCharRaceChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        UpdateCreateButton();
    }

    private void OnCharClassChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var rd = RaceStrData.GetValueOrDefault(_vm.SelectedRace, (0, 18, false));
        if (_vm.CharStr == 18)
            RollExceptionalStrength(_vm.CharStr, rd.Item3);
        UpdateCreateButton();
    }

    private void OnCharCreateClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Character creation complete — not saving yet.
    }

    private void OnCharNameChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        UpdateCreateButton();
    }

    private void OnCharBackClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "ApiMenu";
    }

    private void UpdateCreateButton()
    {
        CreateCharButton2.IsEnabled =
            !string.IsNullOrWhiteSpace(_vm.CharName) &&
            _vm.SelectedRaceIndex >= 0 &&
            _vm.SelectedClassIndex >= 0 &&
            _vm.CharStr > 0;
    }

    private void OnTurnBasedClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Mode = "TurnBased";
        TurnBasedButton.IsEnabled = false;
        AutoModeButton.IsEnabled = true;
    }

    private void OnAutoModeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Mode = "Auto";
        TurnBasedButton.IsEnabled = true;
        AutoModeButton.IsEnabled = false;
    }

    private void OnBackToMainFromSetupClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _waitForNext?.Dispose();
        _cts = null;
        _waitForNext = null;

        _vm.Phase = "MainMenu";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.IsRunning = false;
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;

        if (_useApi)
        {
            _useApi = false;
            _vm.IsApiMode = false;
            HeroListBox.ItemsSource = Roster.AllHeroes;
        }
    }

    private async Task RunCombat(Party party1, Party party2)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _waitForNext?.Dispose();

        _cts = new CancellationTokenSource();
        _waitForNext = new ManualResetEventSlim(false);

        _presenter = null;
        _vm.IsRunning = true;
        NextButton.IsEnabled = true;
        AutoButton.IsEnabled = true;
        AutoButton.Content = "Auto Play";
        NewCombatButton.IsVisible = false;

        foreach (var pm in party1.Members)
        {
            _vm.Heroes.Add(new CharCardViewModel
            {
                Name = pm.Character.Name,
                MaxHp = pm.Character.MaxHitPoints,
                MaxMana = pm.Character.MaxMana,
                IsHero = true,
                Level = pm.Character.Level,
                ClassName = pm.Character.ClassName,
                Sex = pm.Character.Sex,
                Race = pm.Character.Race?.Name ?? "",
                Hp = pm.Character.MaxHitPoints,
                Mana = pm.Character.CurrentMana,
                CurrentWeapon = pm.AttackSource?.Name ?? ""
            });
        }
        foreach (var pm in party2.Members)
        {
            _vm.Enemies.Add(new CharCardViewModel
            {
                Name = pm.Character.Name,
                MaxHp = pm.Character.MaxHitPoints,
                MaxMana = pm.Character.MaxMana,
                IsHero = false,
                Level = pm.Character.Level,
                ClassName = pm.Character.ClassName,
                Sex = pm.Character.Sex,
                Race = pm.Character.Race?.Name ?? "",
                Hp = pm.Character.MaxHitPoints,
                Mana = pm.Character.CurrentMana,
                CurrentWeapon = pm.AttackSource?.Name ?? ""
            });
        }

        var charStates = new List<CharDisplayState>();
        foreach (var c in _vm.Heroes)
            charStates.Add(MakeState(c));
        foreach (var c in _vm.Enemies)
            charStates.Add(MakeState(c));

        var layout = CombatLayout.From(
            party1.Members.Select(m => m.Character.Name),
            party2.Members.Select(m => m.Character.Name),
            isDuel: true);

        var state = new CombatDisplayState(charStates, layout, isApiMode: _useApi);

        CombatResult result;
        if (_useApi && _apiClient is not null)
        {
            result = await _apiClient.SimulateCombatAsync(
                _fighter1!.Name,
                new List<int> { _fighter1!.Id },
                _fighter2!.Name,
                new List<int> { _fighter2!.Id },
                maxTicks: 500);
        }
        else
        {
            var diceService = new LoggingDiceService();
            var combatStats = new CombatStatsService();
            var combatService = new CombatService(diceService, combatStats, [new RangeModifier()]);
            var turnmeterService = new TurnmeterService();
            var statusEffectService = new StatusEffectService();
            var simulator = new CombatSimulator(
                combatService, turnmeterService, statusEffectService, diceService,
                new LowestHpTargetSelector(), new LowestHpTargetSelector(),
                new AutoActionDecisionSource(diceService), new AutoActionDecisionSource(diceService));

            result = await Task.Run(() => simulator.Simulate(party1, party2, 200), _cts.Token);

            result.DiceLog = diceService.DiceLog;
            result.Log = CombatLogMerger.Merge(result.Log, result.DiceLog);
        }

        _waitForNext.Reset();

        _presenter = new AvaloniaCombatPresenter(_vm, _displayConfig, _waitForNext, Dispatcher.UIThread)
        {
            PacingMultiplier = SpeedSlider.Value,
            AutoMode = _vm.Mode == "Auto"
        };

        _ = Task.Run(() =>
        {
            try
            {
                CombatPlaybackEngine.PlayTurnBased(result, state, _presenter,
                    prepareEventState: (entry, s) =>
                    {
                        if (!string.IsNullOrWhiteSpace(entry.SummonedPetName) && s.TryGet(entry.SummonedPetName) is null)
                        {
                            s.EnsurePet(entry.SummonedPetName, 20, entry.ActorName);
                        }
                    });
            }
            finally
            {
                DumpCombatLogFiles(result);

                Dispatcher.UIThread.Post(() =>
                {
                    _vm.IsRunning = false;
                    _vm.CombatOver = true;
                    NextButton.IsEnabled = false;
                    AutoButton.IsEnabled = false;
                    NewCombatButton.IsVisible = true;
                });
            }
        }, _cts.Token);
    }

    private void DumpCombatLogFiles(CombatResult result)
    {
        try
        {
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir)
                && !File.Exists(Path.Combine(dir, "src", "BattleArena.sln")))
                dir = Path.GetDirectoryName(dir);

            var outputDir = Path.Combine(
                string.IsNullOrEmpty(dir) ? AppContext.BaseDirectory : dir,
                "combat-logs");
            Directory.CreateDirectory(outputDir);

            var p1Name = result.Party1?.Name ?? "Party1";
            var p2Name = result.Party2?.Name ?? "Party2";
            var label = $"{p1Name}_vs_{p2Name}".Replace(" ", "_");

            CombatLogWriter.Write(result, label, outputDir, "GUI");

            CombatLogPruner.Prune(new DirectoryInfo(outputDir));
        }
        catch
        {
            // Best-effort — don't crash the GUI if file I/O fails
        }
    }

    private async void OnCopyLogClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        foreach (var entry in _vm.CombatLog)
        {
            foreach (var seg in entry.Segments)
                sb.Append(seg.Text);
            sb.AppendLine();
        }

        if (sb.Length > 0)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(sb.ToString());
        }
    }

    private static CharDisplayState MakeState(CharCardViewModel c) =>
        new()
        {
            Name = c.Name,
            MaxHp = c.MaxHp,
            Hp = c.MaxHp,
            Level = c.Level,
            ClassName = c.ClassName,
            Sex = c.Sex,
            Race = c.Race,
            MaxMana = c.MaxMana,
            Mana = c.Mana,
            Weapon = c.CurrentWeapon
        };

    private static void ResetCombatant(Character c)
    {
        c.CurrentHitPoints = c.MaxHitPoints;
        c.CurrentMana = c.MaxMana;
        c.ActiveStatusEffects.Clear();
    }

    private void OnNextClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _waitForNext?.Set();
    }

    private void OnAutoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_presenter is null) return;

        _presenter.AutoMode = !_presenter.AutoMode;
        AutoButton.Content = _presenter.AutoMode ? "Stop" : "Auto";
        NextButton.IsEnabled = !_presenter.AutoMode;

        if (_presenter.AutoMode)
            _waitForNext?.Set();
    }

    private void OnSpeedChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var sliderVal = (int)Math.Round(e.NewValue);
        var pacing = sliderVal switch
        {
            0 => 2.0,
            1 => 1.0,
            2 => 0.1,
            _ => 1.0
        };
        SpeedLabel.Text = pacing switch
        {
            >= 1.5 => "Slow",
            >= 0.75 => "Normal",
            _ => "Fast"
        };
        if (_presenter is not null)
            _presenter.PacingMultiplier = pacing;
    }
}
