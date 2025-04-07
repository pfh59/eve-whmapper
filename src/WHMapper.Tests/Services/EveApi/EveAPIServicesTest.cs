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
    public async Task SetEveCharacterAuthenticatication_ShouldInitializeServicesWithUserToken()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var userToken = new UserToken { AccessToken = "test-token", RefreshToken = "refresh-token" };
        var eveApiServices = new EveAPIServices(mockHttpClient.Object);

        // Act
        await eveApiServices.SetEveCharacterAuthenticatication(userToken);

        // Assert
        Assert.NotNull(eveApiServices.LocationServices);
        Assert.NotNull(eveApiServices.SearchServices);
        Assert.NotNull(eveApiServices.DogmaServices);
        Assert.NotNull(eveApiServices.AssetsServices);
        Assert.NotNull(eveApiServices.UserInterfaceServices);

        // Verify that the services are initialized with the correct HttpClient and UserToken
        Assert.IsType<LocationServices>(eveApiServices.LocationServices);
        Assert.IsType<SearchServices>(eveApiServices.SearchServices);
        Assert.IsType<DogmaServices>(eveApiServices.DogmaServices);
        Assert.IsType<AssetsServices>(eveApiServices.AssetsServices);
        Assert.IsType<UserInterfaceServices>(eveApiServices.UserInterfaceServices);
    }

    [Fact]
    public async Task SetEveCharacterAuthenticatication_ShouldThrowArgumentNullException_WhenUserTokenIsNull()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var eveApiServices = new EveAPIServices(mockHttpClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => eveApiServices.SetEveCharacterAuthenticatication(null));
    }

    [Fact]
    public async Task SetEveCharacterAuthenticatication_ShouldNotAffectOtherServices()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var userToken = new UserToken { AccessToken = "test-token", RefreshToken = "refresh-token" };
        var eveApiServices = new EveAPIServices(mockHttpClient.Object);

        var initialAllianceServices = eveApiServices.AllianceServices;
        var initialCorporationServices = eveApiServices.CorporationServices;
        var initialCharacterServices = eveApiServices.CharacterServices;

        // Act
        await eveApiServices.SetEveCharacterAuthenticatication(userToken);

        // Assert
        Assert.Equal(initialAllianceServices, eveApiServices.AllianceServices);
        Assert.Equal(initialCorporationServices, eveApiServices.CorporationServices);
        Assert.Equal(initialCharacterServices, eveApiServices.CharacterServices);
    }
}