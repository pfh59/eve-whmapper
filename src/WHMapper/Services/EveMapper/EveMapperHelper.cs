using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Anoik;
using WHMapper.Services.SDE;

namespace WHMapper.Services.EveMapper
{
    public class EveMapperHelper : IEveMapperHelper
    {
        private const string WH_VALIDATION_REGEX = "J[0-9]{6}|Thera|J1226-0|Sentinel MZ|Liberated Barbican|Sanctified Vidette|Conflux Eyrie|Azdaja Redoubt";
        private const string REGION_POCHVVEN_NAME = "Pochven";

        private const int GROUPE_WORMHOLE_ID = 988;
        private const string C14_NAME = "J055520";
        private const string C14_ALTERNATE_NAME = "Sentinel MZ";
        private const string C15_NAME = "J110145";
        private const string C15_ALTERNATE_NAME = "Liberated Barbican";
        private const string C16_NAME = "J164710";
        private const string C16_ALTERNATE_NAME = "Sanctified Vidette";
        private const string C17_NAME = "J200727";
        private const string C17_ALTERNATE_NAME = "Conflux Eyrie";
        private const string C18_NAME = "J174618";
        private const string C18_ALTERNATE_NAME = "Azdaja Redoubt";

        private readonly Dictionary<WHEffect, Dictionary<EveSystemType, IList<EveSystemEffect>>> _whEffects = new();

        private volatile IList<WormholeType> _whTypes = new List<WormholeType>();
        private readonly Task _initWormholeTypesTask;

        private ParallelOptions _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        private readonly ILogger? _logger;

        private readonly IAnoikServices _anoikServices;
        private readonly ISDEService _sdeServices;
        private readonly IWHNoteRepository _noteServices;
        private readonly IEveMapperService _eveMapperEntity;

        public EveMapperHelper(ILogger<EveMapperHelper> logger, IEveMapperService eveMapperEntity, ISDEService sdeServices, IAnoikServices anoikServices, IWHNoteRepository noteServices)
        {

            _logger = logger;
            _eveMapperEntity = eveMapperEntity;
            _sdeServices = sdeServices;
            _anoikServices = anoikServices;
            _noteServices = noteServices;

            LoadEffectsFromJson();

            _initWormholeTypesTask = Task.Run(InitWormholeTypeList);
        }

        private async Task EnsureWormholeTypesInitializedAsync()
        {
            await _initWormholeTypesTask.ConfigureAwait(false);
        }

        private void LoadEffectsFromJson()
        {
            _logger?.LogInformation("Loading WH effects from embedded resource");

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("WHMapper.Resources.wh-effects.json")
                ?? throw new InvalidOperationException("Embedded resource 'WHMapper.Resources.wh-effects.json' not found");

            var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<EveSystemEffect>>>>(stream)
                ?? throw new InvalidOperationException("Failed to deserialize wh-effects.json");

            foreach (var (effectName, classes) in raw)
            {
                if (!Enum.TryParse<WHEffect>(effectName, out var effect))
                    continue;

                var classDict = new Dictionary<EveSystemType, IList<EveSystemEffect>>();
                foreach (var (className, effects) in classes)
                {
                    if (Enum.TryParse<EveSystemType>(className, out var systemType))
                        classDict[systemType] = effects;
                }

                _whEffects[effect] = classDict;
            }
        }



        public ReadOnlyCollection<WormholeType> WormholeTypes
        {
            get
            {
                _initWormholeTypesTask.GetAwaiter().GetResult();
                return new ReadOnlyCollection<WormholeType>(_whTypes);
            }
        }

