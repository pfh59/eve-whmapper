using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;

namespace WHMapper.Services.EveAPI.Characters
{
    public interface ICharacterServices
    {
        Task<Result<Character>> GetCharacter(int character_id);
        Task<Result<Portrait>> GetCharacterPortrait(int character_id);
    }
}