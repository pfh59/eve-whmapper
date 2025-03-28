using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI.Locations;
using WHMapper.Services.EveOAuthProvider.Services;
using Xunit;

namespace WHMapper.Tests.Services.EveAPI.Locations
{
    public class LocationServicesTest
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly UserToken _userToken;

        public LocationServicesTest()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://esi.evetech.net")
            };
            _userToken = new UserToken { AccountId = "12345", AccessToken = "test-token" };
        }

        [Fact]
        public async Task GetLocation_ShouldReturnEveLocation_WhenUserTokenIsValid()
        {
            // Arrange
            var expectedLocation = new EveLocation(30000142,1,1);
            
            var jsonResponse = "{\"solar_system_id\": 30000142}";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var locationServices = new LocationServices(_httpClient, _userToken);

            // Act
            var result = await locationServices.GetLocation();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocation.SolarSystemId, result?.SolarSystemId);
        }

        [Fact]
        public async Task GetLocation_ShouldReturnNull_WhenUserTokenIsNull()
        {
            // Arrange
            var locationServices = new LocationServices(_httpClient, null);

            // Act
            var result = await locationServices.GetLocation();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLocation_ShouldReturnNull_WhenApiResponseIsError()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var locationServices = new LocationServices(_httpClient, _userToken);

            // Act
            var result = await locationServices.GetLocation();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentShip_ShouldReturnShip_WhenUserTokenIsValid()
        {
            // Arrange
            var expectedShip = new Ship { ShipTypeId = 123, ShipName = "Test Ship" };
            var jsonResponse = "{\"ship_type_id\": 123, \"ship_name\": \"Test Ship\"}";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var locationServices = new LocationServices(_httpClient, _userToken);

            // Act
            var result = await locationServices.GetCurrentShip();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedShip.ShipTypeId, result?.ShipTypeId);
            Assert.Equal(expectedShip.ShipName, result?.ShipName);
        }

        [Fact]
        public async Task GetCurrentShip_ShouldReturnNull_WhenUserTokenIsNull()
        {
            // Arrange
            var locationServices = new LocationServices(_httpClient, null);

            // Act
            var result = await locationServices.GetCurrentShip();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentShip_ShouldReturnNull_WhenApiResponseIsError()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var locationServices = new LocationServices(_httpClient, _userToken);

            // Act
            var result = await locationServices.GetCurrentShip();

            // Assert
            Assert.Null(result);
        }
    }
}