using Microsoft.EntityFrameworkCore;
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
                            /*.Include(x => x.WHAccesses)
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                                .ThenInclude(x => x.JumpHistory)*/
                            .OrderBy(x => x.Name)
                            .ToListAsync();
            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHMaps
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .ThenInclude(x => x.JumpHistory)
                            .SingleOrDefaultAsync(x => x.Id == id);;
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
    }
}

