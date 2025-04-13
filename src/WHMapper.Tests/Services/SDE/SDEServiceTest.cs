using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.Cache;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.Services.SDE
{
    public class SDEServiceTest
    {
        private readonly Mock<ILogger<SDEService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly SDEService _sdeService;

        public SDEServiceTest()
        {
            _mockLogger = new Mock<ILogger<SDEService>>();
            _mockCacheService = new Mock<ICacheService>();
            _sdeService = new SDEService(_mockLogger.Object, _mockCacheService.Object);
        }

        [Fact]
        public async Task GetSolarSystemList_ShouldReturnData_WhenCacheHasData()
        {
            // Arrange
            var mockData = new List<SDESolarSystem> { new SDESolarSystem { SolarSystemID = 1, Name = "TestSystem" } };
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _sdeService.GetSolarSystemList();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestSystem", result.First().Name);
        }

        [Fact]
        public async Task GetSolarSystemList_ShouldReturnEmptyList_WhenCacheIsNull()
        {
            // Arrange
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<SDESolarSystem>?)null);

            // Act
            var result = await _sdeService.GetSolarSystemList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSolarSystemList_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Cache error"));

            // Act
            var result = await _sdeService.GetSolarSystemList();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchSystemById_ShouldReturnSystem_WhenFound()
        {
            // Arrange
            var mockData = new List<SDESolarSystem>
            {
                new SDESolarSystem { SolarSystemID = 1, Name = "TestSystem" }
            };
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _sdeService.SearchSystemById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.SolarSystemID);
        }

        [Fact]
        public async Task SearchSystemById_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var mockData = new List<SDESolarSystem>
            {
                new SDESolarSystem { SolarSystemID = 1, Name = "TestSystem" }
            };
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _sdeService.SearchSystemById(2);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchSystem_ShouldReturnResults_WhenMatchFound()
        {
            // Arrange
            var mockData = new List<SDESolarSystem>
            {
                new SDESolarSystem { SolarSystemID = 1, Name = "TestSystem" },
                new SDESolarSystem { SolarSystemID = 2, Name = "AnotherSystem" }
            };
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _sdeService.SearchSystem("Test");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestSystem", result.First().Name);
        }

        [Fact]
        public async Task SearchSystem_ShouldReturnNull_WhenInputIsInvalid()
        {
            // Act
            var result = await _sdeService.SearchSystem("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchSystem_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            _mockCacheService
                .Setup(c => c.Get<IEnumerable<SDESolarSystem>?>(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Cache error"));

            // Act
            var result = await _sdeService.SearchSystem("Test");

            // Assert
            Assert.Null(result);
        }
    }
}