using System;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSignatures
{

    public interface IWHSignatureRepository : IDefaultRepository<WHSignature, int>
    {
        public Task<WHSignature?> GetByName(string name);
        public Task<IEnumerable<WHSignature?>> Update(IEnumerable<WHSignature> whSignatures);
    }
}

