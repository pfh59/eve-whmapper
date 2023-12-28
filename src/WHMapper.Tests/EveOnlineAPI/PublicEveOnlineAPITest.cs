using System;
using Microsoft.Extensions.DependencyInjection;
using WHMapper.Services.EveAPI.Alliance;
using WHMapper.Services.EveAPI.Character;
using WHMapper.Services.EveAPI.Corporation;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Universe;
using Xunit.Priority;

namespace WHMapper.Tests.EveOnlineAPI
{

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

        private const int TYPE_MAGNETAR_ID = 30574;//Magnetar
        private const int TYPE_BLACK_HOLE_ID = 30575;//Black Hole
        private const int TYPE_RED_GIANT_ID = 30576;//Red Giant
        private const int TYPE_PULSAR_ID = 30577;//Pulsar
        private const int TYPE_WOLFRAYET_ID = 30669;//Wolf-Rayet Star
        private const int TYPE_CATACLYSMIC_ID = 30670;//Cataclysmic Variable

        //public API
        private IUniverseServices _eveUniverseApi;
        private IDogmaServices _eveDogmaApi;
        private IAllianceServices _eveAllianceApi;
        private ICorporationServices _eveCorpoApi;
        private ICharacterServices _eveCharacterApi;
        private IRouteServices _routeServices;

        public PublicEveOnlineAPITest()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            var httpclientfactory = provider.GetService<IHttpClientFactory>();


