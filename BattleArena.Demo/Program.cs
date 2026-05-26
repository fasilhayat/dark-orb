using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "BattleArena — Combat Simulation";

// ── Services ──────────────────────────────────────────────────────────────────
var combatStats = new CombatStatsService();

// ── Hero characters ───────────────────────────────────────────────────────────

var longsword = new BattleArena.Core.Entities.Weapon
{
    Name        = "Longsword",
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 1,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Slashing,
    AttackType  = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 2
};
var theron = new BattleArena.Core.Entities.Character
{
    Name             = "Theron",
    Level            = 5,
    Strength         = 18,
    Dexterity        = 12,
    Intelligence     = 10,
    StrikeRating     = 14,
    TurnSpeed        = 10,
    MaxHitPoints     = 50,
    CurrentHitPoints = 50,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest     = new BattleArena.Core.Entities.Armor { Name = "Chain Mail",   ArmorClass = 5,  Mitigation = 2, MaxDexterityBonus = 6 },
        RightHand = longsword
    }
};

var battleAxe = new BattleArena.Core.Entities.Weapon
{
    Name        = "Battle Axe",
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 1,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Slashing,
    AttackType  = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 1
};
var gruk = new BattleArena.Core.Entities.Character
{
    Name             = "Gruk",
    Level            = 3,
    Strength         = 16,
    Dexterity        = 8,
    Intelligence     = 8,
    StrikeRating     = 16,
    TurnSpeed        = 6,
    MaxHitPoints     = 35,
    CurrentHitPoints = 35,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest     = new BattleArena.Core.Entities.Armor { Name = "Leather Armor", ArmorClass = 7, Mitigation = 1, MaxDexterityBonus = 6 },
        RightHand = battleAxe
    }
};

var fireball = new BattleArena.Core.Entities.Spell
{
    Name        = "Fireball",
    Description = "A blazing orb of fire",
    School      = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D6,
    DamageCount = 3,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Fire,
    AttackBonus = 2,
    SpellLevel  = 3
};
var iceBolt = new BattleArena.Core.Entities.Spell
{
    Name        = "Ice Bolt",
    Description = "A shard of magical ice",
    School      = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 2,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Cold,
    AttackBonus = 2,
    SpellLevel  = 2
};
var lightningStrike = new BattleArena.Core.Entities.Spell
{
    Name        = "Lightning Strike",
    Description = "A bolt of crackling lightning",
    School      = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D10,
    DamageCount = 2,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Lightning,
    AttackBonus = 3,
    SpellLevel  = 4
};
var lyra = new BattleArena.Core.Entities.Character
{
    Name             = "Lyra",
    Level            = 5,
    Strength         = 8,
    Dexterity        = 14,
    Intelligence     = 18,
    StrikeRating     = 13,
    TurnSpeed        = 8,
    MaxHitPoints     = 30,
    CurrentHitPoints = 30,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest = new BattleArena.Core.Entities.Armor { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
    },
    MemorizedSpells = new List<BattleArena.Core.Entities.Spell> { fireball, iceBolt, lightningStrike }
};

// ── Enemy characters ──────────────────────────────────────────────────────────

var orcAxe = new BattleArena.Core.Entities.Weapon
{
    Name        = "Orcish Axe",
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D10,
    DamageCount = 1,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Slashing,
    AttackType  = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 1
};
var krag = new BattleArena.Core.Entities.Character
{
    Name             = "Krag",
    Level            = 4,
    Strength         = 17,
    Dexterity        = 9,
    Intelligence     = 6,
    StrikeRating     = 15,
    TurnSpeed        = 7,
    MaxHitPoints     = 45,
    CurrentHitPoints = 45,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest     = new BattleArena.Core.Entities.Armor { Name = "Orcish Hide", ArmorClass = 6, Mitigation = 2, MaxDexterityBonus = 4 },
        RightHand = orcAxe
    }
};

var goblinDagger = new BattleArena.Core.Entities.Weapon
{
    Name        = "Poisoned Dagger",
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D4,
    DamageCount = 2,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Piercing,
    AttackType  = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 3
};
var skrix = new BattleArena.Core.Entities.Character
{
    Name             = "Skrix",
    Level            = 2,
    Strength         = 9,
    Dexterity        = 16,
    Intelligence     = 10,
    StrikeRating     = 12,
    TurnSpeed        = 12,
    MaxHitPoints     = 20,
    CurrentHitPoints = 20,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest     = new BattleArena.Core.Entities.Armor { Name = "Worn Leather", ArmorClass = 8, Mitigation = 0, MaxDexterityBonus = 6 },
        RightHand = goblinDagger
    }
};

var shadowBolt = new BattleArena.Core.Entities.Spell
{
    Name        = "Shadow Bolt",
    Description = "A bolt of shadow energy",
    School      = BattleArena.Core.Entities.Enums.SpellSchool.Other,
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 2,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Cold,
    AttackBonus = 2,
    SpellLevel  = 2
};
var soulDrain = new BattleArena.Core.Entities.Spell
{
    Name        = "Soul Drain",
    Description = "Saps the life force of a target",
    School      = BattleArena.Core.Entities.Enums.SpellSchool.Other,
    DamageDie   = BattleArena.Core.Entities.Enums.DieType.D10,
    DamageCount = 1,
    DamageType  = BattleArena.Core.Entities.Enums.DamageType.Fire,
    AttackBonus = 1,
    SpellLevel  = 2
};
var mordak = new BattleArena.Core.Entities.Character
{
    Name             = "Mordak",
    Level            = 3,
    Strength         = 7,
    Dexterity        = 12,
    Intelligence     = 16,
    StrikeRating     = 14,
    TurnSpeed        = 9,
    MaxHitPoints     = 25,
    CurrentHitPoints = 25,
    Equipment        = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest = new BattleArena.Core.Entities.Armor { Name = "Dark Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
    },
    MemorizedSpells = new List<BattleArena.Core.Entities.Spell> { shadowBolt, soulDrain }
};

