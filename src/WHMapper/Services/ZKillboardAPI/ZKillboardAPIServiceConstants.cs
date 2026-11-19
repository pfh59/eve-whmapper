namespace WHMapper.Services.ZKillboardAPI;

public static class ZKillboardAPIServiceConstants
{
    public const string ZKillboardUrl = "https://zkillboard.com";

    /// <summary>
    /// zKillboard requires a descriptive User-Agent that identifies the application.
    /// </summary>
    public const string USER_AGENT = "WHMapper (+https://github.com/pfh59/eve-whmapper)";

    public const string SYSTEM_KILLMAILS_PATH = "api/solarSystemID/{0}/";
    public const string SYSTEM_STATS_PATH = "api/stats/solarSystemID/{0}/kills/";

    /// <summary>
    /// Public page of a killmail on zKillboard.
    /// </summary>
    public const string KILLMAIL_URL = "https://zkillboard.com/kill/{0}/";
}
