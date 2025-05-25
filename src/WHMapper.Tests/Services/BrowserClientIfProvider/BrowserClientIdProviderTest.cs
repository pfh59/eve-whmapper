using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Services.BrowserClientIdProvider;
using Xunit;

namespace WHMapper.Services.BrowserClientIdProvider.Tests
{
    public class BrowserClientIdProviderTests
    {
        private readonly Mock<ILogger<BrowserClientIdProvider>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly BrowserClientIdProvider _provider;

        public BrowserClientIdProviderTests()
        {
            _mockLogger = new Mock<ILogger<BrowserClientIdProvider>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _provider = new BrowserClientIdProvider(_mockLogger.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task GetClientIdAsync_ReturnsNull_WhenHttpContextIsNull()
        {
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var result = await _provider.GetClientIdAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClientIdAsync_ReturnsClientId_WhenCookieExists()
        {
            var mockHttpContext = new DefaultHttpContext();
            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c.ContainsKey("client_uid")).Returns(true);
            cookiesMock.Setup(c => c["client_uid"]).Returns("test-client-id");
            mockHttpContext.Request.Cookies = cookiesMock.Object;

            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext);

            var result = await _provider.GetClientIdAsync();

            Assert.Equal("test-client-id", result);
        }

        [Fact]
        public async Task GetClientIdAsync_ReturnsNull_WhenCookieDoesNotExist()
        {
            var mockHttpContext = new DefaultHttpContext();
            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c.ContainsKey("client_uid")).Returns(false);
            mockHttpContext.Request.Cookies = cookiesMock.Object;

            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext);

            var result = await _provider.GetClientIdAsync();

            Assert.Null(result);
        }
    }
}