using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.WHSignature
{
    public interface IWHSignatureHelper
    {
        const string SCAN_VALIDATION_REGEX = @"[A-Z]{3}-\d{3}\s+[\S\s]+?\s+\d*,\d+\s*\S*%\s+\d{1,3}(?:['\s]\d{3})*(?:,\d{1,2})?\s(?:UA|AU|km|m|а\.е\.|AE|км|м)";
        Task<bool> ValidateScanResult(string? scanResult);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> ParseScanResult(string scanUser, int currentSystemScannedId, string? scanResult);
        Task<bool> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> GetCurrentSystemSignatures(int whId);
        Task<IEnumerable<WHAnalizedSignature>?> AnalyzedSignatures(IEnumerable<WHMapper.Models.Db.WHSignature>? parsedSigs,IEnumerable<WHMapper.Models.Db.WHSignature>? currentSystemSigs , bool lazyDeleted);
    }
}
