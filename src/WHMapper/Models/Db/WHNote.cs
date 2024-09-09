using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{
    public class WHNote
	{
        [Key]
        public int Id { get; set; }

        [Required]
        public int MapId { get; set; } = -1;

        [Required]
        public int SoloarSystemId { get; set; } = -1;

        [Required, StringLength(255, ErrorMessage = "Comment is too long.")]
        public String Comment { get; set; } = string.Empty;

        public WHSystemStatus SystemStatus { get; set; } = WHSystemStatus.Unknown;

        [Obsolete("EF Requires it")]
        protected WHNote() { }

        public WHNote(int mapId,int soloarSystemId,string comment) 
        : this(mapId,soloarSystemId,WHSystemStatus.Unknown,comment)
        {

        }

        public WHNote(int mapId,int soloarSystemId, WHSystemStatus systemStatus)
            :this(mapId,soloarSystemId,systemStatus,string.Empty)

        {

        }

        public WHNote(int mapId,int soloarSystemId, WHSystemStatus systemStatus,string comment)
        {
            MapId = mapId;
            SoloarSystemId = soloarSystemId;
            SystemStatus = systemStatus;
            Comment = comment;
        }
    }
}

