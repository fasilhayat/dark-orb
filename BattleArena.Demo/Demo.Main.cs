namespace BattleArena.Demo;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using BattleArena.Presentation;
using System.IO;
using Core.Entities;
using Core.Entities.Enums;
using Microsoft.Extensions.Configuration;

static partial class Demo
{
    // ── Services ──────────────────────────────────────────────────────────────────
    private static readonly CombatStatsService CombatStats = new();

    // ── GUI display configuration (loaded from gui-display-contract.json) ─────────
    internal static GuiDisplayConfig DisplayConfig { get; private set; } = GuiDisplayConfig.Default;

    // ── Optional API connection ───────────────────────────────────────────────────
    private static BattleArenaApiClient? ApiClient;
    private static ApiDiceService? _apiDiceService;
    private static IDiceService? _diceService;
    private static List<Character> ApiRoster = [];
    private static List<Weapon> ApiWeapons = [];
    private static bool UseApiRoster;

    // ── Block layout constants ────────────────────────────────────────────────────
    internal const int BLOCK_W   = 35;
    internal const int CONTENT_W = 31;
    internal const int BAR_W     = 14;

    // ── Lookup tables ─────────────────────────────────────────────────────────────
    private static Dictionary<char, Character> AllHeroes = [];
    private static Dictionary<string, IAttackSource?> AttackMap = [];

    // ── Outer state (shared by playback functions) ────────────────────────────────
    internal static CombatResult Result = null!;
    internal static Dictionary<string, int> MaxHp = new();
    internal static Dictionary<string, int> CurHp = new();
    internal static Party HeroParty = null!;
    internal static Party EnemyParty = null!;
    private static char Scenario;

    internal static CombatStatsService Stats => CombatStats;

