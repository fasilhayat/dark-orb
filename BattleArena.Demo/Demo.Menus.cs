namespace BattleArena.Demo;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using System.IO;

static partial class Demo
{
    // ── PickScenario ──────────────────────────────────────────────────────────────

    private static char PickScenario()
    {
        CWL("\n  Choose scenario:", ConsoleColor.Yellow);
        CW("    "); CW("[D]", ConsoleColor.Cyan); CWL("  Duel         -- 1v1, pick two of your characters", ConsoleColor.White);
        CW("    "); CW("[P]", ConsoleColor.Cyan); CWL("  Party Combat  -- build a hero party vs the Enemy Horde (3 enemies)", ConsoleColor.White);
        CW("    "); CW("[W]", ConsoleColor.Cyan); CWL("  Watch Replay  -- replay a saved combat from the replays/ folder\n", ConsoleColor.White);
        CW("  > ", ConsoleColor.Cyan);
        while (true)
        {
            var k = Console.ReadKey(true).KeyChar;
            if (k is 'D' or 'd') { CWL("Duel", ConsoleColor.Cyan); return 'D'; }
            if (k is 'P' or 'p') { CWL("Party Combat", ConsoleColor.Cyan); return 'P'; }
            if (k is 'W' or 'w') { CWL("Watch Replay", ConsoleColor.Cyan); return 'W'; }
        }
    }

    // ── PickFighter ───────────────────────────────────────────────────────────────

    private static Character PickFighter(string label, string? excludedName)
    {
        if (UseApiRoster)
            return PickFighterFromRoster(label, excludedName);

        while (true)
        {
            CW($"  Pick {label}:  ", ConsoleColor.Yellow);
            CW("[T]", ConsoleColor.Cyan); CW(" Theron   ");
            CW("[G]", ConsoleColor.Cyan); CW(" Gruk   ");
            CW("[L]", ConsoleColor.Cyan); CW(" Lyra", ConsoleColor.White);
            if (!string.IsNullOrEmpty(excludedName))
                CW($"   (cannot pick {excludedName})", ConsoleColor.DarkGray);
            Console.WriteLine();
            CW("  > ", ConsoleColor.Cyan);

            var pick = char.ToUpperInvariant(Console.ReadKey(true).KeyChar);
            if (!AllHeroes.TryGetValue(pick, out var selected)) continue;
            if (selected.Name == excludedName)
            {
                CWL($"  {selected.Name} is already selected. Choose another fighter.\n", ConsoleColor.DarkYellow);
                continue;
            }
            CWL(selected.Name, ConsoleColor.Cyan);
            return selected;
        }
    }

    // ── PickFighterFromRoster ─────────────────────────────────────────────────────

    private static Character PickFighterFromRoster(string label, string? excludedName)
    {
        var available = ApiRoster.Where(c => c.Name != excludedName).ToList();
        while (true)
        {
            Console.WriteLine();
            CWL($"  Pick {label}:", ConsoleColor.Yellow);
            for (var i = 0; i < available.Count; i++)
            {
                var ch = available[i];
                var atk = GetAttackSource(ch);
                CW($"    [{i + 1}]  ", ConsoleColor.Cyan);
                CW($"{ch.Name,-20}", ConsoleColor.White);
                CW($"{atk.Name,-16}", ConsoleColor.Yellow);
                CWL($"  Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}", ConsoleColor.DarkGray);
            }
            CW("  > ", ConsoleColor.Cyan);

            var key = Console.ReadKey(true).KeyChar;
            if (int.TryParse(key.ToString(), out var idx) && idx >= 1 && idx <= available.Count)
            {
                var ch = available[idx - 1];
                CWL(ch.Name, ConsoleColor.Cyan);
                ch.CurrentHitPoints = ch.MaxHitPoints;
                AttackMap[ch.Name] = GetAttackSource(ch);
                return ch;
            }
        }
    }

    // ── PickWeaponForCharacter ────────────────────────────────────────────────────

