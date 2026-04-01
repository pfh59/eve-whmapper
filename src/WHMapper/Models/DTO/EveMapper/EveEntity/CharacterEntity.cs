using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity
{
    public class CharacterEntity : AEveEntity
    {
        public CharacterEntity(int id, Character entity)
            : base(id, entity.Name, EveEntityEnums.Character)
        {
        }

        [JsonConstructor]
        public CharacterEntity(int id, string name)
            : base(id, name, EveEntityEnums.Character)
        {
        }
    }
}
