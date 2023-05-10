using System;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Services.Anoik;

namespace WHMapper.Services.EveMapper
{
	public class EveMapperHelper : IEveMapperHelper
    {
        private IAnoikServices _anoikServices;

        public EveMapperHelper(IAnoikServices anoikServices)
		{
            _anoikServices = anoikServices;
        }


        public async Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh)
        {
            EveSystemNodeModel res = null;
            if (wh == null)
                throw new ArgumentNullException();

            if (wh.SecurityStatus <= -0.9)
            {

                string whClass = await _anoikServices.GetSystemClass(wh.Name);
                string whEffect = await _anoikServices.GetSystemEffects(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whStatics = await _anoikServices.GetSystemStatics(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whEffectsInfos = null;
                if (!String.IsNullOrWhiteSpace(whEffect))
                {
                    whEffectsInfos = await _anoikServices.GetSystemEffectsInfos(whEffect, whClass);
                }

                res = new EveSystemNodeModel(wh, whClass, whEffect, whEffectsInfos, whStatics);


            }
            else
            {
                res = new EveSystemNodeModel(wh);
            }

            res.SetPosition(wh.PosX, wh.PosY);
            return res;
        }
    }


}

