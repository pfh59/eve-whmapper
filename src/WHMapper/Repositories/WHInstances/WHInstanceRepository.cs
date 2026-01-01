using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Repositories.WHInstances
{
    public class WHInstanceRepository : ADefaultRepository<WHMapperContext, WHInstance, int>, IWHInstanceRepository
    {
        public WHInstanceRepository(ILogger<WHInstanceRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }

        protected override async Task<IEnumerable<WHInstance>?> AGetAll()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances
                .Include(x => x.Administrators)
                .Include(x => x.InstanceAccesses)
                .ToListAsync();
        }

        protected override async Task<WHInstance?> AGetById(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances
                .Include(x => x.Administrators)
                .Include(x => x.InstanceAccesses)
                .Include(x => x.WHMaps)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override async Task<WHInstance?> ACreate(WHInstance item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var result = await context.DbWHInstances.AddAsync(item);
                await context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WHInstance");
                return null;
            }
        }

        protected override async Task<WHInstance?> AUpdate(int id, WHInstance item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var existing = await context.DbWHInstances.FindAsync(id);
                if (existing == null)
                    return null;

                existing.Name = item.Name;
                existing.Description = item.Description;
                existing.IsActive = item.IsActive;
                existing.OwnerEveEntityId = item.OwnerEveEntityId;
                existing.OwnerEveEntityName = item.OwnerEveEntityName;
                existing.OwnerType = item.OwnerType;
                existing.CreatorCharacterId = item.CreatorCharacterId;
                existing.CreatorCharacterName = item.CreatorCharacterName;

                await context.SaveChangesAsync();
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WHInstance {Id}", id);
                return null;
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var instance = await context.DbWHInstances.FindAsync(id);
                if (instance == null)
                    return false;

                context.DbWHInstances.Remove(instance);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WHInstance {Id}", id);
                return false;
            }
        }

        protected override async Task<int> AGetCountAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances.CountAsync();
        }

        public async Task<WHInstance?> GetByOwnerAsync(int ownerEveEntityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances
                .Include(x => x.Administrators)
                .Include(x => x.InstanceAccesses)
                .FirstOrDefaultAsync(x => x.OwnerEveEntityId == ownerEveEntityId);
        }

        public async Task<IEnumerable<WHInstance>?> GetInstancesForAdminAsync(int characterId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances
                .Include(x => x.Administrators)
                .Include(x => x.InstanceAccesses)
                .Where(x => x.Administrators.Any(a => a.EveCharacterId == characterId))
                .ToListAsync();
        }

        public async Task<IEnumerable<WHInstance>?> GetAccessibleInstancesAsync(int characterId, int? corporationId, int? allianceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstances
                .Include(x => x.Administrators)
                .Include(x => x.InstanceAccesses)
                .Include(x => x.WHMaps)
                .Where(x => x.IsActive && (
                    // User is an admin
                    x.Administrators.Any(a => a.EveCharacterId == characterId) ||
                    // User has direct access
                    x.InstanceAccesses.Any(a => 
                        (a.EveEntityId == characterId && a.EveEntity == WHAccessEntity.Character) ||
                        (corporationId.HasValue && a.EveEntityId == corporationId.Value && a.EveEntity == WHAccessEntity.Corporation) ||
                        (allianceId.HasValue && a.EveEntityId == allianceId.Value && a.EveEntity == WHAccessEntity.Alliance))
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<WHInstanceAdmin>?> GetInstanceAdminsAsync(int instanceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstanceAdmins
                .Where(x => x.WHInstanceId == instanceId)
                .ToListAsync();
        }

        public async Task<WHInstanceAdmin?> AddInstanceAdminAsync(int instanceId, int characterId, string characterName, bool isOwner = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var admin = new WHInstanceAdmin(instanceId, characterId, characterName, isOwner);
                var result = await context.DbWHInstanceAdmins.AddAsync(admin);
                await context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding admin to instance {InstanceId}", instanceId);
                return null;
            }
        }

        public async Task<bool> RemoveInstanceAdminAsync(int instanceId, int characterId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var admin = await context.DbWHInstanceAdmins
                    .FirstOrDefaultAsync(x => x.WHInstanceId == instanceId && x.EveCharacterId == characterId);
                
                if (admin == null)
                    return false;

                // Prevent removing the owner admin
                if (admin.IsOwner)
                {
                    _logger.LogWarning("Cannot remove owner admin from instance {InstanceId}", instanceId);
                    return false;
                }

                context.DbWHInstanceAdmins.Remove(admin);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing admin from instance {InstanceId}", instanceId);
                return false;
            }
        }

        public async Task<bool> IsInstanceAdminAsync(int instanceId, int characterId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstanceAdmins
                .AnyAsync(x => x.WHInstanceId == instanceId && x.EveCharacterId == characterId);
        }

        public async Task<IEnumerable<WHInstanceAccess>?> GetInstanceAccessesAsync(int instanceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHInstanceAccesses
                .Where(x => x.WHInstanceId == instanceId)
                .ToListAsync();
        }

        public async Task<WHInstanceAccess?> AddInstanceAccessAsync(WHInstanceAccess access)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var result = await context.DbWHInstanceAccesses.AddAsync(access);
                await context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding access to instance {InstanceId}", access.WHInstanceId);
                return null;
            }
        }

        public async Task<bool> RemoveInstanceAccessAsync(int instanceId, int accessId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var access = await context.DbWHInstanceAccesses
                    .FirstOrDefaultAsync(x => x.WHInstanceId == instanceId && x.Id == accessId);
                
                if (access == null)
                    return false;

                context.DbWHInstanceAccesses.Remove(access);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing access from instance {InstanceId}", instanceId);
                return false;
            }
        }

        public async Task<IEnumerable<WHMap>?> GetInstanceMapsAsync(int instanceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DbWHMaps
                .Include(x => x.WHMapAccesses)
                .Where(x => x.WHInstanceId == instanceId)
                .ToListAsync();
        }

        public async Task<bool> HasInstanceAccessAsync(int instanceId, int characterId, int? corporationId, int? allianceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Check if user is an admin
            var isAdmin = await context.DbWHInstanceAdmins
                .AnyAsync(x => x.WHInstanceId == instanceId && x.EveCharacterId == characterId);
            
            if (isAdmin)
                return true;

            // Check access entries
            return await context.DbWHInstanceAccesses
                .AnyAsync(x => x.WHInstanceId == instanceId && (
                    (x.EveEntityId == characterId && x.EveEntity == WHAccessEntity.Character) ||
                    (corporationId.HasValue && x.EveEntityId == corporationId.Value && x.EveEntity == WHAccessEntity.Corporation) ||
                    (allianceId.HasValue && x.EveEntityId == allianceId.Value && x.EveEntity == WHAccessEntity.Alliance)
                ));
        }
    }
}
