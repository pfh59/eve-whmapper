using Microsoft.Extensions.Logging;
using WHMapper.Shared.Models.DTO;
using WHMapper.Shared.Services.EveAPI;
using WHMapper.Shared.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Tests.Services.EveApi
{
    public class EveAPIServicesTests
    {
        [Theory, AutoDomainData]
        public void WhenLoggerIsNull_Constructing_ThrowsException(
                IHttpClientFactory httpClientFactory,
                TokenProvider tokenProvider,
                IEveUserInfosServices userService
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(null!, httpClientFactory, tokenProvider, userService));
        }       
        
        [Theory, AutoDomainData]
        public void WhenClientFactoryIsNull_Constructing_ThrowsException(
                ILogger<EveAPIServices> logger,
                TokenProvider tokenProvider,
                IEveUserInfosServices userService
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(logger, null!, tokenProvider, userService));
        }        

        [Theory, AutoDomainData]
        public void WhenTokenProviderIsNull_Constructing_ThrowsException(
                ILogger<EveAPIServices> logger,
                IHttpClientFactory httpClientFactory,
                IEveUserInfosServices userService
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(logger, httpClientFactory, null!, userService));
        }       
        
        [Theory, AutoDomainData]
        public void WhenUserServiceIsNull_Constructing_ThrowsException(
                IHttpClientFactory httpClientFactory,
                TokenProvider tokenProvider,
                ILogger<EveAPIServices> logger
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(logger, httpClientFactory, tokenProvider, null!));
        }


        [Theory, AutoDomainData]
        public void WhenConstructing_HasLocationServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.LocationServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasUniverseServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.UniverseServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasUserInterfaceServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.UserInterfaceServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasAllianceServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.AllianceServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasCorporationServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.CorporationServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasCharacterServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.CharacterServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasSearchServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.SearchServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasDogmaServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.DogmaServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasAssetsServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.AssetsServices);
        }

        [Theory, AutoDomainData]
        public void WhenConstructing_HasRouteServices(
            EveAPIServices sut)
        {
            Assert.NotNull(sut.RouteServices);
        }
    }
}
