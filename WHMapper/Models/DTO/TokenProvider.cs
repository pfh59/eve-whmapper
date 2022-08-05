using System;
namespace WHMapper.Models.DTO
{
    public class TokenProvider
    {
        public string AccessToken { get; set; } = String.Empty;
        public string RefreshToken { get; set; } = String.Empty;
        public string CharacterId { get; set; } = String.Empty;


    }
}

