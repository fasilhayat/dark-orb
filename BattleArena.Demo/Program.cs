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

// ── Block layout constants ────────────────────────────────────────────────────
const int BLOCK_W   = 35;  // total block width including │ border chars
const int CONTENT_W = 31;  // content between border+space: BLOCK_W - 4
const int BAR_W     = 14;  // fill width of TM / HP bars

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

// ── BuildDisplayStates ────────────────────────────────────────────────────────
// Creates an initial display snapshot for every combatant (full HP, TM = 0).

Dictionary<string, CharDisplayState> BuildDisplayStates()
{
    var dict = new Dictionary<string, CharDisplayState>();
    foreach (var m in heroParty.Members)
        dict[m.Character.Name] = new CharDisplayState
        {
            Name   = m.Character.Name,
            MaxHp  = maxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
            Hp     = maxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
            IsHero = true
        };
    foreach (var m in enemyParty.Members)
        dict[m.Character.Name] = new CharDisplayState
        {
            Name   = m.Character.Name,
            MaxHp  = maxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
            Hp     = maxHp.GetValueOrDefault(m.Character.Name, m.Character.MaxHitPoints),
            IsHero = false
        };
    return dict;
}

// ── DrawBattleScreen ──────────────────────────────────────────────────────────
// Clears the console and renders the left-right battle layout:
//   Heroes (left column)  ║  Enemies (right column)
// The active combatant is highlighted with ╔═╗ white borders.

