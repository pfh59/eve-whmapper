using WHMapper.Models.DTO.EveAPI.Search;

namespace WHMapper.Services.EveAPI.Search
{
    public interface ISearchServices
    {
        Task<SearchAllianceResults?> SearchAlliance(string searchValue, bool isStrict = false);
        Task<SearchCoporationResults?> SearchCorporation(string searchValue, bool isStrict = false);
        Task<SearchCharacterResults?> SearchCharacter(string searchValue, bool isStrict = false);

    }
}

