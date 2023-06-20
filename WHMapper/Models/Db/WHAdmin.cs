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


        public WHAdmin(int eveCharacterId)
		{
            EveCharacterId = eveCharacterId;

        }
	}
}

