using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Universe;


namespace WHMapper.Services.EveAPI
{
    public class EveAPIServices : IEveAPIServices
    {
        private readonly HttpClient _httpClient;
        private readonly TokenProvider _tokenProvider;

        public ILocationServices LocationServices { get; private set; }
        public IUniverseServices UniverseServices { get; private set; }


        public EveAPIServices(HttpClient httpClient, TokenProvider tokenProvider)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;

            

            LocationServices = new LocationServices(_httpClient, _tokenProvider.AccessToken, _tokenProvider.CharacterId);
            UniverseServices = new UniverseServices(_httpClient, _tokenProvider.AccessToken);
        }



    }
}

