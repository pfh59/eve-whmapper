using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

[Collection("10-WHMapperEntity")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]

public class EveWHMapperEntityTest
{
    private const int BAD_ID = -1;
    private const int ALLIANCE_GOONS_ID = 1354830081;
    private const string ALLIANCE_GOONS_NAME = "Goonswarm Federation";

    private const int CORPORATION_GOONS_ID = 1344654522;
    private const string CORPORATION_GOONS_NAME = "DJ's Retirement Fund";


    private const int CHARACTER_GOONS_ID = 2113720458;
    private const string CHARACTER_GOONS_NAME = "Sexy Gym Teacher";

    private const int SHIP_RIFTER_ID = 587;
    private const string SHIP_RIFTER_NAME = "Rifter";

    private const int SYSTEM_JITA_ID = 30000142;
    private const string SYSTEM_JITA_NAME = "Jita";

    private const int CONSTELLATION_Kimotoro_ID = 20000020;
    private const string CONSTELLATION_Kimotoro_NAME = "Kimotoro";

    private const int REGION_THE_FORGE_ID = 10000002;
    private const string REGION_THE_FORGE_NAME = "The Forge";

    private const int STARGATE_JITA_TO_MAURASI_ID = 50001248;
    private const string STARGATE_JITA_TO_MAURASI_NAME = "Maurasi";


    private const int GROUPE_WORMHOLE_ID = 988;

    private const int SECONDARY_SUN_ID_TYPE = 30577;

    private const int WH_TYPE_ID1 = 30583;
    private const int WH_TYPE_ID2 = 30584;

    private readonly IEveMapperService _eveMapperService;
    private readonly IEveMapperCacheService _eveMapperCacheService;

