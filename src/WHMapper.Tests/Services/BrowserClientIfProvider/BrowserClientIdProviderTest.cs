using System;

namespace WHMapper.Tests.Services.BrowserClientIfProvider;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;


public class BrowserClientIdProviderTest
{
    private readonly Mock<ILogger<WHMapper.Services.BrowserClientIdProvider.BrowserClientIdProvider>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly WHMapper.Services.BrowserClientIdProvider.BrowserClientIdProvider _clientIdProvider;

    public BrowserClientIdProviderTest()
    {
        _mockLogger = new Mock<ILogger<WHMapper.Services.BrowserClientIdProvider.BrowserClientIdProvider>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _clientIdProvider = new WHMapper.Services.BrowserClientIdProvider.BrowserClientIdProvider(
            _mockLogger.Object,
            _mockHttpContextAccessor.Object
        );
    }

    [Fact]
    public async Task GetOrCreateClientIdAsync_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = await _clientIdProvider.GetOrCreateClientIdAsync();

        // Assert
        Assert.Null(result);
        /*_mockLogger.Verify(
            l => l.LogError("HttpContext is null. Cannot access cookies."),
            Times.Once
        );*/
    }

    [Fact]
    public async Task GetOrCreateClientIdAsync_ShouldCreateAndReturnNewClientId_WhenCookieDoesNotExist()
    {
        // Arrange
        var mockHttpContext = new DefaultHttpContext();
        mockHttpContext.Request.Cookies = new Mock<IRequestCookieCollection>().Object;
       // mockHttpContext.Response.Cookies = new Mock<IResponseCookies>().Object;

        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext);

        var responseCookiesMock = new Mock<IResponseCookies>();
        responseCookiesMock
            .Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()));

       // mockHttpContext.Response.Cookies = responseCookiesMock.Object;

        // Act
        var result = await _clientIdProvider.GetOrCreateClientIdAsync();

        // Assert
        Assert.NotNull(result);
        /*responseCookiesMock.Verify(
            c => c.Append(
                "client_uid",
                It.IsAny<string>(),
                It.Is<CookieOptions>(o => o.HttpOnly && o.Secure && o.SameSite == SameSiteMode.Lax)
            ),
            Times.Once
        );*/
    }

    [Fact]
    public async Task GetOrCreateClientIdAsync_ShouldReturnExistingClientId_WhenCookieExists()
    {
        // Arrange
        var mockHttpContext = new DefaultHttpContext();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock.Setup(c => c.ContainsKey("client_uid")).Returns(true);
        requestCookiesMock.Setup(c => c["client_uid"]).Returns("existing-client-id");

        mockHttpContext.Request.Cookies = requestCookiesMock.Object;
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext);

        // Act
        var result = await _clientIdProvider.GetOrCreateClientIdAsync();

        // Assert
        Assert.Equal("existing-client-id", result);
    }
}
