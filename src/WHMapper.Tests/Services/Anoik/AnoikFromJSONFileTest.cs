using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Services.Anoik;
using Xunit.Priority;
using static MudBlazor.Colors;

namespace WHMapper.Tests.Services
{

    [Collection("Services")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class AnoikFromJSONFileTest
    {
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";

        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_CLASS = "C3";
        private const string SOLAR_SYSTEM_WH_EFFECT = "Pulsar";
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";

        private IAnoikServices _anoik;

        public AnoikFromJSONFileTest()
        {
            ILogger<AnoikServices> logger = new NullLogger<AnoikServices>();
            _anoik = new AnoikServices(logger);

        }

        [Fact]
        public void Get_System_Id()
        {
            var sysJitaId = _anoik.GetSystemId(SOLAR_SYSTEM_JITA_NAME);
            Assert.Null(sysJitaId);

            var sysWHId = _anoik.GetSystemId(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(sysWHId);
            Assert.Equal(SOLAR_SYSTEM_WH_ID, sysWHId);

        }

        [Fact]
        public void Get_System_Class()
        {
            var sysJitaClass = _anoik.GetSystemClass(SOLAR_SYSTEM_JITA_NAME);
            Assert.Null(sysJitaClass);

            var sysClass = _anoik.GetSystemClass(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(sysClass);
            Assert.NotEmpty(sysClass);
            Assert.Equal(SOLAR_SYSTEM_WH_CLASS, sysClass);
        }

        [Fact]
        public void Get_System_Effect()
        {
            var sysJitaEffect = _anoik.GetSystemEffects(SOLAR_SYSTEM_JITA_NAME);
            Assert.Null(sysJitaEffect);

            var sysEffect = _anoik.GetSystemEffects(SOLAR_SYSTEM_WH_NAME);
            Assert.NotNull(sysEffect);
            Assert.NotEmpty(sysEffect);
            Assert.Equal(SOLAR_SYSTEM_WH_EFFECT, sysEffect);
        }
        [Fact]
        public void Get_System_Effects_Infos()
        {
            var badResult = _anoik.GetSystemEffectsInfos(string.Empty, string.Empty);
            Assert.Null(badResult);

            var sysEffectsINfos = _anoik.GetSystemEffectsInfos(SOLAR_SYSTEM_WH_EFFECT, SOLAR_SYSTEM_WH_CLASS);
            Assert.NotNull(sysEffectsINfos);
        }


        [Fact]
        public async Task Get_System_Statics()
        {
            var sysJitaStatics = await _anoik.GetSystemStatics(SOLAR_SYSTEM_JITA_NAME);
            Assert.Null(sysJitaStatics);

            var sysStatics = await _anoik.GetSystemStatics(SOLAR_SYSTEM_WH_NAME);
            Assert.NotEmpty(sysStatics);
            Assert.Contains(new KeyValuePair<string,string>(SOLAR_SYSTEM_WH_STATICS, "HS"), sysStatics);
        }

        [Fact]
        public async Task Get_Wormhole_Types()
        {
            var whTypes = await _anoik.GetWormholeTypes();
            Assert.NotNull(whTypes);
            var whType = whTypes.FirstOrDefault(x => x.Name == SOLAR_SYSTEM_WH_STATICS);
            Assert.NotNull(whType);
            Assert.Equal(SOLAR_SYSTEM_WH_STATICS, whType.Name);
            Assert.Equal("HS", whType.Destination);
            Assert.NotNull(whType.Sources);
            Assert.Contains("C3", whType.Sources);
            Assert.Equal(SOLAR_SYSTEM_WH_STATICS + " -> HS", whType.ToString());

        }
    }
}

