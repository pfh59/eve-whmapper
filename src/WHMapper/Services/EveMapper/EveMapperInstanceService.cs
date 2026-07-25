using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Services.EveMapper
{
    /// <summary>
    /// Service for managing WHMapper instances (multi-tenant registration)
    /// </summary>
    public class EveMapperInstanceService : IEveMapperInstanceService
    {
        private readonly ILogger<EveMapperInstanceService> _logger;
        private readonly IWHInstanceRepository _instanceRepository;
        private readonly IWHMapRepository _mapRepository;
        private readonly IWHMapAccessRepository _mapAccessRepository;
        private readonly bool _singleTenantMode;

        public EveMapperInstanceService(
            ILogger<EveMapperInstanceService> logger,
            IWHInstanceRepository instanceRepository,
            IWHMapRepository mapRepository,
            IWHMapAccessRepository mapAccessRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _instanceRepository = instanceRepository;
            _mapRepository = mapRepository;
            _mapAccessRepository = mapAccessRepository;
            _singleTenantMode = configuration.GetValue<bool>("Instances:SingleTenantMode");
        }

        public async Task<WHInstance?> CreateInstanceAsync(
            string name,
            string? description,
            int ownerEntityId,
            string ownerEntityName,
            WHAccessEntity ownerType,
            int creatorCharacterId,
            string creatorCharacterName)
        {
            try
            {
                // Single-tenant mode: block creation once the dedicated instance exists
                if (await IsInstanceCreationLockedAsync())
                {
                    _logger.LogWarning("Permission denied: instance creation is locked (single-tenant mode) - owner {OwnerId}", ownerEntityId);
                    return null;
                }

                var existingInstance = await _instanceRepository.GetByOwnerAsync(ownerEntityId);
                if (existingInstance != null)
                {
                    _logger.LogWarning("Instance already exists for owner {OwnerId}", ownerEntityId);
                    return null;
                }

                var instance = new WHInstance(
                    name,
                    ownerEntityId,
                    ownerEntityName,
                    ownerType,
                    creatorCharacterId,
                    creatorCharacterName);

                instance.Description = description;

                var createdInstance = await _instanceRepository.Create(instance);
                if (createdInstance == null)
                {
                    _logger.LogError("Failed to create instance for owner {OwnerId}", ownerEntityId);
                    return null;
                }

                var admin = await _instanceRepository.AddInstanceAdminAsync(
                    createdInstance.Id,
                    creatorCharacterId,
                    creatorCharacterName,
                    isOwner: true);

                if (admin == null)
                {
                    _logger.LogError("Failed to add owner admin for instance {InstanceId}", createdInstance.Id);
                    // Rollback: delete the instance
                    await _instanceRepository.DeleteById(createdInstance.Id);
                    return null;
                }

                // Add access for the owner entity (character, corp, or alliance)
                var access = new WHInstanceAccess(
                    createdInstance.Id,
                    ownerEntityId,
                    ownerEntityName,
                    ownerType);

                await _instanceRepository.AddInstanceAccessAsync(access);

                _logger.LogInformation("Created instance {InstanceId} for owner {OwnerId}", createdInstance.Id, ownerEntityId);
                return createdInstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instance for owner {OwnerId}", ownerEntityId);
                return null;
            }
        }

        public async Task<WHInstance?> GetInstanceAsync(int instanceId)
        {
            return await _instanceRepository.GetById(instanceId);
        }

        public async Task<WHInstance?> GetInstanceByOwnerAsync(int ownerEveEntityId)
        {
            return await _instanceRepository.GetByOwnerAsync(ownerEveEntityId);
        }

        public async Task<IEnumerable<WHInstance>?> GetAdministeredInstancesAsync(int characterId)
        {
            return await _instanceRepository.GetInstancesForAdminAsync(characterId);
        }

        public async Task<IEnumerable<WHInstance>?> GetAccessibleInstancesAsync(int characterId, int? corporationId, int? allianceId)
        {
            return await _instanceRepository.GetAccessibleInstancesAsync(characterId, corporationId, allianceId);
        }

        public async Task<WHInstance?> UpdateInstanceAsync(int instanceId, string name, string? description)
        {
            var instance = await _instanceRepository.GetById(instanceId);
            if (instance == null)
                return null;

            instance.Name = name;
            instance.Description = description;

            return await _instanceRepository.Update(instanceId, instance);
        }

        public async Task<bool> DeleteInstanceAsync(int instanceId, int requestingCharacterId)
        {
            if (!await IsOwnerAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to delete instance {InstanceId} but is not owner", 
                    requestingCharacterId, instanceId);
                return false;
            }

            return await _instanceRepository.DeleteById(instanceId);
        }

        public async Task<WHInstanceAdmin?> AddAdminAsync(int instanceId, int characterId, string characterName, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to add admin to instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return null;
            }

            return await _instanceRepository.AddInstanceAdminAsync(instanceId, characterId, characterName, isOwner: false);
        }

        public async Task<bool> RemoveAdminAsync(int instanceId, int characterIdToRemove, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to remove admin from instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return false;
            }

            return await _instanceRepository.RemoveInstanceAdminAsync(instanceId, characterIdToRemove);
        }

        public async Task<IEnumerable<WHInstanceAdmin>?> GetAdminsAsync(int instanceId)
        {
            return await _instanceRepository.GetInstanceAdminsAsync(instanceId);
        }

        public async Task<bool> IsAdminAsync(int instanceId, int characterId)
        {
            return await _instanceRepository.IsInstanceAdminAsync(instanceId, characterId);
        }

        public async Task<bool> IsOwnerAsync(int instanceId, int characterId)
        {
            var instance = await _instanceRepository.GetById(instanceId);
            if (instance == null)
                return false;

            return instance.CreatorCharacterId == characterId;
        }

        public async Task<WHInstanceAccess?> AddAccessAsync(int instanceId, int eveEntityId, string eveEntityName, WHAccessEntity entityType, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to add access to instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return null;
            }

            var access = new WHInstanceAccess(instanceId, eveEntityId, eveEntityName, entityType);
            return await _instanceRepository.AddInstanceAccessAsync(access);
        }

        public async Task<(bool Success, IDictionary<int, IEnumerable<int>> RemovedMapAccesses)> RemoveAccessAsync(int instanceId, int accessId, int requestingCharacterId)
        {
            var emptyResult = new Dictionary<int, IEnumerable<int>>();
            
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to remove access from instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return (false, emptyResult);
            }

            var accesses = await _instanceRepository.GetInstanceAccessesAsync(instanceId);
            var accessToRemove = accesses?.FirstOrDefault(a => a.Id == accessId);
            
            if (accessToRemove == null)
            {
                _logger.LogWarning("Instance access {AccessId} not found for instance {InstanceId}", accessId, instanceId);
                return (false, emptyResult);
            }

            // First, remove all map accesses for this entity
            var removedMapAccesses = await _mapAccessRepository.RemoveMapAccessesByEntityAsync(
                instanceId, 
                accessToRemove.EveEntityId, 
                accessToRemove.EveEntity);

            if (removedMapAccesses.Any())
            {
                _logger.LogInformation("Removed {Count} map accesses for entity {EntityId} ({EntityType}) when removing instance access",
                    removedMapAccesses.Sum(x => x.Value.Count()), accessToRemove.EveEntityId, accessToRemove.EveEntity);
            }

            // Then remove the instance access
            var success = await _instanceRepository.RemoveInstanceAccessAsync(instanceId, accessId);
            return (success, removedMapAccesses);
        }

        public async Task<IEnumerable<WHInstanceAccess>?> GetAccessesAsync(int instanceId)
        {
            return await _instanceRepository.GetInstanceAccessesAsync(instanceId);
        }

        public async Task<bool> HasAccessAsync(int instanceId, int characterId, int? corporationId, int? allianceId)
        {
            return await _instanceRepository.HasInstanceAccessAsync(instanceId, characterId, corporationId, allianceId);
        }

        public async Task<WHMap?> CreateMapAsync(int instanceId, string mapName, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to create map in instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return null;
            }

            var map = new WHMap(mapName, instanceId);
            return await _mapRepository.Create(map);
        }

        public async Task<bool> DeleteMapAsync(int instanceId, int mapId, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to delete map from instance {InstanceId} but is not admin",
                    requestingCharacterId, instanceId);
                return false;
            }

            var map = await _mapRepository.GetById(mapId);
            if (map == null || map.WHInstanceId != instanceId)
            {
                _logger.LogWarning("Map {MapId} not found or doesn't belong to instance {InstanceId}", mapId, instanceId);
                return false;
            }

            return await _mapRepository.DeleteById(mapId);
        }

        public async Task<IEnumerable<WHMap>?> GetMapsAsync(int instanceId)
        {
            return await _instanceRepository.GetInstanceMapsAsync(instanceId);
        }

        public async Task<bool> CanRegisterAsync(int ownerEveEntityId)
        {
            if (await IsInstanceCreationLockedAsync())
                return false;

            var existingInstance = await _instanceRepository.GetByOwnerAsync(ownerEveEntityId);
            return existingInstance == null;
        }

        public async Task<bool> IsInstanceCreationLockedAsync()
        {
            if (!_singleTenantMode)
                return false;

            return await _instanceRepository.AnyAsync();
        }

        #region Map Access Management

        public async Task<IEnumerable<WHMapAccess>?> GetMapAccessesAsync(int instanceId, int mapId, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to view map accesses for map {MapId} but is not admin",
                    requestingCharacterId, mapId);
                return null;
            }

            var map = await _mapRepository.GetById(mapId);
            if (map == null || map.WHInstanceId != instanceId)
            {
                _logger.LogWarning("Map {MapId} not found or doesn't belong to instance {InstanceId}", mapId, instanceId);
                return null;
            }

            return await _mapAccessRepository.GetMapAccessesAsync(mapId);
        }

        public async Task<WHMapAccess?> AddMapAccessAsync(int instanceId, int mapId, int eveEntityId, string eveEntityName, WHAccessEntity entityType, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to add map access for map {MapId} but is not admin",
                    requestingCharacterId, mapId);
                return null;
            }

            var map = await _mapRepository.GetById(mapId);
            if (map == null || map.WHInstanceId != instanceId)
            {
                _logger.LogWarning("Map {MapId} not found or doesn't belong to instance {InstanceId}", mapId, instanceId);
                return null;
            }

            var access = new WHMapAccess(mapId, eveEntityId, eveEntityName, entityType);
            return await _mapAccessRepository.AddMapAccessAsync(access);
        }

        public async Task<bool> RemoveMapAccessAsync(int instanceId, int mapId, int accessId, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to remove map access from map {MapId} but is not admin",
                    requestingCharacterId, mapId);
                return false;
            }

            var map = await _mapRepository.GetById(mapId);
            if (map == null || map.WHInstanceId != instanceId)
            {
                _logger.LogWarning("Map {MapId} not found or doesn't belong to instance {InstanceId}", mapId, instanceId);
                return false;
            }

            return await _mapAccessRepository.RemoveMapAccessAsync(mapId, accessId);
        }

        public async Task<bool> ClearMapAccessesAsync(int instanceId, int mapId, int requestingCharacterId)
        {
            if (!await IsAdminAsync(instanceId, requestingCharacterId))
            {
                _logger.LogWarning("Character {CharacterId} attempted to clear map accesses for map {MapId} but is not admin",
                    requestingCharacterId, mapId);
                return false;
            }

            var map = await _mapRepository.GetById(mapId);
            if (map == null || map.WHInstanceId != instanceId)
            {
                _logger.LogWarning("Map {MapId} not found or doesn't belong to instance {InstanceId}", mapId, instanceId);
                return false;
            }

            return await _mapAccessRepository.ClearMapAccessesAsync(mapId);
        }

        public async Task<bool> HasMapAccessAsync(int instanceId, int mapId, int characterId, int? corporationId, int? allianceId)
        {
            var hasInstanceAccess = await HasAccessAsync(instanceId, characterId, corporationId, allianceId);
            if (!hasInstanceAccess)
                return false;

            // Instance admins always have access to all maps
            if (await IsAdminAsync(instanceId, characterId))
                return true;

            return await _mapAccessRepository.HasMapAccessAsync(mapId, characterId, corporationId, allianceId);
        }

        public async Task<bool> MapHasAccessRestrictionsAsync(int mapId)
        {
            return await _mapAccessRepository.HasAccessRestrictionsAsync(mapId);
        }

        #endregion
    }
}
