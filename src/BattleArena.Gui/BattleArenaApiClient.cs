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
}
