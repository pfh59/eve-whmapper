using System;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSystems
{
    public interface IWHSignature : IDefaultRepository<WHSystem, int>
    {
        public Task<WHSystem?> GetByName(string name);
    }
}
