using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using Moq;
using Moq.Protected;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;

namespace WHMapper.Tests.Services.EveAPI;

public class UniverseServicesTest
{
    [Fact]
    public async Task GetNames_PostsIdsAndReturnsNames()
    {
        HttpRequestMessage? request = null;
        string? body = null;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) =>
            {
                request = r;
                body = r.Content!.ReadAsStringAsync().Result;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""[{"id":1,"name":"Pilot","category":"character"},{"id":2,"name":"Corp","category":"corporation"}]""", Encoding.UTF8, "application/json")
            });
        var service = new UniverseServices(new HttpClient(handlerMock.Object) { BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl) });

        var result = await service.GetNames([1, 2]);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Pilot", "Corp"], result.Data!.Select(x => x.Name));
        Assert.Equal(HttpMethod.Post, request!.Method);
        Assert.Equal("/universe/names/", request.RequestUri!.AbsolutePath);
        Assert.Equal([1, 2], JsonSerializer.Deserialize<int[]>(body!));
    }

    [Fact]
    public async Task GetNames_WhenTooManyIds_FailsWithoutRequest()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var service = new UniverseServices(new HttpClient(handlerMock.Object) { BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl) });

        var result = await service.GetNames(Enumerable.Range(1, UniverseServices.MAX_NAMES_PER_REQUEST + 1).ToArray());

        Assert.False(result.IsSuccess);
    }
}
