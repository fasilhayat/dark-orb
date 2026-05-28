using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Core.Entities.Enums;

namespace BattleArena;

/// <summary>
/// IDiceService implementation that routes every roll through the BattleArena REST API.
/// All dice calls are logged (dimmed console + file) by the underlying BattleArenaApiClient,
/// and also accumulated in DiceLog as ApiCall CombatLogEntry items for the combat log.
/// </summary>
internal sealed class ApiDiceService : IDiceService
{
    private readonly BattleArenaApiClient _api;

    public ApiDiceService(BattleArenaApiClient api) => _api = api;

    public int Seed => 0; // server generates the randomness

    /// <summary>All dice calls made during the simulation, in order.</summary>
    public List<CombatLogEntry> DiceLog { get; } = new();

    private void AddEntry(string endpoint, int result)
    {
        DiceLog.Add(new CombatLogEntry
        {
            Tick      = 0,
            EventType = "ApiCall",
            ActorName = "API",
            Message   = $"[{DateTime.Now:HH:mm:ss}]  GET {endpoint}  →  {result}"
        });
    }

    public int Roll(DieType dieType)
    {
        var result = _api.RollDieAsync(dieType).GetAwaiter().GetResult();
        AddEntry($"/v1/roll/{dieType}", result);
        return result;
    }

    public int Roll(int count, int sides)
    {
        var result = _api.RollDiceAsync(count, sides).GetAwaiter().GetResult();
        AddEntry($"/v1/roll/{count}d{sides}", result);
        return result;
    }

    public int RollWithAdvantage(DieType dieType)
    {
        var result = _api.RollWithAdvantageAsync(dieType).GetAwaiter().GetResult();
        AddEntry($"/v1/roll/advantage/{dieType}", result);
        return result;
    }

    public int RollWithDisadvantage(DieType dieType)
    {
        var result = _api.RollWithDisadvantageAsync(dieType).GetAwaiter().GetResult();
        AddEntry($"/v1/roll/disadvantage/{dieType}", result);
        return result;
    }

    public int RollIndex(int maxExclusive) => Roll(1, maxExclusive) - 1;
}
