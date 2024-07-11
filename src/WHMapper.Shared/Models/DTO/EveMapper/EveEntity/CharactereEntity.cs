using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveAPI.Character;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity
{
    public class CharactereEntity : AEveEntity
    {
        public CharactereEntity(int id, Character entity)
            : base(id, entity.Name, EveEntityEnums.Character)
        {
        }

        [JsonConstructor]
        public CharactereEntity(int id, string name)
            : base(id, name, EveEntityEnums.Character)
        {
        }
    }
}
