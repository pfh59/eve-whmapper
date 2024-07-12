using Microsoft.Extensions.Logging;
using WHMapper.Services.EveAPI.UserInterface;
using WHMapper.Shared.Models.DTO;
using WHMapper.Shared.Services.EveAPI.Alliances;
using WHMapper.Shared.Services.EveAPI.Assets;
using WHMapper.Shared.Services.EveAPI.Characters;
using WHMapper.Shared.Services.EveAPI.Corporations;
using WHMapper.Shared.Services.EveAPI.Dogma;
using WHMapper.Shared.Services.EveAPI.Locations;
using WHMapper.Shared.Services.EveAPI.Routes;
using WHMapper.Shared.Services.EveAPI.Search;
using WHMapper.Shared.Services.EveAPI.Universe;
using WHMapper.Shared.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Shared.Services.EveAPI
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
        public ISearchServices SearchServices { get; private set; }
        public IDogmaServices DogmaServices { get; private set; }
        public IAssetsServices AssetsServices { get; private set; }
        public IRouteServices RouteServices { get; private set; }

        public EveAPIServices(ILogger<EveAPIServices> logger,
            IHttpClientFactory httpClientFactory,
            TokenProvider tokenProvider,
            IEveUserInfosServices userService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(httpClientFactory);
            ArgumentNullException.ThrowIfNull(tokenProvider);
            ArgumentNullException.ThrowIfNull(userService);

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
            AssetsServices = new AssetsServices(eveAPIClient, _tokenProvider, userService);
        }
    }
}
