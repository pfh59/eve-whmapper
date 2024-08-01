using System;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Models.DTO
{
    public class TokenProvider
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}

