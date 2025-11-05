using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSystemLinks
{
    public class WHSystemLinkRepository : ADefaultRepository<WHMapperContext, WHSystemLink, int>, IWHSystemLinkRepository
	{
        public WHSystemLinkRepository(ILogger<WHSystemLinkRepository> logger,IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        protected override async Task<WHSystemLink?> ACreate(WHSystemLink item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHSystemLinks.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHSystemLink From/To : {IdWHSystemFrom}/{IdWHSystemTo}", item.IdWHSystemFrom,item.IdWHSystemTo);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int rowDeleted = await context.DbWHSystemLinks.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (rowDeleted > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHSystemLink>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (!await context.DbWHRoutes.AnyAsync())
                    return await context.DbWHSystemLinks.ToListAsync();
                else
                    return await context.DbWHSystemLinks
                            .Include(x => x.JumpHistory)
                            .ToListAsync();
            }
        }

        protected override async Task<WHSystemLink?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSystemLinks.Include(x => x.JumpHistory).SingleOrDefaultAsync(x => x.Id == id);
            }
        }

        protected override async Task<WHSystemLink?> AUpdate(int id, WHSystemLink item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHSystemLinks.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHSystemLink From/To : {IdWHSystemFrom}/{IdWHSystemTo}", item.IdWHSystemFrom, item.IdWHSystemTo);
                    return null;
                }
            }
        }

        protected override async Task<int> AGetCountAsync()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSystemLinks.CountAsync();
            }
        }
    }
}

