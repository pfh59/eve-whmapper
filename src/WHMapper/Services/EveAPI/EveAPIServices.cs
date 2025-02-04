using WHMapper.Models.DTO;
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

namespace WHMapper.Services.EveAPI
{
    public class EveAPIServices : IEveAPIServices
    {
        public ILocationServices LocationServices { get; private set; }
        public IUniverseServices UniverseServices { get; private set; }
        public IUserInterfaceServices UserInterfaceServices { get; private set; }
        public IAllianceServices AllianceServices { get; private set; }
        public ICorporationServices CorporationServices { get; private set; }
        public ICharacterServices CharacterServices { get; private set; }
        public ISearchServices SearchServices { get; private set; }
        public IDogmaServices DogmaServices { get; private set; }
        public IAssetsServices AssetsServices { get; private set; }
        public IRouteServices RouteServices { get; private set; }

        public EveAPIServices(HttpClient httpClient, IEveUserInfosServices userService)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(userService);

            LocationServices = new LocationServices(httpClient, userService);
            UniverseServices = new UniverseServices(httpClient);
            UserInterfaceServices = new UserInterfaceServices(httpClient);
            AllianceServices = new AllianceServices(httpClient);
            CorporationServices = new CorporationServices(httpClient);
            CharacterServices = new CharacterServices(httpClient);
            SearchServices = new SearchServices(httpClient, userService);
            DogmaServices = new DogmaServices(httpClient);
            RouteServices = new RouteServices(httpClient);
            AssetsServices = new AssetsServices(httpClient, userService);
        }
    }
}
