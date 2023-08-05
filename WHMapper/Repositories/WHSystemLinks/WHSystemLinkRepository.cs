using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Repositories.WHSystemLinks
{
    public class WHSystemLinkRepository : ADefaultRepository<WHMapperContext, WHSystemLink, int>, IWHSystemLinkRepository
	{
        public WHSystemLinkRepository(IDbContextFactory<WHMapperContext> context)
            : base(context)
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
                    return null;
                }
            }
        }
    }
}