            _eveUniverseApi = new UniverseServices(httpclientfactory.CreateClient());
            _eveDogmaApi = new DogmaServices(httpclientfactory.CreateClient());
            _eveAllianceApi = new AllianceServices(httpclientfactory.CreateClient());
            _eveCorpoApi = new CorporationServices(httpclientfactory.CreateClient());
            _eveCharacterApi = new CharacterServices(httpclientfactory.CreateClient());
            _routeServices= new RouteServices(httpclientfactory.CreateClient());
        }

        [Fact]
        public async Task Get_Universe_System_Constellation_Region_Star_And_Stargate()
        {
            //getsystems
            int[] systems = await _eveUniverseApi.GetSystems();
            Assert.NotNull(systems);
            Assert.NotEmpty(systems);
            Assert.Contains(systems, item => SOLAR_SYSTEM_JITA_ID==item);

            //constellations
            int[] constellations = await _eveUniverseApi.GetContellations();
            Assert.NotNull(constellations);
            Assert.NotEmpty(constellations);
            Assert.Contains(constellations, item => CONSTELLATION_ID == item);

            //regions
            int[] regions = await _eveUniverseApi.GetRegions();
            Assert.NotNull(regions);
            Assert.NotEmpty(regions);
            Assert.Contains(regions, item => REGION_ID == item);


            //test Jita
            var jita = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_JITA_ID);
            Assert.NotNull(jita);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, jita.Name);
            Assert.NotNull(jita.Stargates);

            var jita_constellation = await _eveUniverseApi.GetContellation(jita.ConstellationId);
            Assert.NotNull(jita_constellation);
            Assert.Equal(CONSTELLATION_NAME, jita_constellation.Name);

            var jita_region= await _eveUniverseApi.GetRegion(jita_constellation.RegionId);
            Assert.NotNull(jita_region);
            Assert.Equal(jita_region.Name, jita_region.Name);

            //test Jita star
            var jitaStar = await _eveUniverseApi.GetStar(jita.StarId);
            Assert.NotNull(jitaStar);
            Assert.Contains(SOLAR_SYSTEM_JITA_NAME, jitaStar.Name);

            //test Jita Stargate
            var jitaStargate = await _eveUniverseApi.GetStargate(jita.Stargates[0]);
            Assert.NotNull(jitaStargate);
            Assert.Equal(SOLAR_SYSTEM_JITA_ID, jitaStargate.SystemId);



            //test wh
            var wh = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_WH_ID);
            Assert.NotNull(wh);
            Assert.Equal(SOLAR_SYSTEM_WH_NAME, wh.Name);
            Assert.Null(wh.Stargates);

            //test wh star
            var whStar = await _eveUniverseApi.GetStar(wh.StarId);
            Assert.NotNull(whStar);
            Assert.Contains(SOLAR_SYSTEM_WH_NAME, whStar.Name);
        }


        [Fact]
        public async Task Get_Universe_Categories_And_Category()
        {
            int[] categories = await _eveUniverseApi.GetCategories();
            Assert.NotNull(categories);

            //Test Celestial
            Assert.Contains<int>(CATEGORY_CELESTIAL_ID, categories);
            var celestialCategory = await _eveUniverseApi.GetCategory(CATEGORY_CELESTIAL_ID);
            Assert.NotNull(celestialCategory);
            Assert.Equal(CELESTIAL_GATEGORY_NAME, celestialCategory.Name);
        }


        [Fact]
        public async Task Get_Universe_Groups_And_Group()
        {
            int[] groups = await _eveUniverseApi.GetGroups();
            Assert.NotNull(groups);

            //test sun
            Assert.Contains<int>(GROUP_STAR_ID, groups);
            var sunGroups = await _eveUniverseApi.GetGroup(GROUP_STAR_ID);
            Assert.NotNull(sunGroups);
            Assert.Equal(SUN_GROUP_NAME, sunGroups.Name);

            //planet
            Assert.Contains<int>(GROUP_PLANET_ID, groups);
            var planetGroups = await _eveUniverseApi.GetGroup(GROUP_PLANET_ID);
            Assert.NotNull(planetGroups);
            Assert.Equal(PLANET_GROUP_NAME, planetGroups.Name);

            //wormhole
            //has been removed ??? but always accessible if you get group with GROUP_WORMHOLE_ID
            // Assert.Contains<int>(GROUP_WORMHOLE_ID, groups);
            var whGroups = await _eveUniverseApi.GetGroup(GROUP_WORMHOLE_ID);
            Assert.NotNull(whGroups);
            Assert.Equal(WORMHOLE_GROUP_NAME, whGroups.Name);
        }

        [Fact]
        public async Task Get_Universe_Types_And_Type()
        {
            int[] types = await _eveUniverseApi.GetTypes();
            Assert.NotNull(types);

            var res = await _eveUniverseApi.GetType(TYPE_F135_ID);
            Assert.NotNull(res);
            Assert.Equal(TYPE_F135_NAME, res.Name);
        }


        [Fact]
        public async Task Get_Dogma_Attributes_And_Effects()
        {
            var attributes = await _eveDogmaApi.GetAttributes();
            Assert.NotNull(attributes);


            var effects = await _eveDogmaApi.GetEffects();
            Assert.NotNull(effects);
        }

        [Fact]
        public async Task Get_Alliances_And_Alliance()
        {
            int[] alliances = await _eveAllianceApi.GetAlliances();
            Assert.NotNull(alliances);
            Assert.NotEmpty(alliances);
            Assert.Contains<int>(ALLIANCE_GOONS_ID, alliances);

            var goonsAlliance = await _eveAllianceApi.GetAlliance(ALLIANCE_GOONS_ID);
            Assert.NotNull(goonsAlliance);
            Assert.Equal(ALLIANCE_GOONS_NAME, goonsAlliance.Name);
        }

        [Fact]
        public async Task Get_Corporation()
        {
            var corpo = await _eveCorpoApi.GetCorporation(CORPORATION_GOONS_ID);
            Assert.NotNull(corpo);
            Assert.Equal(ALLIANCE_GOONS_ID, corpo.AllianceId);
            Assert.Equal(CORPORATION_GOONS_NAME, corpo.Name);
        }


        [Fact]
        public async Task Get_Character()
        {
            var character = await _eveCharacterApi.GetCharacter(CHARACTER_GOONS_ID);
            Assert.NotNull(character);
            Assert.Equal(ALLIANCE_GOONS_ID, character.AllianceId);
            Assert.Equal(CORPORATION_GOONS_ID, character.CorporationId);
            Assert.Equal(CHARACTER_GOONS_NAME, character.Name);
        }

        [Fact]
        public async Task Get_Route()
        {
            //simple route in HS to HS via shortest path
            var route = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID);
            Assert.NotNull(route);
            Assert.NotEmpty(route);
            Assert.Equal(24, route.Length);
            Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[0]);
            Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[23]);

            //WH route to Jita without connections
            route = await _routeServices.GetRoute(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_JITA_ID);
            Assert.Null(route);

            //WH route to Jita with connections by amarr
            var connections = new int[][] { new int[] { SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_AMARR_ID } };

            route = await _routeServices.GetRoute(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_JITA_ID, connections);
            Assert.NotNull(route);
            Assert.NotEmpty(route);
            Assert.Equal(25, route.Length);
            Assert.Equal(SOLAR_SYSTEM_WH_ID, route[0]);
            Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[1]);
            Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[24]);

            //wh route from jita to amarr with avoid Ahbazon
            var avoid = new int[] { SOLAR_SYSTEM_AHBAZON_ID };
            route = await _routeServices.GetRoute(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_AMARR_ID, avoid);
            Assert.NotNull(route);
            Assert.NotEmpty(route);
            Assert.Equal(24, route.Length);
            Assert.Equal(SOLAR_SYSTEM_JITA_ID, route[0]);
            Assert.Equal(SOLAR_SYSTEM_AMARR_ID, route[23]);
            Assert.DoesNotContain(SOLAR_SYSTEM_AHBAZON_ID, route);

        }

    }

}

