using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

public class WHAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public virtual ICollection<WHAccount> AdditionnalAccounts { get; } = new HashSet<WHAccount>();


    [Obsolete("EF Requires it")]
    protected WHAccount() { }

    public WHAccount(int characterId)
    {
        CharacterId = characterId;
    }
}
