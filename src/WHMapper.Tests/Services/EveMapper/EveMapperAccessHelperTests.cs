using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;
using Xunit;

namespace WHMapper.Tests.Services.EveMapper
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }))
        {
        }
    }

    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] values) 
            : base(new AutoMoqDataAttribute(), values)
        {
        }
    }

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
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));
            
            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance>());

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }

        [Theory, AutoMoqData]
        public async Task IfInstanceExistsAndUserHasAccess_WhenGettingAccess_ReturnsTrue(
            WHInstance instance,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));
            
            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance> { instance });

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(1));
        }

        [Theory, AutoMoqData]
        public async Task IfCharacterNotFound_WhenGettingAccess_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut)
        {
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Failure("Character not found"));

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
            EveMapperAccessHelper sut)
        {
            instanceRepository.Setup(x => x.GetInstancesForAdminAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<WHInstance>());

            Assert.False(await sut.IsEveMapperAdminAccessAuthorized(characterId));
        }

        [Theory, AutoMoqData]
        public async Task IfUserIsAdmin_WhenGettingAdminState_ReturnsTrue(
            WHInstance instance,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            instanceRepository.Setup(x => x.GetInstancesForAdminAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<WHInstance> { instance });

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
            EveMapperAccessHelper sut)
        {
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync((WHMap?)null);

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(characterId, mapId));
        }

        [Theory, AutoMoqData]
        public async Task IfMapExistsAndUserHasInstanceAccess_WhenGettingMapAccess_ReturnsTrue(
            WHMap map,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<IWHMapAccessRepository> mapAccessRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            mapAccessRepository.Setup(x => x.HasMapAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            Assert.True(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }

        [Theory, AutoMoqData]
        public async Task IfCharacterNotFound_WhenGettingMapAccess_ReturnsFalse(
            WHMap map,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Failure("Character not found"));

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }

        [Theory, AutoMoqData]
        public async Task IfUserHasNoInstanceAccess_WhenGettingMapAccess_ReturnsFalse(
            WHMap map,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }

        [Theory, AutoMoqData]
        public async Task IfUserHasInstanceAccessButNoMapAccess_WhenGettingMapAccess_ReturnsFalse(
            WHMap map,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<IWHMapAccessRepository> mapAccessRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            mapAccessRepository.Setup(x => x.HasMapAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }

        [Theory, AutoMoqData]
        public async Task IfMapHasNoInstance_WhenGettingMapAccess_ReturnsFalse(
            WHMap map,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = null;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            Assert.False(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
        }
        #endregion

        #region IsEveMapperUserAccessAuthorizedForAny()
        [Theory, AutoMoqData]
        public async Task IfNoCharacterIds_WhenGettingAccessForAny_ReturnsFalse(
            EveMapperAccessHelper sut)
        {
            Assert.False(await sut.IsEveMapperUserAccessAuthorizedForAny(Array.Empty<int>()));
            Assert.False(await sut.IsEveMapperUserAccessAuthorizedForAny(null!));
        }

        [Theory, AutoMoqData]
        public async Task IfOneCharacterHasAccess_WhenGettingAccessForAny_ReturnsTrue(
            WHInstance instance,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            // First character has no access, second has access
            instanceRepository.SetupSequence(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance>())
                .ReturnsAsync(new List<WHInstance> { instance });

            var characterIds = new List<int> { 1, 2 };
            Assert.True(await sut.IsEveMapperUserAccessAuthorizedForAny(characterIds));
        }

        [Theory, AutoMoqData]
        public async Task IfNoCharacterHasAccess_WhenGettingAccessForAny_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance>());

            var characterIds = new List<int> { 1, 2, 3 };
            Assert.False(await sut.IsEveMapperUserAccessAuthorizedForAny(characterIds));
        }
        #endregion

        #region IsEveMapperInstanceAccessAuthorized()
        [Theory, AutoMoqData]
        public async Task IfCharacterNotFound_WhenGettingInstanceAccess_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut)
        {
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Failure("Character not found"));

            Assert.False(await sut.IsEveMapperInstanceAccessAuthorized(1, 1));
        }

        [Theory, AutoMoqData]
        public async Task IfUserHasAccess_WhenGettingInstanceAccess_ReturnsTrue(
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(1, 1, 100, 200))
                .ReturnsAsync(true);

            Assert.True(await sut.IsEveMapperInstanceAccessAuthorized(1, 1));
        }

        [Theory, AutoMoqData]
        public async Task IfUserHasNoCorpOrAllianceAndNoAccess_WhenGettingInstanceAccess_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 0, AllianceId = 0 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            Assert.False(await sut.IsEveMapperInstanceAccessAuthorized(1, 1));
            instanceRepository.Verify(x => x.HasInstanceAccessAsync(1, 1, null, null), Times.Once);
        }
        #endregion

        #region GetAccessibleInstanceIdsAsync()
        [Theory, AutoMoqData]
        public async Task IfCharacterNotFound_WhenGettingAccessibleInstanceIds_ReturnsEmpty(
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut)
        {
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Failure("Character not found"));

            Assert.Empty(await sut.GetAccessibleInstanceIdsAsync(1));
        }

        [Theory, AutoMoqData]
        public async Task IfInstancesAccessible_WhenGettingAccessibleInstanceIds_ReturnsTheirIds(
            WHInstance instance1,
            WHInstance instance2,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance> { instance1, instance2 });

            var ids = await sut.GetAccessibleInstanceIdsAsync(1);

            Assert.Equal(new[] { instance1.Id, instance2.Id }, ids);
        }

        [Theory, AutoMoqData]
        public async Task IfRepositoryReturnsNull_WhenGettingAccessibleInstanceIds_ReturnsEmpty(
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((IEnumerable<WHInstance>?)null);

            Assert.Empty(await sut.GetAccessibleInstanceIdsAsync(1));
        }
        #endregion

        #region IsInstanceAdminAuthorized()
        [Theory]
        [InlineAutoMoqData(true)]
        [InlineAutoMoqData(false)]
        public async Task WhenCheckingInstanceAdmin_ReturnsRepositoryResult(
            bool isAdmin,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            instanceRepository.Setup(x => x.IsInstanceAdminAsync(2, 1))
                .ReturnsAsync(isAdmin);

            Assert.Equal(isAdmin, await sut.IsInstanceAdminAuthorized(1, 2));
        }
        #endregion

        #region GetMapInstanceIdAsync()
        [Theory, AutoMoqData]
        public async Task IfMapDoesNotExist_WhenGettingMapInstanceId_ReturnsNull(
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut)
        {
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync((WHMap?)null);

            Assert.Null(await sut.GetMapInstanceIdAsync(1));
        }

        [Theory, AutoMoqData]
        public async Task IfMapExists_WhenGettingMapInstanceId_ReturnsItsInstanceId(
            WHMap map,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 42;
            mapRepository.Setup(x => x.GetById(map.Id))
                .ReturnsAsync(map);

            Assert.Equal(42, await sut.GetMapInstanceIdAsync(map.Id));
        }
        #endregion

        #region Corp/alliance id branches
        [Theory, AutoMoqData]
        public async Task IfRepositoryReturnsNull_WhenGettingAccess_ReturnsFalse(
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 100, AllianceId = 200 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((IEnumerable<WHInstance>?)null);

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(1));
        }

        [Theory, AutoMoqData]
        public async Task IfRepositoryReturnsNull_WhenGettingAdminState_ReturnsFalse(
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            instanceRepository.Setup(x => x.GetInstancesForAdminAsync(It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<WHInstance>?)null);

            Assert.False(await sut.IsEveMapperAdminAccessAuthorized(1));
        }

        [Theory, AutoMoqData]
        public async Task IfCharacterHasNoCorpOrAlliance_WhenGettingMapAccess_PassesNullIds(
            WHMap map,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHMapRepository> mapRepository,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            [Frozen] Mock<IWHMapAccessRepository> mapAccessRepository,
            EveMapperAccessHelper sut)
        {
            map.WHInstanceId = 1;
            mapRepository.Setup(x => x.GetById(It.IsAny<int>()))
                .ReturnsAsync(map);

            var character = new Character() { CorporationId = 0, AllianceId = 0 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.HasInstanceAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            mapAccessRepository.Setup(x => x.HasMapAccessAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            Assert.True(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
            instanceRepository.Verify(x => x.HasInstanceAccessAsync(1, 1, null, null), Times.Once);
            mapAccessRepository.Verify(x => x.HasMapAccessAsync(map.Id, 1, null, null), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task IfCharacterHasNoCorpOrAlliance_WhenGettingAccessibleInstanceIds_PassesNullIds(
            WHInstance instance,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 0, AllianceId = 0 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance> { instance });

            Assert.Equal(new[] { instance.Id }, await sut.GetAccessibleInstanceIdsAsync(1));
            instanceRepository.Verify(x => x.GetAccessibleInstancesAsync(1, null, null), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task IfCharacterHasNoCorpOrAlliance_WhenGettingAccess_PassesNullIds(
            WHInstance instance,
            [Frozen] Mock<ICharacterServices> characterServices,
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
            EveMapperAccessHelper sut)
        {
            var character = new Character() { CorporationId = 0, AllianceId = 0 };
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>()))
                .ReturnsAsync(Result<Character>.Success(character));

            instanceRepository.Setup(x => x.GetAccessibleInstancesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WHInstance> { instance });

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(1));
            instanceRepository.Verify(x => x.GetAccessibleInstancesAsync(1, null, null), Times.Once);
        }
        #endregion
    }
}