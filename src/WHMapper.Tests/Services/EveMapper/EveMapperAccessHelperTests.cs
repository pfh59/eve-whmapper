using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapper
{
    public class EveMapperAccessHelperTests
    {
        #region IsEveMapperUserAccessAuthorized()
        [Theory]
        [InlineAutoMoqData(1)]
        [InlineAutoMoqData(10000)]
        [InlineAutoMoqData(int.MinValue)]
        [InlineAutoMoqData(int.MaxValue)]
        public async Task IfNoInstancesExist_WhenGettingState_ReturnsFalse(
            int characterId,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut
            )
        {
            // No instances exist, user should NOT have access
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Character>.Success(character)));
            
            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(Task.FromResult<IEnumerable<WHInstance>?>(new List<WHInstance>()));

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }

        [Theory, AutoDomainData]
        public async Task IfInstanceExistsAndUserHasAccess_WhenGettingAccess_ReturnsTrue(
            WHInstance instance,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut
            )
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Character>.Success(character)));
            
            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(Task.FromResult<IEnumerable<WHInstance>?>(new List<WHInstance> { instance }));

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(1));
        }

        [Theory, AutoDomainData]
        public async Task IfCharacterNotFound_WhenGettingAccess_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut
            )
        {
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Character>.Failure("Character not found")));

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(1));
        }
        #endregion

        #region IsEveMapperAdminAccessAuthorized()
        [Theory]
        [InlineAutoMoqData(1)]
        [InlineAutoMoqData(10000)]
        public async Task IfNoAdminInstances_WhenGettingAdminState_ReturnsFalse(
            int characterId,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut
            )
        {
            instanceRepository.Setup(x => x.GetInstancesForAdminAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<IEnumerable<WHInstance>?>(new List<WHInstance>()));

            Assert.False(await sut.IsEveMapperAdminAccessAuthorized(characterId));
        }

        [Theory, AutoDomainData]
        public async Task IfUserIsAdmin_WhenGettingAdminState_ReturnsTrue(
            WHInstance instance,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut
            )
        {
            instanceRepository.Setup(x => x.GetInstancesForAdminAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<IEnumerable<WHInstance>?>(new List<WHInstance> { instance }));

            Assert.True(await sut.IsEveMapperAdminAccessAuthorized(1));
        }
        #endregion

        #region IsEveMapperMapAccessAuthorized()
        [Theory]
        [InlineAutoMoqData(1, 1)]
        public async Task IfMapDoesNotExist_WhenGettingMapAccess_ReturnsFalse(
            int mapId,
            int characterId,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut
        )
        {
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Task.FromResult<WHMap?>(null));

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(characterId, mapId));
        }

        [Theory, AutoDomainData]
        public async Task IfMapExistsAndUserHasInstanceAccess_WhenGettingMapAccess_ReturnsTrue(
            WHMap map,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut
        )
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Task.FromResult<WHMap?>(map));

            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .Returns(Task.FromResult(Result<Character>.Success(character)));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(Task.FromResult(true));

            Assert.True(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }

        [Theory, AutoDomainData]
        public async Task IfMapHasNoInstance_WhenGettingMapAccess_ReturnsFalse(
            WHMap map,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut
        )
        {
            map.WHInstanceId = null;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Task.FromResult<WHMap?>(map));

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }
        #endregion

    }
}
