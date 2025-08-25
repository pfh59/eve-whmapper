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


namespace WHMapper.Services.EveAPI
{
    public class EveAPIServices : IEveAPIServices
    {
        private readonly HttpClient _httpClient;

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

        public EveAPIServices(HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            _httpClient = httpClient;


            LocationServices = new LocationServices(httpClient);
            UniverseServices = new UniverseServices(httpClient);
            UserInterfaceServices = new UserInterfaceServices(httpClient);
            AllianceServices = new AllianceServices(httpClient);
            CorporationServices = new CorporationServices(httpClient);
            CharacterServices = new CharacterServices(httpClient);
            SearchServices = new SearchServices(httpClient);
            DogmaServices = new DogmaServices(httpClient);
            RouteServices = new RouteServices(httpClient);
            AssetsServices = new AssetsServices(httpClient);
        }

        public Task SetEveCharacterAuthenticatication(UserToken userToken)
        {
            ArgumentNullException.ThrowIfNull(userToken);

            LocationServices = new LocationServices(_httpClient, userToken);
            SearchServices = new SearchServices(_httpClient, userToken);
            DogmaServices = new DogmaServices(_httpClient, userToken);
            AssetsServices = new AssetsServices(_httpClient, userToken);
            UserInterfaceServices = new UserInterfaceServices(_httpClient, userToken);

            return Task.CompletedTask;
        }
    }
}
