using System.Text.RegularExpressions;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Services.WHSignature;

namespace WHMapper.Services.WHSignatures
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

        public async Task<bool> ImportScanResult(string scanUser, int currentSystemScannedId, string? scanResult, bool lazyDeleted)
        {

            bool sigUpdated = false;
            bool sigAdded = false;

            if (!await ValidateScanResult(scanResult))
                throw new Exception("Bad signatures format");


            var sigs = await ParseScanResult(scanUser, currentSystemScannedId, scanResult);

            if (sigs != null && sigs.Count() > 0)
            {

                if (currentSystemScannedId > 0)
                {
                    var currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
                    if (currentSystemSigs == null)
                        return false;//to do lod

                    if (lazyDeleted)
                    {
                        var sigsToDeleted = currentSystemSigs.ExceptBy(sigs.Select(x => x.Name), y => y.Name);
                        if (sigsToDeleted != null && sigsToDeleted.Count() > 0)
                        {
                            foreach (var sig in sigsToDeleted)
                            {
                                await _dbWHSignatures.DeleteById(sig.Id);
                            }

                            currentSystemSigs = await _dbWHSignatures.GetByWHId(currentSystemScannedId);
                            if (currentSystemSigs == null)
                                return false;
                        }

                    }
                    var sigsToUpdate = currentSystemSigs.IntersectBy(sigs.Select(x => x.Name), y => y.Name);
                    if (sigsToUpdate != null && sigsToUpdate.Count() > 0)
                    {
                        foreach (var sig in sigsToUpdate)
                        {
                            var sigParse = sigs.Where(x => x.Name == sig.Name).FirstOrDefault();
                            if (sigParse != null)
                            {
                                if (sigParse.Group != WHSignatureGroup.Unknow)
                                {

                                    sig.Group = sigParse.Group;
                                    if (String.IsNullOrEmpty(sig.Type))
                                        sig.Type = sigParse.Type;

                                }

                                sig.Updated = sigParse.Updated;
                                sig.UpdatedBy = sigParse.UpdatedBy;
                            }
                            else
                            {
                                //todo add log
                            }
                        }

                        var resUpdate = await _dbWHSignatures.Update(sigsToUpdate);

                        if (resUpdate != null && resUpdate.Count() == sigsToUpdate.Count())
                            sigUpdated = true;
                        else
                            sigUpdated = false;
                    }


                    var sigsToAdd = sigs.ExceptBy(currentSystemSigs.Select(x => x.Name), y => y.Name);
                    if (sigsToAdd != null && sigsToAdd.Count() > 0)
                    {
                        //var resAdd = await _dbWHSystem.AddWHSignatures(currentSystemScannedId, sigsToAdd);

                        var resAdd = await _dbWHSignatures.Create(sigsToAdd);
                        if (resAdd != null && resAdd.Count() == sigsToAdd.Count())
                            sigAdded = true;
                        else
                            sigAdded = false;
                    }
                    await Task.Delay(500);
                    return (sigUpdated || sigAdded);
                }
                else
                    throw new Exception("Current System is nullable");
            }
            else
                throw new Exception("Bad signature parsing parameters");
        }


    }

}

