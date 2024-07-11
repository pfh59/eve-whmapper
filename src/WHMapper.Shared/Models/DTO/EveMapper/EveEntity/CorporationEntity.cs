using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveAPI.Corporation;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity
{
    public class CorporationEntity : AEveEntity
    {
        public CorporationEntity(int id, Corporation entity)
            : base(id, entity.Name, EveEntityEnums.Corporation)
        {
        }

        [JsonConstructor]
        public CorporationEntity(int id, string name)
            : base(id, name, EveEntityEnums.Corporation)
        {
        }
    }
}
