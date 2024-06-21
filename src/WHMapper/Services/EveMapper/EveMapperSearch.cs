using System.Collections.Concurrent;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveAPI.Corporation;
using WHMapper.Models.DTO.EveAPI.Search;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.EveAPI;
using WHMapper.Services.SDE;

namespace WHMapper.Services.EveMapper
{
    public class EveMapperSearch : IEveMapperSearch
    {
        private const string MSG_VALUE_REQUIRED = "Value is required";
        private const string MSG_CHARACTERS_REQUIRED = "Please enter 3 or more characters";
        private const string MSG_UNKNOW_VALUE = "Unknow value";

        private readonly ILogger _logger;
        private readonly ISDEService _sdeServices;
        private readonly IEveAPIServices _eveAPIServices;

        public EveMapperSearch(ILogger<EveMapperSearch> logger, ISDEService sdeServices, IEveAPIServices eveAPIServices)
        {
            _logger = logger;
            _sdeServices = sdeServices;
            _eveAPIServices = eveAPIServices;
        }

        private async Task<IEnumerable<T>?> Search<T>(string value, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Search {value}");

                IEnumerable<T>? results = null;

                if (typeof(T) == typeof(SDESolarSystem))
                {
                    if (string.IsNullOrEmpty(value) || _sdeServices == null || value.Length < IEveMapperSearch.MIN_SEARCH_SYSTEM_CHARACTERS)
                        return null;

                    results = (IEnumerable<T>?)await _sdeServices.SearchSystem(value);
                }
                else if (typeof(T) == typeof(CharactereEntity) || (typeof(T) == typeof(CorporationEntity)) || (typeof(T) == typeof(AllianceEntity)))
                {
                    if (string.IsNullOrEmpty(value) || _eveAPIServices == null || _eveAPIServices.SearchServices == null || value.Length < IEveMapperSearch.MIN_SEARCH_ENTITY_CHARACTERS)
                        return null;

                    ParallelOptions options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = (Environment.ProcessorCount == 1 ? 1 : Environment.ProcessorCount / 2),
                        CancellationToken = cancellationToken
                    };

                    if (typeof(T) == typeof(CharactereEntity))
                    {
                        BlockingCollection<CharactereEntity> eveEntityResults = new BlockingCollection<CharactereEntity>();
                        SearchCharacterResults? characterResults = await _eveAPIServices.SearchServices.SearchCharacter(value);
                        if (characterResults != null && characterResults.Characters != null)
                        {
                            await Parallel.ForEachAsync(characterResults.Characters, options, async (characterId, token) =>
                            {
                                Character? character = await _eveAPIServices.CharacterServices.GetCharacter(characterId);
                                if (character != null)
                                    while (!eveEntityResults.TryAdd(new CharactereEntity(characterId, character), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }

                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                    else if (typeof(T) == typeof(CorporationEntity))
                    {
                        BlockingCollection<CorporationEntity> eveEntityResults = new BlockingCollection<CorporationEntity>();
                        SearchCoporationResults? coporationResults = await _eveAPIServices.SearchServices.SearchCorporation(value);
                        if (coporationResults != null && coporationResults.Corporations != null)
                        {
                            await Parallel.ForEachAsync(coporationResults.Corporations, options, async (corpoId, token) =>
                            {
                                Corporation? corpo = await _eveAPIServices.CorporationServices.GetCorporation(corpoId);
                                if (corpo != null)
                                    while (!eveEntityResults.TryAdd(new CorporationEntity(corpoId, corpo), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }
                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                    else if (typeof(T) == typeof(AllianceEntity))
                    {
                        BlockingCollection<AllianceEntity> eveEntityResults = new BlockingCollection<AllianceEntity>();
                        SearchAllianceResults? allianceResults = await _eveAPIServices.SearchServices.SearchAlliance(value);
                        if (allianceResults != null && allianceResults.Alliances != null)
                        {
                            await Parallel.ForEachAsync(allianceResults.Alliances, options, async (allianceId, token) =>
                            {
                                Alliance? alliance = await _eveAPIServices.AllianceServices.GetAlliance(allianceId);
                                if (alliance != null)
                                    while (!eveEntityResults.TryAdd(new AllianceEntity(allianceId, alliance), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }
                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                }
                else
                {
                    _logger.LogError($"Search {value} Unknow value");
                    return null;
                }

                if (results != null)
                    return results;
                else
                {
                    _logger.LogDebug($"Search {value} not found");
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Search {value} cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Search {value}");
                return null;
            }
        }
        public async Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<SDESolarSystem>(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"SearchSystem {value} cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SearchSystem {value}");
                return null;
            }
        }
        public async Task<IEnumerable<CharactereEntity>?> SearchCharactere(string value, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<CharactereEntity>(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"SearchCharactere {value} cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SearchCharactere {value}");
                return null;
            }
        }
        public async Task<IEnumerable<CorporationEntity>?> SearchCorporation(string value, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<CorporationEntity>(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"SearchCoorporation {value} cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SearchCoorporation {value}");
                return null;
            }
        }
        public async Task<IEnumerable<AllianceEntity>?> SearchAlliance(string value, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<AllianceEntity>(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"SearchAlliance {value} cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SearchAlliance {value}");
                return null;
            }
        }
        public async Task<IEnumerable<AEveEntity>?> SearchEveEntities(string value, CancellationToken cancellationToken)
        {
            IEnumerable<AEveEntity>? results = new BlockingCollection<AEveEntity>();
            IEnumerable<AllianceEntity>? allianceEntities = await SearchAlliance(value, cancellationToken);
            IEnumerable<CorporationEntity>? coorporationEntities = await SearchCorporation(value, cancellationToken);
            IEnumerable<CharactereEntity>? characterEntities = await SearchCharactere(value, cancellationToken);

            if (allianceEntities != null)
            {
                results = results.Union(allianceEntities as IEnumerable<AEveEntity>);
            }

            if (coorporationEntities != null)
            {
                results = results.Union(coorporationEntities as IEnumerable<AEveEntity>);
            }

            if (characterEntities != null)
            {
                results = results.Union(characterEntities as IEnumerable<AEveEntity>);
            }

            return results.OrderByDescending(x => x.Name).ThenBy(x => x.EntityType);
        }


        public IEnumerable<string> ValidateSearchType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield return MSG_VALUE_REQUIRED;
                yield break;
            }

            if (value.Length < 3)
            {
                yield return MSG_CHARACTERS_REQUIRED;
                yield break;
            }
        }
    }
}
