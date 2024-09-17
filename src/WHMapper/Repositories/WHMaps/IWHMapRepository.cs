using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public interface IWHMapRepository : IDefaultRepository<WHMap,int>
    {
        Task<WHMap?> GetByNameAsync(string mapName);
        Task<bool> DeleteAll();
    }
}

