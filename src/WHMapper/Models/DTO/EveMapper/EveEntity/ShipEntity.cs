using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

public class ShipEntity: AEveEntity
{
        public float Mass {get;private set;}
        public ShipEntity(int id, Models.DTO.EveAPI.Universe.Type entity)
            : base(id, entity.Name, EveEntityEnums.Ship)
        {
            Mass = entity.Mass;
        }

        [JsonConstructor]
        public ShipEntity(int id, string name,float mass)
            : base(id, name, EveEntityEnums.Ship)
        {
            Mass = mass;
        }
}


