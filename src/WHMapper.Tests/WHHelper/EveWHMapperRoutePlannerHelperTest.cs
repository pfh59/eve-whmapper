using System.Collections.Concurrent;
using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Anoik;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;


[Collection("C7-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class EveWHMapperRoutePlannerHelperTest
{
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;
    private const int SOLAR_SYSTEM_AMARR_ID = 30002187;
    private const int SOLAR_SYSTEM_AHBAZON_ID = 30005196;

    private const int SOLAR_SYSTEM_WH_ID = 31001123;

    private IEveMapperRoutePlannerHelper _eveMapperRoutePlannerHelper = null!;

    public EveWHMapperRoutePlannerHelperTest()
    {
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection")));

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration.GetConnectionString("RedisConnection");
            option.InstanceName = "WHMapper";
        });

        var provider = services.BuildServiceProvider();

        IDbContextFactory<WHMapperContext>? _contextFactory = provider.GetService<IDbContextFactory<WHMapperContext>>();
        IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();

        if(_contextFactory != null && _distriCache != null)
        {

            ILogger<EveMapperRoutePlannerHelper> logger = new NullLogger<EveMapperRoutePlannerHelper>();
            ILogger<EveAPIServices> loggerAPI = new NullLogger<EveAPIServices>();
            ILogger<SDEService> loggerSDE = new NullLogger<SDEService>();
            ILogger<CacheService> loggerCacheService = new NullLogger<CacheService>();
            ILogger<WHRouteRepository> loggerWHRouteRepository = new NullLogger<WHRouteRepository>();

            IWHRouteRepository whRouteRepository = new WHRouteRepository(loggerWHRouteRepository, _contextFactory);

            ICacheService cacheService = new CacheService(loggerCacheService, _distriCache);
            
            IFileSystem fileSystem = new FileSystem();
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(configuration.GetValue<string>("SdeDataSupplier:BaseUrl")) };
            ISDEDataSupplier dataSupplier = new SdeDataSupplier(new NullLogger<SdeDataSupplier>(), httpClient);
            ISDEService sdeServices = new SDEService(loggerSDE, cacheService);

            _eveMapperRoutePlannerHelper = new EveMapperRoutePlannerHelper(logger,
                whRouteRepository,
                null!,
                sdeServices);       
        }
    }

    [Fact, Priority(1)]
    public async Task Add_Delete_Route()
    {
        var result = await _eveMapperRoutePlannerHelper.AddRoute(SOLAR_SYSTEM_AHBAZON_ID,true);
        Assert.NotNull(result);
        Assert.Equal(SOLAR_SYSTEM_AHBAZON_ID,result.SolarSystemId);
        Assert.Null(result.EveEntityId);

        var result2 = await _eveMapperRoutePlannerHelper.DeleteRoute(result.Id);
        Assert.True(result2);

        var result3 = await _eveMapperRoutePlannerHelper.DeleteRoute(-10);
        Assert.False(result3);

        var myRouteNull = await _eveMapperRoutePlannerHelper.AddRoute(SOLAR_SYSTEM_AHBAZON_ID,false);
        Assert.Null(myRouteNull);

    }




    [Fact, Priority(2)]
    public async Task Get_Route()
    {
        var badRoute = await _eveMapperRoutePlannerHelper.GetMyRoutes(123456789,RouteType.Shortest, null);
        Assert.Null(badRoute);

        var myRoutes = await _eveMapperRoutePlannerHelper.GetMyRoutes(SOLAR_SYSTEM_JITA_ID,RouteType.Shortest, null);
        Assert.Null(myRoutes);

        var routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_WH_ID,RouteType.Shortest, null);
        Assert.NotNull(routes);
        Assert.Empty(routes);

        var result = await _eveMapperRoutePlannerHelper.AddRoute(SOLAR_SYSTEM_AHBAZON_ID,true);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_JITA_ID,RouteType.Shortest, null);
        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_JITA_AHBAZON = routes.FirstOrDefault();
        Assert.NotNull(route_JITA_AHBAZON);
        Assert.True(route_JITA_AHBAZON.IsAvailable);
        Assert.False(route_JITA_AHBAZON.IsShowed);
        Assert.NotNull(route_JITA_AHBAZON.Route);
        Assert.Equal(5,route_JITA_AHBAZON.RouteLength);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_WH_ID,RouteType.Shortest, new BlockingCollection<RouteConnection> 
        {
            new RouteConnection(SOLAR_SYSTEM_WH_ID,-1.0f,SOLAR_SYSTEM_AHBAZON_ID,0.4f),
            new RouteConnection(SOLAR_SYSTEM_AHBAZON_ID,0.4f,SOLAR_SYSTEM_WH_ID,-1.0f)
        });
        

        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_WH_AHBAZON = routes.FirstOrDefault();
        Assert.NotNull(route_WH_AHBAZON);
        Assert.NotNull(route_WH_AHBAZON.Route);
        Assert.Equal(2,route_WH_AHBAZON.RouteLength);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_JITA_ID,RouteType.Shortest,new BlockingCollection<RouteConnection> 
        {
            new RouteConnection(SOLAR_SYSTEM_WH_ID,-1.0f,SOLAR_SYSTEM_AHBAZON_ID,0.4f),
            new RouteConnection(SOLAR_SYSTEM_AHBAZON_ID,0.4f,SOLAR_SYSTEM_WH_ID,-1.0f),
            new RouteConnection(SOLAR_SYSTEM_WH_ID,-1.0f,SOLAR_SYSTEM_JITA_ID,1.0f),
            new RouteConnection(SOLAR_SYSTEM_JITA_ID,1.0f,SOLAR_SYSTEM_WH_ID,-1.0f)
        });
        
    
        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_JITA_WH_AHBAZON = routes.FirstOrDefault();
        Assert.NotNull(route_JITA_WH_AHBAZON);
        Assert.NotNull(route_JITA_WH_AHBAZON.Route);
        Assert.Equal(3,route_JITA_WH_AHBAZON.RouteLength);

        var result2 = await _eveMapperRoutePlannerHelper.DeleteRoute(result!.Id);
        Assert.True(result2);

        result = await _eveMapperRoutePlannerHelper.AddRoute(SOLAR_SYSTEM_AMARR_ID,true);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_JITA_ID,RouteType.Secure, null);
        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_JITA_AMARR = routes.FirstOrDefault();
        Assert.NotNull(route_JITA_AMARR);
        Assert.True(route_JITA_AMARR.IsAvailable);
        Assert.False(route_JITA_AMARR.IsShowed);
        Assert.NotNull(route_JITA_AMARR.Route);
        Assert.Equal(46,route_JITA_AMARR.RouteLength);

        result2 = await _eveMapperRoutePlannerHelper.DeleteRoute(result!.Id);
        Assert.True(result2);
    }   

}
