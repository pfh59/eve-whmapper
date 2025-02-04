using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Alliances;
using WHMapper.Services.EveAPI.Assets;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveAPI.Corporations;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Locations;
using WHMapper.Services.EveAPI.Routes;
using WHMapper.Services.EveAPI.Search;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveAPI.UserInterface;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Tests.Services.EveApi;


public class EveAPIServicesTest
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientIsNull()
    {
        // Arrange
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EveAPIServices(null!, userServiceMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenUserServiceIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EveAPIServices(httpClient, null!));
    }

    [Fact]
    public void Constructor_ShouldInitializeServices_WhenDependenciesAreValid()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.NotNull(eveAPIServices.LocationServices);
        Assert.NotNull(eveAPIServices.UniverseServices);
        Assert.NotNull(eveAPIServices.UserInterfaceServices);
        Assert.NotNull(eveAPIServices.AllianceServices);
        Assert.NotNull(eveAPIServices.CorporationServices);
        Assert.NotNull(eveAPIServices.CharacterServices);
        Assert.NotNull(eveAPIServices.SearchServices);
        Assert.NotNull(eveAPIServices.DogmaServices);
        Assert.NotNull(eveAPIServices.RouteServices);
        Assert.NotNull(eveAPIServices.AssetsServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeLocationServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<LocationServices>(eveAPIServices.LocationServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeUniverseServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<UniverseServices>(eveAPIServices.UniverseServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeUserInterfaceServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<UserInterfaceServices>(eveAPIServices.UserInterfaceServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeAllianceServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<AllianceServices>(eveAPIServices.AllianceServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorporationServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<CorporationServices>(eveAPIServices.CorporationServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeCharacterServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<CharacterServices>(eveAPIServices.CharacterServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeSearchServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<SearchServices>(eveAPIServices.SearchServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeDogmaServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<DogmaServices>(eveAPIServices.DogmaServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeRouteServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<RouteServices>(eveAPIServices.RouteServices);
    }

    [Fact]
    public void Constructor_ShouldInitializeAssetsServicesCorrectly()
    {
        // Arrange
        var httpClient = new HttpClient();
        var userServiceMock = new Mock<IEveUserInfosServices>();

        // Act
        var eveAPIServices = new EveAPIServices(httpClient, userServiceMock.Object);

        // Assert
        Assert.IsType<AssetsServices>(eveAPIServices.AssetsServices);
    }
}

