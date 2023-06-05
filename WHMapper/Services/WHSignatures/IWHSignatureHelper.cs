using System;
using WHMapper.Models.Custom;

namespace WHMapper.Services.WHSignature
{
	public interface IWHSignatureHelper
	{
        Task<bool> ValidateScanResult(string? scanResult);
        Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> ParseScanResult(string scanUser,int currentSystemScannedId, string? scanResult);
        Task<bool> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult,bool lazyDeleted);
    }
}

