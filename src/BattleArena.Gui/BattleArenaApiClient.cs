using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleArena.Application.Models;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Gui;

internal sealed class BattleArenaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public BattleArenaApiClient(string baseUrl, string? apiKey = null)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout = TimeSpan.FromSeconds(10)
        };
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Character>>("/v1/characters", JsonOptions);
        return result ?? [];
    }

    public async Task<List<Race>> GetRacesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Race>>("/v1/races", JsonOptions);
        return result ?? [];
    }

    public async Task<List<Subrace>> GetSubracesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Subrace>>("/v1/subraces", JsonOptions);
        return result ?? [];
    }

    public async Task<int> RollDieAsync(DieType dieType)
    {
        var dto = await _http.GetFromJsonAsync<DieRollResponse>($"/v1/roll/{dieType}", JsonOptions);
        return dto?.Result ?? 0;
    }

    public async Task<int> RollDiceAsync(int count, int sides)
    {
        var dto = await _http.GetFromJsonAsync<DiceRollResponse>($"/v1/roll/{count}d{sides}", JsonOptions);
        return dto?.Result ?? 0;
    }

    public async Task<int> RollWithAdvantageAsync(DieType dieType)
    {
        var dto = await _http.GetFromJsonAsync<DieRollResponse>($"/v1/roll/advantage/{dieType}", JsonOptions);
        return dto?.Result ?? 0;
    }

    public async Task<int> RollWithDisadvantageAsync(DieType dieType)
    {
        var dto = await _http.GetFromJsonAsync<DieRollResponse>($"/v1/roll/disadvantage/{dieType}", JsonOptions);
        return dto?.Result ?? 0;
    }

    internal sealed record DieRollResponse(string Die, int Result);
    internal sealed record DiceRollResponse(string Dice, int Result);

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var result = await _http.GetAsync("/api/healthcheck", cts.Token);
            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CombatResult> SimulateCombatAsync(
        string heroPartyName, List<int> heroMemberIds,
        string enemyPartyName, List<int> enemyMemberIds,
        int maxTicks = 500,
        string heroTargetStrategy = "lowestHp",
        string enemyTargetStrategy = "lowestHp")
    {
        var req = new CombatSimulateByMembersRequest(
            heroPartyName, heroMemberIds,
            enemyPartyName, enemyMemberIds,
            maxTicks, heroTargetStrategy, enemyTargetStrategy);
        var resp = await _http.PostAsJsonAsync("/v1/combat/simulate", req, JsonOptions);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CombatResult>(JsonOptions) ?? new CombatResult();
    }
}

public record CombatSimulateByMembersRequest(
    string HeroPartyName,
    List<int> HeroPartyMemberIds,
    string EnemyPartyName,
    List<int> EnemyPartyMemberIds,
    int MaxTicks = 500,
    string HeroTargetStrategy = "lowestHp",
    string EnemyTargetStrategy = "lowestHp"
);
