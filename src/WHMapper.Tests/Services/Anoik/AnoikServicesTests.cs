using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Shared.Services.Anoik;

namespace WHMapper.Tests.Services.Anoik
{
    public class AnoikServicesTests
    {
        [Theory]
        [InlineAutoMoqData("J120450", 31001554)]
        public void GetSystemId_KnownSystemName_ReturnsSystemId(string systemName, int systemId, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemId(systemName);

            Assert.NotNull(result);
            Assert.Equal(systemId, result);
        }

        [Theory]
        [InlineAutoMoqData("Jita")]
        [InlineAutoMoqData("Amarr")]
        [InlineAutoMoqData("Hek")]
        public void GetSystemId_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemId(systemName);

            Assert.Null(result);
        }

        [Theory, AutoDomainData]
        public void GetSystemId_InvalidParameter_ThrowsException(ILogger<AnoikServices> logger, IAnoikDataSupplier anoikDataSupplier)
        {
            string? parameter = null;
            var service = new AnoikServices(logger, anoikDataSupplier);
            Assert.ThrowsAny<ArgumentNullException>(() => service.GetSystemId(parameter));
        }

        [Theory]
        [InlineAutoMoqData("J120450", "C4")]
        public void GetSystemClass_ValidSystemName_ReturnsSystemClass(string systemName, string systemClass, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemClass(systemName);

            Assert.NotNull(result);
            Assert.Equal(systemClass, result);
        }

        [Theory]
        [InlineAutoMoqData("Jita")]
        [InlineAutoMoqData("Amarr")]
        [InlineAutoMoqData("Hek")]
        [InlineAutoMoqData("NotASystem")]
        [InlineAutoMoqData("1231564")]
        [InlineAutoMoqData("")]
        public void GetSystemClass_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemClass(systemName);

            Assert.Null(result);
        }

        [Theory]
        [InlineAutoMoqData("J120450", "Red Giant")]
        public void GetSystemEffects_ValidSystemName_ReturnsSystemEffects(string systemName, string systemEffect, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemEffects(systemName);

            Assert.Equal(systemEffect, result);
        }

        [Theory]
        [InlineAutoMoqData("Jita")]
        [InlineAutoMoqData("Amarr")]
        [InlineAutoMoqData("Hek")]
        [InlineAutoMoqData("NotASystem")]
        [InlineAutoMoqData("1231564")]
        [InlineAutoMoqData("")]
        public void GetSystemEffects_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemEffects(systemName);

            Assert.Null(result);
        }

        [Theory]
        [InlineAutoMoqData("J120450", "H900", "X877")]
        public async Task GetSystemStatics_ValidSystemName_ReturnsSystemStatics(string systemName, string static1, string static2, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = await service.GetSystemStatics(systemName);

            Assert.Contains(static1, result.Select(x => x.Key));
            if (!string.IsNullOrEmpty(static2))
            {
                Assert.Contains(static2, result.Select(x => x.Key));
            }
        }

        [Theory]
        [InlineAutoMoqData("Jita")]
        [InlineAutoMoqData("Amarr")]
        [InlineAutoMoqData("NotASystem")]
        [InlineAutoMoqData("")]
        public async Task GetSystemStatics_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = await service.GetSystemStatics(systemName);
            Assert.Null(result);
        }

        [Theory]
        [InlineAutoMoqData("Pulsar", "C1", "Shield Capacity", "+30%")]
        [InlineAutoMoqData("Pulsar", "C2", "Shield Capacity", "+44%")]
        [InlineAutoMoqData("Pulsar", "C3", "Shield Capacity", "+58%")]
        [InlineAutoMoqData("Pulsar", "C4", "Shield Capacity", "+72%")]
        [InlineAutoMoqData("Pulsar", "C5", "Shield Capacity", "+86%")]
        [InlineAutoMoqData("Pulsar", "C6", "Shield Capacity", "+100%")]
        public void GetSystemEffectsInfos_ValidInputs_ReturnsSystemEffectsInfos(string systemEffect, string systemClass, string shipEffect, string strength, ILogger<AnoikServices> logger, Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            SetupMockDataSupplier(anoikDataSupplier);
            var service = new AnoikServices(logger, anoikDataSupplier.Object);

            var result = service.GetSystemEffectsInfos(systemEffect, systemClass);

            Assert.NotNull(result);
            var effects = new Dictionary<string, string>(result);
            Assert.Equal(strength, effects[shipEffect]);
        }

        private static void SetupMockDataSupplier(Mock<IAnoikDataSupplier> anoikDataSupplier)
        {
            anoikDataSupplier.Setup(x => x.GetSystems()).Returns(AnoikServiceTestConstants.GetElement("systems"));
            anoikDataSupplier.Setup(x => x.GetEffects()).Returns(AnoikServiceTestConstants.GetElement("effects"));
            anoikDataSupplier.Setup(x => x.GetWormHoles()).Returns(AnoikServiceTestConstants.GetElement("wormholes"));
        }
    }
}
