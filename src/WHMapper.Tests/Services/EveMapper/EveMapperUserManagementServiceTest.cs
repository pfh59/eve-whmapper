using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider.Services;
using Xunit;

namespace WHMapper.Tests.Services.EveMapper;

public class EveMapperUserManagementServiceTest
{
    private readonly Mock<ILogger<EveMapperUserManagementService>> _mockLogger;
    private readonly Mock<IEveOnlineTokenProvider> _mockTokenProvider;
    private readonly Mock<ICharacterServices> _mockCharacterServices;
    private readonly EveMapperUserManagementService _service;

    public EveMapperUserManagementServiceTest()
    {
        _mockLogger = new Mock<ILogger<EveMapperUserManagementService>>();
        _mockTokenProvider = new Mock<IEveOnlineTokenProvider>();
        _mockCharacterServices = new Mock<ICharacterServices>();
        _service = new EveMapperUserManagementService(_mockLogger.Object, _mockTokenProvider.Object, _mockCharacterServices.Object);
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
}