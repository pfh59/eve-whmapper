using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Models.Db;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHColor;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class EveWHMapperHelperTest
    {
        private const int DEFAULT_MAP_ID = 1;
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const char SOLAR_SYSTEM_EXTENSION_NAME = 'B';

        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_CLASS = "C3";
        private const string SOLAR_SYSTEM_WH_EFFECT = "Pulsar";
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";

        private IEveMapperHelper _whEveMapper;

        public EveWHMapperHelperTest()
        {
            ILogger<AnoikServices> logger = new NullLogger<AnoikServices>();
            _whEveMapper = new EveMapperHelper(new AnoikServices(logger));
        }

        [Fact]
        public async Task Define_Eve_System_Node_Model()
        {
            var jita_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, 1.0f));
            Assert.NotNull(jita_result);
            Assert.Equal("H", jita_result.Class);


            var wh_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, -1.0f));
            Assert.NotNull(wh_result.Effect);
            Assert.Equal(SOLAR_SYSTEM_WH_CLASS, wh_result.Class);
            Assert.NotNull(wh_result.Effect);
            Assert.NotEmpty(wh_result.Effect);
            Assert.Equal(SOLAR_SYSTEM_WH_EFFECT, wh_result.Effect);

        }
    }
}

