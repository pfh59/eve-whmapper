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

        [Fact]
        public async Task GetInstanceByOwnerAsync_ReturnsInstance_WhenExists()
        {
            // Arrange
            var ownerEntityId = 1800;
            var expectedInstance = new WHInstance("Test", ownerEntityId, "Owner", WHAccessEntity.Character, 1801, "Creator") { Id = 1 };
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync(expectedInstance);

            // Act
            var result = await _service.GetInstanceByOwnerAsync(ownerEntityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ownerEntityId, result.OwnerEveEntityId);
        }

        [Fact]
        public async Task GetInstanceByOwnerAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ownerEntityId = 1900;
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.GetInstanceByOwnerAsync(ownerEntityId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAdministeredInstancesAsync_ReturnsInstances()
        {
            // Arrange
            var characterId = 2000;
            var instances = new List<WHInstance>
            {
                new WHInstance("Instance1", 2001, "Owner1", WHAccessEntity.Character, characterId, "Creator") { Id = 1 },
                new WHInstance("Instance2", 2002, "Owner2", WHAccessEntity.Corporation, characterId, "Creator") { Id = 2 }
            };
            _instanceRepoMock.Setup(r => r.GetInstancesForAdminAsync(characterId)).ReturnsAsync(instances);

            // Act
            var result = await _service.GetAdministeredInstancesAsync(characterId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAccessibleInstancesAsync_ReturnsInstances()
        {
            // Arrange
            var characterId = 2100;
            int? corpId = 2101;
            int? allianceId = 2102;
            var instances = new List<WHInstance>
            {
                new WHInstance("Instance1", 2103, "Owner1", WHAccessEntity.Character, 2104, "Creator") { Id = 1 }
            };
            _instanceRepoMock.Setup(r => r.GetAccessibleInstancesAsync(characterId, corpId, allianceId)).ReturnsAsync(instances);

            // Act
            var result = await _service.GetAccessibleInstancesAsync(characterId, corpId, allianceId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAdminsAsync_ReturnsAdmins()
        {
            // Arrange
            var instanceId = 2200;
            var admins = new List<WHInstanceAdmin>
            {
                new WHInstanceAdmin(instanceId, 2201, "Admin1", true),
                new WHInstanceAdmin(instanceId, 2202, "Admin2", false)
            };
            _instanceRepoMock.Setup(r => r.GetInstanceAdminsAsync(instanceId)).ReturnsAsync(admins);

            // Act
            var result = await _service.GetAdminsAsync(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task IsAdminAsync_ReturnsTrue_WhenIsAdmin()
        {
            // Arrange
            var instanceId = 2300;
            var characterId = 2301;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, characterId)).ReturnsAsync(true);

            // Act
            var result = await _service.IsAdminAsync(instanceId, characterId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAdminAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 2400;
            var characterId = 2401;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, characterId)).ReturnsAsync(false);

            // Act
            var result = await _service.IsAdminAsync(instanceId, characterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAccessesAsync_ReturnsAccesses()
        {
            // Arrange
            var instanceId = 2500;
            var accesses = new List<WHInstanceAccess>
            {
                new WHInstanceAccess(instanceId, 2501, "Entity1", WHAccessEntity.Character),
                new WHInstanceAccess(instanceId, 2502, "Entity2", WHAccessEntity.Corporation)
            };
            _instanceRepoMock.Setup(r => r.GetInstanceAccessesAsync(instanceId)).ReturnsAsync(accesses);

            // Act
            var result = await _service.GetAccessesAsync(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task HasAccessAsync_ReturnsTrue_WhenHasAccess()
        {
            // Arrange
            var instanceId = 2600;
            var characterId = 2601;
            int? corpId = 2602;
            int? allianceId = 2603;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(true);

            // Act
            var result = await _service.HasAccessAsync(instanceId, characterId, corpId, allianceId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAccessAsync_ReturnsFalse_WhenNoAccess()
        {
            // Arrange
            var instanceId = 2700;
            var characterId = 2701;
            int? corpId = 2702;
            int? allianceId = 2703;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(false);

            // Act
            var result = await _service.HasAccessAsync(instanceId, characterId, corpId, allianceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateMapAsync_ReturnsNull_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 2800;
            var requestingCharacterId = 2801;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateMapAsync(instanceId, "MapName", requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateMapAsync_CreatesMap_WhenAdmin()
        {
            // Arrange
            var instanceId = 2900;
            var requestingCharacterId = 2901;
            var map = new WHMap("NewMap", instanceId) { Id = 1 };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.Create(It.IsAny<WHMap>())).ReturnsAsync(map);

            // Act
            var result = await _service.CreateMapAsync(instanceId, "NewMap", requestingCharacterId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewMap", result.Name);
            _mapRepoMock.Verify(r => r.Create(It.IsAny<WHMap>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMapAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 3000;
            var mapId = 3001;
            var requestingCharacterId = 3002;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteMapAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteMapAsync_ReturnsFalse_WhenMapNotFound()
        {
            // Arrange
            var instanceId = 3100;
            var mapId = 3101;
            var requestingCharacterId = 3102;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync((WHMap?)null);

            // Act
            var result = await _service.DeleteMapAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteMapAsync_ReturnsFalse_WhenMapBelongsToDifferentInstance()
        {
            // Arrange
            var instanceId = 3200;
            var mapId = 3201;
            var requestingCharacterId = 3202;
            var map = new WHMap("Map", 9999) { Id = mapId }; // Different instance
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);

            // Act
            var result = await _service.DeleteMapAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteMapAsync_DeletesMap_WhenAdminAndMapBelongsToInstance()
        {
            // Arrange
            var instanceId = 3300;
            var mapId = 3301;
            var requestingCharacterId = 3302;
            var map = new WHMap("Map", instanceId) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);
            _mapRepoMock.Setup(r => r.DeleteById(mapId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteMapAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.True(result);
            _mapRepoMock.Verify(r => r.DeleteById(mapId), Times.Once);
        }

        [Fact]
        public async Task GetMapsAsync_ReturnsMaps()
        {
            // Arrange
            var instanceId = 3400;
            var maps = new List<WHMap>
            {
                new WHMap("Map1", instanceId) { Id = 1 },
                new WHMap("Map2", instanceId) { Id = 2 }
            };
            _instanceRepoMock.Setup(r => r.GetInstanceMapsAsync(instanceId)).ReturnsAsync(maps);

            // Act
            var result = await _service.GetMapsAsync(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMapAccessesAsync_ReturnsNull_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 3500;
            var mapId = 3501;
            var requestingCharacterId = 3502;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.GetMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMapAccessesAsync_ReturnsNull_WhenMapNotFound()
        {
            // Arrange
            var instanceId = 3600;
            var mapId = 3601;
            var requestingCharacterId = 3602;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync((WHMap?)null);

            // Act
            var result = await _service.GetMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMapAccessesAsync_ReturnsNull_WhenMapBelongsToDifferentInstance()
        {
            // Arrange
            var instanceId = 3700;
            var mapId = 3701;
            var requestingCharacterId = 3702;
            var map = new WHMap("Map", 9999) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);

            // Act
            var result = await _service.GetMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMapAccessesAsync_ReturnsAccesses_WhenAdminAndMapBelongsToInstance()
        {
            // Arrange
            var instanceId = 3800;
            var mapId = 3801;
            var requestingCharacterId = 3802;
            var map = new WHMap("Map", instanceId) { Id = mapId };
            var accesses = new List<WHMapAccess>
            {
                new WHMapAccess(mapId, 3803, "Entity1", WHAccessEntity.Character),
                new WHMapAccess(mapId, 3804, "Entity2", WHAccessEntity.Corporation)
            };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);
            _mapAccessRepoMock.Setup(r => r.GetMapAccessesAsync(mapId)).ReturnsAsync(accesses);

            // Act
            var result = await _service.GetMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddMapAccessAsync_ReturnsNull_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 3900;
            var mapId = 3901;
            var requestingCharacterId = 3902;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.AddMapAccessAsync(instanceId, mapId, 3903, "Entity", WHAccessEntity.Character, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMapAccessAsync_ReturnsNull_WhenMapNotFound()
        {
            // Arrange
            var instanceId = 4000;
            var mapId = 4001;
            var requestingCharacterId = 4002;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync((WHMap?)null);

            // Act
            var result = await _service.AddMapAccessAsync(instanceId, mapId, 4003, "Entity", WHAccessEntity.Character, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMapAccessAsync_ReturnsNull_WhenMapBelongsToDifferentInstance()
        {
            // Arrange
            var instanceId = 4100;
            var mapId = 4101;
            var requestingCharacterId = 4102;
            var map = new WHMap("Map", 9999) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);

            // Act
            var result = await _service.AddMapAccessAsync(instanceId, mapId, 4103, "Entity", WHAccessEntity.Character, requestingCharacterId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMapAccessAsync_AddsAccess_WhenAdminAndMapBelongsToInstance()
        {
            // Arrange
            var instanceId = 4200;
            var mapId = 4201;
            var requestingCharacterId = 4202;
            var map = new WHMap("Map", instanceId) { Id = mapId };
            var access = new WHMapAccess(mapId, 4203, "Entity", WHAccessEntity.Character) { Id = 1 };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);
            _mapAccessRepoMock.Setup(r => r.AddMapAccessAsync(It.IsAny<WHMapAccess>())).ReturnsAsync(access);

            // Act
            var result = await _service.AddMapAccessAsync(instanceId, mapId, 4203, "Entity", WHAccessEntity.Character, requestingCharacterId);

            // Assert
            Assert.NotNull(result);
            _mapAccessRepoMock.Verify(r => r.AddMapAccessAsync(It.IsAny<WHMapAccess>()), Times.Once);
        }

        [Fact]
        public async Task RemoveMapAccessAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 4300;
            var mapId = 4301;
            var accessId = 4302;
            var requestingCharacterId = 4303;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.RemoveMapAccessAsync(instanceId, mapId, accessId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMapAccessAsync_ReturnsFalse_WhenMapNotFound()
        {
            // Arrange
            var instanceId = 4400;
            var mapId = 4401;
            var accessId = 4402;
            var requestingCharacterId = 4403;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync((WHMap?)null);

            // Act
            var result = await _service.RemoveMapAccessAsync(instanceId, mapId, accessId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMapAccessAsync_ReturnsFalse_WhenMapBelongsToDifferentInstance()
        {
            // Arrange
            var instanceId = 4500;
            var mapId = 4501;
            var accessId = 4502;
            var requestingCharacterId = 4503;
            var map = new WHMap("Map", 9999) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);

            // Act
            var result = await _service.RemoveMapAccessAsync(instanceId, mapId, accessId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMapAccessAsync_RemovesAccess_WhenAdminAndMapBelongsToInstance()
        {
            // Arrange
            var instanceId = 4600;
            var mapId = 4601;
            var accessId = 4602;
            var requestingCharacterId = 4603;
            var map = new WHMap("Map", instanceId) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);
            _mapAccessRepoMock.Setup(r => r.RemoveMapAccessAsync(mapId, accessId)).ReturnsAsync(true);

            // Act
            var result = await _service.RemoveMapAccessAsync(instanceId, mapId, accessId, requestingCharacterId);

            // Assert
            Assert.True(result);
            _mapAccessRepoMock.Verify(r => r.RemoveMapAccessAsync(mapId, accessId), Times.Once);
        }

        [Fact]
        public async Task ClearMapAccessesAsync_ReturnsFalse_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 4700;
            var mapId = 4701;
            var requestingCharacterId = 4702;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(false);

            // Act
            var result = await _service.ClearMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ClearMapAccessesAsync_ReturnsFalse_WhenMapNotFound()
        {
            // Arrange
            var instanceId = 4800;
            var mapId = 4801;
            var requestingCharacterId = 4802;
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync((WHMap?)null);

            // Act
            var result = await _service.ClearMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ClearMapAccessesAsync_ReturnsFalse_WhenMapBelongsToDifferentInstance()
        {
            // Arrange
            var instanceId = 4900;
            var mapId = 4901;
            var requestingCharacterId = 4902;
            var map = new WHMap("Map", 9999) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);

            // Act
            var result = await _service.ClearMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ClearMapAccessesAsync_ClearsAccesses_WhenAdminAndMapBelongsToInstance()
        {
            // Arrange
            var instanceId = 5000;
            var mapId = 5001;
            var requestingCharacterId = 5002;
            var map = new WHMap("Map", instanceId) { Id = mapId };
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _mapRepoMock.Setup(r => r.GetById(mapId)).ReturnsAsync(map);
            _mapAccessRepoMock.Setup(r => r.ClearMapAccessesAsync(mapId)).ReturnsAsync(true);

            // Act
            var result = await _service.ClearMapAccessesAsync(instanceId, mapId, requestingCharacterId);

            // Assert
            Assert.True(result);
            _mapAccessRepoMock.Verify(r => r.ClearMapAccessesAsync(mapId), Times.Once);
        }

        [Fact]
        public async Task HasMapAccessAsync_ReturnsFalse_WhenNoInstanceAccess()
        {
            // Arrange
            var instanceId = 5100;
            var mapId = 5101;
            var characterId = 5102;
            int? corpId = 5103;
            int? allianceId = 5104;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(false);

            // Act
            var result = await _service.HasMapAccessAsync(instanceId, mapId, characterId, corpId, allianceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasMapAccessAsync_ReturnsTrue_WhenIsAdmin()
        {
            // Arrange
            var instanceId = 5200;
            var mapId = 5201;
            var characterId = 5202;
            int? corpId = 5203;
            int? allianceId = 5204;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, characterId)).ReturnsAsync(true);

            // Act
            var result = await _service.HasMapAccessAsync(instanceId, mapId, characterId, corpId, allianceId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasMapAccessAsync_ChecksMapAccess_WhenNotAdmin()
        {
            // Arrange
            var instanceId = 5300;
            var mapId = 5301;
            var characterId = 5302;
            int? corpId = 5303;
            int? allianceId = 5304;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, characterId)).ReturnsAsync(false);
            _mapAccessRepoMock.Setup(r => r.HasMapAccessAsync(mapId, characterId, corpId, allianceId)).ReturnsAsync(true);

            // Act
            var result = await _service.HasMapAccessAsync(instanceId, mapId, characterId, corpId, allianceId);

            // Assert
            Assert.True(result);
            _mapAccessRepoMock.Verify(r => r.HasMapAccessAsync(mapId, characterId, corpId, allianceId), Times.Once);
        }

        [Fact]
        public async Task HasMapAccessAsync_ReturnsFalse_WhenNotAdminAndNoMapAccess()
        {
            // Arrange
            var instanceId = 5400;
            var mapId = 5401;
            var characterId = 5402;
            int? corpId = 5403;
            int? allianceId = 5404;
            _instanceRepoMock.Setup(r => r.HasInstanceAccessAsync(instanceId, characterId, corpId, allianceId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, characterId)).ReturnsAsync(false);
            _mapAccessRepoMock.Setup(r => r.HasMapAccessAsync(mapId, characterId, corpId, allianceId)).ReturnsAsync(false);

            // Act
            var result = await _service.HasMapAccessAsync(instanceId, mapId, characterId, corpId, allianceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MapHasAccessRestrictionsAsync_ReturnsTrue_WhenHasRestrictions()
        {
            // Arrange
            var mapId = 5500;
            _mapAccessRepoMock.Setup(r => r.HasAccessRestrictionsAsync(mapId)).ReturnsAsync(true);

            // Act
            var result = await _service.MapHasAccessRestrictionsAsync(mapId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task MapHasAccessRestrictionsAsync_ReturnsFalse_WhenNoRestrictions()
        {
            // Arrange
            var mapId = 5600;
            _mapAccessRepoMock.Setup(r => r.HasAccessRestrictionsAsync(mapId)).ReturnsAsync(false);

            // Act
            var result = await _service.MapHasAccessRestrictionsAsync(mapId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateInstanceAsync_ReturnsNull_WhenCreateFails()
        {
            // Arrange
            var ownerEntityId = 5700;
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ReturnsAsync((WHInstance?)null);
            _instanceRepoMock.Setup(r => r.Create(It.IsAny<WHInstance>())).ReturnsAsync((WHInstance?)null);

            // Act
            var result = await _service.CreateInstanceAsync("Name", "Desc", ownerEntityId, "Owner", WHAccessEntity.Character, 5701, "Creator");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateInstanceAsync_ReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var ownerEntityId = 5800;
            _instanceRepoMock.Setup(r => r.GetByOwnerAsync(ownerEntityId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _service.CreateInstanceAsync("Name", "Desc", ownerEntityId, "Owner", WHAccessEntity.Character, 5801, "Creator");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAccessAsync_RemovesAccess_WithNoMapAccesses()
        {
            // Arrange
            var instanceId = 5900;
            var requestingCharacterId = 5901;
            var accessId = 5902;
            var access = new WHInstanceAccess(instanceId, 5903, "Entity", WHAccessEntity.Corporation) { Id = accessId };
            var emptyMapAccesses = new Dictionary<int, IEnumerable<int>>();

            _instanceRepoMock.Setup(r => r.IsInstanceAdminAsync(instanceId, requestingCharacterId)).ReturnsAsync(true);
            _instanceRepoMock.Setup(r => r.GetInstanceAccessesAsync(instanceId)).ReturnsAsync(new List<WHInstanceAccess> { access });
            _mapAccessRepoMock.Setup(r => r.RemoveMapAccessesByEntityAsync(instanceId, access.EveEntityId, access.EveEntity)).ReturnsAsync(emptyMapAccesses);
            _instanceRepoMock.Setup(r => r.RemoveInstanceAccessAsync(instanceId, accessId)).ReturnsAsync(true);

            // Act
            var (success, removed) = await _service.RemoveAccessAsync(instanceId, accessId, requestingCharacterId);

            // Assert
            Assert.True(success);
            Assert.Empty(removed);
        }
    }
}