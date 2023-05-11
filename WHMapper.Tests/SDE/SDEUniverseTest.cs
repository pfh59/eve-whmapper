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
        private const string SOLAR_SYSTEM_JIT_NAME = "jit";

        private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
        private const string SOLAR_SYSTEM_AMA_NAME = "ama";

        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

        private ISDEServices _services;

        public SDEUniverseTest()
        {
            ILogger<SDEServices> logger = new NullLogger<SDEServices>();
            _services = new SDEServices(logger);
        }

        [Fact]
        public void Search_System()
        {
            //TEST empty
            var empty_result = _services.SearchSystem("");
            Assert.Null(empty_result);
            //TEST JITA
            var jita_result = _services.SearchSystem(SOLAR_SYSTEM_JITA_NAME);
            Assert.NotNull(jita_result);
            Assert.Single(jita_result);

            //TEST JI for JITA partial
            var ji_result = _services.SearchSystem(SOLAR_SYSTEM_JIT_NAME);
            Assert.NotNull(ji_result);
            Assert.Contains(ji_result, x => x.Name.Contains(SOLAR_SYSTEM_JITA_NAME, StringComparison.OrdinalIgnoreCase));


            //TEST AMARR
            var amarr_result = _services.SearchSystem(SOLAR_SYSTEM_AMARR_NAME);
            Assert.NotNull(amarr_result);
            Assert.Single(amarr_result);

            //TEST AMA for AMARR partial
            var ama_result = _services.SearchSystem(SOLAR_SYSTEM_AMA_NAME);
            Assert.NotNull(ama_result);
            Assert.Contains(ama_result, x => x.Name.Contains(SOLAR_SYSTEM_AMARR_NAME, StringComparison.OrdinalIgnoreCase));


            //TEST WH
            var wh_result = _services.SearchSystem(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(wh_result);
            Assert.Single(wh_result);

            var wh_partial_result = _services.SearchSystem(SOLAR_SYSTEM_WH_PARTIAL_NAME);
            Assert.NotNull(wh_partial_result);
            Assert.Contains(wh_partial_result, x => x.Name.Contains(SOLAR_SYSTEM_WH_NAME, StringComparison.OrdinalIgnoreCase));


            //TESTABYSSAL


        }
    }
}

