using System.Text.Json;
using WHMapper.Models.DTO.Anoik;

namespace WHMapper.Services.Anoik
{
    public class AnoikServices : IAnoikServices
    {
        private readonly ILogger _logger;
        private readonly JsonElement _jsonEffects;
        private readonly JsonElement _jsonWormholes;
        private readonly JsonElement _jsonSystems;

        public AnoikServices(ILogger<AnoikServices> logger, IAnoikDataSupplier dataSupplier)
        {
            _logger = logger;
            _logger.LogInformation("AnoikServices Initialization");
            _jsonEffects = dataSupplier.GetEffect();
            _jsonWormholes = dataSupplier.GetWormHoles();
            _jsonSystems = dataSupplier.GetSystems();
            _logger.LogInformation("AnoikServices Initialized");
        }

        public int? GetSystemId(string systemName)
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
        /// Returns the System Class for a system.
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public string? GetSystemClass(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var whClassJSON = sys.GetProperty("wormholeClass");
                var whClass = whClassJSON.GetString();

                if (String.IsNullOrEmpty(whClass))
                {
                    return string.Empty;
                }
                else
                {
                    return whClass.ToUpper();
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return null;
            }
        }

        /// <summary>
        /// Returns the System Effects for a system.
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public string? GetSystemEffects(string systemName)
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
        /// Returns the System Statics for a system. 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns> A null value represent a not found system</returns>
        public Task<IEnumerable<KeyValuePair<string, string>>> GetSystemStatics(string systemName)
        {
            try
            {
                var sys = _jsonSystems.GetProperty(systemName);
                var statics = sys.GetProperty("statics");
                var collectionOfStatics = statics.EnumerateArray();

                var res = new Dictionary<string, string>();

                while (collectionOfStatics.MoveNext())
                {
                    var whStaticType = collectionOfStatics.Current.GetString();
                    if (!String.IsNullOrEmpty(whStaticType))
                    {
                        var whStaticDest = GetWHClassFromWHType(whStaticType);
                        if (!string.IsNullOrEmpty(whStaticDest))
                            res.Add(whStaticType, whStaticDest);
                    }
                }

                return Task.FromResult<IEnumerable<KeyValuePair<string, string>>>(res);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", systemName));
                return Task.FromResult<IEnumerable<KeyValuePair<string, string>>>(null!);
            }
        }

        /// <summary>
        /// Returns the System Effects for a given effect and systemclass combination.
        /// </summary>
        /// <param name="effectName"></param>
        /// <param name="systemClass"></param>
        /// <returns>return null effectname or systemclass are bad</returns>
        public IEnumerable<KeyValuePair<string, string>> GetSystemEffectsInfos(string effectName, string systemClass)
        {
            int classlvl = -1;
            if (string.IsNullOrWhiteSpace(effectName)
                || string.IsNullOrWhiteSpace(systemClass)
                || !(systemClass.Length >= 2 && systemClass.Length < 4 && systemClass.ToUpper().Contains('C')))
            {
                return null!;
            }

            if (!string.IsNullOrWhiteSpace(systemClass) && systemClass.ToUpper().Contains('C'))
            {
                int.TryParse(systemClass.ToUpper().Split('C')[1], out classlvl);
            }

            if (classlvl > 6)
            {
                classlvl = 6;
            }

            try
            {
                var effects = _jsonEffects.GetProperty(effectName);
                var res = new Dictionary<string, string>();

                foreach (var jsonProperty in effects.EnumerateObject())
                {
                    var effectLevelJSONProperty = jsonProperty.Value.EnumerateArray().ElementAt(classlvl - 1);
                    var effectLevel = effectLevelJSONProperty.GetString();
                    if (!string.IsNullOrEmpty(effectLevel))
                    {
                        res.Add(jsonProperty.Name, effectLevel);
                    }
                }

                return res;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", effectName));
                return null!;
            }
        }

        private string? GetWHClassFromWHType(string whType)
        {
            try
            {
                var whInfos = _jsonWormholes.GetProperty(whType);
                var whDestJSONProperty = whInfos.GetProperty("dest");
                var whDest = whDestJSONProperty.GetString();

                if (!string.IsNullOrEmpty(whDest))
                {
                    return whDest.ToUpper();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation(string.Format("Key not found {0} not found", whType));
                return null;
            }
        }

        public Task<IEnumerable<WormholeTypeInfo>> GetWormholeTypes()
        {
            var res = _jsonWormholes.EnumerateObject()
                .Where(x => x.Name != "K162")
                .Select(x =>
                    new WormholeTypeInfo(
                        x.Name.ToString(),
                        x.Value.GetProperty("dest").GetString(),
                        x.Value.GetProperty("src").Deserialize<string[]>()
                    ));

            res = res.Append(new WormholeTypeInfo("K162", "C1/2/3", null));
            res = res.Append(new WormholeTypeInfo("K162", "C4/5", null));
            res = res.Append(new WormholeTypeInfo("K162", "C6", null));
            res = res.Append(new WormholeTypeInfo("K162", "HS", null));
            res = res.Append(new WormholeTypeInfo("K162", "LS", null));
            res = res.Append(new WormholeTypeInfo("K162", "NS", null));
            res = res.Append(new WormholeTypeInfo("K162", "C12", null));
            res = res.Append(new WormholeTypeInfo("X450", "Pochven", null));
            res = res.Append(new WormholeTypeInfo("R081", "Pochven", null));
            res = res.Append(new WormholeTypeInfo("U372", "Pochven", null));
            res = res.Append(new WormholeTypeInfo("F216", "Pochven", null));
            res = res.Append(new WormholeTypeInfo("C729", "Pochven", null));

            return Task.FromResult<IEnumerable<WormholeTypeInfo>>(res.OrderBy(x => x.Name));
        }
    }
}