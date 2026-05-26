using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

// ── Encoding (needed for block-character HP/TM bars on Windows) ───────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "BattleArena — Combat Simulation";

// ── Wire up services (no DI container needed for the demo) ────────────────────
var combatStats = new CombatStatsService();
var simulator   = new BattleSimulator(
    new CombatService(new DiceService(), combatStats),
    new TurnmeterService(),
    new StatusEffectService());

// ── Build combatants ──────────────────────────────────────────────────────────

var longsword = new Weapon
{
    Name = "Longsword", DamageDie = DieType.D8, DamageCount = 1,
    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 2
};
var theron = new Character
{
    Name = "Theron", Level = 5, Strength = 18, Dexterity = 12,
    StrikeRating = 14, TurnSpeed = 10, MaxHitPoints = 50, CurrentHitPoints = 50,
    Equipment = new ArmorSlots
    {
        Chest     = new Armor { Name = "Chain Mail",    ArmorClass = 5, Mitigation = 2, MaxDexterityBonus = 6 },
        RightHand = longsword
    }
};

var battleAxe = new Weapon
{
    Name = "Battle Axe", DamageDie = DieType.D8, DamageCount = 1,
    DamageType = DamageType.Slashing, AttackType = AttackType.Melee, AttackBonus = 1
};
var gruk = new Character
{
    Name = "Gruk", Level = 3, Strength = 16, Dexterity = 8,
    StrikeRating = 16, TurnSpeed = 6, MaxHitPoints = 35, CurrentHitPoints = 35,
    Equipment = new ArmorSlots
    {
        Chest     = new Armor { Name = "Leather Armor", ArmorClass = 7, Mitigation = 1, MaxDexterityBonus = 6 },
        RightHand = battleAxe
    }
};

var maxHp = new Dictionary<string, int>
{
    [theron.Name] = theron.MaxHitPoints,
    [gruk.Name]   = gruk.MaxHitPoints
};

// HP tracker — updated as Damage events are processed during playback
var hp = new Dictionary<string, int>
{
    [theron.Name] = theron.MaxHitPoints,
    [gruk.Name]   = gruk.MaxHitPoints
};

// Tracks who is currently acting — drives CharColor() during battle.
// Empty string means "not in battle" (used for character sheet / summary).
string activeActor = "";

// ── Intro screen ──────────────────────────────────────────────────────────────

PrintHeader();

var theronAp = combatStats.ComputeAttackerStats(theron, longsword).AttackPower;
var theronDp = combatStats.ComputeDefenderStats(theron).DefensePower;
var grukAp   = combatStats.ComputeAttackerStats(gruk, battleAxe).AttackPower;
var grukDp   = combatStats.ComputeDefenderStats(gruk).DefensePower;

ShowSheet("FIGHTER", theron, longsword, theronAp, theronDp);
CWL("\n                           --- VS ---\n", ConsoleColor.DarkGray);
ShowSheet("ORC", gruk, battleAxe, grukAp, grukDp);

// ── Mode selection ────────────────────────────────────────────────────────────

CWL("\n  Choose battle mode:", ConsoleColor.Yellow);
CW("    "); CW("[T]", ConsoleColor.Cyan); CWL("  Turn-based  -- press any key to advance each turn", ConsoleColor.White);
CW("    "); CW("[R]", ConsoleColor.Cyan); CWL("  Real-time   -- fully automatic tick-by-tick playback\n", ConsoleColor.White);
CW("  > ", ConsoleColor.Cyan);

char mode;
while (true)
{
    var k = Console.ReadKey(true).KeyChar;
    if (k is 'T' or 't') { CWL("Turn-based", ConsoleColor.Cyan);  mode = 'T'; break; }
    if (k is 'R' or 'r') { CWL("Real-time",  ConsoleColor.Cyan);  mode = 'R'; break; }
}

CWL("\n  Press any key to start the battle...", ConsoleColor.DarkGray);
Console.ReadKey(true);
Console.Clear();
PrintHeader();

// ── Run simulation (fast — dice resolved up-front) ───────────────────────────

var result = simulator.Simulate(theron, longsword, gruk, battleAxe, 500);

if (mode == 'T')
    PlayTurnBased();
else
    PlayRealTime();

PrintSummary();

