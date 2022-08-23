
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    public class WHSystem
    {
        [Key]
        public int Id { get; set; }


        [Required]
        [StringLength(255, ErrorMessage = "Map name is too long.")]
        public String Name { get; set; }

        [Required]
        public float SecurityStatus { get;  set; }

        public ICollection<WHSignature> WHSignatures { get; } = new HashSet<WHSignature>();

        public WHSystem(string name,float securityStatus)
        {
            Name = name;
            SecurityStatus = securityStatus;
        }
    }
}

