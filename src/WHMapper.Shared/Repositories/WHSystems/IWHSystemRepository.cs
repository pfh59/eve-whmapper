using WHMapper.Shared.Models.Db;

namespace WHMapper.Shared.Repositories.WHSystems
{
    public interface IWHSystemRepository : IDefaultRepository<WHSystem, int>
    {
        public Task<WHSystem?> GetByName(string name);
    }
}