// ═════════════════════════════════════════════════════════════════════════════
// TURN-BASED MODE
// Each press of any key advances to the next combatant action.
// The screen clears and refreshes with the current HP/TM state.
// ═════════════════════════════════════════════════════════════════════════════
void PlayTurnBased()
{
    // Split the full log into "turns" — a new turn starts at every TurnStart entry.
    // Each turn slice contains all events from TurnStart through TurnEnd/Death.
    var turns = new List<List<BattleLogEntry>>();
    List<BattleLogEntry>? current = null;

    foreach (var e in result.Log)
    {
        if (e.EventType == "TurnStart")
        {
            current = new List<BattleLogEntry>();
            turns.Add(current);
        }
        // Exclude TurnMeterGain from turn slices (shown separately via ShowTm)
        if (current != null && e.EventType != "TurnMeterGain")
            current.Add(e);
    }

    for (var idx = 0; idx < turns.Count; idx++)
    {
        var turnEntries = turns[idx];
        var tick = turnEntries.FirstOrDefault()?.Tick ?? 0;

        Console.Clear();
        PrintHeader();

        // Identify who acts this turn for context
        var actorName  = turnEntries.FirstOrDefault(e => e.EventType == "TurnStart")?.ActorName ?? "?";
        var targetName = actorName == theron.Name ? gruk.Name : theron.Name;
        var actorMax   = actorName  == theron.Name ? theron.MaxHitPoints : gruk.MaxHitPoints;
        var targetMax2 = targetName == theron.Name ? theron.MaxHitPoints : gruk.MaxHitPoints;

        // Drive CharColor — active = green, waiting = dark gray
        activeActor = actorName;

        CWL($"\n  Turn {idx + 1}  |  Tick {tick}", ConsoleColor.DarkGray);
        CW("  "); CW($"{actorName.ToUpper()}", CharColor(actorName));
        CW($"  HP "); CW($"{hp[actorName]}/{actorMax}", hp[actorName] > actorMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CW("   vs   ");
        CW($"{targetName.ToUpper()}", CharColor(targetName));
        CW($"  HP "); CWL($"{hp[targetName]}/{targetMax2}", hp[targetName] > targetMax2 / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);
        Console.WriteLine();

        // HP bars at the top of each screen reflect state BEFORE this turn's damage
        ShowHp(theron.Name, hp[theron.Name], maxHp[theron.Name]);
        ShowHp(gruk.Name,   hp[gruk.Name],   maxHp[gruk.Name]);
        Console.WriteLine();

        // Turnmeter bars pulled from the full log for this tick — use IsReady/IsActive from the entry
        foreach (var tme in result.Log.Where(e => e.EventType == "TurnMeterGain" && e.Tick == tick))
            ShowTm(tme.ActorName, tme.TurnMeterAfter ?? 0, tme.IsReady, tme.IsActive);

        Console.WriteLine();

        foreach (var e in turnEntries)
        {
            switch (e.EventType)
            {
                case "TurnStart":
                {
                    var target = e.ActorName == theron.Name ? gruk.Name : theron.Name;
                    CW($"  >> ", ConsoleColor.DarkCyan);
                    CW($"{e.ActorName.ToUpper()}", CharColor(e.ActorName));
                    CW($" readies their attack on ");
                    CW(target, CharColor(target));
                    CWL("!", ConsoleColor.White);
                    Console.WriteLine();
                    break;
                }

                case "Attack":
                    PrintAttack(e);
                    break;

                case "Damage":
                    // Update HP tracker then show updated bars
                    hp[e.ActorName] = e.TargetHpAfter ?? hp[e.ActorName];
                    Console.WriteLine();
                    CW("  ");
                    CW($"{e.ActorName}", CharColor(e.ActorName));
                    CW(" takes ");
                    CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowHp(theron.Name, hp[theron.Name], maxHp[theron.Name]);
                    ShowHp(gruk.Name,   hp[gruk.Name],   maxHp[gruk.Name]);
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
            }
        }

        Console.WriteLine();
        var battleOver = turnEntries.Any(e => e.EventType == "Death");
        CWL(battleOver
            ? "  Battle over!  Press any key for results..."
            : "  Press any key for next turn...",
            ConsoleColor.DarkGray);
        Console.ReadKey(true);
    }
    activeActor = ""; // reset so summary uses neutral colours
}

// ═════════════════════════════════════════════════════════════════════════════
// REAL-TIME (TICK-BASED) MODE — fully automatic, no keypresses needed
//
// Quiet ticks (no action) scroll by quickly (50 ms each) showing meter gains.
// Action ticks pause and display the full attack resolution with delays:
//   300 ms before showing who acts
//   500 ms before showing the attack roll
//   200 ms before showing the damage result + updated HP bars
//   800 ms pause after damage to let the player read it
//   1500 ms pause after a death announcement
// ═════════════════════════════════════════════════════════════════════════════
void PlayRealTime()
{
    var byTick = result.Log
        .GroupBy(e => e.Tick)
        .OrderBy(g => g.Key)
        .ToList();

    CWL("\n  BATTLE BEGINS\n", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.DarkCyan);

    // Track HP locally for display
    var curHp = new Dictionary<string, int>
    {
        [theron.Name] = theron.MaxHitPoints,
        [gruk.Name]   = gruk.MaxHitPoints
    };

    int quietStart = -1;   // first tick of current quiet run
    int quietEnd   = -1;   // last  tick of current quiet run
    // turnmeter states captured during quiet ticks for compact summary
    var quietTmStart = new Dictionary<string, int>();
    var quietTmEnd   = new Dictionary<string, int>();

    void FlushQuiet()
    {
        if (quietStart < 0) return;
        // Only show if at least a few ticks passed
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
        var entries   = tickGroup.ToList();
        var hasAction = entries.Any(e => e.EventType == "TurnStart");

        if (!hasAction)
        {
            // Accumulate meter snapshots, suppress individual lines
            foreach (var e in entries.Where(e => e.EventType == "TurnMeterGain"))
            {
                if (quietStart < 0) { quietStart = e.Tick; quietTmStart[e.ActorName] = e.TurnMeterBefore ?? 0; }
                quietEnd = e.Tick;
                quietTmEnd[e.ActorName] = e.TurnMeterAfter ?? 0;
            }
            Thread.Sleep(40);
            continue;
        }

        // Flush any quiet ticks before this action tick
        FlushQuiet();

        // ── Identify attacker and target for this tick ────────────────────────
        var turnStart   = entries.First(e => e.EventType == "TurnStart");
        var attackEntry = entries.FirstOrDefault(e => e.EventType == "Attack");
        var attacker    = turnStart.ActorName;
        var target      = attacker == theron.Name ? gruk.Name : theron.Name;

        var attackerHp  = curHp[attacker];
        var targetHp    = curHp[target];
        var attackerMax = attacker == theron.Name ? theron.MaxHitPoints : gruk.MaxHitPoints;
        var targetMax   = target   == theron.Name ? theron.MaxHitPoints : gruk.MaxHitPoints;

        // Drive CharColor for this tick
        activeActor = attacker;

        // ── Action tick header ────────────────────────────────────────────────
        Console.WriteLine();
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);
        CW($"  Tick {tickGroup.Key,-3}  |  ", ConsoleColor.DarkGray);
        CW($"{attacker.ToUpper()}", CharColor(attacker));
        CW($"  HP ");
        CW($"{attackerHp}/{attackerMax}", attackerHp > attackerMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CW($"   vs   ");
        CW($"{target.ToUpper()}", CharColor(target));
        CW($"  HP ");
        CWL($"{targetHp}/{targetMax}", targetHp > targetMax / 2 ? ConsoleColor.Green : ConsoleColor.Red);
        CWL("  " + new string('-', 65), ConsoleColor.DarkCyan);

        Thread.Sleep(300);

        foreach (var e in entries)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    // Already shown in quiet summary — skip
                    break;

                case "TurnStart":
                    Console.WriteLine();
                    CW($"  >> ", ConsoleColor.DarkCyan);
                    CW($"{e.ActorName.ToUpper()}", CharColor(e.ActorName));
                    CW($" readies their attack on ");
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
                    hp[e.ActorName]    = curHp[e.ActorName];
                    Console.WriteLine();
                    CW("  ");
                    CW($"{e.ActorName}", CharColor(e.ActorName));
                    CW($" takes ");
                    CW($"{e.DamageDealt}", ConsoleColor.Red);
                    CWL($" damage!   HP: {e.TargetHpBefore} -> {e.TargetHpAfter}", ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ShowHp(theron.Name, curHp[theron.Name], theron.MaxHitPoints);
                    ShowHp(gruk.Name,   curHp[gruk.Name],   gruk.MaxHitPoints);
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
            }
        }
    }

    FlushQuiet();
    activeActor = ""; // reset so summary uses neutral colours
}

