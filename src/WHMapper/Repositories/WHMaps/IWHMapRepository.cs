using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public interface IWHMapRepository : IDefaultRepository<WHMap,int>
    {
        Task<WHMap?> GetByNameAsync(string mapName);
        Task<bool> DeleteAll();
        Task<IEnumerable<WHAccess>?> GetMapAccesses(int id);
        Task<bool> DeleteMapAccess(int mapId, int accessId);
        Task<bool> DeleteMapAccesses(int mapId);
        Task<bool> AddMapAccess(int mapId, int accessId);
    }
}

