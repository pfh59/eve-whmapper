using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using WHMapper.Models.DTO.Anoik;
using WHMapper.Services.Anoik;

namespace WHMapper.Tests.Services.Anoik
{
    public class AnoikServicesTests
    {
        [Theory]
        [InlineAutoMoqData("J120450", 31001554)]
        [InlineAutoMoqData("J100009", 31001040)]
        [InlineAutoMoqData("J100001", 31001753)]
        [InlineAutoMoqData("J164501", 31001181)]
        [InlineAutoMoqData("J164550", 31000123)]
        public void GetSystemId_ValidSystemName_ReturnsSystemId(string systemName, int systemId, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
            var result = service.GetSystemId(systemName);
            Assert.NotNull(result);
            Assert.Equal(systemId, result);
        }

        [Theory]
        [InlineAutoMoqData("Jita")]
        [InlineAutoMoqData("Amarr")]
        [InlineAutoMoqData("Hek")]
        [InlineAutoMoqData("NotASystem")]
        [InlineAutoMoqData("1231564")]
        [InlineAutoMoqData("")]
        public void GetSystemId_InvalidSystemName_ReturnsNull(string system, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
            var result = service.GetSystemId(system);
            Assert.Null(result);
        }

        [Theory, AutoDomainData]
        public void GetSystemId_InvalidParameter_ThrowsException(ILogger<AnoikServices> logger)
        {
            string parameter = null;
            var service = new AnoikServices(logger);
            Assert.ThrowsAny<ArgumentNullException>(() => service.GetSystemId(parameter));
        }

        [Theory]
        [InlineAutoMoqData("J120450", "C4")]
        [InlineAutoMoqData("J100009", "C3")]
        [InlineAutoMoqData("J100001", "C4")]
        [InlineAutoMoqData("J164501", "C3")]
        [InlineAutoMoqData("J164550", "C1")]
        public void GetSystemClass_ValidSystemName_ReturnsSystemClass(string systemName, string systemClass, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
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
        public void GetSystemClass_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
            var result = service.GetSystemClass(systemName);
            Assert.Null(result);
        }

        [Theory]
        [InlineAutoMoqData("J120450", "Red Giant")]
        [InlineAutoMoqData("J100009", "Pulsar")]
        [InlineAutoMoqData("J164550", "Magnetar")]
        [InlineAutoMoqData("J100001", "")]
        [InlineAutoMoqData("J164501", "")]
        public void GetSystemEffects_ValidSystemName_ReturnsSystemEffects(string systemName, string systemEffect, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
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
        public void GetSystemEffects_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
            var result = service.GetSystemEffects(systemName);
            Assert.Null(result);
        }

        [Theory]
        [InlineAutoMoqData("J120450", "H900", "X877")]
        [InlineAutoMoqData("J100009", "U210", "")]
        [InlineAutoMoqData("J164550", "J244", "")]
        [InlineAutoMoqData("J100001", "C247", "U574")]
        [InlineAutoMoqData("J164501", "U210", "")]
        public async Task GetSystemStatics_ValidSystemName_ReturnsSystemStatics(string systemName, string static1, string static2, Dictionary<string, string> statics, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
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
        [InlineAutoMoqData("Hek")]
        [InlineAutoMoqData("NotASystem")]
        [InlineAutoMoqData("1231564")]
        [InlineAutoMoqData("")]
        public async Task GetSystemStatics_InvalidSystemName_ReturnsNull(string systemName, ILogger<AnoikServices> logger)
        {
            var service = new AnoikServices(logger);
            var result = await service.GetSystemStatics(systemName);
            Assert.Null(result);
        }
    }
}
