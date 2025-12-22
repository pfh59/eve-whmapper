using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{
    /// <summary>
    /// Represents a WHMapper instance (tenant) that can be owned by a character, corporation, or alliance.
    /// Each instance has its own maps, access controls, and administrators.
    /// </summary>
    public class WHInstance
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Display name for the instance
        /// </summary>
        [Required]
        [StringLength(255, ErrorMessage = "Instance name is too long.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the instance
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        /// <summary>
        /// The EVE entity ID that owns this instance (character, corporation, or alliance ID)
        /// </summary>
        [Required]
        public int OwnerEveEntityId { get; set; }

        /// <summary>
        /// The name of the owner entity
        /// </summary>
        [Required]
        public string OwnerEveEntityName { get; set; } = string.Empty;

        /// <summary>
        /// The type of entity that owns this instance
        /// </summary>
        [Required]
        public WHAccessEntity OwnerType { get; set; }

        /// <summary>
        /// The character ID of the user who created the instance
        /// </summary>
        [Required]
        public int CreatorCharacterId { get; set; }

        /// <summary>
        /// The character name of the user who created the instance
        /// </summary>
        [Required]
        public string CreatorCharacterName { get; set; } = string.Empty;

        /// <summary>
        /// Date when the instance was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the instance is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Maps belonging to this instance
        /// </summary>
        public virtual ICollection<WHMap> WHMaps { get; } = new HashSet<WHMap>();

        /// <summary>
        /// Administrators of this instance
        /// </summary>
        public virtual ICollection<WHInstanceAdmin> Administrators { get; } = new HashSet<WHInstanceAdmin>();

        /// <summary>
        /// Access entries for this instance (who can access the instance)
        /// </summary>
        public virtual ICollection<WHInstanceAccess> InstanceAccesses { get; } = new HashSet<WHInstanceAccess>();

        [Obsolete("EF Requires it")]
        protected WHInstance() { }

        public WHInstance(string name, int ownerEveEntityId, string ownerEveEntityName, WHAccessEntity ownerType, 
            int creatorCharacterId, string creatorCharacterName)
        {
            Name = name;
            OwnerEveEntityId = ownerEveEntityId;
            OwnerEveEntityName = ownerEveEntityName;
            OwnerType = ownerType;
            CreatorCharacterId = creatorCharacterId;
            CreatorCharacterName = creatorCharacterName;
        }
    }
}
