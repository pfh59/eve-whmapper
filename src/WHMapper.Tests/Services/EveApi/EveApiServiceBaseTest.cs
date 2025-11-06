using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;
using Xunit;
using Moq;
using Moq.Protected;

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

    private Mock<HttpMessageHandler> CreateMockHttpHandler(HttpStatusCode statusCode, string? content = null, Dictionary<string, string>? headers = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(statusCode)
        {
            Content = content != null ? new StringContent(content) : null
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }
        }

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }

    #region Success Response Tests

    [Fact]
    public async Task Execute_WithOkStatusAndValidJson_ReturnsSuccessWithDeserializedData()
    {
        var testData = new { id = 1, name = "Test" };
        var json = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Exception);
    }

    [Fact]
    public async Task Execute_WithCreatedStatusAndValidJson_ReturnsSuccessWithDeserializedData()
    {
        var testData = new { id = 2, status = "created" };
        var json = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.Created, json);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Post, "https://api.test.com/test", new { name = "Test" });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Execute_WithAcceptedStatusAndValidJson_ReturnsSuccessWithDeserializedData()
    {
        var testData = new { status = "accepted" };
        var json = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.Accepted, json);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Put, "https://api.test.com/test/1", new { data = "updated" });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Execute_WithNoContentStatus_ReturnsSuccessWithDefaultValue()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.NoContent, null);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<string>(RequestSecurity.Public, RequestMethod.Delete, "https://api.test.com/test/1");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Execute_WithOkStatusAndEmptyContent_ReturnsSuccessWithDefaultValue()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, string.Empty);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<int>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.True(result.IsSuccess);
        Assert.Equal(default(int), result.Data);
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public async Task Execute_With429StatusAndRetryAfterHeader_ReturnsFailureWithRetryAfter()
    {
        var headers = new Dictionary<string, string> { { "Retry-After", "60" } };
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.TooManyRequests, null, headers);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Rate limit exceeded", result.ErrorMessage);
        Assert.Equal(TimeSpan.FromSeconds(60), result.RetryAfter);
    }

    [Fact]
    public async Task Execute_With429StatusAndoutRetryAfterHeader_ReturnsFailureWithDefaultRetryAfter()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.TooManyRequests, null);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Rate limit exceeded", result.ErrorMessage);
        Assert.Equal(TimeSpan.Zero, result.RetryAfter);
    }

    #endregion

    #region Error Response Tests

    [Fact]
    public async Task Execute_WithBadRequestStatus_ReturnsFailureWithErrorMessage()
    {
        var errorContent = "Invalid request parameters";
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.BadRequest, errorContent);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Post, "https://api.test.com/test", new { invalid = "data" });

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Invalid request parameters", result.ErrorMessage);
        Assert.Null(result.Exception);
    }

    [Fact]
    public async Task Execute_WithNotFoundStatus_ReturnsFailureWithErrorMessage()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.NotFound, "Resource not found");
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test/999");

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Resource not found", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WithInternalServerErrorStatus_ReturnsFailureWithErrorMessage()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.InternalServerError, "Internal server error");
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Internal server error", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WithErrorStatusAndEmptyContent_ReturnsFailureWithStatusMessage()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.ServiceUnavailable, string.Empty);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(503, result.StatusCode);
        Assert.Contains("Request failed with status", result.ErrorMessage);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Execute_WithAuthenticatedSecurityAndNoUserToken_ReturnsFailureWithUnauthorized()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, null);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient, userToken: null);
        var result = await service.Execute<dynamic>(RequestSecurity.Authenticated, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("UserToken is required", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WithAuthenticatedSecurityAndValidUserToken_IncludesAuthorizationHeader()
    {
        var userToken = new UserToken { AccessToken = "test-token-12345" };
        var testData = new { id = 1 };
        var json = JsonSerializer.Serialize(testData);
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient, userToken);
        var result = await service.Execute<dynamic>(RequestSecurity.Authenticated, RequestMethod.Get, "https://api.test.com/test");

        Assert.True(result.IsSuccess);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Authorization != null &&
                req.Headers.Authorization.Scheme == "Bearer" &&
                req.Headers.Authorization.Parameter == "test-token-12345"),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Deserialization Error Tests

    [Fact]
    public async Task Execute_WithOkStatusAndInvalidJson_ReturnsFailureWithJsonException()
    {
        var invalidJson = "{ invalid json }";
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, invalidJson);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Failed to deserialize response", result.ErrorMessage);
        Assert.NotNull(result.Exception);
        Assert.IsType<JsonException>(result.Exception);
    }

    #endregion

    #region HTTP Exception Tests

    [Fact]
    public async Task Execute_WithHttpRequestException_ReturnsFailureWithException()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };
        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Contains("HTTP request failed", result.ErrorMessage);
        Assert.NotNull(result.Exception);
        Assert.IsType<HttpRequestException>(result.Exception);
    }

    [Fact]
    public async Task Execute_WithTimeoutException_ReturnsFailureWithTimeoutMessage()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", new TimeoutException("Request timeout")));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };
        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Contains("Request timed out", result.ErrorMessage);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task Execute_WithTaskCanceledException_ReturnsFailureWithCancelledMessage()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request was cancelled"));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };
        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Contains("Request was cancelled", result.ErrorMessage);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task Execute_WithGeneralException_ReturnsFailureWithUnexpectedErrorMessage()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error occurred"));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };
        var service = new TestEveApiService(httpClient);
        var result = await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error occurred", result.ErrorMessage);
        Assert.NotNull(result.Exception);
        Assert.IsType<InvalidOperationException>(result.Exception);
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task Execute_WithGetMethod_CallsHttpGetAsync()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new { id = 1 }));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Get, "https://api.test.com/test");

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithPostMethod_CallsHttpPostAsync()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.Created, JsonSerializer.Serialize(new { id = 2 }));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Post, "https://api.test.com/test", new { name = "Test" });

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithPutMethod_CallsHttpPutAsync()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new { id = 1, name = "Updated" }));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Put, "https://api.test.com/test/1", new { name = "Updated" });

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithDeleteMethod_CallsHttpDeleteAsync()
    {
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.NoContent, null);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.test.com") };

        var service = new TestEveApiService(httpClient);
        await service.Execute<dynamic>(RequestSecurity.Public, RequestMethod.Delete, "https://api.test.com/test/1");

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}