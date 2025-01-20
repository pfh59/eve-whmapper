using System;
using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

public class WHAdditionnalAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    public virtual WHMainAccount MainAccount { get; set; } = null!;

    [Obsolete("EF Requires it")]
    protected WHAdditionnalAccount() { }

    public WHAdditionnalAccount(int characterId)
    {
        CharacterId = characterId;
    }

}
