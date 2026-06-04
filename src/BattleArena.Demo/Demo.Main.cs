namespace BattleArena.Demo;

using Application.Interfaces;
using Application.Modifiers;
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

    // ── API connection ────────────────────────────────────────────────────────────
    private static BattleArenaApiClient? ApiClient;
    private static IDiceService? _diceService;
    internal static List<Character> ApiRoster = [];
    internal static List<Weapon> ApiWeapons = [];

    // ── Block layout constants ────────────────────────────────────────────────────
    internal const int BLOCK_W   = 46;
    internal const int CONTENT_W = 42;
    internal const int BAR_W     = 25;

    // ── Outer state (shared by playback functions) ────────────────────────────────
    internal static CombatResult Result = null!;
    internal static Dictionary<string, int> MaxHp = new();
    internal static Dictionary<string, int> CurHp = new();
    internal static Party HeroParty = null!;
    internal static Party EnemyParty = null!;
    private static char Scenario;
    private static string _combatModeLabel = "";

    internal static CombatStatsService Stats => CombatStats;

    internal static void Run()
    {
        DisplayConfig = GuiDisplayConfig.Load(logger: message => Console.Error.WriteLine(message));
        ConnectApi();

        PrintHeader();

        while (true)
        {
            Scenario = PickScenario();

            if (Scenario == 'Q')
                return;

            // ── Replay path ──
            if (Scenario == 'W')
            {
                if (!RunReplay()) continue;
                var replayMode = PickCombatMode();
                _combatModeLabel = replayMode == 'T' ? "Turn-based" : "Auto";
                PacingMultiplier = PickPacing();
                CWL("\n  Press any key to watch the replay...", ConsoleColor.Gray);
                Console.ReadKey(true);
                Console.Clear();
                PrintHeader();
                if (replayMode == 'T') PlayTurnBased(); else PlayRealTime();
                PrintSummary();
                DumpCombatLog();
                Console.WriteLine();
                CWL("  Press any key to return to the main menu...", ConsoleColor.Gray);
                Console.ReadKey(true);
                Console.Clear();
                PrintHeader();
                continue;
            }

            // ── Combat loop (fight again returns to roster selection) ──
            char combatEndKey;
            do
            {
                if (Scenario == 'D')
                    RunDuel();
                else
                    RunPartyCombat();

                var mode = PickCombatMode();
                _combatModeLabel = mode == 'T' ? "Turn-based" : "Auto";

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
                    var f1Atk = GetAttackSource(f1.Character);
                    var f2Atk = GetAttackSource(f2.Character);
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
                        var atk = GetAttackSource(m.Character);
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

                IDiceService diceSvc;
                if (ApiClient is not null)
                {
                    diceSvc = new ApiDiceService(ApiClient);
                }
                else
                {
                    diceSvc = new DiceService();
                }
                _diceService = diceSvc;

                var makeInteractive = new Func<IActionDecisionSource>(() =>
                    new ConsoleActionDecisionSource((tick, actorName) =>
                        DrawCombatScreen(BuildCurrentDisplayState(), tick, actorName)));

                IActionDecisionSource heroDecision;
                IActionDecisionSource enemyDecision;

                if (mode == 'T')
                {
                    heroDecision = makeInteractive();
                    enemyDecision = Scenario == 'D' ? makeInteractive() : new AutoActionDecisionSource(diceSvc);
                }
                else
                {
                    heroDecision = new AutoActionDecisionSource(diceSvc);
                    enemyDecision = new AutoActionDecisionSource(diceSvc);
                }

                var simulator = new CombatSimulator(
                    new CombatService(diceSvc, CombatStats, [new RangeModifier()]),
                    new TurnmeterService(),
                    new StatusEffectService(),
                    diceSvc,
                    heroSelector,
                    enemySelector,
                    heroDecision,
                    enemyDecision);

                if (mode == 'T')
                {
                    var observer = new CombatConsoleObserver(Paced);
                    Result = simulator.SimulateAsync(HeroParty, EnemyParty, 500, observer)
                        .GetAwaiter().GetResult();
                }
                else
                {
                    Result = simulator.Simulate(HeroParty, EnemyParty, 500);
                }

                Result.DiceLog = _diceService?.DiceLog;
                Result.Log = CombatLogMerger.Merge(Result.Log, Result.DiceLog);

                if (mode != 'T')
                    PlayRealTime();

                PrintSummary();
                DumpCombatLog();
                AwardCombatXp();

                Console.WriteLine();
                CWL("  [F]ight again (new roster)  |  [M]ain menu  |  [Q]uit", ConsoleColor.Yellow);
                CW("  > ", ConsoleColor.Cyan);

                do { combatEndKey = Console.ReadKey(true).KeyChar; }
                while (combatEndKey is not ('F' or 'f' or 'M' or 'm' or 'Q' or 'q'));

                switch (combatEndKey)
                {
                    case 'F': case 'f': CWL("Fight again", ConsoleColor.Cyan); break;
                    case 'M': case 'm': CWL("Main menu", ConsoleColor.Cyan); break;
                    case 'Q': case 'q': CWL("Quit", ConsoleColor.Cyan); break;
                }

                Console.Clear();
                PrintHeader();

            } while (combatEndKey is 'F' or 'f');

            if (combatEndKey is 'Q' or 'q')
                return;
        }
    }

    private static void RunDuel()
    {
        var fighter1 = PickFighter("Fighter 1", null);
        var fighter2 = PickFighter("Fighter 2", fighter1.Name);

        foreach (var ch in ApiRoster) ResetCombatant(ch);

        var f1Atk = GetAttackSource(fighter1);
        var f2Atk = GetAttackSource(fighter2);
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

        foreach (var ch in ApiRoster) ResetCombatant(ch);

        Console.WriteLine();
        CWL("  ── YOUR HEROES ───────────────────────────────────────────", ConsoleColor.Cyan);
        foreach (var h in heroes)
        {
            var atk = GetAttackSource(h);
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
            heroes.Select(h => new PartyMember { Character = h, AttackSource = GetAttackSource(h) }));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    internal static void ResetCombatant(Character character)
    {
        character.CurrentHitPoints = character.MaxHitPoints;
        character.CurrentMana = character.MaxMana;
        character.ActiveStatusEffects.Clear();
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
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Race = m.Character.Race?.Name ?? "",
                Weapon = m.AttackSource?.Name ?? "",
                WeaponStats = FormatWeaponStats(m.AttackSource),
                ArmorName = m.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = m.Character.Equipment.TotalArmorClass,
                StrikeRating = m.Character.StrikeRating,
                MagicResistance = m.Character.ComputeResistance(ResistanceType.Magic),
                MaxMana = m.Character.MaxMana,
                Mana = m.Character.CurrentMana
            });
        foreach (var m in EnemyParty.Members)
            characters.Add(new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Race = m.Character.Race?.Name ?? "",
                Weapon = m.AttackSource?.Name ?? "",
                WeaponStats = FormatWeaponStats(m.AttackSource),
                ArmorName = m.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = m.Character.Equipment.TotalArmorClass,
                StrikeRating = m.Character.StrikeRating,
                MagicResistance = m.Character.ComputeResistance(ResistanceType.Magic),
                MaxMana = m.Character.MaxMana,
                Mana = m.Character.CurrentMana
            });

        var layout = CombatLayout.From(
            HeroParty.Members.Select(m => m.Character.Name),
            EnemyParty.Members.Select(m => m.Character.Name),
            Scenario == 'D');

        return new CombatDisplayState(characters, layout);
    }

    internal static CombatDisplayState BuildCurrentDisplayState()
    {
        var characters = new List<CharDisplayState>();
        foreach (var m in HeroParty.Members)
            characters.Add(new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = Math.Max(0, m.Character.CurrentHitPoints),
                IsAlive = m.Character.CurrentHitPoints > 0,
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Race = m.Character.Race?.Name ?? "",
                Weapon = m.AttackSource?.Name ?? "",
                WeaponStats = FormatWeaponStats(m.AttackSource),
                ArmorName = m.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = m.Character.Equipment.TotalArmorClass,
                StrikeRating = m.Character.StrikeRating,
                MagicResistance = m.Character.ComputeResistance(ResistanceType.Magic),
                MaxMana = m.Character.MaxMana,
                Mana = Math.Max(0, m.Character.CurrentMana)
            });
        foreach (var m in EnemyParty.Members)
            characters.Add(new CharDisplayState
            {
                Name = m.Character.Name,
                MaxHp = MaxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
                Hp = Math.Max(0, m.Character.CurrentHitPoints),
                IsAlive = m.Character.CurrentHitPoints > 0,
                Level = m.Character.Level,
                ClassName = m.Character.ClassName,
                Sex = m.Character.Sex,
                Race = m.Character.Race?.Name ?? "",
                Weapon = m.AttackSource?.Name ?? "",
                WeaponStats = FormatWeaponStats(m.AttackSource),
                ArmorName = m.Character.Equipment.Chest?.Name ?? "None",
                ArmorClass = m.Character.Equipment.TotalArmorClass,
                StrikeRating = m.Character.StrikeRating,
                MagicResistance = m.Character.ComputeResistance(ResistanceType.Magic),
                MaxMana = m.Character.MaxMana,
                Mana = Math.Max(0, m.Character.CurrentMana)
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
        var maxHp = pet?.MaxHitPoints ?? 1;
        var weaponName = pet is null ? string.Empty : $"{pet.Name}'s Attack";

        MaxHp[entry.SummonedPetName] = maxHp;
        CurHp[entry.SummonedPetName] = maxHp;
        state.EnsurePet(entry.SummonedPetName, maxHp, entry.ActorName);
        if (state.TryGet(entry.SummonedPetName) is { } petState)
            petState.Weapon = weaponName;
    }

    private static Pet? FindSummonedPet(string petName)
    {
        return ApiRoster
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

    internal static string FormatWeaponStats(IAttackSource? source)
    {
        if (source is null) return "";
        var dice = $"{source.DamageCount}d{DieSides(source.DamageDie)}";
        return source.AttackBonus > 0 ? $"{dice}+{source.AttackBonus}" : dice;
    }

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

        // Open log file at repo root / combat-logs
        var repoDir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(repoDir)
            && !File.Exists(Path.Combine(repoDir, "src", "BattleArena.sln")))
            repoDir = Path.GetDirectoryName(repoDir)!;

        var logDir = Path.Combine(string.IsNullOrEmpty(repoDir) ? AppContext.BaseDirectory : repoDir, "combat-logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, "api-calls.log");
        var logWriter = new StreamWriter(logPath, append: true) { AutoFlush = true };

        var initClient = new BattleArenaApiClient(
            apiUrl,
            apiKey: apiKey,
            consoleLogger: msg => { Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine(msg); Console.ResetColor(); },
            fileLogger: logWriter);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ── BattleArena API ─────────────────────────────────────");
        Console.ResetColor();
        Console.Write("  Connecting to BattleArena API at ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(apiUrl);
        Console.ResetColor();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Connection established.");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  Retrieving characters and gear...");
            Console.ResetColor();
            ApiRoster  = initClient.GetCharactersAsync().GetAwaiter().GetResult();
            ApiWeapons = initClient.GetWeaponsAsync().GetAwaiter().GetResult();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  Retrieval complete  ─  {ApiRoster.Count} characters, {ApiWeapons.Count} weapons loaded.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  API unreachable ({ex.Message})");
            Console.ResetColor();

            var rosterPath = Path.Combine(AppContext.BaseDirectory, "roster.json");
            if (File.Exists(rosterPath))
            {
                var data = RosterLoader.ForceLoad(rosterPath);
                foreach (var h in data.Heroes)  { h.Npc = 0; ApiRoster.Add(h); }
                foreach (var e in data.Enemies) { e.Npc = 1; ApiRoster.Add(e); }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  Loaded local roster  ─  {data.Heroes.Count} heroes, {data.Enemies.Count} enemies.");
                Console.ResetColor();
            }
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

    private static void DumpCombatLog()
    {
        try
        {
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir)
                && !File.Exists(Path.Combine(dir, "src", "BattleArena.sln")))
                dir = Path.GetDirectoryName(dir)!;

            var outputDir = Path.Combine(string.IsNullOrEmpty(dir) ? AppContext.BaseDirectory : dir, "combat-logs");
            Directory.CreateDirectory(outputDir);

            var winner  = Result.WinningParty?.Name ?? "unknown";
            var loser   = Result.LosingParty?.Name  ?? "unknown";
            var label   = $"{winner}_vs_{loser}".Replace(" ", "_");
            var txtPath = CombatLogWriter.Write(Result, label, outputDir, _combatModeLabel);
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
                // Best-effort cleanup
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
