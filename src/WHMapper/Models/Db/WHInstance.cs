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

        [Required]
        [StringLength(255, ErrorMessage = "Instance name is too long.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        /// <summary>
        /// The EVE entity ID that owns this instance (character, corporation, or alliance ID)
        /// </summary>
        [Required]
        public int OwnerEveEntityId { get; set; }

        [Required]
        public string OwnerEveEntityName { get; set; } = string.Empty;

        [Required]
        public WHAccessEntity OwnerType { get; set; }

        [Required]
        public int CreatorCharacterId { get; set; }

        [Required]
        public string CreatorCharacterName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<WHMap> WHMaps { get; } = new HashSet<WHMap>();

        public virtual ICollection<WHInstanceAdmin> Administrators { get; } = new HashSet<WHInstanceAdmin>();

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

        public WHInstance(string name, int ownerEveEntityId, string ownerEveEntityName, WHAccessEntity ownerType,
            int creatorCharacterId, string creatorCharacterName, string? description)
            : this(name, ownerEveEntityId, ownerEveEntityName, ownerType, creatorCharacterId, creatorCharacterName)
        {
            Description = description;
        }
    }
}
