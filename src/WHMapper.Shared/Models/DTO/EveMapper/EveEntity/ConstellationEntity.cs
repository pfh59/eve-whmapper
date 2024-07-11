using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveAPI.Universe;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

public class ConstellationEntity : AEveEntity
{

    public int RegionId { get; private set; }
    public int[] Systems { get; private set; }

    public ConstellationEntity(int id, Constellation constellation)
        : base(id, constellation.Name, EveEntityEnums.Constellation)
    {
        RegionId = constellation.RegionId;
        Systems = constellation.Systems;
    }

    [JsonConstructor]
    public ConstellationEntity(int id, string name, int regionId, int[] systems)
        : base(id, name, EveEntityEnums.Constellation)
    {
        RegionId = regionId;
        Systems = systems;
    }
}
