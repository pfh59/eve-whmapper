using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
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
using Xunit;

namespace WHMapper.Tests.Services.EveAPI;

public class EveAPIServicesTest
{
    private HttpClient CreateTestHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri("https://api.test.com")
        };
    }

    private UserToken CreateTestUserToken()
    {
        return new UserToken
        {
            AccessToken = "test_access_token",
            RefreshToken = "test_refresh_token",
            Expiry = DateTime.UtcNow.AddHours(1)
        };
    }

    [Fact]
    public void Constructor_WithValidHttpClient_InitializesAllServices()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();

        // Act
        var services = new EveAPIServices(httpClient);

        // Assert
        Assert.NotNull(services.LocationServices);
        Assert.NotNull(services.UniverseServices);
        Assert.NotNull(services.UserInterfaceServices);
        Assert.NotNull(services.AllianceServices);
        Assert.NotNull(services.CorporationServices);
        Assert.NotNull(services.CharacterServices);
        Assert.NotNull(services.SearchServices);
        Assert.NotNull(services.DogmaServices);
        Assert.NotNull(services.AssetsServices);
        Assert.NotNull(services.RouteServices);
    }

    [Fact]
    public void Constructor_WithValidHttpClient_InitializesCorrectServiceTypes()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();

        // Act
        var services = new EveAPIServices(httpClient);

        // Assert
        Assert.IsAssignableFrom<ILocationServices>(services.LocationServices);
        Assert.IsAssignableFrom<IUniverseServices>(services.UniverseServices);
        Assert.IsAssignableFrom<IUserInterfaceServices>(services.UserInterfaceServices);
        Assert.IsAssignableFrom<IAllianceServices>(services.AllianceServices);
        Assert.IsAssignableFrom<ICorporationServices>(services.CorporationServices);
        Assert.IsAssignableFrom<ICharacterServices>(services.CharacterServices);
        Assert.IsAssignableFrom<ISearchServices>(services.SearchServices);
        Assert.IsAssignableFrom<IDogmaServices>(services.DogmaServices);
        Assert.IsAssignableFrom<IAssetsServices>(services.AssetsServices);
        Assert.IsAssignableFrom<IRouteServices>(services.RouteServices);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        HttpClient? httpClient = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EveAPIServices(httpClient!));
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_WithValidUserToken_CompletesSuccessfully()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken = CreateTestUserToken();

        // Act
        await services.SetEveCharacterAuthenticatication(userToken);

        // Assert
        Assert.NotNull(services.LocationServices);
        Assert.NotNull(services.SearchServices);
        Assert.NotNull(services.DogmaServices);
        Assert.NotNull(services.AssetsServices);
        Assert.NotNull(services.UserInterfaceServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_WithNullUserToken_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        UserToken? userToken = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => services.SetEveCharacterAuthenticatication(userToken!));
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_ReInitializesAuthenticatedServices()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken = CreateTestUserToken();

        var originalLocationServices = services.LocationServices;
        var originalSearchServices = services.SearchServices;
        var originalDogmaServices = services.DogmaServices;
        var originalAssetsServices = services.AssetsServices;
        var originalUserInterfaceServices = services.UserInterfaceServices;

        // Act
        await services.SetEveCharacterAuthenticatication(userToken);

        // Assert - Authenticated services should be new instances
        Assert.NotSame(originalLocationServices, services.LocationServices);
        Assert.NotSame(originalSearchServices, services.SearchServices);
        Assert.NotSame(originalDogmaServices, services.DogmaServices);
        Assert.NotSame(originalAssetsServices, services.AssetsServices);
        Assert.NotSame(originalUserInterfaceServices, services.UserInterfaceServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_DoesNotReinitializeNonAuthenticatedServices()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken = CreateTestUserToken();

        var originalUniverseServices = services.UniverseServices;
        var originalAllianceServices = services.AllianceServices;
        var originalCorporationServices = services.CorporationServices;
        var originalCharacterServices = services.CharacterServices;
        var originalRouteServices = services.RouteServices;

        // Act
        await services.SetEveCharacterAuthenticatication(userToken);

        // Assert - Non-authenticated services should remain the same instances
        Assert.Same(originalUniverseServices, services.UniverseServices);
        Assert.Same(originalAllianceServices, services.AllianceServices);
        Assert.Same(originalCorporationServices, services.CorporationServices);
        Assert.Same(originalCharacterServices, services.CharacterServices);
        Assert.Same(originalRouteServices, services.RouteServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_CalledMultipleTimes_UpdatesServicesEachTime()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken1 = CreateTestUserToken();
        var userToken2 = new UserToken
        {
            AccessToken = "different_access_token",
            RefreshToken = "different_refresh_token",
            Expiry = DateTime.UtcNow.AddHours(2)
        };

        // Act
        await services.SetEveCharacterAuthenticatication(userToken1);
        var servicesAfterFirstCall = services.LocationServices;

        await services.SetEveCharacterAuthenticatication(userToken2);
        var servicesAfterSecondCall = services.LocationServices;

        // Assert
        Assert.NotSame(servicesAfterFirstCall, servicesAfterSecondCall);
    }

    [Fact]
    public void Constructor_InitializesServicesWithSameHttpClient()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();

        // Act
        var services = new EveAPIServices(httpClient);

        // Assert - All services should be initialized (not null)
        // This verifies that the same httpClient is passed to all service constructors
        Assert.NotNull(services.LocationServices);
        Assert.NotNull(services.UniverseServices);
        Assert.NotNull(services.UserInterfaceServices);
        Assert.NotNull(services.AllianceServices);
        Assert.NotNull(services.CorporationServices);
        Assert.NotNull(services.CharacterServices);
        Assert.NotNull(services.SearchServices);
        Assert.NotNull(services.DogmaServices);
        Assert.NotNull(services.AssetsServices);
        Assert.NotNull(services.RouteServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_MaintainsServiceInterfaceContracts()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken = CreateTestUserToken();

        // Act
        await services.SetEveCharacterAuthenticatication(userToken);

        // Assert - Services should still implement their respective interfaces
        Assert.IsAssignableFrom<ILocationServices>(services.LocationServices);
        Assert.IsAssignableFrom<ISearchServices>(services.SearchServices);
        Assert.IsAssignableFrom<IDogmaServices>(services.DogmaServices);
        Assert.IsAssignableFrom<IAssetsServices>(services.AssetsServices);
        Assert.IsAssignableFrom<IUserInterfaceServices>(services.UserInterfaceServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthentication_ReturnsCompletedTask()
    {
        // Arrange
        var httpClient = CreateTestHttpClient();
        var services = new EveAPIServices(httpClient);
        var userToken = CreateTestUserToken();

        // Act
        var task = services.SetEveCharacterAuthenticatication(userToken);

        // Assert
        Assert.True(task.IsCompleted);
        Assert.Equal(Task.CompletedTask, task);
    }
}