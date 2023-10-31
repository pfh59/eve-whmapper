using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;
using WHMapper.Pages.Mapper.Administration;
using WHMapper.Services.Anoik;
using WHMapper.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.SDE
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("Sequential")]
    public class SDEUniverseTest
    {

        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const string SOLAR_SYSTEM_JIT_NAME = "jit";

        private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
        private const string SOLAR_SYSTEM_AMA_NAME = "ama";

        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        private const string SDE_ZIP_MOVE_PATH = @"./Resources/SDE/sde2.zip";
        private const string SDE_TARGET_DIRECTORY= @"./Resources/SDE/universe";
        private ISDEServices _services;

        public SDEUniverseTest()
        {
            if (Directory.Exists(SDE_TARGET_DIRECTORY))
                Directory.Delete(SDE_TARGET_DIRECTORY,true);

            if(File.Exists(SDE_ZIP_MOVE_PATH))
                File.Delete(SDE_ZIP_MOVE_PATH);
        }


        [Fact, Priority(1)]
        public void Init_SDE_SERVICES()
        {
            ILogger<SDEServices> logger = new NullLogger<SDEServices>();
            File.Move(SDE_ZIP_PATH, SDE_ZIP_MOVE_PATH);
            //test catch
            _services = new SDEServices(logger);
            Assert.False(_services.ExtractSuccess);

            File.Move(SDE_ZIP_MOVE_PATH, SDE_ZIP_PATH);
            _services = new SDEServices(logger);
            Assert.True(_services.ExtractSuccess);
        }


        [Fact, Priority(2)]
        public async void Search_System()
        {
            ILogger<SDEServices> logger = new NullLogger<SDEServices>();
            _services = new SDEServices(logger);
            Assert.True(_services.ExtractSuccess);

            //TEST empty
            var empty_result = await _services.SearchSystem("");
            Assert.Null(empty_result);
            //TEST JITA
            var jita_result = await _services.SearchSystem(SOLAR_SYSTEM_JITA_NAME);
            Assert.NotNull(jita_result);
            Assert.Single(jita_result);

            //TEST JI for JITA partial
            var ji_result = await _services.SearchSystem(SOLAR_SYSTEM_JIT_NAME);
            Assert.NotNull(ji_result);
            Assert.Contains(ji_result, x => x.Name.Contains(SOLAR_SYSTEM_JITA_NAME, StringComparison.OrdinalIgnoreCase));


            //TEST AMARR
            var amarr_result = await _services.SearchSystem(SOLAR_SYSTEM_AMARR_NAME);
            Assert.NotNull(amarr_result);
            Assert.Single(amarr_result);

            //TEST AMA for AMARR partial
            var ama_result = await _services.SearchSystem(SOLAR_SYSTEM_AMA_NAME);
            Assert.NotNull(ama_result);
            Assert.Contains(ama_result, x => x.Name.Contains(SOLAR_SYSTEM_AMARR_NAME, StringComparison.OrdinalIgnoreCase));


            //TEST WH
            var wh_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(wh_result);
            Assert.Single(wh_result);

            var wh_partial_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_PARTIAL_NAME);
            Assert.NotNull(wh_partial_result);
            Assert.Contains(wh_partial_result, x => x.Name.Contains(SOLAR_SYSTEM_WH_NAME, StringComparison.OrdinalIgnoreCase));


            //TESTABYSSAL
        }



    }
}

