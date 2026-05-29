namespace BattleArena.Demo;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

static partial class Demo
{
    // ── Display strategies: shared by turn-based and realtime ────────────

    private delegate void DisplayHandler(CombatLogEntry e, Dictionary<string, CharDisplayState> states);

    private static readonly Dictionary<string, DisplayHandler> _display = new()
    {
        ["TurnStart"] = (e, states) =>
        {
            var tgtSt = states.GetValueOrDefault(e.TargetName ?? "");
            var actSt = states.GetValueOrDefault(e.ActorName);
            var verb = e.IsSpell == true ? "conjures" : "readies";
            Console.WriteLine();
            CWL("  " + new string('·', 77), ConsoleColor.Gray);
            CW("  ▶ ", ConsoleColor.White);
            CW(e.ActorName.ToUpper(), actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            CW($"  {verb}  ", ConsoleColor.Gray);
            CW($"[{e.AttackSourceName}]", e.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
            CW("  →  ", ConsoleColor.Gray);
            CWL(e.TargetName ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
        },
        ["Attack"] = (e, _) => PrintAttack(e),
        ["Damage"] = (e, _) =>
        {
            var mhp = MaxHp!.TryGetValue(e.ActorName, out var m) ? m : 1;
            CW($"     {e.ActorName}", ConsoleColor.White);
            CW("  takes  ");
            CW($"{e.DamageDealt}", ConsoleColor.Red);
            CW("  damage   ");
            CW("[", ConsoleColor.Gray);
            CW($"{e.TargetHpBefore}", ConsoleColor.Gray);
            CW(" → ", ConsoleColor.Gray);
            CW($"{Math.Max(0, e.TargetHpAfter ?? 0)}", HpColorInline(e.TargetHpAfter ?? 0, mhp));
            CW("/", ConsoleColor.Gray);
            CW($"{mhp}", ConsoleColor.Gray);
            CWL(" HP]", ConsoleColor.Gray);
        },
        ["FumblePenalty"] = (e, _) =>
        {
            CW("  ⚠ ", ConsoleColor.DarkYellow);
            CWL(e.Message, ConsoleColor.DarkYellow);
        },
        ["DoTTick"] = (e, _) =>
        {
            CW("  ↓ ", ConsoleColor.DarkYellow);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CW("  suffers  ");
            CW($"{e.DamageDealt}", ConsoleColor.Red);
            CW($"  {e.StatusEffectName ?? "DoT"} damage", ConsoleColor.DarkYellow);
            Console.WriteLine();
        },
        ["EffectApplied"] = (e, _) =>
        {
            CW("  ★ ", ConsoleColor.DarkYellow);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CWL($"  is afflicted with  {e.StatusEffectName}!", ConsoleColor.DarkYellow);
        },
        ["EffectResisted"] = (e, _) =>
        {
            CW("  ✓ ", ConsoleColor.Green);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CW($"  resists  ");
            CW(e.StatusEffectName ?? "the effect", ConsoleColor.Green);
            CWL($"   (rolled {e.ResistRoll} vs {e.ResistThreshold})", ConsoleColor.Gray);
        },
        ["EffectExpired"] = (e, _) =>
        {
            CW("  ○ ", ConsoleColor.Gray);
            CW(e.StatusEffectName ?? "", ConsoleColor.Green);
            CW("  has worn off  ");
            CWL(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
        },
        ["SkippedTurn"] = (e, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('·', 77), ConsoleColor.Gray);
            CW("  ⊘ ", ConsoleColor.DarkYellow);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CW("  ");
            CWL(e.Message.Split("is ")[^1], ConsoleColor.DarkYellow);
        },
        ["TurnEnd"] = (_, _) => { },
        ["Death"] = (e, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('*', 65), ConsoleColor.Red);
            CWL($"  ✝  {e.Message}", ConsoleColor.Red);
            CWL("  " + new string('*', 65), ConsoleColor.Red);
        },
        ["KnockedOut"] = (e, _) =>
        {
            Console.WriteLine();
            CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
            CWL($"  ⊘  {e.Message}", ConsoleColor.DarkYellow);
            CWL("  " + new string('~', 65), ConsoleColor.DarkYellow);
        },
        ["ApiCall"] = (e, _) =>
        {
            CW("     ⚡ ", ConsoleColor.DarkCyan);
            CWL(e.Message, ConsoleColor.DarkCyan);
        },
        ["ManaRegen"] = (e, _) =>
        {
            CW("  ♪ ", ConsoleColor.Magenta);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CW($"  regen  ");
            CWL($" +{e.ManaRegen} mana", ConsoleColor.Magenta);
        },
        ["ManaDeduct"] = (e, _) =>
        {
            CW("  ◆ ", ConsoleColor.Magenta);
            CW(e.ActorName, CharColor(e.ActorName, e.ActiveActorName));
            CW($"  casts  ");
            CW(e.AttackSourceName ?? "unknown", ConsoleColor.Magenta);
            CWL($"  (-{e.ManaCost} mana)", ConsoleColor.DarkMagenta);
        },
    };

    // ── Realtime state updates (per event type) ─────────────────────────

    private static readonly Dictionary<string, Action<CombatLogEntry, Dictionary<string, CharDisplayState>>> _realtimeUpdate = new()
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
                st.Tm = e.TurnMeterAfter ?? st.Tm;
        },
        ["Death"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
                st.IsAlive = false;
        },
        ["KnockedOut"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st))
                st.IsAlive = false;
        },
        ["ManaDeduct"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st) && e.ManaAfter.HasValue)
                st.Mana = e.ManaAfter.Value;
        },
        ["ManaRegen"] = (e, states) =>
        {
            if (states.TryGetValue(e.ActorName, out var st) && e.ManaAfter.HasValue)
                st.Mana = e.ManaAfter.Value;
        },
    };

    // ── Realtime display delays (ms after each event) ──────────────────

    private static readonly Dictionary<string, int> _realtimeDelay = new()
    {
        ["TurnStart"]     = 900,   // "readies weapon" — give player time to read who acts next
        ["Attack"]        = 900,   // roll result + hit/miss verdict
        ["Damage"]        = 1100,  // HP bar update + damage number
        ["TurnEnd"]       = 300,
        ["DoTTick"]       = 700,
        ["EffectApplied"] = 700,
        ["EffectResisted"]= 700,
        ["EffectExpired"] = 500,
        ["SkippedTurn"]   = 900,
        ["FumblePenalty"] = 600,
        ["Death"]         = 1800,
        ["KnockedOut"]    = 1800,
    };

    // ── PlayTurnBased ───────────────────────────────────────────────────

    private static void PlayTurnBased()
    {
        var states = BuildDisplayStates();
        var turnEvents = new List<CombatLogEntry>();
        var pendingMessages = new List<CombatLogEntry>();
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

            var activeActorName = ts.ActiveActorName;
            DrawCombatScreen(states, turnTick, activeActorName);

            Console.WriteLine();
            CW($"  Turn {turnCount}  ", ConsoleColor.Gray);
            CW("|  ", ConsoleColor.Gray);
            CW(ts.ActorName.ToUpper(), actSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            CW("  →  ", ConsoleColor.Gray);
            CWL(ts.TargetName?.ToUpper() ?? "?", tgtSt?.IsHero == true ? ConsoleColor.Cyan : ConsoleColor.Red);
            Console.WriteLine();

            foreach (var e in turnEvents)
                if (_display.TryGetValue(e.EventType, out var handler))
                    handler(e, states);

            Console.WriteLine();
            CWL("  " + new string('-', 77), ConsoleColor.Gray);
            var over = turnEvents.Any(e => e.EventType is "Death" or "KnockedOut");
            CWL(over ? "  Combat over!  Press any key for results..."
                     : "  Press any key for next turn...", ConsoleColor.Gray);
            Console.ReadKey(true);

            turnEvents.Clear();
            inTurn = false;
        }

        PreSeedTurnMeters(states);

        DrawCombatScreen(states, 0);
        Console.WriteLine();
        CWL("  Press any key for first action...", ConsoleColor.Gray);
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
                    if (states.TryGetValue(e.ActorName, out var actSt2))
                        actSt2.Weapon = e.AttackSourceName ?? "";
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
                        defSt.IsAlive = false;
                    if (inTurn) turnEvents.Add(e);
                    break;

                case "TurnEnd":
                    if (states.TryGetValue(e.ActorName, out var endSt))
                        endSt.Tm = e.TurnMeterAfter ?? endSt.Tm;
                    break;

                case "DoTTick":
                    if (states.TryGetValue(e.ActorName, out var dotSt))
                        dotSt.Hp = Math.Max(dotSt.Hp - (e.DamageDealt ?? 0), -10);
                    if (inTurn) turnEvents.Add(e);
                    break;

                case "EffectApplied":
                case "EffectResisted":
                    if (inTurn) turnEvents.Add(e);
                    break;

                case "ApiCall":
                    turnEvents.Add(e);
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
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"\n  ... {quietEnd - quietStart + 1} quiet ticks (TM building)");
                Console.ResetColor();
                Thread.Sleep(300);
            }
            quietStart = quietEnd = -1;
        }

        PreSeedTurnMeters(states);

        DrawCombatScreen(states, 0);
        Thread.Sleep(1200);

        foreach (var tickGroup in byTick.Where(g => g.Key >= 1))
        {
            var entries = tickGroup.ToList();
            var hasAction = entries.Any(e => e.EventType == "TurnStart");

            foreach (var e in entries.Where(e => e.EventType == "TurnMeterGain"))
                if (states.TryGetValue(e.ActorName, out var st)) st.Tm = e.TurnMeterAfter ?? 0;

            if (!hasAction)
            {
                if (quietStart < 0) quietStart = tickGroup.Key;
                quietEnd = tickGroup.Key;
                // Animate TM bars every 3 quiet ticks so the viewer can watch them fill
                if ((tickGroup.Key - quietStart) % 3 == 0)
                    DrawCombatScreen(states, tickGroup.Key);
                Thread.Sleep(80);
                continue;
            }

            FlushQuiet();

            var turnStart = entries.First(e => e.EventType == "TurnStart");
            var attacker = turnStart.ActorName;

            if (states.TryGetValue(attacker, out var actSt))
                actSt.Weapon = turnStart.AttackSourceName ?? actSt.Weapon;

            var activeActorName = turnStart.ActiveActorName;
            DrawCombatScreen(states, tickGroup.Key, activeActorName);
            Thread.Sleep(600);

            foreach (var e in entries)
            {
                if (_realtimeUpdate.TryGetValue(e.EventType, out var update))
                    update(e, states);

                if (_display.TryGetValue(e.EventType, out var display))
                    display(e, states);

                if (_realtimeDelay.TryGetValue(e.EventType, out var delay))
                    Thread.Sleep(delay);
            }

            // Events stay visible — the next tick's DrawCombatScreen will
            // refresh the HP/TM bars when a new action begins.
            Thread.Sleep(500);
        }

        FlushQuiet();
    }

    // ── PreSeedTurnMeters ───────────────────────────────────────────────
    // Apply all TurnMeterGain events that occur before the first TurnStart
    // so the opening screen shows each character's true accumulated TM
    // rather than every bar starting at zero.
    private static void PreSeedTurnMeters(Dictionary<string, CharDisplayState> states)
    {
        foreach (var e in Result.Log)
        {
            if (e.EventType == "TurnStart") break;
            if (e.EventType == "TurnMeterGain" && states.TryGetValue(e.ActorName, out var st))
                st.Tm = e.TurnMeterAfter ?? 0;
        }
    }
}
