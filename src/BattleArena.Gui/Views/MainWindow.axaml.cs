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
    }

    private void OnDuelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Scenario = "Duel";
        DuelButton.IsEnabled = false;
        PartyButton.IsEnabled = true;
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        SelectionHint.Text = "Select Fighter 1";
    }

    private void OnPartyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Scenario = "Party";
        DuelButton.IsEnabled = true;
        PartyButton.IsEnabled = false;
        _fighter1 = null;
        _fighter2 = null;
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        SelectionHint.Text = "Party mode coming soon — pick two fighters for now";
    }

    private void OnHeroSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (HeroListBox.SelectedItem is not Character hero) return;

        if (_fighter1 is null || _fighter1.Name == hero.Name)
        {
            _fighter1 = hero;
            _vm.Fighter1Name = hero.Name;
            SelectionHint.Text = "Select Fighter 2";
        }
        else if (_fighter2 is null || _fighter2.Name == hero.Name)
        {
            _fighter2 = hero;
            _vm.Fighter2Name = hero.Name;
            SelectionHint.Text = "Both fighters selected!";
        }
        else
        {
            // Both selected, clicking again replaces Fighter 2
            _fighter2 = hero;
            _vm.Fighter2Name = hero.Name;
        }

        HeroListBox.SelectedItem = null;
    }

    private async void OnFightClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_fighter1 is null || _fighter2 is null) return;

        _vm.Phase = "Combat";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.ActiveActorName = "";
        _vm.CombatOver = false;

        ResetCombatant(_fighter1);
        ResetCombatant(_fighter2);

        var party1 = Party.Solo(_fighter1, Roster.GetAttackSource(_fighter1));
        var party2 = Party.Solo(_fighter2, Roster.GetAttackSource(_fighter2));

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
        NextButton.IsEnabled = false;
        AutoButton.IsEnabled = false;
    }

    private void OnDemoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "Setup";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.Fighter1Name = "";
        _vm.Fighter2Name = "";
        _fighter1 = null;
        _fighter2 = null;
        DuelButton.IsEnabled = false;
        PartyButton.IsEnabled = true;
        SelectionHint.Text = "Select Fighter 1";
        HeroListBox.SelectedItem = null;
    }

    private void OnApiModeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "ApiMenu";
    }

    private void OnCreateCharClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Not yet implemented
    }

    private void OnVsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Not yet implemented
    }

    private void OnPartyVsPartyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Not yet implemented
    }

    private void OnBackToMainClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "MainMenu";
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

        var state = new CombatDisplayState(charStates, layout);

        var diceService = new LoggingDiceService();
        var combatStats = new CombatStatsService();
        var combatService = new CombatService(diceService, combatStats, [new RangeModifier()]);
        var turnmeterService = new TurnmeterService();
        var statusEffectService = new StatusEffectService();
        var simulator = new CombatSimulator(
            combatService, turnmeterService, statusEffectService, diceService,
            new LowestHpTargetSelector(), new LowestHpTargetSelector(),
            new AutoActionDecisionSource(diceService), new AutoActionDecisionSource(diceService));

        var result = await Task.Run(() => simulator.Simulate(party1, party2, 200), _cts.Token);

        result.DiceLog = diceService.DiceLog;
        result.Log = CombatLogMerger.Merge(result.Log, result.DiceLog);

        _waitForNext.Reset();

        _presenter = new AvaloniaCombatPresenter(_vm, _displayConfig, _waitForNext, Dispatcher.UIThread)
        {
            PacingMultiplier = SpeedSlider.Value
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
                    NextButton.IsEnabled = false;
                    AutoButton.IsEnabled = false;
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
