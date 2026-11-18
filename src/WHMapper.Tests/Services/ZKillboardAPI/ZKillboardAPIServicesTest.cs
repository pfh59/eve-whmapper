using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using WHMapper.Services.ZKillboardAPI;

namespace WHMapper.Tests.Services.ZKillboardAPI;

public class ZKillboardAPIServicesTest
{
    private const int SYSTEM_ID = 31000005;

    private const string KILLMAILS_JSON = """
        [
          {
            "killmail_id": 1, "killmail_time": "2026-11-14T05:10:34Z", "solar_system_id": 31000005,
            "victim": { "character_id": 10, "corporation_id": 98000001, "alliance_id": 99000001, "ship_type_id": 35833, "items": [ { "item_type_id": 1 } ] },
            "attackers": [ { "character_id": 20, "corporation_id": 98000002, "ship_type_id": 12005, "final_blow": true } ],
            "zkb": { "hash": "abc", "totalValue": "1500000000.5", "npc": false, "solo": true, "awox": false, "labels": [ "cat:65", "pvp" ] }
          },
          {
            "killmail_id": 2, "killmail_time": "2026-11-18T17:05:05Z", "solar_system_id": 31000005,
            "victim": { "corporation_id": 1000125, "ship_type_id": 670 },
            "attackers": [ { "corporation_id": 1000125, "final_blow": true } ],
            "zkb": { "hash": "def", "totalValue": 10000, "npc": true, "solo": false, "awox": false, "labels": [] }
          }
        ]
        """;

    private const string STATS_JSON = """
        {
          "type": "solarSystemID", "id": 31000005,
          "topLists": [
            { "type": "character", "title": "Top Characters", "values": [ { "kills": 38, "id": 97233059, "name": "Gideon Zendikar" } ] },
            { "type": "corporation", "title": "Top Corporations", "values": [ { "kills": 63, "id": 1831131676, "name": "V0LTA" } ] }
          ]
        }
        """;

    private static (ZKillboardAPIServices Service, List<HttpRequestMessage> Requests) CreateService(HttpStatusCode statusCode, string content)
    {
        var requests = new List<HttpRequestMessage>();
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => requests.Add(request))
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(ZKillboardAPIServiceConstants.ZKillboardUrl) };
        return (new ZKillboardAPIServices(client, NullLogger<ZKillboardAPIServices>.Instance), requests);
    }

    [Fact]
    public void Constructor_WhenBaseAddressIsNotZKillboard_Throws()
    {
        var client = new HttpClient { BaseAddress = new Uri("https://example.com") };

        Assert.Throws<ArgumentException>(() => new ZKillboardAPIServices(client, NullLogger<ZKillboardAPIServices>.Instance));
    }

    [Fact]
    public async Task GetSystemKillmailsAsync_ReturnsKillmailsNewestFirst()
    {
        var (service, requests) = CreateService(HttpStatusCode.OK, KILLMAILS_JSON);

        var killmails = await service.GetSystemKillmailsAsync(SYSTEM_ID);

        Assert.NotNull(killmails);
        Assert.Equal([2L, 1L], killmails!.Select(x => x.KillmailId));
        Assert.Equal("/api/solarSystemID/31000005/", requests.Single().RequestUri!.AbsolutePath);

        var structureKill = killmails[1];
        Assert.Equal(98000001, structureKill.Victim.CorporationId);
        Assert.Equal(99000001, structureKill.Victim.AllianceId);
        Assert.Equal(35833, structureKill.Victim.ShipTypeId);
        Assert.Equal(1500000000.5, structureKill.Zkb.TotalValue);
        Assert.True(structureKill.Zkb.Solo);
        Assert.Equal(65, structureKill.Zkb.VictimCategoryId);
        Assert.True(structureKill.Attackers.Single().FinalBlow);
        Assert.Null(killmails[0].Zkb.VictimCategoryId);
    }

    [Fact]
    public async Task GetSystemKillmailsAsync_WhenBodyIsEmpty_ReturnsEmptyList()
    {
        var (service, _) = CreateService(HttpStatusCode.OK, "");

        var killmails = await service.GetSystemKillmailsAsync(SYSTEM_ID);

        Assert.NotNull(killmails);
        Assert.Empty(killmails!);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "")]
    [InlineData(HttpStatusCode.InternalServerError, "")]
    [InlineData(HttpStatusCode.OK, """{ "error": "Invalid request" }""")]
    [InlineData(HttpStatusCode.OK, "<html>maintenance</html>")]
    public async Task GetSystemKillmailsAsync_WhenZKillboardFails_ReturnsNull(HttpStatusCode statusCode, string content)
    {
        var (service, _) = CreateService(statusCode, content);

        Assert.Null(await service.GetSystemKillmailsAsync(SYSTEM_ID));
    }

    [Fact]
    public async Task GetSystemStatsAsync_ReturnsTopLists()
    {
        var (service, requests) = CreateService(HttpStatusCode.OK, STATS_JSON);

        var stats = await service.GetSystemStatsAsync(SYSTEM_ID);

        Assert.NotNull(stats);
        Assert.Equal("/api/stats/solarSystemID/31000005/kills/", requests.Single().RequestUri!.AbsolutePath);
        var characters = stats!.TopLists.Single(x => x.Type == "character");
        Assert.Equal(97233059, characters.Values.Single().Id);
        Assert.Equal("Gideon Zendikar", characters.Values.Single().Name);
        Assert.Equal(38, characters.Values.Single().Kills);
    }

    [Fact]
    public async Task GetSystemStatsAsync_WhenSystemHasNoStatistics_ReturnsEmptyStats()
    {
        var (service, _) = CreateService(HttpStatusCode.OK, "[]");

        var stats = await service.GetSystemStatsAsync(SYSTEM_ID);

        Assert.NotNull(stats);
        Assert.Empty(stats!.TopLists);
    }

    [Fact]
    public async Task GetSystemStatsAsync_WhenRateLimited_ReturnsNull()
    {
        var (service, _) = CreateService(HttpStatusCode.TooManyRequests, "");

        Assert.Null(await service.GetSystemStatsAsync(SYSTEM_ID));
    }
}
