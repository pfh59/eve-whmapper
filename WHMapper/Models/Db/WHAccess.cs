using System;
using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{
	public class WHAccess
	{
        [Key]
        public int Id { get; set; }

        [Required]
        public int EveEntityId { get; set; }

        [Required]
        public WHAccessEntity EveEntity { get; set; }

        public WHAccess(int eveEntityId)
            : this(eveEntityId,WHAccessEntity.Character)
        {
            
        }

        public WHAccess(int eveEntityId, WHAccessEntity entityType)
		{
            EveEntityId = eveEntityId;
            EveEntity = entityType;
        }
	}
}

