using System;
using System.Threading.Tasks;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSystems
{
    public interface IWHSystemRepository : IDefaultRepository<WHSystem, int>
    {
        public Task<WHSystem?> GetByName(string name);

        public Task<WHSignature?> AddWHSignature(int idWHSystem, WHSignature whSignature);
        public Task<IEnumerable<WHSignature?>> AddWHSignatures(int idWHSystem, IEnumerable<WHSignature> whSignatures);
        public Task<WHSignature?> RemoveWHSignature(int idWHSystem, int idWHSignature);
        public Task<bool> RemoveAllWHSignature(int idWHSystem);
    }
}
