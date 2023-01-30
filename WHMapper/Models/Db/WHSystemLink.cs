using System;
using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{


    public class WHSystemLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdWHSystemFrom { get; set; }

        [Required]
        public int IdWHSystemTo  { get; set; }

        [Required]
        public bool IsEndOfLifeConnection { get; set; } = false;

        [Required]
        public SystemLinkSize Size { get; set; } = SystemLinkSize.Large;

        [Required]
        public SystemLinkMassStatus MassStatus { get; set; } = SystemLinkMassStatus.Normal;


        public WHSystemLink(int idWHSystemFrom, int idWHSystemTo)
        {
            IdWHSystemFrom = idWHSystemFrom;
            IdWHSystemTo = idWHSystemTo;
        }
    }
}

