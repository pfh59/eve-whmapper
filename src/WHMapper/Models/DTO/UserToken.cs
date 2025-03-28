using System;

namespace WHMapper.Models.DTO;

public class UserToken
{   public string? AccountId { get; set; } // Unique user identifier
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime Expiry { get; set; }
}
