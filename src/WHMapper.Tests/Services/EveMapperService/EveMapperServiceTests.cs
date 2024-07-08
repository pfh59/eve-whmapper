using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.Cache;
using WHMapper.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapperService
{
    public class EveMapperServiceTests
    {
        [Theory]
        [InlineAutoMoqData(typeof(AllianceEntity), IEveMapperCacheService.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(typeof(CorporationEntity), IEveMapperCacheService.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(typeof(CharactereEntity), IEveMapperCacheService.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(typeof(ShipEntity), IEveMapperCacheService.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(typeof(SystemEntity), IEveMapperCacheService.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(typeof(ConstellationEntity), IEveMapperCacheService.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(typeof(RegionEntity), IEveMapperCacheService.REDIS_REGION_KEY)]
        [InlineAutoMoqData(typeof(StargateEntity), IEveMapperCacheService.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(typeof(GroupEntity), IEveMapperCacheService.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(typeof(WHEntity), IEveMapperCacheService.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(typeof(SunEntity), IEveMapperCacheService.REDIS_SUN_KEY)]
        public async Task IfEntityIsMapped_WhenClearingCache_CallsRemoveForCorrectEntity(
            Type entityType,
            string redisKey,
            [Frozen] Mock<ICacheService> cacheService,
            EveMapperCacheService sut
            )
        {
            cacheService.Setup(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var sutMethod = sut.GetType().GetMethod("ClearCacheAsync")!.MakeGenericMethod(entityType);

            var result = await (Task<bool>)sutMethod.Invoke(sut, null)!;

            Assert.True(result);
            cacheService.Verify(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))), Times.Once);
            cacheService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData(typeof(AllianceEntity), IEveMapperCacheService.REDIS_ALLIANCE_KEY)]
        [InlineAutoMoqData(typeof(CorporationEntity), IEveMapperCacheService.REDIS_COORPORATION_KEY)]
        [InlineAutoMoqData(typeof(CharactereEntity), IEveMapperCacheService.REDIS_CHARACTER_KEY)]
        [InlineAutoMoqData(typeof(ShipEntity), IEveMapperCacheService.REDIS_SHIP_KEY)]
        [InlineAutoMoqData(typeof(SystemEntity), IEveMapperCacheService.REDIS_SYSTEM_KEY)]
        [InlineAutoMoqData(typeof(ConstellationEntity), IEveMapperCacheService.REDIS_CONSTELLATION_KEY)]
        [InlineAutoMoqData(typeof(RegionEntity), IEveMapperCacheService.REDIS_REGION_KEY)]
        [InlineAutoMoqData(typeof(StargateEntity), IEveMapperCacheService.REDIS_STARTGATE_KEY)]
        [InlineAutoMoqData(typeof(GroupEntity), IEveMapperCacheService.REDIS_GROUP_KEY)]
        [InlineAutoMoqData(typeof(WHEntity), IEveMapperCacheService.REDIS_WORMHOLE_KEY)]
        [InlineAutoMoqData(typeof(SunEntity), IEveMapperCacheService.REDIS_SUN_KEY)]
        public async Task IfCacheThrowsException_WhenClearingCache_ReturnsFalse(
            Type entityType,
            string redisKey,
            [Frozen] Mock<ICacheService> cacheService,
            EveMapperCacheService sut
            )
        {
            cacheService.Setup(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))))
                .Throws<Exception>()
                .Verifiable();

            var sutMethod = sut.GetType().GetMethod("ClearCacheAsync")!.MakeGenericMethod(entityType);
            var result = await (Task<bool>)sutMethod.Invoke(sut, null)!;

            Assert.False(result);
            cacheService.Verify(x => x.Remove(It.Is<String>(x => x.Contains(redisKey))), Times.Once);
            cacheService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData(typeof(AEveEntity))]
        public async Task IfEntityIsNotMapped_WhenClearingCache_ReturnsFalse(
            Type entityType,
            [Frozen] Mock<ICacheService> cacheService,
            EveMapperCacheService sut
        )
        {
            cacheService.Setup(x => x.Remove(It.IsAny<string>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var sutMethod = sut.GetType().GetMethod("ClearCacheAsync")!.MakeGenericMethod(entityType);
            var result = await (Task<bool>)sutMethod.Invoke(sut, null)!;

            Assert.False(result);
            cacheService.VerifyNoOtherCalls();
        }
    }
}
