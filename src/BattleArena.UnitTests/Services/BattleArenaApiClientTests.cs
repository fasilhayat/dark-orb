namespace BattleArena.UnitTests.Services;

using System.Net;
using System.Text;
using System.Text.Json;
using BattleArena.Application.Models;
using BattleArena.Gui;

public class BattleArenaApiClientTests
{
    [Fact]
    public async Task SimulateCombatAsync_Http400_ThrowsApiException()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("None of the HeroPartyMemberIds matched a known character.")
            };
        });
        var client = new BattleArenaApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        });

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            client.SimulateCombatAsync("hero", [1], "enemy", [0]));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("None of the HeroPartyMemberIds", ex.Body);
        Assert.Contains("400 (Bad Request)", ex.Message);
    }

    [Fact]
    public async Task SimulateCombatAsync_Http200_ReturnsResult()
    {
        var result = new CombatResult { CombatId = Guid.NewGuid() };
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var handler = new FakeHttpMessageHandler(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var client = new BattleArenaApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        });

        var actual = await client.SimulateCombatAsync("hero", [1], "enemy", [2]);

        Assert.Equal(result.CombatId, actual.CombatId);
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
        }
    }
}
