﻿using WHMapper.Models.DTO.EveAPI.Character;

namespace WHMapper.Services.EveAPI.Characters
{
    public class CharacterServices : EveApiServiceBase, ICharacterServices
    {
        public CharacterServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Character?> GetCharacter(int character_id)
        {
            return await base.Execute<Character>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v5/characters/{0}/?datasource=tranquility", character_id));
        }
    }
}
