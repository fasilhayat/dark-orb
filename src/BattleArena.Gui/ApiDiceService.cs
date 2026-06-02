using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Gui;

internal sealed class ApiDiceService : IDiceService
{
    private readonly BattleArenaApiClient _api;

    public ApiDiceService(BattleArenaApiClient api) => _api = api;

    public int Seed => 0;

    public int CurrentTick { get; set; }

    public List<CombatLogEntry> DiceLog { get; } = new();

    private void AddEntry(string endpoint, int result)
    {
        DiceLog.Add(new CombatLogEntry
        {
            Tick = CurrentTick,
            EventType = "ApiCall",
            ActorName = "API",
            Message = $"GET {endpoint} → {result}"
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
