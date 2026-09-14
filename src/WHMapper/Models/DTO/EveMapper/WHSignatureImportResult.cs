namespace WHMapper.Models.DTO.EveMapper;

/// <summary>
/// Outcome of a scan result import into a system.
/// </summary>
/// <param name="Persisted">
/// True when the import wrote to the database: every existing signature found in the scan was refreshed,
/// or every new signature was created.
/// </param>
/// <param name="FullyScannedCreatedCount">Number of signatures created from a scan line at 100% signal strength.</param>
/// <param name="FullyScannedChangedCount">
/// Number of existing signatures whose name, group or type changed, from a scan line at 100% signal strength.
/// </param>
public record WHSignatureImportResult(bool Persisted, int FullyScannedCreatedCount, int FullyScannedChangedCount)
{
    /// <summary>
    /// Import that wrote nothing to the database.
    /// </summary>
    public static WHSignatureImportResult None { get; } = new(false, 0, 0);
}
