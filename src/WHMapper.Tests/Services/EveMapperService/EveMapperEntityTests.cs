using AutoFixture.Xunit2;
using Moq;
using WHMapper.Services.Cache;
using WHMapper.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapperService
{
    public class EveMapperEntityTests
    {
        [Theory]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearAllianceCache), IEveMapperEntity.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearCharacterCache), IEveMapperEntity.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearCorporationCache), IEveMapperEntity.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearShipCache), IEveMapperEntity.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearSystemCache), IEveMapperEntity.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearConstellationCache), IEveMapperEntity.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearRegionCache), IEveMapperEntity.REDIS_REGION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearStargateCache), IEveMapperEntity.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearGroupCache), IEveMapperEntity.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearWormholeCache), IEveMapperEntity.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearSunCache), IEveMapperEntity.REDIS_SUN_KEY)]
        public async Task IfCacheIsEmpty_WhenClearingCache_CallsRemoveForCorrectEntity(
            string methodName,
            string redisKey,
            [Frozen]Mock<ICacheService> cacheService,
            EveMapperEntity sut
            )
        {
            cacheService.Setup(x => x.Remove(It.Is<String>(x => x.Contains(redisKey)))).Verifiable();

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
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearAllianceCache), IEveMapperEntity.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearCharacterCache), IEveMapperEntity.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearCorporationCache), IEveMapperEntity.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearShipCache), IEveMapperEntity.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearSystemCache), IEveMapperEntity.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearConstellationCache), IEveMapperEntity.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearRegionCache), IEveMapperEntity.REDIS_REGION_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearStargateCache), IEveMapperEntity.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearGroupCache), IEveMapperEntity.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearWormholeCache), IEveMapperEntity.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(nameof(EveMapperEntity.ClearSunCache), IEveMapperEntity.REDIS_SUN_KEY)]
        public async Task IfCacheThrowsException_WhenClearingCache_ReturnsFalse(
            string methodName,
            string redisKey,
            [Frozen]Mock<ICacheService> cacheService,
            EveMapperEntity sut
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
