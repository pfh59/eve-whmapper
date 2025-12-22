using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Repositories.WHMapAccesses
{
    public class WHMapAccessRepository : ADefaultRepository<WHMapperContext, WHMapAccess, int>, IWHMapAccessRepository
    {
        public WHMapAccessRepository(ILogger<WHMapAccessRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }

        protected override async Task<IEnumerable<WHMapAccess>?> AGetAll()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses.ToListAsync();
        }

        protected override async Task<WHMapAccess?> AGetById(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses.FindAsync(id);
        }

        protected override async Task<WHMapAccess?> ACreate(WHMapAccess item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var result = await context.DbWHMapAccesses.AddAsync(item);
                await context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WHMapAccess");
                return null;
            }
        }

        protected override async Task<WHMapAccess?> AUpdate(int id, WHMapAccess item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var existing = await context.DbWHMapAccesses.FindAsync(id);
                if (existing == null)
                    return null;

                existing.EveEntityId = item.EveEntityId;
                existing.EveEntityName = item.EveEntityName;
                existing.EveEntity = item.EveEntity;

                await context.SaveChangesAsync();
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WHMapAccess {Id}", id);
                return null;
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var access = await context.DbWHMapAccesses.FindAsync(id);
                if (access == null)
                    return false;

                context.DbWHMapAccesses.Remove(access);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WHMapAccess {Id}", id);
                return false;
            }
        }

        protected override async Task<int> AGetCountAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses.CountAsync();
        }

        public async Task<IEnumerable<WHMapAccess>?> GetMapAccessesAsync(int mapId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses
                .Where(x => x.WHMapId == mapId)
                .ToListAsync();
        }

        public async Task<bool> HasAccessRestrictionsAsync(int mapId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses.AnyAsync(x => x.WHMapId == mapId);
        }

        public async Task<bool> HasMapAccessAsync(int mapId, int characterId, int? corporationId, int? allianceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // If no access restrictions exist, everyone with instance access can view the map
            var hasRestrictions = await context.DbWHMapAccesses.AnyAsync(x => x.WHMapId == mapId);
            if (!hasRestrictions)
                return true;

            // Check if the entity has explicit access
            return await context.DbWHMapAccesses.AnyAsync(x =>
                x.WHMapId == mapId && (
                    (x.EveEntityId == characterId && x.EveEntity == WHAccessEntity.Character) ||
                    (corporationId.HasValue && x.EveEntityId == corporationId.Value && x.EveEntity == WHAccessEntity.Corporation) ||
                    (allianceId.HasValue && x.EveEntityId == allianceId.Value && x.EveEntity == WHAccessEntity.Alliance)
                ));
        }

        public async Task<WHMapAccess?> AddMapAccessAsync(WHMapAccess access)
        {
            return await ACreate(access);
        }

        public async Task<bool> RemoveMapAccessAsync(int mapId, int accessId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var access = await context.DbWHMapAccesses
                    .FirstOrDefaultAsync(x => x.Id == accessId && x.WHMapId == mapId);
                
                if (access == null)
                    return false;

                context.DbWHMapAccesses.Remove(access);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing map access {AccessId} from map {MapId}", accessId, mapId);
                return false;
            }
        }

        public async Task<bool> ClearMapAccessesAsync(int mapId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var accesses = await context.DbWHMapAccesses
                    .Where(x => x.WHMapId == mapId)
                    .ToListAsync();

                context.DbWHMapAccesses.RemoveRange(accesses);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing map accesses for map {MapId}", mapId);
                return false;
            }
        }

        public async Task<int> GetMapAccessCountAsync(int mapId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMapAccesses.CountAsync(x => x.WHMapId == mapId);
        }
    }
}
