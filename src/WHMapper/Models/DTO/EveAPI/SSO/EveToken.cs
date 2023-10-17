using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.SSO
{

    public class EveToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; private set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; private set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; private set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; private set; }


        [JsonConstructor]
        public EveToken(string accessToken, string tokenType, int expiresIn, string refreshToken) => (AccessToken, TokenType, ExpiresIn, RefreshToken) = (accessToken, tokenType, expiresIn, refreshToken);
    }
}

