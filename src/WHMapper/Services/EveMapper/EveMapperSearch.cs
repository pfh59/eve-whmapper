using System.Collections.Concurrent;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveAPI.Corporation;
using WHMapper.Models.DTO.EveAPI.Search;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider.Services;
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
        private readonly IEveOnlineTokenProvider _tokenProvider;
        private readonly IEveMapperUserManagementService _userManagement;
        private readonly ClientUID _UID;

        public EveMapperSearch(ILogger<EveMapperSearch> logger, ISDEService sdeServices, IEveAPIServices eveAPIServices, IEveOnlineTokenProvider tokenProvider,IEveMapperUserManagementService userManagement,ClientUID UID)
        {

            _logger = logger;
            _sdeServices = sdeServices;
            _eveAPIServices = eveAPIServices;
            _tokenProvider = tokenProvider;
            _userManagement = userManagement;
            _UID = UID;
        }

        private async Task<IEnumerable<T>?> Search<T>(string value,bool strict, CancellationToken cancellationToken)
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

                    
                    var defaultAccount = await _userManagement.GetPrimaryAccountAsync(_UID.ClientId);
                    if (defaultAccount == null)
                    {
                        _logger.LogError("Search {Value}, No account authenticate to search", value);
                        return null;
                    }
                    var token = await _tokenProvider.GetToken(defaultAccount.Id.ToString());
                    if (token == null)
                    {
                        _logger.LogError("Search {Value}, No token authenticate to search", value);
                        return null;
                    }
                    await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            

                    if (typeof(T) == typeof(CharactereEntity))
                    {
                        BlockingCollection<CharactereEntity> eveEntityResults = new BlockingCollection<CharactereEntity>();
                        Result<SearchCharacterResults> characterResults = await _eveAPIServices.SearchServices.SearchCharacter(value, strict);

                        if (characterResults.IsSuccess && characterResults.Data != null && characterResults.Data.Characters != null)
                        {
                            await Parallel.ForEachAsync(characterResults.Data.Characters, options, async (characterId, token) =>
                            {
                                Result<Character> characterResult = await _eveAPIServices.CharacterServices.GetCharacter(characterId);
                                if (characterResult.IsSuccess && characterResult.Data != null)
                                    while (!eveEntityResults.TryAdd(new CharactereEntity(characterId, characterResult.Data), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }

                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                    else if (typeof(T) == typeof(CorporationEntity))
                    {
                        BlockingCollection<CorporationEntity> eveEntityResults = new BlockingCollection<CorporationEntity>();
                        Result<SearchCoporationResults> corporationResults = await _eveAPIServices.SearchServices.SearchCorporation(value,strict);
                        if (corporationResults.IsSuccess && corporationResults.Data != null && corporationResults.Data.Corporations != null)
                        {
                            await Parallel.ForEachAsync(corporationResults.Data.Corporations, options, async (corpoId, token) =>
                            {
                                Result<Corporation> corpo = await _eveAPIServices.CorporationServices.GetCorporation(corpoId);
                                if (corpo.IsSuccess && corpo.Data != null)
                                    while (!eveEntityResults.TryAdd(new CorporationEntity(corpoId, corpo.Data), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }
                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                    else if (typeof(T) == typeof(AllianceEntity))
                    {
                        BlockingCollection<AllianceEntity> eveEntityResults = new BlockingCollection<AllianceEntity>();
                        Result<SearchAllianceResults> allianceResults = await _eveAPIServices.SearchServices.SearchAlliance(value,strict);
                        if (allianceResults.IsSuccess && allianceResults.Data != null && allianceResults.Data.Alliances != null)
                        {
                            await Parallel.ForEachAsync(allianceResults.Data.Alliances, options, async (allianceId, token) =>
                            {
                                Result<Alliance> alliance = await _eveAPIServices.AllianceServices.GetAlliance(allianceId);
                                if (alliance.IsSuccess && alliance.Data != null)
                                    while (!eveEntityResults.TryAdd(new AllianceEntity(allianceId, alliance.Data), 100, token))
                                        await Task.Delay(1);

                                await Task.Yield();
                            });
                        }
                        results = (IEnumerable<T>?)eveEntityResults.OrderBy(x => x.Name);
                    }
                }
                else
                {
                    _logger.LogError("Search {Value} Unknow value", value);
                    return null;
                }

                if (results != null)
                    return results;
                else
                {
                    _logger.LogDebug("Search {Value} not found", value);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Search {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search {Value}", value);
                return null;
            }
        }
        public async Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<SDESolarSystem>(value,true, cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogInformation(oce,"SearchSystem {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchSystem {Value}", value);
                return null;
            }
        }

        public async Task<IEnumerable<CharactereEntity>?> SearchCharactere(string value, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<CharactereEntity>? results = new BlockingCollection<CharactereEntity>();
                IEnumerable<CharactereEntity>? strictCharacterEntities = await Search<CharactereEntity>(value, true, cancellationToken);
                IEnumerable<CharactereEntity>? characterEntities = await Search<CharactereEntity>(value, false, cancellationToken);

                if (strictCharacterEntities != null)
                {
                    results = results.Union(strictCharacterEntities.OrderBy(x => x.Name));
                }

                if (characterEntities != null)
                {
                    results = results.UnionBy(characterEntities.OrderBy(x => x.Name), x => x.Id);
                }

                return results;
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogInformation(oce,"SearchCharactere {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchCharactere {Value}", value);
                return null;
            }


        } 

        public async Task<IEnumerable<CharactereEntity>?> SearchCharactere(string value,bool strict, CancellationToken cancellationToken)
        {
            try
            {
                return await Search<CharactereEntity>(value,strict, cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogInformation(oce,"SearchCharactere {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchCharactere {Value}", value);
                return null;
            }
        }
        public async Task<IEnumerable<CorporationEntity>?> SearchCorporation(string value,bool strict,  CancellationToken cancellationToken)
        {
            try
            {
                return await Search<CorporationEntity>(value,strict, cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogInformation(oce,"SearchCoorporation {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchCoorporation {Value}", value);
                return null;
            }
        }
        public async Task<IEnumerable<AllianceEntity>?> SearchAlliance(string value,bool strict,  CancellationToken cancellationToken)
        {
            try
            {
                return await Search<AllianceEntity>(value,strict, cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogInformation(oce,"SearchAlliance {Value} cancelled", value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchAlliance {Value}", value);
                return null;
            }
        }
        public async Task<IEnumerable<AEveEntity>?> SearchEveEntities(string value, CancellationToken cancellationToken)
        {
            IEnumerable<AEveEntity>? results = new BlockingCollection<AEveEntity>();
            IEnumerable<AEveEntity>? results2 = new BlockingCollection<AEveEntity>();
            IEnumerable<AllianceEntity>? strictAllianceEntities = await SearchAlliance(value,true, cancellationToken);
            IEnumerable<CorporationEntity>? strictCoorporationEntities = await SearchCorporation(value,true, cancellationToken);
            IEnumerable<CharactereEntity>? strictCharacterEntities = await SearchCharactere(value,true, cancellationToken);


            if (strictAllianceEntities != null)
            {
                results = results.Union(strictAllianceEntities as IEnumerable<AEveEntity>);
            }

            if (strictCoorporationEntities != null)
            {
                results = results.Union(strictCoorporationEntities as IEnumerable<AEveEntity>);
            }

            if (strictCharacterEntities != null)
            {
                results = results.Union(strictCharacterEntities as IEnumerable<AEveEntity>);
            }

            results = results.OrderByDescending(x => x.EntityType).ThenBy(x => x.Name);



           IEnumerable<AllianceEntity>? allianceEntities = await SearchAlliance(value,false, cancellationToken);
           IEnumerable<CorporationEntity>? coorporationEntities = await SearchCorporation(value,false, cancellationToken);
           IEnumerable<CharactereEntity>? characterEntities = await SearchCharactere(value,false, cancellationToken);

            if (allianceEntities != null)
            {
                results2 = results2.Union(allianceEntities as IEnumerable<AEveEntity>);
            }

            if (coorporationEntities != null)
            {
                results2 = results2.Union(coorporationEntities as IEnumerable<AEveEntity>);
            }

            if (characterEntities != null)
            {
                results2 = results2.Union(characterEntities as IEnumerable<AEveEntity>);
            }

            results2 = results2.OrderByDescending(x => x.EntityType).ThenBy(x => x.Name);



            return results.UnionBy(results2, x => x.Id);
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