// ── Lookup tables ─────────────────────────────────────────────────────────────

var allHeroes = new Dictionary<char, BattleArena.Core.Entities.Character>
{
    ['T'] = theron,
    ['G'] = gruk,
    ['L'] = lyra
};
var attackMap = new Dictionary<string, BattleArena.Core.Entities.IAttackSource?>
{
    [theron.Name] = longsword,
    [gruk.Name]   = battleAxe,
    [lyra.Name]   = null,
    [krag.Name]   = orcAxe,
    [skrix.Name]  = goblinDagger,
    [mordak.Name] = null
};

// ── Outer state (shared by play functions via closure) ────────────────────────
string activeActor = "";
BattleResult result = null!;
Dictionary<string, int> maxHp = new();
Dictionary<string, int> curHp = new();
BattleArena.Core.Entities.Party heroParty;
BattleArena.Core.Entities.Party enemyParty;

// ── Main flow ─────────────────────────────────────────────────────────────────

PrintHeader();
var scenario = PickScenario();

if (scenario == 'D')
{
    // Duel: two fighters from the hero roster
    var fighter1 = PickFighter("Fighter 1", null);
    var fighter2 = PickFighter("Fighter 2", fighter1.Name);
    ResetAll();

    var f1Atk = attackMap[fighter1.Name];
    var f2Atk = attackMap[fighter2.Name];
    var f1Ap  = combatStats.ComputeAttackerStats(fighter1, GetSheetAttackSource(fighter1, f1Atk)).AttackPower;
    var f1Dp  = combatStats.ComputeDefenderStats(fighter1).DefensePower;
    var f2Ap  = combatStats.ComputeAttackerStats(fighter2, GetSheetAttackSource(fighter2, f2Atk)).AttackPower;
    var f2Dp  = combatStats.ComputeDefenderStats(fighter2).DefensePower;

    Console.WriteLine();
    ShowSheet("FIGHTER 1", fighter1, f1Atk, f1Ap, f1Dp);
    CWL("\n                           --- VS ---\n", ConsoleColor.DarkGray);
    ShowSheet("FIGHTER 2", fighter2, f2Atk, f2Ap, f2Dp);

    heroParty  = BattleArena.Core.Entities.Party.Solo(fighter1, f1Atk);
    enemyParty = BattleArena.Core.Entities.Party.Solo(fighter2, f2Atk);
}
else
{
    // Party Battle: build hero team vs pre-made Enemy Horde
    var heroes = PickHeroParty();
    enemyParty = BuildEnemyParty();
    ResetAll();

    Console.WriteLine();
    CWL("  ── YOUR HEROES ───────────────────────────────────────────", ConsoleColor.Cyan);
    foreach (var h in heroes)
    {
        var atk = attackMap[h.Name];
        ShowSheet("HERO", h, atk,
            combatStats.ComputeAttackerStats(h, GetSheetAttackSource(h, atk)).AttackPower,
            combatStats.ComputeDefenderStats(h).DefensePower);
    }
    CWL("\n  ── ENEMY HORDE ───────────────────────────────────────────", ConsoleColor.Red);
    foreach (var m in enemyParty.Members)
    {
        var atk = m.AttackSource;
        ShowSheet("ENEMY", m.Character, atk,
            combatStats.ComputeAttackerStats(m.Character, GetSheetAttackSource(m.Character, atk)).AttackPower,
            combatStats.ComputeDefenderStats(m.Character).DefensePower);
    }

    heroParty = BattleArena.Core.Entities.Party.HeroParty(
        "Heroes",
        heroes.Select(h => new BattleArena.Core.Entities.PartyMember { Character = h, AttackSource = attackMap[h.Name] }));
}

var mode = PickBattleMode();

// Targeting mode: only meaningful for Party Battle (1v1 always has one target).
ITargetSelector heroSelector;
ITargetSelector enemySelector = new LowestHpTargetSelector();   // enemies always focus weakest hero
if (scenario == 'P')
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
foreach (var m in heroParty.Members)  ResetCombatant(m.Character);
foreach (var m in enemyParty.Members) ResetCombatant(m.Character);

var allMembers = heroParty.Members.Concat(enemyParty.Members).ToList();
maxHp = allMembers.ToDictionary(m => m.Character.Name, m => m.Character.MaxHitPoints);
curHp = new Dictionary<string, int>(maxHp);

CWL("\n  Press any key to start the battle...", ConsoleColor.DarkGray);
Console.ReadKey(true);
Console.Clear();
PrintHeader();

var simulator = new BattleSimulator(
    new CombatService(new DiceService(), combatStats),
    new TurnmeterService(),
    new StatusEffectService(),
    heroSelector,
    enemySelector);

result = simulator.Simulate(heroParty, enemyParty, 500);

if (mode == 'T')
    PlayTurnBased();
else
    PlayRealTime();

PrintSummary();

// ── PickScenario ──────────────────────────────────────────────────────────────

