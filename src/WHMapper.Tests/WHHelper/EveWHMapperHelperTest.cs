using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.SDE;
using WHMapper.Services.WHColor;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("Sequential2")]
    public class EveWHMapperHelperTest
    {
        private const int DEFAULT_MAP_ID = 1;
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const char SOLAR_SYSTEM_EXTENSION_NAME = 'B';
        private const string CONSTELLATION_JITA_NAME = "Kimotoro";
        private const string REGION_JITA_NAME = "The Forge";

        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const int CONSTELLATION_WH_ID = 21000113;
        private const string CONSTELLATION_WH_NAME = "C-C00113";
        private const string REGION_WH_NAME = "C-R00012";
        private const EveSystemType SOLAR_SYSTEM_WH_CLASS = EveSystemType.C3;
        private const WHEffect SOLAR_SYSTEM_WH_EFFECT = WHEffect.Pulsar;
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";


        private const string REGION_WH_C1_NAME = "A-R00001";
        private const string SOLAR_SYSTEM_WH_C1_NAME = "J100744";

        private const string REGION_WH_C2_NAME = "B-R00004";
        private const string SOLAR_SYSTEM_WH_C2_NAME =  "J101524";

        private const string REGION_WH_C4_NAME = "D-R00016";
        private const string SOLAR_SYSTEM_WH_C4_NAME =  "J104754";

        private const string REGION_WH_C5_NAME = "E-R00024";
        private const string SOLAR_SYSTEM_WH_C5_NAME = "J103251";

        private const string REGION_WH_C6_NAME = "F-R00030";
        private const string SOLAR_SYSTEM_WH_C6_NAME = "J104859";

        private const string REGION_WH_THERA_NAME = "G-R00031";
        private const string SOLAR_SYSTEM_WH_THERA_NAME = "Thera";

        private const string REGION_WH_C13_NAME = "H-R00032";
        private const string SOLAR_SYSTEM_WH_C13_NAME = "J000487";

        private const string REGION_WH_POCHVEN_NAME = "Pochven";
        private const string SOLAR_SYSTEM_WH_POCHVEN_NAME = "Archee";

        private const string REGION_SPECIAL = "K-R00033";
        private const string C14_NAME = "J055520";
        private const string C15_NAME = "J110145";
        private const string C16_NAME = "J164710";
        private const string C17_NAME = "J200727";
        private const string C18_NAME = "J174618";

        private IEveMapperHelper _whEveMapper;

        public EveWHMapperHelperTest()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            var httpclientfactory = provider.GetService<IHttpClientFactory>();



            ILogger<SDEServices> loggerSDE = new NullLogger<SDEServices>();
            ILogger<AnoikServices> loggerAnoik = new NullLogger<AnoikServices>();
            ILogger<EveMapperHelper> loggerMapperHelper = new NullLogger<EveMapperHelper>();
            ILogger<EveAPIServices> loggerAPI = new NullLogger<EveAPIServices>();

            

            _whEveMapper = new EveMapperHelper(loggerMapperHelper
                , new EveAPIServices(loggerAPI, httpclientfactory, new Models.DTO.TokenProvider(), null)
                , new SDEServices(loggerSDE),
                new AnoikServices(loggerAnoik), new WHNoteRepository(new NullLogger<WHNoteRepository>(), null));
        }

        [Fact, Priority(1)]
        public async Task Is_Wormhole()
        {
            bool not_wh_result = _whEveMapper.IsWorhmole(SOLAR_SYSTEM_JITA_NAME);
            Assert.False(not_wh_result);

            bool is_wh_result = _whEveMapper.IsWorhmole(SOLAR_SYSTEM_WH_NAME);
            Assert.True(is_wh_result);
        }


        [Fact, Priority(2)]
        public async Task Get_Wormhole_Class()
        {
            var result_C3_Bis= await _whEveMapper.GetWHClass(new Models.DTO.EveAPI.Universe.SolarSystem(0, 31001123, SOLAR_SYSTEM_WH_NAME,null,-1.0f,string.Empty,CONSTELLATION_WH_ID,null,null));
            Assert.Equal(EveSystemType.C3, result_C3_Bis);

            var result_HS = await _whEveMapper.GetWHClass(REGION_JITA_NAME, "UNUSED", SOLAR_SYSTEM_JITA_NAME);
            Assert.Equal(EveSystemType.None, result_HS);

            var result_C1 = await _whEveMapper.GetWHClass(REGION_WH_C1_NAME, "UNUSED", SOLAR_SYSTEM_WH_C1_NAME);
            Assert.Equal(EveSystemType.C1, result_C1);

            var result_C2 = await _whEveMapper.GetWHClass(REGION_WH_C2_NAME, "UNUSED", SOLAR_SYSTEM_WH_C2_NAME);
            Assert.Equal(EveSystemType.C2, result_C2);

            var result_C3 = await _whEveMapper.GetWHClass(REGION_WH_NAME, "UNUSED", SOLAR_SYSTEM_WH_NAME);
            Assert.Equal(EveSystemType.C3, result_C3);

            var result_C4 = await _whEveMapper.GetWHClass(REGION_WH_C4_NAME, "UNUSED", SOLAR_SYSTEM_WH_C4_NAME);
            Assert.Equal(EveSystemType.C4, result_C4);

            var result_C5 = await _whEveMapper.GetWHClass(REGION_WH_C5_NAME, "UNUSED", SOLAR_SYSTEM_WH_C5_NAME);
            Assert.Equal(EveSystemType.C5, result_C5);

            var result_C6 = await _whEveMapper.GetWHClass(REGION_WH_C6_NAME, "UNUSED", SOLAR_SYSTEM_WH_C6_NAME);
            Assert.Equal(EveSystemType.C6, result_C6);

            var result_THERA= await _whEveMapper.GetWHClass(REGION_WH_THERA_NAME, "UNUSED", SOLAR_SYSTEM_WH_THERA_NAME);
            Assert.Equal(EveSystemType.Thera, result_THERA);

            var result_C13 = await _whEveMapper.GetWHClass(REGION_WH_C13_NAME, "UNUSED", SOLAR_SYSTEM_WH_C13_NAME);
            Assert.Equal(EveSystemType.C13, result_C13);

            var result_C14 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C14_NAME);
            Assert.Equal(EveSystemType.C14, result_C14);

            var result_C15 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C15_NAME);
            Assert.Equal(EveSystemType.C15, result_C15);

            var result_C16 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C16_NAME);
            Assert.Equal(EveSystemType.C16, result_C16);

            var result_C17 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C17_NAME);
            Assert.Equal(EveSystemType.C17, result_C17);

            var result_C18 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C18_NAME);
            Assert.Equal(EveSystemType.C18, result_C18);

            var result_POCHVEN = await _whEveMapper.GetWHClass(REGION_WH_POCHVEN_NAME, "UNUSED", SOLAR_SYSTEM_WH_POCHVEN_NAME);
            Assert.Equal(EveSystemType.None, result_POCHVEN);
        }

        [Fact, Priority(3)]
        public async Task Define_Eve_System_Node_Model()
        {
            var jita_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, 1.0f));
            Assert.NotNull(jita_result);
            Assert.Equal(Models.DTO.EveMapper.Enums.EveSystemType.HS, jita_result.SystemType);


            var wh_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, -1.0f));
            Assert.Equal(SOLAR_SYSTEM_WH_CLASS, wh_result.SystemType);
            Assert.NotEqual(WHEffect.None, wh_result.Effect);
            Assert.Equal(SOLAR_SYSTEM_WH_EFFECT, wh_result.Effect);
        }
    }
}

