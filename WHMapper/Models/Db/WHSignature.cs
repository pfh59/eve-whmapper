using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{

    public enum WHSignatureGroup
    {
        Unknow,
        Combat,
        Wormhole,
        Data,
        Relic,
        Ore,
        Gas,
        Ghost

    }

    public class WHSignature
    {
        [Key]
        public int Id { get;  set; }

        [Required]
        [StringLength(7, ErrorMessage = "Bad Signature Format")]
        public string Name { get; set; }


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
        public string? CreatedBy { get; set; } = "Unknown";
        [Required]
        public DateTime Updated { get;  set; } = DateTime.UtcNow;
        [Required]
        public string? UpdatedBy { get; set; } = "Unknown";

        public WHSignature()
        {

        }

        public WHSignature(string name)
        {
            Name = name;
        }

        public WHSignature(string name,string scanUser)
        {
            Name = name;
            CreatedBy = scanUser;
            UpdatedBy = scanUser;
        }

        public WHSignature(string name, WHSignatureGroup group, string? type)
        {
            Name = name;
            Group = group;
            Type = type;
        }


    }

}

