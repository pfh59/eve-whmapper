using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveAPI.Corporation;
using WHMapper.Models.DTO.EveAPI.Universe;
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
        Func<IEveAPIServices, TEveApiEntity> getEveApiEntityAction,
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
            var apiResult = getEveApiEntityAction.Invoke(_eveApiService);
            if (apiResult == null)
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
        return await Get<AllianceEntity, Alliance>(allianceId,
            x => x.AllianceServices.GetAlliance(allianceId).Result,
            x => new AllianceEntity(allianceId, x)
        );
    }

    public async Task<CharactereEntity?> GetCharacter(int characterId)
    {
        return await Get<CharactereEntity, Character>(characterId,
            x => x.CharacterServices.GetCharacter(characterId).Result,
            x => new CharactereEntity(characterId, x)
        );
    }

    public async Task<CorporationEntity?> GetCorporation(int corporationId)
    {
        return await Get<CorporationEntity, Corporation>(corporationId,
            x => x.CorporationServices.GetCorporation(corporationId).Result,
            x => new CorporationEntity(corporationId, x));
    }

    public async Task<ShipEntity?> GetShip(int shipTypeId)
    {
        return await Get<ShipEntity, Models.DTO.EveAPI.Universe.Type>(shipTypeId,
            x => x.UniverseServices.GetType(shipTypeId).Result,
            x => new ShipEntity(shipTypeId, x));
    }

    public async Task<SystemEntity?> GetSystem(int systemId)
    {
        return await Get<SystemEntity, ESISolarSystem>(systemId,
            x => x.UniverseServices.GetSystem(systemId).Result,
            x => new SystemEntity(systemId, x));
    }

    public async Task<ConstellationEntity?> GetConstellation(int constellationId)
    {
        return await Get<ConstellationEntity, Constellation>(constellationId,
            x => x.UniverseServices.GetConstellation(constellationId).Result,
            x => new ConstellationEntity(constellationId, x));
    }

    public async Task<RegionEntity?> GetRegion(int regionId)
    {
        return await Get<RegionEntity, Region>(regionId,
            x => x.UniverseServices.GetRegion(regionId).Result,
            x => new RegionEntity(regionId, x));
    }

    public async Task<StargateEntity?> GetStargate(int stargateId)
    {
        return await Get<StargateEntity, Stargate>(stargateId,
            x => x.UniverseServices.GetStargate(stargateId).Result,
            x => new StargateEntity(stargateId, x));
    }

    public async Task<GroupEntity?> GetGroup(int groupId)
    {
        return await Get<GroupEntity, Models.DTO.EveAPI.Universe.Group>(groupId,
            x => x.UniverseServices.GetGroup(groupId).Result,
            x => new GroupEntity(groupId, x));
    }

    public async Task<WHEntity?> GetWormhole(int wormholeTypeId)
    {
        return await Get<WHEntity, Models.DTO.EveAPI.Universe.Type>(wormholeTypeId,
            x => x.UniverseServices.GetType(wormholeTypeId).Result,
            x => new WHEntity(wormholeTypeId, x));
    }

    public async Task<SunEntity?> GetSun(int sunTypeId)
    {
        return await Get<SunEntity, Models.DTO.EveAPI.Universe.Type>(sunTypeId,
            x => x.UniverseServices.GetType(sunTypeId).Result,
            x => new SunEntity(sunTypeId, x));
    }
}
