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

        public async Task<IEnumerable<WHRoute>> GetRoutesByEveEntityId(int eveEntityId)
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHRoutes.Where(x => x.EveEntityId == eveEntityId).ToListAsync();
            }
        }

        public async Task<IEnumerable<WHRoute>> GetRoutesForAll()
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHRoutes.Where(x => x.EveEntityId==null).ToListAsync();
            }
        }

        protected override async Task<WHRoute?> ACreate(WHRoute item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHRoutes.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHRoute : {0}", item.SolarSystemId));
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
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
            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.DbWHRoutes.Count() == 0)
                    return await context.DbWHRoutes.ToListAsync();
                else
                    return await context.DbWHRoutes.OrderBy(x => x.Id)
                            .ToListAsync();
            }
        }

        protected override async Task<WHRoute?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
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

            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    context.DbWHRoutes.Update(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to update WHRoute : {0}", item.SolarSystemId));
                    return null;
                }
            }
        }
    }
}
