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

            return sigUpdated || sigAdded;
        }

        private async Task<bool> UpdateSignatures(IEnumerable<Models.Db.WHSignature> currentSystemSigs, IEnumerable<Models.Db.WHSignature> sigs)
        {
            var sigsToUpdate = currentSystemSigs.IntersectBy(sigs.Select(x => x.Name), y => y.Name);
            if (!sigsToUpdate.Any()) return false;

            foreach (var sig in sigsToUpdate)
            {
                var sigParse = sigs.FirstOrDefault(x => x.Name == sig.Name);
                if (sigParse != null)
                {
                    sig.Updated = sigParse.Updated;
                    sig.UpdatedBy = sigParse.UpdatedBy;
                    if(sigParse.Group != WHSignatureGroup.Unknow)
                    {
                        sig.Group = sigParse.Group;
                        sig.Type = String.IsNullOrEmpty(sig.Type) ? sigParse.Type : sig.Type;
                    }
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
