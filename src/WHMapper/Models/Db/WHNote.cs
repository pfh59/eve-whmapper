using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
	public class WHNote
	{
        [Key]
        public int Id { get; set; }

        [Required]
        public int SoloarSystemId { get; set; } = -1;

        [Required, StringLength(255, ErrorMessage = "Comment is too long.")]
        public String Comment { get; set; }

        public WHNote()
		{

        }

        public WHNote(int soloarSystemId,string comment)
        {
            SoloarSystemId = soloarSystemId;
            Comment = comment;
        }
    }
}

