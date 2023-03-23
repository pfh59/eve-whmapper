
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

        public byte NameExtension { get; set; }

        [Required]
        public float SecurityStatus { get;  set; }

        public ICollection<WHSignature> WHSignatures { get; } = new HashSet<WHSignature>();

        public double PosX { get; set; } = 0.0;

        public double PosY { get; set; } = 0.0;


        public WHSystem(string name,float securityStatus) :
            this(name,securityStatus,0,0)
        {
        }

        public WHSystem(string name, float securityStatus,double posX, double posY)
        {
            Name = name;
            SecurityStatus = securityStatus;
            PosX = posX;
            PosY = posY;
        }

        public WHSystem(string name, char nameExtension, float securityStatus) :
            this(name,nameExtension,securityStatus,0,0)
        {

        }

        public WHSystem(string name, char nameExtension, float securityStatus,double posX,double posY)
        {
            Name = name;
            NameExtension = Convert.ToByte(nameExtension);
            SecurityStatus = securityStatus;
            PosX = posX;
            PosY = posY;
        }
    }
}

