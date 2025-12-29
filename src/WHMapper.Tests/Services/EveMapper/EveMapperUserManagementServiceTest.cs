using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider.Services;
using Xunit;

namespace WHMapper.Tests.Services.EveMapper;

public class EveMapperUserManagementServiceTest
{
    private readonly Mock<IEveOnlineTokenProvider> _mockTokenProvider;
    private readonly Mock<ICharacterServices> _mockCharacterServices;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<EveMapperUserManagementService>> _mockLogger;
    private readonly EveMapperUserManagementService _service;

    public EveMapperUserManagementServiceTest()
    {
        _mockTokenProvider = new Mock<IEveOnlineTokenProvider>();
        _mockCharacterServices = new Mock<ICharacterServices>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<EveMapperUserManagementService>>();
        _service = new EveMapperUserManagementService(_mockTokenProvider.Object, _mockCharacterServices.Object, _mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldReturnUsers_WhenClientIdExists()
    {
        // Arrange
        var clientId = "test-client-id";
        var users = new[]
        {
            new WHMapperUser(1, "portrait1.jpg"),
            new WHMapperUser(2, "portrait2.jpg")
        };
        var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
        whMapperUsers.TryAdd(clientId, users);

        typeof(EveMapperUserManagementService)
            .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_service, whMapperUsers);

        // Act
        var result = await _service.GetAccountsAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Contains(result, user => user.Id == 1);
        Assert.Contains(result, user => user.Id == 2);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldReturnEmptyArray_WhenClientIdDoesNotExist()
    {
        // Arrange
        var clientId = "non-existent-client-id";

        // Act
        var result = await _service.GetAccountsAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldThrowArgumentNullException_WhenClientIdIsNull()
    {
        // Arrange
        string? clientId = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetAccountsAsync(clientId!));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldThrowArgumentNullException_WhenClientIdIsEmpty()
    {
        // Arrange
        var clientId = string.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetAccountsAsync(clientId));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldThrowArgumentNullException_WhenClientIdIsWhitespace()
    {
        // Arrange
        var clientId = " ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetAccountsAsync(clientId));
        Assert.Equal("clientId", exception.ParamName);
    }

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldAddNewUser_WhenUserDoesNotExist()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "123";
    var token = new UserToken { AccessToken = "test-token" };
    var portrait = new Portrait { Picture64x64 = "portrait.jpg" };

    _mockCharacterServices
        .Setup(cs => cs.GetCharacterPortrait(It.IsAny<int>()))
        .ReturnsAsync(portrait);

    _mockTokenProvider
        .Setup(tp => tp.SaveToken(It.IsAny<UserToken>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.AddAuthenticateWHMapperUser(clientId, accountId, token);

    // Assert
    var users = await _service.GetAccountsAsync(clientId);
    Assert.Single(users);
    Assert.Equal(123, users[0].Id);
    Assert.Equal("portrait.jpg", users[0].PortraitUrl);
    _mockTokenProvider.Verify(tp => tp.SaveToken(token), Times.Once);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldUpdateToken_WhenUserAlreadyExists()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "123";
    var token = new UserToken { AccessToken = "test-token" };
    var existingUser = new WHMapperUser(123, "existing-portrait.jpg");

    var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    whMapperUsers.TryAdd(clientId, new[] { existingUser });

    typeof(EveMapperUserManagementService)
        .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_service, whMapperUsers);

    _mockTokenProvider
        .Setup(tp => tp.SaveToken(It.IsAny<UserToken>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.AddAuthenticateWHMapperUser(clientId, accountId, token);

    // Assert
    var users = await _service.GetAccountsAsync(clientId);
    Assert.Single(users);
    Assert.Equal(123, users[0].Id);
    Assert.Equal("existing-portrait.jpg", users[0].PortraitUrl);
    _mockTokenProvider.Verify(tp => tp.SaveToken(token), Times.Once);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientIdIsNull()
{
    // Arrange
    string? clientId = null;
    var accountId = "123";
    var token = new UserToken { AccessToken = "test-token" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAuthenticateWHMapperUser(clientId!, accountId, token));
    Assert.Equal("clientId", exception.ParamName);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientIdIsEmpty()
{
    // Arrange
    var clientId = string.Empty;
    var accountId = "123";
    var token = new UserToken { AccessToken = "test-token" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAuthenticateWHMapperUser(clientId, accountId, token));
    Assert.Equal("clientId", exception.ParamName);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenAccountIdIsEmpty()
{
    // Arrange
    var clientId = "test-client-id";
    string? accountId = string.Empty;
    var token = new UserToken { AccessToken = "test-token" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAuthenticateWHMapperUser(clientId, accountId, token));
    Assert.Equal("accountId", exception.ParamName);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenAccountIdIsWhitespace()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = " ";
    var token = new UserToken { AccessToken = "test-token" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddAuthenticateWHMapperUser(clientId, accountId, token));
    Assert.Equal("accountId", exception.ParamName);
}

[Fact]
public async Task AddAuthenticateWHMapperUser_ShouldThrowArgumentException_WhenAccountIdIsNotInteger()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "abc";
    var token = new UserToken { AccessToken = "test-token" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAuthenticateWHMapperUser(clientId, accountId, token));
    Assert.Equal("accountId", exception.ParamName);
}


[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldRemoveUser_WhenUserExists()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "123";
    var existingUser = new WHMapperUser(123, "portrait.jpg");

    var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    whMapperUsers.TryAdd(clientId, new[] { existingUser });

    typeof(EveMapperUserManagementService)
        .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_service, whMapperUsers);

    _mockTokenProvider
        .Setup(tp => tp.ClearToken(It.IsAny<string>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.RemoveAuthenticateWHMapperUser(clientId, accountId);

    // Assert
    var users = await _service.GetAccountsAsync(clientId);
    Assert.Empty(users);
    _mockTokenProvider.Verify(tp => tp.ClearToken(accountId), Times.Once);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldDoNothing_WhenUserDoesNotExist()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "123";

    _mockTokenProvider
        .Setup(tp => tp.ClearToken(It.IsAny<string>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.RemoveAuthenticateWHMapperUser(clientId, accountId);

    // Assert
    var users = await _service.GetAccountsAsync(clientId);
    Assert.Empty(users);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldRemoveAllUsers_WhenClientIdExists()
{
    // Arrange
    var clientId = "test-client-id";
    var users = new[]
    {
        new WHMapperUser(123, "portrait1.jpg"),
        new WHMapperUser(456, "portrait2.jpg")
    };

    var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    whMapperUsers.TryAdd(clientId, users);

    typeof(EveMapperUserManagementService)
        .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_service, whMapperUsers);

    _mockTokenProvider
        .Setup(tp => tp.ClearToken(It.IsAny<string>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.RemoveAuthenticateWHMapperUser(clientId);

    // Assert
    var result = await _service.GetAccountsAsync(clientId);
    Assert.Empty(result);
    _mockTokenProvider.Verify(tp => tp.ClearToken("123"), Times.Once);
    _mockTokenProvider.Verify(tp => tp.ClearToken("456"), Times.Once);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientIdIsNull()
{
    // Arrange
    string? clientId = null;

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RemoveAuthenticateWHMapperUser(clientId!));
    Assert.Equal("clientId", exception.ParamName);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientIdIsEmpty()
{
    // Arrange
    var clientId = string.Empty;

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RemoveAuthenticateWHMapperUser(clientId));
    Assert.Equal("clientId", exception.ParamName);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientAccountIdIsEmpty()
{
    // Arrange
    var clientId = "test-client-id";
    string? accountId = null;

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RemoveAuthenticateWHMapperUser(clientId, accountId!));
    Assert.Equal("accountId", exception.ParamName);

}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldThrowArgumentNullException_WhenClientAccountIdIsWhitespace()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = " ";

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RemoveAuthenticateWHMapperUser(clientId, accountId));
    Assert.Equal("accountId", exception.ParamName);
}

[Fact]
public async Task RemoveAuthenticateWHMapperUser_ShouldThrowArgumentException_WhenClientAccountIdIsNotInteger()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "abc";

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveAuthenticateWHMapperUser(clientId, accountId));
    Assert.Equal("accountId", exception.ParamName);
}



[Fact]
public async Task SetPrimaryAccountAsync_ShouldSetPrimaryAccount_WhenAccountExists()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "123";
    var users = new[]
    {
        new WHMapperUser(123, "portrait1.jpg") { IsPrimary = false },
        new WHMapperUser(456, "portrait2.jpg") { IsPrimary = true }
    };

    var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    whMapperUsers.TryAdd(clientId, users);

    typeof(EveMapperUserManagementService)
        .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_service, whMapperUsers);

    // Act
    await _service.SetPrimaryAccountAsync(clientId, accountId);

    // Assert
    var result = await _service.GetAccountsAsync(clientId);
    Assert.True(result.First(user => user.Id == 123).IsPrimary);
    Assert.False(result.First(user => user.Id == 456).IsPrimary);
}

[Fact]
public async Task SetPrimaryAccountAsync_ShouldThrowException_WhenAccountDoesNotExist()
{
    // Arrange
    var clientId = "test-client-id";
    var accountId = "999";
    var users = new[]
    {
        new WHMapperUser(123, "portrait1.jpg"),
        new WHMapperUser(456, "portrait2.jpg")
    };

    var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    whMapperUsers.TryAdd(clientId, users);

    typeof(EveMapperUserManagementService)
        .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_service, whMapperUsers);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SetPrimaryAccountAsync(clientId, accountId));
    Assert.Equal("accountId", exception.ParamName);
}

    



}