void DrawBattleScreen(Dictionary<string, CharDisplayState> states, int tick)
{
    Console.Clear();
    PrintHeader();

    var heroes  = heroParty.Members.Select(m => states[m.Character.Name]).ToList();
    var enemies = enemyParty.Members.Select(m => states[m.Character.Name]).ToList();

    bool isDuel = scenario == 'D';
    var leftLabel  = isDuel ? "── CHARACTER 1 ──" : "── HEROES ──────";
    var rightLabel = isDuel ? "── CHARACTER 2 ──" : "── ENEMIES ──────";

    Console.WriteLine();
    Console.Write("  ");
    CW($"Tick {tick,-4}  ", ConsoleColor.DarkGray);
    CW(leftLabel,  isDuel ? ConsoleColor.White  : ConsoleColor.Blue);
    CW("─────────── vs ───────────", ConsoleColor.DarkGray);
    CWL(rightLabel, isDuel ? ConsoleColor.White : ConsoleColor.DarkMagenta);
    Console.WriteLine();

    var empty    = BuildEmptyBlock();
    int maxCount = Math.Max(heroes.Count, enemies.Count);

    for (var i = 0; i < maxCount; i++)
    {
        var left  = i < heroes .Count ? BuildCharBlock(heroes [i]) : empty;
        var right = i < enemies.Count ? BuildCharBlock(enemies[i]) : empty;
        PrintBlockPair(left, right);
        if (i < maxCount - 1) Console.WriteLine();
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  " + new string('─', 77));
    Console.ResetColor();
    Console.WriteLine();
}

// ── BuildEmptyBlock / BuildCharBlock ─────────────────────────────────────────

List<List<Seg>> BuildEmptyBlock()
{
    var blank = new List<Seg> { new Seg(new string(' ', BLOCK_W), ConsoleColor.Black) };
    return new List<List<Seg>> { blank, blank, blank, blank, blank };
}

// Returns 5 lines: top border, name+weapon, TM bar, HP bar, bottom border.
// Each line is a List<Seg> whose total visual width = BLOCK_W (35).
//
// Width checks:
//   Top/bot border : ┌ + (CONTENT_W+2)×─ + ┐ = 1+33+1 = 35  ✓
//   Content line   : │ + space + content(31) + space + │ = 35  ✓
//     Name content : indicator(2)+name(10)+"   "(3)+"["+weapon(14)+"]"(16) = 31  ✓
//     TM content   : "  TM ["(6)+bars(14)+"]  "(3)+" {tm,3}"(4)+"/100"(4) = 31  ✓
//     HP content   : " HP ["(5)+bars(14)+"]  "(3)+hp(3)+" / "(3)+max(3)   = 31  ✓
List<List<Seg>> BuildCharBlock(CharDisplayState s)
{
    var active = s.IsActive;
    var dead   = !s.IsAlive;

    var borderFg = active     ? ConsoleColor.White
                 : dead       ? ConsoleColor.DarkGray
                 : s.IsHero   ? ConsoleColor.Blue
                 :               ConsoleColor.DarkMagenta;

    char h  = active ? '═' : '─';
    char tl = active ? '╔' : '┌';
    char tr = active ? '╗' : '┐';
    char bl = active ? '╚' : '└';
    char br = active ? '╝' : '┘';
    char vb = active ? '║' : '│';

    var top = new List<Seg> { new Seg($"{tl}{new string(h, CONTENT_W + 2)}{tr}", borderFg) };
    var bot = new List<Seg> { new Seg($"{bl}{new string(h, CONTENT_W + 2)}{br}", borderFg) };

    if (dead)
    {
        var status   = s.Hp <= -10 ? "[ SLAIN  ]" : "[ K.O.   ]";   // 10 chars
        var namePart = $"  ✕ {s.Name.ToUpper()}".PadRight(CONTENT_W - status.Length);
        var empty    = new string(' ', CONTENT_W);
        return new List<List<Seg>>
        {
            top,
            CL(vb, borderFg, new Seg(namePart, ConsoleColor.DarkGray), new Seg(status, ConsoleColor.DarkRed)),
            CL(vb, borderFg, new Seg(empty, ConsoleColor.DarkGray)),
            CL(vb, borderFg, new Seg(empty, ConsoleColor.DarkGray)),
            bot
        };
    }

    // Name + weapon line
    var indicator = active ? "► " : "  ";
    var indicFg   = active ? ConsoleColor.White : s.IsHero ? ConsoleColor.Cyan : ConsoleColor.Red;
    var nameStr   = (s.Name.Length > 10 ? s.Name.ToUpper()[..10] : s.Name.ToUpper()).PadRight(10);
    var weapTrunc = s.Weapon.Length > 14 ? s.Weapon[..14] : s.Weapon.PadRight(14);
    var weapStr   = $"[{weapTrunc}]"; // always 16 chars

    var nameLine = CL(vb, borderFg,
        new Seg(indicator, indicFg),
        new Seg(nameStr,   active ? ConsoleColor.White : ConsoleColor.White),
        new Seg("   ",     ConsoleColor.DarkGray),
        new Seg(weapStr,   active ? ConsoleColor.Yellow : ConsoleColor.Gray));

    // TM bar line
    var tmFilled = Math.Min(BAR_W, (int)(Math.Min(1.0, s.Tm / 100.0) * BAR_W));
    var tmLine   = CL(vb, borderFg,
        new Seg("  TM [",                       ConsoleColor.DarkGray),
        new Seg(new string('|', tmFilled),       ConsoleColor.Cyan),
        new Seg(new string('░', BAR_W-tmFilled), ConsoleColor.DarkGray),
        new Seg("]  ",                           ConsoleColor.DarkGray),
        new Seg($" {s.Tm,3}",                    ConsoleColor.Cyan),
        new Seg("/100",                          ConsoleColor.DarkGray));

    // HP bar line
    var pct      = (double)Math.Max(0, s.Hp) / Math.Max(1, s.MaxHp);
    var hpFilled = s.Hp > 0 ? Math.Max(1, (int)(pct * BAR_W)) : 0;
    var hpFg     = HpColor(s.Hp, s.MaxHp);
    var hpLine   = CL(vb, borderFg,
        new Seg(" HP [",                        ConsoleColor.DarkGray),
        new Seg(new string('█', hpFilled),      hpFg),
        new Seg(new string('░', BAR_W-hpFilled),ConsoleColor.DarkGray),
        new Seg("]  ",                          ConsoleColor.DarkGray),
        new Seg($"{Math.Max(0, s.Hp),3}",       hpFg),
        new Seg(" / ",                          ConsoleColor.DarkGray),
        new Seg($"{s.MaxHp,-3}",                ConsoleColor.DarkGray));

    return new List<List<Seg>> { top, nameLine, tmLine, hpLine, bot };
}

// Builds a full block content line: vb + ' ' + [segs] + ' ' + vb  (= BLOCK_W chars).
// Content segs must sum to CONTENT_W (31) characters.
List<Seg> CL(char vb, ConsoleColor borderFg, params Seg[] segs)
{
    var line = new List<Seg> { new Seg($"{vb} ", borderFg) };
    line.AddRange(segs);
    line.Add(new Seg($" {vb}", borderFg));
    return line;
}

// ── PrintBlockPair ────────────────────────────────────────────────────────────
// Renders two blocks side by side:  "  " + left(35) + "  ║  " + right(35)  = 77 chars.

void PrintBlockPair(List<List<Seg>> left, List<List<Seg>> right)
{
    var maxLines = Math.Max(left.Count, right.Count);
    var blank    = new List<Seg> { new Seg(new string(' ', BLOCK_W), ConsoleColor.Black) };

    for (var i = 0; i < maxLines; i++)
    {
        var l = i < left .Count ? left [i] : blank;
        var r = i < right.Count ? right[i] : blank;

        Console.Write("  ");
        foreach (var seg in l) { Console.ForegroundColor = seg.Fg; Console.Write(seg.Text); }
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  ║  ");
        Console.ResetColor();
        foreach (var seg in r) { Console.ForegroundColor = seg.Fg; Console.Write(seg.Text); }
        Console.ResetColor();
        Console.WriteLine();
    }
}

// ── PlayTurnBased ─────────────────────────────────────────────────────────────
// Replays the battle log one turn at a time.  Each keypress advances one turn.
// State (HP, TM, alive) is updated incrementally from log events before display.

void PlayTurnBased()
{
    var states     = BuildDisplayStates();
    var turnEvents = new List<BattleLogEntry>();
    bool inTurn    = false;
    int  turnCount = 0;
    int  turnTick  = 0;
    string actorName = "";

    void FlushTurn()
    {
        if (!inTurn || turnEvents.Count == 0) return;

        var ts    = turnEvents.First(e => e.EventType == "TurnStart");
        var actSt = states.GetValueOrDefault(ts.ActorName);
        var tgtSt = states.GetValueOrDefault(ts.TargetName ?? "");

        DrawBattleScreen(states, turnTick);

        Console.WriteLine();
        CW($"  Turn {turnCount}  ", ConsoleColor.DarkGray);
        CW("│  ", ConsoleColor.DarkGray);
        CW(ts.ActorName.ToUpper(), actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
        CW("  ─→  ", ConsoleColor.DarkGray);
        CWL(ts.TargetName?.ToUpper() ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
        Console.WriteLine();

        foreach (var e in turnEvents)
        {
            switch (e.EventType)
            {
                case "TurnStart":
                {
                    var verb = e.IsSpell == true ? "conjures" : "readies";
                    CW("  >> ", ConsoleColor.DarkCyan);
                    CW(e.ActorName, actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
                    CW($" {verb} ");
                    CW($"[{e.AttackSourceName}]", e.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
                    CW(" targeting ");
                    CW(ts.TargetName ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
                    CWL("!", ConsoleColor.White);
                    Console.WriteLine();
                    break;
                }
                case "Attack":
                    PrintAttack(e);
                    break;
                case "Damage":
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, ConsoleColor.White);
                    CW(" takes "); CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage   HP: {e.TargetHpBefore} -> {Math.Max(0, e.TargetHpAfter ?? 0)}", ConsoleColor.DarkGray);
                    break;
                case "FumblePenalty":
                    CWL($"\n  {e.Message}", ConsoleColor.DarkYellow);
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
        CWL("  " + new string('─', 77), ConsoleColor.DarkGray);
        var over = turnEvents.Any(e => e.EventType is "Death" or "KnockedOut");
        CWL(over ? "  Battle over!  Press any key for results..."
                 : "  Press any key for next turn...", ConsoleColor.DarkGray);
        Console.ReadKey(true);

        if (states.TryGetValue(actorName, out var actorDisp)) actorDisp.IsActive = false;
        turnEvents.Clear();
        inTurn = false;
    }

    foreach (var e in result.Log)
    {
        switch (e.EventType)
        {
            case "TurnMeterGain":
                if (states.TryGetValue(e.ActorName, out var tmSt))
                    tmSt.Tm = e.TurnMeterAfter ?? 0;
                break;

            case "TurnStart":
                FlushTurn();
                inTurn    = true;
                turnCount++;
                turnTick  = e.Tick;
                actorName = e.ActorName;
                if (states.TryGetValue(e.ActorName, out var actSt2))
                {
                    actSt2.IsActive = true;
                    actSt2.Weapon   = e.AttackSourceName ?? "";
                }
                turnEvents.Add(e);
                break;

            case "Damage":
                if (states.TryGetValue(e.ActorName, out var dmgSt))
                    dmgSt.Hp = e.TargetHpAfter ?? dmgSt.Hp;
                if (inTurn) turnEvents.Add(e);
                break;

            case "Death":
            case "KnockedOut":
                if (states.TryGetValue(e.ActorName, out var defSt))
                { defSt.IsAlive = false; defSt.IsActive = false; }
                if (inTurn) turnEvents.Add(e);
                break;

            default:
                if (inTurn) turnEvents.Add(e);
                break;
        }
    }

    FlushTurn();
    activeActor = "";
}

// ── PlayRealTime ──────────────────────────────────────────────────────────────

void PlayRealTime()
{
    var states  = BuildDisplayStates();
    var byTick  = result.Log.GroupBy(e => e.Tick).OrderBy(g => g.Key).ToList();

    // Track quiet TM-only runs to summarise them as "... N ticks pass"
    int quietStart = -1, quietEnd = -1;

    void FlushQuiet()
    {
        if (quietStart < 0) return;
        if (quietEnd > quietStart + 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  ... {quietEnd - quietStart + 1} quiet ticks (TM building)");
            Console.ResetColor();
            Thread.Sleep(80);
        }
        quietStart = quietEnd = -1;
    }

    foreach (var tickGroup in byTick)
    {
        var entries   = tickGroup.ToList();
        var hasAction = entries.Any(e => e.EventType == "TurnStart");

        // Apply TM updates regardless of whether this is a quiet tick
        foreach (var e in entries.Where(e => e.EventType == "TurnMeterGain"))
            if (states.TryGetValue(e.ActorName, out var st)) st.Tm = e.TurnMeterAfter ?? 0;

        if (!hasAction)
        {
            if (quietStart < 0) quietStart = tickGroup.Key;
            quietEnd = tickGroup.Key;
            Thread.Sleep(40);
            continue;
        }

        FlushQuiet();

        var turnStart = entries.First(e => e.EventType == "TurnStart");
        var attacker  = turnStart.ActorName;
        activeActor   = attacker;

        // Mark active / update weapon
        foreach (var st in states.Values) st.IsActive = false;
        if (states.TryGetValue(attacker, out var actSt))
        {
            actSt.IsActive = true;
            actSt.Weapon   = turnStart.AttackSourceName ?? actSt.Weapon;
        }

        DrawBattleScreen(states, tickGroup.Key);
        Thread.Sleep(300);

        foreach (var e in entries)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    break;

                case "TurnStart":
                {
                    var tgtSt  = states.GetValueOrDefault(e.TargetName ?? "");
                    var verb   = e.IsSpell == true ? "conjures" : "readies";
                    Console.WriteLine();
                    CW("  >> ", ConsoleColor.DarkCyan);
                    CW(e.ActorName, actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
                    CW($" {verb} ");
                    CW($"[{e.AttackSourceName}]", e.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
                    CW(" targeting ");
                    CW(e.TargetName ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
                    CWL("!", ConsoleColor.White);
                    Thread.Sleep(200);
                    break;
                }

                case "Attack":
                    Thread.Sleep(500);
                    PrintAttack(e);
                    break;

                case "Damage":
                {
                    Thread.Sleep(200);
                    if (states.TryGetValue(e.ActorName, out var dmgSt))
                        dmgSt.Hp = e.TargetHpAfter ?? dmgSt.Hp;
                    Console.WriteLine();
                    CW("  "); CW(e.ActorName, ConsoleColor.White);
                    CW(" takes "); CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage   HP: {e.TargetHpBefore} -> {Math.Max(0, e.TargetHpAfter ?? 0)}", ConsoleColor.DarkGray);
                    Thread.Sleep(800);
                    break;
                }

                case "FumblePenalty":
                    CWL($"  {e.Message}", ConsoleColor.DarkYellow);
                    break;

                case "TurnEnd":
                    if (states.TryGetValue(e.ActorName, out var endSt))
                    { endSt.IsActive = false; endSt.Tm = e.TurnMeterAfter ?? endSt.Tm; }
                    Console.WriteLine();
                    CWL("  " + new string('─', 77), ConsoleColor.DarkGray);
                    Thread.Sleep(300);
                    break;

                case "Death":
                {
                    if (states.TryGetValue(e.ActorName, out var deathSt))
                    { deathSt.IsAlive = false; deathSt.IsActive = false; }
                    Thread.Sleep(500);
                    Console.WriteLine();
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    CWL($"  *** {e.Message} ***", ConsoleColor.Red);
                    CWL("  " + new string('*', 65), ConsoleColor.Red);
                    Thread.Sleep(1500);
                    break;
                }

                case "KnockedOut":
                {
                    if (states.TryGetValue(e.ActorName, out var koSt))
                    { koSt.IsAlive = false; koSt.IsActive = false; }
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

// ── Seg / CharDisplayState ────────────────────────────────────────────────────
// Lightweight types used by the left/right visual layout helpers.

record Seg(string Text, ConsoleColor Fg = ConsoleColor.Gray);

class CharDisplayState
{
    public required string Name   { get; init; }
    public required int    MaxHp  { get; init; }
    public required bool   IsHero { get; init; }
    public int    Hp       { get; set; }
    public int    Tm       { get; set; }
    public bool   IsActive { get; set; }
    public bool   IsAlive  { get; set; } = true;
    public string Weapon   { get; set; } = "";
}

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
