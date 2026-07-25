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

        [Required]
        public int WHInstanceId { get; set; }

        [Required]
        public int EveCharacterId { get; set; }

        [Required]
        public string EveCharacterName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this admin is the primary/owner admin (created the instance)
        /// </summary>
        public bool IsOwner { get; set; } = false;

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
