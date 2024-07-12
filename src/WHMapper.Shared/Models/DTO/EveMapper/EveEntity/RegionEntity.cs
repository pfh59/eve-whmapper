using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveAPI.Universe;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

public class RegionEntity : AEveEntity
{
    public int[] Constellations { get; private set; }

    public RegionEntity(int id, Region region)
        : base(id, region.Name, EveEntityEnums.Region)
    {
        Constellations = region.Constellations;

    }

    [JsonConstructor]
    public RegionEntity(int id, string name, int[] constellations)
        : base(id, name, EveEntityEnums.Region)
    {
        Constellations = constellations;
    }
}
