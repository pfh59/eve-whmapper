using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

public class WHMainAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public virtual ICollection<WHAdditionnalAccount> AdditionnalAccounts { get; } = new HashSet<WHAdditionnalAccount>();


    [Obsolete("EF Requires it")]
    protected WHMainAccount() { }

    public WHMainAccount(int characterId)
    {
        CharacterId = characterId;
    }
}
