using AutoFixture.Xunit2;
using Moq;
using WHMapper.Shared.Models.DTO.EveMapper.EveEntity;
using WHMapper.Shared.Services.EveAPI;
using WHMapper.Shared.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapper
{
    public class EveMapperServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task CacheContainsGroup_WhenGettingGroup_ReturnsItemFromCache(
            [Frozen] Mock<IEveMapperCacheService> cacheService,
            [Frozen] Mock<IEveAPIServices> apiService,
            EveMapperService sut
            )
        {
            var groupEntity = new GroupEntity(1, new WHMapper.Shared.Models.DTO.EveAPI.Universe.Group(1, "1", true, 1, []));
            cacheService.Setup(x => x.GetAsync<GroupEntity>(1))
                .ReturnsAsync(groupEntity)
                .Verifiable();

            var result = await sut.GetGroup(1);

            Assert.NotNull(result);
            Assert.Equal(groupEntity, result);
            cacheService.Verify(x => x.GetAsync<GroupEntity>(1), Times.Once);
            cacheService.VerifyNoOtherCalls();
            apiService.VerifyNoOtherCalls();
        }        
        
        [Theory]
        [AutoMoqData]
        public async Task ApiReturnsGroup_WhenGettingGroup_ReturnsItemFromCache(
            [Frozen] Mock<IEveMapperCacheService> cacheService,
            [Frozen] Mock<IEveAPIServices> apiService,
            EveMapperService sut
            )
        {
            GroupEntity groupEntity = null!;
            cacheService.Setup(x => x.GetAsync<GroupEntity>(1))
                .ReturnsAsync(groupEntity)
                .Verifiable();

            var group = new WHMapper.Shared.Models.DTO.EveAPI.Universe.Group(1, "hi", true, 1, []);
            apiService.Setup(x => x.UniverseServices.GetGroup(1)).ReturnsAsync(group);

            var result = await sut.GetGroup(1);

            Assert.NotNull(result);
            cacheService.Verify(x => x.GetAsync<GroupEntity>(1), Times.Once);
            cacheService.Verify(x => x.AddAsync<GroupEntity>(It.IsAny<GroupEntity>()), Times.Once);
            cacheService.VerifyNoOtherCalls();

            apiService.Verify(x => x.UniverseServices.GetGroup(1), Times.Once);
            apiService.VerifyNoOtherCalls();
        }
    }
}