        private WHEffect GetWHEffectValueDescription(string description)
        {
            object? res = null;
            foreach (var field in typeof(WHEffect).GetFields())
            {
                if (System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                    {
                        res = field.GetValue(null);
                        if (res is WHEffect)
                            return (WHEffect)res;
                        else
                            return WHEffect.None;
                    }

                }
                else
                {
                    if (field.Name == description)
                    {
                        res = field.GetValue(null);
                        if (res is WHEffect)
                            return (WHEffect)res;
                        else
                            return WHEffect.None;
                    }
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
        }

        public bool IsWormhole(string systemName)
        {
            try
            {
                if (!string.IsNullOrEmpty(systemName))
                {
                    Match match = Regex.Match(systemName, WH_VALIDATION_REGEX, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
                    return match.Success;
                }
                return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public async Task<EveSystemType> GetWHClass(SystemEntity whSystem)
        {
            var system_constellation = await _eveMapperEntity.GetConstellation(whSystem.ConstellationId);
            if (system_constellation == null)
                throw new InvalidDataException("Constellation not found");

            var system_region = await _eveMapperEntity.GetRegion(system_constellation!.RegionId);
            if (system_region == null)
                throw new InvalidDataException("Region not found");

            return await GetWHClass(system_region!.Name, system_constellation!.Name, whSystem.Name, whSystem.SecurityStatus);
        }

    public Task<EveSystemType> GetWHClass(string regionName, string constellationName, string systemName, float securityStatus)
    {
        if (IsWormhole(systemName))
        {
            return Task.FromResult(GetWormholeSystemType(regionName, systemName));
        }
        else if (regionName == REGION_POCHVVEN_NAME) // Trig system
        {
            return Task.FromResult(EveSystemType.Pochven);
        }
        else
        {
            return Task.FromResult(GetKSpaceSystemType(securityStatus));
        }
    }

    private EveSystemType GetWormholeSystemType(string regionName, string systemName)
    {
        var firstChar = regionName.FirstOrDefault();
        return firstChar switch
        {
            'A' => EveSystemType.C1,
            'B' => EveSystemType.C2,
            'C' => EveSystemType.C3,
            'D' => EveSystemType.C4,
            'E' => EveSystemType.C5,
            'F' => EveSystemType.C6,
            'G' => EveSystemType.Thera,
            'H' => EveSystemType.C13,
            'K' => GetSpecialWormholeSystemType(systemName),
            _ => EveSystemType.None,
        };
    }

    private EveSystemType GetSpecialWormholeSystemType(string systemName)
    {
        return systemName switch
        {
            C14_NAME => EveSystemType.C14,
            C15_NAME => EveSystemType.C15,
            C16_NAME => EveSystemType.C16,
            C17_NAME => EveSystemType.C17,
            C18_NAME => EveSystemType.C18,
            C14_ALTERNATE_NAME => EveSystemType.C14,
            C15_ALTERNATE_NAME => EveSystemType.C15,
            C16_ALTERNATE_NAME => EveSystemType.C16,
            C17_ALTERNATE_NAME => EveSystemType.C17,
            C18_ALTERNATE_NAME => EveSystemType.C18,
            _ => EveSystemType.None,
        };
    }

    private EveSystemType GetKSpaceSystemType(float securityStatus)
    {
        if (securityStatus >= 0.5)
            return EveSystemType.HS;
        else if (securityStatus < 0.5 && securityStatus > 0)
            return EveSystemType.LS;
        else
            return EveSystemType.NS;
    }

        private async Task<WHEffect> GetSystemEffect(string systemName)
        {
            WHEffect effect = WHEffect.None;
            if (IsWormhole(systemName))//WH system
            {
                IEnumerable<SDESolarSystem>? sdeWormholesInfos = await _sdeServices!.SearchSystem(systemName);
                SDESolarSystem? sdeInfos = sdeWormholesInfos?.FirstOrDefault();

                if (sdeInfos != null && sdeInfos.SecondarySun != null)
                {
                    SunEntity? secondSun = await _eveMapperEntity.GetSun(sdeInfos.SecondarySun.TypeID);
                    if (secondSun != null)
                        effect = GetWHEffectValueDescription(secondSun.Name);
                    else
                        effect = WHEffect.None;
                }
            }
            return effect;
        }

        private IList<EveSystemEffect>? GetWHEffectDetails(WHEffect effect, EveSystemType whClass)
        {
            if (_whEffects.TryGetValue(effect, out var classes) && classes.TryGetValue(whClass, out var effects))
                return effects;

            return null;
        }

        public async Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh)
        {
            EveSystemNodeModel res = null!;

            var system = await _eveMapperEntity.GetSystem(wh.SoloarSystemId);
            if (system == null)
                throw new InvalidDataException("System not found");

            var system_constellation = await _eveMapperEntity.GetConstellation(system.ConstellationId);
            if (system_constellation == null)
                throw new InvalidDataException("Constellation not found");

            var system_region = await _eveMapperEntity.GetRegion(system_constellation.RegionId);
            if (system_region == null)
                throw new InvalidDataException("Region not found");

            

            var note = await _noteServices.Get(wh.WHMapId,system.Id);

            if (IsWormhole(wh.Name))//WH system
            {
                EveSystemType whClass = await GetWHClass(system_region!.Name, system_constellation.Name, system.Name, system.SecurityStatus);
                WHEffect whEffect = await GetSystemEffect(system.Name);
                IList<EveSystemEffect>? effectDetails = GetWHEffectDetails(whEffect, whClass);
                IList<WormholeType>? statics = null;

                IEnumerable<KeyValuePair<string, string>>? whStatics = await _anoikServices!.GetSystemStatics(wh.Name);
                
                await EnsureWormholeTypesInitializedAsync();
                if (whStatics!=null && whStatics.Any() && _whTypes != null && _whTypes.Any()) 
                {
                    statics = whStatics.Select(x => _whTypes.FirstOrDefault<WormholeType>(y => String.Equals(y.Name,x.Key,StringComparison.OrdinalIgnoreCase))).Where(y => y != null).ToList<WormholeType>();
                }

                res = new EveSystemNodeModel(wh, note, system_region.Name, system_constellation.Name, whClass, whEffect, effectDetails, statics);
            }
            else if (system_region!.Name == REGION_POCHVVEN_NAME)//trig system
            {
                res = new EveSystemNodeModel(wh, note, system_region.Name, system_constellation.Name, EveSystemType.Pochven, WHEffect.None, null, null);
            }
            else// K-space
            {
                res = new EveSystemNodeModel(wh, note, system_region.Name, system_constellation.Name);
            }

            res.SetPosition(wh.PosX, wh.PosY);
            return res;
        }

        private async Task InitWormholeTypeList()
        {
            _logger?.LogInformation("Init wormhole type list");
            try
            {
                GroupEntity? whGroup = await _eveMapperEntity.GetGroup(GROUPE_WORMHOLE_ID);
                if (whGroup == null)
                    throw new InvalidDataException("Wormhole group not found");

                var bag = new ConcurrentBag<WormholeType>();
                var addedNames = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

                await Parallel.ForEachAsync(whGroup.Types, _options, async (whTypeId, token) =>
                {
                    var whType = await _eveMapperEntity.GetWormhole(whTypeId);
                    if (whType != null)
                    {
                        if (addedNames.TryAdd(whType.Name, 0))
                        {
                            switch (whType.SystemTypeValue)
                            {
                                case 0: //K162
                                    bag.Add(new WormholeType("K162", EveSystemType.C1));
                                    bag.Add(new WormholeType("K162", EveSystemType.C2));
                                    bag.Add(new WormholeType("K162", EveSystemType.C3));
                                    bag.Add(new WormholeType("K162", EveSystemType.C4));
                                    bag.Add(new WormholeType("K162", EveSystemType.C5));
                                    bag.Add(new WormholeType("K162", EveSystemType.C6));
                                    bag.Add(new WormholeType("K162", EveSystemType.HS));
                                    bag.Add(new WormholeType("K162", EveSystemType.LS));
                                    bag.Add(new WormholeType("K162", EveSystemType.NS));
                                    break;
                                case 10:
                                case 11:
                                    //QA WH A and QA WH B, unused WH
                                    break;
                                default:
                                    int sys_type_value = (int)whType.SystemTypeValue;
                                    if (Enum.IsDefined(typeof(EveSystemType), sys_type_value))
                                    {
                                        bag.Add(new WormholeType(whType));
                                    }
                                    else
                                    {
                                        _logger?.LogWarning("Unknown wormhole type");
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            _logger?.LogInformation("Already added: {Name}", whType.Name);
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("Nullable wormhole type, value : {whTypeId}", whTypeId);
                    }
                });

                _whTypes = bag.OrderBy(x => x.Name).ToList();
                _logger?.LogInformation("Wormhole type list initialized with {Count} entries", _whTypes.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize wormhole type list");
                throw;
            }
        }

        public async Task<bool> IsRouteViaWH(SystemEntity src, SystemEntity dst)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (dst == null)
                throw new ArgumentNullException(nameof(dst));

            if (src.Stargates == null || dst.Stargates == null)
                return true;

            if (src.Stargates.Length == 0 || dst.Stargates.Length == 0)
                return true;

            int[]? startgatesToCheck = null;
            int systemTarget = -1;

            if (src.Stargates.Length <= dst.Stargates.Length)
            {
                startgatesToCheck = dst.Stargates;
                systemTarget = src.Id;
            }
            else
            {
                startgatesToCheck = src.Stargates;
                systemTarget = dst.Id;
            }

            foreach (int sgId in startgatesToCheck)
            {
                var sg = await _eveMapperEntity.GetStargate(sgId);
                if (sg != null && sg.DestinationId == systemTarget)
                    return false;
            }

            return true;
        }
    }
}
