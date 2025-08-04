using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WHMapper.Models.DTO.EveScout;
using WHMapper.Services.EveScoutAPI;
using Xunit;

namespace WHMapper.Tests.Services.EveScoutAPI;

public class EveScoutAPIServicesTest
{


    private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri(EveScoutAPIServiceConstants.EveScoutUrl)
        };
        return client;
    }

    [Fact]
    public async Task GetTurnurSystemsAsync_ReturnsDeserializedObjects()
    {
        var entries = new List<EveScoutSystemEntry>
        {
            new EveScoutSystemEntry { Id = 2, OutSystemName = "Turnur" }
        };
        string json = JsonSerializer.Serialize(entries);

        var client = CreateMockHttpClient(HttpStatusCode.OK, json);
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTurnurSystemsAsync();

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Turnur", result!.First().OutSystemName);
    }

    [Fact]
    public async Task GetTurnurSystemsAsync_ReturnsNullOnNoContent()
    {
        var client = CreateMockHttpClient(HttpStatusCode.NoContent, "");
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTurnurSystemsAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTurnurSystemsAsync_ReturnsNullOnEmptyContent()
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, "");
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTurnurSystemsAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTurnurSystemsAsync_ReturnsNullOnNonSuccessStatus()
    {
        var client = CreateMockHttpClient(HttpStatusCode.BadRequest, "");
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTurnurSystemsAsync();
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_ThrowsOnWrongBaseAddress()
    {
        var client = new HttpClient { BaseAddress = new System.Uri("https://wrong-url.com/") };
        Assert.Throws<ArgumentException>(() => new EveScoutAPIServices(client));
    }

    [Fact]

    public async Task GetTheraSystemsAsync_Integration_ReturnsData()
    {
        var client = new HttpClient
        {
            BaseAddress = new System.Uri(EveScoutAPIServiceConstants.EveScoutUrl)
        };
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTheraSystemsAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result!);
        Assert.All(result!, entry => Assert.NotNull(entry.OutSystemName));
    }

    [Fact]
    public async Task GetTurnurSystemsAsync_Integration_ReturnsData()
    {
        var client = new HttpClient
        {
            BaseAddress = new System.Uri(EveScoutAPIServiceConstants.EveScoutUrl)
        };
        var service = new EveScoutAPIServices(client);

        var result = await service.GetTurnurSystemsAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result!);
        Assert.All(result!, entry => Assert.NotNull(entry.OutSystemName));
    }
}