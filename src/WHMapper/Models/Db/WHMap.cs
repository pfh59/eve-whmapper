using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    public class WHMap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Map name is too long.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The instance this map belongs to. Null for legacy maps (backward compatibility).
        /// </summary>
        public int? WHInstanceId { get; set; }

        public virtual ICollection<WHSystem> WHSystems { get; } = new HashSet<WHSystem>();
        public virtual ICollection<WHSystemLink> WHSystemLinks { get; } = new HashSet<WHSystemLink>();
        public virtual ICollection<WHMapAccess> WHMapAccesses { get; } = new HashSet<WHMapAccess>();

        [Obsolete("EF Requires it")]
        protected WHMap() { }

        public WHMap(string name)
        {
            Name = name;
        }

        public WHMap(string name, int whInstanceId)
        {
            Name = name;
            WHInstanceId = whInstanceId;
        }
    }
}

