using System.ComponentModel.DataAnnotations;
using WHMapper.Shared.Models.Db.Enums;

namespace WHMapper.Shared.Models.Db
{
    public class WHAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EveEntityId { get; set; }

        [Required]
        public string EveEntityName { get; set; } = string.Empty;

        [Required]
        public WHAccessEntity EveEntity { get; set; }


        [Obsolete("EF Requires it")]
        protected WHAccess() { }
        public WHAccess(int eveEntityId, string eveEntityName) : this(eveEntityId, eveEntityName, WHAccessEntity.Character)
        {

        }

        public WHAccess(int eveEntityId, string eveEntityName, WHAccessEntity entityType)
        {
            EveEntityId = eveEntityId;
            EveEntityName = eveEntityName;
            EveEntity = entityType;
        }
    }
}

