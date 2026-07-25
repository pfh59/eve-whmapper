using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{
    /// <summary>
    /// Represents an access entry for a specific map within an instance.
    /// When no access entries exist for a map, all instance members can access it.
    /// When at least one access entry exists, only explicitly granted entities can access the map.
    /// </summary>
    public class WHMapAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WHMapId { get; set; }

        /// <summary>
        /// The EVE entity ID (character, corporation, or alliance ID)
        /// </summary>
        [Required]
        public int EveEntityId { get; set; }

        [Required]
        [StringLength(255)]
        public string EveEntityName { get; set; } = string.Empty;

        [Required]
        public WHAccessEntity EveEntity { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public virtual WHMap? WHMap { get; set; }

        [Obsolete("EF Requires it")]
        protected WHMapAccess() { }

        public WHMapAccess(int whMapId, int eveEntityId, string eveEntityName, WHAccessEntity eveEntity)
        {
            WHMapId = whMapId;
            EveEntityId = eveEntityId;
            EveEntityName = eveEntityName;
            EveEntity = eveEntity;
        }
    }
}
