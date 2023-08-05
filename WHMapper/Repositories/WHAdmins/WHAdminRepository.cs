using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Repositories.WHAdmins
{
	public class WHAdminRepository : ADefaultRepository<WHMapperContext, WHAdmin, int>, IWHAdminRepository
    {
        public WHAdminRepository(IDbContextFactory<WHMapperContext> context)
            : base(context)
        {
        }

        protected override async Task<WHAdmin?> ACreate(WHAdmin item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHAdmins.AddAsync(item);
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
                    var deleteRow = await context.DbWHAdmins.Where(x => x.Id == id).ExecuteDeleteAsync();
                    if (deleteRow > 0)
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

        protected override async Task<IEnumerable<WHAdmin>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHAdmins.ToListAsync();
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }

        protected override async Task<WHAdmin?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHAdmins.FindAsync(id);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }

        protected override async Task<WHAdmin?> AUpdate(int id, WHAdmin item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHAdmins.Update(item);
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

