using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Repositories.WHInstances;
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
            [Frozen] Mock<IWHInstanceRepository> instanceRepository,
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

            Assert.True(await sut.IsEveMapperMapAccessAuthorized(1, map.Id));
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
    }
}