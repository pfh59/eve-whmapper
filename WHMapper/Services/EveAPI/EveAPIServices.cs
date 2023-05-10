using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Location;
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
        }
    }
}

