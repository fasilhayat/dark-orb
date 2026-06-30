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

    public static string Write(CombatResult result, string label, string outputDirectory, string mode = "")
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
        File.WriteAllText(txtPath, BuildContent(result, label, snapshot, baseName, mode), Encoding.UTF8);

        return txtPath;
    }

    // ── Content builder ───────────────────────────────────────────────────────────

    private static string BuildContent(CombatResult result, string label, CombatSnapshot snapshot, string baseName, string mode = "")
    {
        var sb   = new StringBuilder();
        var bar  = new string('═', 80);
        var thin = new string('─', 80);

        // ── Header ──────────────────────────────────────────────────────────────
        sb.AppendLine(bar);
        sb.AppendLine($"  COMBAT LOG  —  {label}");
        sb.AppendLine($"  Generated : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        if (!string.IsNullOrEmpty(mode))
            sb.AppendLine($"  Mode      : {mode}");
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
        bool inTurn = false;
        int attackNo = 0;
        int totalAttacksInTurn = 0;
        string? turnActor = null;
        List<string> pendingDice = new();
        CombatLogEntry? pendingMana = null;

        // Peek ahead to count Attack entries for multi-attack labeling.
        int CountAttacksForCurrentActor(int fromIndex)
        {
            int count = 0;
            for (int i = fromIndex; i < result.Log.Count; i++)
            {
                var e2 = result.Log[i];
                if (e2.EventType == "TurnStart" && e2.ActorName != turnActor) break;
                if (e2.EventType == "TurnEnd") break;
                if (e2.EventType == "Attack" && e2.ActorName == turnActor) count++;
            }
            return count;
        }

        void FlushPendingMana()
        {
            if (pendingMana != null)
            {
                sb.AppendLine($"  MANA  {pendingMana.Message}  tick={pendingMana.Tick}");
                pendingMana = null;
            }
        }

        void FlushDice()
        {
            if (pendingDice.Count == 0) return;
            var parts = pendingDice
                .Select(d => d.Split(" → "))
                .GroupBy(p => p[0])
                .Select(g => $"{char.ToUpperInvariant(g.Key[0]) + g.Key[1..]}: {string.Join(" ", g.Select(p => p[1]))}");
            sb.AppendLine($"    {string.Join("  |  ", parts)}");
            pendingDice.Clear();
        }

        for (int i = 0; i < result.Log.Count; i++)
        {
            var e = result.Log[i];

            // Skip raw TM gain noise — only show READY transitions
            if (e.EventType == "TurnMeterGain")
            {
                if (!e.IsReady || e.IsActive) continue;
                FlushPendingMana();
                sb.AppendLine($"  TM  {e.ActorName,-12}  {e.TurnMeterBefore,3} → {e.TurnMeterAfter,3}  [READY]  tick={e.Tick}");
                continue;
            }

            // ── In-turn events ────────────────────────────────────────────────
            if (inTurn)
            {
                switch (e.EventType)
                {
                    case "ApiCall":
                        var diceMsg = e.Message;
                        int arrowIdx = diceMsg.IndexOf(" → ");
                        if (arrowIdx > 0)
                        {
                            int spaceIdx = diceMsg.LastIndexOf(' ', arrowIdx - 1);
                            var die = spaceIdx >= 0 ? diceMsg[(spaceIdx + 1)..arrowIdx] : diceMsg[..arrowIdx];
                            pendingDice.Add($"{die} → {diceMsg[(arrowIdx + 3)..]}");
                        }
                        continue;

                    case "ExtraAttack":
                        continue;

                    case "Attack":
                        FlushDice();
                        attackNo++;
                        if (attackNo == 1)
                            totalAttacksInTurn = CountAttacksForCurrentActor(i);

                        var hitMiss = e.IsHit == true ? "HIT" : "MISS";
                        var tags = "";
                        if (e.IsCritical == true) tags += "  CRIT!";
                        if (e.IsFumble == true) tags += "  FUMBLE!";
                        if (e.IsDevastatingStrike == true) tags += "  DEVASTATING!";
                        if (e.IsClash == true) tags += "  CLASH!";
                        if (e.IsPerfectParry == true) tags += "  PARRY!";
                        if (e.IsTotalReversal == true) tags += "  REVERSAL!";
                        int atkTotal = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
                        int defTotal = (e.DefenseRoll ?? 0) + (e.DefensePower ?? 0);
                        var atkLabel = totalAttacksInTurn > 1 ? $" {attackNo}/{totalAttacksInTurn}" : "";
                        sb.AppendLine($"    ATTACK{atkLabel}  {hitMiss}{tags}  [{e.DieRoll}+{e.AttackPower}={atkTotal} vs {e.DefenseRoll}+{e.DefensePower}={defTotal}]");
                        if (!string.IsNullOrEmpty(e.Phrase))
                            sb.AppendLine($"    \"{e.Phrase}\"");
                        // Damage formula embedded in Attack entry message
                        if (e.IsHit == true && !string.IsNullOrEmpty(e.Message))
                        {
                            int dmgIdx = e.Message.IndexOf(" | Dmg: ", StringComparison.Ordinal);
                            if (dmgIdx >= 0)
                                sb.AppendLine($"    DMG   {e.Message[(dmgIdx + 8)..]}");
                        }
                        continue;

                    case "Damage":
                        if (e.TargetHpBefore.HasValue)
                            sb.AppendLine($"    HP   {e.ActorName,-12}  {e.TargetHpBefore,4} → {e.TargetHpAfter,4}  (-{e.DamageDealt})");
                        continue;

                    case "EffectApplied":
                        sb.AppendLine($"    EFFECT  [{e.StatusEffectName}] applied  dur={e.EffectDuration}");
                        continue;

                    case "EffectResisted":
                        sb.AppendLine($"    RESIST  [{e.StatusEffectName}]  rolled {e.ResistRoll} vs {e.ResistThreshold}");
                        continue;

                    case "EffectExpired":
                        sb.AppendLine($"    EXPIRED  [{e.StatusEffectName}] worn off {e.ActorName}");
                        continue;

                    case "DoTTick":
                        sb.AppendLine($"    DOT  {e.ActorName}  -{e.DamageDealt} HP  [{e.StatusEffectName}]");
                        continue;

                    case "FumblePenalty":
                        sb.AppendLine($"    FUMBLE  {e.ActorName} — attack power penalised next turn");
                        continue;

                    case "Healed":
                        sb.AppendLine($"    HEAL  {e.ActorName}  +{e.DamageDealt} HP  ({e.TargetHpBefore} → {e.TargetHpAfter})  [{e.AttackSourceName}]");
                        continue;

                    case "LeechTick":
                        var lsym = e.LeechResourceType == LeechResources.Mana ? "♦" : "♥";
                        var lres = e.LeechResourceType == LeechResources.Mana ? "mana" : "HP";
                        sb.AppendLine($"    LEECH  {lsym} {e.ActorName} -{e.LeechAmount} {lres} → {e.LeechCasterName} +{e.LeechAmount}  [{e.StatusEffectName}]");
                        continue;

                    case "ManaRegen":
                        sb.AppendLine($"    MANA  {e.Message}");
                        continue;

                    case "ManaDeduct":
                        sb.AppendLine($"    MANA  {e.Message}");
                        continue;

                    case "Death":
                    case "KnockedOut":
                        sb.AppendLine($"    *** {e.EventType}  {e.Message}");
                        continue;

                    case "PerfectParry":
                        sb.AppendLine($"    PARRY  {e.ActorName}  TM {e.TurnMeterBefore} → {e.TurnMeterAfter}");
                        continue;

                    case "TotalReversal":
                        sb.AppendLine($"    REVERSAL  {e.ActorName}  TM {e.TurnMeterBefore} → {e.TurnMeterAfter}");
                        continue;

                    case "Clash":
                        sb.AppendLine($"    CLASH  {e.ActorName} vs {e.TargetName}");
                        continue;

                    case "DevastatingStrike":
                        sb.AppendLine($"    DEVAST  {e.ActorName} → {e.TargetName}  ×3  ({e.DamageDealt} dmg)");
                        continue;

                    case "SkippedTurn":
                        sb.AppendLine($"    SKIP  {e.ActorName}  {e.Message}");
                        continue;

                    case "TurnEnd":
                        FlushDice();
                        sb.AppendLine($"    END  TM {e.TurnMeterBefore} → {e.TurnMeterAfter}");
                        inTurn = false;
                        totalAttacksInTurn = 0;
                        continue;
                }

                if (e.SoundDescription is not null)
                    sb.AppendLine($"    ♪ {e.SoundDescription}");

                continue;
            }

            // ── Inter-turn / standalone events ─────────────────────────────────
            switch (e.EventType)
            {
                case "RoundStart":
                    sb.AppendLine($"\n══ ROUND {e.RoundNumber} ═══════════════════════════════════════════════════════════════════════  tick={e.Tick}");
                    break;

                case "RoundEnd":
                    break;

                case "TurnStart":
                    FlushPendingMana();

                    // If a ManaDeduct for this actor was buffered, absorb into the turn block
                    if (pendingMana?.ActorName == e.ActorName)
                    {
                        sb.AppendLine($"\n══ TURN {++turnNo,3} ═══════════════════════════════════════════════════════════════════════  tick={e.Tick}");
                        var sp = e.IsSpell == true ? " (spell)" : "";
                        sb.AppendLine($"  {e.ActorName} → {e.TargetName}  [{e.AttackSourceName}{sp}]");
                        sb.AppendLine($"    MANA  {pendingMana.Message}");
                        pendingMana = null;
                    }
                    else
                    {
                        sb.AppendLine($"\n══ TURN {++turnNo,3} ═══════════════════════════════════════════════════════════════════════  tick={e.Tick}");
                        var sp = e.IsSpell == true ? " (spell)" : "";
                        sb.AppendLine($"  {e.ActorName} → {e.TargetName}  [{e.AttackSourceName}{sp}]");
                    }

                    inTurn = true;
                    turnActor = e.ActorName;
                    attackNo = 0;
                    pendingDice.Clear();
                    break;

                case "ManaDeduct":
                    pendingMana = e;
                    break;

                case "ApiCall":
                    FlushPendingMana();
                    sb.AppendLine($"  DICE  {e.Message}  tick={e.Tick}");
                    break;

                case "Death":
                case "KnockedOut":
                    FlushPendingMana();
                    sb.AppendLine($"  *** {e.EventType}  {e.Message}  tick={e.Tick}");
                    break;

                case "SkippedTurn":
                    FlushPendingMana();
                    sb.AppendLine($"  SKIP  {e.ActorName}  {e.Message}  tick={e.Tick}");
                    break;

                case "ManaRegen":
                    FlushPendingMana();
                    sb.AppendLine($"  MANA  {e.Message}  tick={e.Tick}");
                    break;

                case "InsufficientMana":
                    FlushPendingMana();
                    sb.AppendLine($"  NOMANA  {e.Message}  tick={e.Tick}");
                    break;

                case "Healed":
                    FlushPendingMana();
                    sb.AppendLine($"  HEAL  {e.ActorName}  +{e.DamageDealt} HP  ({e.TargetHpBefore} → {e.TargetHpAfter})  [{e.AttackSourceName}]  tick={e.Tick}");
                    break;

                case "LeechTick":
                    FlushPendingMana();
                        var lsym2 = e.LeechResourceType == LeechResources.Mana ? "♦" : "♥";
                    var lres2 = e.LeechResourceType == LeechResources.Mana ? "mana" : "HP";
                    sb.AppendLine($"  LEECH  {lsym2} {e.ActorName} -{e.LeechAmount} {lres2} → {e.LeechCasterName} +{e.LeechAmount}  [{e.StatusEffectName}]  tick={e.Tick}");
                    break;

                case "DoTTick":
                    FlushPendingMana();
                    sb.AppendLine($"  DOT  {e.ActorName}  -{e.DamageDealt} HP  [{e.StatusEffectName}]  tick={e.Tick}");
                    break;

                case "EffectApplied":
                    FlushPendingMana();
                    sb.AppendLine($"  EFFECT  [{e.StatusEffectName}] applied to {e.ActorName}  dur={e.EffectDuration?.ToString() ?? "?"}  tick={e.Tick}");
                    break;

                case "EffectResisted":
                    FlushPendingMana();
                    sb.AppendLine($"  RESIST  [{e.StatusEffectName}] by {e.ActorName}  ({e.ResistRoll} vs {e.ResistThreshold})  tick={e.Tick}");
                    break;

                case "EffectExpired":
                    FlushPendingMana();
                    sb.AppendLine($"  EXPIRED  [{e.StatusEffectName}] worn off {e.ActorName}  tick={e.Tick}");
                    break;

                case "SpellQueued":
                    FlushPendingMana();
                    sb.AppendLine($"  QUEUE  {e.Message}  tick={e.Tick}");
                    break;

                case "SpellCharging":
                    FlushPendingMana();
                    sb.AppendLine($"  CHARGE  {e.Message}  tick={e.Tick}");
                    break;

                case "SpellLost":
                    FlushPendingMana();
                    sb.AppendLine($"  LOST  {e.Message}  tick={e.Tick}");
                    break;

                case "ConcentrationPass":
                    FlushPendingMana();
                    sb.AppendLine($"  CONC  {e.Message}  tick={e.Tick}");
                    break;

                case "PetSummoned":
                    FlushPendingMana();
                    sb.AppendLine($"  SUMMON  {e.ActorName}  tick={e.Tick}");
                    break;

                case "PetExpired":
                    FlushPendingMana();
                    sb.AppendLine($"  DISMISS  {e.ActorName}  tick={e.Tick}");
                    break;

                default:
                    // Unknown event — show raw message if present
                    if (!string.IsNullOrEmpty(e.Message))
                        sb.AppendLine($"  {e.EventType}  {e.Message}  tick={e.Tick}");
                    break;
            }

            if (e.SoundDescription is not null && !inTurn)
                sb.AppendLine($"  ♪ {e.SoundDescription}");
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
        var turnStarts = result.Log.Where(e => e.EventType == "TurnStart").ToList();
        var effects    = result.Log.Where(e => e.EventType == "EffectApplied").ToList();
        var resisted   = result.Log.Where(e => e.EventType == "EffectResisted").ToList();
        var dots       = result.Log.Where(e => e.EventType == "DoTTick").ToList();
        var allActors  = turnStarts.Select(e => e.ActorName).Distinct().OrderBy(n => n).ToList();

        sb.AppendLine("  Per-character breakdown:");
        sb.AppendLine($"  {"Name",-14} {"Turns",6} {"Hits",5} {"Crits",6} {"Fmb",4} {"DmgDealt",9} {"Avg",6} {"DoT",5} {"FxApplied",10} {"FxResisted",10}");
        sb.AppendLine($"  {new string('-', 76)}");

        foreach (var name in allActors)
        {
            var myTurns = turnStarts.Count(e => e.ActorName == name);
            var myAtk   = attacks.Where(e => e.ActorName == name).ToList();
            var myHits  = myAtk.Count(e => e.IsHit == true);
            var myCrit  = myAtk.Count(e => e.IsCritical == true);
            var myFmb   = myAtk.Count(e => e.IsFumble == true);
            var myDmg   = result.Log
                .Where(e => e.EventType == "Damage" && e.ActorName != name)
                .Where(e => IsActorTurn(result.Log, e, name))
                .Sum(e => e.DamageDealt ?? 0);
            var myAvg   = myHits > 0 ? (double)myDmg / myHits : 0;
            var myDot   = dots.Count(e => e.ActorName == name);
            var myFx    = effects.Count(e => e.Message.Contains(name) || e.ActorName == name);
            var myRes   = resisted.Count(e => e.ActorName == name);

            sb.AppendLine($"  {name,-14} {myTurns,6} {myHits,5} {myCrit,6} {myFmb,4} {myDmg,9} {myAvg,6:F1} {myDot,5} {myFx,10} {myRes,10}");
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


