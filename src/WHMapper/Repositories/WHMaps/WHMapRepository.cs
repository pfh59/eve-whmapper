using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAccesses;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext, WHMap, int>, IWHMapRepository
    {

        public WHMapRepository(ILogger<WHMapRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        protected override async Task<WHMap?> ACreate(WHMap item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHMaps.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHMap : {0}", item.Name));
                    return null;
                }
            }
            
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
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
            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.DbWHMaps.Count() == 0)
                    return await context.DbWHMaps.ToListAsync();
                else
                    return await context.DbWHMaps
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .OrderBy(x => x.Name).ToListAsync();
            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHMaps
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .OrderBy(x => x.Name).FirstOrDefaultAsync(x => x.Id==id);
            }
        }

        protected override async Task<WHMap?> AUpdate(int id, WHMap item)
        {
            using (var context = _contextFactory.CreateDbContext())
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
                    _logger.LogError(ex, String.Format("Impossible to update WHMap : {0}", item.Name));
                    return null;
                }
            }
        }
    }
}

