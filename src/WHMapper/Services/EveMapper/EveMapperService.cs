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
        Func<IEveAPIServices, Task<TEveApiEntity>> getEveApiEntityAction,
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
            if (apiResult == null)
            {
                _logger.LogWarning("{entityname} with Id {key} not found", typeof(TEntity).Name, key);
                return null;
            }
            else
            {
                // Add to cache (fire and forget) and return the entity
                var entity = entityMap(apiResult);
                if (entity != null)
                {
                    await _cacheService.AddAsync(entity);
                    return entity;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting {entityname} with Id {key}", typeof(TEntity).Name, key);
        }
        return null;
    }

    public async Task<AllianceEntity?> GetAlliance(int allianceId)
    {
        return await Get(allianceId,
            async x => (await x.AllianceServices.GetAlliance(allianceId))?.Data,
            x => new AllianceEntity(allianceId, x)
        );
    }

    public async Task<CharactereEntity?> GetCharacter(int characterId)
    {
        return await Get(characterId,
            async x => (await x.CharacterServices.GetCharacter(characterId))?.Data,
            x => new CharactereEntity(characterId, x)
        );
    }

    public async Task<CorporationEntity?> GetCorporation(int corporationId)
    {
        return await Get(corporationId,
            async x => (await x.CorporationServices.GetCorporation(corporationId))?.Data,
            x => new CorporationEntity(corporationId, x));
    }

    public async Task<ShipEntity?> GetShip(int shipTypeId)
    {
        return await Get(shipTypeId,
            async x => (await x.UniverseServices.GetType(shipTypeId))?.Data,
            x => new ShipEntity(shipTypeId, x));
    }

    public async Task<SystemEntity?> GetSystem(int systemId)
    {
        return await Get(systemId,
            async x => (await x.UniverseServices.GetSystem(systemId))?.Data,
            x => new SystemEntity(systemId, x));
    }

    public async Task<ConstellationEntity?> GetConstellation(int constellationId)
    {
        return await Get(constellationId,
            async x => (await x.UniverseServices.GetConstellation(constellationId))?.Data,
            x => new ConstellationEntity(constellationId, x));
    }

    public async Task<RegionEntity?> GetRegion(int regionId)
    {
        return await Get(regionId,
            async x => (await x.UniverseServices.GetRegion(regionId))?.Data,
            x => new RegionEntity(regionId, x));
    }

    public async Task<StargateEntity?> GetStargate(int stargateId)
    {
        return await Get(stargateId,
            async x => (await x.UniverseServices.GetStargate(stargateId))?.Data,
            x => new StargateEntity(stargateId, x));
    }

    public async Task<GroupEntity?> GetGroup(int groupId)
    {
        return await Get(groupId,
            async x => (await x.UniverseServices.GetGroup(groupId))?.Data,
            x => new GroupEntity(groupId, x));
    }

    public async Task<WHEntity?> GetWormhole(int wormholeTypeId)
    {
        return await Get(wormholeTypeId,
            async x => (await x.UniverseServices.GetType(wormholeTypeId))?.Data,
            x => new WHEntity(wormholeTypeId, x));
    }

    public async Task<SunEntity?> GetSun(int sunTypeId)
    {
        return await Get(sunTypeId,
            async x => (await x.UniverseServices.GetType(sunTypeId))?.Data,
            x => new SunEntity(sunTypeId, x));
    }
}
