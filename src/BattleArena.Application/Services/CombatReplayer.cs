namespace BattleArena.Application.Services;

using System.Text.Json;
using Application.Models;

// Replays a previously recorded combat from a .json snapshot file, producing
// an identical CombatResult when the same seed and character data are used.
//
// Usage:
//   var result = CombatReplayer.ReplayFromFile("combat-logs/Duel_WarriorVsWarrior_20260527_100517.json");
//   // result.Log is byte-for-byte identical to the original run
public static class CombatReplayer
{
    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling  = JsonCommentHandling.Skip
    };

    // ── Replay from a snapshot file path ──────────────────────────────────────

    public static CombatResult ReplayFromFile(string jsonPath)
    {
        var json     = File.ReadAllText(jsonPath);
        var snapshot = JsonSerializer.Deserialize<CombatSnapshot>(json, _json)
            ?? throw new InvalidOperationException($"Failed to deserialise snapshot: {jsonPath}");

        return Replay(snapshot);
    }

    // ── Replay from a snapshot object ────────────────────────────────────────

    public static CombatResult Replay(CombatSnapshot snapshot)
    {
        var (party1, party2) = snapshot.ToParties();
        var dice = new DiceService(snapshot.Seed);
        var sim  = BuildSimulator(dice);
        return sim.Simulate(party1, party2);
    }

    // ── JSON helpers ──────────────────────────────────────────────────────────

    public static string Serialize(CombatSnapshot snapshot) =>
        JsonSerializer.Serialize(snapshot, _json);

    public static CombatSnapshot Deserialize(string json) =>
        JsonSerializer.Deserialize<CombatSnapshot>(json, _json)
            ?? throw new InvalidOperationException("Failed to deserialise snapshot JSON");

    // ── Private ───────────────────────────────────────────────────────────────

    private static CombatSimulator BuildSimulator(DiceService dice) =>
        new(new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);
}
