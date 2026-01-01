using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
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
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceProvider> _mockScopedServiceProvider;
    private readonly Mock<IWHMapRepository> _mockWHMapRepository;
    private readonly Mock<IEveMapperAccessHelper> _mockAccessHelper;
    private readonly EveMapperUserManagementService _service;

    public EveMapperUserManagementServiceTest()
    {
        _mockTokenProvider = new Mock<IEveOnlineTokenProvider>();
        _mockCharacterServices = new Mock<ICharacterServices>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<EveMapperUserManagementService>>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockScopedServiceProvider = new Mock<IServiceProvider>();
        _mockWHMapRepository = new Mock<IWHMapRepository>();
        _mockAccessHelper = new Mock<IEveMapperAccessHelper>();

        // Setup the scoped service provider to return the IWHMapRepository
        _mockScopedServiceProvider.Setup(sp => sp.GetService(typeof(IWHMapRepository))).Returns(_mockWHMapRepository.Object);
        _mockScopedServiceProvider.Setup(sp => sp.GetService(typeof(IEveMapperAccessHelper))).Returns(_mockAccessHelper.Object);

        // Setup the service scope to return the scoped service provider
        _mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockScopedServiceProvider.Object);
        _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(_mockServiceScopeFactory.Object);

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
 [Fact]
    public async Task GetPrimaryAccountAsync_ShouldReturnPrimaryAccount_WhenExists()
    {
        // Arrange
        var clientId = "test-client-id";
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
        var result = await _service.GetPrimaryAccountAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.Id);
        Assert.True(result.IsPrimary);
    }

    [Fact]
    public async Task GetPrimaryAccountAsync_ShouldReturnNull_WhenNoPrimaryAccount()
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

        // Act
        var result = await _service.GetPrimaryAccountAsync(clientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPrimaryAccountAsync_ShouldThrowArgumentNullException_WhenClientIdIsNull()
    {
        // Arrange
        string? clientId = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetPrimaryAccountAsync(clientId!));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task GetPrimaryAccountAsync_ShouldThrowArgumentNullException_WhenClientIdIsWhitespace()
    {
        // Arrange
        var clientId = " ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetPrimaryAccountAsync(clientId));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task UpdateAccountsMapAccessAsync_ShouldSetHasMapAccessCorrectly_WhenPrimaryHasAccess()
    {
        // Arrange
        var clientId = "test-client-id";
        var users = new[]
        {
            new WHMapperUser(123, "portrait1.jpg") { IsPrimary = true },
            new WHMapperUser(456, "portrait2.jpg") { IsPrimary = false }
        };

        var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
        whMapperUsers.TryAdd(clientId, users);

        typeof(EveMapperUserManagementService)
            .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_service, whMapperUsers);

        var maps = new[] { new WHMap ("map1"){ Id = 1 }, new WHMap("map2") { Id = 2 } };
        _mockWHMapRepository.Setup(r => r.GetAll()).ReturnsAsync(maps.AsEnumerable());
        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(123, 1)).ReturnsAsync(true);
        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(123, 2)).ReturnsAsync(false);
        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(456, 1)).ReturnsAsync(true);
        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(456, 2)).ReturnsAsync(false);

        // Act
        await _service.UpdateAccountsMapAccessAsync(clientId);

        // Assert
        Assert.True(users[0].HasMapAccess); // primary always true
        Assert.True(users[1].HasMapAccess); // secondary has access to at least one map
    }

    [Fact]
    public async Task UpdateAccountsMapAccessAsync_ShouldSetHasMapAccessFalse_WhenNoMaps()
    {
        // Arrange
        var clientId = "test-client-id";
        var users = new[]
        {
            new WHMapperUser(123, "portrait1.jpg") { IsPrimary = true },
            new WHMapperUser(456, "portrait2.jpg") { IsPrimary = false }
        };

        var whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
        whMapperUsers.TryAdd(clientId, users);

        typeof(EveMapperUserManagementService)
            .GetField("_whMapperUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_service, whMapperUsers);

        _mockWHMapRepository.Setup(r => r.GetAll()).ReturnsAsync(Array.Empty<WHMap>());

        // Act
        await _service.UpdateAccountsMapAccessAsync(clientId);

        // Assert
        Assert.True(users[0].HasMapAccess);
        Assert.False(users[1].HasMapAccess);
    }

    [Fact]
    public async Task UpdateAccountsMapAccessAsync_ShouldThrowArgumentNullException_WhenClientIdIsNull()
    {
        // Arrange
        string? clientId = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAccountsMapAccessAsync(clientId!));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task UpdateAccountsCurrentMapAccessAsync_ShouldSetHasCurrentMapAccessCorrectly()
    {
        // Arrange
        var clientId = "test-client-id";
        var mapId = 42;
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

        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(123, mapId)).ReturnsAsync(true);
        _mockAccessHelper.Setup(h => h.IsEveMapperMapAccessAuthorized(456, mapId)).ReturnsAsync(false);

        // Act
        await _service.UpdateAccountsCurrentMapAccessAsync(clientId, mapId);

        // Assert
        Assert.True(users[0].HasCurrentMapAccess);
        Assert.False(users[1].HasCurrentMapAccess);
    }

    [Fact]
    public async Task UpdateAccountsCurrentMapAccessAsync_ShouldThrowArgumentNullException_WhenClientIdIsNull()
    {
        // Arrange
        string? clientId = null;
        int mapId = 1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAccountsCurrentMapAccessAsync(clientId!, mapId));
        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public async Task UpdateAccountsCurrentMapAccessAsync_ShouldDoNothing_WhenNoAccounts()
    {
        // Arrange
        var clientId = "test-client-id";
        int mapId = 1;

        // No users for this clientId

        // Act
        var ex = await Record.ExceptionAsync(() => _service.UpdateAccountsCurrentMapAccessAsync(clientId, mapId));

        // Assert
        Assert.Null(ex); // Should not throw
    }
    



}