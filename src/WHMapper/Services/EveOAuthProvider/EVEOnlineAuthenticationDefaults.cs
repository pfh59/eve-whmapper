namespace WHMapper.Services.EveOAuthProvider
{
    public static class EVEOnlineAuthenticationDefaults
    {
        /// <summary>
        /// Default value for <see cref="AuthenticationScheme.Name"/>.
        /// </summary>
        public const string AuthenticationScheme = "EVEOnline";

        /// <summary>
        /// Default value for <see cref="AuthenticationScheme.DisplayName"/>.
        /// </summary>
        public static readonly string DisplayName = "EVEOnline";

        /// <summary>
        /// Default value for <see cref="AuthenticationSchemeOptions.ClaimsIssuer"/>.
        /// </summary>
        public static readonly string Issuer = "EVEOnline";

        /// <summary>
        /// Default value for <see cref="RemoteAuthenticationOptions.CallbackPath"/>.
        /// </summary>
        public static readonly string CallbackPath = "/signin-eveonline";

        /// <summary>
        /// Default value for <see cref="OAuthOptions.AuthorizationEndpoint"/>.
        /// </summary>
        public static readonly string AuthorizationEndpoint = "https://login.eveonline.com/v2/oauth/authorize";

        /// <summary>
        /// Default value for <see cref="OAuthOptions.TokenEndpoint"/>.
        /// </summary>
        public static readonly string TokenEndpoint = "https://login.eveonline.com/v2/oauth/token";
        public static readonly string JWKEndpoint = "https://login.eveonline.com/oauth/jwks";
        public static readonly string RevokeTokenEndpoint = "https://login.eveonline.com/v2/oauth/revoke";

        public const string Scopes = "urn:eveonline:scopes";

        public static readonly string ValideIssuer = "https://login.eveonline.com";
        public static readonly string ValideAudience = "EVE Online";
    }
}
