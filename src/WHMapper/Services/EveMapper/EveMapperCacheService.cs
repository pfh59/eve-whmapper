using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.Cache;

namespace WHMapper.Services.EveMapper;

public class EveMapperCacheService : IEveMapperCacheService
{
    private readonly ILogger<EveMapperCacheService> _logger;
    private readonly ICacheService _cacheService;

    public EveMapperCacheService(ILogger<EveMapperCacheService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<bool> ClearCacheAsync<TEntity>()
        where TEntity : AEveEntity
    {
        try
        {
            string cacheKey = GetEntityCacheKey<TEntity>();
            return await _cacheService.Remove(cacheKey);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while cache");
            return false;
        }
    }

    public async Task<TEntity> GetAsync<TEntity>(int key)
        where TEntity : AEveEntity
    {
        try
        {
            string cacheKey = GetEntityCacheKey<TEntity>();
            var result = await _cacheService.Get<IEnumerable<TEntity>>(cacheKey);
            return result?.FirstOrDefault(x => x.Id == key)!;
        } 
        catch(InvalidOperationException e)
        {
            _logger.LogWarning(e, "Warning while getting entities {entity}", typeof(TEntity).Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting entities {entity}", typeof(TEntity).Name);
        }
        return null!;
    }

    public async Task<bool> AddAsync<TEntity>(TEntity entity)
        where TEntity : AEveEntity
    {
        try
        {
            string cacheKey = GetEntityCacheKey<TEntity>();
            var result = await _cacheService.Get<List<TEntity>>(cacheKey) ?? new List<TEntity>();

            if (result.Contains(entity))
            {
                _logger.LogInformation("{entityName} already in cache", typeof(TEntity).Name);
                return true;
            }

            result.Add(entity);

            return await _cacheService.Set(cacheKey, result, GetTtlForEntity<TEntity>());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while saving entity {entity}", typeof(TEntity).Name);
            return false;
        }
    }

    private static string GetEntityCacheKey<T>()
        where T : AEveEntity
    {
        return typeof(T).Name switch
        {
            "AllianceEntity" => IEveMapperCacheService.REDIS_ALLIANCE_KEY,
            "CorporationEntity" => IEveMapperCacheService.REDIS_COORPORATION_KEY,
            "CharacterEntity" => IEveMapperCacheService.REDIS_CHARACTER_KEY,
            "ShipEntity" => IEveMapperCacheService.REDIS_SHIP_KEY,
            "SystemEntity" => IEveMapperCacheService.REDIS_SYSTEM_KEY,
            "ConstellationEntity" => IEveMapperCacheService.REDIS_CONSTELLATION_KEY,
            "RegionEntity" => IEveMapperCacheService.REDIS_REGION_KEY,
            "StargateEntity" => IEveMapperCacheService.REDIS_STARTGATE_KEY,
            "GroupEntity" => IEveMapperCacheService.REDIS_GROUP_KEY,
            "WHEntity" => IEveMapperCacheService.REDIS_WORMHOLE_KEY,
            "SunEntity" => IEveMapperCacheService.REDIS_SUN_KEY,
            _ => throw new InvalidCastException("Invalid entity type"),
        };
    }

    private static TimeSpan GetTtlForEntity<T>()
        where T : AEveEntity
    {
        return typeof(T).Name switch
        {
            "CharacterEntity" => TimeSpan.FromDays(1),
            "CorporationEntity" => TimeSpan.FromDays(1),
            "AllianceEntity" => TimeSpan.FromDays(1),
            "ShipEntity" => TimeSpan.FromDays(7),
            "SystemEntity" => TimeSpan.FromDays(7),
            "ConstellationEntity" => TimeSpan.FromDays(7),
            "RegionEntity" => TimeSpan.FromDays(7),
            "StargateEntity" => TimeSpan.FromDays(7),
            "GroupEntity" => TimeSpan.FromDays(7),
            "WHEntity" => TimeSpan.FromDays(7),
            "SunEntity" => TimeSpan.FromDays(7),
            _ => TimeSpan.FromHours(1),
        };
    }
}
