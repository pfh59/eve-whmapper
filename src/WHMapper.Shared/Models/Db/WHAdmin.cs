using System.ComponentModel.DataAnnotations;

namespace WHMapper.Shared.Models.Db
{
    public class WHAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EveCharacterId { get; set; }

        [Required]
        public string EveCharacterName { get; set; } = string.Empty;

        [Obsolete("EF Requires it")]
        protected WHAdmin() { }
        public WHAdmin(int eveCharacterId, string eveCharacterName)
        {
            EveCharacterId = eveCharacterId;
            EveCharacterName = eveCharacterName;
        }
    }
}

