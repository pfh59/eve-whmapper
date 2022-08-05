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
        private JsonDocument _json;
        private JsonElement _jsonSystems;

        public AnoikServices()
        {

            WebClient MyWebClient = new WebClient();
            byte[] BytesFile = MyWebClient.DownloadData(_anoikjson);

            MemoryStream iStream = new MemoryStream(BytesFile);


            _json = JsonDocument.Parse(iStream);
            _jsonSystems = _json.RootElement.GetProperty("systems");
        }

        public async Task<string> GetSystemClass(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var whClass = sys.GetProperty("wormholeClass");


            return whClass.GetString();
        }

        public async Task<string> GetSystemEffects(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var whEffect = sys.GetProperty("effectName");
            return whEffect.GetString();
        }

        public async Task<IEnumerable<string>> GetSystemStatics(string systemName)
        {
            var sys = _jsonSystems.GetProperty(systemName);
            var statics = sys.GetProperty("statics").EnumerateArray();

            var res = new List<string>();

            while (statics.MoveNext())
            {
                res.Add(statics.Current.GetString());
            }

            return res;

        }
    }
}
