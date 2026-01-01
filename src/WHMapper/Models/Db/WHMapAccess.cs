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

        /// <summary>
        /// The map this access belongs to
        /// </summary>
        [Required]
        public int WHMapId { get; set; }

        /// <summary>
        /// The EVE entity ID (character, corporation, or alliance ID)
        /// </summary>
        [Required]
        public int EveEntityId { get; set; }

        /// <summary>
        /// The name of the EVE entity
        /// </summary>
        [Required]
        [StringLength(255)]
        public string EveEntityName { get; set; } = string.Empty;

        /// <summary>
        /// The type of EVE entity
        /// </summary>
        [Required]
        public WHAccessEntity EveEntity { get; set; }

        /// <summary>
        /// Date when the access was granted
        /// </summary>
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to the map
        /// </summary>
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
