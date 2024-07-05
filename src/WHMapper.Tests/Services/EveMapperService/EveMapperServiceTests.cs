using AutoFixture.Xunit2;
using Moq;
using WHMapper.Services.Cache;
using WHMapper.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapperService
{
    public class EveMapperServiceTests
    {
        [Theory]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearAllianceCache), IEveMapperService.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearCharacterCache), IEveMapperService.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearCorporationCache), IEveMapperService.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearShipCache), IEveMapperService.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearSystemCache), IEveMapperService.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearConstellationCache), IEveMapperService.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearRegionCache), IEveMapperService.REDIS_REGION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearStargateCache), IEveMapperService.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearGroupCache), IEveMapperService.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearWormholeCache), IEveMapperService.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearSunCache), IEveMapperService.REDIS_SUN_KEY)]
        public async Task IfCacheIsEmpty_WhenClearingCache_CallsRemoveForCorrectEntity(
            string methodName,
            string redisKey,
            [Frozen]Mock<ICacheService> cacheService,
            WHMapper.Services.EveMapper.EveMapperService sut
            )
        {
            cacheService.Setup(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var method = sut.GetType().GetMethod(methodName);
            if (method == null)
                throw new Exception("Method not found");

            var task = (Task<bool>)method.Invoke(sut, null)!;
            var result = await task;

            Assert.True(result);
            cacheService.Verify(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))), Times.Once);
            cacheService.VerifyNoOtherCalls();
        }              
        
        [Theory]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearAllianceCache), IEveMapperService.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearCharacterCache), IEveMapperService.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearCorporationCache), IEveMapperService.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearShipCache), IEveMapperService.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearSystemCache), IEveMapperService.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearConstellationCache), IEveMapperService.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearRegionCache), IEveMapperService.REDIS_REGION_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearStargateCache), IEveMapperService.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearGroupCache), IEveMapperService.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearWormholeCache), IEveMapperService.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(nameof(WHMapper.Services.EveMapper.EveMapperService.ClearSunCache), IEveMapperService.REDIS_SUN_KEY)]
        public async Task IfCacheThrowsException_WhenClearingCache_ReturnsFalse(
            string methodName,
            string redisKey,
            [Frozen]Mock<ICacheService> cacheService,
            WHMapper.Services.EveMapper.EveMapperService sut
            )
        {
            cacheService
                .Setup(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))))
                .Throws<Exception>()
                .Verifiable();

            var method = sut.GetType().GetMethod(methodName);
            if (method == null)
                throw new Exception("Method not found");

            var task = (Task<bool>)method.Invoke(sut, null)!;
            var result = await task;

            Assert.False(result);
            cacheService.Verify(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))), Times.Once);
            cacheService.VerifyNoOtherCalls();
        }              
    }
}
