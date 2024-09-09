using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHNotes
{
    public interface IWHNoteRepository : IDefaultRepository<WHNote, int>
    {
        public Task<WHNote?> Get(int mapId,int solardSystemId);
    }
}