char PickScenario()
{
    CWL("\n  Choose scenario:", ConsoleColor.Yellow);
    CW("    "); CW("[D]", ConsoleColor.Cyan); CWL("  Duel         -- 1v1, pick two of your characters", ConsoleColor.White);
    CW("    "); CW("[P]", ConsoleColor.Cyan); CWL("  Party Battle  -- build a hero party vs the Enemy Horde (3 enemies)\n", ConsoleColor.White);
    CW("  > ", ConsoleColor.Cyan);
    while (true)
    {
        var k = Console.ReadKey(true).KeyChar;
        if (k is 'D' or 'd') { CWL("Duel", ConsoleColor.Cyan); return 'D'; }
        if (k is 'P' or 'p') { CWL("Party Battle", ConsoleColor.Cyan); return 'P'; }
    }
}

// ── PickFighter ───────────────────────────────────────────────────────────────

BattleArena.Core.Entities.Character PickFighter(string label, string? excludedName)
{
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
        if (!allHeroes.TryGetValue(pick, out var selected)) continue;
        if (selected.Name == excludedName)
        {
            CWL($"  {selected.Name} is already selected. Choose another fighter.\n", ConsoleColor.DarkYellow);
            continue;
        }
        CWL(selected.Name, ConsoleColor.Cyan);
        return selected;
    }
}

// ── PickHeroParty ─────────────────────────────────────────────────────────────

List<BattleArena.Core.Entities.Character> PickHeroParty()
{
    var selected = new List<BattleArena.Core.Entities.Character>();
    while (true)
    {
        Console.Clear();
        PrintHeader();
        CWL($"\n  BUILD YOUR HERO PARTY  (max {BattleArena.Core.Entities.Party.HeroPartyMaxSize})", ConsoleColor.Yellow);
        Console.WriteLine();
        foreach (var (key, ch) in allHeroes)
        {
            var picked = selected.Any(s => s.Name == ch.Name);
            CW($"    [{key}] ", ConsoleColor.Cyan);
            if (picked) CWL($"{ch.Name,-12}  [SELECTED]", ConsoleColor.Green);
            else        CWL(ch.Name, ConsoleColor.White);
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
        if (!allHeroes.TryGetValue(pick, out var hero)) continue;

        var idx = selected.FindIndex(c => c.Name == hero.Name);
        if (idx >= 0) selected.RemoveAt(idx);
        else if (selected.Count < BattleArena.Core.Entities.Party.HeroPartyMaxSize)
            selected.Add(hero);
    }
}

// ── BuildEnemyParty ───────────────────────────────────────────────────────────

BattleArena.Core.Entities.Party BuildEnemyParty() => new()
{
    Name    = "Enemy Horde",
    Members = new List<BattleArena.Core.Entities.PartyMember>
    {
        new() { Character = krag,   AttackSource = orcAxe       },
        new() { Character = skrix,  AttackSource = goblinDagger  },
        new() { Character = mordak, AttackSource = null          }
    }
};

// ── PickBattleMode ────────────────────────────────────────────────────────────

char PickBattleMode()
{
    CWL("\n  Choose battle mode:", ConsoleColor.Yellow);
    CW("    "); CW("[T]", ConsoleColor.Cyan); CWL("  Turn-based  -- press any key to advance each turn", ConsoleColor.White);
    CW("    "); CW("[R]", ConsoleColor.Cyan); CWL("  Real-time   -- fully automatic tick-by-tick playback\n", ConsoleColor.White);
    CW("  > ", ConsoleColor.Cyan);
    while (true)
    {
        var k = Console.ReadKey(true).KeyChar;
        if (k is 'T' or 't') { CWL("Turn-based", ConsoleColor.Cyan); return 'T'; }
        if (k is 'R' or 'r') { CWL("Real-time",  ConsoleColor.Cyan); return 'R'; }
    }
}

// ── PickTargetingMode ─────────────────────────────────────────────────────────

char PickTargetingMode()
{
    CWL("\n  Choose targeting mode:", ConsoleColor.Yellow);
    CW("    "); CW("[A]", ConsoleColor.Cyan); CWL("  Auto    -- heroes and enemies both focus the weakest target", ConsoleColor.White);
    CW("    "); CW("[M]", ConsoleColor.Cyan); CWL("  Manual  -- you pick each hero's target when they act\n", ConsoleColor.White);
    CW("  > ", ConsoleColor.Cyan);
    while (true)
    {
        var k = Console.ReadKey(true).KeyChar;
        if (k is 'A' or 'a') { CWL("Auto",   ConsoleColor.Cyan); return 'A'; }
        if (k is 'M' or 'm') { CWL("Manual", ConsoleColor.Cyan); return 'M'; }
    }
}

// ── ResetCombatant / ResetAll ─────────────────────────────────────────────────

void ResetCombatant(BattleArena.Core.Entities.Character character)
{
    character.CurrentHitPoints = character.MaxHitPoints;
    character.ActiveStatusEffects.Clear();
}

void ResetAll()
{
    foreach (var (_, ch) in allHeroes) ResetCombatant(ch);
    ResetCombatant(krag);
    ResetCombatant(skrix);
    ResetCombatant(mordak);
}

// ── GetSheetAttackSource ──────────────────────────────────────────────────────

BattleArena.Core.Entities.IAttackSource GetSheetAttackSource(
    BattleArena.Core.Entities.Character character,
    BattleArena.Core.Entities.IAttackSource? attackSource)
{
    if (attackSource is not null) return attackSource;
    return character.MemorizedSpells
        .OrderByDescending(s => s.AttackBonus)
        .ThenByDescending(s => s.DamageCount)
        .First();
}

// ── PlayTurnBased ─────────────────────────────────────────────────────────────

void PlayTurnBased()
{
    // Group log entries into turns (each turn starts at a TurnStart event).
    var turns = new List<List<BattleLogEntry>>();
    List<BattleLogEntry>? current = null;
    foreach (var e in result.Log)
    {
        if (e.EventType == "TurnStart") { current = new(); turns.Add(current); }
        if (current != null && e.EventType != "TurnMeterGain") current.Add(e);
    }

    for (var idx = 0; idx < turns.Count; idx++)
    {
        var turnEntries = turns[idx];
        var tick        = turnEntries.FirstOrDefault()?.Tick ?? 0;

        Console.Clear();
        PrintHeader();

        var ts         = turnEntries.FirstOrDefault(e => e.EventType == "TurnStart");
        var actorName  = ts?.ActorName  ?? "?";
        var targetName = ts?.TargetName ?? "?";
        activeActor    = actorName;

        CWL($"\n  Turn {idx + 1}  |  Tick {tick}", ConsoleColor.DarkGray);
        CW("  "); CW(actorName.ToUpper(), CharColor(actorName));
        CW("  HP "); CW($"{curHp.GetValueOrDefault(actorName)}/{maxHp.GetValueOrDefault(actorName)}",
            HpColor(curHp.GetValueOrDefault(actorName), maxHp.GetValueOrDefault(actorName, 1)));
        CW("   ->   ");
        CW(targetName.ToUpper(), CharColor(targetName));
        CW("  HP "); CWL($"{curHp.GetValueOrDefault(targetName)}/{maxHp.GetValueOrDefault(targetName)}",
            HpColor(curHp.GetValueOrDefault(targetName), maxHp.GetValueOrDefault(targetName, 1)));
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);
        Console.WriteLine();

        ShowAllHp();
        Console.WriteLine();
        ShowTmForTick(tick);
        Console.WriteLine();

        foreach (var e in turnEntries)
        {
            switch (e.EventType)
            {
                case "TurnStart":
                {
                    var verb = e.IsSpell ? "conjures" : "readies";
                    CW("  >> ", ConsoleColor.DarkCyan);
                    CW(e.ActorName.ToUpper(), CharColor(e.ActorName));
                    CW($" {verb} ");
                    CW($"[{e.AttackSourceName ?? "weapon"}]", e.IsSpell ? ConsoleColor.Magenta : ConsoleColor.Yellow);
                    CW(" targeting ");
                    CW(targetName, CharColor(targetName));
                    CWL("!", ConsoleColor.White);
                    Console.WriteLine();
                    break;
                }

                case "Attack":
                    PrintAttack(e);
                    break;

                case "Damage":
                    curHp[e.ActorName] = e.TargetHpAfter ?? curHp.GetValueOrDefault(e.ActorName);
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, CharColor(e.ActorName));
                    CW(" takes "); CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowAllHp();
                    break;

                case "FumblePenalty":
                    CWL($"\n  {e.Message}", ConsoleColor.DarkYellow);
                    break;

                case "TurnEnd":
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, CharColor(e.ActorName));
                    CWL(" ends their turn.", ConsoleColor.DarkGray);
                    break;

                case "Death":
                    Console.WriteLine();
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    CWL($"  *** {e.Message} ***", ConsoleColor.Red);
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    break;

                case "KnockedOut":
                    Console.WriteLine();
                    CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
                    CWL($"  ~~~ {e.Message} ~~~", ConsoleColor.DarkYellow);
                    CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
                    break;
            }
        }

        Console.WriteLine();
        CWL("  " + new string('─', 65), ConsoleColor.DarkGray);
        var battleOver = turnEntries.Any(e => e.EventType is "Death" or "KnockedOut");
        CWL(battleOver ? "  Battle over!  Press any key for results..."
                       : "  Press any key for next turn...", ConsoleColor.DarkGray);
        Console.ReadKey(true);
    }

    activeActor = "";
}

