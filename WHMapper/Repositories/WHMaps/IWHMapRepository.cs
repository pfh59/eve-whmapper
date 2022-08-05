using System;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public interface IWHMapRepository : IDefaultRepository<WHMap,int>
    {
        public Task<WHSystem?> AddWHSystem(int idWHMap, WHSystem whSystem);
        public Task<WHSystem?> RemoveWHSystem(int idWHMap, int idWHSystem);
    }
}