    public EveWHMapperEntityTest()
    {
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration.GetConnectionString("RedisConnection");
            option.InstanceName = "WHMapper";
        });

        var provider = services.BuildServiceProvider();
        if (provider != null)
        {
            var httpclientfactory = provider.GetService<IHttpClientFactory>();
            IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();

            if (_distriCache != null && httpclientfactory != null)
            {
                var userInfoService = new EveUserInfosServices(null!);
                ILogger<EveMapperService> logger = new NullLogger<EveMapperService>();
                ILogger<EveAPIServices> loggerAPI = new NullLogger<EveAPIServices>();
                ILogger<CacheService> loggerCacheService = new NullLogger<CacheService>();
                ILogger<EveMapperCacheService> loggereveMapperCacheService = new NullLogger<EveMapperCacheService>();

                var cacheService = new CacheService(loggerCacheService, _distriCache);
                _eveMapperCacheService = new EveMapperCacheService(loggereveMapperCacheService, cacheService);
                var eveApiservice = new EveAPIServices(loggerAPI, httpclientfactory, new TokenProvider(), userInfoService);

                _eveMapperService = new EveMapperService(logger, _eveMapperCacheService, eveApiservice);
            }
        }
    }

    [Fact, Priority(1)]

    public async Task Get_Character_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<CharactereEntity>();
        Assert.True(clearing);

        var character = await _eveMapperService.GetCharacter(BAD_ID);
        Assert.Null(character);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        character = await _eveMapperService.GetCharacter(CHARACTER_GOONS_ID);
        sw.Stop();
        Assert.NotNull(character);
        Assert.Equal(CHARACTER_GOONS_NAME, character.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        character = await _eveMapperService.GetCharacter(CHARACTER_GOONS_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(character);
        Assert.Equal(CHARACTER_GOONS_NAME, character.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<CharactereEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(2)]
    public async Task Get_Corporation_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<CorporationEntity>();
        Assert.True(clearing);

        var corporation = await _eveMapperService.GetCorporation(BAD_ID);
        Assert.Null(corporation);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        corporation = await _eveMapperService.GetCorporation(CORPORATION_GOONS_ID);
        sw.Stop();
        Assert.NotNull(corporation);
        Assert.Equal(CORPORATION_GOONS_NAME, corporation.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        corporation = await _eveMapperService.GetCorporation(CORPORATION_GOONS_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(corporation);
        Assert.Equal(CORPORATION_GOONS_NAME, corporation.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<CorporationEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(3)]
    public async Task Get_Alliance_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<AllianceEntity>();
        Assert.True(clearing);

        var alliance = await _eveMapperService.GetAlliance(BAD_ID);
        Assert.Null(alliance);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        alliance = await _eveMapperService.GetAlliance(ALLIANCE_GOONS_ID);
        sw.Stop();
        Assert.NotNull(alliance);
        Assert.Equal(ALLIANCE_GOONS_NAME, alliance.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        alliance = await _eveMapperService.GetAlliance(ALLIANCE_GOONS_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(alliance);
        Assert.Equal(ALLIANCE_GOONS_NAME, alliance.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<AllianceEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(4)]
    public async Task Get_Ship_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<ShipEntity>();
        Assert.True(clearing);

        var ship = await _eveMapperService.GetShip(BAD_ID);
        Assert.Null(ship);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        ship = await _eveMapperService.GetShip(SHIP_RIFTER_ID);
        sw.Stop();
        Assert.NotNull(ship);
        Assert.Equal(SHIP_RIFTER_NAME, ship.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        ship = await _eveMapperService.GetShip(SHIP_RIFTER_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(ship);
        Assert.Equal(SHIP_RIFTER_NAME, ship.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<ShipEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(5)]
    public async Task Get_System_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<SystemEntity>();
        Assert.True(clearing);

        var system = await _eveMapperService.GetSystem(BAD_ID);
        Assert.Null(system);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        system = await _eveMapperService.GetSystem(SYSTEM_JITA_ID);
        sw.Stop();
        Assert.NotNull(system);
        Assert.Equal(SYSTEM_JITA_NAME, system.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        system = await _eveMapperService.GetSystem(SYSTEM_JITA_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(system);
        Assert.Equal(SYSTEM_JITA_NAME, system.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<SystemEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(6)]
    public async Task Get_Constellation_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<ConstellationEntity>();
        Assert.True(clearing);

        var constellation = await _eveMapperService.GetConstellation(BAD_ID);
        Assert.Null(constellation);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        constellation = await _eveMapperService.GetConstellation(CONSTELLATION_Kimotoro_ID);
        sw.Stop();
        Assert.NotNull(constellation);
        Assert.Equal(CONSTELLATION_Kimotoro_NAME, constellation.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        constellation = await _eveMapperService.GetConstellation(CONSTELLATION_Kimotoro_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(constellation);
        Assert.Equal(CONSTELLATION_Kimotoro_NAME, constellation.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<ConstellationEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(7)]
    public async Task Get_Region_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<RegionEntity>();
        Assert.True(clearing);

        var region = await _eveMapperService.GetRegion(BAD_ID);
        Assert.Null(region);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        region = await _eveMapperService.GetRegion(REGION_THE_FORGE_ID);
        sw.Stop();
        Assert.NotNull(region);
        Assert.Equal(REGION_THE_FORGE_NAME, region.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        region = await _eveMapperService.GetRegion(REGION_THE_FORGE_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(region);
        Assert.Equal(REGION_THE_FORGE_NAME, region.Name);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<RegionEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(8)]
    public async Task Get_Stargate_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<StargateEntity>();
        Assert.True(clearing);

        var stargate = await _eveMapperService.GetStargate(BAD_ID);
        Assert.Null(stargate);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        stargate = await _eveMapperService.GetStargate(STARGATE_JITA_TO_MAURASI_ID);
        sw.Stop();
        Assert.NotNull(stargate);
        Assert.Equal(SYSTEM_JITA_ID, stargate.SourceId);
        Assert.Contains(STARGATE_JITA_TO_MAURASI_NAME, stargate.Name);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        stargate = await _eveMapperService.GetStargate(STARGATE_JITA_TO_MAURASI_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(stargate);
        Assert.Contains(STARGATE_JITA_TO_MAURASI_NAME, stargate.Name);
        Assert.Equal(SYSTEM_JITA_ID, stargate.SourceId);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<StargateEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(9)]
    public async Task Get_Group_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<GroupEntity>();
        Assert.True(clearing);

        var group = await _eveMapperService.GetGroup(BAD_ID);
        Assert.Null(group);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        group = await _eveMapperService.GetGroup(GROUPE_WORMHOLE_ID);
        sw.Stop();
        Assert.NotNull(group);
        Assert.NotEmpty(group.Types);
        Assert.Equal(GROUPE_WORMHOLE_ID, group.Id);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        group = await _eveMapperService.GetGroup(GROUPE_WORMHOLE_ID);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(group);
        Assert.NotEmpty(group.Types);
        Assert.Equal(GROUPE_WORMHOLE_ID, group.Id);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<GroupEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(10)]
    public async Task Get_Wormhole_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<WHEntity>();
        Assert.True(clearing);

        var wormhole = await _eveMapperService.GetWormhole(BAD_ID);
        Assert.Null(wormhole);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        wormhole = await _eveMapperService.GetWormhole(WH_TYPE_ID1);
        sw.Stop();
        Assert.NotNull(wormhole);
        Assert.Equal(WH_TYPE_ID1, wormhole.Id);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        wormhole = await _eveMapperService.GetWormhole(WH_TYPE_ID1);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(wormhole);
        Assert.Equal(WH_TYPE_ID1, wormhole.Id);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<WHEntity>();
        Assert.True(clearing);
    }

    [Fact, Priority(11)]
    public async Task Get_Sun_Test()
    {
        var clearing = await _eveMapperCacheService.ClearCacheAsync<SunEntity>();
        Assert.True(clearing);

        var sun = await _eveMapperService.GetSun(BAD_ID);
        Assert.Null(sun);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        sun = await _eveMapperService.GetSun(SECONDARY_SUN_ID_TYPE);
        sw.Stop();
        Assert.NotNull(sun);
        Assert.Equal(SECONDARY_SUN_ID_TYPE, sun.Id);
        long without_cache = sw.ElapsedMilliseconds;

        //Check if cache is working
        sw.Reset();
        sw.Restart();
        sun = await _eveMapperService.GetSun(SECONDARY_SUN_ID_TYPE);
        sw.Stop();
        long with_cache = sw.ElapsedMilliseconds;
        Assert.NotNull(sun);
        Assert.Equal(SECONDARY_SUN_ID_TYPE, sun.Id);
        Assert.True(with_cache < without_cache);

        clearing = await _eveMapperCacheService.ClearCacheAsync<SunEntity>();
        Assert.True(clearing);
    }
}