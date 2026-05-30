namespace BattleArena.Demo;

using Application.Models;
using Application.Services;
using BattleArena.Presentation;
using Core.Entities;
using Core.Entities.Enums;

// Console rendering for combat screens, stat sheets, attack logs and summaries.
static partial class Demo
{
    // ── CW / CWL ─────────────────────────────────────────────────────────────────

    internal static void CW(string text, ConsoleColor col = ConsoleColor.White)
    {
        Console.ForegroundColor = col;
        Console.Write(text);
        Console.ResetColor();
    }

    internal static void CWL(string text, ConsoleColor col = ConsoleColor.White)
    {
        Console.ForegroundColor = col;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    // ── PrintHeader ───────────────────────────────────────────────────────────────

    internal static void PrintHeader()
    {
        CWL("  " + new string('=', 65), ConsoleColor.Cyan);
        CWL("        ***  BATTLE ARENA  --  COMBAT SIMULATION DEMO  ***", ConsoleColor.Cyan);
        CWL("  " + new string('=', 65) + "\n", ConsoleColor.Cyan);
    }

    // ── GetClassName ──────────────────────────────────────────────────────────────

    internal static string GetClassName(int classId) => classId switch
    {
        1 => "Barbarian",
        2 => "Knight",
        3 => "Paladin",
        4 => "Priest",
        5 => "Mage",
        6 => "Bard",
        7 => "Druid",
        8 => "Fighter",
        9 => "Rogue",
        _ => $"Class {classId}"
    };

    // ── ShowSheet ─────────────────────────────────────────────────────────────────

    internal static void ShowSheet(string role, Character ch, IAttackSource? attackSource, int ap, int dp)
    {
        var displaySource = attackSource ?? GetSheetAttackSource(ch, attackSource);
        var abilityScore = displaySource.UsesIntelligence ? ch.Intelligence
                      : displaySource.AttackType == AttackType.Ranged ? ch.Dexterity
                      : ch.Strength;
        var abilityMod = (abilityScore - 10) / 2;
        var dexMod = (ch.Dexterity - 10) / 2;
        var dexCap = Math.Min(dexMod, ch.Equipment.Chest?.MaxDexterityBonus ?? 6);
        var ac = ch.Equipment.Chest?.ArmorClass ?? 0;
        var mit = ch.Equipment.Chest?.Mitigation ?? 0;

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
        Row2($"{role}: {ch.Name}", $"Level {ch.Level} {GetClassName(ch.ClassId)}", ConsoleColor.White);
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
                     : displaySource.AttackType == AttackType.Ranged ? "dex" : "str";
        Row($"Atk Power : {ap,-4}  (20-{ch.StrikeRating}) + {ch.Level} (lvl) + ({Sign(abilityMod)}{abilityMod}) ({abilityLabel}) + {displaySource.AttackBonus} (src)");
        Row($"Def Power : {dp,-4}  (20-{ac}) + ({Sign(dexCap)}{dexCap}) (dex)");
        Sep();
        Console.WriteLine();
    }

    // ── PrintAttack ───────────────────────────────────────────────────────────────

    internal static void PrintAttack(CombatLogEntry e)
    {
        var cfg = DisplayConfig;
        var total  = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
        var margin = total - (e.DefensePower ?? 0);
        var src    = e.AttackSourceName ?? "Unknown";
        var srcCol = e.IsSpell ? ConsoleColor.Magenta : ConsoleColor.Yellow;

        Console.WriteLine();
        CW("  ", ConsoleColor.White);
        CW(e.ActorName, CharColor(e.ActorName));
        CW(e.IsSpell ? "  casts  " : "  attacks with  ");
        CWL($"[{src}]", srcCol);

        if (cfg.IsFieldEnabled("attackEvent", "DieRoll") ||
            cfg.IsFieldEnabled("attackEvent", "AttackPower") ||
            cfg.IsFieldEnabled("attackEvent", "DefensePower"))
        {
            if (ApiClient is not null)
                CW("     ⚡ d20=", ConsoleColor.DarkCyan);
            else
                CW("     d20=", ConsoleColor.Gray);

            if (cfg.IsFieldEnabled("attackEvent", "DieRoll"))
                CW($"{e.DieRoll,2}", ConsoleColor.Cyan);

            if (cfg.IsFieldEnabled("attackEvent", "AttackPower"))
            {
                CW("  ATK ");
                CW($"{e.AttackPower}", ConsoleColor.Yellow);
            }

            CW("  →  total ");
            CW($"{total,2}", ConsoleColor.White);

            if (cfg.IsFieldEnabled("attackEvent", "DefensePower"))
            {
                CW("   vs  DEF ");
                CW($"{e.DefensePower}", ConsoleColor.Yellow);
            }

            CW("   ┃  margin ");
            if (margin >= 0) CWL($"+{margin}", ConsoleColor.Green);
            else             CWL($"{margin}", ConsoleColor.Red);
        }

        if (cfg.IsFieldEnabled("attackEvent", "IsHit"))
        {
            Console.Write("     ");
            if (cfg.IsFieldEnabled("attackEvent", "IsCritical") && e.IsCritical == true)
            {
                CW("⚡ CRITICAL HIT", ConsoleColor.Magenta);
            }
            else if (cfg.IsFieldEnabled("attackEvent", "IsFumble") && e.IsFumble == true)
            {
                CW("⚠ FUMBLE", ConsoleColor.DarkYellow);
            }
            else if (e.IsHit == true)
            {
                var damage = e.DamageDealt ?? 0;
                var defMaxHp = MaxHp.GetValueOrDefault(e.TargetName ?? "", Math.Max(1, damage));
                var rawLabel = CombatHitLabelService.GetLabel(damage, defMaxHp);

                (string label, ConsoleColor color) hit = rawLabel switch
                {
                    "CRUSHING HIT" => ("■ CRUSHING HIT", ConsoleColor.Magenta),
                    "HEAVY HIT" => ("■ HEAVY HIT", ConsoleColor.Yellow),
                    "SOLID HIT" => ("■ SOLID HIT", ConsoleColor.Green),
                    "GLANCING HIT" => ("▪ GLANCING HIT", ConsoleColor.White),
                    _ => ("▫ GRAZE", ConsoleColor.Gray),
                };
                CW(hit.label, hit.color);

                if (cfg.IsFieldEnabled("attackEvent", "DamageDealt"))
                {
                    var dmgIdx = e.Message.IndexOf("Dmg:", StringComparison.Ordinal);
                    if (dmgIdx >= 0)
                    {
                        CW("   │   ", ConsoleColor.Gray);
                        CW(e.Message[dmgIdx..], ConsoleColor.DarkCyan);
                    }
                }
            }
            else
            {
                var label = margin >= -3 ? "○ NEAR MISS" : "○ MISS";
                CW(label, ConsoleColor.Red);
            }
            Console.WriteLine();
        }

        if (!string.IsNullOrEmpty(e.Phrase))
            CWL($"     \"{e.Phrase}\"", ConsoleColor.DarkCyan);
    }

    // ── ShowHp ────────────────────────────────────────────────────────────────────

    internal static void ShowHp(string name, int current, int max, int w = 24)
    {
        var pct = (double)Math.Max(0, current) / Math.Max(1, max);
        var filled = current > 0 ? Math.Max(1, (int)(pct * w)) : 0;
        var barCol = HpColor(current, max);

        Console.Write("  ");
        CW($"{name,-10}", CharColor(name));
        Console.Write("  HP [");
        Console.ForegroundColor = barCol;
        Console.Write(new string('\u2588', filled));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(new string('\u2591', w - filled));
        Console.ResetColor();
        var hpDisplay = current < 0 ? current.ToString() : Math.Max(0, current).ToString();
        Console.Write("]  ");
        if (current < 0) CW($"{current,3}", ConsoleColor.Red);
        else CW($"{Math.Max(0, current),3}", barCol);
        CWL($" / {max,3}", ConsoleColor.Gray);
    }

    // ── HpColor ───────────────────────────────────────────────────────────────────

    internal static ConsoleColor HpColor(int current, int max)
    {
        if (current <= 0) return ConsoleColor.Red;
        var pct = (double)current / Math.Max(1, max);
        return pct > 0.5 ? ConsoleColor.Green
             : pct > 0.25 ? ConsoleColor.Yellow
             : ConsoleColor.Red;
    }

    internal static ConsoleColor HpColorInline(int current, int max) => HpColor(current, max);

    // ── CharColor ─────────────────────────────────────────────────────────────────

    internal static ConsoleColor CharColor(string name, string? activeActorName = null) =>
        activeActorName is null or "" ? ConsoleColor.White :
        name == activeActorName ? ConsoleColor.Green :
        ConsoleColor.Gray;

    // ── PrintSummary ──────────────────────────────────────────────────────────────

    internal static void PrintSummary()
    {
        Console.Clear();
        PrintHeader();

        if (Result.MaxTicksReached)
        {
            Console.WriteLine();
            CWL("  COMBAT TIMEOUT — no winner declared.", ConsoleColor.DarkYellow);
            CWL($"  Total ticks: {Result.TotalTicks}", ConsoleColor.White);
            CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
            return;
        }

        var wParty = Result.WinningParty!;
        var lParty = Result.LosingParty!;

        Console.WriteLine();
        if (DisplayConfig.IsFieldEnabled("combatSummary", "WinnerName"))
        {
            CW("  COMBAT COMPLETE  --  ", ConsoleColor.Green);
            CW(wParty.Name, ConsoleColor.Green);
            CWL("  WINS!", ConsoleColor.Green);
        }
        CWL("  " + new string('=', 62), ConsoleColor.Cyan);

        var attacks = Result.Log.Where(e => e.EventType == "Attack").ToList();
        var hits = attacks.Count(e => e.IsHit == true);
        var misses = attacks.Count(e => e.IsHit == false && e.IsFumble == false);
        var crits = attacks.Count(e => e.IsCritical == true);
        var fumbles = attacks.Count(e => e.IsFumble == true);

        CWL($"\n  Total actions :  {attacks.Count}", ConsoleColor.White);
        CW("  Results       :  "); CW($"{hits} hits", ConsoleColor.Green);
        CW($" / {misses} misses"); CW($" / {crits} crits", ConsoleColor.Magenta);
        CWL($" / {fumbles} fumbles", ConsoleColor.DarkYellow);

        CWL("\n  Damage dealt:", ConsoleColor.White);
        foreach (var m in wParty.Members.Concat(lParty.Members))
        {
            var dmg = attacks.Where(e => e.ActorName == m.Character.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);
            var isWinner = wParty.Members.Any(wm => wm.Character.Name == m.Character.Name);
            CW("    "); CW($"{m.Character.Name,-12}", isWinner ? ConsoleColor.Green : ConsoleColor.Gray);
            CW($"  {dmg,3} dmg", ConsoleColor.Yellow);
            CWL(isWinner ? "  [winner side]" : "  [loser side]", isWinner ? ConsoleColor.Green : ConsoleColor.Gray);
        }

        CWL("\n  Final HP:", ConsoleColor.White);
        CWL("  ── Winners ──────────────────────────────────────────────", ConsoleColor.Green);
        foreach (var m in wParty.Members)
            ShowHp(m.Character.Name, m.Character.CurrentHitPoints, MaxHp.GetValueOrDefault(m.Character.Name, 1));
        CWL("  ── Losers ───────────────────────────────────────────────", ConsoleColor.Red);
        foreach (var m in lParty.Members)
            ShowHp(m.Character.Name, m.Character.CurrentHitPoints, MaxHp.GetValueOrDefault(m.Character.Name, 1));

        if (DisplayConfig.IsFieldEnabled("combatSummary", "LoserStatus") ||
            DisplayConfig.IsFieldEnabled("combatSummary", "LoserName"))
        {
            var loserName = DisplayConfig.IsFieldEnabled("combatSummary", "LoserName") ? lParty.Name : "The losing party";
            var loserTag = Result.LoserStatus == CharacterVitalStatus.Dead ? "SLAIN" : "unconscious";
            CWL($"\n  {loserName} is {loserTag}!",
                Result.LoserStatus == CharacterVitalStatus.Dead
                    ? ConsoleColor.Red : ConsoleColor.DarkYellow);
        }

        if (DisplayConfig.IsFieldEnabled("combatSummary", "CombatId"))
            CWL($"  Combat ID     :  {Result.CombatId}", ConsoleColor.Gray);
        if (DisplayConfig.IsFieldEnabled("combatSummary", "TotalTicks"))
            CWL($"\n  Combat length :  {Result.TotalTicks} ticks", ConsoleColor.White);
        CWL("\n  " + new string('=', 62), ConsoleColor.Cyan);
        Console.WriteLine();
    }

    // ── DrawRoundBar ──────────────────────────────────────────────────────────────

    private static void DrawRoundBar(int tick)
    {
        const int RoundLength = 10;
        const int BarWidth    = 55;

        int roundNumber  = (tick - 1) / RoundLength + 1;
        int tickInRound  = (tick - 1) % RoundLength + 1;
        int filled       = (int)Math.Round((double)tickInRound / RoundLength * BarWidth);

        Console.Write("  ");
        CW($"ROUND {roundNumber,-2}  ", ConsoleColor.Yellow);
        CW("[", ConsoleColor.DarkYellow);
        CW(new string('\u2588', filled), ConsoleColor.DarkYellow);
        CW(new string('\u2591', BarWidth - filled), ConsoleColor.DarkGray);
        CW("]", ConsoleColor.DarkYellow);
        CW($"  {tickInRound}", ConsoleColor.White);
        CW(" / ", ConsoleColor.DarkGray);
        CWL($"{RoundLength} ticks", ConsoleColor.Gray);

        // Pacing indicator
        var pacingLabel = PacingMultiplier switch
        {
            <= 0.6 => "Fast (2.0x)",
            >= 1.8 => "Slow (0.5x)",
            _      => "Normal (1.0x)"
        };
        CWL($"  Pacing: {pacingLabel}", ConsoleColor.Gray);
    }

    // ── DrawCombatScreen ──────────────────────────────────────────────────────────

    internal static void DrawCombatScreen(CombatDisplayState state, int tick, string? activeActorName = null)
    {
        Console.Clear();
        PrintHeader();
        DrawRoundBar(tick);

        var heroNames = state.Layout.HeroNames.ToHashSet(StringComparer.Ordinal);
        var enemyNames = state.Layout.EnemyNames.ToHashSet(StringComparer.Ordinal);
        var heroes = state.Layout.HeroNames
            .Select(name => state.All[name])
            .Concat(state.All.Values.Where(s => s.IsHero && !heroNames.Contains(s.Name)).OrderBy(s => s.Name))
            .ToList();
        var enemies = state.Layout.EnemyNames
            .Select(name => state.All[name])
            .Concat(state.All.Values.Where(s => !s.IsHero && !enemyNames.Contains(s.Name)).OrderBy(s => s.Name))
            .ToList();

        bool isDuel = state.Layout.IsDuel;
        var leftLabel = isDuel ? "── CHARACTER 1 ──" : "── HEROES ──────";
        var rightLabel = isDuel ? "── CHARACTER 2 ──" : "── ENEMIES ──────";

        Console.WriteLine();
        Console.Write("  ");
        CW($"Tick {tick,-4}  ", ConsoleColor.Gray);
        CW(leftLabel, isDuel ? ConsoleColor.White : ConsoleColor.Blue);
        CW("─────────── vs ───────────", ConsoleColor.Gray);
        CWL(rightLabel, isDuel ? ConsoleColor.White : ConsoleColor.DarkMagenta);
        Console.WriteLine();

        var empty = BuildEmptyBlock();
        int maxCount = Math.Max(heroes.Count, enemies.Count);

        for (var i = 0; i < maxCount; i++)
        {
            var left = i < heroes.Count ? BuildCharBlock(heroes[i], activeActorName) : empty;
            var right = i < enemies.Count ? BuildCharBlock(enemies[i], activeActorName) : empty;
            PrintBlockPair(left, right);
            if (i < maxCount - 1) Console.WriteLine();
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("  " + new string('─', 77));
        Console.ResetColor();
        Console.WriteLine();
    }

    // ── BuildEmptyBlock / BuildCharBlock ─────────────────────────────────────────

    private static List<List<Seg>> BuildEmptyBlock()
    {
        var blank = new List<Seg> { new Seg(new string(' ', BLOCK_W), ConsoleColor.Black) };
        return [blank, blank, blank, blank, blank, blank];
    }

    private static List<List<Seg>> BuildCharBlock(CharDisplayState s, string? activeActorName = null)
    {
        var active = string.Equals(s.Name, activeActorName, StringComparison.Ordinal);
        var dead = !s.IsAlive;

        var borderFg = active ? ConsoleColor.White
                     : dead ? ConsoleColor.Gray
                     : s.IsHero ? ConsoleColor.Blue
                     : ConsoleColor.DarkMagenta;

        char h = active ? '═' : '─';
        char tl = active ? '╔' : '┌';
        char tr = active ? '╗' : '┐';
        char bl = active ? '╚' : '└';
        char br = active ? '╝' : '┘';
        char vb = active ? '║' : '│';

        var top = new List<Seg> { new Seg($"{tl}{new string(h, CONTENT_W + 2)}{tr}", borderFg) };
        var bot = new List<Seg> { new Seg($"{bl}{new string(h, CONTENT_W + 2)}{br}", borderFg) };

        if (dead)
        {
            var status = s.Hp <= -10 ? "[ SLAIN  ]" : "[UNCONSC ]";
            var namePart = $"  \u2715 {s.Name.ToUpper()}".PadRight(CONTENT_W - status.Length);
            var empty = new string(' ', CONTENT_W);
            return
            [
                top,
                CL(vb, borderFg, new Seg(namePart, ConsoleColor.Gray), new Seg(status, ConsoleColor.DarkRed)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                bot
            ];
        }

        var indicator = active ? "\u25b6 " : "  ";
        var indicFg = active ? ConsoleColor.White : s.IsHero ? ConsoleColor.Cyan : ConsoleColor.Red;
        var nameStr = (s.Name.Length > 10 ? s.Name.ToUpper()[..10] : s.Name.ToUpper()).PadRight(10);
        var weapTrunc = s.Weapon.Length > 14 ? s.Weapon[..14] : s.Weapon.PadRight(14);
        var weapStr = DisplayConfig.IsFieldEnabled("characterCard", "CurrentWeapon")
            ? $"[{weapTrunc}]" : new string(' ', weapTrunc.Length + 2);

        var nameLine = CL(vb, borderFg,
            new Seg(indicator, indicFg),
            new Seg(nameStr, active ? ConsoleColor.White : ConsoleColor.White),
            new Seg("   ", ConsoleColor.Gray),
            new Seg(weapStr, active ? ConsoleColor.Yellow : ConsoleColor.Gray));

        var tmFilled = Math.Min(BAR_W, (int)(Math.Min(1.0, s.Tm / 100.0) * BAR_W));
        var tmLine = CL(vb, borderFg,
            new Seg(" TM [", ConsoleColor.Gray),
            new Seg(new string('|', tmFilled), ConsoleColor.Cyan),
            new Seg(new string('\u2591', BAR_W - tmFilled), ConsoleColor.Gray),
            new Seg("]  ", ConsoleColor.Gray),
            new Seg($"  {s.Tm,3}", ConsoleColor.Cyan),
            new Seg("/100", ConsoleColor.Gray));

        var manaLine = s.MaxMana > 0
            ? CL(vb, borderFg,
                new Seg(" MP [", ConsoleColor.Gray),
                new Seg(new string('\u2588', Math.Max(1, (int)((double)Math.Max(0, s.Mana) / s.MaxMana * BAR_W))), ConsoleColor.Magenta),
                new Seg(new string('\u2591', BAR_W - Math.Max(1, (int)((double)Math.Max(0, s.Mana) / s.MaxMana * BAR_W))), ConsoleColor.Gray),
                new Seg("]  ", ConsoleColor.Gray),
                new Seg($"{Math.Max(0, s.Mana),3}", ConsoleColor.Magenta),
                new Seg(" / ", ConsoleColor.Gray),
                new Seg($"{s.MaxMana,-3}", ConsoleColor.Gray))
            : CL(vb, borderFg,
                new Seg(new string(' ', CONTENT_W), ConsoleColor.Black));

        var pct = (double)Math.Max(0, s.Hp) / Math.Max(1, s.MaxHp);
        var hpFilled = s.Hp > 0 ? Math.Max(1, (int)(pct * BAR_W)) : 0;
        var hpFg = HpColor(s.Hp, s.MaxHp);
        var maxHpSuffix = DisplayConfig.IsFieldEnabled("characterCard", "MaxHp")
            ? $"{s.MaxHp,-3}" : "   ";
        var hpLine = CL(vb, borderFg,
            new Seg(" HP [", ConsoleColor.Gray),
            new Seg(new string('\u2588', hpFilled), hpFg),
            new Seg(new string('\u2591', BAR_W - hpFilled), ConsoleColor.Gray),
            new Seg("]  ", ConsoleColor.Gray),
            new Seg($"{Math.Max(0, s.Hp),3}", hpFg),
            new Seg(" / ", ConsoleColor.Gray),
            new Seg(maxHpSuffix, ConsoleColor.Gray));

        var lines = new List<List<Seg>> { top, nameLine };
        if (DisplayConfig.IsFieldEnabled("characterCard", "TurnMeter")) lines.Add(tmLine);
        lines.Add(manaLine);
        if (DisplayConfig.IsFieldEnabled("characterCard", "CurrentHp") ||
            DisplayConfig.IsFieldEnabled("characterCard", "MaxHp"))
            lines.Add(hpLine);
        lines.Add(bot);
        return lines;
    }

    private static List<Seg> CL(char vb, ConsoleColor borderFg, params Seg[] segs)
    {
        var line = new List<Seg> { new Seg($"{vb} ", borderFg) };
        line.AddRange(segs);
        line.Add(new Seg($" {vb}", borderFg));
        return line;
    }

    private static void PrintBlockPair(List<List<Seg>> left, List<List<Seg>> right)
    {
        var maxLines = Math.Max(left.Count, right.Count);
        var blank = new List<Seg> { new Seg(new string(' ', BLOCK_W), ConsoleColor.Black) };

        for (var i = 0; i < maxLines; i++)
        {
            var l = i < left.Count ? left[i] : blank;
            var r = i < right.Count ? right[i] : blank;

            Console.Write("  ");
            foreach (var seg in l) { Console.ForegroundColor = seg.Fg; Console.Write(seg.Text); }
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  \u2551  ");
            Console.ResetColor();
            foreach (var seg in r) { Console.ForegroundColor = seg.Fg; Console.Write(seg.Text); }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}

// ── Seg ────────────────────────────────────────────────────────────────────────

internal record Seg(string Text, ConsoleColor Fg = ConsoleColor.Gray);
