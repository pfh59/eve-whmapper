using System.ComponentModel.DataAnnotations;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Db
{

    public class WHSignature
    {
        private const string DEFAULT_SCAN_USER_VALUE = "Unknown";

        [Key]
        public int Id { get;  set; }

        [Required]
        public int WHId { get; set; }


        [Required]
        [StringLength(7, ErrorMessage = "Bad Signature Format")]
        public string Name { get; set; } = string.Empty;


        private WHSignatureGroup _sigGroup = WHSignatureGroup.Unknow;

        [Required]
        public WHSignatureGroup Group
        {
            get
            {
                return _sigGroup;
            }

            set
            {
                if (_sigGroup != value)
                    Type = String.Empty;

                _sigGroup = value;
            }
        } 

        public string? Type { get;  set; }

        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;
        [Required]
        public string? CreatedBy { get; set; } = DEFAULT_SCAN_USER_VALUE;
        [Required]
        public DateTime Updated { get;  set; } = DateTime.UtcNow;
        [Required]
        public string? UpdatedBy { get; set; } = DEFAULT_SCAN_USER_VALUE;

        [Obsolete("EF Requires it")]
        protected WHSignature() { }

        public WHSignature(int whId,string name) :
            this(whId,name, DEFAULT_SCAN_USER_VALUE)
        {

        }

        public WHSignature(int whId, string name,string scanUser)
            : this(whId,name,WHSignatureGroup.Unknow,null,scanUser)
        {

        }

        public WHSignature(int whId, string name, WHSignatureGroup group, string? type,string scanUser)
        {
            WHId = whId;
            Name = name;
            Group = group;
            Type = type;
            CreatedBy = scanUser;
            UpdatedBy = scanUser;
        }

        public WHSignature(int whId, string name, WHSignatureGroup group, string? type) :
            this(whId,name, group,type, DEFAULT_SCAN_USER_VALUE)
        {

        }


    }

}

