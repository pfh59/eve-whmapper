using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSignatures
{

    public interface IWHSignatureRepository : IDefaultRepository<WHSignature, int>
    {
        public Task<WHSignature?> GetByName(string name);
        public Task<IEnumerable<WHSignature?>?> Update(IEnumerable<WHSignature> whSignatures);
        public Task<IEnumerable<WHSignature>?> GetByWHId(int whid);
        public Task<bool> DeleteByWHId(int whid);
        public Task<IEnumerable<WHSignature?>?> Create(IEnumerable<WHSignature> whSignatures);

    }
}

