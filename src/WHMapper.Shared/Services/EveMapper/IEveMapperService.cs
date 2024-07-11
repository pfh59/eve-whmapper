using WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Shared.Services.EveMapper;

public interface IEveMapperService
{
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
}