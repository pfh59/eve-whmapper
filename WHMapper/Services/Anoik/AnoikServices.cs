using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace WHMapper.Services.Anoik
{
    public class AnoikServices : IAnoikServices
    {
        private const string _anoikjson = "http://anoik.is/static/static.json";
        private JsonDocument? _json;
        private JsonElement _jsonSystems;
        private JsonElement _jsonEffects;
        private JsonElement _jsonWormholes;
        

        //private HttpClient _client;

        public AnoikServices()
        {
            Init();
        }


        private async Task Init()
        {
            var _client = new HttpClient();

            HttpResponseMessage response = await _client.GetAsync(_anoikjson);
            response.EnsureSuccessStatusCode();

            Stream anoikStream = await response.Content.ReadAsStreamAsync();


            _json = JsonDocument.Parse(anoikStream);
            _jsonSystems = _json.RootElement.GetProperty("systems");
            _jsonEffects = _json.RootElement.GetProperty("effects");
            _jsonWormholes = _json.RootElement.GetProperty("wormholes");
        }



        public Task<string> GetSystemClass(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var whClass = sys.GetProperty("wormholeClass");


            return Task.FromResult(whClass.GetString().ToUpper());
        }

        public Task<string> GetSystemEffects(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var whEffect = sys.GetProperty("effectName");
            return Task.FromResult(whEffect.GetString());
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetSystemStatics(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var statics = sys.GetProperty("statics").EnumerateArray();

            var res = new Dictionary<string,string>();

            while (statics.MoveNext())
            {
                var whStaticType = statics.Current.GetString();
                var whStaticDest = await GetWHClassFromWHType(whStaticType);
                res.Add(whStaticType, whStaticDest);
            }

            return res;

        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetSystemEffectsInfos(string effectName, string systemClass)
        {
            int classlvl = -1;

            if (!string.IsNullOrWhiteSpace(systemClass) && systemClass.ToUpper().Contains('C'))
            {
                int.TryParse(systemClass.ToUpper().Split('C')[1], out classlvl);
            }
            if (classlvl > 6)
                classlvl = 6;

            if (_json == null)
                await Init();

            var effects = _jsonEffects.GetProperty(effectName);
            var res = new Dictionary<string, string>();

            foreach (var jsonProperty in effects.EnumerateObject())
            {
                var effectLevel = jsonProperty.Value.EnumerateArray().ElementAt(classlvl - 1);
                res.Add(jsonProperty.Name, effectLevel.GetString());
            }

            return res;
        }



        private Task<string> GetWHClassFromWHType(string whType)
        {
            var whInfos = _jsonWormholes.GetProperty(whType);

            var whDest = whInfos.GetProperty("dest");

            return Task.FromResult(whDest.GetString().ToLower().ToUpper());
        }
    }
}

