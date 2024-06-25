using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperEntity
{
    const string REDIS_ALLIANCE_KEY = "alliance:list";
    const string REDIS_COORPORATION_KEY = "coorporation:list";
    const string REDIS_CHARACTER_KEY = "charactere:list";
    const string REDIS_SHIP_KEY = "ship:list";
    const string REDIS_SYSTEM_KEY = "system:list";
    const string REDIS_CONSTELLATION_KEY = "constellation:list";
    const string REDIS_REGION_KEY = "region:list";
    const string REDIS_STARTGATE_KEY = "stargate:list";
    const string REDIS_GROUP_KEY = "group:list";
    const string REDIS_WORMHOLE_KEY = "wormhole:list";
    const string REDIS_SUN_KEY = "sun:list";
    Task<CharactereEntity?> GetCharacter(int characterId);
    Task<CorporationEntity?> GetCorporation(int corporationId);
    Task<AllianceEntity?> GetAlliance(int allianceId);
    Task<ShipEntity?> GetShip(int shipTypeId);
    Task<SystemEntity?> GetSystem(int systemId);
    Task<ConstellationEntity?> GetConstellation(int constellationId);
    Task<RegionEntity?> GetRegion(int regionId);
    Task<StargateEntity?> GetStargate(int stargateId);
    Task<GroupEntity?> GetGroup(int groupId);
    Task<WHEntity?> GetWormhole(int wormholeTypeId);
    Task<SunEntity?> GetSun(int sunTypeId);
    Task<bool> ClearCharacterCache();
    Task<bool> ClearCorporationCache();
    Task<bool> ClearAllianceCache();
    Task<bool> ClearShipCache();
    Task<bool> ClearSystemCache();
    Task<bool> ClearConstellationCache();
    Task<bool> ClearRegionCache();
    Task<bool> ClearStargateCache();
    Task<bool> ClearGroupCache();
    Task<bool> ClearWormholeCache();
    Task<bool> ClearSunCache();
}