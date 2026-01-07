namespace WHMapper.Services.EveAPI
{
    public static class EveAPIServiceConstants
    {
        public const string ESIUrl = "https://esi.evetech.net";
        
        /// <summary>
        /// The X-Compatibility-Date header for ESI API versioning.
        /// This replaces the old per-route versioning (v1, v2, etc.) with a single date-based version.
        /// See: https://developers.eveonline.com/blog/changing-versions-v42-was-getting-out-of-hand
        /// </summary>
        public const string CompatibilityDateHeader = "X-Compatibility-Date";
        
        /// <summary>
        /// The compatibility date value to use for ESI API requests.
        /// Format: YYYY-MM-DD. ESI will return API behavior as it was at this date.
        /// </summary>
        public const string CompatibilityDateValue = "2025-12-16";
    }
}