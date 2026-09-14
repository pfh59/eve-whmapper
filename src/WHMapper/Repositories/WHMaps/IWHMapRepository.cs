using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public interface IWHMapRepository : IDefaultRepository<WHMap,int>
    {
        Task<WHMap?> GetByNameAsync(string mapName);
        Task<bool> DeleteAll();

        /// <summary>
        /// Returns the instance a map belongs to, without loading the map accesses, systems or links.
        /// </summary>
        /// <returns>The instance id; null when the map does not exist or belongs to no instance.</returns>
        Task<int?> GetInstanceIdAsync(int mapId);
    }
}

