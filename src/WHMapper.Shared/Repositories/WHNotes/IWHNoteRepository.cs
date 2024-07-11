using WHMapper.Shared.Models.Db;

namespace WHMapper.Shared.Repositories.WHNotes
{
    public interface IWHNoteRepository : IDefaultRepository<WHNote, int>
    {
        public Task<WHNote?> GetBySolarSystemId(int solardSystemId);
    }
}

