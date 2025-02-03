using System;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using WHMapper.Services.EveOAuthProvider.Services;


namespace WHMapper.Tests.Services.OAuth;

public class EveUserInfosServicesTest
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthenticationStateProvider;
    private readonly EveUserInfosServices _eveUserInfosServices;

    public EveUserInfosServicesTest()
    {
        _mockAuthenticationStateProvider = new Mock<AuthenticationStateProvider>();
        _eveUserInfosServices = new EveUserInfosServices(_mockAuthenticationStateProvider.Object);
    }

    [Fact]
    public async Task GetUserName_UserAuthenticated_ReturnsUserName()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "TestUser")
        }, "mock"));
        var authState = new AuthenticationState(claimsPrincipal);
        _mockAuthenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _eveUserInfosServices.GetUserName();

        // Assert
        Assert.Equal("TestUser", result);
    }

    [Fact]
    public async Task GetUserName_UserNotAuthenticated_ReturnsAnonymousUsername()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(claimsPrincipal);
        _mockAuthenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _eveUserInfosServices.GetUserName();

        // Assert
        Assert.Equal(IEveUserInfosServices.ANONYMOUS_USERNAME, result);
    }

    [Fact]
    public async Task GetCharactedID_UserAuthenticated_ReturnsCharacterID()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345")
        }, "mock"));
        var authState = new AuthenticationState(claimsPrincipal);
        _mockAuthenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _eveUserInfosServices.GetCharactedID();

        // Assert
        Assert.Equal(12345, result);
    }

    [Fact]
    public async Task GetCharactedID_UserAuthenticated_NoCharacterID_ReturnsZero()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "")
        }, "mock"));
        var authState = new AuthenticationState(claimsPrincipal);
        _mockAuthenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _eveUserInfosServices.GetCharactedID();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCharactedID_UserNotAuthenticated_ReturnsZero()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(claimsPrincipal);
        _mockAuthenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _eveUserInfosServices.GetCharactedID();

        // Assert
        Assert.Equal(0, result);
    }
}