// ── PlayRealTime ──────────────────────────────────────────────────────────────

void PlayRealTime()
{
    var byTick = result.Log.GroupBy(e => e.Tick).OrderBy(g => g.Key).ToList();

    CWL("\n  BATTLE BEGINS\n", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.DarkCyan);

    var allM = heroParty.Members.Concat(enemyParty.Members).ToList();

    var curTm       = allM.ToDictionary(m => m.Character.Name, _ => 0);
    var curTmReady  = allM.ToDictionary(m => m.Character.Name, _ => false);
    var curTmActive = allM.ToDictionary(m => m.Character.Name, _ => false);

    int quietStart = -1, quietEnd = -1;
    var quietTmStart = new Dictionary<string, int>();
    var quietTmEnd   = new Dictionary<string, int>();

    void FlushQuiet()
    {
        if (quietStart < 0) return;
        if (quietEnd > quietStart + 1)
        {
            CWL($"\n  ... {quietEnd - quietStart + 1} ticks pass", ConsoleColor.DarkGray);
            foreach (var n in quietTmStart.Keys)
            {
                var start = quietTmStart[n];
                var end   = quietTmEnd.GetValueOrDefault(n, start);
                CWL($"      {n,-12}  TM: {start,3} -> {end,3}", ConsoleColor.DarkGray);
            }
            Thread.Sleep(80);
        }
        quietStart = quietEnd = -1;
        quietTmStart.Clear();
        quietTmEnd.Clear();
    }

    foreach (var tickGroup in byTick)
    {
        var entries   = tickGroup.ToList();
        var hasAction = entries.Any(e => e.EventType == "TurnStart");

        if (!hasAction)
        {
            foreach (var e in entries.Where(e => e.EventType == "TurnMeterGain"))
            {
                curTm[e.ActorName]      = e.TurnMeterAfter ?? 0;
                curTmReady[e.ActorName] = e.IsReady;

                if (quietStart < 0)
                {
                    quietStart = e.Tick;
                    quietTmStart[e.ActorName] = e.TurnMeterBefore ?? 0;
                }
                quietEnd = e.Tick;
                quietTmEnd[e.ActorName] = e.TurnMeterAfter ?? 0;
            }
            Thread.Sleep(40);
            continue;
        }

        FlushQuiet();

        var turnStart = entries.First(e => e.EventType == "TurnStart");
        var attacker  = turnStart.ActorName;
        var target    = turnStart.TargetName ?? "?";
        activeActor   = attacker;

        Console.WriteLine();
        CWL("  " + new string('=', 65), ConsoleColor.DarkCyan);
        CW($"  Tick {tickGroup.Key,-3}  |  ", ConsoleColor.DarkGray);
        CW(attacker.ToUpper(), CharColor(attacker));
        CW("  HP "); CW($"{curHp.GetValueOrDefault(attacker)}/{maxHp.GetValueOrDefault(attacker)}",
            HpColor(curHp.GetValueOrDefault(attacker), maxHp.GetValueOrDefault(attacker, 1)));
        CW("   ->   ");
        CW(target.ToUpper(), CharColor(target));
        CW("  HP ");
        CWL($"{curHp.GetValueOrDefault(target)}/{maxHp.GetValueOrDefault(target)}",
            HpColor(curHp.GetValueOrDefault(target), maxHp.GetValueOrDefault(target, 1)));
        CWL("  " + new string('=', 65), ConsoleColor.DarkCyan);

        curTmActive[attacker] = true;
        if (curTmActive.ContainsKey(target)) curTmActive[target] = false;
        Console.WriteLine();
        ShowAllTm(curTm, curTmReady, curTmActive);
        Console.WriteLine();

        Thread.Sleep(300);

        foreach (var e in entries)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    break;

                case "TurnStart":
                    Console.WriteLine();
                    CW("  >> ", ConsoleColor.DarkCyan);
                    CW(e.ActorName.ToUpper(), CharColor(e.ActorName));
                    CW(e.IsSpell ? " conjures " : " readies ");
                    CW($"[{e.AttackSourceName ?? "weapon"}]", e.IsSpell ? ConsoleColor.Magenta : ConsoleColor.Yellow);
                    CW(" targeting ");
                    CW(target, CharColor(target));
                    CWL("!", ConsoleColor.White);
                    break;

                case "Attack":
                    Thread.Sleep(500);
                    PrintAttack(e);
                    break;

                case "Damage":
                    Thread.Sleep(200);
                    curHp[e.ActorName] = e.TargetHpAfter ?? curHp.GetValueOrDefault(e.ActorName);
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, CharColor(e.ActorName));
                    CW(" takes "); CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowAllHp();
                    Thread.Sleep(800);
                    break;

                case "FumblePenalty":
                    CWL($"  {e.Message}", ConsoleColor.DarkYellow);
                    break;

                case "TurnEnd":
                    curTm[e.ActorName]       = e.TurnMeterAfter ?? 0;
                    curTmReady[e.ActorName]  = e.IsReady;
                    curTmActive[e.ActorName] = false;
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, CharColor(e.ActorName));
                    CWL(" ends their turn.", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowAllTm(curTm, curTmReady, curTmActive);
                    Console.WriteLine();
                    CWL("  " + new string('─', 65), ConsoleColor.DarkGray);
                    break;

                case "Death":
                    Thread.Sleep(500);
                    Console.WriteLine();
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    CWL($"  *** {e.Message} ***", ConsoleColor.Red);
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    Thread.Sleep(1500);
                    break;

                case "KnockedOut":
                    Thread.Sleep(500);
                    Console.WriteLine();
                    CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
                    CWL($"  ~~~ {e.Message} ~~~", ConsoleColor.DarkYellow);
                    CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
                    Thread.Sleep(1500);
                    break;
            }
        }
    }

    FlushQuiet();
    activeActor = "";
}