    internal static void Run()
    {
        DisplayConfig = GuiDisplayConfig.Load(logger: message => Console.Error.WriteLine(message));
        ConnectApi();
        PickDataSource();
        InitializeData();

        PrintHeader();
        Scenario = PickScenario();

        // ── Replay path — all setup and simulation already done by RunReplay() ──
        if (Scenario == 'W')
        {
            if (!RunReplay()) return;
            var replayMode = PickCombatMode();
            PacingMultiplier = PickPacing();
            CWL("\n  Press any key to watch the replay...", ConsoleColor.Gray);
            Console.ReadKey(true);
            Console.Clear();
            PrintHeader();
            if (replayMode == 'T') PlayTurnBased(); else PlayRealTime();
            PrintSummary();
            DumpCombatLog();
            return;
        }

        if (Scenario == 'D')
        {
            RunDuel();
        }
        else
        {
            RunPartyCombat();
        }

        var mode = PickCombatMode();

        // Targeting mode selection.
        ITargetSelector heroSelector;
        ITargetSelector enemySelector = new LowestHpTargetSelector();
        if (Scenario == 'P')
        {
            var targeting = PickTargetingMode();
            heroSelector = targeting == 'M'
                ? new ManualConsoleTargetSelector()
                : new LowestHpTargetSelector();
        }
        else
        {
            heroSelector = new LowestHpTargetSelector();
        }

        PacingMultiplier = PickPacing();

        // Reset + build state dicts
        foreach (var m in HeroParty.Members)  ResetCombatant(m.Character);
        foreach (var m in EnemyParty.Members) ResetCombatant(m.Character);

        var allMembers = HeroParty.Members.Concat(EnemyParty.Members).ToList();
        MaxHp = allMembers.ToDictionary(m => m.Character.Name, m => m.Character.MaxHitPoints);
        CurHp = new Dictionary<string, int>(MaxHp);

        CWL("\n  Press any key to start the combat...", ConsoleColor.Gray);
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        // Show combatant stat sheets prior to combat
        Console.WriteLine();
        if (Scenario == 'D')
        {
            var f1 = HeroParty.Members[0];
            var f2 = EnemyParty.Members[0];
            var f1Atk = f1.AttackSource;
            var f2Atk = f2.AttackSource;
            ShowSheet("FIGHTER 1", f1.Character, f1Atk,
                CombatStats.ComputeAttackerStats(f1.Character, GetSheetAttackSource(f1.Character, f1Atk)).AttackPower,
                CombatStats.ComputeDefenderStats(f1.Character).DefensePower);
            CWL("\n                           --- VS ---\n", ConsoleColor.Gray);
            ShowSheet("FIGHTER 2", f2.Character, f2Atk,
                CombatStats.ComputeAttackerStats(f2.Character, GetSheetAttackSource(f2.Character, f2Atk)).AttackPower,
                CombatStats.ComputeDefenderStats(f2.Character).DefensePower);
        }
        else
        {
            CWL("  ── YOUR HEROES ───────────────────────────────────────────", ConsoleColor.Cyan);
            foreach (var m in HeroParty.Members)
            {
                var atk = m.AttackSource;
                ShowSheet("HERO", m.Character, atk,
                    CombatStats.ComputeAttackerStats(m.Character, GetSheetAttackSource(m.Character, atk)).AttackPower,
                    CombatStats.ComputeDefenderStats(m.Character).DefensePower);
            }
            CWL("\n  ── ENEMY HORDE ───────────────────────────────────────────", ConsoleColor.Red);
            foreach (var m in EnemyParty.Members)
            {
                var atk = m.AttackSource;
                ShowSheet("ENEMY", m.Character, atk,
                    CombatStats.ComputeAttackerStats(m.Character, GetSheetAttackSource(m.Character, atk)).AttackPower,
                    CombatStats.ComputeDefenderStats(m.Character).DefensePower);
            }
        }

        // Combat always runs client-side.  When the API is reachable, dice rolls
        // are delegated to the API's /v1/roll/* endpoints via ApiDiceService;
        // otherwise a local seeded DiceService is used.
        _apiDiceService = null;

        IDiceService diceSvc;
        if (ApiClient is not null)
        {
            _apiDiceService = new ApiDiceService(ApiClient);
            diceSvc = _apiDiceService;
        }
        else
        {
            diceSvc = new DiceService();
        }
        _diceService = diceSvc;

        var simulator = new CombatSimulator(
            new CombatService(diceSvc, CombatStats),
            new TurnmeterService(),
            new StatusEffectService(),
            diceSvc,
            heroSelector,
            enemySelector);

        Result = simulator.Simulate(HeroParty, EnemyParty, 500);
        var diceLog = _apiDiceService?.DiceLog;
        Result.DiceLog = diceLog?.ToList();
        Result.Log = CombatLogMerger.Merge(Result.Log, diceLog);

        if (mode == 'T')
            PlayTurnBased();
        else
            PlayRealTime();

        PrintSummary();
        DumpCombatLog();
        AwardCombatXp();
    }

    private static void RunDuel()
    {
        var fighter1 = PickFighter("Fighter 1", null);
        var fighter2 = PickFighter("Fighter 2", fighter1.Name);
        ResetAll();

        var f1Atk = AttackMap[fighter1.Name];
        var f2Atk = AttackMap[fighter2.Name];
        var f1Ap = CombatStats.ComputeAttackerStats(fighter1, GetSheetAttackSource(fighter1, f1Atk)).AttackPower;
        var f1Dp = CombatStats.ComputeDefenderStats(fighter1).DefensePower;
        var f2Ap = CombatStats.ComputeAttackerStats(fighter2, GetSheetAttackSource(fighter2, f2Atk)).AttackPower;
        var f2Dp = CombatStats.ComputeDefenderStats(fighter2).DefensePower;

        Console.WriteLine();
        ShowSheet("FIGHTER 1", fighter1, f1Atk, f1Ap, f1Dp);
        CWL("\n                           --- VS ---\n", ConsoleColor.Gray);
        ShowSheet("FIGHTER 2", fighter2, f2Atk, f2Ap, f2Dp);

        HeroParty = Party.Solo(fighter1, f1Atk);
        EnemyParty = Party.Solo(fighter2, f2Atk);
    }

