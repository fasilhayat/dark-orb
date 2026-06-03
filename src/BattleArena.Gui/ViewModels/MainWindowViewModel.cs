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

    private string _phase = "MainMenu";
    public string Phase
    {
        get => _phase;
        set
        {
            if (SetField(ref _phase, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMainMenuPhase)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSetupPhase)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCombatPhase)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsApiMenuPhase)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCharCreationPhase)));
            }
        }
    }
    public bool IsMainMenuPhase => Phase == "MainMenu";
    public bool IsSetupPhase => Phase == "Setup";
    public bool IsCombatPhase => Phase == "Combat";
    public bool IsApiMenuPhase => Phase == "ApiMenu";
    public bool IsCharCreationPhase => Phase == "CharCreation";

    private string _scenario = "Duel";
    public string Scenario
    {
        get => _scenario;
        set
        {
            if (SetField(ref _scenario, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDuel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsParty)));
            }
        }
    }
    public bool IsDuel => Scenario == "Duel";
    public bool IsParty => Scenario == "Party";

    private string _mode = "TurnBased";
    public string Mode
    {
        get => _mode;
        set
        {
            if (SetField(ref _mode, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTurnBased)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAutoMode)));
            }
        }
    }
    public bool IsTurnBased => Mode == "TurnBased";
    public bool IsAutoMode => Mode == "Auto";

    // ── Character Creation ────────────────────────────────────

    public ObservableCollection<string> RaceOptions { get; } =
    [
        "Human", "Elf", "Dwarf", "Orc", "Lizard", "Kobold", "Ogre", "Gladefolk", "Half-Elf"
    ];

    public ObservableCollection<string> ClassOptions { get; } =
    [
        "Barbarian", "Knight", "Paladin", "Priest", "Mage", "Bard", "Druid", "Fighter", "Rogue"
    ];

    private string _charName = "";
    public string CharName
    {
        get => _charName;
        set => SetField(ref _charName, value);
    }

    private int _selectedRaceIndex = -1;
    public int SelectedRaceIndex
    {
        get => _selectedRaceIndex;
        set
        {
            if (SetField(ref _selectedRaceIndex, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRace)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRollStats)));
            }
        }
    }
    public string SelectedRace => SelectedRaceIndex >= 0 && SelectedRaceIndex < RaceOptions.Count
        ? RaceOptions[SelectedRaceIndex] : "";

    private int _selectedClassIndex = -1;
    public int SelectedClassIndex
    {
        get => _selectedClassIndex;
        set
        {
            if (SetField(ref _selectedClassIndex, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedClass)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRollStats)));
            }
        }
    }
    public string SelectedClass => SelectedClassIndex >= 0 && SelectedClassIndex < ClassOptions.Count
        ? ClassOptions[SelectedClassIndex] : "";

    public bool CanRollStats => SelectedRaceIndex >= 0 && SelectedClassIndex >= 0;

    private int _charStr;
    public int CharStr
    {
        get => _charStr;
        set
        {
            if (SetField(ref _charStr, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrDisplay)));
            }
        }
    }

    private int _charStrExceptional;
    public int CharStrExceptional
    {
        get => _charStrExceptional;
        set
        {
            if (SetField(ref _charStrExceptional, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrDisplay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrMod)));
            }
        }
    }
    public string CharStrDisplay => CharStrExceptional > 0
        ? $"{CharStr}/{FormatExceptional(CharStrExceptional)}" : CharStr.ToString();

    private static string FormatExceptional(int pct) => pct == 100 ? "00" : pct.ToString("00");

    private int _charDex;
    public int CharDex
    {
        get => _charDex;
        set
        {
            if (SetField(ref _charDex, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharDexMod)));
        }
    }

    private int _charSta;
    public int CharSta
    {
        get => _charSta;
        set
        {
            if (SetField(ref _charSta, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStaMod)));
        }
    }

    private int _charInt;
    public int CharInt
    {
        get => _charInt;
        set
        {
            if (SetField(ref _charInt, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharIntMod)));
        }
    }

    private int _charWis;
    public int CharWis
    {
        get => _charWis;
        set
        {
            if (SetField(ref _charWis, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharWisMod)));
        }
    }

    private int _charCha;
    public int CharCha
    {
        get => _charCha;
        set
        {
            if (SetField(ref _charCha, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharChaMod)));
        }
    }

    public string CharStrMod => $"({(CharStr - 10) / 2:+#;-#;0})";
    public string CharDexMod => $"({(CharDex - 10) / 2:+#;-#;0})";
    public string CharStaMod => $"({(CharSta - 10) / 2:+#;-#;0})";
    public string CharIntMod => $"({(CharInt - 10) / 2:+#;-#;0})";
    public string CharWisMod => $"({(CharWis - 10) / 2:+#;-#;0})";
    public string CharChaMod => $"({(CharCha - 10) / 2:+#;-#;0})";

    private bool _isApiMode;
    public bool IsApiMode
    {
        get => _isApiMode;
        set => SetField(ref _isApiMode, value);
    }

    private bool _isApiReachable = true;
    public bool IsApiReachable
    {
        get => _isApiReachable;
        set
        {
            if (SetField(ref _isApiReachable, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsApiUnreachable)));
        }
    }
    public bool IsApiUnreachable => !IsApiReachable;

    private string _fighter1Name = "";
    public string Fighter1Name
    {
        get => _fighter1Name;
        set
        {
            if (SetField(ref _fighter1Name, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStartCombat)));
        }
    }

    private string _fighter2Name = "";
    public string Fighter2Name
    {
        get => _fighter2Name;
        set
        {
            if (SetField(ref _fighter2Name, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStartCombat)));
        }
    }

    public bool CanStartCombat => !string.IsNullOrEmpty(Fighter1Name) && !string.IsNullOrEmpty(Fighter2Name);

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
    public double HpEmptyFraction => 1.0 - HpFraction;
    public double TmFraction => Math.Clamp((double)Tm / 100, 0, 1);
    public double ManaFraction => MaxMana > 0 ? Math.Clamp((double)Math.Max(0, Mana) / MaxMana, 0, 1) : 0;
    public double ManaEmptyFraction => 1.0 - ManaFraction;

    public string HpDisplay => IsDead ? "" : $"{Math.Max(0, Hp)}/{MaxHp}";
    public string TmDisplay => $"{Tm}";
    public string ManaDisplay => MaxMana > 0 ? $"{Math.Max(0, Mana)}" : "--";
    public string ActiveIndicator => IsDead ? "  " : "\u25b6 ";
    public string PortraitInitial =>
        string.IsNullOrWhiteSpace(Name) ? "?" : Name.TrimStart()[0].ToString().ToUpperInvariant();

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
                Raise(nameof(HpEmptyFraction));
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
                Raise(nameof(ManaEmptyFraction));
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
