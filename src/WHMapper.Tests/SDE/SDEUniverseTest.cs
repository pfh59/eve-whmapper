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
    [Collection("Sequential2")]
    public class SDEUniverseTest
    {

        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const string SOLAR_SYSTEM_JIT_NAME = "jit";

        private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
        private const string SOLAR_SYSTEM_AMA_NAME = "ama";

        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        //private const string SDE_ZIP_MOVE_PATH = @"./Resources/SDE/sde2.zip";
        private const string SDE_TARGET_DIRECTORY= @"./Resources/SDE/universe";

        private const string SDE_CHECKSUM_FILE = @"./Resources/SDE/checksum";
        private const string SDE_CHECKSUM_CURRENT_FILE = @"./Resources/SDE/currentchecksum";

        public SDEUniverseTest()
        {

        }


        [Fact, Priority(1)]
        public async Task Init_SDE_SERVICES()
        {
            if (Directory.Exists(SDE_TARGET_DIRECTORY))
                Directory.Delete(SDE_TARGET_DIRECTORY,true);

            if(File.Exists(SDE_CHECKSUM_CURRENT_FILE))
                File.Delete(SDE_CHECKSUM_CURRENT_FILE);

            if(File.Exists(SDE_CHECKSUM_FILE))
                File.Delete(SDE_CHECKSUM_FILE);

            ILogger<SDEServices> logger = new NullLogger<SDEServices>();

            //from scratch
            ISDEServices goodServices = new SDEServices(logger);
            Assert.True(goodServices.ExtractSuccess);

            //currentcheck not equal to download checksum
            File.WriteAllText(SDE_CHECKSUM_CURRENT_FILE,"azerty");
            goodServices = new SDEServices(logger);
            Assert.True(goodServices.ExtractSuccess);
        }

        [Fact, Priority(2)]
        public async Task Is_New_SDE_Available()
        {
            if (Directory.Exists(SDE_TARGET_DIRECTORY))
                Directory.Delete(SDE_TARGET_DIRECTORY,true);

            if(File.Exists(SDE_CHECKSUM_CURRENT_FILE))
                File.Delete(SDE_CHECKSUM_CURRENT_FILE);

            if(File.Exists(SDE_CHECKSUM_FILE))
                File.Delete(SDE_CHECKSUM_FILE);

            ILogger<SDEServices> logger = new NullLogger<SDEServices>();

            //from scratch
            ISDEServices goodServices = new SDEServices(logger);
            Assert.True(goodServices.ExtractSuccess);

            bool sameSDE = goodServices.IsNewSDEAvailable();
            Assert.False(sameSDE);

            //currentcheck not equal to download checksum
            File.WriteAllText(SDE_CHECKSUM_CURRENT_FILE,"azerty");
            bool newSDE = goodServices.IsNewSDEAvailable();
            Assert.True(newSDE);
        }

        [Fact, Priority(3)]
        public async Task Search_System()
        {
            ILogger<SDEServices> logger = new NullLogger<SDEServices>();
            ISDEServices _services = new SDEServices(logger);
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