    private static void RunPartyCombat()
    {
        var heroes = PickHeroParty();
        EnemyParty = BuildEnemyParty();
        ResetAll();

        Console.WriteLine();
        CWL("  ── YOUR HEROES ───────────────────────────────────────────", ConsoleColor.Cyan);
        foreach (var h in heroes)
        {
            var atk = AttackMap[h.Name];
            ShowSheet("HERO", h, atk,
                CombatStats.ComputeAttackerStats(h, GetSheetAttackSource(h, atk)).AttackPower,
                CombatStats.ComputeDefenderStats(h).DefensePower);
        }
        CWL("\n  ── ENEMY HORDE ───────────────────────────────────────────", ConsoleColor.Red);
        foreach (var m in EnemyParty.Members)
        {
            var atk = m.AttackSource;
            ShowSheet("ENEMY", m.Character, atk,
                CombatStats.ComputeAttackerStats(m.Character, GetSheetAttackSource(m.Character, atk)).AttackPower,
                CombatStats.ComputeDefenderStats(m.Character).DefensePower);
        }

        HeroParty = Party.HeroParty(
            "Heroes",
            heroes.Select(h => new PartyMember { Character = h, AttackSource = AttackMap[h.Name] }));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    internal static void ResetCombatant(Character character)
    {
        character.CurrentHitPoints = character.MaxHitPoints;
        character.CurrentMana = character.MaxMana;
        character.ActiveStatusEffects.Clear();
    }

    private static void ResetAll()
    {
        if (UseApiRoster)
            foreach (var ch in ApiRoster) ResetCombatant(ch);
        else
            foreach (var (_, ch) in AllHeroes) ResetCombatant(ch);

        ResetCombatant(Krag);
        ResetCombatant(Skrix);
        ResetCombatant(Mordak);
        ResetCombatant(Zarath);
    }

    internal static IAttackSource GetSheetAttackSource(Character character, IAttackSource? attackSource)
    {
        if (attackSource is not null) return attackSource;
        if (character.MemorizedSpells.Count > 0)
            return character.MemorizedSpells
                .OrderByDescending(s => s.AttackBonus)
                .ThenByDescending(s => s.DamageCount)
                .First();
        return UnarmedStrike.Default;
    }

    // Returns null for spellcasters so the simulator picks the best spell each turn.
    // The equipped weapon is still shown on the card via Character.Equipment.RightHand.
    internal static IAttackSource? GetAttackSource(Character character)
    {
        if (character.MemorizedSpells.Count > 0) return null;
        if (character.Equipment.RightHand is { } weapon) return weapon;
        return UnarmedStrike.Default;
    }

    // For display purposes only — shows the best attack name in the picker list.
    internal static string GetAttackDisplayName(Character character)
    {
        if (character.MemorizedSpells.Count > 0)
            return character.MemorizedSpells
                .OrderByDescending(s => s.AttackBonus)
                .ThenByDescending(s => s.DamageCount)
                .First().Name;
        if (character.Equipment.RightHand is { } weapon) return weapon.Name;
        return "Unarmed";
    }

    internal static CombatDisplayState BuildDisplayStates()
    {
        var characters = new List<CharDisplayState>();
        foreach (var m in HeroParty.Members)
            characters.Add(new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                IsHero = true,
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Weapon = m.AttackSource?.Name ?? "",
                MaxMana = m.Character.MaxMana,
                Mana = m.Character.CurrentMana
            });
        foreach (var m in EnemyParty.Members)
            characters.Add(new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                IsHero = false,
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Weapon = m.AttackSource?.Name ?? "",
                MaxMana = m.Character.MaxMana,
                Mana = m.Character.CurrentMana
            });

        var layout = CombatLayout.From(
            HeroParty.Members.Select(m => m.Character.Name),
            EnemyParty.Members.Select(m => m.Character.Name),
            Scenario == 'D');