// ── PrintSummary ──────────────────────────────────────────────────────────────

void PrintSummary()
{
    Console.Clear();
    PrintHeader();

    if (result.MaxTicksReached)
    {
        Console.WriteLine();
        CWL("  BATTLE TIMEOUT — no winner declared.", ConsoleColor.DarkYellow);
        CWL($"  Total ticks: {result.TotalTicks}", ConsoleColor.White);
        CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
        return;
    }

    var wParty = result.WinningParty!;
    var lParty = result.LosingParty!;

    Console.WriteLine();
    CW("  BATTLE COMPLETE  --  ", ConsoleColor.Green);
    CW(wParty.Name, ConsoleColor.Green);
    CWL("  WINS!", ConsoleColor.Green);
    CWL("  " + new string('=', 62), ConsoleColor.Cyan);

    var attacks = result.Log.Where(e => e.EventType == "Attack").ToList();
    var hits    = attacks.Count(e => e.IsHit == true);
    var misses  = attacks.Count(e => e.IsHit == false && e.IsFumble == false);
    var crits   = attacks.Count(e => e.IsCritical == true);
    var fumbles = attacks.Count(e => e.IsFumble == true);

    CWL($"\n  Total actions :  {attacks.Count}", ConsoleColor.White);
    CW("  Results       :  "); CW($"{hits} hits", ConsoleColor.Green);
    CW($" / {misses} misses"); CW($" / {crits} crits", ConsoleColor.Magenta);
    CWL($" / {fumbles} fumbles", ConsoleColor.DarkYellow);

    CWL("\n  Damage dealt:", ConsoleColor.White);
    foreach (var m in wParty.Members.Concat(lParty.Members))
    {
        var dmg      = attacks.Where(e => e.ActorName == m.Character.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);
        var isWinner = wParty.Members.Any(wm => wm.Character.Name == m.Character.Name);
        CW("    "); CW($"{m.Character.Name,-12}", isWinner ? ConsoleColor.Green : ConsoleColor.DarkGray);
        CW($"  {dmg,3} dmg", ConsoleColor.Yellow);
        CWL(isWinner ? "  [winner side]" : "  [loser side]", isWinner ? ConsoleColor.Green : ConsoleColor.DarkGray);
    }

    CWL("\n  Final HP:", ConsoleColor.White);
    CWL("  ── Winners ──────────────────────────────────────────────", ConsoleColor.Green);
    foreach (var m in wParty.Members)
        ShowHp(m.Character.Name, m.Character.CurrentHitPoints, maxHp.GetValueOrDefault(m.Character.Name, 1));
    CWL("  ── Losers ───────────────────────────────────────────────", ConsoleColor.Red);
    foreach (var m in lParty.Members)
        ShowHp(m.Character.Name, m.Character.CurrentHitPoints, maxHp.GetValueOrDefault(m.Character.Name, 1));

    var loserTag = result.LoserStatus == BattleArena.Core.Entities.Enums.CharacterVitalStatus.Dead
        ? "SLAIN" : "KNOCKED OUT";
    CWL($"\n  {lParty.Name} is {loserTag}!",
        result.LoserStatus == BattleArena.Core.Entities.Enums.CharacterVitalStatus.Dead
            ? ConsoleColor.Red : ConsoleColor.DarkYellow);

    CWL($"\n  Battle length :  {result.TotalTicks} ticks", ConsoleColor.White);
    CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
    Console.WriteLine();
}

