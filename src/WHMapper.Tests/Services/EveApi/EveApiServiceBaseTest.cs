using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;
using Xunit;

namespace WHMapper.Tests.Services.EveAPI;

public class EveApiServiceBaseTest
{
    private class TestEveApiService : EveApiServiceBase
    {
        public TestEveApiService(HttpClient httpClient, UserToken? userToken = null)
            : base(httpClient, userToken)
        {
        }
    }

    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
        return mockHandler;
    }

    [Fact]
    public async Task HandleSuccessResponse_ShouldDeserializeValidJson_ReturnsSuccessResult()
    {
        // Arrange
        var testData = new TestResponse { Id = 123, Name = "Test Item" };
        var jsonContent = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, jsonContent);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(123, result.Data.Id);
        Assert.Equal("Test Item", result.Data.Name);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithEmptyContent_ReturnsSuccessWithDefault()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, string.Empty);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithInvalidJson_ReturnsFailureResult()
    {
        // Arrange
        var invalidJson = "{ invalid json content }";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to deserialize response", result.ErrorMessage);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithMismatchedJsonStructure_ReturnsFailureResult()
    {
        // Arrange
        var mismatchedJson = "{\"wrongField\": \"value\", \"anotherId\": 999}";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, mismatchedJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.Id);
        Assert.Equal(string.Empty, result.Data.Name);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithCreatedStatus_ReturnsSuccessResult()
    {
        // Arrange
        var testData = new TestResponse { Id = 456, Name = "Created Item" };
        var jsonContent = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.Created, jsonContent);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Post, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(456, result.Data.Id);
        Assert.Equal("Created Item", result.Data.Name);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithAcceptedStatus_ReturnsSuccessResult()
    {
        // Arrange
        var testData = new TestResponse { Id = 789, Name = "Accepted Item" };
        var jsonContent = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.Accepted, jsonContent);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Put, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(789, result.Data.Id);
        Assert.Equal("Accepted Item", result.Data.Name);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithComplexNestedObject_ReturnsSuccessResult()
    {
        // Arrange
        var complexJson = "{\"Id\": 999, \"Name\": \"Complex\", \"nested\": {\"value\": true}}";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, complexJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(999, result.Data.Id);
        Assert.Equal("Complex", result.Data.Name);
    }

    [Fact]
    public async Task HandleSuccessResponse_WithArrayResponse_ReturnsSuccessResult()
    {
        // Arrange
        var arrayJson = "[{\"Id\": 1, \"Name\": \"Item1\"}, {\"Id\": 2, \"Name\": \"Item2\"}]";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, arrayJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        var service = new TestEveApiService(httpClient);

        // Act
        var result = await service.Execute<TestResponse[]>(RequestSecurity.Public, RequestMethod.Get, "/test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Length);
        Assert.Equal(1, result.Data[0].Id);
        Assert.Equal("Item1", result.Data[0].Name);
    }
}