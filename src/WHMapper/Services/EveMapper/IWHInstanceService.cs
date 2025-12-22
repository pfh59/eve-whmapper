using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.EveMapper
{
    /// <summary>
    /// Service for managing WHMapper instances (multi-tenant registration)
    /// </summary>
    public interface IWHInstanceService
    {
        /// <summary>
        /// Creates a new instance for a character, corporation, or alliance
        /// </summary>
        /// <param name="name">Display name for the instance</param>
        /// <param name="description">Optional description</param>
        /// <param name="ownerEntityId">EVE entity ID (character, corp, or alliance)</param>
        /// <param name="ownerEntityName">Name of the owner entity</param>
        /// <param name="ownerType">Type of entity (Character, Corporation, Alliance)</param>
        /// <param name="creatorCharacterId">Character ID of the user creating the instance</param>
        /// <param name="creatorCharacterName">Character name of the creator</param>
        /// <returns>Created instance or null if failed</returns>
        Task<WHInstance?> CreateInstanceAsync(
            string name,
            string? description,
            int ownerEntityId,
            string ownerEntityName,
            WHAccessEntity ownerType,
            int creatorCharacterId,
            string creatorCharacterName);

        /// <summary>
        /// Gets an instance by ID
        /// </summary>
        Task<WHInstance?> GetInstanceAsync(int instanceId);

        /// <summary>
        /// Gets the instance owned by a specific entity
        /// </summary>
        Task<WHInstance?> GetInstanceByOwnerAsync(int ownerEveEntityId);

        /// <summary>
        /// Gets all instances where the character is an admin
        /// </summary>
        Task<IEnumerable<WHInstance>?> GetAdministeredInstancesAsync(int characterId);

        /// <summary>
        /// Gets all instances accessible by a character
        /// </summary>
        Task<IEnumerable<WHInstance>?> GetAccessibleInstancesAsync(int characterId, int? corporationId, int? allianceId);

        /// <summary>
        /// Updates instance details
        /// </summary>
        Task<WHInstance?> UpdateInstanceAsync(int instanceId, string name, string? description);

        /// <summary>
        /// Deletes an instance (only owner can do this)
        /// </summary>
        Task<bool> DeleteInstanceAsync(int instanceId, int requestingCharacterId);

        /// <summary>
        /// Adds an administrator to an instance
        /// </summary>
        Task<WHInstanceAdmin?> AddAdminAsync(int instanceId, int characterId, string characterName, int requestingCharacterId);

        /// <summary>
        /// Removes an administrator from an instance
        /// </summary>
        Task<bool> RemoveAdminAsync(int instanceId, int characterIdToRemove, int requestingCharacterId);

        /// <summary>
        /// Gets all admins of an instance
        /// </summary>
        Task<IEnumerable<WHInstanceAdmin>?> GetAdminsAsync(int instanceId);

        /// <summary>
        /// Checks if a character is an admin of an instance
        /// </summary>
        Task<bool> IsAdminAsync(int instanceId, int characterId);

        /// <summary>
        /// Checks if a character is the owner of an instance
        /// </summary>
        Task<bool> IsOwnerAsync(int instanceId, int characterId);

        /// <summary>
        /// Adds access to an instance for a character, corporation, or alliance
        /// </summary>
        Task<WHInstanceAccess?> AddAccessAsync(int instanceId, int eveEntityId, string eveEntityName, WHAccessEntity entityType, int requestingCharacterId);

        /// <summary>
        /// Removes access from an instance
        /// </summary>
        Task<bool> RemoveAccessAsync(int instanceId, int accessId, int requestingCharacterId);

        /// <summary>
        /// Gets all access entries for an instance
        /// </summary>
        Task<IEnumerable<WHInstanceAccess>?> GetAccessesAsync(int instanceId);

        /// <summary>
        /// Checks if a character has access to an instance
        /// </summary>
        Task<bool> HasAccessAsync(int instanceId, int characterId, int? corporationId, int? allianceId);

        /// <summary>
        /// Creates a new map within an instance
        /// </summary>
        Task<WHMap?> CreateMapAsync(int instanceId, string mapName, int requestingCharacterId);

        /// <summary>
        /// Deletes a map from an instance
        /// </summary>
        Task<bool> DeleteMapAsync(int instanceId, int mapId, int requestingCharacterId);

        /// <summary>
        /// Gets all maps in an instance
        /// </summary>
        Task<IEnumerable<WHMap>?> GetMapsAsync(int instanceId);

        /// <summary>
        /// Checks if a character can register a new instance (hasn't already created one for their entity)
        /// </summary>
        Task<bool> CanRegisterAsync(int ownerEveEntityId);

        #region Map Access Management

        /// <summary>
        /// Gets all access entries for a specific map
        /// </summary>
        Task<IEnumerable<WHMapAccess>?> GetMapAccessesAsync(int instanceId, int mapId, int requestingCharacterId);

        /// <summary>
        /// Adds access to a map for a character, corporation, or alliance
        /// </summary>
        Task<WHMapAccess?> AddMapAccessAsync(int instanceId, int mapId, int eveEntityId, string eveEntityName, WHAccessEntity entityType, int requestingCharacterId);

        /// <summary>
        /// Removes access from a map
        /// </summary>
        Task<bool> RemoveMapAccessAsync(int instanceId, int mapId, int accessId, int requestingCharacterId);

        /// <summary>
        /// Clears all access restrictions from a map (makes it accessible to all instance members)
        /// </summary>
        Task<bool> ClearMapAccessesAsync(int instanceId, int mapId, int requestingCharacterId);

        /// <summary>
        /// Checks if a character has access to a specific map (considering both instance and map-level access)
        /// </summary>
        Task<bool> HasMapAccessAsync(int instanceId, int mapId, int characterId, int? corporationId, int? allianceId);

        /// <summary>
        /// Checks if a map has any access restrictions
        /// </summary>
        Task<bool> MapHasAccessRestrictionsAsync(int mapId);

        #endregion
    }
}
