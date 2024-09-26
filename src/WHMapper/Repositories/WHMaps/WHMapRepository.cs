﻿using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext, WHMap, int>, IWHMapRepository
    {

        public WHMapRepository(ILogger<WHMapRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        

        public async Task<bool> DeleteAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var deleteRow = await context.DbWHMaps.ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }


        public async Task<WHMap?> GetByNameAsync(string mapName)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    return await context.DbWHMaps.SingleOrDefaultAsync(x => x.Name == mapName);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to get WHMap by name : {Name}", mapName);
                    return null;
                }
            }
           
          
        }


        protected override async Task<WHMap?> ACreate(WHMap item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHMaps.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHMap : {Name}", item.Name);
                    return null;
                }
            }
            
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var deleteRow = await context.DbWHMaps.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHMap>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (!await context.DbWHMaps.AnyAsync())
                    return await context.DbWHMaps.OrderBy(x => x.Name).ToListAsync();
                else
                    return await context.DbWHMaps.AsNoTracking()
                            .Include(x => x.WHAccesses)
                            //.Include(x => x.WHSystems)
                            //.Include(x => x.WHSystemLinks)
                            //    .ThenInclude(x => x.JumpHistory)
                            .OrderBy(x => x.Name)
                            .ToListAsync();
            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHMaps.AsNoTracking()
                            .Include(x => x.WHAccesses)
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .ThenInclude(x => x.JumpHistory)
                            .SingleOrDefaultAsync(x => x.Id == id);
            }
        }

        protected override async Task<WHMap?> AUpdate(int id, WHMap item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHMaps.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHMap : {Name}", item.Name);
                    return null;
                }
            }
        }

        public async Task<IEnumerable<WHAccess>?> GetMapAccesses(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var map =  await context.DbWHMaps.AsNoTracking()
                            .Include(x => x.WHAccesses)
                            .SingleOrDefaultAsync(x => x.Id == id);

                if (map == null)
                    return null;
                else
                    return map.WHAccesses;
            }
        }

        public async Task<bool> DeleteMapAccess(int mapId, int accessId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var map = await context.DbWHMaps.Include(x => x.WHAccesses).SingleOrDefaultAsync(x => x.Id == mapId);
                if (map == null)
                    return false;

                var access = map.WHAccesses.SingleOrDefault(x => x.Id == accessId);
                if (access == null)
                    return false;

                map.WHAccesses.Remove(access);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteMapAccesses(int mapId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var map = await context.DbWHMaps.Include(x => x.WHAccesses).SingleOrDefaultAsync(x => x.Id == mapId);
                if (map == null)
                    return false;

                map.WHAccesses.Clear();
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> AddMapAccess(int mapId, int accessId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var map = await context.DbWHMaps.Include(x => x.WHAccesses).SingleOrDefaultAsync(x => x.Id == mapId);
                if (map == null)
                    return false;

                var access = await context.DbWHAccesses.SingleOrDefaultAsync(x => x.Id == accessId);
                if (access == null)
                    return false;

                map.WHAccesses.Add(access);
                await context.SaveChangesAsync();
                return true;
            }
        }


        
    }
}

