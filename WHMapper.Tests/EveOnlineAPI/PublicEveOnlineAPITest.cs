using System;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Tests.Attributes;

namespace WHMapper.Tests.EveOnlineAPI
{

    [TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.EveOnlineAPI")]
    public class PublicEveOnlineAPITest
    {
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";

        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";

        

        private const int CATEGORY_CELESTIAL_ID = 2;
        private const int CATEGORY_STRUCTURE_ID = 65;
        private const int CATEGORY_SHIP_ID = 6;

        private const int GROUP_STAR_ID = 6;
        private const int GROUP_PLANET_ID = 7;
        private const int GROUP_WORMHOLE_ID = 988;

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

        public PublicEveOnlineAPITest()
		{
            var services = new ServiceCollection();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            var httpclientfactory = provider.GetService<IHttpClientFactory>();


            _eveUniverseApi = new UniverseServices(httpclientfactory.CreateClient());
            _eveDogmaApi = new DogmaServices(httpclientfactory.CreateClient());
        }

        [Fact]
        public async Task Get_Universe_System_And_Star()
        {
            //test Jita
            var jita = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_JITA_ID);
            Assert.NotNull(jita);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, jita.Name);

            //test Jita star
            var jitaStar = await _eveUniverseApi.GetStar(jita.StarId);
            Assert.NotNull(jitaStar);
            Assert.Contains(SOLAR_SYSTEM_JITA_NAME, jitaStar.Name);

            //test wh
            var wh = await _eveUniverseApi.GetSystem(SOLAR_SYSTEM_WH_ID);
            Assert.NotNull(wh);
            Assert.Equal(SOLAR_SYSTEM_WH_NAME, wh.Name);

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
            var  celestialCategory = await _eveUniverseApi.GetCategory(CATEGORY_CELESTIAL_ID);
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

    }
}

