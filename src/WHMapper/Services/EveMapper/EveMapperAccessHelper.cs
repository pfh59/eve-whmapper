using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Services.EveAPI.Characters;

namespace WHMapper.Services.EveMapper
{
    public class EveMapperAccessHelper : IEveMapperAccessHelper
    {
        private readonly ICharacterServices _characterServices;
        private readonly IWHMapRepository _mapRepo;
        private readonly IWHInstanceRepository _instanceRepo;
        private readonly IWHMapAccessRepository _mapAccessRepo;

        public EveMapperAccessHelper(
            IWHMapRepository mapRepo, 
            IWHInstanceRepository instanceRepo,
            IWHMapAccessRepository mapAccessRepo,
            ICharacterServices characterServices)
        {
            _mapRepo = mapRepo;
            _instanceRepo = instanceRepo;
            _mapAccessRepo = mapAccessRepo;
            _characterServices = characterServices;
        }

        /// <summary>
        /// Check if a user has access to at least one instance.
        /// If no instances exist, user has no access (must register first).
        /// </summary>
        public async Task<bool> IsEveMapperUserAccessAuthorized(int eveCharacterId)
        {
            // Get character info for corp/alliance check
            var characterInfo = await _characterServices.GetCharacter(eveCharacterId);
            if (!characterInfo.IsSuccess || characterInfo.Data == null)
                return false;

            var charData = characterInfo.Data;
            
            // Check if user has access to any instance
            var accessibleInstances = await _instanceRepo.GetAccessibleInstancesAsync(
                eveCharacterId, 
                charData.CorporationId > 0 ? charData.CorporationId : null,
                charData.AllianceId > 0 ? charData.AllianceId : null);

            return accessibleInstances?.Any() == true;
        }

        /// <summary>
        /// Check if a user is admin of at least one instance.
        /// </summary>
        public async Task<bool> IsEveMapperAdminAccessAuthorized(int eveCharacterId)
        {
            var administeredInstances = await _instanceRepo.GetInstancesForAdminAsync(eveCharacterId);
            return administeredInstances?.Any() == true;
        }

        /// <summary>
        /// Check if a user has access to a specific map through its instance and map-level access control.
        /// </summary>
        public async Task<bool> IsEveMapperMapAccessAuthorized(int eveCharacterId, int mapId)
        {
            // Get the map to check its instance
            var map = await _mapRepo.GetById(mapId);
            if (map == null)
                return false;

            // Map must belong to an instance
            if (!map.WHInstanceId.HasValue)
                return false;

            var characterResult = await _characterServices.GetCharacter(eveCharacterId);
            if (!characterResult.IsSuccess || characterResult.Data == null)
                return false;

            var character = characterResult.Data;
            int? corporationId = character.CorporationId > 0 ? character.CorporationId : null;
            int? allianceId = character.AllianceId > 0 ? character.AllianceId : null;

            // First check instance-level access
            var hasInstanceAccess = await _instanceRepo.HasInstanceAccessAsync(
                map.WHInstanceId.Value,
                eveCharacterId,
                corporationId,
                allianceId);

            if (!hasInstanceAccess)
                return false;

            // Instance admins always have access to all maps
            var isInstanceAdmin = await _instanceRepo.IsInstanceAdminAsync(map.WHInstanceId.Value, eveCharacterId);
            if (isInstanceAdmin)
                return true;

            // Check map-level access
            // If no restrictions exist on the map, all instance members can access it
            // If restrictions exist, check if user has explicit access
            return await _mapAccessRepo.HasMapAccessAsync(mapId, eveCharacterId, corporationId, allianceId);
        }

        /// <summary>
        /// Checks if a character is an admin of a specific instance
        /// </summary>
        public async Task<bool> IsInstanceAdminAuthorized(int eveCharacterId, int instanceId)
        {
            return await _instanceRepo.IsInstanceAdminAsync(instanceId, eveCharacterId);
        }
    }
}
