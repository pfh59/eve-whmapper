using WHMapper.Models.DTO;
using Xunit;

namespace WHMapper.Tests.Models.DTO;

public class ClientUIDTest
{
    [Fact]
    public void ClientUID_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var clientUID = new ClientUID();

        // Assert
        Assert.NotNull(clientUID.ClientId);
        Assert.Equal(string.Empty, clientUID.ClientId);
    }

    [Fact]
    public void ClientUID_ShouldAllowSettingAndGettingClientId()
    {
        // Arrange
        var clientUID = new ClientUID();
        var testClientId = "TestClient123";

        // Act
        clientUID.ClientId = testClientId;

        // Assert
        Assert.Equal(testClientId, clientUID.ClientId);
    }

    [Fact]
    public void ClientUID_ShouldHandleNullClientId()
    {
        // Arrange
        var clientUID = new ClientUID();

        // Act
        clientUID.ClientId = null;

        // Assert
        Assert.Null(clientUID.ClientId);
    }

    [Fact]
    public void ClientUID_ShouldHandleEmptyClientId()
    {
        // Arrange
        var clientUID = new ClientUID();

        // Act
        clientUID.ClientId = string.Empty;

        // Assert
        Assert.Equal(string.Empty, clientUID.ClientId);
    }
}