using System;
using System.ComponentModel.DataAnnotations;
using WHMapper.Models.DTO.ResponseMessage;

namespace WHMapper.Models.Db
{
	public class WHAdmin
	{
        [Key]
        public int Id { get; set; }

        [Required]
        public int EveCharacterId { get; set; }

        [Required]
        public string EveCharacterName { get; set; }=string.Empty;

        [Obsolete("EF Requires it")]
        protected WHAdmin() { }
        public WHAdmin(int eveCharacterId,string eveCharacterName)
		{
            EveCharacterId = eveCharacterId;
            EveCharacterName = eveCharacterName;
        }
	}
}

