using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories;
using WHMapper.Repositories.WHJumpLogs;

namespace WHMapper.Repositories.WHJumpLogs
{
    public class WHJumpLogRepository : ADefaultRepository<WHMapperContext, WHJumpLog, int>, IWHJumpLogRepository
    {
        public WHJumpLogRepository(ILogger<WHJumpLogRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        protected override async Task<WHJumpLog?> ACreate(WHJumpLog item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHJumpLogs.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHJumpLog : {Id}", item.Id);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var deleteRow = await context.DbWHJumpLogs.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHJumpLog>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHJumpLogs.ToListAsync();
            }
        }

        protected override async Task<WHJumpLog?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHJumpLogs.FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        protected override async Task<WHJumpLog?> AUpdate(int id,WHJumpLog item)
        {

            if(item==null)
            {
                _logger.LogError("Impossible to update WHJumpLog, item is null");
                return null;
            }

            if (id != item.Id)
            {
                _logger.LogError("Impossible to update WHJumpLog, id is different from item.Id");
                return null;
            }

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    context.DbWHJumpLogs.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Impossible to update WHJumpLog : {Id}", item.Id);
                    return null;
                }
            }
        }
    }
}
