using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{
    /// <summary>
    /// Represents an access entry for a WHMapper instance.
    /// Defines who (character, corporation, or alliance) can access the instance.
    /// </summary>
    public class WHInstanceAccess
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The instance this access belongs to
        /// </summary>
        [Required]
        public int WHInstanceId { get; set; }

        /// <summary>
        /// The EVE entity ID (character, corporation, or alliance ID)
        /// </summary>
        [Required]
        public int EveEntityId { get; set; }

        /// <summary>
        /// The name of the EVE entity
        /// </summary>
        [Required]
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

        [Obsolete("EF Requires it")]
        protected WHInstanceAccess() { }

        public WHInstanceAccess(int whInstanceId, int eveEntityId, string eveEntityName, WHAccessEntity eveEntity)
        {
            WHInstanceId = whInstanceId;
            EveEntityId = eveEntityId;
            EveEntityName = eveEntityName;
            EveEntity = eveEntity;
        }
    }
}
