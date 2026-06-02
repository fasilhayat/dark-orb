using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using BattleArena.Presentation;

namespace BattleArena.Gui.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<CharCardViewModel> Heroes { get; } = [];
    public ObservableCollection<CharCardViewModel> Enemies { get; } = [];
    public ObservableCollection<LogEntryViewModel> CombatLog { get; } = [];

    private int _tick;
    public int Tick
    {
        get => _tick;
        set => SetField(ref _tick, value);
    }

    private int _roundNumber;
    public int RoundNumber
    {
        get => _roundNumber;
        set
        {
            if (SetField(ref _roundNumber, value))
                NotifyRoundProps();
        }
    }

    private int _tickInRound;
    public int TickInRound
    {
        get => _tickInRound;
        set
        {
            if (SetField(ref _tickInRound, value))
                NotifyRoundProps();
        }
    }

    public double RoundProgress => Math.Clamp((double)TickInRound / 10, 0, 1);
    public double RoundRemainder => 1.0 - RoundProgress;
    public string RoundBarHeader => $"ROUND {RoundNumber}  —  {TickInRound}/10 ticks";

    private void NotifyRoundProps()
    {
        var e = new PropertyChangedEventArgs(nameof(RoundBarHeader));
        PropertyChanged?.Invoke(this, e);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundProgress)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundRemainder)));
    }

    private string _activeActorName = "";
    public string ActiveActorName
    {
        get => _activeActorName;
        set => SetField(ref _activeActorName, value);
    }

    private bool _combatOver;
    public bool CombatOver
    {
        get => _combatOver;
        set => SetField(ref _combatOver, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set => SetField(ref _isRunning, value);
    }

    public void UpdateFromState(CombatDisplayState state, int tick)
    {
        Tick = tick;
        TickInRound = (tick - 1) % 10 + 1;
        RoundNumber = (tick - 1) / 10 + 1;

        foreach (var kvp in state.All)
        {
            var card = FindCard(kvp.Key);
            if (card is not null)
                card.UpdateFrom(kvp.Value);
        }
    }

    public void AddLogEntry(IReadOnlyList<LogSegment> segments)
    {
        CombatLog.Add(new LogEntryViewModel { Segments = segments });
    }

    private static IBrush MakeBrush(string hex) => new SolidColorBrush(Color.Parse(hex));

    private CharCardViewModel? FindCard(string name)
    {
        foreach (var h in Heroes)
            if (h.Name == name) return h;
        foreach (var e in Enemies)
            if (e.Name == name) return e;
        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        return true;
    }
}

public sealed class CharCardViewModel : INotifyPropertyChanged
{
    private const int TmTotalPipes = 64;
    private static readonly IBrush TmPipeFilled = new SolidColorBrush(Color.Parse("#00bfff"));
    private static readonly IBrush TmPipeEmpty = new SolidColorBrush(Color.Parse("#1a1a2e"));

    public string Name { get; init; } = "";
    public bool IsHero { get; init; }
    public int MaxHp { get; init; }
    public int MaxMana { get; init; }
    public string ClassName { get; init; } = "";
    public string Race { get; init; } = "";
    public string Sex { get; init; } = "";
    public int Level { get; init; }

    public ObservableCollection<IBrush> TmPipes { get; }

    public CharCardViewModel()
    {
        TmPipes = new ObservableCollection<IBrush>();
        for (var i = 0; i < TmTotalPipes; i++)
            TmPipes.Add(TmPipeEmpty);
    }

    private int _hp;
    public int Hp
    {
        get => _hp;
        set => SetField(ref _hp, value);
    }

    private int _tm;
    public int Tm
    {
        get => _tm;
        set => SetField(ref _tm, value);
    }

    private int _mana;
    public int Mana
    {
        get => _mana;
        set => SetField(ref _mana, value);
    }

    private bool _isAlive = true;
    public bool IsAlive
    {
        get => _isAlive;
        set => SetField(ref _isAlive, value);
    }

    private string _currentWeapon = "";
    public string CurrentWeapon
    {
        get => _currentWeapon;
        set => SetField(ref _currentWeapon, value);
    }

    private string _activeEffects = "";
    public string ActiveEffects
    {
        get => _activeEffects;
        set => SetField(ref _activeEffects, value);
    }

    // Computed display properties

