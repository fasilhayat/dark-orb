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
        var sexDisplay = ch.Sex switch { "F" => "Female", "M" => "Male", _ => "None" };
        var raceDisplay = ch.Race?.Name ?? "";
        Row2($"{role}: {ch.Name}", $"{sexDisplay} · {raceDisplay} · Level {ch.Level} {ch.ClassName}", ConsoleColor.White);
        Sep();
        Row($"HP: {ch.MaxHitPoints}   TurnSpeed: {ch.TurnSpeed}   StrikeRating: {ch.StrikeRating}");
        Row($"STR: {ch.Strength} ({Sign((ch.Strength - 10) / 2)}{(ch.Strength - 10) / 2})   DEX: {ch.Dexterity} ({Sign(dexMod)}{dexMod})   INT: {ch.Intelligence} ({Sign((ch.Intelligence - 10) / 2)}{(ch.Intelligence - 10) / 2})");
        Sep();
        Row($"Armor   : {ch.Equipment.Chest?.Name ?? "None",-18} AC {ac,-2}  Mitigation: {mit}");
        if (ch.MemorizedSpells.Count > 0)
            foreach (var spell in ch.MemorizedSpells)
                Row($"Spells  : {spell.Name,-18} {spell.DamageCount}d{DieSides(spell.DamageDie)} {spell.DamageType}");
        else if (attackSource is not null)
            Row($"Weapon  : {attackSource.Name,-18} {attackSource.DamageCount}d{DieSides(attackSource.DamageDie)} {attackSource.DamageType,-10} +{attackSource.AttackBonus} atk bonus");
        Sep();
        var abilityLabel = displaySource.UsesIntelligence ? "int"
                     : displaySource.AttackType == AttackType.Ranged ? "dex" : "str";
        Row($"Atk Power : {ap,-4}  {ch.StrikeRating} (SR) + {ch.Level} (lvl) + ({Sign(abilityMod)}{abilityMod}) ({abilityLabel}) + {displaySource.AttackBonus} (src)");
        Row($"Def Power : {dp,-4}  {ac} (AC) + ({Sign(dexCap)}{dexCap}) (dex)");
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
                CW("     ⚡ d20=", ConsoleColor.Cyan);
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
                CW("⚠ FUMBLE", ConsoleColor.Yellow);
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
                        CW(e.Message[dmgIdx..], ConsoleColor.Cyan);
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
            CWL($"     \"{e.Phrase}\"", ConsoleColor.Cyan);
    }

    // ── ShowHp ────────────────────────────────────────────────────────────────────

    internal static void ShowHp(string name, int current, int max, int w = 24, string info = "")
    {
        var pct = (double)Math.Max(0, current) / Math.Max(1, max);
        var filled = current > 0 ? Math.Max(1, (int)(pct * w)) : 0;
        var barCol = HpColor(current, max);

        Console.Write("  ");
        CW($"{name,-18}", CharColor(name));
        if (info.Length > 0) CW($" {info,-38}", ConsoleColor.DarkGray);
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

    internal static string CharInfo(Character ch)
    {
        var sex = ch.Sex switch { "F" => "Female", "M" => "Male", _ => "None" };
        return $"{sex} · {ch.Race?.Name ?? ""} · Lvl {ch.Level} {ch.ClassName}";
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
            CWL("  COMBAT TIMEOUT — no winner declared.", ConsoleColor.Yellow);
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
        CWL($" / {fumbles} fumbles", ConsoleColor.Yellow);

        CWL("\n  Damage dealt:", ConsoleColor.White);
        foreach (var m in wParty.Members.Concat(lParty.Members))
        {
            var dmg = attacks.Where(e => e.ActorName == m.Character.Name && e.IsHit == true).Sum(e => e.DamageDealt ?? 0);
            var isWinner = wParty.Members.Any(wm => wm.Character.Name == m.Character.Name);
            var info = CharInfo(m.Character);
            CW("    ");
            CW($"{m.Character.Name,-18}", isWinner ? ConsoleColor.Green : ConsoleColor.Gray);
            CW($" {info,-38}", ConsoleColor.DarkGray);
            CW($"  {dmg,3} dmg", ConsoleColor.Yellow);
            CWL(isWinner ? "  [winner side]" : "  [loser side]", isWinner ? ConsoleColor.Green : ConsoleColor.Gray);
        }

        CWL("\n  Final HP:", ConsoleColor.White);
        CWL("  ── Winners ──────────────────────────────────────────────", ConsoleColor.Green);
        foreach (var m in wParty.Members)
            ShowHp(m.Character.Name, m.Character.CurrentHitPoints, MaxHp.GetValueOrDefault(m.Character.Name, 1), info: CharInfo(m.Character));
        CWL("  ── Losers ───────────────────────────────────────────────", ConsoleColor.Red);
        foreach (var m in lParty.Members)
            ShowHp(m.Character.Name, m.Character.CurrentHitPoints, MaxHp.GetValueOrDefault(m.Character.Name, 1), info: CharInfo(m.Character));

        if (DisplayConfig.IsFieldEnabled("combatSummary", "LoserStatus") ||
            DisplayConfig.IsFieldEnabled("combatSummary", "LoserName"))
        {
            var loserName = DisplayConfig.IsFieldEnabled("combatSummary", "LoserName") ? lParty.Name : "The losing party";
            var loserTag = Result.LoserStatus == CharacterVitalStatus.Dead ? "SLAIN" : "unconscious";
            CWL($"\n  {loserName} is {loserTag}!",
                Result.LoserStatus == CharacterVitalStatus.Dead
                    ? ConsoleColor.Red : ConsoleColor.Yellow);
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
        CW("[", ConsoleColor.Yellow);
        CW(new string('\u2588', filled), ConsoleColor.Yellow);
        CW(new string('\u2591', BarWidth - filled), ConsoleColor.Gray);
        CW("]", ConsoleColor.Yellow);
        CW($"  {tickInRound}", ConsoleColor.White);
        CW(" / ", ConsoleColor.Gray);
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
        CWL(rightLabel, isDuel ? ConsoleColor.White : ConsoleColor.Magenta);
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

        // ── Card field widths (sum must equal CONTENT_W = 42) ──────────────────
        // Row 1 (name):     W_INDICATOR(2) + W_NAME(18) + W_GAP(3) + [ + W_WEAPON(17) + ] = 42
        // Row 2 (info):     W_SEX(6) + "  ·  "(5) + "Lvl "(4) + {level,2}(2) + "  "(2) + W_CLASS(10) + "  ·  "(5) + W_RACE(8) = 42
        // Row 3-5 (TM/HP):  " TM["(5) + BAR_W(25) + "]  "(3) + value_fields(9) = 42
        const int W_NAME   = 18;
        const int W_GAP    = 3;
        const int W_WEAPON = 17;
        const int W_SEX    = 6;
        const int W_CLASS  = 10;
        const int W_RACE   = 8;

        var borderFg = active ? ConsoleColor.White
                     : dead ? ConsoleColor.Gray
                     : s.IsHero ? ConsoleColor.Blue
                     : ConsoleColor.Magenta;

        const char h  = '─';
        const char tl = '┌';
        const char tr = '┐';
        const char bl = '└';
        const char br = '┘';
        const char vb = '│';

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
                CL(vb, borderFg, new Seg(namePart, ConsoleColor.Gray), new Seg(status, ConsoleColor.Red)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                CL(vb, borderFg, new Seg(empty, ConsoleColor.Gray)),
                bot
            ];
        }

        // ── Name row ───────────────────────────────────────────────────────────
        var indicator = active ? "\u25b6 " : "  ";
        var indicFg = active ? ConsoleColor.White : s.IsHero ? ConsoleColor.Cyan : ConsoleColor.Red;
        var nameStr = (s.Name.Length > W_NAME ? s.Name.ToUpper()[..W_NAME] : s.Name.ToUpper()).PadRight(W_NAME);
        var weapTrunc = s.Weapon.Length > W_WEAPON ? s.Weapon[..W_WEAPON] : s.Weapon.PadRight(W_WEAPON);
        var weapStr = DisplayConfig.IsFieldEnabled("characterCard", "CurrentWeapon")
            ? $"[{weapTrunc}]" : new string(' ', W_WEAPON + 2);

        var nameLine = CL(vb, borderFg,
            new Seg(indicator, indicFg),
            new Seg(nameStr, active ? ConsoleColor.White : ConsoleColor.White),
            new Seg(new string(' ', W_GAP), ConsoleColor.Gray),
            new Seg(weapStr, active ? ConsoleColor.Yellow : ConsoleColor.Gray));
        // Total: W_INDICATOR(2) + W_NAME(18) + W_GAP(3) + [ + W_WEAPON(17) + ] = 42 ✓

        // ── Info row (Sex  ·  Lvl Level  Class  ·  Race) ───────────────────────
        var sexLabel = s.Sex switch { "F" => "Female", "M" => "Male", _ => "None" };
        var infoStr = $"{sexLabel,-W_SEX}  ·  Lvl {s.Level,2}  {s.ClassName,-W_CLASS}  ·  {s.Race,-W_RACE}";
        var classLine = CL(vb, borderFg, new Seg(infoStr, ConsoleColor.Gray));

        // ── TM row ─────────────────────────────────────────────────────────────
        var cappedTm = Math.Min(100, s.Tm);
        var tmFilled = Math.Min(BAR_W, (int)(Math.Min(1.0, cappedTm / 100.0) * BAR_W));
        var tmLine = CL(vb, borderFg,
            new Seg(" TM [", ConsoleColor.Gray),
            new Seg(new string('|', tmFilled), ConsoleColor.Cyan),
            new Seg(new string('\u2591', BAR_W - tmFilled), ConsoleColor.Gray),
            new Seg("]  ", ConsoleColor.Gray),
            new Seg($"{cappedTm,3}", ConsoleColor.Cyan),
            new Seg(" / ", ConsoleColor.Gray),
            new Seg("100", ConsoleColor.Gray));

        // ── Mana row ───────────────────────────────────────────────────────────
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

        // ── HP row ─────────────────────────────────────────────────────────────
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

        var lines = new List<List<Seg>> { top, nameLine, classLine };
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
            Console.Write("  \u2502  ");
            Console.ResetColor();
            foreach (var seg in r) { Console.ForegroundColor = seg.Fg; Console.Write(seg.Text); }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}

// ── Seg ────────────────────────────────────────────────────────────────────────

internal record Seg(string Text, ConsoleColor Fg = ConsoleColor.Gray);