        return new CombatDisplayState(characters, layout);
    }

    internal static void EnsureSummonedPetDisplayState(CombatLogEntry entry, CombatDisplayState state)
    {
        if (string.IsNullOrWhiteSpace(entry.SummonedPetName) || state.TryGet(entry.SummonedPetName) is not null)
            return;

        var pet = FindSummonedPet(entry.SummonedPetName);
        var summonerState = state.TryGet(entry.ActorName);
        var isHero = summonerState?.IsHero
            ?? HeroParty.Members.Any(m => m.Character.Name == entry.ActorName);
        var maxHp = pet?.MaxHitPoints ?? 1;
        var weaponName = pet is null ? string.Empty : $"{pet.Name}'s Attack";

        MaxHp[entry.SummonedPetName] = maxHp;
        CurHp[entry.SummonedPetName] = maxHp;
        state.EnsurePet(entry.SummonedPetName, maxHp, isHero);
        if (state.TryGet(entry.SummonedPetName) is { } petState)
            petState.Weapon = weaponName;
    }

    private static Pet? FindSummonedPet(string petName)
    {
        var roster = AllHeroes.Values.Concat([Krag, Skrix, Mordak, Zarath]);
        return roster
            .SelectMany(character => character.MemorizedSpells)
            .Select(spell => spell.SummonedPet)
            .FirstOrDefault(pet => pet is not null && string.Equals(pet.Name, petName, StringComparison.OrdinalIgnoreCase));
    }

    internal static string Sign(int n) => n >= 0 ? "+" : "";

    internal static int DieSides(DieType d) => d switch
    {
        DieType.D4 => 4,
        DieType.D6 => 6,
        DieType.D8 => 8,
        DieType.D10 => 10,
        DieType.D12 => 12,
        DieType.D20 => 20,
        _ => 0
    };

    private static void ConnectApi()
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? "Production";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var apiOptions = config.GetSection("BattleArenaApi").Get<BattleArenaApiOptions>()
                      ?? new BattleArenaApiOptions();

        var apiUrl = apiOptions.Url;
        var apiKey = apiOptions.ApiKey;

        if (string.IsNullOrWhiteSpace(apiUrl))
            return;

        // Open log file at repo root / logs
        var repoDir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(repoDir) && !File.Exists(Path.Combine(repoDir, "BattleArena.sln")))
            repoDir = Path.GetDirectoryName(repoDir)!;

        var logDir = Path.Combine(string.IsNullOrEmpty(repoDir) ? AppContext.BaseDirectory : repoDir, "logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, "api-calls.log");
        var logWriter = new StreamWriter(logPath, append: true) { AutoFlush = true };

        // Use console logger during initial connection so the user can see the API calls being made.
        // After connecting, store a silent client (no console logger) for use during simulation.
        var initClient = new BattleArenaApiClient(
            apiUrl,
            apiKey: apiKey,
            consoleLogger: msg => { Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine(msg); Console.ResetColor(); },
            fileLogger: logWriter);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ── BattleArena API ─────────────────────────────────────");
        Console.ResetColor();
        Console.Write("  Connecting to BattleArena API at ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(apiUrl);
        Console.ResetColor();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Connection established.");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  Retrieving characters and gear...");
            Console.ResetColor();
            ApiRoster  = initClient.GetCharactersAsync().GetAwaiter().GetResult();
            ApiWeapons = initClient.GetWeaponsAsync().GetAwaiter().GetResult();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Retrieval complete  ─  {ApiRoster.Count} characters, {ApiWeapons.Count} weapons loaded.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Unreachable ({ex.Message})");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ────────────────────────────────────────────────────────");
        Console.ResetColor();
        Console.WriteLine();

        ApiClient = new BattleArenaApiClient(
            apiUrl,
            apiKey: apiKey,
            consoleLogger: null,
            fileLogger: logWriter);
    }

    private static void PickDataSource()
    {
        if (ApiRoster.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Using local hardcoded characters.");
            Console.ResetColor();
            return;
        }

        while (true)
        {
            Console.WriteLine();
            CWL("  Character source:", ConsoleColor.Yellow);
            CW("    "); CW("[A]", ConsoleColor.Cyan); CWL("  API characters  — loaded from BattleArena API", ConsoleColor.White);
            CW("    "); CW("[L]", ConsoleColor.Cyan); CWL("  Local characters  — hardcoded demo data\n", ConsoleColor.White);
            CW("  > ", ConsoleColor.Cyan);
            var k = Console.ReadKey(true).KeyChar;
            if (k is 'A' or 'a')
            {
                UseApiRoster = true;
                CWL("API characters", ConsoleColor.Cyan);
                return;
            }
            if (k is 'L' or 'l')
            {
                UseApiRoster = false;
                CWL("Local characters", ConsoleColor.Cyan);
                return;
            }
        }
    }

    private static void DumpCombatLog()
    {
        try
        {
            // Walk up from the executable to find the repo root (contains BattleArena.sln)
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir) && !File.Exists(Path.Combine(dir, "BattleArena.sln")))
                dir = Path.GetDirectoryName(dir)!;

            var outputDir = Path.Combine(string.IsNullOrEmpty(dir) ? AppContext.BaseDirectory : dir, "combat-logs");
            Directory.CreateDirectory(outputDir);

            var winner  = Result.WinningParty?.Name ?? "unknown";
            var loser   = Result.LosingParty?.Name  ?? "unknown";
            var label   = $"{winner}_vs_{loser}".Replace(" ", "_");
            var txtPath = CombatLogWriter.Write(Result, label, outputDir);
            var jsonPath = Path.ChangeExtension(txtPath, ".json");

            Console.WriteLine();
            CWL("  " + new string('─', 62), ConsoleColor.Gray);
            CW("  Combat log saved  ", ConsoleColor.Gray);
            CWL(Path.GetFileName(txtPath), ConsoleColor.Green);
            CW("  Replay data saved ", ConsoleColor.Gray);
            CWL(Path.GetFileName(jsonPath), ConsoleColor.Green);
            CW("  Directory: ", ConsoleColor.Gray);
            CWL(outputDir, ConsoleColor.Gray);
            CWL("  " + new string('─', 62), ConsoleColor.Gray);
            Console.WriteLine();

            try
            {
                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(jsonPath)!);
                CombatLogPruner.Prune(dirInfo);
            }
            catch
            {
                // Best-effort cleanup — don't fail the save if pruning fails
            }
        }
        catch (Exception ex)
        {
            CWL($"  [warn] Could not write combat log: {ex.Message}", ConsoleColor.Yellow);
        }
    }

    private static void AwardCombatXp()
    {
        var svc = new LevelingService(_diceService ?? new DiceService());
        if (Result.WinningParty is null || Result.LosingParty is null) return;

        var winners = Result.WinningParty.Members.Select(m => m.Character);
        var losers  = Result.LosingParty.Members.Select(m => m.Character);
        var awards  = svc.AwardCombatXp(winners, losers, Result.Log, Result.TotalTicks);

        Console.WriteLine();
        CWL("  " + new string('=', 62), ConsoleColor.Cyan);
        CWL("  POST-BATTLE XP", ConsoleColor.Yellow);
        CWL("  " + new string('=', 62), ConsoleColor.Cyan);

        var total = 0;
        foreach (var (name, xp) in awards)
        {
            total += xp;
            var ch = winners.FirstOrDefault(c => c.Name == name);
            if (ch is null) continue;
            CW($"  {name,-12}", ConsoleColor.White);
            CW($"+{xp,3} XP  ", ConsoleColor.Green);
            CW($"Level {ch.Level}", ConsoleColor.Cyan);
            Console.WriteLine();
        }

        foreach (var c in losers)
            CWL($"  {c.Name,-12}  -    XP  (defeated)", ConsoleColor.Gray);

        CWL($"\n  Total XP awarded: {total}", ConsoleColor.Yellow);
        CWL("  " + new string('=', 62) + "\n", ConsoleColor.Cyan);
    }
}
