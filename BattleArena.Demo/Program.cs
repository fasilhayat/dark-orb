using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "BattleArena — Combat Simulation";

var combatStats = new CombatStatsService();
var simulator = new BattleSimulator(
    new CombatService(new DiceService(), combatStats),
    new TurnmeterService(),
    new StatusEffectService());

var longsword = new BattleArena.Core.Entities.Weapon
{
    Name = "Longsword",
    DamageDie = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 1,
    DamageType = BattleArena.Core.Entities.Enums.DamageType.Slashing,
    AttackType = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 2
};
var theron = new BattleArena.Core.Entities.Character
{
    Name = "Theron",
    Level = 5,
    Strength = 18,
    Dexterity = 12,
    Intelligence = 10,
    StrikeRating = 14,
    TurnSpeed = 10,
    MaxHitPoints = 50,
    CurrentHitPoints = 50,
    Equipment = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest = new BattleArena.Core.Entities.Armor { Name = "Chain Mail", ArmorClass = 5, Mitigation = 2, MaxDexterityBonus = 6 },
        RightHand = longsword
    }
};

var battleAxe = new BattleArena.Core.Entities.Weapon
{
    Name = "Battle Axe",
    DamageDie = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 1,
    DamageType = BattleArena.Core.Entities.Enums.DamageType.Slashing,
    AttackType = BattleArena.Core.Entities.Enums.AttackType.Melee,
    AttackBonus = 1
};
var gruk = new BattleArena.Core.Entities.Character
{
    Name = "Gruk",
    Level = 3,
    Strength = 16,
    Dexterity = 8,
    Intelligence = 8,
    StrikeRating = 16,
    TurnSpeed = 6,
    MaxHitPoints = 35,
    CurrentHitPoints = 35,
    Equipment = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest = new BattleArena.Core.Entities.Armor { Name = "Leather Armor", ArmorClass = 7, Mitigation = 1, MaxDexterityBonus = 6 },
        RightHand = battleAxe
    }
};

var fireball = new BattleArena.Core.Entities.Spell
{
    Name = "Fireball",
    Description = "A blazing orb of fire",
    School = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie = BattleArena.Core.Entities.Enums.DieType.D6,
    DamageCount = 3,
    DamageType = BattleArena.Core.Entities.Enums.DamageType.Fire,
    AttackBonus = 2,
    SpellLevel = 3
};
var iceBolt = new BattleArena.Core.Entities.Spell
{
    Name = "Ice Bolt",
    Description = "A shard of magical ice",
    School = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie = BattleArena.Core.Entities.Enums.DieType.D8,
    DamageCount = 2,
    DamageType = BattleArena.Core.Entities.Enums.DamageType.Cold,
    AttackBonus = 2,
    SpellLevel = 2
};
var lightningStrike = new BattleArena.Core.Entities.Spell
{
    Name = "Lightning Strike",
    Description = "A bolt of crackling lightning",
    School = BattleArena.Core.Entities.Enums.SpellSchool.Evocation,
    DamageDie = BattleArena.Core.Entities.Enums.DieType.D10,
    DamageCount = 2,
    DamageType = BattleArena.Core.Entities.Enums.DamageType.Lightning,
    AttackBonus = 3,
    SpellLevel = 4
};
var lyra = new BattleArena.Core.Entities.Character
{
    Name = "Lyra",
    Level = 5,
    Strength = 8,
    Dexterity = 14,
    Intelligence = 18,
    StrikeRating = 13,
    TurnSpeed = 8,
    MaxHitPoints = 30,
    CurrentHitPoints = 30,
    Equipment = new BattleArena.Core.Entities.ArmorSlots
    {
        Chest = new BattleArena.Core.Entities.Armor { Name = "Mage Robes", ArmorClass = 14, Mitigation = 0, MaxDexterityBonus = 6 }
    },
    MemorizedSpells = new List<BattleArena.Core.Entities.Spell> { fireball, iceBolt, lightningStrike }
};

var characterMap = new Dictionary<char, BattleArena.Core.Entities.Character>
{
    ['T'] = theron,
    ['G'] = gruk,
    ['L'] = lyra
};
var attackMap = new Dictionary<string, BattleArena.Core.Entities.IAttackSource?>
{
    [theron.Name] = longsword,
    [gruk.Name] = battleAxe,
    [lyra.Name] = null
};

