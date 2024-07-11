using System.ComponentModel.DataAnnotations;
using WHMapper.Shared.Models.Db.Enums;

namespace WHMapper.Shared.Models.Db
{
    public class WHSystemLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WHMapId { get; set; }

        [Required]
        public int IdWHSystemFrom { get; set; }

        [Required]
        public int IdWHSystemTo { get; set; }

        [Required]
        public bool IsEndOfLifeConnection { get; set; } = false;

        [Required]
        public SystemLinkSize Size { get; set; } = SystemLinkSize.Large;

        [Required]
        public SystemLinkMassStatus MassStatus { get; set; } = SystemLinkMassStatus.Normal;

        [Required]
        public ICollection<WHJumpLog> JumpHistory { get; } = new HashSet<WHJumpLog>();


        [Obsolete("EF Requires it")]
        protected WHSystemLink() { }
        public WHSystemLink(int idWHSystemFrom, int idWHSystemTo) :
            this(0, idWHSystemFrom, idWHSystemTo)
        {
        }

        public WHSystemLink(int whMapId, int idWHSystemFrom, int idWHSystemTo)
        {
            WHMapId = whMapId;
            IdWHSystemFrom = idWHSystemFrom;
            IdWHSystemTo = idWHSystemTo;
        }
    }
}

