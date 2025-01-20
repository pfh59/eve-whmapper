using Microsoft.EntityFrameworkCore;
using System;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHMainAccounts;

public class WHMainAccountRepository : ADefaultRepository<WHMapperContext, WHMainAccount, int>, IWHMainAccountRepository
{
    public WHMainAccountRepository(ILogger<WHMainAccountRepository> logger,IDbContextFactory<WHMapperContext> context)
        : base(logger,context)
    {
    }

    protected override async Task<WHMainAccount?> ACreate(WHMainAccount item)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                await context.DbWHMainAccounts.AddAsync(item);
                await context.SaveChangesAsync();

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Impossible to create WHMainAccount access : {CharacterId}", item.CharacterId);
                return null;
            }
        }
    }

    protected override async Task<bool> ADeleteById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var deleteRow = await context.DbWHMainAccounts.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (deleteRow > 0)
                return true;
            else
                return false;
        }
    }

    protected override async Task<IEnumerable<WHMainAccount>?> AGetAll()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHMainAccounts.ToListAsync();
        }
    }

    protected override async Task<WHMainAccount?> AGetById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHMainAccounts.FindAsync(id);
        }
    }

    protected override async Task<WHMainAccount?> AUpdate(int id,WHMainAccount item)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                if (id != item.Id)
                    return null;

                context.DbWHMainAccounts.Update(item);
                await context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Impossible to update WHMainAccount access : {CharacterId}", item.CharacterId);
                return null;
            }
        }
    }
}