// ── ShowAllHp ─────────────────────────────────────────────────────────────────

void ShowAllHp()
{
    var multi = heroParty.Members.Count > 1 || enemyParty.Members.Count > 1;
    if (multi) { CW("  "); CWL("── Heroes ───────────────", ConsoleColor.Cyan); }
    foreach (var m in heroParty.Members)
        ShowHp(m.Character.Name, curHp.GetValueOrDefault(m.Character.Name), maxHp.GetValueOrDefault(m.Character.Name, 1));
    if (multi)
    {
        Console.WriteLine();
        CW("  "); CWL("── Enemies ──────────────", ConsoleColor.Red);
    }
    foreach (var m in enemyParty.Members)
        ShowHp(m.Character.Name, curHp.GetValueOrDefault(m.Character.Name), maxHp.GetValueOrDefault(m.Character.Name, 1));
}

// ── ShowAllTm (real-time live state) ─────────────────────────────────────────

void ShowAllTm(Dictionary<string, int> tmValues, Dictionary<string, bool> tmReady, Dictionary<string, bool> tmActive)
{
    var multi = heroParty.Members.Count > 1 || enemyParty.Members.Count > 1;
    if (multi) { CW("  "); CWL("── Heroes ───────────────", ConsoleColor.Cyan); }
    foreach (var m in heroParty.Members)
    {
        var n = m.Character.Name;
        ShowTm(n, tmValues.GetValueOrDefault(n), tmReady.GetValueOrDefault(n), tmActive.GetValueOrDefault(n));
    }
    if (multi) { Console.WriteLine(); CW("  "); CWL("── Enemies ──────────────", ConsoleColor.Red); }
    foreach (var m in enemyParty.Members)
    {
        var n = m.Character.Name;
        ShowTm(n, tmValues.GetValueOrDefault(n), tmReady.GetValueOrDefault(n), tmActive.GetValueOrDefault(n));
    }
}

// ── ShowTmForTick (turn-based — reads from log) ───────────────────────────────

void ShowTmForTick(int tick)
{
    var multi   = heroParty.Members.Count > 1 || enemyParty.Members.Count > 1;
    var entries = result.Log.Where(e => e.EventType == "TurnMeterGain" && e.Tick == tick).ToList();

    if (multi) { CW("  "); CWL("── Heroes ───────────────", ConsoleColor.Cyan); }
    foreach (var m in heroParty.Members)
    {
        var e = entries.FirstOrDefault(x => x.ActorName == m.Character.Name);
        if (e != null) ShowTm(e.ActorName, e.TurnMeterAfter ?? 0, e.IsReady, e.IsActive);
    }
    if (multi) { Console.WriteLine(); CW("  "); CWL("── Enemies ──────────────", ConsoleColor.Red); }
    foreach (var m in enemyParty.Members)
    {
        var e = entries.FirstOrDefault(x => x.ActorName == m.Character.Name);
        if (e != null) ShowTm(e.ActorName, e.TurnMeterAfter ?? 0, e.IsReady, e.IsActive);
    }
}

// ── PrintHeader ───────────────────────────────────────────────────────────────

void PrintHeader()
{
    CWL("  " + new string('=', 65), ConsoleColor.Cyan);
    CWL("        ***  BATTLE ARENA  --  COMBAT SIMULATION DEMO  ***", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.Cyan);
}

// ── ShowSheet ─────────────────────────────────────────────────────────────────

