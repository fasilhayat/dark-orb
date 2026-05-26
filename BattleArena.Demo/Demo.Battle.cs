namespace BattleArena.Demo;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

static partial class Demo
{
    // ── PlayTurnBased ─────────────────────────────────────────────────────────────

    private static void PlayTurnBased()
    {
        var states = BuildDisplayStates();
        var turnEvents = new List<BattleLogEntry>();
        bool inTurn = false;
        int turnCount = 0;
        int turnTick = 0;
        string actorName = "";

        void FlushTurn()
        {
            if (!inTurn || turnEvents.Count == 0) return;

            var ts = turnEvents.First(e => e.EventType == "TurnStart");
            var actSt = states.GetValueOrDefault(ts.ActorName);
            var tgtSt = states.GetValueOrDefault(ts.TargetName ?? "");

            DrawBattleScreen(states, turnTick);

            Console.WriteLine();
            CW($"  Turn {turnCount}  ", ConsoleColor.DarkGray);
            CW("|  ", ConsoleColor.DarkGray);
            CW(ts.ActorName.ToUpper(), actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            CW("  -?  ", ConsoleColor.DarkGray);
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
            CWL("  " + new string('-', 77), ConsoleColor.DarkGray);
            var over = turnEvents.Any(e => e.EventType is "Death" or "KnockedOut");
            CWL(over ? "  Battle over!  Press any key for results..."
                     : "  Press any key for next turn...", ConsoleColor.DarkGray);
            Console.ReadKey(true);

            if (states.TryGetValue(actorName, out var actorDisp)) actorDisp.IsActive = false;
            turnEvents.Clear();
            inTurn = false;
        }

        foreach (var e in Result.Log)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    if (states.TryGetValue(e.ActorName, out var tmSt))
                        tmSt.Tm = e.TurnMeterAfter ?? 0;
                    break;

                case "TurnStart":
                    FlushTurn();
                    inTurn = true;
                    turnCount++;
                    turnTick = e.Tick;
                    actorName = e.ActorName;
                    ActiveActor = e.ActorName;
                    if (states.TryGetValue(e.ActorName, out var actSt2))
                    {
                        actSt2.IsActive = true;
                        actSt2.Weapon = e.AttackSourceName ?? "";
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

                case "TurnEnd":
                    if (states.TryGetValue(e.ActorName, out var endSt))
                    { endSt.IsActive = false; endSt.Tm = e.TurnMeterAfter ?? endSt.Tm; }
                    break;

                default:
                    if (inTurn) turnEvents.Add(e);
                    break;
            }
        }

        FlushTurn();
        ActiveActor = "";
    }

    // ── PlayRealTime ──────────────────────────────────────────────────────────────

    private static void PlayRealTime()
    {
        var states = BuildDisplayStates();
        var byTick = Result.Log.GroupBy(e => e.Tick).OrderBy(g => g.Key).ToList();

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
            var entries = tickGroup.ToList();
            var hasAction = entries.Any(e => e.EventType == "TurnStart");

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
            var attacker = turnStart.ActorName;
            ActiveActor = attacker;

            foreach (var st in states.Values) st.IsActive = false;
            if (states.TryGetValue(attacker, out var actSt))
            {
                actSt.IsActive = true;
                actSt.Weapon = turnStart.AttackSourceName ?? actSt.Weapon;
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
                        var tgtSt = states.GetValueOrDefault(e.TargetName ?? "");
                        var verb = e.IsSpell == true ? "conjures" : "readies";
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
                        CWL("  " + new string('-', 77), ConsoleColor.DarkGray);
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
        ActiveActor = "";
    }
}
