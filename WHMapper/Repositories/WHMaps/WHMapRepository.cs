using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext, WHMap, int>, IWHMapRepository
    {
        public WHMapRepository(IDbContextFactory<WHMapperContext> context)
            : base(context)
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
                    var deleteRow = await context.DbWHMaps.Where(x => x.Id == id).ExecuteDeleteAsync();
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

        protected override async Task<IEnumerable<WHMap>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {

                    if (context.DbWHMaps.Count() == 0)
                        return await context.DbWHMaps.ToListAsync();
                    else
                        return await context.DbWHMaps
                                .Include(x => x.WHSystems)
                                .Include(x => x.WHSystemLinks)
                                .OrderBy(x => x.Name).ToListAsync();
                }
                catch(Exception ex)
                {
                    return null;
                }

            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHMaps
                                .Include(x => x.WHSystems)
                                .Include(x => x.WHSystemLinks)
                                .OrderBy(x => x.Name).FirstOrDefaultAsync(x => x.Id==id);
                }
                catch (Exception ex)
                {
                    return null;
                }
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
                    return null;
                }
            }
        }
    }
}

