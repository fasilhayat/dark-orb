using BattleArena.Core.Entities.Enums;
using BattleArena.Application.Interfaces;

namespace BattleArena;

/// <summary>
/// IDiceService implementation that routes every roll through the BattleArena REST API.
/// All dice calls are logged (dimmed console + file) by the underlying BattleArenaApiClient.
/// </summary>
internal sealed class ApiDiceService : IDiceService
{
    private readonly BattleArenaApiClient _api;

    public ApiDiceService(BattleArenaApiClient api)
    {
        _api = api;
    }

    public int Seed => 0; // seed lives on the server

    public int Roll(DieType dieType) =>
        _api.RollDieAsync(dieType).GetAwaiter().GetResult();

    public int Roll(int count, int sides) =>
        _api.RollDiceAsync(count, sides).GetAwaiter().GetResult();

    public int RollWithAdvantage(DieType dieType) =>
        _api.RollWithAdvantageAsync(dieType).GetAwaiter().GetResult();

    public int RollWithDisadvantage(DieType dieType) =>
        _api.RollWithDisadvantageAsync(dieType).GetAwaiter().GetResult();

    public int RollIndex(int maxExclusive) =>
        Roll(1, maxExclusive) - 1;
}
