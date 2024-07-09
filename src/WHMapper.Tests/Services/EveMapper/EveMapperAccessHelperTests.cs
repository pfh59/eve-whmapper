using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;
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
        public async Task IfAccessRepositoryIsEmpty_WhenGettingState_ReturnsTrue(
            int characterId,
            [Frozen] Mock<IWHAccessRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            var accessRepositoryReturn = Task.FromResult(new List<WHAccess>() as IEnumerable<WHAccess>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }

        [Theory, AutoDomainData]
        public async Task IfWHAccessExistsAndCharacterIsNull_WhenGettingAccess_ReturnsFalse(
            WHAccess wHAccess,
            [Frozen] Mock<IWHAccessRepository> accessRepository,
            [Frozen] Mock<ICharacterServices> characterServices,
            EveMapperAccessHelper sut
            )
        {
            var accessRepositoryReturn = Task.FromResult(new List<WHAccess>() { wHAccess } as IEnumerable<WHAccess>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Character? characterReturn = null!;
            characterServices.Setup(x => x.GetCharacter(It.IsAny<int>())).Returns(Task.FromResult<Character?>(characterReturn));

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(1));
        }

        [Theory, AutoDomainData]
        public async Task IfWHAccessExistsAndCharacterNotExists_WhenGettingAccess_ReturnsFalse(
            WHAccess wHAccess,
            [Frozen] Mock<IWHAccessRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            var accessRepositoryReturn = Task.FromResult(new List<WHAccess>() { wHAccess } as IEnumerable<WHAccess>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.False(await sut.IsEveMapperUserAccessAuthorized(1));
        }

        [Theory, AutoDomainData]
        public async Task IfWHAccessExistsAndCharacterExists_WhenGettingAccess_ReturnsTrue(
            WHAccess wHAccess,
            [Frozen] Mock<IWHAccessRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            wHAccess.EveEntityId = 2;
            var accessRepositoryReturn = Task.FromResult(new List<WHAccess>() { wHAccess } as IEnumerable<WHAccess>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(2));
        }
        #endregion

        #region IsEveMapperAdminAccessAuthorized()
        [Theory]
        [InlineAutoMoqData(1)]
        [InlineAutoMoqData(10000)]
        [InlineAutoMoqData(int.MinValue)]
        [InlineAutoMoqData(int.MaxValue)]
        public async Task IfAccessRepositoryIsEmpty_WhenGettingAdminState_ReturnsTrue(
            int characterId,
            [Frozen] Mock<IWHAdminRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            var accessRepositoryReturn = Task.FromResult(new List<WHAdmin>() as IEnumerable<WHAdmin>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }

        [Theory]
        [InlineAutoMoqData(1)]
        [InlineAutoMoqData(10000)]
        [InlineAutoMoqData(int.MinValue)]
        [InlineAutoMoqData(int.MaxValue)]
        public async Task IfAccessRepositoryIsPopulatedGettingUnknownCharacterId_WhenGettingAdminState_ReturnsFalse(
            int characterId,
            WHAdmin wHAdmin,
            [Frozen] Mock<IWHAdminRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            wHAdmin.EveCharacterId = 2;
            var accessRepositoryReturn = Task.FromResult(new List<WHAdmin>() { wHAdmin } as IEnumerable<WHAdmin>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }

        [Theory]
        [InlineAutoMoqData(1)]
        [InlineAutoMoqData(10000)]
        [InlineAutoMoqData(int.MinValue)]
        [InlineAutoMoqData(int.MaxValue)]
        public async Task IfAccessRepositoryIsPopulatedGettingKnownCharacterId_WhenGettingAdminState_ReturnsFalse(
            int characterId,
            WHAdmin wHAdmin,
            [Frozen] Mock<IWHAdminRepository> accessRepository,
            EveMapperAccessHelper sut
            )
        {
            wHAdmin.EveCharacterId = characterId;
            var accessRepositoryReturn = Task.FromResult(new List<WHAdmin>() { wHAdmin } as IEnumerable<WHAdmin>);
            accessRepository.Setup(x => x.GetAll()).Returns(accessRepositoryReturn!);

            Assert.True(await sut.IsEveMapperUserAccessAuthorized(characterId));
        }
        #endregion
    }
}