// ═════════════════════════════════════════════════════════════════════════════
// SUMMARY SCREEN
// ═════════════════════════════════════════════════════════════════════════════
void PrintSummary()
{
    Console.Clear();
    PrintHeader();

    var winner = result.Winner;
    var loser  = result.Loser;

    Console.WriteLine();
    CW("  BATTLE COMPLETE  --  ", ConsoleColor.Green);
    CW(winner?.Name ?? "?", CharColor(winner?.Name ?? "?"));
    CWL(" WINS!", ConsoleColor.Green);
    CWL("  " + new string('=', 62), ConsoleColor.Cyan);

    var attacks     = result.Log.Where(e => e.EventType == "Attack").ToList();
    var hits        = attacks.Count(e => e.IsHit == true);
    var misses      = attacks.Count(e => e.IsHit == false && e.IsFumble == false);
    var crits       = attacks.Count(e => e.IsCritical == true);
    var fumbles     = attacks.Count(e => e.IsFumble   == true);
    var winnerDmg   = attacks.Where(e => e.ActorName == winner?.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);
    var loserDmg    = attacks.Where(e => e.ActorName == loser?.Name  && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);

    CWL($"\n  Total actions :  {attacks.Count}", ConsoleColor.White);
    CW(  "  Results       :  "); CW($"{hits} hits", ConsoleColor.Green);
    CW($" / {misses} misses"); CW($" / {crits} crits", ConsoleColor.Magenta);
    CWL($" / {fumbles} fumbles", ConsoleColor.DarkYellow);

    CWL($"\n  Damage dealt:", ConsoleColor.White);
    CW("    "); CW($"{winner?.Name,-12}", CharColor(winner?.Name ?? "?")); CW($"  {winnerDmg,3} dmg ", ConsoleColor.Yellow); CW("dealt to "); CWL(loser?.Name ?? "?", CharColor(loser?.Name ?? "?"));
    CW("    "); CW($"{loser?.Name,-12}",  CharColor(loser?.Name  ?? "?")); CW($"  {loserDmg,3} dmg ",  ConsoleColor.Yellow); CW("dealt to "); CWL(winner?.Name ?? "?", CharColor(winner?.Name ?? "?"));

    CWL($"\n  Final state:", ConsoleColor.White);
    ShowHp(winner?.Name ?? "?", winner?.CurrentHitPoints ?? 0, maxHp.GetValueOrDefault(winner?.Name ?? "", 1));
    ShowHp(loser?.Name  ?? "?", 0,                            maxHp.GetValueOrDefault(loser?.Name  ?? "", 1));

    CWL($"\n  Battle length :  {result.TotalTicks} ticks", ConsoleColor.White);
    CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
    Console.WriteLine();
}

