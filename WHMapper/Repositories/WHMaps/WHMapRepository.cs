using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext, WHMap, int>, IWHMapRepository
    {
        public WHMapRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHMap?> ACreate(WHMap item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHMaps.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                semSlim.Release();
            }
            
        }

        protected override async Task<WHMap?> ADeleteById(int id)
        {

            var item = await AGetById(id);

            if (item == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                _dbContext.DbWHMaps.Remove(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<IEnumerable<WHMap>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {

                if (_dbContext.DbWHMaps.Count() == 0)
                    return await _dbContext.DbWHMaps.ToListAsync();
                else
                    return await _dbContext.DbWHMaps
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .OrderBy(x => x.Name).ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHMaps.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }

        }

        protected override async Task<WHMap?> AUpdate(int id, WHMap item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext?.DbWHMaps?.Update(item);
                await _dbContext?.SaveChangesAsync();
                return item;
            }
            finally
            {
                semSlim.Release();
            }
        }


        public async Task<WHSystem?> AddWHSystem(int idWHMap, WHSystem whSystem)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {

                _dbContext.DbWHSystems.Add(whSystem);
                map.WHSystems.Add(whSystem);

                _dbContext?.DbWHMaps?.Update(map);
                await _dbContext?.SaveChangesAsync();

                return whSystem;
            }
            finally
            {
                semSlim.Release();
            }
        }

        public async Task<WHSystem?> RemoveWHSystem(int idWHMap, int idWHSystem)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                var system = map.WHSystems.Where(x => x.Id == idWHSystem).FirstOrDefault();
                if (system == null)
                    return null;

                _dbContext.DbWHSystems.Remove(system);
                await _dbContext.SaveChangesAsync();

                return system;
            }
            finally
            {
                semSlim.Release();
            }

        }

        public async Task<WHSystem?> RemoveWHSystemByName(int idWHMap, string whSystemname)
        {

            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                var system = map.WHSystems.Where(x => x.Name == whSystemname).FirstOrDefault();
                if (system == null)
                    return null;

                _dbContext.DbWHSystems.Remove(system);
                await _dbContext.SaveChangesAsync();

                return system;
            }
            finally
            {
                semSlim.Release();
            }

        }


        public async Task<WHSystemLink?> AddWHSystemLink(int idWHMap, int idWHSystemFrom, int idWHSystemTo)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                WHSystemLink whLink = new WHSystemLink(idWHSystemFrom, idWHSystemTo);

                _dbContext.DbWHSystemLinks.Add(whLink);
                map.WHSystemLinks.Add(whLink);

                _dbContext?.DbWHMaps?.Update(map);
                await _dbContext?.SaveChangesAsync();

                return whLink;
            }
            finally
            {
                semSlim.Release();
            }
        }

        public async Task<WHSystemLink?> RemoveWHSystemLink(int idWHMap, int idWHSystemLink)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                var whLink = map.WHSystemLinks.Where(x => x.Id == idWHSystemLink).FirstOrDefault();
                if (whLink == null)
                    return null;

                _dbContext.DbWHSystemLinks.Remove(whLink);
                await _dbContext.SaveChangesAsync();

                return whLink;
            }
            finally
            {
                semSlim.Release();
            }
        }

        public async Task<WHSystemLink?> RemoveWHSystemLink(int idWHMap, int idWHSystemFrom, int idWHSystemTo)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            await semSlim.WaitAsync();
            try
            {

                var whLink = map.WHSystemLinks.Where(x => x.IdWHSystemFrom == idWHSystemFrom && x.IdWHSystemTo == idWHSystemTo).FirstOrDefault();
                if (whLink == null)
                    return null;

                _dbContext.DbWHSystemLinks.Remove(whLink);
                await _dbContext.SaveChangesAsync();

                return whLink;
            }
            finally
            {
                semSlim.Release();
            }
        }
    }
}

