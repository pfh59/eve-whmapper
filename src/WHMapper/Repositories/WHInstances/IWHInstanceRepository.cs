using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHInstances
{
    public interface IWHInstanceRepository : IDefaultRepository<WHInstance, int>
    {
        /// <summary>
        /// Gets an instance by owner (character, corporation, or alliance)
        /// </summary>
        Task<WHInstance?> GetByOwnerAsync(int ownerEveEntityId);

        /// <summary>
        /// Gets all instances where a character is an administrator
        /// </summary>
        Task<IEnumerable<WHInstance>?> GetInstancesForAdminAsync(int characterId);

        /// <summary>
        /// Gets all instances accessible by a character (through instance access, not map access)
        /// </summary>
        Task<IEnumerable<WHInstance>?> GetAccessibleInstancesAsync(int characterId, int? corporationId, int? allianceId);

        /// <summary>
        /// Gets all administrators of an instance
        /// </summary>
        Task<IEnumerable<WHInstanceAdmin>?> GetInstanceAdminsAsync(int instanceId);

        /// <summary>
        /// Adds an administrator to an instance
        /// </summary>
        Task<WHInstanceAdmin?> AddInstanceAdminAsync(int instanceId, int characterId, string characterName, bool isOwner = false);

        /// <summary>
        /// Removes an administrator from an instance
        /// </summary>
        Task<bool> RemoveInstanceAdminAsync(int instanceId, int characterId);

        /// <summary>
        /// Checks if a character is an administrator of an instance
        /// </summary>
        Task<bool> IsInstanceAdminAsync(int instanceId, int characterId);

        /// <summary>
        /// Gets all access entries for an instance
        /// </summary>
        Task<IEnumerable<WHInstanceAccess>?> GetInstanceAccessesAsync(int instanceId);

        /// <summary>
        /// Adds an access entry to an instance
        /// </summary>
        Task<WHInstanceAccess?> AddInstanceAccessAsync(WHInstanceAccess access);

        /// <summary>
        /// Removes an access entry from an instance
        /// </summary>
        Task<bool> RemoveInstanceAccessAsync(int instanceId, int accessId);

        /// <summary>
        /// Gets all maps belonging to an instance
        /// </summary>
        Task<IEnumerable<WHMap>?> GetInstanceMapsAsync(int instanceId);

        /// <summary>
        /// Checks if a character has access to an instance
        /// </summary>
        Task<bool> HasInstanceAccessAsync(int instanceId, int characterId, int? corporationId, int? allianceId);
    }
}
