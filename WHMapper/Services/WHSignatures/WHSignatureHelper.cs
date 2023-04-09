using System;
using System.Text.RegularExpressions;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.Db;

namespace WHMapper.Services.WHSignatures
{
	public class WHSignatureHelper
	{
		public WHSignatureHelper() : IWHSignatureHelper
		{
		}

        private async Task<IEnumerable<WHSignature>> ParseScanResult()
        {
            IList<WHSignature> sigResult = new List<WHSignature>();

            Regex lineRegex = new Regex("\n");
            Regex tabRegex = new Regex("\t");
            string[] sigvalues = lineRegex.Split(_scanResult);

            string scanUser = await UserService.GetUserName();

            foreach (string sigValue in sigvalues)
            {
                var splittedSig = tabRegex.Split(sigValue);
                WHSignature newSig = new WHSignature(splittedSig[0], scanUser);

                if (!String.IsNullOrWhiteSpace(splittedSig[2]))
                {
                    WHSignatureGroup group = WHSignatureGroup.Unknow;

                    var sigGroup = splittedSig[2];
                    if (splittedSig[2].Contains(' '))
                        sigGroup = splittedSig[2].Split(' ').First();

                    if (Enum.TryParse<WHSignatureGroup>(sigGroup, out group))
                        newSig.Group = group;
                }


                sigResult.Add(newSig);
            }

            return sigResult;

        }

}

