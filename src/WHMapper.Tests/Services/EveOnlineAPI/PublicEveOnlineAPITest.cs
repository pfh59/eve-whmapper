using Microsoft.Extensions.DependencyInjection;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Alliances;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveAPI.Corporations;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Routes;
using WHMapper.Services.EveAPI.Universe;
using Xunit.Priority;

namespace WHMapper.Tests.Services;

[Collection("C3-Services")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class PublicEveOnlineAPITest
{
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;
    private const int SOLAR_SYSTEM_AMARR_ID = 30002187;
    private const int SOLAR_SYSTEM_AHBAZON_ID = 30005196;



    private const int CONSTELLATION_ID = 20000020;
    private const string CONSTELLATION_NAME = "Kimotoro";
    private const int REGION_ID = 10000002;
    private const string REGION_NAME = "The Forge";



    private const string SOLAR_SYSTEM_JITA_NAME = "Jita";

    private const int SOLAR_SYSTEM_WH_ID = 31001123;
    private const string SOLAR_SYSTEM_WH_NAME = "J165153";



    private const int CATEGORY_CELESTIAL_ID = 2;
    private const int CATEGORY_STRUCTURE_ID = 65;
    private const int CATEGORY_SHIP_ID = 6;

    private const int GROUP_STAR_ID = 6;
    private const int GROUP_PLANET_ID = 7;
    private const int GROUP_WORMHOLE_ID = 988;


    private const int ALLIANCE_GOONS_ID = 1354830081;
    private const string ALLIANCE_GOONS_NAME = "Goonswarm Federation";

    private const int CORPORATION_GOONS_ID = 1344654522;
    private const string CORPORATION_GOONS_NAME = "DJ's Retirement Fund";


    private const int CHARACTER_GOONS_ID = 2113720458;
    private const string CHARACTER_GOONS_NAME = "Sexy Gym Teacher";

    //private const int DOGMA_ATTRIBUTE_SCANWHSTRENGTH_ID=1908

    private const string SUN_GROUP_NAME = "Sun";
    private const string PLANET_GROUP_NAME = "Planet";
    private const string WORMHOLE_GROUP_NAME = "Wormhole";


    private const string CELESTIAL_GATEGORY_NAME = "Celestial";

    private const int TYPE_F135_ID = 34372;//WH F135
    private const string TYPE_F135_NAME = "Wormhole F135";
    private const int TYPE_Skybreaker_ID = 54731;//Skybreaker

    private const int TYPE_MAGNETAR_ID = 30574;//Magnetar
    private const int TYPE_BLACK_HOLE_ID = 30575;//Black Hole
    private const int TYPE_RED_GIANT_ID = 30576;//Red Giant
    private const int TYPE_PULSAR_ID = 30577;//Pulsar
    private const int TYPE_WOLFRAYET_ID = 30669;//Wolf-Rayet Star
    private const int TYPE_CATACLYSMIC_ID = 30670;//Cataclysmic Variable

    //public API
    private IUniverseServices? _eveUniverseApi;
    private IDogmaServices? _eveDogmaApi;
    private IAllianceServices? _eveAllianceApi;
    private ICorporationServices? _eveCorpoApi;
    private ICharacterServices? _eveCharacterApi;
    private IRouteServices? _routeServices;

    public PublicEveOnlineAPITest()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        var provider = services.BuildServiceProvider();
        var httpclientfactory = provider.GetService<IHttpClientFactory>();

        if (httpclientfactory != null)
        {
            var httpClient = httpclientfactory.CreateClient();
            httpClient.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);

            _eveUniverseApi = new UniverseServices(httpClient);
            _eveDogmaApi = new DogmaServices(httpClient);
            _eveAllianceApi = new AllianceServices(httpClient);
            _eveCorpoApi = new CorporationServices(httpClient);
            _eveCharacterApi = new CharacterServices(httpClient);
            _routeServices = new RouteServices(httpClient);
        }
    }

    [Fact]
    public async Task Get_Universe_System_Constellation_Region_Star_And_Stargate()
    {
        Assert.NotNull(_eveUniverseApi);
        //getsystems
        var systemsResult = await _eveUniverseApi.GetSystems();
        int[]? systems = systemsResult?.Data;
        Assert.NotNull(systems);
        Assert.NotEmpty(systems);
        Assert.Contains(systems, item => SOLAR_SYSTEM_JITA_ID==item);

        //constellations
        var constellationsResult = await _eveUniverseApi.GetContellations();
        int[]? constellations = constellationsResult?.Data;
        Assert.NotNull(constellations);
        Assert.NotEmpty(constellations);
        Assert.Contains(constellations, item => CONSTELLATION_ID == item);

        //regions
        var regionsResult = await _eveUniverseApi.GetRegions();
        int[]? regions = regionsResult?.Data;
        Assert.NotNull(regions);
        Assert.NotEmpty(regions);
        Assert.Contains(regions, item => REGION_ID == item);


        //test Jita
        var jitaResult = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_JITA_ID);
        var jita = jitaResult?.Data;
        Assert.NotNull(jita);
        Assert.Equal(SOLAR_SYSTEM_JITA_NAME, jita.Name);
        Assert.NotNull(jita.Stargates);

        var jita_constellationResult = await _eveUniverseApi.GetConstellation(jita.ConstellationId);
        var jita_constellation = jita_constellationResult?.Data;
        Assert.NotNull(jita_constellation);
        Assert.Equal(CONSTELLATION_NAME, jita_constellation.Name);

        var jita_regionResult = await _eveUniverseApi.GetRegion(jita_constellation.RegionId);
        var jita_region = jita_regionResult?.Data;
        Assert.NotNull(jita_region);
        Assert.Equal(jita_region.Name, jita_region.Name);

        //test Jita star
        var jitaStarResult = await _eveUniverseApi.GetStar(jita.StarId);
        var jitaStar = jitaStarResult?.Data;
        Assert.NotNull(jitaStar);
        Assert.Contains(SOLAR_SYSTEM_JITA_NAME, jitaStar.Name);

        //test Jita Stargate
        var jitaStargateResult = await _eveUniverseApi.GetStargate(jita.Stargates[0]);
        var jitaStargate = jitaStargateResult?.Data;
        Assert.NotNull(jitaStargate);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID, jitaStargate.SystemId);



        //test wh
        var whResult = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_WH_ID);
        var wh = whResult?.Data;
        Assert.NotNull(wh);
        Assert.Equal(SOLAR_SYSTEM_WH_NAME, wh.Name);
        Assert.Null(wh.Stargates);

        //test wh star
        var whStarResult = await _eveUniverseApi.GetStar(wh.StarId);
        var whStar = whStarResult?.Data;
        Assert.NotNull(whStar);
        Assert.Contains(SOLAR_SYSTEM_WH_NAME, whStar.Name);
    }


    [Fact]
    public async Task Get_Universe_Categories_And_Category()
    {
        Assert.NotNull(_eveUniverseApi);
        var categoriesResult = await _eveUniverseApi.GetCategories();
        int[]? categories = categoriesResult?.Data;
        Assert.NotNull(categories);

        //Test Celestial
        Assert.Contains<int>(CATEGORY_CELESTIAL_ID, categories);
        var celestialCategoryResult = await _eveUniverseApi.GetCategory(CATEGORY_CELESTIAL_ID);
        var celestialCategory = celestialCategoryResult?.Data;
        Assert.NotNull(celestialCategory);
        Assert.Equal(CELESTIAL_GATEGORY_NAME, celestialCategory.Name);
    }


    [Fact]
    public async Task Get_Universe_Groups_And_Group()
    {
        Assert.NotNull(_eveUniverseApi);
        var groupsResult = await _eveUniverseApi.GetGroups();
        int[]? groups = groupsResult?.Data;
        Assert.NotNull(groups);

        //test sun
        Assert.Contains<int>(GROUP_STAR_ID, groups);
        var sunGroupsResult = await _eveUniverseApi.GetGroup(GROUP_STAR_ID);
        var sunGroups = sunGroupsResult?.Data;
        Assert.NotNull(sunGroups);
        Assert.Equal(SUN_GROUP_NAME, sunGroups.Name);

        //planet
        Assert.Contains<int>(GROUP_PLANET_ID, groups);
        var planetGroupsResult = await _eveUniverseApi.GetGroup(GROUP_PLANET_ID);
        var planetGroups = planetGroupsResult?.Data;
        Assert.NotNull(planetGroups);
        Assert.Equal(PLANET_GROUP_NAME, planetGroups.Name);

        //wormhole
        //has been removed ??? but always accessible if you get group with GROUP_WORMHOLE_ID
        // Assert.Contains<int>(GROUP_WORMHOLE_ID, groups);
        var whGroupsResult = await _eveUniverseApi.GetGroup(GROUP_WORMHOLE_ID);
        var whGroups = whGroupsResult?.Data;
        Assert.NotNull(whGroups);
        Assert.Equal(WORMHOLE_GROUP_NAME, whGroups.Name);
    }

    [Fact]
    public async Task Get_Universe_Types_And_Type()
    {
        Assert.NotNull(_eveUniverseApi);
        var typesResult = await _eveUniverseApi.GetTypes();
        int[]? types = typesResult?.Data;
        Assert.NotNull(types);

        var resResult = await _eveUniverseApi.GetType(TYPE_F135_ID);
        var res = resResult?.Data;
        Assert.NotNull(res);
        Assert.Equal(TYPE_F135_NAME, res.Name);

        var res2Result = await _eveUniverseApi.GetType(TYPE_Skybreaker_ID);
        var res2 = res2Result?.Data;
        Assert.NotNull(res2);
        Assert.Equal("Skybreaker", res2.Name);
        
    }


    [Fact]
    public async Task Get_Dogma_Attributes_And_Effects()
    {
        Assert.NotNull(_eveDogmaApi);
        var attributes = await _eveDogmaApi.GetAttributes();
        Assert.NotNull(attributes);


        var effects = await _eveDogmaApi.GetEffects();
        Assert.NotNull(effects);
    }

    [Fact]
    public async Task Get_Alliances_And_Alliance()
    {
        Assert.NotNull(_eveAllianceApi);
        var alliancesResult = await _eveAllianceApi.GetAlliances();
        int[]? alliances = alliancesResult?.Data;
        Assert.NotNull(alliances);
        Assert.NotEmpty(alliances);
        Assert.Contains<int>(ALLIANCE_GOONS_ID, alliances);

        var goonsAllianceResult = await _eveAllianceApi.GetAlliance(ALLIANCE_GOONS_ID);
        var goonsAlliance = goonsAllianceResult?.Data;
        Assert.NotNull(goonsAlliance);
        Assert.Equal(ALLIANCE_GOONS_NAME, goonsAlliance.Name);
    }

    [Fact]
    public async Task Get_Corporation()
    {
        Assert.NotNull(_eveCorpoApi);
        var corpoResult = await _eveCorpoApi.GetCorporation(CORPORATION_GOONS_ID);
        Assert.NotNull(corpoResult);
        var corpo = corpoResult.Data;
        Assert.NotNull(corpo);
        Assert.Equal(ALLIANCE_GOONS_ID, corpo.AllianceId);
        Assert.Equal(CORPORATION_GOONS_NAME, corpo.Name);
    }


    [Fact]
    public async Task Get_Character()
    {
        Assert.NotNull(_eveCharacterApi);
        var characterResult = await _eveCharacterApi.GetCharacter(CHARACTER_GOONS_ID);
        Assert.NotNull(characterResult);
        var character = characterResult.Data;
        Assert.NotNull(character);
        Assert.Equal(ALLIANCE_GOONS_ID, character.AllianceId);
        Assert.Equal(CORPORATION_GOONS_ID, character.CorporationId);
        Assert.Equal(CHARACTER_GOONS_NAME, character.Name);
    }

    [Fact]
    public async Task Get_Charchter_Portrait()
    {
        Assert.NotNull(_eveCharacterApi);
        var portraitResult = await _eveCharacterApi.GetCharacterPortrait(CHARACTER_GOONS_ID);
        Assert.NotNull(portraitResult);
        var portrait = portraitResult.Data;
        Assert.NotNull(portrait);
        Assert.NotNull(portrait.Picture512x512);
        Assert.NotNull(portrait.Picture256x256);
        Assert.NotNull(portrait.Picture128x128);
        Assert.NotNull(portrait.Picture64x64);
    }

    [Fact]
    public async Task Get_Route()
    {
        //simple route in HS to HS via shortest path
        Assert.NotNull(_routeServices);
        var routeResult = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID);
        Assert.NotNull(routeResult?.Data);
        var route = routeResult.Data;
        Assert.NotEmpty(route);
        Assert.Equal(12, route.Length);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[0]);
        Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[11]);

        //WH route to Jita without connections
        var routeResult2 = await _routeServices.GetRoute(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_JITA_ID);
        Assert.Null(routeResult2?.Data);

        //WH route to Jita with connections by amarr
        var connections = new int[][] { new int[] { SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_AMARR_ID } };

        var routeResult3 = await _routeServices.GetRoute(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_JITA_ID, connections);
        Assert.NotNull(routeResult3?.Data);
        route = routeResult3.Data;
        Assert.NotEmpty(route);
        Assert.Equal(13, route.Length);
        Assert.Equal(SOLAR_SYSTEM_WH_ID, route[0]);
        Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[1]);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[12]);

        //wh route from jita to amarr with avoid Ahbazon
        var avoid = new int[] { SOLAR_SYSTEM_AHBAZON_ID };
        var routeResult4 = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID, avoid);
        Assert.NotNull(routeResult4?.Data);
        route = routeResult4.Data;
        Assert.NotEmpty(route);
        Assert.Equal(24, route.Length);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[0]);
        Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[23]);
        Assert.DoesNotContain(SOLAR_SYSTEM_AHBAZON_ID, route);

        //WH route to Jita with connections by amarr and avoid Ahbazon
        var routeResult5 = await _routeServices.GetRoute(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_JITA_ID,RouteType.Shortest, avoid,connections);
        Assert.NotNull(routeResult5?.Data);
        route = routeResult5.Data;
        Assert.NotEmpty(route);


        //wh route from jita to amarr safer
        var routeResult6 = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID, RouteType.Secure,null,null);
        Assert.NotNull(routeResult6?.Data);
        route = routeResult6.Data;
        Assert.NotEmpty(route);
        Assert.Equal(46, route.Length);


        //wh route from jita to amarr safer
        var routeResult7 = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID, RouteType.Insecure,null,null);
        Assert.NotNull(routeResult7?.Data);
        route = routeResult7.Data;
        Assert.NotEmpty(route);
        Assert.Equal(41, route.Length);

    }

}

