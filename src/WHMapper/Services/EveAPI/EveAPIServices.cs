using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Alliance;
using WHMapper.Services.EveAPI.Assets;
using WHMapper.Services.EveAPI.Character;
using WHMapper.Services.EveAPI.Corporation;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Search;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveAPI.UserInterface;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Services.EveAPI
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EveAPIServices : IEveAPIServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenProvider _tokenProvider;
        private readonly ILogger _logger;

        public ILocationServices LocationServices { get; private set; }
        public IUniverseServices UniverseServices { get; private set; }
        public IUserInterfaceServices UserInterfaceServices { get; private set; }
        public IAllianceServices AllianceServices { get; private set; }
        public ICorporationServices CorporationServices { get; private set; }
        public ICharacterServices CharacterServices { get; private set; }
        public ISearchServices SearchServices { get;private set; }
        public IDogmaServices DogmaServices { get; private set; }

        public IAssetsServices AssetsServices { get; private set; }

        public IRouteServices RouteServices { get; private set; }

        public EveAPIServices(ILogger<EveAPIServices> logger,IHttpClientFactory httpClientFactory, TokenProvider tokenProvider, IEveUserInfosServices userService)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _logger = logger;

            _logger.LogInformation("Init EveAPIServices");
            var eveAPIClient = _httpClientFactory.CreateClient();

            LocationServices = new LocationServices(eveAPIClient, _tokenProvider, userService);
            UniverseServices = new UniverseServices(eveAPIClient);
            UserInterfaceServices = new UserInterfaceServices(eveAPIClient, _tokenProvider);
            AllianceServices = new AllianceServices(eveAPIClient);
            CorporationServices = new CorporationServices(eveAPIClient);
            CharacterServices = new CharacterServices(eveAPIClient);
            SearchServices = new SearchServices(eveAPIClient, _tokenProvider, userService);
            DogmaServices = new DogmaServices(eveAPIClient);
            RouteServices = new RouteServices(eveAPIClient);
            AssetsServices = new AssetsServices(eveAPIClient, _tokenProvider,userService);
        }
    }
}

