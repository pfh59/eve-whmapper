using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveAPI.Corporation;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;

namespace WHMapper.Services.EveMapper;

public class EveMapperEntity : IEveMapperEntity
{
    private readonly ILogger<EveMapperEntity> _logger;
    private readonly ICacheService _cacheService;
    private readonly IEveAPIServices _eveApiService;

    public EveMapperEntity(ILogger<EveMapperEntity> logger, ICacheService cacheService, IEveAPIServices eveApiService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _eveApiService = eveApiService;
    }

    private Task<string> GetEntityCacheKey<T>() where T : AEveEntity
    {
        switch (typeof(T).Name)
        {
            case "AllianceEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_ALLIANCE_KEY);
            case "CorporationEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_COORPORATION_KEY);
            case "CharactereEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_CHARACTER_KEY);
            case "ShipEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_SHIP_KEY);
            case "SystemEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_SYSTEM_KEY);
            case "ConstellationEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_CONSTELLATION_KEY);
            case "RegionEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_REGION_KEY);
            case "StargateEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_STARTGATE_KEY);
            case "GroupEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_GROUP_KEY);
            case "WHEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_WORMHOLE_KEY);
            case "SunEntity":
                return Task.FromResult(IEveMapperEntity.REDIS_SUN_KEY);
            default:
                throw new InvalidCastException("Invalid entity type");
        }
    }