string activeActor = "";

PrintHeader();
var fighter1 = PickFighter("Fighter 1", null);
var fighter2 = PickFighter("Fighter 2", fighter1.Name);
var fighter1Attack = attackMap[fighter1.Name];
var fighter2Attack = attackMap[fighter2.Name];

ResetCombatant(theron);
ResetCombatant(gruk);
ResetCombatant(lyra);

var fighter1SheetSource = GetSheetAttackSource(fighter1, fighter1Attack);
var fighter2SheetSource = GetSheetAttackSource(fighter2, fighter2Attack);
var fighter1Ap = combatStats.ComputeAttackerStats(fighter1, fighter1SheetSource).AttackPower;
var fighter1Dp = combatStats.ComputeDefenderStats(fighter1).DefensePower;
var fighter2Ap = combatStats.ComputeAttackerStats(fighter2, fighter2SheetSource).AttackPower;
var fighter2Dp = combatStats.ComputeDefenderStats(fighter2).DefensePower;

Console.WriteLine();
ShowSheet("FIGHTER 1", fighter1, fighter1Attack, fighter1Ap, fighter1Dp);
CWL("\n                           --- VS ---\n", ConsoleColor.DarkGray);
ShowSheet("FIGHTER 2", fighter2, fighter2Attack, fighter2Ap, fighter2Dp);

var maxHp = new Dictionary<string, int>
{
    [fighter1.Name] = fighter1.MaxHitPoints,
    [fighter2.Name] = fighter2.MaxHitPoints
};
var hp = new Dictionary<string, int>
{
    [fighter1.Name] = fighter1.MaxHitPoints,
    [fighter2.Name] = fighter2.MaxHitPoints
};

CWL("\n  Choose battle mode:", ConsoleColor.Yellow);
CW("    "); CW("[T]", ConsoleColor.Cyan); CWL("  Turn-based  -- press any key to advance each turn", ConsoleColor.White);
CW("    "); CW("[R]", ConsoleColor.Cyan); CWL("  Real-time   -- fully automatic tick-by-tick playback\n", ConsoleColor.White);
CW("  > ", ConsoleColor.Cyan);

char mode;
while (true)
{
    var k = Console.ReadKey(true).KeyChar;
    if (k is 'T' or 't') { CWL("Turn-based", ConsoleColor.Cyan); mode = 'T'; break; }
    if (k is 'R' or 'r') { CWL("Real-time", ConsoleColor.Cyan); mode = 'R'; break; }
}

CWL("\n  Press any key to start the battle...", ConsoleColor.DarkGray);
Console.ReadKey(true);
Console.Clear();
PrintHeader();

var result = simulator.Simulate(fighter1, fighter1Attack, fighter2, fighter2Attack, 500);

if (mode == 'T')
    PlayTurnBased();
else
    PlayRealTime();

PrintSummary();

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
        if (!characterMap.TryGetValue(pick, out var selected))
            continue;

        if (selected.Name == excludedName)
        {
            CWL($"  {selected.Name} is already selected. Choose another fighter.\n", ConsoleColor.DarkYellow);
            continue;
        }

        CWL(selected.Name, CharColor(selected.Name));
        return selected;
    }
}

void ResetCombatant(BattleArena.Core.Entities.Character character)
{
    character.CurrentHitPoints = character.MaxHitPoints;
    character.ActiveStatusEffects.Clear();
}

BattleArena.Core.Entities.IAttackSource GetSheetAttackSource(BattleArena.Core.Entities.Character character, BattleArena.Core.Entities.IAttackSource? attackSource)
{
    if (attackSource is not null)
        return attackSource;

    return character.MemorizedSpells
        .OrderByDescending(spell => spell.AttackBonus)
        .ThenByDescending(spell => spell.DamageCount)
        .First();
}

