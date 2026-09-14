using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.WHSignature
{
    public interface IWHSignatureHelper
    {
        const string SCAN_VALIDATION_REGEX = @"[A-Z]{3}-\d{3}\s+[\S\s]+?\s+\d*(,|.)\d+\s*\S*%(\s+\d{1,3}(?:['\s]\d{3})*(?:(,|.)\d{1,2})?\s(?:UA|AU|km|m|а\.е\.|AE|км|м))*";
        Task<bool> ValidateScanResult(string? scanResult);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> ParseScanResult(string scanUser, int currentSystemScannedId, string? scanResult);
        /// <summary>
        /// Imports a scan result into a system: creates the new signatures and refreshes the existing ones.
        /// </summary>
        /// <returns>
        /// Whether signatures were written, the number of signatures created, and the number of existing
        /// signatures whose name, group or type changed; both counts only include scan lines at 100% signal strength.
        /// </returns>
        Task<WHSignatureImportResult> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> GetCurrentSystemSignatures(int whId);
        Task<IEnumerable<WHAnalizedSignature>?> AnalyzedSignatures(IEnumerable<WHMapper.Models.Db.WHSignature>? parsedSigs,IEnumerable<WHMapper.Models.Db.WHSignature>? currentSystemSigs , bool lazyDeleted);
    }
}
