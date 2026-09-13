using System.Globalization;
using System.Text.RegularExpressions;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Services.WHSignature;


namespace WHMapper.Services.WHSignatures
{
    public class WHSignatureHelper : IWHSignatureHelper
    {
        /// <summary>
        /// Signal strength, in percent, of a signature fully probed.
        /// </summary>
        private const double FULL_SIGNAL_STRENGTH = 100.0;

        /// <summary>
        /// Zero-based column of the signal strength in a probe scanner line, such as <c>100,0%</c>.
        /// </summary>
        private const int SIGNAL_STRENGTH_COLUMN = 4;

        private readonly IWHSignatureRepository _dbWHSignatures;

        public WHSignatureHelper(IWHSignatureRepository sigRepo)
        {
            _dbWHSignatures = sigRepo;
        }

        public Task<bool> ValidateScanResult(string? scanResult)
        {
            try
            {
                if (!string.IsNullOrEmpty(scanResult))
                {
                    var matches = Regex.Matches(scanResult, IWHSignatureHelper.SCAN_VALIDATION_REGEX, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(2));
                    // Compter le nombre de lignes non vides dans scanResult
                    int nonEmptyLinesCount = scanResult.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
                    
                    return Task.FromResult<bool>(matches.Count == nonEmptyLinesCount);
                }
                return Task.FromResult<bool>(false);
            }
            catch (RegexMatchTimeoutException)
            {
                return Task.FromResult<bool>(false);
            }

        }

    public Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> ParseScanResult(string scanUser, int currentSystemScannedId, string? scanResult)
    {

        IList<WHMapper.Models.Db.WHSignature> sigResult = new List<WHMapper.Models.Db.WHSignature>();

        if (string.IsNullOrEmpty(scanResult))
        {
            return Task.FromResult<IEnumerable<WHMapper.Models.Db.WHSignature>?>(sigResult);
        }

        try
        {
            string[] sigValues = scanResult.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string sigValue in sigValues)
            {
                string[] splittedSig = sigValue.Split('\t');
                if (splittedSig.Length < 4) continue; // Assurez-vous qu'il y a suffisamment d'éléments pour éviter les erreurs d'index

                string sigName = splittedSig[0];
                WHSignatureGroup sigGroup = WHSignatureGroup.Unknow;
                string sigType = string.Empty;

                if (!string.IsNullOrWhiteSpace(splittedSig[2]))
                {
                    string textGroup = splittedSig[2].Contains(' ') ? splittedSig[2].Split(' ').First() : splittedSig[2];
                    Enum.TryParse(textGroup, out sigGroup);
                    sigType = splittedSig[3];
                }

                sigResult.Add(new WHMapper.Models.Db.WHSignature(currentSystemScannedId, sigName, sigGroup, sigType, scanUser));
            }
        }
        catch (Exception ex) when (ex is RegexMatchTimeoutException || ex is ArgumentException)
        {
            // Log l'exception si nécessaire
            return Task.FromResult<IEnumerable<WHMapper.Models.Db.WHSignature>?>(null);
        }

        return Task.FromResult<IEnumerable<WHMapper.Models.Db.WHSignature>?>(sigResult);
    }

