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
                var checksum = _httpClient.GetStringAsync("checksum").Result;
                _logger.LogInformation($"Retrieved checksum: {checksum}");
                return checksum;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Couldn't retrieve checksum: {ex}");
            }
            return string.Empty;
        }

        public Task<Stream> GetSDEDataStreamAsync()
        {
            return _httpClient.GetStreamAsync("sde.zip");
        }
    }
}
