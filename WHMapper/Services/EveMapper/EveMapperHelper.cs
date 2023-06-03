using System;
using Microsoft.AspNetCore.Components;
using WHMapper.Hubs;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Services.EveMapper
{
	public class EveMapperHelper : IEveMapperHelper
    {
        private IAnoikServices _anoikServices = null!;

        public EveMapperHelper(IAnoikServices anoikServices)
		{
            _anoikServices = anoikServices;
        }


        public async Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh)
        {
            EveSystemNodeModel res = null!;
            if (wh == null)
                throw new ArgumentNullException();

            if (wh.SecurityStatus <= -0.9)
            {

                var whClass = _anoikServices.GetSystemClass(wh.Name);
                var whEffect = _anoikServices.GetSystemEffects(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whStatics = await _anoikServices.GetSystemStatics(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whEffectsInfos = null!;
                if (!String.IsNullOrWhiteSpace(whEffect) && !string.IsNullOrEmpty(whClass))
                    whEffectsInfos = _anoikServices.GetSystemEffectsInfos(whEffect, whClass);
                
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

