using System;
using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

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

        public WHSystemStatusEnum SystemStatus { get; set; } = WHSystemStatusEnum.Unknown;

        public WHNote()
		{

        }

        public WHNote(int soloarSystemId,string comment)
        {
            SoloarSystemId = soloarSystemId;
            Comment = comment;
        }

        public WHNote(int soloarSystemId, WHSystemStatusEnum systemStatus)
        {
            SoloarSystemId = soloarSystemId;
            SystemStatus = systemStatus;
            Comment = String.Empty;
        }
    }
}

