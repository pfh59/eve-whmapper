using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private const string WH_VALIDATION_REGEX = "J[0-9]{6}|Thera|J1226-0";
        private const string REGION_POCHVVEN_NAME = "Pochven";

        private const int GROUPE_WORMHOLE_ID = 988;
        private const string C14_NAME = "J055520";
        private const string C15_NAME = "J110145";
        private const string C16_NAME = "J164710";
        private const string C17_NAME = "J200727";
        private const string C18_NAME = "J174618";

        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _magnetarEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(7);
        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _redGiantEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(7);
        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _pulsarEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(7);
        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _wolfRayetEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(8);
        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _cataclysmicEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(7);
        private readonly IDictionary<EveSystemType, IList<EveSystemEffect>> _blackHoleEffects = new Dictionary<EveSystemType, IList<EveSystemEffect>>(6);

        private IList<WormholeType> _whTypes = new List<WormholeType>();

        private ParallelOptions _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        private readonly ILogger? _logger;

        private readonly IAnoikServices _anoikServices;
        private readonly ISDEService _sdeServices;
        private readonly IWHNoteRepository _noteServices;
        private readonly IEveMapperEntity _eveMapperEntity;

        public EveMapperHelper(ILogger<EveMapperHelper> logger, IEveMapperEntity eveMapperEntity, ISDEService sdeServices, IAnoikServices anoikServices, IWHNoteRepository noteServices)
        {

            _logger = logger;
            _eveMapperEntity = eveMapperEntity;
            _sdeServices = sdeServices;
            _anoikServices = anoikServices;
            _noteServices = noteServices;

            InitMagnetarEffects();
            InitRedGiantEffects();
            InitPulsarEffects();
            InitWolfRayetEffects();
            InitCataclysmicEffects();
            InitBlackHoleEffects();

            Task.Run(async () => await InitWormholeTypeList());
        }

        #region Init WH effects
        private void InitMagnetarEffects()
        {
            _logger?.LogInformation("Init magnetar effects");
            _magnetarEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 30),
                new EveSystemEffect("Missile exp. radius", 15),
                new EveSystemEffect("Drone tracking", -15),
                new EveSystemEffect("Targeting range", -15),
                new EveSystemEffect("Tracking speed", -15),
                new EveSystemEffect("Target Painter strength", -15)
            });


            _magnetarEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 44),
                new EveSystemEffect("Missile exp. radius", 22),
                new EveSystemEffect("Drone tracking", -22),
                new EveSystemEffect("Targeting range", -22),
                new EveSystemEffect("Tracking speed", -22),
                new EveSystemEffect("Target Painter strength", -22)
            });

            _magnetarEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 58),
                new EveSystemEffect("Missile exp. radius", 29),
                new EveSystemEffect("Drone tracking", -29),
                new EveSystemEffect("Targeting range", -29),
                new EveSystemEffect("Tracking speed", -29),
                new EveSystemEffect("Target Painter strength", -29)
            });

            _magnetarEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile exp. radius", 36),
                new EveSystemEffect("Drone tracking", -36),
                new EveSystemEffect("Targeting range", -36),
                new EveSystemEffect("Tracking speed", -36),
                new EveSystemEffect("Target Painter strength", -36)
            });

            _magnetarEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 86),
                new EveSystemEffect("Missile exp. radius", 43),
                new EveSystemEffect("Drone tracking", -43),
                new EveSystemEffect("Targeting range", -43),
                new EveSystemEffect("Tracking speed", -43),
                new EveSystemEffect("Target Painter strength", -43)
            });

            _magnetarEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 100),
                new EveSystemEffect("Missile exp. radius", 50),
                new EveSystemEffect("Drone tracking", -50),
                new EveSystemEffect("Targeting range", -50),
                new EveSystemEffect("Tracking speed", -50),
                new EveSystemEffect("Target Painter strength", -50)

            });

            _magnetarEffects.Add(EveSystemType.C16, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Damage", 44),
                new EveSystemEffect("Missile exp. radius", 22),
                new EveSystemEffect("Drone tracking", -22),
                new EveSystemEffect("Targeting range", -22),
                new EveSystemEffect("Tracking speed", -22),
                new EveSystemEffect("Target Painter strength", -22)
            });
        }

        private void InitRedGiantEffects()
        {
            _logger?.LogInformation("Init redgiant effects");
            _redGiantEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 15),
                new EveSystemEffect("Overload bonus", 30),
                new EveSystemEffect("Smart Bomb range", 30),
                new EveSystemEffect("Smart Bomb damage", 30),
                new EveSystemEffect("Bomb damage", 30)
            });

            _redGiantEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 22),
                new EveSystemEffect("Overload bonus", 44),
                new EveSystemEffect("Smart Bomb range", 44),
                new EveSystemEffect("Smart Bomb damage", 44),
                new EveSystemEffect("Bomb damage", 44)

            });

            _redGiantEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 29),
                new EveSystemEffect("Overload bonus", 58),
                new EveSystemEffect("Smart Bomb range", 58),
                new EveSystemEffect("Smart Bomb damage", 58),
                new EveSystemEffect("Bomb damage", 58)
            });

            _redGiantEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 36),
                new EveSystemEffect("Overload bonus", 72),
                new EveSystemEffect("Smart Bomb range", 72),
                new EveSystemEffect("Smart Bomb damage", 72),
                new EveSystemEffect("Bomb damage", 72)
            });

            _redGiantEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 43),
                new EveSystemEffect("Overload bonus", 86),
                new EveSystemEffect("Smart Bomb range", 86),
                new EveSystemEffect("Smart Bomb damage", 86),
                new EveSystemEffect("Bomb damage", 86)
            });

            _redGiantEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 50),
                new EveSystemEffect("Overload bonus", 100),
                new EveSystemEffect("Smart Bomb range", 100),
                new EveSystemEffect("Smart Bomb damage", 100),
                new EveSystemEffect("Bomb damage", 100)
            });

            _redGiantEffects.Add(EveSystemType.C14, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Heat damage", 22),
                new EveSystemEffect("Overload bonus", 44),
                new EveSystemEffect("Smart Bomb range", 44),
                new EveSystemEffect("Smart Bomb damage", 44),
                new EveSystemEffect("Bomb damage", 44)
            });
        }

        private void InitPulsarEffects()
        {
            _logger?.LogInformation("Init pulsar effects");
            _pulsarEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 30),
                new EveSystemEffect("Armor resist", -15),
                new EveSystemEffect("Capacitor recharge", -15),
                new EveSystemEffect("Signature", 30),
                new EveSystemEffect("NOS/Neut drain", 30)
            });

            _pulsarEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 44),
                new EveSystemEffect("Armor resist", -22),
                new EveSystemEffect("Capacitor recharge", -22),
                new EveSystemEffect("Signature", 44),
                new EveSystemEffect("NOS/Neut drain", 44)
            });

            _pulsarEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 58),
                new EveSystemEffect("Armor resist", -29),
                new EveSystemEffect("Capacitor recharge", -29),
                new EveSystemEffect("Signature", 58),
                new EveSystemEffect("NOS/Neut drain", 58)
            });

            _pulsarEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 72),
                new EveSystemEffect("Armor resist", -36),
                new EveSystemEffect("Capacitor recharge", -36),
                new EveSystemEffect("Signature", 72),
                new EveSystemEffect("NOS/Neut drain", 72)
            });

            _pulsarEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 86),
                new EveSystemEffect("Armor resist", -43),
                new EveSystemEffect("Capacitor recharge", -43),
                new EveSystemEffect("Signature", 86),
                new EveSystemEffect("NOS/Neut drain", 86)
            });

            _pulsarEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 100),
                new EveSystemEffect("Armor resist", -50),
                new EveSystemEffect("Capacitor recharge", -50),
                new EveSystemEffect("Signature", 100),
                new EveSystemEffect("NOS/Neut drain", 100)
            });

            _pulsarEffects.Add(EveSystemType.C17, new List<EveSystemEffect>(5)
            {
                new EveSystemEffect("Shield HP", 44),
                new EveSystemEffect("Armor resist", -22),
                new EveSystemEffect("Capacitor recharge", -22),
                new EveSystemEffect("Signature", 44),
                new EveSystemEffect("NOS/Neut drain", 44)
            });
        }

        private void InitWolfRayetEffects()
        {
            _logger?.LogInformation("Init wolf-rayet effects");
            _wolfRayetEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 30),
                new EveSystemEffect("Shield resist", -15),
                new EveSystemEffect("Small Weapon damage", 60),
                new EveSystemEffect("Signature size", -15)
            });

            _wolfRayetEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 44),
                new EveSystemEffect("Shield resist", -22),
                new EveSystemEffect("Small Weapon damage", 88),
                new EveSystemEffect("Signature size", -22)
            });

            _wolfRayetEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 58),
                new EveSystemEffect("Shield resist", -29),
                new EveSystemEffect("Small Weapon damage", 116),
                new EveSystemEffect("Signature size", 29)
            });

            _wolfRayetEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 72),
                new EveSystemEffect("Shield resist", -36),
                new EveSystemEffect("Small Weapon damage", 144),
                new EveSystemEffect("Signature size", 36)
            });

            _wolfRayetEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 86),
                new EveSystemEffect("Shield resist", -43),
                new EveSystemEffect("Small Weapon damage", 172),
                new EveSystemEffect("Signature size", -43)
            });

            _wolfRayetEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 100),
                new EveSystemEffect("Shield resist", -50),
                new EveSystemEffect("Small Weapon damage", 200),
                new EveSystemEffect("Signature size", -50)
            });

            _wolfRayetEffects.Add(EveSystemType.C13, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 100),
                new EveSystemEffect("Shield resist", -50),
                new EveSystemEffect("Small Weapon damage", 200),
                new EveSystemEffect("Signature size", -50)
            });

            _wolfRayetEffects.Add(EveSystemType.C18, new List<EveSystemEffect>(4)
            {
                new EveSystemEffect("Armor HP", 44),
                new EveSystemEffect("Shield resist", -22),
                new EveSystemEffect("Small Weapon damage", 88),
                new EveSystemEffect("Signature size", -22)
            });
        }

        private void InitCataclysmicEffects()
        {
            _logger?.LogInformation("Init cataclismic effects");
            _cataclysmicEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -15),
                new EveSystemEffect("Local shield boost amount", -15),
                new EveSystemEffect("Shield transfer amount", 30),
                new EveSystemEffect("Remote repair amount", 30),
                new EveSystemEffect("Capacitor capacity'", 30),
                new EveSystemEffect("Capacitor recharge time", 15),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -15)
            });

            _cataclysmicEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -22),
                new EveSystemEffect("Local shield boost amount", -22),
                new EveSystemEffect("Shield transfer amount", 44),
                new EveSystemEffect("Remote repair amount", 44),
                new EveSystemEffect("Capacitor capacity", 44),
                new EveSystemEffect("Capacitor recharge time", 22),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -22)
            });

            _cataclysmicEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -29),
                new EveSystemEffect("Local shield boost amount", -29),
                new EveSystemEffect("Shield transfer amount", 58),
                new EveSystemEffect("Remote repair amount", 58),
                new EveSystemEffect("Capacitor capacity", 58),
                new EveSystemEffect("Capacitor recharge time", 29),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -29)
            });

            _cataclysmicEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -36),
                new EveSystemEffect("Local shield boost amount", -36),
                new EveSystemEffect("Shield transfer amount", 72),
                new EveSystemEffect("Remote repair amount", 72),
                new EveSystemEffect("Capacitor capacity", 72),
                new EveSystemEffect("Capacitor recharge time", 36),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -36)
            });

            _cataclysmicEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -43),
                new EveSystemEffect("Local shield boost amount", -43),
                new EveSystemEffect("Shield transfer amount", 86),
                new EveSystemEffect("Remote repair amount", 86),
                new EveSystemEffect("Capacitor capacity", 86),
                new EveSystemEffect("Capacitor recharge time", 43),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -43)
            });

            _cataclysmicEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -50),
                new EveSystemEffect("Local shield boost amount", -50),
                new EveSystemEffect("Shield transfer amount", 100),
                new EveSystemEffect("Remote repair amount", 100),
                new EveSystemEffect("Capacitor capacity", 100),
                new EveSystemEffect("Capacitor recharge time", 50),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -50)
            });

            _cataclysmicEffects.Add(EveSystemType.C15, new List<EveSystemEffect>(7)
            {
                new EveSystemEffect("Local armor repair amount", -22),
                new EveSystemEffect("Local shield boost amount", -22),
                new EveSystemEffect("Shield transfer amount", 44),
                new EveSystemEffect("Remote repair amount", 44),
                new EveSystemEffect("Capacitor capacity", 44),
                new EveSystemEffect("Capacitor recharge time", 22),
                new EveSystemEffect("Remote Capacitor Transmitter amount", -22)
            });
        }

        private void InitBlackHoleEffects()
        {
            _logger?.LogInformation("Init blackhole effects");
            _blackHoleEffects.Add(EveSystemType.C1, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 15),
                new EveSystemEffect("Missile exp. velocity", 30),
                new EveSystemEffect("Ship velocity", 30),
                new EveSystemEffect("Stasis Webifier strength", -15),
                new EveSystemEffect("Inertia", 15),
                new EveSystemEffect("Targeting range", 30)
            });

            _blackHoleEffects.Add(EveSystemType.C2, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 22),
                new EveSystemEffect("Missile exp. velocity", 44),
                new EveSystemEffect("Ship velocity", 44),
                new EveSystemEffect("Stasis Webifier strength", -22),
                new EveSystemEffect("Inertia", 22),
                new EveSystemEffect("Targeting range", 44)
            });

            _blackHoleEffects.Add(EveSystemType.C3, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 29),
                new EveSystemEffect("Missile exp. velocity", 58),
                new EveSystemEffect("Ship velocity", 58),
                new EveSystemEffect("Stasis Webifier strength", -29),
                new EveSystemEffect("Inertia", 29),
                new EveSystemEffect("Targeting range", 58)
            });

            _blackHoleEffects.Add(EveSystemType.C4, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 36),
                new EveSystemEffect("Missile exp. velocity", 72),
                new EveSystemEffect("Ship velocity", 72),
                new EveSystemEffect("Stasis Webifier strength", -36),
                new EveSystemEffect("Inertia", 36),
                new EveSystemEffect("Targeting range", 72)
            });

            _blackHoleEffects.Add(EveSystemType.C5, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 43),
                new EveSystemEffect("Missile exp. velocity", 86),
                new EveSystemEffect("Ship velocity", 86),
                new EveSystemEffect("Stasis Webifier strength", -43),
                new EveSystemEffect("Inertia", 43),
                new EveSystemEffect("Targeting range", 86)
            });

            _blackHoleEffects.Add(EveSystemType.C6, new List<EveSystemEffect>(6)
            {
                new EveSystemEffect("Missile velocity", 50),
                new EveSystemEffect("Missile exp. velocity", 100),
                new EveSystemEffect("Ship velocity", 100),
                new EveSystemEffect("Stasis Webifier strength", -50),
                new EveSystemEffect("Inertia", 50),
                new EveSystemEffect("Targeting range", 100)
            });
        }
        #endregion


        public ReadOnlyCollection<WormholeType> WormholeTypes
        {
            get
            {
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

        public bool IsWorhmole(string systemName)
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
            if (IsWorhmole(systemName))
            {
                switch (regionName.First())
                {
                    case 'A':
                        return Task.FromResult(EveSystemType.C1);
                    case 'B':
                        return Task.FromResult(EveSystemType.C2);
                    case 'C':
                        return Task.FromResult(EveSystemType.C3);
                    case 'D':
                        return Task.FromResult(EveSystemType.C4);
                    case 'E':
                        return Task.FromResult(EveSystemType.C5);
                    case 'F':
                        return Task.FromResult(EveSystemType.C6);
                    case 'G':
                        return Task.FromResult(EveSystemType.Thera);
                    case 'H':
                        return Task.FromResult(EveSystemType.C13);
                    case 'K':
                        if (systemName == C14_NAME)
                            return Task.FromResult(EveSystemType.C14);
                        else if (systemName == C15_NAME)
                            return Task.FromResult(EveSystemType.C15);
                        else if (systemName == C16_NAME)
                            return Task.FromResult(EveSystemType.C16);
                        else if (systemName == C17_NAME)
                            return Task.FromResult(EveSystemType.C17);
                        else if (systemName == C18_NAME)
                            return Task.FromResult(EveSystemType.C18);
                        else
                            return Task.FromResult(EveSystemType.None);
                    default:
                        return Task.FromResult(EveSystemType.None);
                }
            }
            else if (regionName == REGION_POCHVVEN_NAME)//trig system
                return Task.FromResult(EveSystemType.Pochven);
            else
            {
                if (securityStatus >= 0.5)
                    return Task.FromResult(EveSystemType.HS);
                else if (securityStatus < 0.5 && securityStatus > 0)
                    return Task.FromResult(EveSystemType.LS);
                else
                    return Task.FromResult(EveSystemType.NS);
            }
        }

        private async Task<WHEffect> GetSystemEffect(string systemName)
        {
            WHEffect effect = WHEffect.None;
            if (IsWorhmole(systemName))//WH system
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

            switch (effect)
            {
                case WHEffect.Magnetar:
                    return _magnetarEffects[whClass];
                case WHEffect.BlackHole:
                    return _blackHoleEffects[whClass];
                case WHEffect.Cataclysmic:
                    return _cataclysmicEffects[whClass];
                case WHEffect.Pulsar:
                    return _pulsarEffects[whClass];
                case WHEffect.RedGiant:
                    return _redGiantEffects[whClass];
                case WHEffect.WolfRayet:
                    return _wolfRayetEffects[whClass];
                default:
                    return null;
            }
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

            var note = await _noteServices.GetBySolarSystemId(system.Id);

            if (IsWorhmole(wh.Name))//WH system
            {
                EveSystemType whClass = await GetWHClass(system_region!.Name, system_constellation.Name, system.Name, system.SecurityStatus);
                WHEffect whEffect = await GetSystemEffect(system.Name);
                IList<EveSystemEffect>? effectDetails = GetWHEffectDetails(whEffect, whClass);
                IList<WHStatic>? statics = null;

                IEnumerable<KeyValuePair<string, string>>? whStatics = await _anoikServices!.GetSystemStatics(wh.Name);
                if (whStatics != null)
                {
                    statics = whStatics.Select(x => new WHStatic(x.Key, Enum.Parse<EveSystemType>(x.Value, true))).ToList<WHStatic>();
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

            GroupEntity whGroup = await _eveMapperEntity.GetGroup(GROUPE_WORMHOLE_ID);

            await Parallel.ForEachAsync(whGroup!.Types, _options, async (whTypeId, token) =>
            {
                var whType = await _eveMapperEntity.GetWormhole(whTypeId);
                if (whType != null)
                {
                    if (_whTypes?.Where(x => x.Name == whType.Name).Count() == 0)
                    {
                        switch (whType.SystemTypeValue)
                        {
                            case 0://K162
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C1, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C2, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C3, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C4, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C5, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.C6, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.HS, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.LS, null));
                                _whTypes.Add(new WormholeType("K162", EveSystemType.NS, null));
                                break;
                            case 1:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C1, null));
                                break;
                            case 2:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C2, null));
                                break;
                            case 3:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C3, null));
                                break;
                            case 4:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C4, null));
                                break;
                            case 5:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C5, null));
                                break;
                            case 6:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C6, null));
                                break;
                            case 7:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.HS, null));
                                break;
                            case 8:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.LS, null));
                                break;
                            case 9:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.NS, null));
                                break;

                            case 10:
                            case 11:
                                //QA WH A abd QA WH B, unused WH
                                break;

                            case 12:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.Thera, null));
                                break;
                            case 13:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C13, null));
                                break;
                            case 14:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C14, null));
                                break;
                            case 15:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C15, null));
                                break;
                            case 16:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C16, null));
                                break;
                            case 17:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C17, null));
                                break;
                            case 18:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.C18, null));
                                break;
                            case 25:
                                _whTypes.Add(new WormholeType(whType.Name, EveSystemType.Pochven, null));
                                break;

                            default:
                                _logger?.LogWarning("Unknow wormhole type");
                                break;
                        }
                    }
                    else
                    {
                        _logger?.LogInformation("Always added");
                    }

                }
                else
                {
                    _logger?.LogWarning("Nullable wormhole type, value : {whTypeId}", whTypeId);
                }
            });
            _whTypes = _whTypes.OrderBy(x => x.Name).ToList<WormholeType>();

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
