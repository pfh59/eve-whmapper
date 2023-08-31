using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHSystems;
using static MudBlazor.CategoryTypes;

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
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHSystemLinks.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHSystemLink From/To : {0}/{1}", item.IdWHSystemFrom,item.IdWHSystemTo));
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    int rowDeleted = await context.DbWHSystemLinks.Where(x => x.Id == id).ExecuteDeleteAsync();
                    if (rowDeleted > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to delete WHSystemLink id : {0}",id));
                    return false;
                }
            }
        }

        protected override async Task<IEnumerable<WHSystemLink>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHSystemLinks.ToListAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Impossible to get all WHSystemLinks");
                    return null;
                }
            }
        }

        protected override async Task<WHSystemLink?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHSystemLinks.FindAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHSystemLink by id : {0}", id));
                    return null;
                }
            }
        }

        protected override async Task<WHSystemLink?> AUpdate(int id, WHSystemLink item)
        {
            using (var context = _contextFactory.CreateDbContext())
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
                    _logger.LogError(ex, String.Format("Impossible to update WHSystemLink From/To : {0}/{1}", item.IdWHSystemFrom,item.IdWHSystemTo));
                    return null;
                }
            }
        }
    }
}

