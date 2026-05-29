namespace BattleArena.Demo;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using System.IO;
using Core.Entities;
using Core.Entities.Enums;
using Microsoft.Extensions.Configuration;

static partial class Demo
{
    // ── Services ──────────────────────────────────────────────────────────────────
    private static readonly CombatStatsService CombatStats = new();

    // ── Optional API connection ───────────────────────────────────────────────────
    private static BattleArenaApiClient? ApiClient;
    private static ApiDiceService? _apiDiceService;
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
            CWL("\n  Press any key to watch the replay...", ConsoleColor.Gray);
            Console.ReadKey(true);
            Console.Clear();
            PrintHeader();
            if (replayMode == 'T') PlayTurnBased(); else PlayRealTime();
            PrintSummary();
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

        var simulator = new CombatSimulator(
            new CombatService(diceSvc, CombatStats),
            new TurnmeterService(),
            new StatusEffectService(),
            diceSvc,
            heroSelector,
            enemySelector);

        Result = simulator.Simulate(HeroParty, EnemyParty, 500);

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

    internal static IAttackSource GetAttackSource(Character character)
    {
        if (character.Equipment.RightHand is { } weapon)
            return weapon;
        if (character.MemorizedSpells.Count > 0)
            return character.MemorizedSpells
                .OrderByDescending(s => s.AttackBonus)
                .ThenByDescending(s => s.DamageCount)
                .First();
        return UnarmedStrike.Default;
    }

    internal static Dictionary<string, CharDisplayState> BuildDisplayStates()
    {
        var dict = new Dictionary<string, CharDisplayState>();
        foreach (var m in HeroParty.Members)
            dict[m.Character.Name] = new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                IsHero = true,
                Weapon = m.AttackSource?.Name ?? ""
            };
        foreach (var m in EnemyParty.Members)
            dict[m.Character.Name] = new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                IsHero = false,
                Weapon = m.AttackSource?.Name ?? ""
            };
        return dict;
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
        {
            Console.Write("  Enter BattleArena API URL (or press Enter for local data): ");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                apiUrl = input;
        }

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

        ApiClient = new BattleArenaApiClient(
            apiUrl,
            apiKey: apiKey,
            consoleLogger: msg =>
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(msg);
                Console.ResetColor();
            },
            fileLogger: logWriter);
        Console.Write("  Connecting to BattleArena API... ");
        try
        {
            ApiRoster = ApiClient.GetCharactersAsync().GetAwaiter().GetResult();
            ApiWeapons = ApiClient.GetWeaponsAsync().GetAwaiter().GetResult();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"OK  ({ApiRoster.Count} characters, {ApiWeapons.Count} weapons loaded)");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"unreachable ({ex.Message})");
            Console.ResetColor();
        }
    }

    private static void PickDataSource()
    {
        if (ApiRoster.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
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
            CWL(Path.GetFileName(jsonPath), ConsoleColor.DarkGreen);
            CW("  Directory: ", ConsoleColor.Gray);
            CWL(outputDir, ConsoleColor.Gray);
            CWL("  " + new string('─', 62), ConsoleColor.Gray);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            CWL($"  [warn] Could not write combat log: {ex.Message}", ConsoleColor.DarkYellow);
        }
    }

    private static void AwardCombatXp()
    {
        var svc = new LevelingService();
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