        public Task<IEnumerable<WHAnalizedSignature>?> AnalyzedSignatures(IEnumerable<WHMapper.Models.Db.WHSignature>? parsedSigs,IEnumerable<WHMapper.Models.Db.WHSignature>? currentSystemSigs , bool lazyDeleted)
        {
            IEnumerable<WHAnalizedSignature>? toAdd = new List<WHAnalizedSignature>();
            IEnumerable<WHAnalizedSignature>? toUpdate = new List<WHAnalizedSignature>();
            IEnumerable<WHAnalizedSignature>? toDelete = new List<WHAnalizedSignature>();

            if (parsedSigs == null || !parsedSigs.Any()) return Task.FromResult<IEnumerable<WHAnalizedSignature>?>(null);

            if (currentSystemSigs == null || !currentSystemSigs.Any())
            {
                toAdd = parsedSigs.Select(x => new WHAnalizedSignature(x, WHAnalizedSignatureEnums.toAdd)).ToList();
                return Task.FromResult<IEnumerable<WHAnalizedSignature>?>(toAdd);
            }

            var sigsToAdd = parsedSigs.ExceptBy(currentSystemSigs.Select(x => x.Name), y => y.Name);
            if (sigsToAdd.Any()) 
                toAdd=sigsToAdd.Select(x=>new WHAnalizedSignature(x,WHAnalizedSignatureEnums.toAdd)).ToList();

            var sigsToUpdate = currentSystemSigs.IntersectBy(parsedSigs.Where(x=> x.Group==WHSignatureGroup.Unknow).Select(x => x.Name), y => y.Name);
            var sigsToUpdate2 = parsedSigs.Where(x=> x.Group!=WHSignatureGroup.Unknow).IntersectBy(currentSystemSigs.Select(x => x.Name), y => y.Name);
            
            sigsToUpdate = sigsToUpdate.Concat(sigsToUpdate2);

            if (sigsToUpdate.Any())
                toUpdate = sigsToUpdate.Select(x=>new WHAnalizedSignature(x,WHAnalizedSignatureEnums.toUpdate)).ToList();

            if (lazyDeleted)
            {
                var sigsToDeleted = currentSystemSigs.ExceptBy(parsedSigs.Select(x => x.Name), y => y.Name);
                if (sigsToDeleted.Any())
                    toDelete = sigsToDeleted.Select(x=>new WHAnalizedSignature(x,WHAnalizedSignatureEnums.toDelete)).ToList();
            }
           

            return Task.FromResult<IEnumerable<WHAnalizedSignature>?>(toAdd.Concat(toUpdate).Concat(toDelete));
        }

        public async Task<IEnumerable<WHMapper.Models.Db.WHSignature>?> GetCurrentSystemSignatures(int whId)
        {
            var signatures = await _dbWHSignatures.GetByWHId(whId);
            return signatures;
        }

