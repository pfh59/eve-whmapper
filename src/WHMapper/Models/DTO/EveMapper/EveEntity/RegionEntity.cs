using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

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