    private async Task<IEnumerable<T>?> GetEntitiesFromCache<T>() where T : AEveEntity
    {
        try
        {
            string redis_key = await GetEntityCacheKey<T>();
            return await _cacheService.Get<IEnumerable<T>>(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting entities {entity}", typeof(T).Name);
            return null;
        }
    }

    private async Task<bool> SetEntityCahing<T>(T entity) where T : AEveEntity
    {
        try
        {
            IEnumerable<T>? results = await GetEntitiesFromCache<T>();
            if (results == null)
            {
                results = new HashSet<T>();
            }

            if (results.Contains(entity))
            {
                _logger.LogInformation("{entityName} already in cache", typeof(T).Name);
                return true;
            }

            string redis_key = await GetEntityCacheKey<T>();
            return await _cacheService.Set(redis_key, results.Append(entity));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while saving entity {entity}", typeof(T).Name);
            return false;
        }
    }


    public async Task<AllianceEntity?> GetAlliance(int allianceId)
    {
        try
        {
            IEnumerable<AllianceEntity>? results = await GetEntitiesFromCache<AllianceEntity>();
            if (results != null)
            {
                AllianceEntity? entityItem = results.FirstOrDefault(x => x.Id == allianceId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get alliance from API
            Alliance? alliance = await _eveApiService.AllianceServices.GetAlliance(allianceId);
            if (alliance == null)
            {
                _logger.LogWarning("Alliance {allianceId} not found", allianceId);
                return null;
            }

            //put on cache and return return entity
            AllianceEntity entity = new AllianceEntity(allianceId, alliance);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting alliance {allianceId}", allianceId);
            return null;
        }
    }

    public async Task<CharactereEntity?> GetCharacter(int characterId)
    {
        try
        {
            IEnumerable<CharactereEntity>? results = await GetEntitiesFromCache<CharactereEntity>();
            if (results != null)
            {
                CharactereEntity? entityItem = results.FirstOrDefault(x => x.Id == characterId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get character from API
            Character? character = await _eveApiService.CharacterServices.GetCharacter(characterId);
            if (character == null)
            {
                _logger.LogWarning("Character {characterId} not found", characterId);
                return null;
            }

            //put on cache and return return entity
            CharactereEntity entity = new CharactereEntity(characterId, character);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting character {characterId}", characterId);
            return null;
        }
    }

    public async Task<CorporationEntity?> GetCorporation(int corporationId)
    {
        try
        {
            IEnumerable<CorporationEntity>? results = await GetEntitiesFromCache<CorporationEntity>();
            if (results != null)
            {
                CorporationEntity? entityItem = results.FirstOrDefault(x => x.Id == corporationId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get corporation from API
            Corporation? corporation = await _eveApiService.CorporationServices.GetCorporation(corporationId);
            if (corporation == null)
            {
                _logger.LogWarning("Corporation {corporationId} not found", corporationId);
                return null;
            }

            //put on cache and return return entity
            CorporationEntity entity = new CorporationEntity(corporationId, corporation);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting corporation {corporationId}", corporationId);
            return null;
        }
    }

    public async Task<ShipEntity?> GetShip(int shipTypeId)
    {
        try
        {
            IEnumerable<ShipEntity>? results = await GetEntitiesFromCache<ShipEntity>();
            if (results != null)
            {
                ShipEntity? entityItem = results.FirstOrDefault(x => x.Id == shipTypeId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get ship from API
            Models.DTO.EveAPI.Universe.Type? ship = await _eveApiService.UniverseServices.GetType(shipTypeId);
            if (ship == null)
            {
                _logger.LogWarning("Ship {shipTypeId} not found", shipTypeId);
                return null;
            }

            //put on cache and return return entity
            ShipEntity entity = new ShipEntity(shipTypeId, ship);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting ship {shipTypeId}", shipTypeId);
            return null;
        }
    }

    public async Task<SystemEntity?> GetSystem(int systemId)
    {
        try
        {

            IEnumerable<SystemEntity>? results = await GetEntitiesFromCache<SystemEntity>();
            if (results != null)
            {
                SystemEntity? entityItem = results.FirstOrDefault(x => x.Id == systemId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get system from API
            ESISolarSystem? system = await _eveApiService.UniverseServices.GetSystem(systemId);
            if (system == null)
            {
                _logger.LogWarning("System {systemId} not found", systemId);
                return null;
            }

            //put on cache and return return entity
            SystemEntity entity = new SystemEntity(systemId, system);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting system {systemId}", systemId);
            return null;
        }
    }

    public async Task<ConstellationEntity?> GetConstellation(int constellationId)
    {
        try
        {
            IEnumerable<ConstellationEntity>? results = await GetEntitiesFromCache<ConstellationEntity>();
            if (results != null)
            {
                ConstellationEntity? entityItem = results.FirstOrDefault(x => x.Id == constellationId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get constellation from API
            Constellation? constellation = await _eveApiService.UniverseServices.GetConstellation(constellationId);
            if (constellation == null)
            {
                _logger.LogWarning("Constellation {constellationId} not found", constellationId);
                return null;
            }

            //put on cache and return return entity
            ConstellationEntity entity = new ConstellationEntity(constellationId, constellation);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting constellation {constellationId}", constellationId);
            return null;
        }
    }

    public async Task<RegionEntity?> GetRegion(int regionId)
    {
        try
        {
            IEnumerable<RegionEntity>? results = await GetEntitiesFromCache<RegionEntity>();
            if (results != null)
            {
                RegionEntity? entityItem = results.FirstOrDefault(x => x.Id == regionId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get region from API
            Region? region = await _eveApiService.UniverseServices.GetRegion(regionId);
            if (region == null)
            {
                _logger.LogWarning("Region {regionId} not found", regionId);
                return null;
            }

            //put on cache and return return entity
            RegionEntity entity = new RegionEntity(regionId, region);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting region {regionId}", regionId);
            return null;
        }
    }

    public async Task<StargateEntity?> GetStargate(int stargateId)
    {
        try
        {
            IEnumerable<StargateEntity>? results = await GetEntitiesFromCache<StargateEntity>();
            if (results != null)
            {
                StargateEntity? entityItem = results.FirstOrDefault(x => x.Id == stargateId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get stargate from API
            Stargate? stargate = await _eveApiService.UniverseServices.GetStargate(stargateId);
            if (stargate == null)
            {
                _logger.LogWarning("Stargate {stargateId} not found", stargateId);
                return null;
            }

            //put on cache and return return entity
            StargateEntity entity = new StargateEntity(stargateId, stargate);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting stargate {stargateId}", stargateId);
            return null;
        }
    }

    public async Task<GroupEntity?> GetGroup(int groupId)
    {
        try
        {
            IEnumerable<GroupEntity>? results = await GetEntitiesFromCache<GroupEntity>();
            if (results != null)
            {
                GroupEntity? entityItem = results.FirstOrDefault(x => x.Id == groupId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get group from API
            Group? group = await _eveApiService.UniverseServices.GetGroup(groupId);
            if (group == null)
            {
                _logger.LogWarning("Group {groupId} not found", groupId);
                return null;
            }

            //put on cache and return return entity
            GroupEntity entity = new GroupEntity(groupId, group);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting group {groupId}", groupId);
            return null;
        }
    }

    public async Task<WHEntity?> GetWormhole(int wormholeTypeId)
    {
        try
        {
            IEnumerable<WHEntity>? results = await GetEntitiesFromCache<WHEntity>();
            if (results != null)
            {
                WHEntity? entityItem = results.FirstOrDefault(x => x.Id == wormholeTypeId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get wormhole from API
            Models.DTO.EveAPI.Universe.Type? wormhole = await _eveApiService.UniverseServices.GetType(wormholeTypeId);
            if (wormhole == null)
            {
                _logger.LogWarning("Wormhole {wormholeTypeId} not found", wormholeTypeId);
                return null;
            }

            //put on cache and return return entity
            WHEntity entity = new WHEntity(wormholeTypeId, wormhole);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting wormhole {wormholeTypeId}", wormholeTypeId);
            return null;
        }
    }

    public async Task<SunEntity?> GetSun(int sunTypeId)
    {
        try
        {
            IEnumerable<SunEntity>? results = await GetEntitiesFromCache<SunEntity>();
            if (results != null)
            {
                SunEntity? entityItem = results.FirstOrDefault(x => x.Id == sunTypeId);

                if (entityItem != null)
                {
                    _logger.LogInformation("{entityName} found in cache", entityItem.Name);
                    return entityItem;
                }
            }

            // Get sun from API
            Models.DTO.EveAPI.Universe.Type? sun = await _eveApiService.UniverseServices.GetType(sunTypeId);
            if (sun == null)
            {
                _logger.LogWarning("Sun {sunTypeId} not found", sunTypeId);
                return null;
            }

            //put on cache and return return entity
            SunEntity entity = new SunEntity(sunTypeId, sun);
            if (await SetEntityCahing(entity))
            {
                return entity;
            }
            else
            {
                _logger.LogError("Error while saving entity {entity} on cache", entity.Name);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting sun {sunTypeId}", sunTypeId);
            return null;
        }

    }

    public async Task<bool> ClearAllianceCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<AllianceEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing alliance cache");
            return false;
        }
    }

    public async Task<bool> ClearCharacterCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<CharactereEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing character cache");
            return false;
        }
    }

    public async Task<bool> ClearCorporationCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<CorporationEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing corporation cache");
            return false;
        }
    }

    public async Task<bool> ClearShipCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<ShipEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing ship cache");
            return false;
        }
    }

    public async Task<bool> ClearSystemCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<SystemEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing system cache");
            return false;
        }
    }

    public async Task<bool> ClearConstellationCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<ConstellationEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing constellation cache");
            return false;
        }
    }

    public async Task<bool> ClearRegionCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<RegionEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing region cache");
            return false;
        }
    }

    public async Task<bool> ClearStargateCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<StargateEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing stargate cache");
            return false;
        }
    }

    public async Task<bool> ClearGroupCache()
    {
        try
        {
            string redis_key = await GetEntityCacheKey<GroupEntity>();
            return await _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing group cache");
            return false;
        }
    }

    public Task<bool> ClearWormholeCache()
    {
        try
        {
            string redis_key = IEveMapperEntity.REDIS_WORMHOLE_KEY;
            return _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing wormhole cache");
            return Task.FromResult(false);
        }
    }

    public Task<bool> ClearSunCache()
    {
        try
        {
            string redis_key = IEveMapperEntity.REDIS_SUN_KEY;
            return _cacheService.Remove(redis_key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while clearing sun cache");
            return Task.FromResult(false);
        }
    }
}
