using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BattleArena.Core.Entities;
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

    public ObservableCollection<string> AvailableClasses { get; } =
    [
        "Barbarian", "Knight", "Paladin", "Priest", "Mage", "Bard", "Druid", "Fighter", "Rogue", "Ranger"
    ];

    public ObservableCollection<string> AvailableRaces { get; } = [];
    public ObservableCollection<string> AvailableSubraces { get; } = [];

    private static readonly Dictionary<string, string[]> ClassRaceRestrictions = new()
    {
        ["Barbarian"] = ["Human", "Orc", "Ogre", "Dwarf"],
        ["Knight"]    = ["Human", "Elf", "Dwarf", "Orc"],
        ["Paladin"]   = ["Human", "Elf", "Dwarf"],
        ["Priest"]    = ["Human", "Elf", "Dwarf", "Lizard", "Kobold", "Gladefolk", "Orc"],
        ["Mage"]      = ["Human", "Elf", "Kobold"],
        ["Bard"]      = ["Human", "Elf", "Gladefolk"],
        ["Druid"]     = ["Human", "Elf", "Gladefolk", "Lizard"],
        ["Fighter"]   = ["Human", "Elf", "Dwarf", "Lizard", "Kobold", "Orc", "Ogre", "Gladefolk"],
        ["Rogue"]     = ["Human", "Elf", "Dwarf", "Gladefolk", "Kobold"],
        ["Ranger"]    = ["Human", "Elf", "Dwarf", "Gladefolk", "Half-Elf"]
    };

    private static readonly Dictionary<string, string[]> RaceSubraceLookup = new()
    {
        ["Human"]     = [],
        ["Elf"]       = ["High Elf", "Dark Elf", "Forest Elf"],
        ["Dwarf"]     = ["Mountain Dwarf", "Hill Dwarf"],
        ["Lizard"]    = ["Swamp Lizard", "Desert Lizard", "Forest Lizard"],
        ["Kobold"]    = ["Cave Kobold", "Desert Kobold", "Swamp Kobold", "Forest Kobold"],
        ["Orc"]       = ["Green Orc", "Blue Orc", "Red Orc"],
        ["Ogre"]      = ["Mountain Ogre", "Hill Ogre", "Desert Ogre", "Forest Ogre"],
        ["Gladefolk"] = ["Forest Gladefolk", "Hill Gladefolk"],
        ["Half-Elf"]  = ["Half-High-Elf", "Half-Wood-Elf"]
    };

    private string _charName = "";
    public string CharName
    {
        get => _charName;
        set
        {
            if (SetField(ref _charName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreate)));
            }
        }
    }

    private string? _selectedClassName;
    public string? SelectedClassName
    {
        get => _selectedClassName;
        set
        {
            if (SetField(ref _selectedClassName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedClass)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMageSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPriestSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedClassFullInfo)));
                SelectedRaceName = null;
                SelectedSubraceName = null;
                SelectedSchool = null;
                SelectedDeity = null;
                AvailableRaces.Clear();
                AvailableSubraces.Clear();
                AvailableDeities.Clear();
                AvailableSchools.Clear();
                if (value is not null && ClassRaceRestrictions.TryGetValue(value, out var races))
                    foreach (var r in races)
                        AvailableRaces.Add(r);
                // Populate school/deity options based on class
                if (value == "Mage")
                    foreach (var s in LoadedSchools)
                        AvailableSchools.Add(s.Name);
                else if (value == "Priest")
                    foreach (var d in LoadedDeities)
                        AvailableDeities.Add(d.Name);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRollStats)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }
    public string SelectedClass => SelectedClassName ?? "";

    public bool IsMageSelected => string.Equals(SelectedClassName, "Mage", StringComparison.OrdinalIgnoreCase);
    public bool IsPriestSelected => string.Equals(SelectedClassName, "Priest", StringComparison.OrdinalIgnoreCase);

    public string SelectedClassFullInfo
    {
        get
        {
            if (SelectedClassName is null) return "";
            var pc = LoadedClasses.FirstOrDefault(c =>
                c.Name.Equals(SelectedClassName, StringComparison.OrdinalIgnoreCase));
            if (pc is null) return "";
            return $"{pc.Description}\nHit Die: D{(int)pc.HitDie}  ·  Strike Rating: {pc.BaseStrikeRating}";
        }
    }
 
    private string? _selectedRaceName;
    public string? SelectedRaceName
    {
        get => _selectedRaceName;
        set
        {
            if (SetField(ref _selectedRaceName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRace)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRaceFullInfo)));
                SelectedSubraceName = null;
                AvailableSubraces.Clear();
                if (value is not null && RaceSubraceLookup.TryGetValue(value, out var subraces))
                    foreach (var s in subraces)
                        AvailableSubraces.Add(s);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSubraces)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRollStats)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }
    public string SelectedRace => SelectedRaceName ?? "";

    public string SelectedRaceFullInfo
    {
        get
        {
            if (SelectedRaceName is null) return "";
            var r = LoadedRaces.FirstOrDefault(race =>
                race.Name.Equals(SelectedRaceName, StringComparison.OrdinalIgnoreCase));
            if (r is null) return "";
            return r.Description;
        }
    }
 
    private string? _selectedSubraceName;
    public string? SelectedSubraceName
    {
        get => _selectedSubraceName;
        set
        {
            if (SetField(ref _selectedSubraceName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubrace)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubraceFullInfo)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
            }
        }
    }
    public string SelectedSubrace => SelectedSubraceName ?? "";

    public string SelectedSubraceFullInfo
    {
        get
        {
            if (SelectedSubraceName is null) return "";
            var sr = LoadedSubraces.FirstOrDefault(s =>
                s.Name.Equals(SelectedSubraceName, StringComparison.OrdinalIgnoreCase));
            if (sr is null) return "";
            return sr.Description;
        }
    }

    public bool CanRollStats => SelectedClassName is not null && SelectedRaceName is not null;

    public string CharCreationSummary
    {
        get
        {
            var cls = SelectedClassName ?? "?";
            var school = SelectedSchool is not null ? $" [{SelectedSchool}]" : "";
            var deity = SelectedDeity is not null ? $" [{SelectedDeity}]" : "";
            var race = SelectedRaceName ?? "?";
            var sub = SelectedSubraceName is not null ? $" ({SelectedSubraceName})" : "";
            var strInfo = CharStrExceptional > 0 ? $"{CharStr}/{CharStrExceptional:00}" : CharStr.ToString();
            return $"{cls}{school}{deity}  ·  {race}{sub}  ·  STR {strInfo}  DEX {CharDex}  STA {CharSta}  INT {CharInt}  WIS {CharWis}  CHA {CharCha}";
        }
    }

    private int _charStr;
    public int CharStr
    {
        get => _charStr;
        set
        {
            if (SetField(ref _charStr, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStrDisplay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
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
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharDexMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }

    private int _charSta;
    public int CharSta
    {
        get => _charSta;
        set
        {
            if (SetField(ref _charSta, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharStaMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }

    private int _charInt;
    public int CharInt
    {
        get => _charInt;
        set
        {
            if (SetField(ref _charInt, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharIntMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }

    private int _charWis;
    public int CharWis
    {
        get => _charWis;
        set
        {
            if (SetField(ref _charWis, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharWisMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }

    private int _charCha;
    public int CharCha
    {
        get => _charCha;
        set
        {
            if (SetField(ref _charCha, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharChaMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
            }
        }
    }

    public string CharStrMod => $"({(CharStr - 10) / 2:+#;-#;0})";
    public string CharDexMod => $"({(CharDex - 10) / 2:+#;-#;0})";
    public string CharStaMod => $"({(CharSta - 10) / 2:+#;-#;0})";
    public string CharIntMod => $"({(CharInt - 10) / 2:+#;-#;0})";
    public string CharWisMod => $"({(CharWis - 10) / 2:+#;-#;0})";
    public string CharChaMod => $"({(CharCha - 10) / 2:+#;-#;0})";

    // ── Character Creation Step Wizard ─────────────────────────

    private int _creationStep;
    public int CreationStep
    {
        get => _creationStep;
        set
        {
            if (SetField(ref _creationStep, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStepName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStepClass)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStepRace)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStepSubrace)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStepStats)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreate)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNextButton)));
            }
        }
    }
    public bool IsStepClass => _creationStep == 0;
    public bool IsStepRace => _creationStep == 1;
    public bool IsStepSubrace => _creationStep == 2;
    public bool IsStepStats => _creationStep == 3;
    public bool IsStepName => _creationStep == 4;
    public bool CanGoBack => _creationStep > 0;
    public bool CanGoNext => _creationStep switch
    {
        0 => SelectedClassName is not null
            && (!IsMageSelected || SelectedSchool is not null)
            && (!IsPriestSelected || SelectedDeity is not null),
        1 => SelectedRaceName is not null,
        2 => SelectedSubraceName is not null,
        3 => CharStr > 0,
        4 => false,
        _ => false,
    };

    public bool ShowNextButton => _creationStep != 4;

    public bool CanCreate => _creationStep == 4 && !string.IsNullOrWhiteSpace(CharName);

    public bool HasSubraces => AvailableSubraces.Count > 0;

    public List<Race> LoadedRaces { get; set; } = [];
    public List<Subrace> LoadedSubraces { get; set; } = [];
    public List<PlayerClass> LoadedClasses { get; set; } = [];
    public List<Deity> LoadedDeities { get; set; } = [];
    public List<SpellSchoolInfo> LoadedSchools { get; set; } = [];

    public ObservableCollection<string> AvailableDeities { get; } = [];
    public ObservableCollection<string> AvailableSchools { get; } = [];

    private string? _selectedSchool;
    public string? SelectedSchool
    {
        get => _selectedSchool;
        set
        {
            if (SetField(ref _selectedSchool, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSchoolInfo)));
            }
        }
    }

    public string SelectedSchoolInfo
    {
        get
        {
            if (SelectedSchool is null) return "";
            var s = LoadedSchools.FirstOrDefault(ss =>
                ss.Name.Equals(SelectedSchool, StringComparison.OrdinalIgnoreCase));
            if (s is null) return "";
            return s.Description;
        }
    }

    private string? _selectedDeity;
    public string? SelectedDeity
    {
        get => _selectedDeity;
        set
        {
            if (SetField(ref _selectedDeity, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharCreationSummary)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNext)));
            }
        }
    }

    public string SelectedDeityInfo
    {
        get
        {
            if (SelectedDeity is null) return "";
            var d = LoadedDeities.FirstOrDefault(dd =>
                dd.Name.Equals(SelectedDeity, StringComparison.OrdinalIgnoreCase));
            if (d is null) return "";
            return $"{d.Description}\nDomain: {d.Domain}";
        }
    }

    // ── Overlay animation state ──────────────────────────────

    private string _overlayText = "";
    public string OverlayText
    {
        get => _overlayText;
        set => SetField(ref _overlayText, value);
    }

    private double _overlayOpacity;
    public double OverlayOpacity
    {
        get => _overlayOpacity;
        set => SetField(ref _overlayOpacity, value);
    }

    private double _overlayScale = 1.0;
    public double OverlayScale
    {
        get => _overlayScale;
        set => SetField(ref _overlayScale, value);
    }

    private double _overlayTrailScale1;
    public double OverlayTrailScale1
    {
        get => _overlayTrailScale1;
        set => SetField(ref _overlayTrailScale1, value);
    }

    private double _overlayTrailOpacity1;
    public double OverlayTrailOpacity1
    {
        get => _overlayTrailOpacity1;
        set => SetField(ref _overlayTrailOpacity1, value);
    }

    private double _overlayTrailScale2;
    public double OverlayTrailScale2
    {
        get => _overlayTrailScale2;
        set => SetField(ref _overlayTrailScale2, value);
    }

    private double _overlayTrailOpacity2;
    public double OverlayTrailOpacity2
    {
        get => _overlayTrailOpacity2;
        set => SetField(ref _overlayTrailOpacity2, value);
    }

    public bool HasOverlay => !string.IsNullOrEmpty(OverlayText);

    public void TriggerOverlay(string text)
    {
        OverlayText = text;
        OverlayOpacity = 1.0;
        OverlayScale = 0.25;
        OverlayTrailScale1 = 0.2;
        OverlayTrailOpacity1 = 0.35;
        OverlayTrailScale2 = 0.14;
        OverlayTrailOpacity2 = 0.15;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasOverlay)));
    }

    public void ClearOverlay()
    {
        OverlayText = "";
        OverlayOpacity = 0;
        OverlayScale = 1.0;
        OverlayTrailScale1 = 0;
        OverlayTrailOpacity1 = 0;
        OverlayTrailScale2 = 0;
        OverlayTrailOpacity2 = 0;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasOverlay)));
    }

    // ── API mode ─────────────────────────────────────────────

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

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetField(ref _errorMessage, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasError)));
            }
        }
    }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _canProceed;
    public bool CanProceed
    {
        get => _canProceed;
        set => SetField(ref _canProceed, value);
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
    private const int TmTotalPipes = 73;
    private static readonly IBrush TmPipeFilled = new SolidColorBrush(Color.Parse("#00bfff"));
    private static readonly IBrush TmPipeEmpty = new SolidColorBrush(Color.Parse("#1a1a2e"));
    private static readonly IBrush TmPipeLocked = new SolidColorBrush(Color.Parse("#ffffff"));

    private static string GetEffectColor(string name)
    {
        if (CcVisualConfig.IsCcEffect(name))
            return CcVisualConfig.GetColor(name);
        return name switch
        {
            "Burning"  => "#ff6600",
            "Ignite"   => "#ff4400",
            "Frozen"   => "#44ccff",
            "Freeze"   => "#44ccff",
            "Shocked"  => "#ffff44",
            "Poisoned" => "#44ff44",
            "Bleeding" => "#ff4444",
            _          => "#88ccff",
        };
    }

    public ObservableCollection<IBrush> TmPipes { get; }

    public string Name { get; init; } = "";
    public bool IsHero { get; init; }
    public int MaxHp { get; init; }
    public int MaxMana { get; init; }
    public string ClassName { get; init; } = "";
    public string Race { get; init; } = "";
    public string Sex { get; init; } = "";
    public int Level { get; init; }
    public int StrikeRating { get; init; }
    public string ArmorName { get; init; } = "";
    public int ArmorClass { get; init; }
    public string WeaponStats { get; init; } = "";
    public int MagicResistance { get; init; }
    public Bitmap? Portrait { get; init; }

    public bool HasPortrait => Portrait is not null;
    public bool PortraitIsNull => Portrait is null;

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

    private bool _isTmLocked;
    public bool IsTmLocked
    {
        get => _isTmLocked;
        set => SetField(ref _isTmLocked, value);
    }

    private string? _ccStatus;
    public string? CcStatus
    {
        get => _ccStatus;
        set => SetField(ref _ccStatus, value);
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
    public string StatusLine => IsDead ? (Hp <= -10 ? "SLAIN" : "UNCONSCIOUS") : "";
    public bool HasStatusOverlay => !string.IsNullOrEmpty(StatusLine);

    public double HpFraction => MaxHp > 0 ? Math.Clamp((double)Math.Max(0, Hp) / MaxHp, 0, 1) : 0;
    public double TmFraction => Math.Clamp((double)Tm / 100, 0, 1);
    public double ManaFraction => MaxMana > 0 ? Math.Clamp((double)Math.Max(0, Mana) / MaxMana, 0, 1) : 0;

    public string HpDisplay => IsDead ? "" : $"{Math.Max(0, Hp)}/{MaxHp}";
    public string TmDisplay => $"{Tm}";
    public string TmBorderBrush => PersistentBorderColor ?? (IsTmLocked ? "#ffffff" : "#333");

    public string ManaDisplay => MaxMana > 0 ? $"{Math.Max(0, Mana)}" : "--";
    public string ActiveIndicator => IsDead ? "  " : "\u25b6 ";
    public string PortraitInitial =>
        string.IsNullOrWhiteSpace(Name) ? "?" : Name.TrimStart()[0].ToString().ToUpperInvariant();

    public string InfoLine => $"{SexDisplay} \u00b7 Lvl {Level,2} {Race} \u00b7 {ClassName}";

    public string ArmorLine => $"{ArmorName} (AC {ArmorClass})";
    public string StrikeLine => $"Strike: {StrikeRating}";
    public string WeaponLine => $"Weapon: {WeaponStats}";
    public string ResistLine => $"Magic Res: {MagicResistance}";
    public string? EffectsDisplay
    {
        get
        {
            if (!string.IsNullOrEmpty(ActiveEffects))
                return $"({ActiveEffects})";
            if (CcStatus is { } s)
                return $"({s})";
            return null;
        }
    }
    public bool HasEffectsDisplay => EffectsDisplay is not null;

    public string EffectsColor
    {
        get
        {
            if (!string.IsNullOrEmpty(ActiveEffects))
                return GetEffectColor(ActiveEffects.Split(',')[0].Trim());
            if (CcStatus is { } s)
                return GetEffectColor(s);
            return "#88ccff";
        }
    }

    private string? _borderFlashColor;
    public string? BorderFlashColor
    {
        get => _borderFlashColor;
        set => SetField(ref _borderFlashColor, value);
    }

    private string? _persistentBorderColor;
    public string? PersistentBorderColor
    {
        get => _persistentBorderColor;
        set => SetField(ref _persistentBorderColor, value);
    }

    public string BorderColor => BorderFlashColor ?? PersistentBorderColor ?? (IsTmLocked ? "#88ccff" : IsDead ? "#666" : IsHero ? "#4488ff" : "#ff4488");
    public string NameColor => IsDead ? "#888" : IsHero ? "#88bbff" : "#ff8888";

    private double _healGlowOpacity;
    public double HealGlowOpacity
    {
        get => _healGlowOpacity;
        set => SetField(ref _healGlowOpacity, value);
    }

    private double _healGlowFraction;
    public double HealGlowFraction
    {
        get => _healGlowFraction;
        set
        {
            var changed = !_healGlowFraction.Equals(value);
            SetField(ref _healGlowFraction, value);
            if (changed) RaiseHealGlowRemainder();
        }
    }

    private double _healGlowStart;
    public double HealGlowStart
    {
        get => _healGlowStart;
        set
        {
            var changed = !_healGlowStart.Equals(value);
            SetField(ref _healGlowStart, value);
            if (changed) RaiseHealGlowRemainder();
        }
    }

    public double HealGlowRemainder => 1.0 - _healGlowStart - _healGlowFraction;

    private void RaiseHealGlowRemainder()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HealGlowRemainder)));
    }

    // ── Mana Drain Overlay (leech target) ──────────────────

    private double _manaDrainOpacity;
    public double ManaDrainOpacity
    {
        get => _manaDrainOpacity;
        set => SetField(ref _manaDrainOpacity, value);
    }

    private double _manaDrainFraction;
    public double ManaDrainFraction
    {
        get => _manaDrainFraction;
        set
        {
            var changed = !_manaDrainFraction.Equals(value);
            SetField(ref _manaDrainFraction, value);
            if (changed) RaiseManaDrainRemainder();
        }
    }

    private double _manaDrainStart;
    public double ManaDrainStart
    {
        get => _manaDrainStart;
        set
        {
            var changed = !_manaDrainStart.Equals(value);
            SetField(ref _manaDrainStart, value);
            if (changed) RaiseManaDrainRemainder();
        }
    }

    public double ManaDrainRemainder => 1.0 - _manaDrainStart - _manaDrainFraction;

    private void RaiseManaDrainRemainder()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManaDrainRemainder)));
    }

    // ── Mana Gain Glow (leech caster) ──────────────────────

    private double _manaGainOpacity;
    public double ManaGainOpacity
    {
        get => _manaGainOpacity;
        set => SetField(ref _manaGainOpacity, value);
    }

    private double _manaGainFraction;
    public double ManaGainFraction
    {
        get => _manaGainFraction;
        set
        {
            var changed = !_manaGainFraction.Equals(value);
            SetField(ref _manaGainFraction, value);
            if (changed) RaiseManaGainRemainder();
        }
    }

    private double _manaGainStart;
    public double ManaGainStart
    {
        get => _manaGainStart;
        set
        {
            var changed = !_manaGainStart.Equals(value);
            SetField(ref _manaGainStart, value);
            if (changed) RaiseManaGainRemainder();
        }
    }

    public double ManaGainRemainder => 1.0 - _manaGainStart - _manaGainFraction;

    private void RaiseManaGainRemainder()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManaGainRemainder)));
    }

    // Mana gain color: animates from light purple to the mana bar color
    private string _manaGainColor = "#cc88ff";
    public string ManaGainColor
    {
        get => _manaGainColor;
        set => SetField(ref _manaGainColor, value);
    }

    // ── Mana Cost Preview (pre-deduction glow) ─────────────

    private double _manaCostPreviewOpacity;
    public double ManaCostPreviewOpacity
    {
        get => _manaCostPreviewOpacity;
        set => SetField(ref _manaCostPreviewOpacity, value);
    }

    private double _manaCostPreviewFraction;
    public double ManaCostPreviewFraction
    {
        get => _manaCostPreviewFraction;
        set
        {
            var changed = !_manaCostPreviewFraction.Equals(value);
            SetField(ref _manaCostPreviewFraction, value);
            if (changed) RaiseManaCostPreviewRemainder();
        }
    }

    private double _manaCostPreviewStart;
    public double ManaCostPreviewStart
    {
        get => _manaCostPreviewStart;
        set
        {
            var changed = !_manaCostPreviewStart.Equals(value);
            SetField(ref _manaCostPreviewStart, value);
            if (changed) RaiseManaCostPreviewRemainder();
        }
    }

    public double ManaCostPreviewRemainder => 1.0 - _manaCostPreviewStart - _manaCostPreviewFraction;

    private void RaiseManaCostPreviewRemainder()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManaCostPreviewRemainder)));
    }

    // ── Damage Preview properties ──────────────────────────

    private double _damagePreviewOpacity;
    public double DamagePreviewOpacity
    {
        get => _damagePreviewOpacity;
        set => SetField(ref _damagePreviewOpacity, value);
    }

    private double _damagePreviewFraction;
    public double DamagePreviewFraction
    {
        get => _damagePreviewFraction;
        set
        {
            var changed = !_damagePreviewFraction.Equals(value);
            SetField(ref _damagePreviewFraction, value);
            if (changed) RaiseDamagePreviewRemainder();
        }
    }

    private double _damagePreviewStart;
    public double DamagePreviewStart
    {
        get => _damagePreviewStart;
        set
        {
            var changed = !_damagePreviewStart.Equals(value);
            SetField(ref _damagePreviewStart, value);
            if (changed) RaiseDamagePreviewRemainder();
        }
    }

    public double DamagePreviewRemainder => 1.0 - _damagePreviewStart - _damagePreviewFraction;

    private void RaiseDamagePreviewRemainder()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DamagePreviewRemainder)));
    }

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
        IsTmLocked = s.IsTmLocked;
        CcStatus = s.CcStatus;
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
            case nameof(BorderFlashColor):
                Raise(nameof(BorderColor));
                break;
            case nameof(PersistentBorderColor):
                Raise(nameof(BorderColor));
                Raise(nameof(TmBorderBrush));
                UpdateTmPipes();
                break;
            case nameof(ActiveEffects):
                Raise(nameof(EffectsDisplay));
                Raise(nameof(HasEffectsDisplay));
                Raise(nameof(EffectsColor));
                break;
            case nameof(CurrentWeapon):
                break;
            case nameof(IsTmLocked):
                Raise(nameof(TmBorderBrush));
                Raise(nameof(BorderColor));
                UpdateTmPipes();
                break;
            case nameof(CcStatus):
                Raise(nameof(EffectsDisplay));
                Raise(nameof(HasEffectsDisplay));
                Raise(nameof(EffectsColor));
                break;
            case nameof(DamagePreviewOpacity):
                break;
        }

        void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    private void UpdateTmPipes()
    {
        var fillCount = (int)Math.Round(TmFraction * TmTotalPipes);
        IBrush? lockFill = PersistentBorderColor is not null
            ? new SolidColorBrush(Color.Parse(PersistentBorderColor))
            : null;

        for (var i = 0; i < TmTotalPipes; i++)
        {
            IBrush brush;
            if (IsTmLocked && i < fillCount)
                brush = lockFill ?? TmPipeLocked;
            else if (IsTmLocked)
                brush = TmPipeEmpty;
            else if (i < fillCount)
                brush = TmPipeFilled;
            else
                brush = TmPipeEmpty;

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
