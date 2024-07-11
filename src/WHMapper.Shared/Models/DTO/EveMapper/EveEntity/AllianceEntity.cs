using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity
{

    public class AllianceEntity : AEveEntity
    {
        public AllianceEntity(int id, Alliance entity) 
            : base(id, entity.Name, EveEntityEnums.Alliance)
        {
        }

        [JsonConstructor]
        public AllianceEntity(int id, string name)
            : base(id, name, EveEntityEnums.Alliance)
        {
        }
    }
}
