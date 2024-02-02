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
        public String Comment { get; set; } = string.Empty;

        public WHSystemStatusEnum SystemStatus { get; set; } = WHSystemStatusEnum.Unknown;

        public WHNote()
		{
            
        }

        public WHNote(int soloarSystemId,string comment) 
        : this(soloarSystemId,WHSystemStatusEnum.Unknown,comment)
        {

        }

        public WHNote(int soloarSystemId, WHSystemStatusEnum systemStatus)
            :this(soloarSystemId,systemStatus,string.Empty)

        {

        }

        public WHNote(int soloarSystemId, WHSystemStatusEnum systemStatus,string comment)
        {
            SoloarSystemId = soloarSystemId;
            SystemStatus = systemStatus;
            Comment = comment;
        }
    }
}

