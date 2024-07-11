using WHMapper.Shared.Models.DTO.EveAPI.Character;

namespace WHMapper.Shared.Services.EveAPI.Characters
{
    public interface ICharacterServices
    {
        Task<Character?> GetCharacter(int character_id);
    }
}