// ═════════════════════════════════════════════════════════════════════════════
// HELPERS
// ═════════════════════════════════════════════════════════════════════════════

void PrintHeader()
{
    CWL("  " + new string('=', 65), ConsoleColor.Cyan);
    CWL("        ***  BATTLE ARENA  --  COMBAT SIMULATION DEMO  ***", ConsoleColor.Cyan);
    CWL("  " + new string('=', 65) + "\n", ConsoleColor.Cyan);
}

void ShowSheet(string role, Character ch, Weapon wp, int ap, int dp)
{
    int strMod  = (ch.Strength  - 10) / 2;
    int dexMod  = (ch.Dexterity - 10) / 2;
    int dexCap  = Math.Min(dexMod, ch.Equipment.Chest?.MaxDexterityBonus ?? 6);
    int ac      = ch.Equipment.Chest?.ArmorClass ?? 0;
    int mit     = ch.Equipment.Chest?.Mitigation ?? 0;

    // Inner content width — every Row pads to exactly this many chars
    // so the right-side border always aligns.
    const int IW = 60;

    void Sep() =>
        CWL("  +" + new string('-', IW + 2) + "+", ConsoleColor.Cyan);

    // Plain row — content left-aligned, padded to IW
    void Row(string content, ConsoleColor col = ConsoleColor.White)
    {
        CW("  | ", ConsoleColor.Cyan);
        Console.ForegroundColor = col;
        Console.Write((" " + content).PadRight(IW));
        Console.ResetColor();
        CWL(" |", ConsoleColor.Cyan);
    }

    // Two-column row — left text, right text pushed to far-right edge
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
    Row2($"{role}: {ch.Name}", $"Level {ch.Level}", CharColor(ch.Name));
    Sep();
    Row($"HP: {ch.MaxHitPoints}   TurnSpeed: {ch.TurnSpeed}   StrikeRating: {ch.StrikeRating}");
    Row($"STR: {ch.Strength} ({Sign(strMod)}{strMod})   DEX: {ch.Dexterity} ({Sign(dexMod)}{dexMod})");
    Sep();
    Row($"Weapon  : {wp.Name,-18} {wp.DamageCount}d{DieSides(wp.DamageDie)} {wp.DamageType,-10} +{wp.AttackBonus} atk bonus");
    Row($"Armor   : {ch.Equipment.Chest?.Name ?? "None",-18} AC {ac,-2}  EffAC {20 - ac,-2}  Mitigation: {mit}");
    Sep();
    Row($"Atk Power : {ap,-4}  (20-{ch.StrikeRating}) + {ch.Level} (lvl) + ({Sign(strMod)}{strMod}) (str) + {wp.AttackBonus} (wpn)");
    Row($"Def Power : {dp,-4}  (20-{ac}) + ({Sign(dexCap)}{dexCap}) (dex)");
    Sep();
    Console.WriteLine();
}

