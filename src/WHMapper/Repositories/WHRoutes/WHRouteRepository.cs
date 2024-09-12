using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Repositories;

namespace WHMapper
{
    public class WHRouteRepository : ADefaultRepository<WHMapperContext, WHRoute, int>, IWHRouteRepository
    {
        public WHRouteRepository(ILogger<WHRouteRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {

        }

        public async Task<IEnumerable<WHRoute>> GetRoutesByEveEntityId(int mapId,int eveEntityId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHRoutes.Where(x => x.MapId==mapId && x.EveEntityId == eveEntityId).ToListAsync();
            }
        }

        public async Task<IEnumerable<WHRoute>> GetRoutesForAll(int mapId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHRoutes.Where(x => x.MapId==mapId && x.EveEntityId==null).ToListAsync();
            }
        }

        protected override async Task<WHRoute?> ACreate(WHRoute item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHRoutes.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHRoute : {SolarSystemId}", item.SolarSystemId);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int rowDeleted = await context.DbWHRoutes.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (rowDeleted > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHRoute>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (!await context.DbWHRoutes.AnyAsync())
                    return await context.DbWHRoutes.ToListAsync();
                else
                    return await context.DbWHRoutes.OrderBy(x => x.Id)
                            .ToListAsync();
            }
        }

        protected override async Task<WHRoute?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHRoutes.SingleOrDefaultAsync(x => x.Id == id);
            }
        }

        protected override async Task<WHRoute?> AUpdate(int id, WHRoute item)
        {
            if (item == null)
            {
                _logger.LogError("Impossible to update WHRoute, item is null");
                return null;
            }

            if(item.Id!=id)
            {
                _logger.LogError("Impossible to update WHRoute, item.Id is not equal to id");
                return null;
            }

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    context.DbWHRoutes.Update(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHRoute : {SolarSystemId}", item.SolarSystemId);
                    return null;
                }
            }
        }
    }
}
