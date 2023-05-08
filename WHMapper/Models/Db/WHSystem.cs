
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WHMapper.Models.Db
{
    public class WHSystem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SoloarSystemId { get; set; } = -1;

        [Required , StringLength(255, ErrorMessage = "Map name is too long.")]
        public String Name { get; set; }

        public byte NameExtension { get; set; }

        [Required]
        public float SecurityStatus { get;  set; }

        public ICollection<WHSignature> WHSignatures { get; } = new HashSet<WHSignature>();

        public double PosX { get; set; } = 0.0;

        public double PosY { get; set; } = 0.0;


        public WHSystem()
        { }

        public WHSystem(int solarSystemId,string name, float securityStatus,double posX, double posY)
        {
            SoloarSystemId = solarSystemId;
            Name = name;
            SecurityStatus = securityStatus;
            PosX = posX;
            PosY = posY;
        }

        public WHSystem(int solarSystemId, string name, char nameExtension, float securityStatus, double posX, double posY)
        {
            SoloarSystemId = solarSystemId;
            Name = name;
            NameExtension = Convert.ToByte(nameExtension);
            SecurityStatus = securityStatus;
            PosX = posX;
            PosY = posY;
        }

        
        public WHSystem(int solarSystemId, string name, float securityStatus) :
            this(solarSystemId, name, securityStatus, 0, 0)
        {
        }

        public WHSystem(int solarSystemId, string name, char nameExtension, float securityStatus) :
            this(solarSystemId,name, nameExtension,securityStatus,0,0)
        {

        }

    }
}

