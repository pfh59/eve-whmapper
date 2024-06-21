using WHMapper.Models.DTO.EveAPI.Character;

namespace WHMapper.Services.EveAPI.Characters
{
    public interface ICharacterServices
    {
        Task<Character?> GetCharacter(int character_id);
    }
}
