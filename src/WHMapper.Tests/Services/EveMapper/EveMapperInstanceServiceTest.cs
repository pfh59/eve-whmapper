using System.Threading.Tasks;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveMapper;
using Xunit;
using Microsoft.Extensions.Logging;

namespace WHMapper.Tests.Services.EveMapper
{
    public class EveMapperInstanceServiceTest
    {
        private readonly Mock<IWHInstanceRepository> _instanceRepoMock;
        private readonly Mock<IWHMapRepository> _mapRepoMock;
        private readonly Mock<IWHMapAccessRepository> _mapAccessRepoMock;
        private readonly Mock<ILogger<EveMapperInstanceService>> _loggerMock;
        private readonly EveMapperInstanceService _service;

        public EveMapperInstanceServiceTest()
        {
            _instanceRepoMock = new Mock<IWHInstanceRepository>();
            _mapRepoMock = new Mock<IWHMapRepository>();
            _mapAccessRepoMock = new Mock<IWHMapAccessRepository>();
            _loggerMock = new Mock<ILogger<EveMapperInstanceService>>();

            _service = new EveMapperInstanceService(
                _loggerMock.Object,
                _instanceRepoMock.Object,
                _mapRepoMock.Object,
                _mapAccessRepoMock.Object
            );
        }

        [Fact]
        public async Task GetInstanceAsync_ReturnsInstance_WhenExists()
        {
            // Arrange
            var instanceId = 1;
            var expectedInstance = new WHInstance("TestInstance", 100, "OwnerName", WHAccessEntity.Character, 200, "CreatorName")
            {
                Id = instanceId
            };
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(expectedInstance);

            // Act
            var result = await _service.GetInstanceAsync(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(instanceId, result.Id);
            Assert.Equal("TestInstance", result.Name);
            _instanceRepoMock.Verify(r => r.GetById(instanceId), Times.Once);
        }

        [Fact]
        public async Task GetInstanceAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var instanceId = 2;
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.GetInstanceAsync(instanceId);

            // Assert
            Assert.Null(result);
            _instanceRepoMock.Verify(r => r.GetById(instanceId), Times.Once);
        }

                [Fact]
        public async Task CreateInstanceAsync_ReturnsNull_WhenInstanceAlreadyExists()
        {
            // Arrange
            var ownerEntityId = 100;
            var existingInstance = new WHInstance("Existing", ownerEntityId, "Owner", WHAccessEntity.Character, 200, "Creator") { Id = 1 };
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync(existingInstance);

            // Act
            var result = await _service.CreateInstanceAsync("New", null, ownerEntityId, "Owner", WHAccessEntity.Character, 200, "Creator");

            // Assert
            Assert.Null(result);
            _instanceRepoMock.Verify(r => r.GetByOwnerAsync(ownerEntityId), Times.Once);
        }

        [Fact]
        public async Task CreateInstanceAsync_ReturnsInstance_WhenCreatedSuccessfully()
        {
            // Arrange
            var ownerEntityId = 101;
            var instance = new WHInstance("New", ownerEntityId, "Owner", WHAccessEntity.Character, 201, "Creator") { Id = 2 };
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync((WHInstance?)null);
            _instanceRepoMock.Setup(r => r.Create(It.IsAny<WHInstance>())).ReturnsAsync(instance);
            _instanceRepoMock.Setup(r => r.AddInstanceAdminAsync(instance.Id, 201, "Creator", true)).ReturnsAsync(new Mock<WHInstanceAdmin>().Object);
            _instanceRepoMock.Setup(r => r.AddInstanceAccessAsync(It.IsAny<WHInstanceAccess>())).ReturnsAsync(new WHInstanceAccess(instance.Id, ownerEntityId, "Owner", WHAccessEntity.Character));

            // Act
            var result = await _service.CreateInstanceAsync("New", "desc", ownerEntityId, "Owner", WHAccessEntity.Character, 201, "Creator");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(instance.Id, result.Id);
            _instanceRepoMock.Verify(r => r.GetByOwnerAsync(ownerEntityId), Times.Once);
            _instanceRepoMock.Verify(r => r.Create(It.IsAny<WHInstance>()), Times.Once);
            _instanceRepoMock.Verify(r => r.AddInstanceAdminAsync(instance.Id, 201, "Creator", true), Times.Once);
            _instanceRepoMock.Verify(r => r.AddInstanceAccessAsync(It.IsAny<WHInstanceAccess>()), Times.Once);
        }

