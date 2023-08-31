using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Services.EveAPI;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHAccesses
{
    public class WHAccessRepository : ADefaultRepository<WHMapperContext, WHAccess, int>, IWHAccessRepository
    {

        public WHAccessRepository(ILogger<WHAccessRepository> logger,IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        protected override async Task<WHAccess?> ACreate(WHAccess item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHAccesses.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHAccess for : {0}",item.EveEntityName));
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
                    var deleteRow = await context.DbWHAccesses.Where(x => x.Id == id).ExecuteDeleteAsync();
                    if (deleteRow > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to delete WHAccess id : {0}", id));
                    return false;
                }
            }
        }

        protected override async Task<IEnumerable<WHAccess>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHAccesses.ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to get all WHAccesses");
                    return null;
                }
            }
        }

        protected override async Task<WHAccess?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {

                try
                {
                    return await context.DbWHAccesses.FindAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHAccess by id : {0}", id));
                    return null;
                }
            }
        }

        protected override async Task<WHAccess?> AUpdate(int id, WHAccess item)
        {
            using (var context = _contextFactory.CreateDbContext())
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
                    _logger.LogError(ex, String.Format("Impossible to update WHAccess : {0}", item.EveEntityName));
                    return null;
                }
            }
        }
    }

}

