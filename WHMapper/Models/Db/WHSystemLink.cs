using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    public enum SystemLinkSize : int
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        XLarge = 3
    }

    public enum SystemLinkMassStatus : int
    {
        Normal = 0,
        Critical = 1,
        Verge = 2
    }

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