void ShowSheet(string role, BattleArena.Core.Entities.Character ch, BattleArena.Core.Entities.IAttackSource? attackSource, int ap, int dp)
{
    var displaySource = attackSource ?? GetSheetAttackSource(ch, attackSource);
    var abilityScore  = displaySource.UsesIntelligence ? ch.Intelligence
                      : displaySource.AttackType == BattleArena.Core.Entities.Enums.AttackType.Ranged ? ch.Dexterity
                      : ch.Strength;
    var abilityMod = (abilityScore - 10) / 2;
    var dexMod     = (ch.Dexterity - 10) / 2;
    var dexCap     = Math.Min(dexMod, ch.Equipment.Chest?.MaxDexterityBonus ?? 6);
    var ac         = ch.Equipment.Chest?.ArmorClass ?? 0;
    var mit        = ch.Equipment.Chest?.Mitigation ?? 0;

    const int IW = 60;
    void Sep() => CWL("  +" + new string('-', IW + 2) + "+", ConsoleColor.Cyan);
    void Row(string content, ConsoleColor col = ConsoleColor.White)
    {
        CW("  | ", ConsoleColor.Cyan);
        Console.ForegroundColor = col;
        Console.Write((" " + content).PadRight(IW));
        Console.ResetColor();
        CWL(" |", ConsoleColor.Cyan);
    }
    void Row2(string left, string right, ConsoleColor col = ConsoleColor.White)
    {
        var inner   = " " + left;
        var padding = IW - inner.Length - right.Length;
        var line    = inner + new string(' ', Math.Max(1, padding)) + right;
        CW("  | ", ConsoleColor.Cyan);
        Console.ForegroundColor = col;
        Console.Write(line.PadRight(IW));
        Console.ResetColor();
        CWL(" |", ConsoleColor.Cyan);
    }

    Sep();
    Row2($"{role}: {ch.Name}", $"Level {ch.Level}", ConsoleColor.White);
    Sep();
    Row($"HP: {ch.MaxHitPoints}   TurnSpeed: {ch.TurnSpeed}   StrikeRating: {ch.StrikeRating}");
    Row($"STR: {ch.Strength} ({Sign((ch.Strength - 10) / 2)}{(ch.Strength - 10) / 2})   DEX: {ch.Dexterity} ({Sign(dexMod)}{dexMod})   INT: {ch.Intelligence} ({Sign((ch.Intelligence - 10) / 2)}{(ch.Intelligence - 10) / 2})");
    Sep();
    Row($"Armor   : {ch.Equipment.Chest?.Name ?? "None",-18} AC {ac,-2}  EffAC {20 - ac,-2}  Mitigation: {mit}");
    if (ch.MemorizedSpells.Count > 0)
        foreach (var spell in ch.MemorizedSpells)
            Row($"Spells  : {spell.Name,-18} {spell.DamageCount}d{DieSides(spell.DamageDie)} {spell.DamageType}");
    else if (attackSource is not null)
        Row($"Weapon  : {attackSource.Name,-18} {attackSource.DamageCount}d{DieSides(attackSource.DamageDie)} {attackSource.DamageType,-10} +{attackSource.AttackBonus} atk bonus");
    Sep();
    var abilityLabel = displaySource.UsesIntelligence ? "int"
                     : displaySource.AttackType == BattleArena.Core.Entities.Enums.AttackType.Ranged ? "dex" : "str";
    Row($"Atk Power : {ap,-4}  (20-{ch.StrikeRating}) + {ch.Level} (lvl) + ({Sign(abilityMod)}{abilityMod}) ({abilityLabel}) + {displaySource.AttackBonus} (src)");
    Row($"Def Power : {dp,-4}  (20-{ac}) + ({Sign(dexCap)}{dexCap}) (dex)");
    Sep();
    Console.WriteLine();
}

// ── PrintAttack ───────────────────────────────────────────────────────────────

void PrintAttack(BattleLogEntry e)
{
    var total  = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
    var margin = total - (e.DefensePower ?? 0);
    var src    = e.AttackSourceName ?? "Unknown";
    var srcCol = e.IsSpell ? ConsoleColor.Magenta : ConsoleColor.Yellow;

    Console.WriteLine();
    CW("  ", ConsoleColor.White);
    CW(e.ActorName, CharColor(e.ActorName));
    CW(e.IsSpell ? " casts " : " attacks with ");
    CW($"[{src}]", srcCol);
    Console.WriteLine();
    CWL("  " + new string('-', 45), ConsoleColor.DarkGray);

    Console.WriteLine();
    CW("  Roll  "); CW($"d20 = {e.DieRoll,2}", ConsoleColor.Yellow);
    CW("   Attack Power "); CW($"{e.AttackPower}", ConsoleColor.Yellow);
    CW("  =  Total "); CW($"{total,2}", ConsoleColor.White);
    CW("   vs  Defence "); CW($"{e.DefensePower}", ConsoleColor.Yellow);
    CW("   |  margin ");
    if (margin >= 0) CWL($"+{margin}", ConsoleColor.Green);
    else             CWL($"{margin}",  ConsoleColor.Red);

    Console.WriteLine();
    if (e.IsCritical == true)
        CWL("  !!! CRITICAL HIT !!!  -- Double damage!", ConsoleColor.Magenta);
    else if (e.IsFumble == true)
        CWL("  ~~~ FUMBLE ~~~  -- Attack Power penalty applied!", ConsoleColor.DarkYellow);
    else if (e.IsHit == true)
    {
        var label = margin >= 8 ? "CRUSHING HIT" : margin >= 4 ? "SOLID HIT" : "GLANCING HIT";
        CWL($"  [ {label} ]", ConsoleColor.Green);
    }
    else
    {
        var label = margin >= -3 ? "NEAR MISS" : "MISS";
        CWL($"  [ {label} ]", ConsoleColor.Red);
    }

    if (e.IsHit == true)
    {
        var dmgIdx = e.Message.IndexOf("Dmg:", StringComparison.Ordinal);
        if (dmgIdx >= 0) { Console.WriteLine(); CW("  Damage  "); CWL(e.Message[dmgIdx..], ConsoleColor.DarkCyan); }
    }

    if (!string.IsNullOrEmpty(e.Phrase))
    {
        Console.WriteLine();
        CWL($"  \"{e.Phrase}\"", ConsoleColor.DarkCyan);
    }
}

