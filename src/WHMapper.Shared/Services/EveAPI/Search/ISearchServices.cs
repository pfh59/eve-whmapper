using WHMapper.Shared.Models.DTO.EveAPI.Search;

namespace WHMapper.Shared.Services.EveAPI.Search
{
    public interface ISearchServices
    {
        Task<SearchAllianceResults?> SearchAlliance(string searchValue, bool isStrict = false);
        Task<SearchCoporationResults?> SearchCorporation(string searchValue, bool isStrict = false);
        Task<SearchCharacterResults?> SearchCharacter(string searchValue, bool isStrict = false);
    }
}
