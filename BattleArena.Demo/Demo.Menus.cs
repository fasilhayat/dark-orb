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
                CW($"   (cannot pick {excludedName})", ConsoleColor.Gray);
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
        while (true)
        {
            Console.WriteLine();
            CWL($"  Pick {label}:", ConsoleColor.Yellow);
            for (var i = 0; i < ApiRoster.Count && i < 26; i++)
            {
                var ch = ApiRoster[i];
                var key = (char)('A' + i);
                var taken = ch.Name == excludedName;
                var atkDisplay = GetAttackDisplayName(ch);
                CW($"    [{key}]  ", ConsoleColor.Cyan);
                CW($"{ch.Name,-20}", taken ? ConsoleColor.Gray : ConsoleColor.White);
                CW($"{atkDisplay,-16}", taken ? ConsoleColor.Gray : ConsoleColor.Yellow);
                if (taken)
                    CWL($"  (already selected)", ConsoleColor.Gray);
                else
                    CWL($"  {GetClassName(ch.ClassId),-10} Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}", ConsoleColor.Gray);
            }
            CW("  > ", ConsoleColor.Cyan);

            var pick = char.ToUpperInvariant(Console.ReadKey(true).KeyChar);
            var idx = pick - 'A';
            if (idx >= 0 && idx < ApiRoster.Count)
            {
                var ch = ApiRoster[idx];
                if (ch.Name == excludedName)
                {
                    CWL($"  {ch.Name} is already selected. Pick another.", ConsoleColor.DarkYellow);
                    continue;
                }
                CWL($"  → {ch.Name}", ConsoleColor.Cyan);
                ch.CurrentHitPoints = ch.MaxHitPoints;
                AttackMap[ch.Name] = GetAttackSource(ch);
                return ch;
            }
            CWL("  Invalid selection — try again.", ConsoleColor.DarkYellow);
        }
    }

    // ── PickWeaponForCharacter ────────────────────────────────────────────────────

    private static IAttackSource? PickWeaponForCharacter(Character ch)
    {
        if (!UseApiRoster || ApiWeapons.Count == 0) return null;

        var weapons = ApiWeapons
            .Where(w => w.AttackType != AttackType.Ranged && ch.CanEquip(w.Archetype))
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
                    _ => ConsoleColor.Gray
                };
                CW($"  {w.DamageCount}d{(int)w.DamageDie}  {w.DamageType,-12}  +{w.AttackBonus} ATK", ConsoleColor.Gray);
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
                if (picked) CWL($"{ch.Name,-12}  {GetClassName(ch.ClassId),-10}  [SELECTED]", ConsoleColor.Green);
                else CWL($"{ch.Name,-12}  {GetClassName(ch.ClassId)}", ConsoleColor.White);
            }
            Console.WriteLine();
            if (selected.Count > 0)
            {
                CW("  Party : ", ConsoleColor.Gray);
                CWL(string.Join(", ", selected.Select(c => c.Name)), ConsoleColor.Green);
            }
            CWL("  Press a key to toggle a hero | [Enter] to confirm (need at least 1)\n", ConsoleColor.Gray);
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

            for (var i = 0; i < ApiRoster.Count && i < 26; i++)
            {
                var key = (char)('A' + i);
                var ch = ApiRoster[i];
                var picked = selected.Any(s => s.Name == ch.Name);
                var atkDisplay = GetAttackDisplayName(ch);
                CW($"    [{key}]  ", ConsoleColor.Cyan);
                if (picked)
                {
                    CW($"{ch.Name,-18}", ConsoleColor.Green);
                    CW($"{atkDisplay,-14}", ConsoleColor.Green);
                    CWL($"  {GetClassName(ch.ClassId),-10} Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}  [✓]", ConsoleColor.Green);
                }
                else
                {
                    CW($"{ch.Name,-18}", ConsoleColor.White);
                    CW($"{atkDisplay,-14}", ConsoleColor.Yellow);
                    CWL($"  {GetClassName(ch.ClassId),-10} Lv{ch.Level}  STR {ch.Strength,-3}  DEX {ch.Dexterity,-3}  HP {ch.MaxHitPoints}", ConsoleColor.Gray);
                }
            }

            Console.WriteLine();
            if (selected.Count > 0)
            {
                CW("  Party : ", ConsoleColor.Gray);
                CWL(string.Join(", ", selected.Select(c => c.Name)), ConsoleColor.Green);
            }
            CWL("  Press a letter key to toggle | [Enter] to confirm (need ≥ 1)\n", ConsoleColor.Gray);
            CW("  > ", ConsoleColor.Cyan);

            var kInfo = Console.ReadKey(true);
            if (kInfo.Key == ConsoleKey.Enter && selected.Count > 0)
            {
                foreach (var hero in selected)
                    AttackMap[hero.Name] = GetAttackSource(hero);
                return selected;
            }

            var pick = char.ToUpperInvariant(kInfo.KeyChar);
            var idx = pick - 'A';
            if (idx >= 0 && idx < ApiRoster.Count)
            {
                var hero = ApiRoster[idx];
                var existing = selected.FindIndex(c => c.Name == hero.Name);
                if (existing >= 0)
                {
                    selected.RemoveAt(existing);
                    AttackMap.Remove(hero.Name);
                }
                else if (selected.Count < Party.HeroPartyMaxSize)
                    selected.Add(hero);
                else
                    CWL($"  Party is full (max {Party.HeroPartyMaxSize}). Remove someone first.", ConsoleColor.DarkYellow);
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
            CWL("  Copy a .json file from combat-logs/ into replays/ to replay it.\n", ConsoleColor.Gray);
            return false;
        }

        Console.WriteLine();
        CW("  Loading replay: ", ConsoleColor.Gray);
        CWL(Path.GetFileNameWithoutExtension(latest), ConsoleColor.Green);

        var snapshot = CombatReplayer.Deserialize(File.ReadAllText(latest));
        var (p1, p2) = snapshot.ToParties();

        HeroParty  = p1;
        EnemyParty = p2;

        var allMembers = HeroParty.Members.Concat(EnemyParty.Members).ToList();
        MaxHp = allMembers.ToDictionary(m => m.Character.Name, m => m.Character.MaxHitPoints);
        CurHp = new Dictionary<string, int>(MaxHp);

        CWL($"  Seed: {snapshot.Seed}  |  replaying...\n", ConsoleColor.Gray);
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
                new PartyMember { Character = Krag,   AttackSource = OrcAxe },
                new PartyMember { Character = Skrix,  AttackSource = GoblinDagger },
                new PartyMember { Character = Mordak, AttackSource = null },
                new PartyMember { Character = Zarath, AttackSource = null }
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

    private static double PickPacing()
    {
        CWL("\n  Choose pacing:", ConsoleColor.Yellow);
        CW("    "); CW("[N]", ConsoleColor.Cyan); CWL("  Normal      (1.0x)", ConsoleColor.White);
        CW("    "); CW("[F]", ConsoleColor.Cyan); CWL("  Fast        (2.0x — all delays halved)", ConsoleColor.White);
        CW("    "); CW("[S]", ConsoleColor.Cyan); CWL("  Slow        (0.5x — all delays doubled)\n", ConsoleColor.White);
        CW("  > ", ConsoleColor.Cyan);
        while (true)
        {
            var k = Console.ReadKey(true).KeyChar;
            if (k is 'N' or 'n') { CWL("Normal", ConsoleColor.Cyan); return 1.0; }
            if (k is 'F' or 'f') { CWL("Fast", ConsoleColor.Cyan); return 0.5; }
            if (k is 'S' or 's') { CWL("Slow", ConsoleColor.Cyan); return 2.0; }
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
