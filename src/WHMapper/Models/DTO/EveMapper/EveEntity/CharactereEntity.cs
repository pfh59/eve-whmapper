using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity
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
