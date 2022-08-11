using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext,WHMap,int>,IWHMapRepository
    {
        public WHMapRepository(WHMapperContext context) : base (context)
        {
        }

        protected override async Task<WHMap?> ACreate(WHMap item)
        {
            try
            {
                await _dbContext.DbWHMaps.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        protected override async Task<WHMap?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.DbWHMaps.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHMap>?> AGetAll()
        {
            
            if(_dbContext.DbWHMaps.Count()==0)
                return await _dbContext.DbWHMaps.ToListAsync();
            else
                return await _dbContext.DbWHMaps
                        .Include(x => x.WHSystems)
                        .Include(x => x.WHSystemLinks)
                        .OrderBy(x => x.Name).ToListAsync();
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            return await _dbContext.DbWHMaps
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override async Task<WHMap?> AUpdate(int id, WHMap item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }


        public async Task<WHSystem?> AddWHSystem(int idWHMap, WHSystem whSystem)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            _dbContext.DbWHSystems.Add(whSystem);
            map.WHSystems.Add(whSystem);


            await this.Update(idWHMap,map);

            return whSystem;
        }

        public async Task<WHSystem?> RemoveWHSystem(int idWHMap, int idWHSystem)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            var system = map.WHSystems.Where(x => x.Id == idWHSystem).FirstOrDefault();
            if (system == null)
                return null;

            _dbContext.DbWHSystems.Remove(system);
            _dbContext.SaveChanges();

            return system;

        }

        public async Task<WHSystemLink?> AddWHSystemLink(int idWHMap, int idWHSystemFrom, int idWHSystemTo)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;


            WHSystemLink whLink = new WHSystemLink(idWHSystemFrom, idWHSystemTo);

            _dbContext.DbWHSystemLinks.Add(whLink);
            map.WHSystemLinks.Add(whLink);


            await this.Update(idWHMap, map);

            return whLink;
        }

        public async Task<WHSystemLink?> RemoveWHSystemLink(int idWHMap, int idWHSystemLink)
        {
            var map = await this.GetById(idWHMap);
            if (map == null)
                return null;

            var whLink = map.WHSystemLinks.Where(x => x.Id == idWHSystemLink).FirstOrDefault();
            if (whLink == null)
                return null;

            _dbContext.DbWHSystemLinks.Remove(whLink);
            _dbContext.SaveChanges();

            return whLink;
        }
    }
}

