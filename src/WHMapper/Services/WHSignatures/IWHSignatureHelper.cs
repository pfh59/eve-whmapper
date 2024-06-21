namespace WHMapper.Services.WHSignature
{
    public interface IWHSignatureHelper
    {
        const string SCAN_VALIDATION_REGEX = "[A-Z]{3}-[0-9]{3}\\s([a-zA-Z\\s]+)([0-9]*[,.][0-9]\\s*%)\\s([0-9]*.[0-9]+\\s(UA|AU|km|m))";

        Task<bool> ValidateScanResult(string? scanResult);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> ParseScanResult(string scanUser, int currentSystemScannedId, string? scanResult);
        Task<bool> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted);
    }
}

