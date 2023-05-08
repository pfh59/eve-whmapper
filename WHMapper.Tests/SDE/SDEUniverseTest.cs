using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Services.Anoik;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.SDE
{
    [TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.SDE")]
    public class SDEUniverseTest
    {

        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const string SOLAR_SYSTEM_JIT_NAME = "Jit";

        private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
        private const string SOLAR_SYSTEM_AMA_NAME = "Ama";

        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

        private ISDEServices _services;

        public SDEUniverseTest()
        {
            ILogger<SDEServices> logger = new NullLogger<SDEServices>();
            _services = new SDEServices(logger);
        }

        [Fact]
        public async Task Search_System()
        {
            //TEST empty
            var empty_result = await _services.SearchSystem("");
            Assert.Null(empty_result);
            //TEST JITA
            var jita_result = await _services.SearchSystem(SOLAR_SYSTEM_JITA_NAME);
            Assert.NotNull(jita_result);
            Assert.Equal(1, jita_result.Count());

            //TEST JI for JITA partial
            var ji_result = await _services.SearchSystem(SOLAR_SYSTEM_JIT_NAME);
            Assert.NotNull(ji_result);
            Assert.True(ji_result.Any(x => x.Name.Contains(SOLAR_SYSTEM_JITA_NAME,StringComparison.OrdinalIgnoreCase)));


            //TEST AMARR
            var amarr_result = await _services.SearchSystem(SOLAR_SYSTEM_AMARR_NAME);
            Assert.NotNull(amarr_result);
            Assert.Equal(1, amarr_result.Count());

            //TEST AMA for AMARR partial
            var ama_result = await _services.SearchSystem(SOLAR_SYSTEM_AMA_NAME);
            Assert.NotNull(ama_result);
            Assert.True(ama_result.Any(x => x.Name.Contains(SOLAR_SYSTEM_AMARR_NAME, StringComparison.OrdinalIgnoreCase)));


            //TEST WH
            var wh_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(wh_result);
            Assert.Equal(1, wh_result.Count());

            var wh_partial_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_PARTIAL_NAME);
            Assert.NotNull(wh_partial_result);
            Assert.True(wh_partial_result.Any(x => x.Name.Contains(SOLAR_SYSTEM_WH_NAME,StringComparison.OrdinalIgnoreCase)));


            //TESTABYSSAL


        }
    }
}

