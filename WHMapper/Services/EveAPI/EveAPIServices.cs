using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Universe;


namespace WHMapper.Services.EveAPI
{
    public class EveAPIServices : IEveAPIServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenProvider _tokenProvider;

        public ILocationServices LocationServices { get; private set; }
        public IUniverseServices UniverseServices { get; private set; }


        public EveAPIServices(IHttpClientFactory httpClientFactory, TokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;

            var eveAPIClient = _httpClientFactory.CreateClient();

            LocationServices = new LocationServices(eveAPIClient, _tokenProvider);
            UniverseServices = new UniverseServices(eveAPIClient, _tokenProvider);
        }
    }
}

