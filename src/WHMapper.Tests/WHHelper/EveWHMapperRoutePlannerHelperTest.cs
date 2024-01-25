using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

    [Collection("WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class EveWHMapperRoutePlannerHelperTest
{
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;
    private const int SOLAR_SYSTEM_AMARR_ID = 30002187;
    private const int SOLAR_SYSTEM_AHBAZON_ID = 30005196;

    private const int SOLAR_SYSTEM_WH_ID = 31001123;

    private IEveMapperRoutePlannerHelper _eveMapperRoutePlannerHelper;

    public EveWHMapperRoutePlannerHelperTest()
    {
        IDbContextFactory<WHMapperContext>? _contextFactory;
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        var provider = services.BuildServiceProvider();

        _contextFactory = provider.GetService<IDbContextFactory<WHMapperContext>>();

        if(_contextFactory != null)
        {

            ILogger<EveMapperRoutePlannerHelper> logger = new NullLogger<EveMapperRoutePlannerHelper>();
            ILogger<EveAPIServices> loggerAPI = new NullLogger<EveAPIServices>();
            ILogger<SDEServices> loggerSDE = new NullLogger<SDEServices>();

            _eveMapperRoutePlannerHelper = new EveMapperRoutePlannerHelper(logger, 
                new WHRouteRepository(new NullLogger<WHRouteRepository>(), _contextFactory),
                null!,new SDEServices(loggerSDE));
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
        Assert.NotNull(route_JITA_AHBAZON.Route);
        Assert.Equal(5,route_JITA_AHBAZON.Route.Length);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_WH_ID,RouteType.Shortest, new BlockingCollection<Models.DTO.RoutePlanner.RouteConnection> 
        {
            new RouteConnection(SOLAR_SYSTEM_WH_ID,-1.0f,SOLAR_SYSTEM_AHBAZON_ID,0.4f),
            new RouteConnection(SOLAR_SYSTEM_AHBAZON_ID,0.4f,SOLAR_SYSTEM_WH_ID,-1.0f)
        });
        

        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_WH_AHBAZON = routes.FirstOrDefault();
        Assert.NotNull(route_WH_AHBAZON);
        Assert.NotNull(route_WH_AHBAZON.Route);
        Assert.Equal(2,route_WH_AHBAZON.Route.Length);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_JITA_ID,RouteType.Shortest,new BlockingCollection<Models.DTO.RoutePlanner.RouteConnection> 
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
        Assert.Equal(3,route_JITA_WH_AHBAZON.Route.Length);

        var result2 = await _eveMapperRoutePlannerHelper.DeleteRoute(result.Id);
        Assert.True(result2);

        result = await _eveMapperRoutePlannerHelper.AddRoute(SOLAR_SYSTEM_AMARR_ID,true);

        routes = await _eveMapperRoutePlannerHelper.GetRoutesForAll(SOLAR_SYSTEM_JITA_ID,RouteType.Secure, null);
        Assert.NotNull(routes);
        Assert.NotEmpty(routes);
        var route_JITA_AMARR = routes.FirstOrDefault();
        Assert.NotNull(route_JITA_AMARR);
        Assert.NotNull(route_JITA_AMARR.Route);
        Assert.Equal(41,route_JITA_AMARR.Route.Length);

        result2 = await _eveMapperRoutePlannerHelper.DeleteRoute(result.Id);
        Assert.True(result2);
    }   

}
