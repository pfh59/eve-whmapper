using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    public class WHMap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Map name is too long.")]
        public string Name { get; set; }


        public ICollection<WHSystem> WHSystems { get; } = new HashSet<WHSystem>();
        public ICollection<WHSystemLink> WHSystemLinks { get; } = new HashSet<WHSystemLink>();

        public WHMap(string name)
        {
            Name = name;
        }
    }
}

