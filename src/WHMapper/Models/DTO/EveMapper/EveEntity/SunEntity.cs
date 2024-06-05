using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

public class SunEntity : AEveEntity
{
    public SunEntity(int id, WHMapper.Models.DTO.EveAPI.Universe.Type type)
        : base(id, type.Name, EveEntityEnums.Sun)
    {
    }

    [JsonConstructor]
    public SunEntity(int id, string name)
        : base(id, name, EveEntityEnums.Sun)
    {
    }

}
