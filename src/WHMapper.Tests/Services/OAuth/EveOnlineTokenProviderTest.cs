using System;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;
using Xunit;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
using Moq.Protected;

namespace WHMapper.Tests.Services.OAuth;


public class EveOnlineTokenProviderTest
{
    private readonly Mock<IOptionsMonitor<EVEOnlineAuthenticationOptions>> _mockOptionsMonitor;
    private readonly EVEOnlineAuthenticationOptions _options;
    private readonly EveOnlineTokenProvider _tokenProvider;

    public EveOnlineTokenProviderTest()
    {
        _mockOptionsMonitor = new Mock<IOptionsMonitor<EVEOnlineAuthenticationOptions>>();
        _options = new EVEOnlineAuthenticationOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            TokenEndpoint = "https://test.token.endpoint",
            Backchannel = new HttpClient()
        };
        _mockOptionsMonitor.Setup(o => o.Get(It.IsAny<string>())).Returns(_options);
        _tokenProvider = new EveOnlineTokenProvider(_mockOptionsMonitor.Object);
    }

    [Fact]
    public async Task SaveToken_ValidToken_SavesSuccessfully()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", AccessToken = "test-access-token" };

        // Act
        await _tokenProvider.SaveToken(token);

        // Assert
        var savedToken = await _tokenProvider.GetToken("test-account-id");
        Assert.Equal(token, savedToken);
    }

    [Fact]
    public async Task SaveToken_NullAccountId_ThrowsArgumentNullException()
    {
        // Arrange
        var token = new UserToken { AccountId = null, AccessToken = "test-access-token" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenProvider.SaveToken(token));
    }

    [Fact]
    public async Task GetToken_ExistingToken_ReturnsToken()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", AccessToken = "test-access-token" };
        await _tokenProvider.SaveToken(token);

        // Act
        var retrievedToken = await _tokenProvider.GetToken("test-account-id");

        // Assert
        Assert.Equal(token, retrievedToken);
    }

    [Fact]
    public async Task GetToken_NonExistingToken_ReturnsNull()
    {
        // Act
        var retrievedToken = await _tokenProvider.GetToken("non-existing-account-id");

        // Assert
        Assert.Null(retrievedToken);
    }

    [Fact]
    public async Task ClearToken_ExistingToken_ClearsSuccessfully()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", AccessToken = "test-access-token" };
        await _tokenProvider.SaveToken(token);

        // Act
        await _tokenProvider.ClearToken("test-account-id");

        // Assert
        var retrievedToken = await _tokenProvider.GetToken("test-account-id");
        Assert.Null(retrievedToken);
    }

    [Fact]
    public async Task IsTokenExpire_ExpiredToken_ReturnsTrue()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", Expiry = DateTime.UtcNow.AddMinutes(-20) };
        await _tokenProvider.SaveToken(token);

        // Act
        var isExpired = await _tokenProvider.IsTokenExpire("test-account-id");

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public async Task IsTokenExpire_NotExpiredToken_ReturnsFalse()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", Expiry = DateTime.UtcNow.AddMinutes(20) };
        await _tokenProvider.SaveToken(token);

        // Act
        var isExpired = await _tokenProvider.IsTokenExpire("test-account-id");

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public async Task RefreshAccessToken_ValidToken_RefreshesSuccessfully()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", RefreshToken = "test-refresh-token" };
        await _tokenProvider.SaveToken(token);

        var newToken = new EveToken("new-access-token","BEARER", 3600,"new-refresh-token");
        
    
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(newToken), Encoding.UTF8, "application/json")
            });

        _options.Backchannel = new HttpClient(httpMessageHandler.Object);

        // Act
        await _tokenProvider.RefreshAccessToken("test-account-id");

        // Assert
        var refreshedToken = await _tokenProvider.GetToken("test-account-id");
        Assert.Equal(newToken.AccessToken, refreshedToken.AccessToken);
        Assert.Equal(newToken.RefreshToken, refreshedToken.RefreshToken);
    }

    [Fact]
    public async Task RefreshAccessToken_NonExistingToken_ThrowsNullReferenceException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _tokenProvider.RefreshAccessToken("non-existing-account-id"));
    }

    [Fact]
    public async Task RevokeToken_ValidToken_RevokesSuccessfully()
    {
        // Arrange
        var token = new UserToken { AccountId = "test-account-id", RefreshToken = "test-refresh-token" };
        await _tokenProvider.SaveToken(token);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        _options.Backchannel = new HttpClient(httpMessageHandler.Object);

        // Act
        await _tokenProvider.RevokeToken("test-account-id");

        // Assert
        var revokedToken = await _tokenProvider.GetToken("test-account-id");
        Assert.Null(revokedToken);
    }

    [Fact]
    public async Task RevokeToken_NonExistingToken_ThrowsNullReferenceException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _tokenProvider.RevokeToken("non-existing-account-id"));
    }
}
