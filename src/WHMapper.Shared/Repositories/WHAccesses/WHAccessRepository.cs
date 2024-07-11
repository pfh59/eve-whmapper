using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WHMapper.Shared.Data;
using WHMapper.Shared.Models.Db;

namespace WHMapper.Shared.Repositories.WHAccesses
{
    public class WHAccessRepository : ADefaultRepository<WHMapperContext, WHAccess, int>, IWHAccessRepository
    {

        public WHAccessRepository(ILogger<WHAccessRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }

        protected override async Task<WHAccess?> ACreate(WHAccess item)
        {

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHAccesses.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHAccess for : {EveEntityName}", item.EveEntityName);
                    return null;
                }

            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {

                var deleteRow = await context.DbWHAccesses.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHAccess>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHAccesses.ToListAsync();
            }
        }

        protected override async Task<WHAccess?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHAccesses.FindAsync(id);
            }
        }

        protected override async Task<WHAccess?> AUpdate(int id, WHAccess item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHAccesses.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHAccess : {EveEntityName}", item.EveEntityName);
                    return null;
                }
            }
        }
    }

}