void PlayTurnBased()
{
    var turns = new List<List<BattleLogEntry>>();
    List<BattleLogEntry>? current = null;

    foreach (var e in result.Log)
    {
        if (e.EventType == "TurnStart")
        {
            current = new List<BattleLogEntry>();
            turns.Add(current);
        }

        if (current != null && e.EventType != "TurnMeterGain")
            current.Add(e);
    }

    for (var idx = 0; idx < turns.Count; idx++)
    {
        var turnEntries = turns[idx];
        var tick = turnEntries.FirstOrDefault()?.Tick ?? 0;

        Console.Clear();
        PrintHeader();

        var actorName = turnEntries.FirstOrDefault(e => e.EventType == "TurnStart")?.ActorName ?? "?";
        var targetName = actorName == fighter1.Name ? fighter2.Name : fighter1.Name;
        var actorMax = maxHp[actorName];
        var targetMax = maxHp[targetName];

        activeActor = actorName;

        CWL($"\n  Turn {idx + 1}  |  Tick {tick}", ConsoleColor.DarkGray);
        CW("  "); CW($"{actorName.ToUpper()}", CharColor(actorName));
        CW($"  HP "); CW($"{hp[actorName]}/{actorMax}", hp[actorName] > actorMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CW("   vs   ");
        CW($"{targetName.ToUpper()}", CharColor(targetName));
        CW($"  HP "); CWL($"{hp[targetName]}/{targetMax}", hp[targetName] > targetMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);
        Console.WriteLine();

        ShowHp(fighter1.Name, hp[fighter1.Name], maxHp[fighter1.Name]);
        ShowHp(fighter2.Name, hp[fighter2.Name], maxHp[fighter2.Name]);
        Console.WriteLine();

        foreach (var tme in result.Log.Where(e => e.EventType == "TurnMeterGain" && e.Tick == tick))
            ShowTm(tme.ActorName, tme.TurnMeterAfter ?? 0, tme.IsReady, tme.IsActive);

        Console.WriteLine();

        foreach (var e in turnEntries)
        {
            switch (e.EventType)
            {
                case "TurnStart":
                {
                    var target = e.ActorName == fighter1.Name ? fighter2.Name : fighter1.Name;
                    CW("  >> ", ConsoleColor.DarkCyan);
                    CW($"{e.ActorName.ToUpper()}", CharColor(e.ActorName));
                    CW(" readies their attack on ");
                    CW(target, CharColor(target));
                    CWL("!", ConsoleColor.White);
                    Console.WriteLine();
                    break;
                }

                case "Attack":
                    PrintAttack(e);
                    break;

                case "Damage":
                    hp[e.ActorName] = e.TargetHpAfter ?? hp[e.ActorName];
                    Console.WriteLine();
                    CW("  ");
                    CW($"{e.ActorName}", CharColor(e.ActorName));
                    CW(" takes ");
                    CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowHp(fighter1.Name, hp[fighter1.Name], maxHp[fighter1.Name]);
                    ShowHp(fighter2.Name, hp[fighter2.Name], maxHp[fighter2.Name]);
                    break;

                case "FumblePenalty":
                    CWL($"\n  {e.Message}", ConsoleColor.DarkYellow);
                    break;

                case "TurnEnd":
                    Console.WriteLine();
                    CW("  ");
                    CW(e.ActorName, CharColor(e.ActorName));
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
        var battleOver = turnEntries.Any(e => e.EventType == "Death" || e.EventType == "KnockedOut");
        CWL(battleOver
            ? "  Battle over!  Press any key for results..."
            : "  Press any key for next turn...",
            ConsoleColor.DarkGray);
        Console.ReadKey(true);
    }

    activeActor = "";
}

void PlayRealTime()
{
    var byTick = result.Log
        .GroupBy(e => e.Tick)
        .OrderBy(g => g.Key)
        .ToList();

    CWL("\n  BATTLE BEGINS\n", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.DarkCyan);

    var curHp = new Dictionary<string, int>
    {
        [fighter1.Name] = fighter1.MaxHitPoints,
        [fighter2.Name] = fighter2.MaxHitPoints
    };

    int quietStart = -1;
    int quietEnd = -1;
    var quietTmStart = new Dictionary<string, int>();
    var quietTmEnd = new Dictionary<string, int>();

    void FlushQuiet()
    {
        if (quietStart < 0) return;
        if (quietEnd > quietStart + 1)
        {
            var parts = quietTmStart.Keys.Select(n =>
                $"{n}  {quietTmStart[n]}%->{quietTmEnd[n]}%");
            CWL($"\n  ... {quietEnd - quietStart + 1} ticks pass  |  " +
                string.Join("   ", parts), ConsoleColor.DarkGray);
            Thread.Sleep(80);
        }

        quietStart = quietEnd = -1;
        quietTmStart.Clear();
        quietTmEnd.Clear();
    }

    foreach (var tickGroup in byTick)
    {
        var entries = tickGroup.ToList();
        var hasAction = entries.Any(e => e.EventType == "TurnStart");

        if (!hasAction)
        {
            foreach (var e in entries.Where(e => e.EventType == "TurnMeterGain"))
            {
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
        var attacker = turnStart.ActorName;
        var target = attacker == fighter1.Name ? fighter2.Name : fighter1.Name;

        var attackerHp = curHp[attacker];
        var targetHp = curHp[target];
        var attackerMax = maxHp[attacker];
        var targetMax = maxHp[target];

        activeActor = attacker;

        Console.WriteLine();
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);
        CW($"  Tick {tickGroup.Key,-3}  |  ", ConsoleColor.DarkGray);
        CW($"{attacker.ToUpper()}", CharColor(attacker));
        CW("  HP ");
        CW($"{attackerHp}/{attackerMax}", attackerHp > attackerMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CW("   vs   ");
        CW($"{target.ToUpper()}", CharColor(target));
        CW("  HP ");
        CWL($"{targetHp}/{targetMax}", targetHp > targetMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);

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
                    CW($"{e.ActorName.ToUpper()}", CharColor(e.ActorName));
                    CW(" readies their attack on ");
                    CW(target, CharColor(target));
                    CWL("!", ConsoleColor.White);
                    break;

                case "Attack":
                    Thread.Sleep(500);
                    PrintAttack(e);
                    break;

                case "Damage":
                    Thread.Sleep(200);
                    curHp[e.ActorName] = e.TargetHpAfter ?? curHp[e.ActorName];
                    hp[e.ActorName] = curHp[e.ActorName];
                    Console.WriteLine();
                    CW("  ");
                    CW($"{e.ActorName}", CharColor(e.ActorName));
                    CW(" takes ");
                    CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowHp(fighter1.Name, curHp[fighter1.Name], fighter1.MaxHitPoints);
                    ShowHp(fighter2.Name, curHp[fighter2.Name], fighter2.MaxHitPoints);
                    Thread.Sleep(800);
                    break;

                case "FumblePenalty":
                    CWL($"  {e.Message}", ConsoleColor.DarkYellow);
                    break;

                case "TurnEnd":
                    Console.WriteLine();
                    CW("  ");
                    CW(e.ActorName, CharColor(e.ActorName));
                    CWL(" ends their turn.", ConsoleColor.DarkGray);
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

void PrintSummary()
{
    Console.Clear();
    PrintHeader();

    var winner = result.Winner;
    var loser = result.Loser;

    Console.WriteLine();
    CW("  BATTLE COMPLETE  --  ", ConsoleColor.Green);
    CW(winner?.Name ?? "?", CharColor(winner?.Name ?? "?"));
    CWL(" WINS!", ConsoleColor.Green);
    CWL("  " + new string('=', 62), ConsoleColor.Cyan);

    var attacks = result.Log.Where(e => e.EventType == "Attack").ToList();
    var hits = attacks.Count(e => e.IsHit == true);
    var misses = attacks.Count(e => e.IsHit == false && e.IsFumble == false);
    var crits = attacks.Count(e => e.IsCritical == true);
    var fumbles = attacks.Count(e => e.IsFumble == true);
    var winnerDmg = attacks.Where(e => e.ActorName == winner?.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);
    var loserDmg = attacks.Where(e => e.ActorName == loser?.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);

    CWL($"\n  Total actions :  {attacks.Count}", ConsoleColor.White);
    CW("  Results       :  "); CW($"{hits} hits", ConsoleColor.Green);
    CW($" / {misses} misses"); CW($" / {crits} crits", ConsoleColor.Magenta);
    CWL($" / {fumbles} fumbles", ConsoleColor.DarkYellow);

    CWL("\n  Damage dealt:", ConsoleColor.White);
    CW("    "); CW($"{winner?.Name,-12}", CharColor(winner?.Name ?? "?")); CW($"  {winnerDmg,3} dmg ", ConsoleColor.Yellow); CW("dealt to "); CWL(loser?.Name ?? "?", CharColor(loser?.Name ?? "?"));
    CW("    "); CW($"{loser?.Name,-12}", CharColor(loser?.Name ?? "?")); CW($"  {loserDmg,3} dmg ", ConsoleColor.Yellow); CW("dealt to "); CWL(winner?.Name ?? "?", CharColor(winner?.Name ?? "?"));

    CWL("\n  Final state:", ConsoleColor.White);
    ShowHp(winner?.Name ?? "?", winner?.CurrentHitPoints ?? 0, maxHp.GetValueOrDefault(winner?.Name ?? "", 1));
    ShowHp(loser?.Name ?? "?", loser?.CurrentHitPoints ?? 0, maxHp.GetValueOrDefault(loser?.Name ?? "", 1));

    var loserTag  = result.LoserStatus == BattleArena.Core.Entities.Enums.CharacterVitalStatus.Dead
        ? "SLAIN"
        : "KNOCKED OUT";
    CWL($"\n  {loser?.Name} is {loserTag}! (HP: {loser?.CurrentHitPoints})", 
        result.LoserStatus == BattleArena.Core.Entities.Enums.CharacterVitalStatus.Dead ? ConsoleColor.Red : ConsoleColor.DarkYellow);

    CWL($"\n  Battle length :  {result.TotalTicks} ticks", ConsoleColor.White);
    CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
    Console.WriteLine();
}

void PrintHeader()
{
    CWL("  " + new string('=', 65), ConsoleColor.Cyan);
    CWL("        ***  BATTLE ARENA  --  COMBAT SIMULATION DEMO  ***", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.Cyan);
}

void ShowSheet(string role, BattleArena.Core.Entities.Character ch, BattleArena.Core.Entities.IAttackSource? attackSource, int ap, int dp)
{
    var displaySource = attackSource ?? GetSheetAttackSource(ch, attackSource);
    var abilityScore = displaySource.UsesIntelligence ? ch.Intelligence : displaySource.AttackType == BattleArena.Core.Entities.Enums.AttackType.Ranged ? ch.Dexterity : ch.Strength;
    var abilityMod = (abilityScore - 10) / 2;
    var dexMod = (ch.Dexterity - 10) / 2;
    var dexCap = Math.Min(dexMod, ch.Equipment.Chest?.MaxDexterityBonus ?? 6);
    var ac = ch.Equipment.Chest?.ArmorClass ?? 0;
    var mit = ch.Equipment.Chest?.Mitigation ?? 0;

    const int IW = 60;

    void Sep() =>
        CWL("  +" + new string('-', IW + 2) + "+", ConsoleColor.Cyan);

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
        var inner = " " + left;
        var padding = IW - inner.Length - right.Length;
        var line = inner + new string(' ', Math.Max(1, padding)) + right;
        CW("  | ", ConsoleColor.Cyan);
        Console.ForegroundColor = col;
        Console.Write(line.PadRight(IW));
        Console.ResetColor();
        CWL(" |", ConsoleColor.Cyan);
    }

    Sep();
    Row2($"{role}: {ch.Name}", $"Level {ch.Level}", CharColor(ch.Name));
    Sep();
    Row($"HP: {ch.MaxHitPoints}   TurnSpeed: {ch.TurnSpeed}   StrikeRating: {ch.StrikeRating}");
    Row($"STR: {ch.Strength} ({Sign((ch.Strength - 10) / 2)}{(ch.Strength - 10) / 2})   DEX: {ch.Dexterity} ({Sign(dexMod)}{dexMod})   INT: {ch.Intelligence} ({Sign((ch.Intelligence - 10) / 2)}{(ch.Intelligence - 10) / 2})");
    Sep();

    Row($"Armor   : {ch.Equipment.Chest?.Name ?? "None",-18} AC {ac,-2}  EffAC {20 - ac,-2}  Mitigation: {mit}");
    if (ch.MemorizedSpells.Count > 0)
    {
        foreach (var spell in ch.MemorizedSpells)
            Row($"Spells  : {spell.Name,-18} {spell.DamageCount}d{DieSides(spell.DamageDie)} {spell.DamageType}");
    }
    else if (attackSource is not null)
    {
        Row($"Weapon  : {attackSource.Name,-18} {attackSource.DamageCount}d{DieSides(attackSource.DamageDie)} {attackSource.DamageType,-10} +{attackSource.AttackBonus} atk bonus");
    }

    Sep();
    var abilityLabel = displaySource.UsesIntelligence ? "int" : displaySource.AttackType == BattleArena.Core.Entities.Enums.AttackType.Ranged ? "dex" : "str";
    Row($"Atk Power : {ap,-4}  (20-{ch.StrikeRating}) + {ch.Level} (lvl) + ({Sign(abilityMod)}{abilityMod}) ({abilityLabel}) + {displaySource.AttackBonus} (src)");
    Row($"Def Power : {dp,-4}  (20-{ac}) + ({Sign(dexCap)}{dexCap}) (dex)");
    Sep();
    Console.WriteLine();
}

void PrintAttack(BattleLogEntry e)
{
    var total = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
    var margin = total - (e.DefensePower ?? 0);

    Console.WriteLine();
    CW("  Roll  "); CW($"d20 = {e.DieRoll,2}", ConsoleColor.Yellow);
    CW("   Attack Power "); CW($"{e.AttackPower}", ConsoleColor.Yellow);
    CW("  =  Total "); CW($"{total,2}", ConsoleColor.White);
    CW("   vs  Defence "); CW($"{e.DefensePower}", ConsoleColor.Yellow);
    CW("   |  margin ");
    if (margin >= 0) CWL($"+{margin}", ConsoleColor.Green);
    else CWL($"{margin}", ConsoleColor.Red);

    Console.WriteLine();
    if (e.IsCritical == true)
    {
        CWL("  !!! CRITICAL HIT !!!  -- Double damage!", ConsoleColor.Magenta);
    }
    else if (e.IsFumble == true)
    {
        CWL("  ~~~ FUMBLE ~~~  -- Attack Power penalty applied!", ConsoleColor.DarkYellow);
    }
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
        if (dmgIdx >= 0)
        {
            Console.WriteLine();
            CW("  Damage  "); CWL(e.Message[dmgIdx..], ConsoleColor.DarkCyan);
        }
    }

    if (!string.IsNullOrEmpty(e.Phrase))
    {
        Console.WriteLine();
        CWL($"  \"{e.Phrase}\"", ConsoleColor.DarkCyan);
    }
}

void ShowHp(string name, int current, int max, int w = 24)
{
    var pct = (double)Math.Max(0, current) / Math.Max(1, max);
    var filled = (int)(pct * w);
    var barCol = pct > 0.5 ? ConsoleColor.Green : pct > 0.25 ? ConsoleColor.Yellow : ConsoleColor.Red;

    Console.Write("  ");
    CW($"{name,-10}", CharColor(name));
    Console.Write("  HP [");
    Console.ForegroundColor = barCol;
    Console.Write(new string('#', filled) + new string('-', w - filled));
    Console.ResetColor();
    Console.WriteLine($"]  {Math.Max(0, current),3} / {max,3}");
}

void ShowTm(string name, int current, bool isReady = false, bool isActive = false, int w = 24)
{
    var filled = (int)(Math.Min(1.0, current / 100.0) * w);
    var barCol = isActive ? ConsoleColor.Green : isReady ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
    var nameCol = CharColor(name);

    Console.Write("  ");
    CW($"{name,-10}", nameCol);
    Console.Write("  TM [");
    Console.ForegroundColor = barCol;
    Console.Write(new string('|', filled) + new string('.', w - filled));
    Console.ResetColor();
    Console.Write($"]  {current,4}");
    if (isActive) { Console.ForegroundColor = ConsoleColor.Green; Console.Write("  ACTING"); Console.ResetColor(); }
    else if (isReady) { Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("  READY"); Console.ResetColor(); }
    Console.WriteLine();
}

ConsoleColor CharColor(string name) =>
    activeActor == "" ? ConsoleColor.White :
    name == activeActor ? ConsoleColor.Green :
    ConsoleColor.DarkGray;

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

static string Sign(int n) => n >= 0 ? "+" : "";

static int DieSides(BattleArena.Core.Entities.Enums.DieType d) => d switch
{
    BattleArena.Core.Entities.Enums.DieType.D4 => 4,
    BattleArena.Core.Entities.Enums.DieType.D6 => 6,
    BattleArena.Core.Entities.Enums.DieType.D8 => 8,
    BattleArena.Core.Entities.Enums.DieType.D10 => 10,
    BattleArena.Core.Entities.Enums.DieType.D12 => 12,
    BattleArena.Core.Entities.Enums.DieType.D20 => 20,
    _ => 0
};
