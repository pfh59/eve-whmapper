using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
using Xunit;

namespace WHMapper.Tests.Services.OAuth;


public class EveCookieServiceCollectionExtensionsTest
{
    private readonly Mock<IServiceCollection> _mockServiceCollection;

    public EveCookieServiceCollectionExtensionsTest()
    {
        _mockServiceCollection = new Mock<IServiceCollection>();
    }

    [Fact]
    public void ConfigureEveCookieRefresh_AddsCookieEveRefresherAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureEveCookieRefresh("eveCookieScheme", "eveScheme");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var refresher = serviceProvider.GetService<CookieEveRefresher>();
        Assert.NotNull(refresher);
    }

    [Fact]
    public void ConfigureEveCookieRefresh_ConfiguresCookieAuthenticationOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<CookieEveRefresher>();

        // Act
        services.ConfigureEveCookieRefresh("eveCookieScheme", "eveScheme");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsMonitor.Get("eveCookieScheme");

        Assert.NotNull(options.Events.OnValidatePrincipal);
    }

    [Fact]
    public void ConfigureEveCookieRefresh_ConfiguresEVEOnlineAuthenticationOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureEveCookieRefresh("eveCookieScheme", "eveScheme");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<EVEOnlineAuthenticationOptions>>();
        var options = optionsMonitor.Get("eveScheme");

        Assert.True(options.SaveTokens);
    }
}
