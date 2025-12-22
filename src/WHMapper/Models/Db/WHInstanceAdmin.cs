using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    /// <summary>
    /// Represents an administrator of a WHMapper instance.
    /// Instance admins can manage maps, access controls, and add other admins for their instance.
    /// </summary>
    public class WHInstanceAdmin
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The instance this admin belongs to
        /// </summary>
        [Required]
        public int WHInstanceId { get; set; }

        /// <summary>
        /// The EVE character ID of the admin
        /// </summary>
        [Required]
        public int EveCharacterId { get; set; }

        /// <summary>
        /// The EVE character name of the admin
        /// </summary>
        [Required]
        public string EveCharacterName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this admin is the primary/owner admin (created the instance)
        /// </summary>
        public bool IsOwner { get; set; } = false;

        /// <summary>
        /// Date when the admin was added
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [Obsolete("EF Requires it")]
        protected WHInstanceAdmin() { }

        public WHInstanceAdmin(int whInstanceId, int eveCharacterId, string eveCharacterName, bool isOwner = false)
        {
            WHInstanceId = whInstanceId;
            EveCharacterId = eveCharacterId;
            EveCharacterName = eveCharacterName;
            IsOwner = isOwner;
        }
    }
}
