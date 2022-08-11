using System;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public interface IWHMapRepository : IDefaultRepository<WHMap,int>
    {
        public Task<WHSystem?> AddWHSystem(int idWHMap, WHSystem whSystem);
        public Task<WHSystem?> RemoveWHSystem(int idWHMap, int idWHSystem);

        public Task<WHSystemLink?> AddWHSystemLink(int idWHMap, int idWHSystemFrom, int idWHSystemTo);
        public Task<WHSystemLink?> RemoveWHSystemLink(int idWHMap, int idWHSystemLink);
    }
}

