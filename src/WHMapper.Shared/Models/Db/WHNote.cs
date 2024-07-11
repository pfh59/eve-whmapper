using System.ComponentModel.DataAnnotations;
using WHMapper.Shared.Models.Db.Enums;

namespace WHMapper.Shared.Models.Db
{
    public class WHNote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SoloarSystemId { get; set; } = -1;

        [Required, StringLength(255, ErrorMessage = "Comment is too long.")]
        public string Comment { get; set; } = string.Empty;

        public WHSystemStatus SystemStatus { get; set; } = WHSystemStatus.Unknown;

        [Obsolete("EF Requires it")]
        protected WHNote() { }

        public WHNote(int soloarSystemId, string comment)
        : this(soloarSystemId, WHSystemStatus.Unknown, comment)
        {

        }

        public WHNote(int soloarSystemId, WHSystemStatus systemStatus)
            : this(soloarSystemId, systemStatus, string.Empty)

        {

        }

        public WHNote(int soloarSystemId, WHSystemStatus systemStatus, string comment)
        {
            SoloarSystemId = soloarSystemId;
            SystemStatus = systemStatus;
            Comment = comment;
        }
    }
}

