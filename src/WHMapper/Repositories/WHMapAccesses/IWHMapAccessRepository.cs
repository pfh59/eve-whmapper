using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMapAccesses
{
    public interface IWHMapAccessRepository : IDefaultRepository<WHMapAccess, int>
    {
        /// <summary>
        /// Gets all access entries for a specific map
        /// </summary>
        Task<IEnumerable<WHMapAccess>?> GetMapAccessesAsync(int mapId);

        /// <summary>
        /// Checks if a map has any access restrictions (if not, all instance members can access it)
        /// </summary>
        Task<bool> HasAccessRestrictionsAsync(int mapId);

        /// <summary>
        /// Checks if an entity (character, corporation, or alliance) has explicit access to a map
        /// </summary>
        Task<bool> HasMapAccessAsync(int mapId, int characterId, int? corporationId, int? allianceId);

        /// <summary>
        /// Adds an access entry to a map
        /// </summary>
        Task<WHMapAccess?> AddMapAccessAsync(WHMapAccess access);

        /// <summary>
        /// Removes an access entry from a map
        /// </summary>
        Task<bool> RemoveMapAccessAsync(int mapId, int accessId);

        /// <summary>
        /// Removes all access entries for a map (resets to default - everyone with instance access)
        /// </summary>
        Task<bool> ClearMapAccessesAsync(int mapId);

        /// <summary>
        /// Gets the count of access entries for a map
        /// </summary>
        Task<int> GetMapAccessCountAsync(int mapId);
    }
}
