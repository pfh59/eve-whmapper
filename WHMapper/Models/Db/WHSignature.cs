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

        [Required]
        public WHSignatureGroup Group { get; set; } = WHSignatureGroup.Unknow;

        public string? Type { get;  set; }

        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get;  set; }
        [Required]
        public DateTime Updated { get;  set; } = DateTime.UtcNow;
        public string? UpdatedBy { get;  set; }


        public WHSignature(string name)
        {
            Name = name;
        }
    }
}

