using System;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHNotes
{
	public interface IWHNoteRepository : IDefaultRepository<WHNote, int>
    {
        public Task<WHNote?> GetBySolarSystemId(int solardSystemId);
    }
}

