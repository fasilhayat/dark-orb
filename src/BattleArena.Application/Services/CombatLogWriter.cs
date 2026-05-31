namespace BattleArena.Application.Services;

using System.Text;
using System.Text.Json;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

// Writes a complete, human-readable combat log to a .txt file and a companion
// .json replay file. Both files share the same base name and timestamp.
//
// The .json contains everything needed to re-run the combat identically:
//   CombatReplayer.ReplayFromFile("combat-logs/Duel_WarriorVsWarrior_20260527_100517.json")
//
// Usage:
//   string path = CombatLogWriter.Write(result, "Duel_WarriorVsWarrior", outputDir);
public static class CombatLogWriter
{
    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ── Entry point ───────────────────────────────────────────────────────────────

    public static string Write(CombatResult result, string label, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var safe      = string.Join("_", label.Split(Path.GetInvalidFileNameChars()));
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var baseName  = $"{safe}_{timestamp}";
        var txtPath   = Path.Combine(outputDirectory, $"{baseName}.txt");
        var jsonPath  = Path.Combine(outputDirectory, $"{baseName}.json");

        // Build snapshot for JSON
        var snapshot = CombatSnapshot.From(result, label);

        // Write companion JSON (replay file)
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(snapshot, _json), Encoding.UTF8);

        // Write human-readable text log
        File.WriteAllText(txtPath, BuildContent(result, label, snapshot, baseName), Encoding.UTF8);

