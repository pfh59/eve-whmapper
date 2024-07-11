using System.Text.RegularExpressions;
using WHMapper.Shared.Models.Db;
using WHMapper.Shared.Models.Db.Enums;
using WHMapper.Shared.Repositories.WHSignatures;


namespace WHMapper.Shared.Services.WHSignatures
{
    public class WHSignatureHelper : IWHSignatureHelper
    {
        private IWHSignatureRepository _dbWHSignatures;

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
                    var match = Regex.Match(scanResult, IWHSignatureHelper.SCAN_VALIDATION_REGEX, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
                    return Task.FromResult(match.Success);
                }
                return Task.FromResult(false);
            }
            catch (RegexMatchTimeoutException)
            {
                return Task.FromResult(false);
            }

        }

        public Task<IEnumerable<WHSignature>?> ParseScanResult(string scanUser, int currentSystemScannedId, string? scanResult)
        {
            string sigName = string.Empty;
            var sigGroup = WHSignatureGroup.Unknow;
            string sigType = string.Empty;
            string[]? sigvalues = null;
            string[]? splittedSig = null;

            IList<WHSignature> sigResult = new List<WHSignature>();

            if (!string.IsNullOrEmpty(scanResult))
            {
                var lineRegex = new Regex("\n", RegexOptions.None, TimeSpan.FromSeconds(2));
                var tabRegex = new Regex("\t", RegexOptions.None, TimeSpan.FromSeconds(2));
                try
                {
                    sigvalues = lineRegex.Split(scanResult);
                }
                catch (RegexMatchTimeoutException)
                {
                    return Task.FromResult<IEnumerable<WHSignature>?>(null);
                }

                foreach (string sigValue in sigvalues)
                {
                    sigGroup = WHSignatureGroup.Unknow;
                    sigType = string.Empty;

                    try
                    {
                        splittedSig = tabRegex.Split(sigValue);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        return Task.FromResult<IEnumerable<WHSignature>?>(null);
                    }

                    sigName = splittedSig[0];


                    if (!string.IsNullOrWhiteSpace(splittedSig[2]))
                    {
                        var textGroup = splittedSig[2];
                        if (splittedSig[2].Contains(' '))
                            textGroup = splittedSig[2].Split(' ').First();

                        Enum.TryParse(textGroup, out sigGroup);

                        sigType = splittedSig[3];
                    }

                    sigResult.Add(new WHSignature(currentSystemScannedId, sigName, sigGroup, sigType, scanUser));
                }
            }

            return Task.FromResult<IEnumerable<WHSignature>?>(sigResult);

        }

        public async Task<bool> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted)
        {
            if (!await ValidateScanResult(scanResult))
                throw new Exception("Bad signatures format");

            var sigs = await ParseScanResult(scanUser, currentSystemScannedId, scanResult);

            if (sigs == null || !sigs.Any())
                throw new Exception("Bad signature parsing parameters");

            if (currentSystemScannedId <= 0)
                throw new Exception("Current System is nullable");

            var currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
            if (currentSystemSigs == null)
                return false;


            bool sigUpdated = false, sigAdded = false;

            if (lazyDeleted)
            {
                await DeleteSignatures(currentSystemSigs, sigs);
                currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
                if (currentSystemSigs == null)
                    return false;
            }

            sigUpdated = await UpdateSignatures(currentSystemSigs, sigs);
            sigAdded = await AddNewSignatures(currentSystemSigs, sigs, currentSystemScannedId);


            await Task.Delay(500); // Consider removing or justifying this delay.
            return sigUpdated || sigAdded;
        }

        private async Task<bool> UpdateSignatures(IEnumerable<WHSignature> currentSystemSigs, IEnumerable<WHSignature> sigs)
        {
            var sigsToUpdate = currentSystemSigs.IntersectBy(sigs.Select(x => x.Name), y => y.Name);
            if (!sigsToUpdate.Any())
                return false;

            foreach (var sig in sigsToUpdate)
            {
                var sigParse = sigs.FirstOrDefault(x => x.Name == sig.Name);
                if (sigParse != null && sigParse.Group != WHSignatureGroup.Unknow)
                {
                    sig.Group = sigParse.Group;
                    sig.Type = string.IsNullOrEmpty(sig.Type) ? sigParse.Type : sig.Type;
                    sig.Updated = sigParse.Updated;
                    sig.UpdatedBy = sigParse.UpdatedBy;
                }
            }

            var resUpdate = await _dbWHSignatures.Update(sigsToUpdate);
            return resUpdate != null && resUpdate.Count() == sigsToUpdate.Count();
        }

        private async Task<bool> AddNewSignatures(IEnumerable<WHSignature> currentSystemSigs, IEnumerable<WHSignature> sigs, int currentSystemScannedId)
        {
            var sigsToAdd = sigs.ExceptBy(currentSystemSigs.Select(x => x.Name), y => y.Name);
            if (!sigsToAdd.Any())
                return false;

            var resAdd = await _dbWHSignatures.Create(sigsToAdd);
            return resAdd != null && resAdd.Count() == sigsToAdd.Count();
        }

        private async Task DeleteSignatures(IEnumerable<WHSignature> currentSystemSigs, IEnumerable<WHSignature> sigs)
        {
            var sigsToDeleted = currentSystemSigs.ExceptBy(sigs.Select(x => x.Name), y => y.Name);
            foreach (var sig in sigsToDeleted)
            {
                await _dbWHSignatures.DeleteById(sig.Id);
            }
        }
    }
}