// ── ShowHp ────────────────────────────────────────────────────────────────────

void ShowHp(string name, int current, int max, int w = 24)
{
    var pct    = (double)Math.Max(0, current) / Math.Max(1, max);
    var filled = current > 0 ? Math.Max(1, (int)(pct * w)) : 0;
    var barCol = HpColor(current, max);

    Console.Write("  ");
    CW($"{name,-10}", CharColor(name));
    Console.Write("  HP [");
    Console.ForegroundColor = barCol;
    Console.Write(new string('\u2588', filled));
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write(new string('\u2591', w - filled));
    Console.ResetColor();
    Console.WriteLine($"]  {Math.Max(0, current),3} / {max,3}");
}

// ── ShowTm ────────────────────────────────────────────────────────────────────

void ShowTm(string name, int current, bool isReady = false, bool isActive = false, int w = 24)
{
    var filled = (int)(Math.Min(1.0, current / 100.0) * w);

    Console.Write("  ");
    CW($"{name,-10}", CharColor(name));
    Console.Write("  TM [");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(new string('|', filled));
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write(new string(' ', w - filled));
    Console.ResetColor();
    Console.Write($"]  {current,4}");
    if (isActive)     { Console.ForegroundColor = ConsoleColor.Green; Console.Write("  ACTING"); Console.ResetColor(); }
    else if (isReady) { Console.ForegroundColor = ConsoleColor.Cyan;  Console.Write("  READY");  Console.ResetColor(); }
    Console.WriteLine();
}

// ── HpColor ───────────────────────────────────────────────────────────────────

static ConsoleColor HpColor(int current, int max)
{
    var pct = (double)Math.Max(0, current) / Math.Max(1, max);
    return pct > 0.5  ? ConsoleColor.Green
         : pct > 0.25 ? ConsoleColor.Yellow
         :              ConsoleColor.Red;
}

// ── CharColor ─────────────────────────────────────────────────────────────────

ConsoleColor CharColor(string name) =>
    activeActor == "" ? ConsoleColor.White :
    name == activeActor ? ConsoleColor.Green :
    ConsoleColor.DarkGray;

// ── CW / CWL ─────────────────────────────────────────────────────────────────

void CW(string text, ConsoleColor col = ConsoleColor.White)
{
    Console.ForegroundColor = col;
    Console.Write(text);
    Console.ResetColor();
}

void CWL(string text, ConsoleColor col = ConsoleColor.White)
{
    Console.ForegroundColor = col;
    Console.WriteLine(text);
    Console.ResetColor();
}

// ── Sign / DieSides ───────────────────────────────────────────────────────────

static string Sign(int n) => n >= 0 ? "+" : "";

static int DieSides(BattleArena.Core.Entities.Enums.DieType d) => d switch
{
    BattleArena.Core.Entities.Enums.DieType.D4  => 4,
    BattleArena.Core.Entities.Enums.DieType.D6  => 6,
    BattleArena.Core.Entities.Enums.DieType.D8  => 8,
    BattleArena.Core.Entities.Enums.DieType.D10 => 10,
    BattleArena.Core.Entities.Enums.DieType.D12 => 12,
    BattleArena.Core.Entities.Enums.DieType.D20 => 20,
    _                                            => 0
};

// ── ManualConsoleTargetSelector ───────────────────────────────────────────────
// Used in Manual targeting mode: pauses during simulation so the player can
// choose which enemy each hero attacks. Enemy turns remain automatic.
// Console I/O is intentionally synchronous here — this is a console-only demo
// class. A real GUI would implement ITargetSelector via TaskCompletionSource.

class ManualConsoleTargetSelector : ITargetSelector
{
    public Task<Character> SelectTargetAsync(
        Character actor,
        IEnumerable<Character> livingEnemies,
        CancellationToken ct = default)
    {
        var targets = livingEnemies.ToList();

        // When only one target is alive, skip the prompt.
        if (targets.Count == 1) return Task.FromResult(targets[0]);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  MANUAL TARGET  --  {actor.Name} is ready to act!");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("  " + new string('─', 50));
        Console.ResetColor();
        Console.WriteLine();

        for (var i = 0; i < targets.Count; i++)
        {
            var t   = targets[i];
            var pct = (double)Math.Max(0, t.CurrentHitPoints) / Math.Max(1, t.MaxHitPoints);
            var bar = BuildHpBar(t.CurrentHitPoints, t.MaxHitPoints, 16);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  [{i + 1}]  ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{t.Name,-12}");
            Console.ResetColor();
            Console.Write("  HP [");
            Console.ForegroundColor = pct > 0.5 ? ConsoleColor.Green : pct > 0.25 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.Write(bar.filled);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(bar.empty);
            Console.ResetColor();
            Console.WriteLine($"]  {Math.Max(0, t.CurrentHitPoints),3} / {t.MaxHitPoints,3}");
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  > ");
        Console.ResetColor();

        while (true)
        {
            if (ct.IsCancellationRequested) return Task.FromResult(targets[0]);

            var k = Console.ReadKey(true).KeyChar;
            if (int.TryParse(k.ToString(), out var idx) && idx >= 1 && idx <= targets.Count)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(targets[idx - 1].Name);
                Console.ResetColor();
                return Task.FromResult(targets[idx - 1]);
            }
        }
    }

    private static (string filled, string empty) BuildHpBar(int current, int max, int w)
    {
        var pct    = (double)Math.Max(0, current) / Math.Max(1, max);
        var filled = current > 0 ? Math.Max(1, (int)(pct * w)) : 0;
        return (new string('\u2588', filled), new string('\u2591', w - filled));
    }
}
