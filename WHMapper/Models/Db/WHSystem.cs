
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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

        public double PosX { get; set; } = 0.0;

        public double PosY { get; set; } = 0.0;


        public WHSystem(string name,float securityStatus)
        {
            Name = name;
            SecurityStatus = securityStatus;
        }
    }
}

