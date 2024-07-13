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
                    Match match = Regex.Match(scanResult, IWHSignatureHelper.SCAN_VALIDATION_REGEX, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
                    return Task.FromResult<bool>(match.Success);
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
            string sigName = string.Empty;
            WHSignatureGroup sigGroup = WHSignatureGroup.Unknow;
            string sigType = string.Empty;
            string[]? sigvalues = null;
            string[]? splittedSig = null;

            IList<WHMapper.Models.Db.WHSignature> sigResult = new List<WHMapper.Models.Db.WHSignature>();

            if (!string.IsNullOrEmpty(scanResult))
            {
                Regex lineRegex = new Regex("\n", RegexOptions.None, TimeSpan.FromSeconds(2));
                Regex tabRegex = new Regex("\t", RegexOptions.None, TimeSpan.FromSeconds(2));
                try
                {
                    sigvalues = lineRegex.Split(scanResult);
                }
                catch (RegexMatchTimeoutException)
                {
                    return Task.FromResult<IEnumerable<WHMapper.Models.Db.WHSignature>?>(null);
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
                        return Task.FromResult<IEnumerable<WHMapper.Models.Db.WHSignature>?>(null);
                    }

                    sigName = splittedSig[0];


                    if (!String.IsNullOrWhiteSpace(splittedSig[2]))
                    {
                        var textGroup = splittedSig[2];
                        if (splittedSig[2].Contains(' '))
                            textGroup = splittedSig[2].Split(' ').First();

                        Enum.TryParse<WHSignatureGroup>(textGroup, out sigGroup);

                        sigType = splittedSig[3];
                    }

                    sigResult.Add(new WHMapper.Models.Db.WHSignature(currentSystemScannedId, sigName, sigGroup, sigType, scanUser));
                }
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

            var sigsToUpdate = currentSystemSigs.IntersectBy(parsedSigs.Select(x => x.Name), y => y.Name);
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
            if (currentSystemSigs == null) return false;


            bool sigUpdated = false, sigAdded = false;

            if (lazyDeleted)
            {
                await DeleteSignatures(currentSystemSigs, sigs);
                currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
                if (currentSystemSigs == null) return false;
            }

            sigUpdated = await UpdateSignatures(currentSystemSigs, sigs);
            sigAdded = await AddNewSignatures(currentSystemSigs, sigs, currentSystemScannedId);


            await Task.Delay(500); // Consider removing or justifying this delay.
            return sigUpdated || sigAdded;
        }

        private async Task<bool> UpdateSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs)
        {
            var sigsToUpdate = currentSystemSigs.IntersectBy(sigs.Select(x => x.Name), y => y.Name);
            if (!sigsToUpdate.Any()) return false;

            foreach (var sig in sigsToUpdate)
            {
                var sigParse = sigs.FirstOrDefault(x => x.Name == sig.Name);
                if (sigParse != null && sigParse.Group != WHSignatureGroup.Unknow)
                {
                    sig.Group = sigParse.Group;
                    sig.Type = String.IsNullOrEmpty(sig.Type) ? sigParse.Type : sig.Type;
                    sig.Updated = sigParse.Updated;
                    sig.UpdatedBy = sigParse.UpdatedBy;
                }
            }

            var resUpdate = await _dbWHSignatures.Update(sigsToUpdate);
                return resUpdate != null && resUpdate.Count() == sigsToUpdate.Count();
        }

        private async Task<bool> AddNewSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs, int currentSystemScannedId)
        {
            var sigsToAdd = sigs.ExceptBy(currentSystemSigs.Select(x => x.Name), y => y.Name);
            if (!sigsToAdd.Any()) return false;

            var resAdd = await _dbWHSignatures.Create(sigsToAdd);
            return resAdd != null && resAdd.Count() == sigsToAdd.Count();
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
