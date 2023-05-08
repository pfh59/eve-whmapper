using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WHMapper.Models.DTO.Anoik;
using static MudBlazor.Colors;

namespace WHMapper.Services.Anoik
{
    public class AnoikServices : IAnoikServices
    {
        private readonly ILogger _logger;

        private const string _anoikjson = @"./Resources/Anoik/static.json";
        private JsonDocument? _json;
        private JsonElement _jsonSystems;
        private JsonElement _jsonEffects;
        private JsonElement _jsonWormholes;



        public AnoikServices(ILogger<AnoikServices> logger)
        {
            _logger = logger;
            string jsonText = File.ReadAllText(_anoikjson);

            _json = JsonDocument.Parse(jsonText);
            _jsonSystems = _json.RootElement.GetProperty("systems");
            _jsonEffects = _json.RootElement.GetProperty("effects");
            _jsonWormholes = _json.RootElement.GetProperty("wormholes");

            _logger.LogInformation("AnoikServices Initialization");
        }


        public async Task<int?> GetSystemId(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var solarSystemID = sys.GetProperty("solarSystemID");

                return solarSystemID.GetInt32();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public async Task<string?> GetSystemClass(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var whClass = sys.GetProperty("wormholeClass");

                return whClass.GetString().ToUpper();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public async Task<string?> GetSystemEffects(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var whEffect = sys.GetProperty("effectName");

                return (whEffect.ValueKind == JsonValueKind.Null) ? String.Empty : whEffect.GetString();

            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public async Task<IEnumerable<KeyValuePair<string, string>>?> GetSystemStatics(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var statics = sys.GetProperty("statics").EnumerateArray();

                var res = new Dictionary<string, string>();

                while (statics.MoveNext())
                {
                    var whStaticType = statics.Current.GetString();
                    var whStaticDest = await GetWHClassFromWHType(whStaticType);
                    if (whStaticDest != null)
                        res.Add(whStaticType, whStaticDest);
                }

                return res;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectName"></param>
        /// <param name="systemClass"></param>
        /// <returns>return null effectname or systemclass are bad</returns>
        public async Task<IEnumerable<KeyValuePair<string, string>>?> GetSystemEffectsInfos(string effectName, string systemClass)
        {
            int classlvl = -1;
            if (string.IsNullOrWhiteSpace(effectName) || string.IsNullOrWhiteSpace(systemClass) || !(systemClass.Length >= 2 && systemClass.Length < 4 && systemClass.ToUpper().Contains('C')))
                return null;


            if (!string.IsNullOrWhiteSpace(systemClass) && systemClass.ToUpper().Contains('C'))
            {
                int.TryParse(systemClass.ToUpper().Split('C')[1], out classlvl);
            }
            if (classlvl > 6)
                classlvl = 6;

            try
            {
                var effects = _jsonEffects.GetProperty(effectName);

                var res = new Dictionary<string, string>();

                foreach (var jsonProperty in effects.EnumerateObject())
                {
                    var effectLevel = jsonProperty.Value.EnumerateArray().ElementAt(classlvl - 1);
                    res.Add(jsonProperty.Name, effectLevel.GetString());
                }

                return res;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", effectName));
                return null;
            }
        }


        private async Task<string?> GetWHClassFromWHType(string whType)
        {

            try
            {
                var whInfos = _jsonWormholes.GetProperty(whType);

                var whDest = whInfos.GetProperty("dest");

                return whDest.GetString().ToUpper();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", whType));
                return null;
            }
        }

        public async Task<IEnumerable<WormholeTypeInfo>?> GetWormholeTypes()
        {

            var res = _jsonWormholes.EnumerateObject()
                .Where(x => x.Name!="K162")
                .Select(x => new WormholeTypeInfo(x.Name.ToString(), x.Value.GetProperty("dest").GetString(), x.Value.GetProperty("src").Deserialize<string[]>()));


            res=res.Append(new WormholeTypeInfo("K162", "C1/2/3",null));
            res = res.Append(new WormholeTypeInfo("K162", "C4/5", null));
            res = res.Append(new WormholeTypeInfo("K162", "C6", null));
            res = res.Append(new WormholeTypeInfo("K162", "HS", null));
            res = res.Append(new WormholeTypeInfo("K162", "LS", null));
            res = res.Append(new WormholeTypeInfo("K162", "NS", null));
            res = res.Append(new WormholeTypeInfo("K162", "C12", null));

            return res.OrderBy(x => x.Name);
        }
    }
    
}

