using Microsoft.Extensions.Logging;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveAPI;

namespace WHMapper.Services.EveMapper;

public class EveMapperService : IEveMapperService
{
    private readonly ILogger<EveMapperService> _logger;
    private readonly IEveMapperCacheService _cacheService;
    private readonly IEveAPIServices _eveApiService;

    public EveMapperService(ILogger<EveMapperService> logger, IEveMapperCacheService cacheService, IEveAPIServices eveApiService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _eveApiService = eveApiService;
    }

    private async Task<TEntity?> Get<TEntity, TEveApiEntity>(
        int key,
        Func<IEveAPIServices, Task<TEveApiEntity?>> getEveApiEntityAction,
        Func<TEveApiEntity, TEntity> entityMap
    )
        where TEntity : AEveEntity
    {
        try
        {
            // Get from cache
            var result = await _cacheService.GetAsync<TEntity>(key);
            if (result != null)
            {
                return result;
            }

            // Get from api if cache is empty
            var apiResult = await getEveApiEntityAction.Invoke(_eveApiService);
            if (EqualityComparer<TEveApiEntity>.Default.Equals(apiResult, default(TEveApiEntity)))
            {
                _logger.LogWarning($"{nameof(TEntity)} with Id {key} not found");
                return null;
            }

            // Add to cache (fire and forget) and return the entity
            var entity = entityMap(apiResult);
            await _cacheService.AddAsync(entity);
            return entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error while getting {nameof(TEntity)} with Id {key}");
        }

        return null;
    }

    public async Task<AllianceEntity?> GetAlliance(int allianceId)
    {
        return await Get(allianceId,
            x => x.AllianceServices.GetAlliance(allianceId),
            x => new AllianceEntity(allianceId, x)
        );
    }

    public async Task<CharactereEntity?> GetCharacter(int characterId)
    {
        return await Get(characterId,
            x => x.CharacterServices.GetCharacter(characterId),
            x => new CharactereEntity(characterId, x)
        );
    }

    public async Task<CorporationEntity?> GetCorporation(int corporationId)
    {
        return await Get(corporationId,
            x => x.CorporationServices.GetCorporation(corporationId),
            x => new CorporationEntity(corporationId, x));
    }

    public async Task<ShipEntity?> GetShip(int shipTypeId)
    {
        return await Get(shipTypeId,
            x => x.UniverseServices.GetType(shipTypeId),
            x => new ShipEntity(shipTypeId, x));
    }

    public async Task<SystemEntity?> GetSystem(int systemId)
    {
        return await Get(systemId,
            x => x.UniverseServices.GetSystem(systemId),
            x => new SystemEntity(systemId, x));
    }

    public async Task<ConstellationEntity?> GetConstellation(int constellationId)
    {
        return await Get(constellationId,
            x => x.UniverseServices.GetConstellation(constellationId),
            x => new ConstellationEntity(constellationId, x));
    }

    public async Task<RegionEntity?> GetRegion(int regionId)
    {
        return await Get(regionId,
            x => x.UniverseServices.GetRegion(regionId),
            x => new RegionEntity(regionId, x));
    }

    public async Task<StargateEntity?> GetStargate(int stargateId)
    {
        return await Get(stargateId,
            x => x.UniverseServices.GetStargate(stargateId),
            x => new StargateEntity(stargateId, x));
    }

    public async Task<GroupEntity?> GetGroup(int groupId)
    {
        return await Get(groupId,
            x => x.UniverseServices.GetGroup(groupId),
            x => new GroupEntity(groupId, x));
    }

    public async Task<WHEntity?> GetWormhole(int wormholeTypeId)
    {
        return await Get(wormholeTypeId,
            x => x.UniverseServices.GetType(wormholeTypeId),
            x => new WHEntity(wormholeTypeId, x));
    }

    public async Task<SunEntity?> GetSun(int sunTypeId)
    {
        return await Get(sunTypeId,
            x => x.UniverseServices.GetType(sunTypeId),
            x => new SunEntity(sunTypeId, x));
    }
}
