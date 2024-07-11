using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

public class ShipEntity : AEveEntity
{
    public float Mass { get; private set; }
    public ShipEntity(int id, EveAPI.Universe.Type entity)
        : base(id, entity.Name, EveEntityEnums.Ship)
    {
        Mass = entity.Mass;
    }

    [JsonConstructor]
    public ShipEntity(int id, string name, float mass)
        : base(id, name, EveEntityEnums.Ship)
    {
        Mass = mass;
    }
}


