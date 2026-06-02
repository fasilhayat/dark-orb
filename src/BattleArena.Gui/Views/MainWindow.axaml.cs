using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using BattleArena.Application.Interfaces;
using BattleArena.Application.Modifiers;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;
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

    public MainWindow()
    {
        Console.WriteLine("MainWindow ctor: starting");
        try
        {
            InitializeComponent();
            Console.WriteLine("MainWindow ctor: InitializeComponent OK");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MainWindow ctor: InitializeComponent FAILED: {ex}");
            throw;
        }
        DataContext = _vm;
        Console.WriteLine("MainWindow ctor: DataContext set");
        _displayConfig = GuiDisplayConfig.Load();
        Console.WriteLine("MainWindow ctor: complete");
    }

    private async void OnStartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
    {
        if (_vm.IsRunning) return;

        _cts = new CancellationTokenSource();
        _waitForNext = new ManualResetEventSlim(false);

        _vm.IsRunning = true;
        StartButton.IsEnabled = false;
        NextButton.IsEnabled = true;
        AutoButton.IsEnabled = true;

        _vm.CombatLog.Clear();
        _vm.Heroes.Clear();
        _vm.Enemies.Clear();

        // Build character data
        var (party1, party2) = BuildDuelParties();

        // Build display states
        var maxHp = new Dictionary<string, int>();
        var heroes = new List<CharCardViewModel>();
        var enemies = new List<CharCardViewModel>();

        foreach (var pm in party1.Members)
        {
            maxHp[pm.Character.Name] = pm.Character.MaxHitPoints;
            heroes.Add(new CharCardViewModel
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
            maxHp[pm.Character.Name] = pm.Character.MaxHitPoints;
            enemies.Add(new CharCardViewModel
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

        foreach (var h in heroes) _vm.Heroes.Add(h);
        foreach (var e in enemies) _vm.Enemies.Add(e);

        var charStates = heroes.Concat(enemies)
            .Select(c => new CharDisplayState
            {
                Name = c.Name,
                MaxHp = c.MaxHp,
                Hp = c.MaxHp,
                IsHero = c.IsHero,
                Level = c.Level,
                ClassName = c.ClassName,
                Sex = c.Sex,
                Race = c.Race,
                MaxMana = c.MaxMana,
                Mana = c.Mana,
                Weapon = c.CurrentWeapon
            }).ToList();

        var layout = CombatLayout.From(
            party1.Members.Select(m => m.Character.Name),
            party2.Members.Select(m => m.Character.Name),
            isDuel: true);

        var state = new CombatDisplayState(charStates, layout);

        // Wire up services
        var diceService = new DiceService();
        var combatStats = new CombatStatsService();
        var combatService = new CombatService(diceService, combatStats, [new RangeModifier()]);
        var turnmeterService = new TurnmeterService();
        var statusEffectService = new StatusEffectService();
        var simulator = new CombatSimulator(
            combatService, turnmeterService, statusEffectService, diceService,
            new LowestHpTargetSelector(), new LowestHpTargetSelector(),
            new AutoActionDecisionSource(diceService), new AutoActionDecisionSource(diceService));

        // Run simulation
        var result = await Task.Run(() => simulator.Simulate(party1, party2, 200), _cts.Token);

        // Merge dice log
        result.Log = CombatLogMerger.Merge(result.Log, result.DiceLog);

        _waitForNext.Reset();

        _presenter = new AvaloniaCombatPresenter(_vm, _displayConfig, _waitForNext, Dispatcher.UIThread)
        {
            PacingMultiplier = SpeedSlider.Value
        };

        // Present via engine on background thread
        _ = Task.Run(() =>
        {
            try
            {
                CombatPlaybackEngine.PlayTurnBased(result, state, _presenter,
                    prepareEventState: (entry, s) =>
                    {
                        if (!string.IsNullOrWhiteSpace(entry.SummonedPetName) && s.TryGet(entry.SummonedPetName) is null)
                        {
                            var petMaxHp = 20;
                            s.EnsurePet(entry.SummonedPetName, petMaxHp, true);
                        }
                    });
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _vm.IsRunning = false;
                    StartButton.IsEnabled = true;
                    NextButton.IsEnabled = false;
                    AutoButton.IsEnabled = false;
                });
            }
        }, _cts.Token);
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
        if (_presenter is not null)
            _presenter.PacingMultiplier = e.NewValue;
    }

    private static (Party, Party) BuildDuelParties()
    {
        var longsword = new Weapon
        {
            Name = "Longsword", DamageDie = DieType.D8, DamageCount = 1,
            DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2,
            Archetype = ArchetypeWeapon.Sword, Hands = 1
        };

        var orcAxe = new Weapon
        {
            Name = "Orcish Axe", DamageDie = DieType.D10, DamageCount = 1,
            DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1,
            Archetype = ArchetypeWeapon.Axe, Hands = 1
        };

        var human = new Race { Name = "Human", BaseMovementSpeed = 30 };
        var orc = new Race { Name = "Orc", BaseMovementSpeed = 30 };

        var hero = new Character
        {
            Name = "Theron", Level = 5, Strength = 18, Dexterity = 12, Intelligence = 10,
            Race = human,
            ClassId = 8, ClassName = "Fighter", Sex = "M",
            StrikeRating = 14, TurnSpeed = 10, MaxHitPoints = 50,
            CurrentHitPoints = 50,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Chain Mail", ArmorClass = 16, Mitigation = 2,
                    MaxDexterityBonus = 6, MovementPenalty = 10 },
                RightHand = longsword
            }
        };

        var enemy = new Character
        {
            Name = "Krag", Level = 4, Strength = 17, Dexterity = 9, Intelligence = 6,
            Race = orc,
            ClassId = 1, ClassName = "Barbarian", Sex = "M",
            StrikeRating = 15, TurnSpeed = 7, MaxHitPoints = 45,
            CurrentHitPoints = 45,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Name = "Orcish Hide", ArmorClass = 12, Mitigation = 2,
                    MaxDexterityBonus = 4, MovementPenalty = 5 },
                RightHand = orcAxe
            }
        };

        return (Party.Solo(hero, longsword), Party.Solo(enemy, orcAxe));
    }
}