        return txtPath;
    }

    // ── Content builder ───────────────────────────────────────────────────────────

    private static string BuildContent(CombatResult result, string label, CombatSnapshot snapshot, string baseName)
    {
        var sb   = new StringBuilder();
        var bar  = new string('═', 80);
        var thin = new string('─', 80);

        // ── Header ──────────────────────────────────────────────────────────────
        sb.AppendLine(bar);
        sb.AppendLine($"  COMBAT LOG  —  {label}");
        sb.AppendLine($"  Generated : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(bar);

        // ── Replay metadata ──────────────────────────────────────────────────────
        sb.AppendLine();
        sb.AppendLine("  REPLAY");
        sb.AppendLine(thin);
        sb.AppendLine($"  Seed      : {result.Seed}");
        sb.AppendLine($"  JSON file : {baseName}.json");
        sb.AppendLine($"  Replay    : CombatReplayer.ReplayFromFile(\"{baseName}.json\")");
        sb.AppendLine($"  Produces  : identical CombatResult when run against the same build");

        // ── Participants ─────────────────────────────────────────────────────────
        sb.AppendLine();
        sb.AppendLine("  PARTICIPANTS");
        sb.AppendLine(thin);

        var allParties = new[] { result.WinningParty, result.LosingParty }
            .Where(p => p is not null)
            .Cast<Party>()
            .Distinct()
            .ToList();

        foreach (var party in allParties)
        {
            sb.AppendLine($"  ── {party.Name} ──");
            foreach (var m in party.Members)
            {
                var ch = m.Character;
                var src = m.AttackSource;
                sb.AppendLine($"    {ch.Name,-12} Lv{ch.Level,-2}  HP {ch.MaxHitPoints,3}  TurnSpeed {ch.TurnSpeed,2}  STR {ch.Strength,2}  DEX {ch.Dexterity,2}  INT {ch.Intelligence,2}");

                if (ch.Equipment.Chest is { } armor)
                    sb.AppendLine($"    {"",12} Armor  : {armor.Name}  AC {armor.ArmorClass}  Mitigation {armor.Mitigation}");

                if (src is not null)
                    sb.AppendLine($"    {"",12} Weapon : {src.Name}  {src.DamageCount}d{DieSides(src.DamageDie)}+{src.AttackBonus}  {src.DamageType}  {src.AttackType}");

                foreach (var spell in ch.MemorizedSpells)
                {
                    sb.AppendLine($"    {"",12} Spell  : {spell.Name}  {spell.DamageCount}d{DieSides(spell.DamageDie)}+{spell.AttackBonus}  {spell.DamageType}");
                    foreach (var eff in spell.OnHitEffects)
                        sb.AppendLine($"    {"",12}          ↳ OnHit [{eff.Name}] {eff.Type}  duration {eff.Duration}  chance {eff.ApplicationChance}%  resist {eff.ResistanceType}");
                }

                if (ch.Race?.Feats.Count > 0)
                    foreach (var feat in ch.Race.Feats)
                        foreach (var res in feat.Resistances)
                            sb.AppendLine($"    {"",12} Racial : {feat.Name}  +{res.Value} {res.Type} resistance");
            }
        }

        // ── Tick log ─────────────────────────────────────────────────────────────
        sb.AppendLine();
        sb.AppendLine(bar);
        sb.AppendLine("  TICK  LOG");
        sb.AppendLine(bar);
        sb.AppendLine();

        int turnNo = 0;
        int lastTick = -1;

        foreach (var e in result.Log)
        {
            // Skip raw TM gain noise — only show READY/ACTIVE transitions
            if (e.EventType == "TurnMeterGain")
            {
                if (!e.IsReady && !e.IsActive) continue;
                if (e.IsActive) continue; // shown via TurnStart already
                sb.AppendLine($"  [{e.Tick,5}]  TM  {e.ActorName,-12}  {e.TurnMeterBefore,3} → {e.TurnMeterAfter,3}  [READY TO ACT]");
                lastTick = e.Tick;
                continue;
            }

            switch (e.EventType)
            {
                case "TurnStart":
                    if (e.Tick != lastTick) sb.AppendLine();
                    turnNo++;
                    var spell = e.IsSpell ? " (spell)" : "";
                    sb.AppendLine($"  [{e.Tick,5}]  ══ TURN {turnNo,3} ══  {e.ActorName,-12} → {e.TargetName,-12}  [{e.AttackSourceName}{spell}]");
                    lastTick = e.Tick;
                    break;

                case "Attack":
                    var hit   = e.IsHit == true ? "HIT" : "MISS";
                    var crit  = e.IsCritical == true      ? " !! CRITICAL !!"      : "";
                    var fumb  = e.IsFumble  == true       ? " ~~ FUMBLE ~~"        : "";
                    var spcl  = e.IsDevastatingStrike == true ? " *** DEVASTATING STRIKE ***" :
                                e.IsClash             == true ? " ~~ CLASH ~~"               :
                                e.IsPerfectParry      == true ? " >> PERFECT PARRY <<"        :
                                e.IsTotalReversal     == true ? " *** TOTAL REVERSAL ***"     : "";
                    var defRoll = e.DefenseRoll.HasValue ? $"  d20_def={e.DefenseRoll,2}" : "";
                    var total = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
                    sb.AppendLine($"           Attack   d20_atk={e.DieRoll,2}{defRoll}  +AP {e.AttackPower,3}  vs DP {e.DefensePower,3}  total={total,3}  -> {hit}{crit}{fumb}{spcl}");
                    if (!string.IsNullOrEmpty(e.Phrase))
                        sb.AppendLine($"                    \"{e.Phrase}\"");
                    if (e.IsHit == true && !string.IsNullOrEmpty(e.Message))
                    {
                        var dmgIdx = e.Message.IndexOf(" | Dmg: ", StringComparison.Ordinal);
                        if (dmgIdx >= 0)
                            sb.AppendLine($"           Damage   {e.Message[(dmgIdx + 8)..]}");
                    }
                    break;

                case "Damage":
                    sb.AppendLine($"           Damage   {e.ActorName,-12}  HP {e.TargetHpBefore,4} → {e.TargetHpAfter,4}  (-{e.DamageDealt})");
                    break;

                case "FumblePenalty":
                    sb.AppendLine($"           FUMBLE   {e.ActorName} — attack power penalised next turn");
                    break;

                case "PerfectParry":
                    sb.AppendLine($"           PARRY    {e.ActorName} — perfect parry! TM {e.TurnMeterBefore} -> {e.TurnMeterAfter}");
                    break;

                case "TotalReversal":
                    sb.AppendLine($"           REVERSAL {e.ActorName} — total reversal! TM {e.TurnMeterBefore} -> {e.TurnMeterAfter}");
                    break;

                case "Clash":
                    sb.AppendLine($"           CLASH    {e.ActorName} vs {e.TargetName} — weapons collide, mutual damage");
                    break;

                case "DevastatingStrike":
                    sb.AppendLine($"           DEVAST   {e.ActorName} -> {e.TargetName} — x3 damage ({e.DamageDealt} dealt)");
                    break;

                case "EffectApplied":
                    sb.AppendLine($"           Effect   [{e.StatusEffectName}] applied to {e.ActorName}  (duration {e.Message.GetDurationFromMessage()} turns)");
                    break;

                case "EffectResisted":
                    sb.AppendLine($"           Resisted [{e.StatusEffectName}] by {e.ActorName}  (rolled {e.ResistRoll} vs threshold {e.ResistThreshold})");
                    break;

                case "EffectExpired":
                    sb.AppendLine($"           Expired  [{e.StatusEffectName}] worn off {e.ActorName}");
                    break;

                case "DoTTick":
                    sb.AppendLine($"           DoT      {e.ActorName,-12}  suffers {e.DamageDealt} {e.StatusEffectName} damage");
                    break;

                case "SkippedTurn":
                    sb.AppendLine($"  [{e.Tick,5}]  SKIP  {e.ActorName,-12}  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "TurnEnd":
                    // Mark the end of a turn visually
                    sb.AppendLine($"                    TM after turn: {e.ActorName} → {e.TurnMeterAfter}");
                    break;

                case "Death":
                    sb.AppendLine();
                    sb.AppendLine($"  [{e.Tick,5}]  *** DEATH    : {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "KnockedOut":
                    sb.AppendLine();
                    sb.AppendLine($"  [{e.Tick,5}]  *** KNOCKOUT : {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "ApiCall":
                    sb.AppendLine($"  [{e.Tick,5}]  DICE  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "SpellQueued":
                    sb.AppendLine($"  [{e.Tick,5}]  QUEUE  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "SpellCharging":
                    sb.AppendLine($"  [{e.Tick,5}]  CHARGE  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "SpellLost":
                    sb.AppendLine($"  [{e.Tick,5}]  LOST  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "ConcentrationPass":
                    sb.AppendLine($"  [{e.Tick,5}]  CONC  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "ManaRegen":
                    sb.AppendLine($"  [{e.Tick,5}]  MANA  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "InsufficientMana":
                    sb.AppendLine($"  [{e.Tick,5}]  NOMANA  {e.Message}");
                    lastTick = e.Tick;
                    break;

                case "ManaDeduct":
                    sb.AppendLine($"  [{e.Tick,5}]  MANACOST  {e.Message}");
                    lastTick = e.Tick;
                    break;
            }
        }

        // ── Summary ──────────────────────────────────────────────────────────────
        sb.AppendLine();
        sb.AppendLine(bar);
        sb.AppendLine("  SUMMARY");
        sb.AppendLine(bar);
        sb.AppendLine();

        if (result.MaxTicksReached)
        {
            sb.AppendLine($"  Result   : TIMEOUT — no winner after {result.TotalTicks} ticks");
        }
        else
        {
            var loserTag = result.LoserStatus == CharacterVitalStatus.Dead ? "SLAIN" : "KNOCKED OUT";
            sb.AppendLine($"  Result   : {result.WinningParty?.Name} WINS  ({result.LosingParty?.Name} {loserTag})");
        }

        sb.AppendLine($"  Duration : {result.TotalTicks} ticks");
        sb.AppendLine();

        var attacks = result.Log.Where(e => e.EventType == "Attack").ToList();
        var hits    = attacks.Count(e => e.IsHit    == true);
        var misses  = attacks.Count(e => e.IsHit    == false && e.IsFumble == false);
        var crits   = attacks.Count(e => e.IsCritical == true);
        var fumbles = attacks.Count(e => e.IsFumble   == true);
        var hitPct  = attacks.Count > 0 ? (int)(100.0 * hits / attacks.Count) : 0;

        sb.AppendLine($"  Turns    : {turnNo}  |  Hits: {hits}  |  Misses: {misses}  |  Crits: {crits}  |  Fumbles: {fumbles}  |  Hit rate: {hitPct}%");
        sb.AppendLine();

        // Per-character stats
        var effects     = result.Log.Where(e => e.EventType == "EffectApplied").ToList();
        var resisted    = result.Log.Where(e => e.EventType == "EffectResisted").ToList();
        var dots        = result.Log.Where(e => e.EventType == "DoTTick").ToList();
        var allActors   = attacks.Select(e => e.ActorName).Distinct().OrderBy(n => n).ToList();

        sb.AppendLine("  Per-character breakdown:");
        sb.AppendLine($"  {"Name",-14} {"Turns",6} {"Hits",5} {"Crits",6} {"Fmb",4} {"DmgDealt",9} {"Avg",6} {"DoT",5} {"FxApplied",10} {"FxResisted",10}");
        sb.AppendLine($"  {new string('-', 76)}");

        foreach (var name in allActors)
        {
            var myAtk  = attacks.Where(e => e.ActorName == name).ToList();
            var myHits = myAtk.Count(e => e.IsHit == true);
            var myCrit = myAtk.Count(e => e.IsCritical == true);
            var myFmb  = myAtk.Count(e => e.IsFumble == true);
            var myDmg  = result.Log
                .Where(e => e.EventType == "Damage" && e.ActorName != name)
                .Where(e => IsActorTurn(result.Log, e, name))
                .Sum(e => e.DamageDealt ?? 0);
            var myAvg  = myHits > 0 ? (double)myDmg / myHits : 0;
            var myDot  = dots.Count(e => e.ActorName == name);
            var myFx   = effects.Count(e => e.Message.Contains(name) || e.ActorName == name);
            var myRes  = resisted.Count(e => e.ActorName == name);

            sb.AppendLine($"  {name,-14} {myAtk.Count,6} {myHits,5} {myCrit,6} {myFmb,4} {myDmg,9} {myAvg,6:F1} {myDot,5} {myFx,10} {myRes,10}");
        }

        sb.AppendLine();

        // Effect summary
        var allEffects = result.Log
            .Where(e => e.EventType is "EffectApplied" or "EffectResisted")
            .GroupBy(e => e.StatusEffectName ?? "?")
            .OrderByDescending(g => g.Count())
            .ToList();

        if (allEffects.Count > 0)
        {
            sb.AppendLine("  Status effect summary:");
            foreach (var g in allEffects)
            {
                var app = g.Count(e => e.EventType == "EffectApplied");
                var res = g.Count(e => e.EventType == "EffectResisted");
                sb.AppendLine($"    {g.Key,-18} applied {app,3}  resisted {res,3}");
            }
            sb.AppendLine();
        }

        sb.AppendLine(bar);
        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static int DieSides(DieType d) => d switch
    {
        DieType.D4  => 4,  DieType.D6  => 6,  DieType.D8  => 8,
        DieType.D10 => 10, DieType.D12 => 12, DieType.D20 => 20,
        _ => 0
    };

    // Determine who dealt damage in a Damage event by finding the most recent TurnStart before it.
    private static bool IsActorTurn(List<CombatLogEntry> log, CombatLogEntry dmgEntry, string actorName)
    {
        for (int i = log.IndexOf(dmgEntry) - 1; i >= 0; i--)
        {
            if (log[i].EventType == "TurnStart")
                return log[i].ActorName == actorName;
        }
        return false;
    }
}

// ── Extension helpers ─────────────────────────────────────────────────────────

internal static class CombatLogStringExtensions
{
    // Extract duration number from a message like "Burning applied for 3 turns" → "3"
    // Falls back to "?" if not parseable.
    internal static string GetDurationFromMessage(this string msg)
    {
        if (string.IsNullOrEmpty(msg)) return "?";
        var parts = msg.Split(' ');
        for (int i = 0; i < parts.Length - 1; i++)
            if (parts[i].Equals("for", StringComparison.OrdinalIgnoreCase) && int.TryParse(parts[i + 1], out _))
                return parts[i + 1];
        return "?";
    }
}