void PrintAttack(BattleLogEntry e)
{
    var total  = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
    var margin = total - (e.DefensePower ?? 0);

    // ── Roll line ────────────────────────────────────────────────────────────
    Console.WriteLine();
    CW("  Roll  "); CW($"d20 = {e.DieRoll,2}", ConsoleColor.Yellow);
    CW("   Attack Power ");  CW($"{e.AttackPower}", ConsoleColor.Yellow);
    CW($"  =  Total "); CW($"{total,2}", ConsoleColor.White);
    CW($"   vs  Defence "); CW($"{e.DefensePower}", ConsoleColor.Yellow);
    CW("   |  margin ");
    if (margin >= 0) CWL($"+{margin}", ConsoleColor.Green);
    else             CWL($"{margin}",  ConsoleColor.Red);

    // ── Outcome banner ───────────────────────────────────────────────────────
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

    // ── Damage breakdown ─────────────────────────────────────────────────────
    if (e.IsHit == true)
    {
        var dmgIdx = e.Message.IndexOf("Dmg:", StringComparison.Ordinal);
        if (dmgIdx >= 0)
        {
            Console.WriteLine();
            CW("  Damage  "); CWL(e.Message[dmgIdx..], ConsoleColor.DarkCyan);
        }
    }

    // ── Narrative phrase ─────────────────────────────────────────────────────
    if (!string.IsNullOrEmpty(e.Phrase))
    {
        Console.WriteLine();
        CWL($"  \"{e.Phrase}\"", ConsoleColor.DarkCyan);
    }
}

void ShowHp(string name, int current, int max, int w = 24)
{
    var pct    = (double)Math.Max(0, current) / Math.Max(1, max);
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
    var filled  = (int)(Math.Min(1.0, current / 100.0) * w);
    var barCol  = isActive ? ConsoleColor.Green : isReady ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
    var nameCol = CharColor(name);

    Console.Write("  ");
    CW($"{name,-10}", nameCol);
    Console.Write("  TM [");
    Console.ForegroundColor = barCol;
    Console.Write(new string('|', filled) + new string('.', w - filled));
    Console.ResetColor();
    Console.Write($"]  {current,4}");
    if      (isActive) { Console.ForegroundColor = ConsoleColor.Green; Console.Write("  ACTING"); Console.ResetColor(); }
    else if (isReady)  { Console.ForegroundColor = ConsoleColor.Cyan;  Console.Write("  READY");  Console.ResetColor(); }
    Console.WriteLine();
}

// Active fighter = bright green (their turn), waiting fighter = dim.
// Outside battle both are white.
ConsoleColor CharColor(string name) =>
    activeActor == ""        ? ConsoleColor.White :
    name == activeActor      ? ConsoleColor.Green :
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

static int DieSides(DieType d) => d switch
{
    DieType.D4   => 4,  DieType.D6  => 6,  DieType.D8  => 8,
    DieType.D10  => 10, DieType.D12 => 12, DieType.D20 => 20,
    _            => 0
};

