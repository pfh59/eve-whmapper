namespace WHMapper.Services.EveAPI.Character
{
    public interface ICharacterServices
	{
        Task<Models.DTO.EveAPI.Character.Character?> GetCharacter(int character_id);
    }
}

