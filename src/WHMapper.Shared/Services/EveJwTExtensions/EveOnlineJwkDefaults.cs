namespace WHMapper.Shared.Services.EveJwTExtensions
{
    public class EveOnlineJwkDefaults
    {
        public const string AuthenticationScheme = "EveOnlineBearer";
        public static readonly string EVE_HOST = "login.eveonline.com";
        public static readonly string SSOUrl = "https://login.eveonline.com";
        public static readonly string JWKEndpoint = "https://login.eveonline.com/oauth/jwks";
        public static readonly string ValideIssuer = "https://login.eveonline.com";
        public static readonly string ValideAudience = "EVE Online";
    }
}