        public async Task<WHSignatureImportResult> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted)
        {
            if (!await ValidateScanResult(scanResult))
                throw new Exception("Bad signatures format");

            var sigs = await ParseScanResult(scanUser, currentSystemScannedId, scanResult);

            if (sigs == null || !sigs.Any())
                throw new Exception("Bad signature parsing parameters");

            if (currentSystemScannedId <= 0)
                throw new Exception("Current System is nullable");

            var currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
            if (currentSystemSigs == null) return WHSignatureImportResult.None;


            bool sigUpdated = false, sigAdded = false;

            if (lazyDeleted)
            {
                await DeleteSignatures(currentSystemSigs, sigs);
                currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
                if (currentSystemSigs == null) return WHSignatureImportResult.None;
            }

            var fullyScannedNames = GetFullyScannedSignatureNames(scanResult);
            (sigUpdated, int changedCount) = await UpdateSignatures(currentSystemSigs, sigs, fullyScannedNames);
            (sigAdded, int createdCount) = await AddNewSignatures(currentSystemSigs, sigs, currentSystemScannedId, fullyScannedNames);

            return new WHSignatureImportResult(sigUpdated || sigAdded, createdCount, changedCount);
        }

        /// <summary>
        /// Returns the names of the signatures whose scan line is at <see cref="FULL_SIGNAL_STRENGTH"/>.
        /// </summary>
        /// <remarks>
        /// Lines are split like <see cref="ParseScanResult"/>. A line without a readable signal strength is ignored.
        /// </remarks>
        /// <returns>The names; empty when the scan result is empty.</returns>
        private static HashSet<string> GetFullyScannedSignatureNames(string? scanResult)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(scanResult))
                return names;

            foreach (string line in scanResult.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] columns = line.Split('\t');
                if (columns.Length > SIGNAL_STRENGTH_COLUMN
                    && TryParseSignalStrength(columns[SIGNAL_STRENGTH_COLUMN], out double strength)
                    && strength >= FULL_SIGNAL_STRENGTH)
                {
                    names.Add(columns[0]);
                }
            }

            return names;
        }

        /// <summary>
        /// Parses a signal strength such as <c>100,0%</c> or <c>100.0 %</c>, whatever the client decimal separator.
        /// </summary>
        /// <returns>False when the value is not a number.</returns>
        private static bool TryParseSignalStrength(string value, out double strength)
        {
            string normalized = new string(value.Where(c => !char.IsWhiteSpace(c) && c != '%').ToArray()).Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out strength);
        }

        /// <summary>
        /// Refreshes every existing signature found in the scan and counts those whose content changed from a
        /// fully probed scan line.
        /// </summary>
        /// <param name="fullyScannedNames">Names of the signatures at 100% signal strength in the scan.</param>
        /// <returns>
        /// Whether every matched signature was saved, and the number of fully probed signatures whose name, group
        /// or type changed; the count is 0 when the save failed.
        /// </returns>
        private async Task<(bool Saved, int ChangedCount)> UpdateSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs, IReadOnlySet<string> fullyScannedNames)
        {
            var sigsToUpdate = currentSystemSigs.IntersectBy(sigs.Select(x => x.Name), y => y.Name).ToList();
            if (!sigsToUpdate.Any()) return (false, 0);

            int changedCount = 0;
            foreach (var sig in sigsToUpdate)
            {
                var sigParse = sigs.FirstOrDefault(x => x.Name == sig.Name);
                if (sigParse != null)
                {
                    var contentBeforeImport = new Models.Db.WHSignature(sig.WHId, sig.Name, sig.Group, sig.Type);

                    sig.Updated = sigParse.Updated;
                    sig.UpdatedBy = sigParse.UpdatedBy;
                    if(sigParse.Group != WHSignatureGroup.Unknow)
                    {
                        sig.Group = sigParse.Group;
                        sig.Type = String.IsNullOrEmpty(sig.Type) ? sigParse.Type : sig.Type;
                    }

                    // Refreshing Updated and UpdatedBy alone is not a change of content.
                    if (!sig.HasSameContent(contentBeforeImport) && fullyScannedNames.Contains(sig.Name))
                        changedCount++;
                }
            }

            var resUpdate = await _dbWHSignatures.Update(sigsToUpdate);
            bool saved = resUpdate != null && resUpdate.Count() == sigsToUpdate.Count;
            return (saved, saved ? changedCount : 0);
        }

        /// <summary>
        /// Creates the signatures of the scan that do not exist in the system yet.
        /// </summary>
        /// <param name="fullyScannedNames">Names of the signatures at 100% signal strength in the scan.</param>
        /// <returns>
        /// Whether every new signature was saved, and the number of fully probed signatures created; the count is 0
        /// when the save failed.
        /// </returns>
        private async Task<(bool Saved, int CreatedCount)> AddNewSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs, int currentSystemScannedId, IReadOnlySet<string> fullyScannedNames)
        {
            var sigsToAdd = sigs.ExceptBy(currentSystemSigs.Select(x => x.Name), y => y.Name).ToList();
            if (!sigsToAdd.Any()) return (false, 0);

            var resAdd = await _dbWHSignatures.Create(sigsToAdd);
            bool saved = resAdd != null && resAdd.Count() == sigsToAdd.Count;
            return (saved, saved ? sigsToAdd.Count(x => fullyScannedNames.Contains(x.Name)) : 0);
        }

        private async Task DeleteSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs)
        {
            var sigsToDeleted = currentSystemSigs.ExceptBy(sigs.Select(x => x.Name), y => y.Name);
            foreach (var sig in sigsToDeleted)
            {
                await _dbWHSignatures.DeleteById(sig.Id);
            }
        }


    }
}
