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
        public virtual ICollection<WHSystem> WHSystems { get; } = new HashSet<WHSystem>();
        public virtual ICollection<WHSystemLink> WHSystemLinks { get; } = new HashSet<WHSystemLink>();
        public virtual ICollection<WHAccess> WHAccesses { get; } = new HashSet<WHAccess>();

        [Obsolete("EF Requires it")]
        protected WHMap() { }

        public WHMap(string name)
        {
            Name = name;
        }
    }
}

