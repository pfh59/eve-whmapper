namespace WHMapper.Services.SDE
{
    public class SdeDataSupplier : ISDEDataSupplier
    {
        private readonly ILogger<SdeDataSupplier> _logger;
        private readonly HttpClient _httpClient;

        public SdeDataSupplier(ILogger<SdeDataSupplier> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public string GetChecksum()
        {
            try
            {
                var checksum = _httpClient.GetStringAsync("/checksum");
                _logger.LogInformation($"Retrieved checksum: {checksum}");
            } catch(Exception ex)
            {
                _logger.LogWarning($"Couldn't retrieve checksum: {ex}");
            }
            return string.Empty;
        }
    }
}