    public bool IsDead => !IsAlive;
    public string StatusLine => IsDead ? (Hp <= -10 ? "SLAIN" : "UNCONSCIOUS") : ActiveEffects;
    public bool HasStatusOverlay => !string.IsNullOrEmpty(StatusLine);

    public double HpFraction => MaxHp > 0 ? Math.Clamp((double)Math.Max(0, Hp) / MaxHp, 0, 1) : 0;
    public double TmFraction => Math.Clamp((double)Tm / 100, 0, 1);
    public double ManaFraction => MaxMana > 0 ? Math.Clamp((double)Math.Max(0, Mana) / MaxMana, 0, 1) : 0;

    public string HpDisplay => IsDead ? "0%" : $"{Math.Max(0, Hp) * 100 / MaxHp}%";
    public string TmDisplay => $"{Tm}%";
    public string ManaDisplay => MaxMana > 0 ? $"{Math.Max(0, Mana) * 100 / MaxMana}%" : "--";
    public string ActiveIndicator => IsDead ? "  " : "\u25b6 ";
    public string InfoLine => $"{SexDisplay} \u00b7 Lvl {Level,2} {Race} \u00b7 {ClassName}";

    public string BorderColor => IsDead ? "#666" : IsHero ? "#4488ff" : "#ff4488";
    public string NameColor => IsDead ? "#888" : IsHero ? "#88bbff" : "#ff8888";

    public string HpBarColor
    {
        get
        {
            if (IsDead) return "#666";
            var frac = (double)Math.Max(0, Hp) / Math.Max(1, MaxHp);
            return frac > 0.5 ? "#44cc44" : frac > 0.25 ? "#d4a017" : "#ff4444";
        }
    }

    public string ManaBarColor => MaxMana > 0 ? "#cc44cc" : "#444";
    public string ManaLabelColor => MaxMana > 0 ? "#999" : "#444";

    private string SexDisplay => Sex switch { "F" => "Female", "M" => "Male", _ => "None" };

    public void UpdateFrom(CharDisplayState s)
    {
        Hp = Math.Max(0, s.Hp);
        Tm = Math.Min(100, s.Tm);
        Mana = Math.Max(0, s.Mana);
        IsAlive = s.IsAlive;
        ActiveEffects = s.ActiveEffects.Count > 0 ? string.Join(", ", s.ActiveEffects) : "";
        if (!string.IsNullOrEmpty(s.Weapon))
            CurrentWeapon = s.Weapon;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        NotifyDerived(prop);
    }

    private void NotifyDerived(string? prop)
    {
        switch (prop)
        {
            case nameof(Hp):
                Raise(nameof(HpFraction));
                Raise(nameof(HpDisplay));
                Raise(nameof(HpBarColor));
                break;
            case nameof(Tm):
                Raise(nameof(TmFraction));
                Raise(nameof(TmDisplay));
                UpdateTmPipes();
                break;
            case nameof(Mana):
                Raise(nameof(ManaFraction));
                Raise(nameof(ManaDisplay));
                break;
            case nameof(IsAlive):
                Raise(nameof(IsDead));
                Raise(nameof(ActiveIndicator));
                Raise(nameof(BorderColor));
                Raise(nameof(NameColor));
                Raise(nameof(HpBarColor));
                Raise(nameof(HpDisplay));
                Raise(nameof(StatusLine));
                Raise(nameof(HasStatusOverlay));
                break;
            case nameof(ActiveEffects):
                Raise(nameof(StatusLine));
                Raise(nameof(HasStatusOverlay));
                break;
            case nameof(CurrentWeapon):
                break;
        }

        void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    private void UpdateTmPipes()
    {
        var fillCount = (int)Math.Round(TmFraction * TmTotalPipes);
        for (var i = 0; i < TmTotalPipes; i++)
        {
            var brush = i < fillCount ? TmPipeFilled : TmPipeEmpty;
            if (!ReferenceEquals(TmPipes[i], brush))
                TmPipes[i] = brush;
        }
    }
}

public sealed class LogSegment
{
    public string Text { get; init; } = "";
    public IBrush? Brush { get; init; }
}

public sealed class LogEntryViewModel
{
    public IReadOnlyList<LogSegment> Segments { get; init; } = Array.Empty<LogSegment>();
}
