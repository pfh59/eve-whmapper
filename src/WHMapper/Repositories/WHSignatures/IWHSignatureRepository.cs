using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSignatures
{

    public interface IWHSignatureRepository : IDefaultRepository<WHSignature, int>
    {
        Task<WHSignature?> GetByName(string name);
        Task<IEnumerable<WHSignature?>?> Update(IEnumerable<WHSignature> whSignatures);
        Task<IEnumerable<WHSignature>?> GetByWHId(int whid);
        Task<bool> DeleteByWHId(int whid);
        Task<IEnumerable<WHSignature?>?> Create(IEnumerable<WHSignature> whSignatures);
    }
}

