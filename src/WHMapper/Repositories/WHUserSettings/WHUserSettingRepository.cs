using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHUserSettings
{
    public class WHUserSettingRepository : ADefaultRepository<WHMapperContext, WHUserSetting, int>, IWHUserSettingRepository
    {
        public WHUserSettingRepository(ILogger<WHUserSettingRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }

        protected override async Task<WHUserSetting?> ACreate(WHUserSetting item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHUserSettings.AddAsync(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHUserSetting for character: {EveCharacterId}", item.EveCharacterId);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int deleteRow = await context.DbWHUserSettings.Where(x => x.Id == id).ExecuteDeleteAsync();
                return deleteRow > 0;
            }
        }

        protected override async Task<IEnumerable<WHUserSetting>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHUserSettings.ToListAsync();
            }
        }

        protected override async Task<WHUserSetting?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHUserSettings.SingleOrDefaultAsync(x => x.Id == id);
            }
        }

        protected override async Task<WHUserSetting?> AUpdate(int id, WHUserSetting item)
        {
            if (item == null)
            {
                _logger.LogError("Impossible to update WHUserSetting, item is null");
                return null;
            }

            if (id != item.Id)
            {
                _logger.LogError("Impossible to update WHUserSetting, id is different from item.Id");
                return null;
            }

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    context.DbWHUserSettings.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHUserSetting for character: {EveCharacterId}", item.EveCharacterId);
                    return null;
                }
            }
        }

        protected override async Task<int> AGetCountAsync()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHUserSettings.CountAsync();
            }
        }

        public async Task<WHUserSetting?> GetByCharacterId(int eveCharacterId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHUserSettings.SingleOrDefaultAsync(x => x.EveCharacterId == eveCharacterId);
            }
        }

        public async Task<bool> DeleteByCharacterId(int eveCharacterId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int deleteRow = await context.DbWHUserSettings.Where(x => x.EveCharacterId == eveCharacterId).ExecuteDeleteAsync();
                return deleteRow > 0;
            }
        }
    }
}
