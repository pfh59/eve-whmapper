using Microsoft.EntityFrameworkCore;
using System;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHAdditionnalAccounts;

public class WHAdditionnalAccountRepository : ADefaultRepository<WHMapperContext, WHAdditionnalAccount, int>, IWHAdditionnalAccountRepository
{
    public WHAdditionnalAccountRepository(ILogger<WHAdditionnalAccountRepository> logger,IDbContextFactory<WHMapperContext> context)
        : base(logger,context)
    {
    }

    protected override async Task<WHAdditionnalAccount?> ACreate(WHAdditionnalAccount item)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                await context.DbWHAdditionnalAccounts.AddAsync(item);
                await context.SaveChangesAsync();

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Impossible to create WHAdditionnalAccount access : {CharacterId}", item.CharacterId);
                return null;
            }
        }
    }

    protected override async Task<bool> ADeleteById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var deleteRow = await context.DbWHAdditionnalAccounts.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (deleteRow > 0)
                return true;
            else
                return false;
        }
    }

    protected override async Task<IEnumerable<WHAdditionnalAccount>?> AGetAll()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHAdditionnalAccounts.ToListAsync();
        }
    }

    protected override async Task<WHAdditionnalAccount?> AGetById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHAdditionnalAccounts.FindAsync(id);
        }
    }

    protected override async Task<WHAdditionnalAccount?> AUpdate(int id,WHAdditionnalAccount item)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                if (id != item.Id)
                    return null;

                context.DbWHAdditionnalAccounts.Update(item);
                await context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Impossible to update WHAdditionnalAccount access : {CharacterId}", item.CharacterId);
                return null;
            }
        }
    }


}
