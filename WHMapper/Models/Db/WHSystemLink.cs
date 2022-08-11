using System;
using System.ComponentModel.DataAnnotations;

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
        
     
        public WHSystemLink(int idWHSystemFrom, int idWHSystemTo)
        {
            IdWHSystemFrom = idWHSystemFrom;
            IdWHSystemTo = idWHSystemTo;
        }
    }
}

