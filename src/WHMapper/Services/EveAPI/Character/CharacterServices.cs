namespace WHMapper.Services.EveAPI.Character
{
    public class CharacterServices : AEveApiServices, ICharacterServices
    {
		public CharacterServices(HttpClient httpClient) : base(httpClient)
		{
		}

        public async Task<Models.DTO.EveAPI.Character.Character?> GetCharacter(int character_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Character.Character>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v5/characters/{0}/?datasource=tranquility", character_id));

        }
    }
}