    private static IAttackSource? PickWeaponForCharacter(Character ch)
    {
        if (!UseApiRoster || ApiWeapons.Count == 0) return null;

        var weapons = ApiWeapons
            .Where(w => w.AttackType != AttackType.Ranged)
            .OrderBy(w => w.Quality)
            .ThenBy(w => w.Name)
            .ToList();

        if (weapons.Count == 0) return null;

        while (true)
        {
            Console.WriteLine();
            CWL($"  Equip weapon for {ch.Name}:", ConsoleColor.Yellow);
            for (var i = 0; i < weapons.Count; i++)
            {
                var w = weapons[i];
                CW($"    [{i + 1,2}]  ", ConsoleColor.Cyan);
                CW($"{w.Name,-26}", ConsoleColor.White);
                var qualColor = w.Quality switch
                {
                    GearQuality.Legendary => ConsoleColor.Yellow,
                    GearQuality.Epic => ConsoleColor.Magenta,
                    GearQuality.Rare => ConsoleColor.Cyan,
                    _ => ConsoleColor.DarkGray
                };
                CW($"  {w.DamageCount}d{(int)w.DamageDie}  {w.DamageType,-12}  +{w.AttackBonus} ATK", ConsoleColor.DarkGray);
                CWL($"  [{w.Quality}]", qualColor);
            }
            CW("  Enter number: ", ConsoleColor.Cyan);

            var input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= weapons.Count)
            {
                var chosen = weapons[idx - 1];
                CWL($"  -> {ch.Name} equips {chosen.Name}", ConsoleColor.Green);
                return chosen;
            }
            CWL("  Invalid selection - try again.", ConsoleColor.DarkYellow);
        }
    }

    // ── PickHeroParty ─────────────────────────────────────────────────────────────

    private static List<Character> PickHeroParty()
    {
        if (UseApiRoster)
            return PickHeroPartyFromRoster();

        var selected = new List<Character>();
        while (true)
        {
            Console.Clear();
            PrintHeader();
            CWL($"\n  BUILD YOUR HERO PARTY  (max {Party.HeroPartyMaxSize})", ConsoleColor.Yellow);
            Console.WriteLine();
            foreach (var (key, ch) in AllHeroes)
            {
                var picked = selected.Any(s => s.Name == ch.Name);
                CW($"    [{key}] ", ConsoleColor.Cyan);
                if (picked) CWL($"{ch.Name,-12}  [SELECTED]", ConsoleColor.Green);
                else CWL(ch.Name, ConsoleColor.White);
            }
            Console.WriteLine();
            if (selected.Count > 0)
            {
                CW("  Party : ", ConsoleColor.DarkGray);
                CWL(string.Join(", ", selected.Select(c => c.Name)), ConsoleColor.Green);
            }
            CWL("  Press a key to toggle a hero | [Enter] to confirm (need at least 1)\n", ConsoleColor.DarkGray);
            CW("  > ", ConsoleColor.Cyan);

            var kInfo = Console.ReadKey(true);
            if (kInfo.Key == ConsoleKey.Enter && selected.Count > 0) return selected;

            var pick = char.ToUpperInvariant(kInfo.KeyChar);
            if (!AllHeroes.TryGetValue(pick, out var hero)) continue;

            var idx = selected.FindIndex(c => c.Name == hero.Name);
            if (idx >= 0) selected.RemoveAt(idx);
            else if (selected.Count < Party.HeroPartyMaxSize)
                selected.Add(hero);
        }
    }

    // ── PickHeroPartyFromRoster ───────────────────────────────────────────────────

    private static List<Character> PickHeroPartyFromRoster()
    {
        var selected = new List<Character>();
        while (true)
        {
            Console.Clear();
            PrintHeader();
            CWL($"\n  BUILD YOUR HERO PARTY  (max {Party.HeroPartyMaxSize})", ConsoleColor.Yellow);
            Console.WriteLine();

            for (var i = 0; i < ApiRoster.Count; i++)
            {
                var ch = ApiRoster[i];
                var picked = selected.Any(s => s.Name == ch.Name);
                var atk = GetAttackSource(ch);
                CW($"    [{i + 1}]  ", ConsoleColor.Cyan);
                if (picked)
                {
                    CW($"{ch.Name,-18}", ConsoleColor.Green);
                    CW($"{atk.Name,-14}", ConsoleColor.Green);
                    CWL($"  Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}  [v]", ConsoleColor.Green);
                }
                else
                {
                    CW($"{ch.Name,-18}", ConsoleColor.White);
                    CW($"{atk.Name,-14}", ConsoleColor.Yellow);
                    CWL($"  Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}", ConsoleColor.DarkGray);
                }
            }

            Console.WriteLine();
            if (selected.Count > 0)
            {
                CW("  Party : ", ConsoleColor.DarkGray);
                CWL(string.Join(", ", selected.Select(c => c.Name)), ConsoleColor.Green);
            }
            CWL("  Press number to toggle | [Enter] to confirm (need at least 1)\n", ConsoleColor.DarkGray);
            CW("  > ", ConsoleColor.Cyan);

            var kInfo = Console.ReadKey(true);
            if (kInfo.Key == ConsoleKey.Enter && selected.Count > 0)
            {
                foreach (var hero in selected)
                    AttackMap[hero.Name] = GetAttackSource(hero);
                return selected;
            }

            if (int.TryParse(kInfo.KeyChar.ToString(), out var idx) && idx >= 1 && idx <= ApiRoster.Count)
            {
                var hero = ApiRoster[idx - 1];
                var existing = selected.FindIndex(c => c.Name == hero.Name);
                if (existing >= 0)
                {
                    selected.RemoveAt(existing);
                    AttackMap.Remove(hero.Name);
                }
                else if (selected.Count < Party.HeroPartyMaxSize)
                    selected.Add(hero);
            }
        }
    }

    // ── RunReplay ─────────────────────────────────────────────────────────────────

    // Returns true if a replay was loaded; false if no files found.
    internal static bool RunReplay()
    {
        var replayDir = FindReplayFolder();
        if (replayDir is null)
        {
            CWL("\n  No replays/ folder found. Place a .json snapshot file there to replay it.\n",
                ConsoleColor.DarkYellow);
            return false;
        }

        // Always pick the most recently written .json — drop oldest files if folder grows
        var latest = Directory.GetFiles(replayDir, "*.json")
                               .OrderByDescending(File.GetLastWriteTime)
                               .FirstOrDefault();

        if (latest is null)
        {
            CWL($"\n  No .json files found in {replayDir}", ConsoleColor.DarkYellow);
            CWL("  Copy a .json file from combat-logs/ into replays/ to replay it.\n", ConsoleColor.DarkGray);
            return false;
        }

        Console.WriteLine();
        CW("  Loading replay: ", ConsoleColor.DarkGray);
        CWL(Path.GetFileNameWithoutExtension(latest), ConsoleColor.Green);

        var snapshot = CombatReplayer.Deserialize(File.ReadAllText(latest));
        var (p1, p2) = snapshot.ToParties();

        HeroParty  = p1;
        EnemyParty = p2;

        var allMembers = HeroParty.Members.Concat(EnemyParty.Members).ToList();
        MaxHp = allMembers.ToDictionary(m => m.Character.Name, m => m.Character.MaxHitPoints);
        CurHp = new Dictionary<string, int>(MaxHp);

        CWL($"  Seed: {snapshot.Seed}  |  replaying...\n", ConsoleColor.DarkGray);
        Result = CombatReplayer.Replay(snapshot);
        return true;
    }

    private static string? FindReplayFolder()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir) && !File.Exists(Path.Combine(dir, "BattleArena.sln")))
            dir = Path.GetDirectoryName(dir)!;
        if (string.IsNullOrEmpty(dir)) return null;
        var path = Path.Combine(dir, "replays");
        return Directory.Exists(path) ? path : null;
    }

    // ── BuildEnemyParty ───────────────────────────────────────────────────────────

    private static Party BuildEnemyParty()
    {
        if (UseApiRoster)
        {
            var enemies = ApiRoster.Where(c => c.Npc == 1).Take(3).ToList();
            return new Party
            {
                Name = "Enemy Horde",
                Members = enemies.Select(c => new PartyMember
                {
                    Character = c,
                    AttackSource = GetAttackSource(c)
                }).ToList()
            };
        }

        return new Party
        {
            Name = "Enemy Horde",
            Members =
            [
                new PartyMember { Character = Krag, AttackSource = OrcAxe },
                new PartyMember { Character = Skrix, AttackSource = GoblinDagger },
                new PartyMember { Character = Mordak, AttackSource = null }
            ]
        };
    }

    // ── PickCombatMode ────────────────────────────────────────────────────────────

    private static char PickCombatMode()
    {
        CWL("\n  Choose combat mode:", ConsoleColor.Yellow);
        CW("    "); CW("[T]", ConsoleColor.Cyan); CWL("  Turn-based  -- press any key to advance each turn", ConsoleColor.White);
        CW("    "); CW("[R]", ConsoleColor.Cyan); CWL("  Real-time   -- fully automatic tick-by-tick playback\n", ConsoleColor.White);
        CW("  > ", ConsoleColor.Cyan);
        while (true)
        {
            var k = Console.ReadKey(true).KeyChar;
            if (k is 'T' or 't') { CWL("Turn-based", ConsoleColor.Cyan); return 'T'; }
            if (k is 'R' or 'r') { CWL("Real-time", ConsoleColor.Cyan); return 'R'; }
        }
    }

    // ── PickTargetingMode ─────────────────────────────────────────────────────────

    private static char PickTargetingMode()
    {
        CWL("\n  Choose targeting mode:", ConsoleColor.Yellow);
        CW("    "); CW("[A]", ConsoleColor.Cyan); CWL("  Auto    -- heroes and enemies both focus the weakest target", ConsoleColor.White);
        CW("    "); CW("[M]", ConsoleColor.Cyan); CWL("  Manual  -- you pick each hero's target when they act\n", ConsoleColor.White);
        CW("  > ", ConsoleColor.Cyan);
        while (true)
        {
            var k = Console.ReadKey(true).KeyChar;
            if (k is 'A' or 'a') { CWL("Auto", ConsoleColor.Cyan); return 'A'; }
            if (k is 'M' or 'm') { CWL("Manual", ConsoleColor.Cyan); return 'M'; }
        }
    }
}
