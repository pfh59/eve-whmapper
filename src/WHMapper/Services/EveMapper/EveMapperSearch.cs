

using WHMapper.Models.DTO.SDE;
using WHMapper.Services.SDE;

namespace WHMapper.Services.EveMapper
{

public class EveMapperSearch : IEveMapperSearch
{
    private const string MSG_VALUE_REQUIRED = "Value is required";
    private const string MSG_CHARACTERS_REQUIRED = "Please enter 3 or more characters";
    private const string MSG_UNKNOW_VALUE= "Unknow value";


    private bool _searchInProgress = false;

    private ILogger _logger= null!;
    private ISDEServices _sdeServices = null!;


    public IEnumerable<SDESolarSystem>? Systems {get;private set;}=null!;

    public EveMapperSearch(ILogger<EveMapperSearch> logger,ISDEServices sdeServices)
    {
        _logger=logger;
        _sdeServices=sdeServices;
    }


    public async Task<IEnumerable<string>?> SearchSystem(string value)
    {
        _logger.LogDebug($"SearchSystem {value}");
        if (string.IsNullOrEmpty(value) || _sdeServices == null || value.Length < 3 || _searchInProgress)
            return null;

        _searchInProgress = true;

        Systems = await _sdeServices.SearchSystem(value);

        _searchInProgress = false;
        if (Systems != null)
            return Systems.Select(x => x.Name);
        else
        {
            _logger.LogDebug($"SearchSystem {value} not found");
            return null;
        }
    }


    public IEnumerable<string> ValidateSearchType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _searchInProgress = false;
                yield return MSG_VALUE_REQUIRED;
                yield break;
            }

            if (value.Length<3)
            {
                _searchInProgress = false;
                yield return MSG_CHARACTERS_REQUIRED;
                yield break;
            }

            if(Systems==null || Systems.Where(x=>x.Name.ToLower() == value.ToLower()).FirstOrDefault()==null)
            {
                _searchInProgress = false;
                yield return MSG_UNKNOW_VALUE;
                yield break;
            }

        }
}
}
