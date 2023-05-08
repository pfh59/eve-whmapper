using System;
using System.Text.RegularExpressions;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.Db;
using WHMapper.Services.WHSignature;
using WHMapper.Models.Custom;
using System.Security.Cryptography;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Services.WHSignatures
{
    public class WHSignatureHelper : IWHSignatureHelper
    {
        private const string SCAN_VALIDATION_REGEX = "[a-zA-Z]{3}-[0-9]{3}\\s([a-zA-Z\\s]+)[0-9]*.[0-9]+%\\s[0-9]*.[0-9]+\\sAU";


        private IWHSystemRepository _dbWHSystem;

        private IWHSignatureRepository _dbWHSignatures;

        
        public WHSignatureHelper(IWHSystemRepository systemRepo, IWHSignatureRepository sigRepo)
        {
            _dbWHSystem = systemRepo;
            _dbWHSignatures = sigRepo;
        }

        public async Task<bool> ValidateScanResult(string? scanResult)
        {
            try
            {
                if (!string.IsNullOrEmpty(scanResult))
                {
                    Match match = Regex.Match(scanResult, SCAN_VALIDATION_REGEX, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
                    return match.Success;
                }
                return false;
            }
            catch(RegexMatchTimeoutException)
            {
                return false;
            }

        }

        public async Task<IEnumerable<WHMapper.Models.Db.WHSignature>> ParseScanResult(string scanUser,string? scanResult)
        {
            string sigName = null;
            WHSignatureGroup sigGroup = WHSignatureGroup.Unknow;
            string sigType = null;
            string[] sigvalues = null;
            string[] splittedSig = null;
            


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
                    return null;
                }

                foreach (string sigValue in sigvalues)
                {
                    sigGroup = WHSignatureGroup.Unknow;
                    sigType = null;

                    try
                    { 
                        splittedSig = tabRegex.Split(sigValue);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        return null;
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
         
                    sigResult.Add(new WHMapper.Models.Db.WHSignature(sigName, sigGroup, sigType, scanUser));
                }
            }

            return sigResult;

        }

        public async Task<bool> ImportScanResult(string scanUser,int currentSystemScannedId,string? scanResult)
        {
            bool sigUpdated = true;
            bool sigAdded = true;

            if (!await ValidateScanResult(scanResult))
                throw new Exception("Bad signatures format");


            var sigs = await ParseScanResult(scanUser, scanResult);

            if (sigs != null && sigs.Count() > 0)
            {
                var currentSystem = await _dbWHSystem.GetById(currentSystemScannedId);
                var sigsToUpdate = currentSystem?.WHSignatures.IntersectBy(sigs.Select(x => x.Name), y => y.Name);

                if (sigsToUpdate != null && sigsToUpdate.Count() > 0)
                {
                    foreach (var sig in sigsToUpdate)
                    {
                        var sigParse = sigs.Where(x => x.Name == sig.Name).FirstOrDefault();
                        if (sigParse.Group != WHSignatureGroup.Unknow)
                        {

                            sig.Group = sigParse.Group;
                            if(String.IsNullOrEmpty(sig.Type))
                                sig.Type = sigParse.Type;
      
                        }

                        sig.Updated = sigParse.Updated;
                        sig.UpdatedBy = sigParse.UpdatedBy;
                    }

                    var resUpdate = await _dbWHSignatures.Update(sigsToUpdate);

                    if (resUpdate != null && resUpdate.Count() == sigsToUpdate.Count())
                        sigUpdated=true;
                    else
                        sigUpdated=false;
                }


                var sigsToAdd = sigs.ExceptBy(currentSystem?.WHSignatures.Select(x => x.Name), y => y.Name);
                if (sigsToAdd != null && sigsToAdd.Count() > 0)
                {
                    var resAdd = await _dbWHSystem.AddWHSignatures(currentSystemScannedId, sigsToAdd);
                    if (resAdd != null && resAdd.Count() == sigsToAdd.Count())
                        sigAdded=true;
                    else
                        sigAdded=false;
                }

                return(sigUpdated || sigAdded);
            }
            else
                throw new Exception("Bad signature parsing parameters");
        }


    }

}