        [Fact]
        public async Task CreateInstanceAsync_RollsBack_WhenAdminCreationFails()
        {
            // Arrange
            var ownerEntityId = 102;
            var instance = new WHInstance("New", ownerEntityId, "Owner", WHAccessEntity.Character, 202, "Creator") { Id = 3 };
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync((WHInstance?)null);
            _instanceRepoMock.Setup(r => r.Create(It.IsAny<WHInstance>())).ReturnsAsync(instance);
            _instanceRepoMock.Setup(r => r.AddInstanceAdminAsync(instance.Id, 202, "Creator", true)).ReturnsAsync((WHInstanceAdmin?)null);
            _instanceRepoMock.Setup(r => r.DeleteById(instance.Id)).ReturnsAsync(true);

            // Act
            var result = await _service.CreateInstanceAsync("New", null, ownerEntityId, "Owner", WHAccessEntity.Character, 202, "Creator");

            // Assert
            Assert.Null(result);
            _instanceRepoMock.Verify(r => r.DeleteById(instance.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateInstanceAsync_UpdatesNameAndDescription()
        {
            // Arrange
            var instanceId = 4;
            var instance = new WHInstance("Old", 103, "Owner", WHAccessEntity.Character, 203, "Creator") { Id = instanceId };
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _instanceRepoMock.Setup(r => r.Update(instanceId, instance)).ReturnsAsync(instance);

            // Act
            var result = await _service.UpdateInstanceAsync(instanceId, "Updated", "NewDesc");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
            Assert.Equal("NewDesc", result.Description);
            _instanceRepoMock.Verify(r => r.Update(instanceId, instance), Times.Once);
        }

        [Fact]
        public async Task UpdateInstanceAsync_ReturnsNull_WhenInstanceNotFound()
        {
            // Arrange
            var instanceId = 5;
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.UpdateInstanceAsync(instanceId, "Name", "Desc");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteInstanceAsync_ReturnsFalse_WhenNotOwner()
        {
            // Arrange
            var instanceId = 6;
            var requestingCharacterId = 300;
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(new WHInstance("Name", 104, "Owner", WHAccessEntity.Character, 301, "Creator") { Id = instanceId });

            // Act
            var result = await _service.DeleteInstanceAsync(instanceId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteInstanceAsync_Deletes_WhenOwner()
        {
            // Arrange
            var instanceId = 7;
            var creatorCharacterId = 302;
            var instance = new WHInstance("Name", 105, "Owner", WHAccessEntity.Character, creatorCharacterId, "Creator") { Id = instanceId };
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _instanceRepoMock.Setup(r => r.DeleteById(instanceId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteInstanceAsync(instanceId, creatorCharacterId);

            // Assert
            Assert.True(result);
            _instanceRepoMock.Verify(r => r.DeleteById(instanceId), Times.Once);
        }

        [Fact]
        public async Task AddAdminAsync_ReturnsNull_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 8;
            var requestingCharacterId = 400;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.AddAdminAsync(instanceId, 401, "NewAdmin", requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAdminAsync_AddsAdmin_WhenRequesterIsAdmin()
        {
            // Arrange
            var instanceId = 9;
            var requestingCharacterId = 500;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.AddInstanceAdminAsync(instanceId, 501, "NewAdmin", false)).ReturnsAsync(new Mock<WHInstanceAdmin>().Object);

            // Act
            var result = await _service.AddAdminAsync(instanceId, 501, "NewAdmin", requestingCharacterId);

            // Assert
            Assert.NotNull(result);
            _instanceRepoMock.Verify(r => r.AddInstanceAdminAsync(instanceId, 501, "NewAdmin", false), Times.Once);
        }

        [Fact]
        public async Task RemoveAdminAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 10;
            var requestingCharacterId = 600;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.RemoveAdminAsync(instanceId, 601, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAdminAsync_RemovesAdmin_WhenRequesterIsAdmin()
        {
            // Arrange
            var instanceId = 11;
            var requestingCharacterId = 700;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.RemoveInstanceAdminAsync(instanceId, 701)).ReturnsAsync(true);

            // Act
            var result = await _service.RemoveAdminAsync(instanceId, 701, requestingCharacterId);

            // Assert
            Assert.True(result);
            _instanceRepoMock.Verify(r => r.RemoveInstanceAdminAsync(instanceId, 701), Times.Once);
        }

        [Fact]
        public async Task AddAccessAsync_ReturnsNull_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 12;
            var requestingCharacterId = 800;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.AddAccessAsync(instanceId, 801, "Entity", WHAccessEntity.Corporation, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAccessAsync_AddsAccess_WhenRequesterIsAdmin()
        {
            // Arrange
            var instanceId = 13;
            var requestingCharacterId = 900;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.AddInstanceAccessAsync(It.IsAny<WHInstanceAccess>())).ReturnsAsync(new WHInstanceAccess(instanceId, 901, "Entity", WHAccessEntity.Corporation));

            // Act
            var result = await _service.AddAccessAsync(instanceId, 901, "Entity", WHAccessEntity.Corporation, requestingCharacterId);

            // Assert
            Assert.NotNull(result);
            _instanceRepoMock.Verify(r => r.AddInstanceAccessAsync(It.IsAny<WHInstanceAccess>()), Times.Once);
        }

        [Fact]
        public async Task RemoveAccessAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 14;
            var requestingCharacterId = 1000;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var (success, removed) = await _service.RemoveAccessAsync(instanceId, 1001, requestingCharacterId);

            // Assert
            Assert.False(success);
            Assert.Empty(removed);
        }

        [Fact]
        public async Task RemoveAccessAsync_ReturnsFalse_WhenAccessNotFound()
        {
            // Arrange
            var instanceId = 15;
            var requestingCharacterId = 1100;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.GetInstanceAccessesAsync(instanceId)).ReturnsAsync(new List<WHInstanceAccess>());

            // Act
            var (success, removed) = await _service.RemoveAccessAsync(instanceId, 1101, requestingCharacterId);

            // Assert
            Assert.False(success);
            Assert.Empty(removed);
        }

        [Fact]
        public async Task RemoveAccessAsync_RemovesAccessAndMapAccesses_WhenAdmin()
        {
            // Arrange
            var instanceId = 16;
            var requestingCharacterId = 1200;
            var accessId = 1201;
            var access = new WHInstanceAccess(instanceId, 1202, "Entity", WHAccessEntity.Corporation) { Id = accessId };
            var removedMapAccesses = new Dictionary<int, IEnumerable<int>> { { 1, new List<int> { 2, 3 } } };

            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.GetInstanceAccessesAsync(instanceId)).ReturnsAsync(new List<WHInstanceAccess> { access });
            _mapAccessRepoMock.Setup(r => r.RemoveMapAccessesByEntityAsync(instanceId, access.EveEntityId, access.EveEntity)).ReturnsAsync(removedMapAccesses);
            _instanceRepoMock.Setup(r => r.RemoveInstanceAccessAsync(instanceId, accessId)).ReturnsAsync(true);

            // Act
            var (success, removed) = await _service.RemoveAccessAsync(instanceId, accessId, requestingCharacterId);

            // Assert
            Assert.True(success);
            Assert.Equal(removedMapAccesses, removed);
        }

        [Fact]
        public async Task CanRegisterAsync_ReturnsFalse_WhenInstanceExists()
        {
            // Arrange
            var ownerEntityId = 1300;
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync(new WHInstance("Name", ownerEntityId, "Owner", WHAccessEntity.Character, 1301, "Creator"));

            // Act
            var result = await _service.CanRegisterAsync(ownerEntityId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanRegisterAsync_ReturnsTrue_WhenNoInstanceExists()
        {
            // Arrange
            var ownerEntityId = 1400;
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.CanRegisterAsync(ownerEntityId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsOwnerAsync_ReturnsTrue_WhenCreatorMatches()
        {
            // Arrange
            var instanceId = 1500;
            var characterId = 1501;
            var instance = new WHInstance("Name", 1502, "Owner", WHAccessEntity.Character, characterId, "Creator") { Id = instanceId };
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);

            // Act
            var result = await _service.IsOwnerAsync(instanceId, characterId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsOwnerAsync_ReturnsFalse_WhenCreatorDoesNotMatch()
        {
            // Arrange
            var instanceId = 1600;
            var characterId = 1601;
            var instance = new WHInstance("Name", 1602, "Owner", WHAccessEntity.Character, 1603, "Creator") { Id = instanceId };
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);

            // Act
            var result = await _service.IsOwnerAsync(instanceId, characterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsOwnerAsync_ReturnsFalse_WhenInstanceNotFound()
        {
            // Arrange
            var instanceId = 1700;
            var characterId = 1701;
            _instanceRepoMock.Setup(r => r.GetById(instanceId)).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.IsOwnerAsync(instanceId, characterId);

            // Assert
            Assert.False(result);
        }
    }
}