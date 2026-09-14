using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.Statistics;

namespace WHMapper.Repositories.WHActivityLogs;

/// <summary>
/// Entity Framework implementation of <see cref="IWHActivityLogRepository"/>.
/// </summary>
public class WHActivityLogRepository : ADefaultRepository<WHMapperContext, WHActivityLog, int>, IWHActivityLogRepository
{
    public WHActivityLogRepository(ILogger<WHActivityLogRepository> logger, IDbContextFactory<WHMapperContext> context)
        : base(logger, context)
    {
    }

    public async Task<bool> CreateRange(IEnumerable<WHActivityLog> activities)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                await context.DbWHActivityLogs.AddRangeAsync(activities);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible to create WHActivityLog range");
                return false;
            }
        }
    }

    public async Task<IReadOnlyList<WHActivityCount>> GetCountsByCharacterAsync(int instanceId, IReadOnlyCollection<int> mapIds, IReadOnlyCollection<int> activityTypeIds, DateTime? fromUtc)
    {
        if (mapIds.Count == 0 || activityTypeIds.Count == 0)
            return Array.Empty<WHActivityCount>();

        int[] mapIdArray = mapIds.ToArray();
        int[] activityTypeIdArray = activityTypeIds.ToArray();

        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                var query = context.DbWHActivityLogs.Where(x =>
                    x.WHInstanceId == instanceId
                    && x.WHMapId.HasValue && mapIdArray.Contains(x.WHMapId.Value)
                    && activityTypeIdArray.Contains(x.WHActivityTypeId));

                if (fromUtc.HasValue)
                {
                    DateTime from = fromUtc.Value;
                    query = query.Where(x => x.ActivityDate >= from);
                }

                return await query
                    .GroupBy(x => new { x.CharacterId, x.WHActivityTypeId })
                    .Select(g => new WHActivityCount(g.Key.CharacterId, g.Key.WHActivityTypeId, g.Count()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible to count WHActivityLog for instance : {InstanceId}", instanceId);
                return Array.Empty<WHActivityCount>();
            }
        }
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                return await context.DbWHActivityLogs.Where(x => x.ActivityDate < cutoffUtc).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible to delete WHActivityLog older than : {CutoffUtc}", cutoffUtc);
                return 0;
            }
        }
    }

    protected override async Task<WHActivityLog?> ACreate(WHActivityLog item)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                await context.DbWHActivityLogs.AddAsync(item);
                await context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible to create WHActivityLog for character : {CharacterId}", item.CharacterId);
                return null;
            }
        }
    }

    protected override async Task<bool> ADeleteById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            int deleteRow = await context.DbWHActivityLogs.Where(x => x.Id == id).ExecuteDeleteAsync();
            return deleteRow > 0;
        }
    }

    protected override async Task<IEnumerable<WHActivityLog>?> AGetAll()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHActivityLogs.OrderBy(x => x.ActivityDate).ToListAsync();
        }
    }

    protected override async Task<WHActivityLog?> AGetById(int id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHActivityLogs.SingleOrDefaultAsync(x => x.Id == id);
        }
    }

    /// <summary>
    /// Always returns null: the activity log is append-only, so a recorded activity is never modified.
    /// </summary>
    protected override Task<WHActivityLog?> AUpdate(int id, WHActivityLog item)
    {
        _logger.LogWarning("Impossible to update WHActivityLog : {Id}, the activity log is append-only", id);
        return Task.FromResult<WHActivityLog?>(null);
    }

    protected override async Task<int> AGetCountAsync()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.DbWHActivityLogs.CountAsync();
        }
    }
}
