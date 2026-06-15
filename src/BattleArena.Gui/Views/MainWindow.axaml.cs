namespace BattleArena.Gui.Views;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Modifiers;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;
using BattleArena.Gui.Data;
using BattleArena.Gui.Models;
using BattleArena.Gui.Presenters;
using BattleArena.Gui.Services;
using BattleArena.Gui.ViewModels;
using BattleArena.Gui.Rendering;
using BattleArena.Gui.ViewModels.World;
using BattleArena.Gui.ViewModels.WorldMap;
using BattleArena.Presentation;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm = new();
    private readonly GuiDisplayConfig _displayConfig;

    private CancellationTokenSource? _cts;
    private ManualResetEventSlim? _waitForNext;
    private AvaloniaCombatPresenter? _presenter;
    private readonly List<CharacterDisplayItem?> _team1 = [];
    private readonly List<CharacterDisplayItem?> _team2 = [];
    private int _heroTeamSize = 1;
    private int _enemyTeamSize = 1;
    private bool _useApi;
    private BattleArenaApiClient? _apiClient;
    private Party? _combatParty1;
    private Party? _combatParty2;
    private List<Character> _apiRoster = [];
    private readonly ISoundPlayer? _soundPlayer;
    private string _previousPhase = "MainMenu";
    private bool _fromSpellPreview;
    private bool _combatFromWorld;

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
        HeroListBox.ItemsSource = ToDisplayItems(Roster.AllHeroes);
        DummyListBox.ItemsSource = ToDisplayItems(Roster.AllDummies);
        DuelButton.IsEnabled = false;
        SelectionHint.Text = "Select Team 1 — 0/1";
        HeroTeamSizeSlider.IsVisible = false;
        EnemyTeamSizeSlider.IsVisible = false;
        HeroTeamSizeLabel.IsVisible = false;
        EnemyTeamSizeLabel.IsVisible = false;

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

        var soundsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds");
        _soundPlayer = Directory.Exists(soundsDir)
            ? new AvaloniaSoundPlayer(soundsDir)
            : null;

        CombatCardGrid.SizeChanged += OnCombatCardAreaSizeChanged;
    }

    private void OnWorldCombatEncounter(string heroName, string enemyName)
    {
        var hero = Roster.AllHeroes.Find(c => c.Name == heroName);
        var enemy = Roster.AllHeroes.Find(c => c.Name == enemyName);
        if (hero is null || enemy is null) return;

        _combatFromWorld = true;
        StartWorldCombat(hero, enemy);
    }

    private void StartWorldCombat(Character hero, Character enemy)
    {
        TurnButton.IsVisible = true;
        AutoPlayButton.IsVisible = true;
        _vm.ErrorMessage = "";
        SpeedSlider.Value = 1;
        _vm.Phase = "Combat";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.ActiveActorName = "";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;

        ResetCombatant(hero);
        ResetCombatant(enemy);

        var party1 = new Party();
        party1.Members.Add(new PartyMember { Character = hero, AttackSource = Roster.GetAttackSource(hero) });
        var party2 = new Party();
        party2.Members.Add(new PartyMember { Character = enemy, AttackSource = Roster.GetAttackSource(enemy) });

        _combatParty1 = party1;
        _combatParty2 = party2;
        _vm.EngagementRange = "Melee";

        PopulateCharacterCards(party1, party2);
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
        _heroTeamSize = 1;
        _enemyTeamSize = 1;
        HeroTeamSizeSlider.IsVisible = false;
        EnemyTeamSizeSlider.IsVisible = false;
        HeroTeamSizeLabel.IsVisible = false;
        EnemyTeamSizeLabel.IsVisible = false;
        ClearSelection();
    }

    private void OnClashClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Scenario = "Party";
        DuelButton.IsEnabled = true;
        ClashButton.IsEnabled = false;
        _heroTeamSize = (int)HeroTeamSizeSlider.Value;
        _enemyTeamSize = (int)EnemyTeamSizeSlider.Value;
        HeroTeamSizeSlider.IsVisible = true;
        EnemyTeamSizeSlider.IsVisible = true;
        HeroTeamSizeLabel.IsVisible = true;
        EnemyTeamSizeLabel.IsVisible = true;
        ClearSelection();
    }

    private void ClearSelection()
    {
        foreach (var item in _team1.Concat(_team2))
        {
            if (item is not null)
                item.TeamSlot = 0;
        }
        _team1.Clear();
        _team2.Clear();
        _vm.CanProceed = false;
        SelectionHint.Text = $"Select Team 1 — 0/{_heroTeamSize}";
    }

    private void UpdateSelectionHint()
    {
        var team1Full = _team1.Count >= _heroTeamSize;
        var team2Full = _team2.Count >= _enemyTeamSize;
        _vm.CanProceed = team1Full && team2Full;

        if (!team1Full)
            SelectionHint.Text = $"Select Team 1 — {_team1.Count}/{_heroTeamSize}";
        else if (!team2Full)
            SelectionHint.Text = $"Select Team 2 — {_team2.Count}/{_enemyTeamSize}";
        else
            SelectionHint.Text = $"All teams ready!  Press PROCEED.";
    }

    private void OnHeroSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not CharacterDisplayItem item) return;
        listBox.SelectedItem = null;

        // Clicking a selected card deselects it
        if (item.IsSelected)
        {
            _team1.Remove(item);
            _team2.Remove(item);
            item.TeamSlot = 0;
            UpdateSelectionHint();
            return;
        }

        // Assign to first team with room
        if (_team1.Count < _heroTeamSize)
        {
            item.TeamSlot = 1;
            _team1.Add(item);
        }
        else if (_team2.Count < _enemyTeamSize)
        {
            item.TeamSlot = 2;
            _team2.Add(item);
        }
        else return; // both teams full

        UpdateSelectionHint();
    }

    private void OnHeroTeamSizeChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var val = (int)e.NewValue;
        HeroTeamSizeLabel.Text = val.ToString();
        if (_vm.Scenario == "Party")
        {
            _heroTeamSize = val;
            ClearSelection();
        }
    }

    private void OnEnemyTeamSizeChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var val = (int)e.NewValue;
        EnemyTeamSizeLabel.Text = val.ToString();
        if (_vm.Scenario == "Party")
        {
            _enemyTeamSize = val;
            ClearSelection();
        }
    }

    private void OnDismissErrorClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.ErrorMessage = "";
    }

    private void OnFightClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_team1.Count == 0 || _team2.Count == 0) return;
        StartCombat();
    }

    private void OnNewCombatClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NewCombatButton.IsVisible = false;

        if (_fromSpellPreview)
        {
            _presenter?.Stop();
            _vm.PropertyChanged -= OnVmWaitingChanged;
            _presenter = null;
            _waitForNext?.Reset();
            OnRunSpellPreviewClick(sender, e);
            return;
        }

        if (_team1.Count == 0 || _team2.Count == 0) return;
        _presenter?.Stop();
        _vm.PropertyChanged -= OnVmWaitingChanged;
        _presenter = null;
        _waitForNext?.Reset();
        StartCombat();
    }

    private void StartCombat()
    {
        TurnButton.IsVisible = true;
        AutoPlayButton.IsVisible = true;
        _vm.ErrorMessage = "";
        SpeedSlider.Value = 1;
        _vm.Phase = "Combat";
        _vm.CombatLog.Clear();
        foreach (var h in _vm.Heroes) h.EffectBars.Clear();
        foreach (var e in _vm.Enemies) e.EffectBars.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.ActiveActorName = "";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;

        foreach (var item in _team1.Concat(_team2))
            ResetCombatant(item!.Character);

        _combatParty1 = BuildParty(_team1);
        _combatParty2 = BuildParty(_team2);
        _vm.EngagementRange = "Melee";

        PopulateCharacterCards(_combatParty1, _combatParty2);
    }

    private void PopulateCharacterCards(Party party1, Party party2)
    {
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
                StrikeRating = pm.Character.StrikeRating,
                ArmorName = pm.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = pm.Character.Equipment.TotalArmorClass,
                WeaponStats = FormatWeaponStats(pm.AttackSource),
                MagicResistance = pm.Character.ComputeResistance(ResistanceType.Magic),
                Hp = pm.Character.CurrentHitPoints,
                Mana = pm.Character.CurrentMana,
                CurrentWeapon = pm.AttackSource?.Name ?? "",
                Portrait = PortraitResolver.GetPortrait(pm.Character.Name)
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
                StrikeRating = pm.Character.StrikeRating,
                ArmorName = pm.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = pm.Character.Equipment.TotalArmorClass,
                WeaponStats = FormatWeaponStats(pm.AttackSource),
                MagicResistance = pm.Character.ComputeResistance(ResistanceType.Magic),
                Hp = pm.Character.CurrentHitPoints,
                Mana = pm.Character.CurrentMana,
                CurrentWeapon = pm.AttackSource?.Name ?? "",
                Portrait = PortraitResolver.GetPortrait(pm.Character.Name)
            });
        }
        RecalcCardLayout();
    }

    private void RecalcCardLayout()
    {
        if (CombatCardGrid.Bounds.Width > 0 && CombatCardGrid.Bounds.Height > 0)
        {
            var sideWidth = (CombatCardGrid.Bounds.Width - 76) / 2.0;
            _vm.RecalcSideLayout(sideWidth, CombatCardGrid.Bounds.Height);
            HeroesCards.Width = _vm.HeroLayoutCentered ? double.NaN : sideWidth;
            EnemiesCards.Width = _vm.EnemyLayoutCentered ? double.NaN : sideWidth;
        }
    }

    private void OnCombatCardAreaSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_vm.IsCombatPhase)
            RecalcCardLayout();
    }

    private static Party BuildParty(List<CharacterDisplayItem?> team)
    {
        var party = new Party();
        foreach (var item in team)
        {
            if (item?.Character is null) continue;
            party.Members.Add(new PartyMember
            {
                Character = item.Character,
                AttackSource = Roster.GetAttackSource(item.Character)
            });
        }
        return party;
    }

    private async void OnStartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // No longer used — replaced by OnFightClick
    }

    private void OnBackToSetupClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter?.Stop();
        _vm.PropertyChanged -= OnVmWaitingChanged;
        _presenter = null;
        _cts?.Cancel();
        _waitForNext?.Set();
        _waitForNext?.Dispose();
        _cts?.Dispose();
        _cts = null;
        _waitForNext = null;

        if (_fromSpellPreview)
        {
            _fromSpellPreview = false;
            _vm.Phase = "SpellPreview";
            _vm.CombatLog.Clear();
            _vm.Heroes.Clear();
            _vm.Enemies.Clear();
            _vm.IsRunning = false;
            _vm.CombatOver = false;
            _vm.Tick = 0;
            _vm.RoundNumber = 0;
            _vm.TickInRound = 0;
            NewCombatButton.IsVisible = false;
            return;
        }

        _vm.Phase = "Setup";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.IsRunning = false;
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        NewCombatButton.IsVisible = false;
        ClearSelection();
    }

    private void OnDemoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "Setup";
        _vm.Mode = "TurnBased";
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        StopBlink();
        TurnButton.Classes.Remove("waiting");
        TurnButton.Content = "Turn based";
        AutoPlayButton.Classes.Remove("waiting");
        AutoPlayButton.Content = "Auto-play";
        NewCombatButton.IsVisible = false;
        _combatParty1 = null;
        _combatParty2 = null;
        _heroTeamSize = 1;
        _enemyTeamSize = 1;
        HeroTeamSizeSlider.IsVisible = false;
        EnemyTeamSizeSlider.IsVisible = false;
        HeroTeamSizeLabel.IsVisible = false;
        EnemyTeamSizeLabel.IsVisible = false;
        ClearSelection();
        DuelButton.IsEnabled = false;
        ClashButton.IsEnabled = true;
        HeroListBox.ItemsSource = ToDisplayItems(Roster.AllHeroes);
        DummyListBox.ItemsSource = ToDisplayItems(Roster.AllDummies);
        DummyListBox.IsVisible = true;
        DummyHeader.IsVisible = true;
    }

    private void PopulatePartyPanel()
    {
        PartyPanel.Children.Clear();
        var heroes = _vm.WorldViewModel.Combatants.Where(c => c.IsHero).ToList();

        foreach (var c in heroes)
        {
            var card = new Border
            {
                Tag = c.Name,
                Background = new SolidColorBrush(Color.Parse("#0d0d18")),
                BorderBrush = new SolidColorBrush(Color.Parse("#1a1a2e")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(0, 0, 0, 4),
                Margin = new Thickness(0, 2),
            };

            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = c.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 10,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(10, 6, 0, 0),
            });

            var portrait = PortraitResolver.GetPortrait(c.Name);
            const double pw = 80;
            const double ph = 120;
            if (portrait is not null)
            {
                var img = new Avalonia.Controls.Image
                {
                    Source = portrait,
                    Width = pw,
                    Height = ph,
                    Stretch = Avalonia.Media.Stretch.Fill,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Margin = new Thickness(10, 2, 10, 0),
                };
                img.PointerEntered += (_, _) => HighlightCombatantOnGrid(c.Name);
                img.PointerExited += (_, _) => ClearGridHighlight();
                stack.Children.Add(img);
            }
            else
            {
                var placeholder = new Border
                {
                    Width = pw, Height = pw,
                    Background = new SolidColorBrush(Color.Parse("#1a1a2e")),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(10, 2, 10, 0),
                    Child = new TextBlock
                    {
                        Text = c.Name.Length > 0 ? c.Name[0].ToString() : "?",
                        Foreground = new SolidColorBrush(Colors.Gray),
                        FontSize = 24,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    },
                };
                placeholder.PointerEntered += (_, _) => HighlightCombatantOnGrid(c.Name);
                placeholder.PointerExited += (_, _) => ClearGridHighlight();
                stack.Children.Add(placeholder);
            }

            var hpFraction = c.MaxHp > 0 ? Math.Clamp((double)c.CurrentHp / c.MaxHp, 0, 1) : 0;
            var hpBar = new Border
            {
                Width = pw,
                Height = 6,
                Background = new SolidColorBrush(Color.Parse("#0a1a0a")),
                BorderBrush = new SolidColorBrush(Color.Parse("#114411")),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Margin = new Thickness(10, 2, 10, 0),
            };
            var hpFill = new Border
            {
                Width = hpFraction * (pw - 2),
                Height = 4,
                Background = new SolidColorBrush(Color.Parse("#44cc44")),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            hpBar.Child = hpFill;
            stack.Children.Add(hpBar);

            card.Child = stack;
            PartyPanel.Children.Add(card);
        }
    }

    private void HighlightCombatantOnGrid(string name)
    {
        var vm = _vm.WorldViewModel;
        var combatant = vm.Combatants.FirstOrDefault(c => c.Name == name);
        if (combatant is null) return;
        CharacterRenderer.HoveredCombatant = combatant.Position;
        CharacterRenderer.RenderCombatants(vm.Combatants, vm.Map, WorldViewControl.GetMapCanvas());
        HighlightPartyPortrait(name);
    }

    private void ClearGridHighlight()
    {
        var vm = _vm.WorldViewModel;
        CharacterRenderer.HoveredCombatant = null;
        CharacterRenderer.RenderCombatants(vm.Combatants, vm.Map, WorldViewControl.GetMapCanvas());
        HighlightPartyPortrait("");
    }

    private void OnWorldMapClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "WorldMap";
    }

    private void OnEnterLocationClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PopulatePartyPanel();
        var enemies = _vm.WorldViewModel.Combatants.Where(c => !c.IsHero).Select(c => c.Name).ToList();
        EnemyList.ItemsSource = enemies;
        WorldViewControl.CombatantHovered += name =>
        {
            HighlightPartyPortrait(name);
        };
        _vm.Phase = "Location";
    }

    private void HighlightPartyPortrait(string name)
    {
        foreach (var child in PartyPanel.Children)
        {
            if (child is Border border)
            {
                var isTarget = !string.IsNullOrEmpty(name) && border.Tag?.ToString() == name;
                border.BorderBrush = new SolidColorBrush(
                    Color.Parse(isTarget ? "#d4a017" : "#1a1a2e"));
            }
        }
    }

    private void OnBackToWorldMapClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "WorldMap";
    }

    private void OnApiModeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "ApiMenu";
    }

    private async void OnCreateCharClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.Phase = "CharCreation";
        _vm.CharName = "";
        _vm.SelectedClassName = null;
        _vm.SelectedRaceName = null;
        _vm.SelectedSubraceName = null;
        _vm.CharStr = 0;
        _vm.CharStrExceptional = 0;
        _vm.CharDex = 0;
        _vm.CharSta = 0;
        _vm.CharInt = 0;
        _vm.CharWis = 0;
        _vm.CharCha = 0;
        ExcStrLabel2.Text = "";
        _vm.CreationStep = 0;

        if (_apiClient is not null)
        {
            try
            {
                _vm.LoadedRaces = await _apiClient.GetRacesAsync();
                _vm.LoadedSubraces = await _apiClient.GetSubracesAsync();
                _vm.LoadedClasses = await _apiClient.GetClassesAsync();
                _vm.LoadedDeities = await _apiClient.GetDeitiesAsync();
                _vm.LoadedSchools = await _apiClient.GetSchoolsAsync();
            }
            catch
            {
                _vm.LoadedRaces = [];
                _vm.LoadedSubraces = [];
                _vm.LoadedClasses = [];
                _vm.LoadedDeities = [];
                _vm.LoadedSchools = [];
            }
        }
    }


    private void OnBackToMainClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter?.Stop();
        _presenter = null;
        _cts?.Cancel();
        _waitForNext?.Set();
        _waitForNext?.Dispose();
        _cts?.Dispose();
        _cts = null;
        _waitForNext = null;

        if (_combatFromWorld)
        {
            _combatFromWorld = false;
            _vm.Phase = "World";
        }
        else
        {
            _vm.Phase = "MainMenu";
        }

        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.IsRunning = false;
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
        _vm.IsApiMode = true;
        _useApi = true;
        _vm.Scenario = "Duel";
        _vm.Phase = "Setup";
        _heroTeamSize = 1;
        _enemyTeamSize = 1;
        HeroTeamSizeSlider.IsVisible = false;
        EnemyTeamSizeSlider.IsVisible = false;
        HeroTeamSizeLabel.IsVisible = false;
        EnemyTeamSizeLabel.IsVisible = false;
        HeroListBox.ItemsSource = null;
        HeroListBox.ItemsSource = ToDisplayItems(_apiRoster);
        DummyListBox.IsVisible = false;
        DummyHeader.IsVisible = false;
        ClearSelection();
        DuelButton.IsEnabled = false;
        ClashButton.IsEnabled = true;
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
        _vm.IsApiMode = true;
        _useApi = true;
        _vm.Scenario = "Party";
        _vm.Phase = "Setup";
        _heroTeamSize = (int)HeroTeamSizeSlider.Value;
        _enemyTeamSize = (int)EnemyTeamSizeSlider.Value;
        HeroTeamSizeSlider.IsVisible = true;
        EnemyTeamSizeSlider.IsVisible = true;
        HeroTeamSizeLabel.IsVisible = true;
        EnemyTeamSizeLabel.IsVisible = true;
        HeroListBox.ItemsSource = null;
        HeroListBox.ItemsSource = ToDisplayItems(_apiRoster);
        DummyListBox.ItemsSource = ToDisplayItems(Roster.AllDummies);
        ClearSelection();
        DuelButton.IsEnabled = true;
        ClashButton.IsEnabled = false;
    }

    // ── Character Creation Handlers ───────────────────────────

    private static readonly Random _charRng = new();
    private static readonly string[] _warriorClasses = ["Barbarian", "Fighter", "Paladin", "Knight", "Ranger"];

    private static int Clamp(int val, int min, int max) => Math.Max(min, Math.Min(max, val));
    private static string FormatExceptional(int pct) => pct == 100 ? "00" : pct.ToString("00");

    private static int Roll4d6DropLowest()
    {
        var rolls = new int[4];
        for (var i = 0; i < 4; i++)
            rolls[i] = _charRng.Next(1, 7);
        Array.Sort(rolls);
        return rolls[1] + rolls[2] + rolls[3];
    }

    private void OnRollStatsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var raceName = _vm.SelectedRace;

        var apiRace = _vm.LoadedRaces.FirstOrDefault(r =>
            r.Name.Equals(raceName, StringComparison.OrdinalIgnoreCase));
        if (apiRace is null) return;

        int[] raceBonuses = [apiRace.AbilityBonuses.GetValueOrDefault("Strength", 0),
                             apiRace.AbilityBonuses.GetValueOrDefault("Dexterity", 0),
                             apiRace.AbilityBonuses.GetValueOrDefault("Stamina", 0),
                             apiRace.AbilityBonuses.GetValueOrDefault("Intelligence", 0),
                             apiRace.AbilityBonuses.GetValueOrDefault("Wisdom", 0),
                             apiRace.AbilityBonuses.GetValueOrDefault("Charisma", 0)];
        int[] raceMin = [apiRace.StrengthMin, apiRace.DexterityMin, apiRace.StaminaMin,
                         apiRace.IntelligenceMin, apiRace.WisdomMin, apiRace.CharismaMin];
        int[] raceMax = [apiRace.StrengthMax, apiRace.DexterityMax, apiRace.StaminaMax,
                         apiRace.IntelligenceMax, apiRace.WisdomMax, apiRace.CharismaMax];

        var subrace = _vm.SelectedSubrace;
        var apiSubrace = subrace is not null
            ? _vm.LoadedSubraces.FirstOrDefault(s =>
                s.Name.Equals(subrace, StringComparison.OrdinalIgnoreCase))
            : null;
        int[] subBonuses = apiSubrace is not null
            ? [apiSubrace.StrengthBonus, apiSubrace.DexterityBonus, apiSubrace.StaminaBonus,
               apiSubrace.IntelligenceBonus, apiSubrace.WisdomBonus, apiSubrace.CharismaBonus]
            : [0, 0, 0, 0, 0, 0];

        var rolls = new[] { Roll4d6DropLowest(), Roll4d6DropLowest(), Roll4d6DropLowest(),
                            Roll4d6DropLowest(), Roll4d6DropLowest(), Roll4d6DropLowest() };

        _vm.CharStr = Clamp(rolls[0] + raceBonuses[0] + subBonuses[0], raceMin[0], raceMax[0]);
        _vm.CharDex = Clamp(rolls[1] + raceBonuses[1] + subBonuses[1], raceMin[1], raceMax[1]);
        _vm.CharSta = Clamp(rolls[2] + raceBonuses[2] + subBonuses[2], raceMin[2], raceMax[2]);
        _vm.CharInt = Clamp(rolls[3] + raceBonuses[3] + subBonuses[3], raceMin[3], raceMax[3]);
        _vm.CharWis = Clamp(rolls[4] + raceBonuses[4] + subBonuses[4], raceMin[4], raceMax[4]);
        _vm.CharCha = Clamp(rolls[5] + raceBonuses[5] + subBonuses[5], raceMin[5], raceMax[5]);

        RollExceptionalStrength(_vm.CharStr, raceName, subrace);
    }

    private void RollExceptionalStrength(int finalStr, string race, string? subrace)
    {
        var cls = _vm.SelectedClass;
        var canExceptional = race.Equals("Human", StringComparison.OrdinalIgnoreCase)
                          || race.Equals("Half-Elf", StringComparison.OrdinalIgnoreCase);
        if (finalStr == 18 && canExceptional && _warriorClasses.Contains(cls))
        {
            _vm.CharStrExceptional = _charRng.Next(1, 101);
            ExcStrLabel2.Text = $"✦ Exceptional Strength: 18/{FormatExceptional(_vm.CharStrExceptional)}";
        }
        else
        {
            _vm.CharStrExceptional = 0;
            ExcStrLabel2.Text = "";
        }
    }

    private void OnClassButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string cls })
            _vm.SelectedClassName = cls;
    }

    private void OnRaceButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string race })
            _vm.SelectedRaceName = race;
    }

    private void OnSchoolButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string school })
        {
            _vm.SelectedSchool = school;
        }
    }

    private void OnDeityButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string deity })
        {
            _vm.SelectedDeity = deity;
        }
    }

    private void OnSubraceButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string sub })
            _vm.SelectedSubraceName = sub;
    }

    private void OnCharCreateClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Character creation complete — not saving yet.
    }

    private void OnCharNameChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        // CanGoNext updates automatically via binding
    }

    private void OnNextStepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var step = _vm.CreationStep;
        if (step == 0 && _vm.CanGoNext)
            _vm.CreationStep = 1;
        else if (step == 1 && _vm.SelectedRaceName is not null)
        {
            var subraces = _vm.AvailableSubraces;
            _vm.CreationStep = subraces.Count > 0 ? 2 : 3;
        }
        else if (step == 2 && _vm.CanGoNext)
            _vm.CreationStep = 3;
        else if (step == 3 && _vm.CanGoNext)
            _vm.CreationStep = 4;
    }

    private void OnCreateCharFinishClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnBackToMainClick(sender, e);
    }

    private void OnBackStepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var step = _vm.CreationStep;
        if (step == 4)
            _vm.CreationStep = 3;
        else if (step == 3)
        {
            var subraces = _vm.AvailableSubraces;
            _vm.CreationStep = subraces.Count > 0 ? 2 : 1;
        }
        else if (step == 2)
            _vm.CreationStep = 1;
        else if (step == 1)
            _vm.CreationStep = 0;
    }

    private async void OnTurnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_presenter is not null)
        {
            _presenter.AutoMode = false;
            _vm.WaitingForNextTurn = false;
            _waitForNext?.Set();
            return;
        }

        _vm.Mode = "TurnBased";
        if (_combatParty1 is not null && _combatParty2 is not null)
            await RunCombat(_combatParty1, _combatParty2);
    }

    private async void OnAutoPlayClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_presenter is not null)
        {
            _presenter.AutoMode = !_presenter.AutoMode;
            AutoPlayButton.Content = _presenter.AutoMode ? "Stop turn" : "Auto-play";
            TurnButton.Content = _presenter.AutoMode ? "Next turn" : "Turn based";
            if (_presenter.AutoMode)
            {
                StopBlink();
                TurnButton.Classes.Remove("waiting");
                AutoPlayButton.Classes.Add("waiting");
                _waitForNext?.Set();
            }
            else
            {
                AutoPlayButton.Classes.Remove("waiting");
                if (_vm.WaitingForNextTurn)
                    StartBlink();
            }
            return;
        }

        _vm.Mode = "Auto";
        if (_combatParty1 is not null && _combatParty2 is not null)
            await RunCombat(_combatParty1, _combatParty2);
    }

    private void OnBackToMainFromSetupClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter?.Stop();
        _presenter = null;
        _cts?.Cancel();
        _waitForNext?.Set();
        _waitForNext?.Dispose();
        _cts?.Dispose();
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
            _vm.CanProceed = false;
            HeroListBox.ItemsSource = ToDisplayItems(Roster.AllHeroes);
            ClearSelection();
        }
    }

    private async Task RunCombat(Party party1, Party party2)
    {
        _presenter?.Stop();
        _vm.PropertyChanged -= OnVmWaitingChanged;
        StopBlink();
        _presenter = null;
        _cts?.Cancel();
        _waitForNext?.Set();
        _waitForNext?.Dispose();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();
        _waitForNext = new ManualResetEventSlim(false);
        _vm.IsRunning = true;
        NewCombatButton.IsVisible = false;
        AutoPlayButton.Content = "Auto-play";
        TurnButton.Content = "Next turn";

        var charStates = new List<CharDisplayState>();
        foreach (var c in _vm.Heroes)
            charStates.Add(MakeState(c));
        foreach (var c in _vm.Enemies)
            charStates.Add(MakeState(c));

        var layout = CombatLayout.From(
            party1.Members.Select(m => m.Character.Name),
            party2.Members.Select(m => m.Character.Name),
            isDuel: _heroTeamSize == 1 && _enemyTeamSize == 1);

        var state = new CombatDisplayState(charStates, layout, isApiMode: _useApi);

        var diceService = new LoggingDiceService();
        var combatStats = new CombatStatsService();
        var combatService = new CombatService(diceService, combatStats, [new RangeModifier()]);
        var turnmeterService = new TurnmeterService();
        var statusEffectService = new StatusEffectService();
        var simulator = new CombatSimulator(
            combatService, turnmeterService, statusEffectService, diceService,
            new LowestHpTargetSelector(), new LowestHpTargetSelector(),
            new AutoActionDecisionSource(diceService), new AutoActionDecisionSource(diceService));

        var result = await Task.Run(() => simulator.Simulate(party1, party2, CombatSimulator.DefaultMaxTicks), _cts.Token);

        result.DiceLog = diceService.DiceLog;
        result.Log = CombatLogMerger.Merge(result.Log, result.DiceLog);
        DumpCombatLogFiles(result);

        _waitForNext.Reset();

        _presenter = new AvaloniaCombatPresenter(_vm, _displayConfig, _waitForNext, Dispatcher.UIThread, _soundPlayer)
        {
            PacingMultiplier = SpeedSlider.Value,
            AutoMode = _vm.Mode == "Auto"
        };

        _vm.PropertyChanged += OnVmWaitingChanged;
        if (_presenter.AutoMode)
            AutoPlayButton.Classes.Add("waiting");

        var playbackToken = _cts.Token;
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
                        if (entry.EventType == "Move" && entry.Message is { } msg)
                        {
                            var arrow = "→ ";
                            var idx = msg.LastIndexOf(arrow);
                            if (idx >= 0)
                            {
                                var to = msg[(idx + arrow.Length)..].TrimEnd('.');
                                _vm.EngagementRange = to;
                            }
                        }
                    });
            }
            finally
            {
                if (!playbackToken.IsCancellationRequested)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _vm.IsRunning = false;
                        _vm.CombatOver = true;
                        StopBlink();
                        TurnButton.Classes.Remove("waiting");
                        TurnButton.Content = "Turn based";
                        AutoPlayButton.Classes.Remove("waiting");
                        AutoPlayButton.Content = "Auto-play";
                        TurnButton.IsVisible = false;
                        AutoPlayButton.IsVisible = false;
                        NewCombatButton.IsVisible = true;
                    });
                }
            }
        }, playbackToken);
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

            var lead1   = result.Party1?.Members?.FirstOrDefault()?.Character?.Name ?? "Party1";
            var lead2   = result.Party2?.Members?.FirstOrDefault()?.Character?.Name ?? "Party2";
            var isParty = (result.Party1?.Members?.Count ?? 1) > 1 || (result.Party2?.Members?.Count ?? 1) > 1;
            var suffix  = isParty ? "_party" : "";
            var label   = $"{lead1}_vs_{lead2}{suffix}".Replace(" ", "_");

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
            Hp = c.Hp,
            Level = c.Level,
            ClassName = c.ClassName,
            Sex = c.Sex,
            Race = c.Race,
            StrikeRating = c.StrikeRating,
            ArmorName = c.ArmorName,
            ArmorClass = c.ArmorClass,
            WeaponStats = c.WeaponStats,
            MagicResistance = c.MagicResistance,
            MaxMana = c.MaxMana,
            Mana = c.Mana,
            Weapon = c.CurrentWeapon
        };

    private DispatcherTimer? _blinkTimer;

    private void OnVmWaitingChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(_vm.WaitingForNextTurn)) return;
        if (_vm.WaitingForNextTurn)
            StartBlink();
        else
            StopBlink();
    }

    private void StartBlink()
    {
        StopBlink();
        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _blinkTimer.Tick += (_, _) =>
        {
            if (TurnButton.Classes.Contains("waiting"))
                TurnButton.Classes.Remove("waiting");
            else
                TurnButton.Classes.Add("waiting");
        };
        _blinkTimer.Start();
    }

    private void StopBlink()
    {
        if (_blinkTimer is null) return;
        _blinkTimer.Stop();
        _blinkTimer = null;
        TurnButton.Classes.Remove("waiting");
    }

    private static void ResetCombatant(Character c)
    {
        c.CurrentHitPoints = c.MaxHitPoints;
        c.CurrentMana = c.MaxMana;
        c.ActiveStatusEffects.Clear();
        c.RemainingCasts = c.MaxCastsPerCombat;
    }

    private void OnSpeedChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var sliderVal = (int)Math.Round(e.NewValue);
        var pacing = sliderVal switch
        {
            0 => 2.0,
            1 => 1.0,
            2 => 0.5,
            3 => 0.1,
            _ => 1.0
        };
        SpeedLabel.Text = sliderVal switch
        {
            0 => "Slow",
            1 => "Normal",
            2 => "Fast",
            3 => "Turbo",
            _ => "Normal"
        };
        if (_presenter is not null)
            _presenter.PacingMultiplier = pacing;
    }

    private static int DieSides(DieType d) => d switch
    {
        DieType.D4 => 4,
        DieType.D6 => 6,
        DieType.D8 => 8,
        DieType.D10 => 10,
        DieType.D12 => 12,
        DieType.D20 => 20,
        _ => 0
    };

    private static string FormatWeaponStats(IAttackSource? source)
    {
        if (source is null) return "";
        var dice = $"{source.DamageCount}d{DieSides(source.DamageDie)}";
        return source.AttackBonus > 0 ? $"{dice}+{source.AttackBonus}" : dice;
    }

    // ── Spell Preview Handlers ───────────────────────────────

    private void OnSpellPreviewClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _previousPhase = _vm.Phase;
        _vm.Phase = "SpellPreview";
        _vm.SelectedCaster = null;
        _vm.SelectedSpell = null;
        _vm.AvailableSpells.Clear();
        _vm.AvailableCasters.Clear();

        var casters = Roster.AllHeroes
            .Where(c => c.MemorizedSpells.Count > 0 && PortraitResolver.HasPortrait(c.Name))
            .Select(c => new CharacterDisplayItem(c))
            .ToList();
        foreach (var c in casters)
            _vm.AvailableCasters.Add(c);
    }

    private void OnSpellPreviewCasterSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is CharacterDisplayItem item)
        {
            _vm.SelectedCaster = item;
            _vm.SelectedSpell = null;
        }
    }

    private void OnSpellPreviewSpellSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is SpellDisplayItem item)
            _vm.SelectedSpell = item;
    }

    private async void OnRunSpellPreviewClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_vm.SelectedCaster is null || _vm.SelectedSpell is null) return;
        _vm.IsPreviewing = true;

        RunPreviewButton.IsEnabled = false;

        var caster = _vm.SelectedCaster.Character;
        var dummy = Roster.AllDummies.FirstOrDefault(d => d.Name == "Practice Dummy");
        if (dummy is null)
        {
            _vm.ErrorMessage = "Practice Dummy not found in roster.";
            _vm.IsPreviewing = false;
            return;
        }

        ResetCombatant(caster);
        ResetCombatant(dummy);

        var spell = PreparePreviewSpell(_vm.SelectedSpell.Spell);
        dummy.MemorizedSpells.Clear();
        dummy.Equipment = new ArmorSlots();
        dummy.ActiveStatusEffects.Clear();
        dummy.Race = null;
        dummy.Subrace = null;

        // Force all on-hit effects to land (preview wants to see the visuals)
        foreach (var effect in spell.OnHitEffects)
            effect.ApplicationChance = 100;

        caster.CurrentHitPoints = caster.MaxHitPoints / 2;
        dummy.CurrentHitPoints = dummy.MaxHitPoints / 2;

        if (spell.IsHealing)
            caster.TurnSpeed = 30;

        var party1 = new Party { Name = caster.Name };
        party1.Members.Add(new PartyMember
        {
            Character = caster,
            AttackSource = Roster.GetAttackSource(caster)
        });

        var party2 = new Party { Name = "Target" };
        party2.Members.Add(new PartyMember
        {
            Character = dummy,
            AttackSource = Roster.GetAttackSource(dummy)
        });

        _combatParty1 = party1;
        _combatParty2 = party2;
        _fromSpellPreview = true;

        TurnButton.IsVisible = true;
        AutoPlayButton.IsVisible = true;
        _vm.ErrorMessage = "";
        SpeedSlider.Value = 1;
        _vm.Phase = "Combat";
        _vm.CombatLog.Clear();
        foreach (var h in _vm.Heroes) h.EffectBars.Clear();
        foreach (var enemy in _vm.Enemies) enemy.EffectBars.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.ActiveActorName = "";
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        _vm.EngagementRange = "Melee";

        PopulateCharacterCards(party1, party2);

        var diceService = new LoggingDiceService();
        var combatStats = new CombatStatsService();
        var combatService = new CombatService(diceService, combatStats, [new RangeModifier()]);
        var turnmeterService = new TurnmeterService();
        var statusEffectService = new StatusEffectService();
        var forcedSpell = new ForcedSpellDecisionSource(spell);
        var autoSource = new AutoActionDecisionSource(diceService);

        _cts = new CancellationTokenSource();
        _waitForNext = new ManualResetEventSlim(false);
        _vm.IsRunning = true;
        NewCombatButton.IsVisible = false;
        AutoPlayButton.Content = "Auto-play";
        TurnButton.Content = "Next turn";

        var simulator = new CombatSimulator(
            combatService, turnmeterService, statusEffectService, diceService,
            new LowestHpTargetSelector(), new LowestHpTargetSelector(),
            forcedSpell, autoSource);

        var maxEffectDuration = spell.OnHitEffects.Count > 0
            ? spell.OnHitEffects.Max(e => e.Duration)
            : 0;
        var previewTicks = 8 + maxEffectDuration + 1;
        var result = await Task.Run(() => simulator.Simulate(party1, party2, previewTicks), _cts.Token);

        result.DiceLog = diceService.DiceLog;
        result.Log = CombatLogMerger.Merge(result.Log, result.DiceLog);

        _waitForNext.Reset();

        var charStates = new List<CharDisplayState>();
        foreach (var c in _vm.Heroes)
            charStates.Add(MakeState(c));
        foreach (var c in _vm.Enemies)
            charStates.Add(MakeState(c));

        var layout = CombatLayout.From(
            party1.Members.Select(m => m.Character.Name),
            party2.Members.Select(m => m.Character.Name),
            isDuel: true);

        var state = new CombatDisplayState(charStates, layout, isApiMode: false);

        _presenter = new AvaloniaCombatPresenter(_vm, _displayConfig, _waitForNext, Dispatcher.UIThread, _soundPlayer)
        {
            PacingMultiplier = SpeedSlider.Value,
            AutoMode = false
        };

        var playbackToken = _cts.Token;
        _ = Task.Run(() =>
        {
            try
            {
                CombatPlaybackEngine.PlayRealTime(result, state, _presenter);
            }
            finally
            {
                if (!playbackToken.IsCancellationRequested)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _vm.IsRunning = false;
                        _vm.CombatOver = true;
                        StopBlink();
                        TurnButton.Classes.Remove("waiting");
                        TurnButton.Content = "Turn based";
                        AutoPlayButton.Classes.Remove("waiting");
                        AutoPlayButton.Content = "Auto-play";
                        TurnButton.IsVisible = false;
                        AutoPlayButton.IsVisible = false;
                        NewCombatButton.IsVisible = true;
                    });
                }
            }
        }, playbackToken);

        _vm.IsPreviewing = false;
        RunPreviewButton.IsEnabled = true;
    }

    private void OnBackToMainFromSpellPreviewClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _presenter?.Stop();
        _presenter = null;
        _cts?.Cancel();
        _waitForNext?.Set();
        _waitForNext?.Dispose();
        _cts?.Dispose();
        _cts = null;
        _waitForNext = null;

        _fromSpellPreview = false;
        _vm.Phase = _previousPhase;
        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();
        _vm.IsRunning = false;
        _vm.CombatOver = false;
        _vm.Tick = 0;
        _vm.RoundNumber = 0;
        _vm.TickInRound = 0;
        _vm.SelectedCaster = null;
        _vm.SelectedSpell = null;
        _vm.AvailableSpells.Clear();
        _vm.AvailableCasters.Clear();
    }

    private static Spell PreparePreviewSpell(Spell original)
    {
        if (original.IsHealing || original.ElementalType != ElementalType.None)
            return original;

        var mapped = MapDamageToElemental(original.DamageType);
        if (mapped == ElementalType.None)
            return original;

        return new Spell
        {
            Id = original.Id,
            Name = original.Name,
            Description = original.Description,
            School = original.School,
            ManaCost = original.ManaCost,
            TurnMeterCost = original.TurnMeterCost,
            SpellLevel = original.SpellLevel,
            DamageCount = original.DamageCount,
            DamageDie = original.DamageDie,
            DamageType = original.DamageType,
            AttackType = original.AttackType,
            AttackBonus = original.AttackBonus,
            FlatDamageBonus = original.FlatDamageBonus,
            ElementalType = mapped,
            ElementalDamage = original.ElementalDamage,
            Tags = original.Tags,
            SummonedPet = original.SummonedPet,
            OnHitEffects = original.OnHitEffects
        };
    }

    private static ElementalType MapDamageToElemental(DamageType dmg) => dmg switch
    {
        DamageType.Fire => ElementalType.Fire,
        DamageType.Ice => ElementalType.Ice,
        DamageType.Lightning => ElementalType.Lightning,
        DamageType.Acid => ElementalType.Poison,
        DamageType.Poison => ElementalType.Poison,
        DamageType.Holy => ElementalType.Holy,
        DamageType.Shadow => ElementalType.Shadow,
        _ => ElementalType.None
    };

    private static List<CharacterDisplayItem> ToDisplayItems(List<Character> characters) =>
        characters
            .Where(c => PortraitResolver.HasPortrait(c.Name))
            .Select(c => new CharacterDisplayItem(c))
            .ToList();
}