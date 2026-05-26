namespace BattleArena.Demo;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

static partial class Demo
{
    // ── Display strategies: shared by turn-based and realtime ────────────

    private delegate void DisplayHandler(BattleLogEntry e, Dictionary<string, CharDisplayState> states);

    private static readonly Dictionary<string, DisplayHandler> _display = new()
    {
        ["TurnStart"] = (e, states) =>
        {
            var tgtSt = states.GetValueOrDefault(e.TargetName ?? "");
            var actSt = states.GetValueOrDefault(e.ActorName);
            var verb = e.IsSpell == true ? "conjures" : "readies";
            Console.WriteLine();
            CW("  >> ", ConsoleColor.DarkCyan);
            CW(e.ActorName, actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            CW($" {verb} ");
            CW($"[{e.AttackSourceName}]", e.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
            CW(" targeting ");
            CW(e.TargetName ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            CWL("!", ConsoleColor.White);
        },
        ["Attack"] = (e, _) => PrintAttack(e),
        ["Damage"] = (e, _) =>
        {
            Console.WriteLine();
            CW("  "); CW(e.ActorName, ConsoleColor.White);
            CW(" takes "); CW($"{e.DamageDealt}", ConsoleColor.Red);
            CWL($" damage   HP: {e.TargetHpBefore} -> {Math.Max(0, e.TargetHpAfter ?? 0)}", ConsoleColor.DarkGray);
        },
        ["FumblePenalty"] = (e, _) => CWL($"  {e.Message}", ConsoleColor.DarkYellow),
        ["DoTTick"] = (e, _) =>
        {
            Console.WriteLine();
            CW("  ", ConsoleColor.DarkGray);
            CW(e.ActorName, CharColor(e.ActorName));
            CW($" suffers "); CW($"{e.DamageDealt}", ConsoleColor.Red);
            CWL($" {e.StatusEffectName ?? "DoT"} damage", ConsoleColor.DarkYellow);
        },
        ["EffectApplied"] = (e, _) =>
        {
            Console.WriteLine();
            CW("  ", ConsoleColor.DarkGray);
            CW(e.ActorName, CharColor(e.ActorName));
            CWL($" is afflicted with {e.StatusEffectName}!", ConsoleColor.DarkYellow);
        },
        ["EffectExpired"] = (e, _) =>
        {
            Console.WriteLine();
            CW("  ", ConsoleColor.DarkGray);
            CW(e.StatusEffectName ?? "", ConsoleColor.Green);
            CW(" has worn off ");
            CWL(e.ActorName, CharColor(e.ActorName));
        },
        ["SkippedTurn"] = (e, _) =>
        {
            Console.WriteLine();
            CW("  ", ConsoleColor.DarkGray);
            CW(e.ActorName, CharColor(e.ActorName));
            CWL($" is {e.Message.Split("is ")[^1]}", ConsoleColor.DarkYellow);
        },
        ["TurnEnd"] = (_, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('-', 77), ConsoleColor.DarkGray);
        },
        ["Death"] = (e, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('*', 65), ConsoleColor.Red);
            CWL($"  *** {e.Message} ***", ConsoleColor.Red);
            CWL("  " + new string('*', 65), ConsoleColor.Red);
        },
        ["KnockedOut"] = (e, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
            CWL($"  ~~~ {e.Message} ~~~", ConsoleColor.DarkYellow);
            CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
        },
    };

    // ── Realtime state updates (per event type) ─────────────────────────

    private static readonly Dictionary<string, Action<BattleLogEntry, Dictionary<string, CharDisplayState>>> _realtimeUpdate = new()
    {
        ["Damage"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
                st.Hp = e.TargetHpAfter ?? st.Hp;
        },
        ["DoTTick"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
                st.Hp = Math.Max(st.Hp - (e.DamageDealt ?? 0), -10);
        },
        ["TurnEnd"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
            { st.IsActive = false; st.Tm = e.TurnMeterAfter ?? st.Tm; }
        },
        ["Death"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
            { st.IsAlive = false; st.IsActive = false; }
        },
        ["KnockedOut"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
            { st.IsAlive = false; st.IsActive = false; }
        },
    };

    // ── Realtime display delays (ms after each event) ──────────────────

    private static readonly Dictionary<string, int> _realtimeDelay = new()
    {
        ["Attack"] = 500,
        ["Damage"] = 800,
        ["TurnStart"] = 200,
        ["TurnEnd"] = 300,
        ["DoTTick"] = 400,
        ["EffectApplied"] = 300,
        ["EffectExpired"] = 300,
        ["SkippedTurn"] = 500,
        ["Death"] = 1500,
        ["KnockedOut"] = 1500,
    };

    // ── PlayTurnBased ───────────────────────────────────────────────────

    private static void PlayTurnBased()
    {
        var states = BuildDisplayStates();
        var turnEvents = new List<BattleLogEntry>();
        var pendingMessages = new List<BattleLogEntry>();
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
                if (_display.TryGetValue(e.EventType, out var handler))
                    handler(e, states);

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

        DrawBattleScreen(states, 0);
        Console.WriteLine();
        CWL("  Press any key for first action...", ConsoleColor.DarkGray);
        Console.ReadKey(true);

        foreach (var e in Result.Log)
        {
            switch (e.EventType)
            {
                case "TurnMeterGain":
                    if (states.TryGetValue(e.ActorName, out var tmSt))
                        tmSt.Tm = e.TurnMeterAfter ?? 0;
                    break;

                case "TurnStart":
                    if (pendingMessages.Count > 0)
                    {
                        turnEvents.InsertRange(0, pendingMessages);
                        pendingMessages.Clear();
                    }
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

                case "DoTTick":
                    if (states.TryGetValue(e.ActorName, out var dotSt))
                        dotSt.Hp = Math.Max(dotSt.Hp - (e.DamageDealt ?? 0), -10);
                    if (inTurn) turnEvents.Add(e);
                    break;

                case "EffectApplied":
                    if (inTurn) turnEvents.Add(e);
                    break;

                case "SkippedTurn":
                    pendingMessages.Add(e);
                    break;

                default:
                    if (inTurn) turnEvents.Add(e);
                    break;
            }
        }

        if (pendingMessages.Count > 0)
        {
            turnEvents.InsertRange(0, pendingMessages);
            pendingMessages.Clear();
        }
        FlushTurn();
        ActiveActor = "";
    }

    // ── PlayRealTime ────────────────────────────────────────────────────

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

        DrawBattleScreen(states, 0);
        Thread.Sleep(1200);

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
                if (_realtimeUpdate.TryGetValue(e.EventType, out var update))
                    update(e, states);

                if (_display.TryGetValue(e.EventType, out var display))
                    display(e, states);

                if (_realtimeDelay.TryGetValue(e.EventType, out var delay))
                    Thread.Sleep(delay);
            }
        }

        FlushQuiet();
        ActiveActor = "";
    }
}
