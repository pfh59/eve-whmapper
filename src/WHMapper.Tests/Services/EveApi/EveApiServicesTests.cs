using Microsoft.Extensions.Logging;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Tests.Services.EveApi
{
    public class EveAPIServicesTests
    {
        [Theory, AutoDomainData]
        public void WhenHTTPCLientIsNull_Constructing_ThrowsException(
                HttpClient client,
                IEveUserInfosServices userServices
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(null!, userServices));
        }       
        
        [Theory, AutoDomainData]
        public void WhenUserServicesIsNull_Constructing_ThrowsException(
                HttpClient client,
                IEveUserInfosServices userService
            )
        {
            Assert.ThrowsAny<ArgumentException>(() => new EveAPIServices(client, null!));
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
