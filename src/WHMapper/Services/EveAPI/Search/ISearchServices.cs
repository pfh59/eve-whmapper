using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Search;

namespace WHMapper.Services.EveAPI.Search
{
    public interface ISearchServices
    {
        Task<Result<SearchAllianceResults>> SearchAlliance(string searchValue, bool isStrict = false);
        Task<Result<SearchCoporationResults>> SearchCorporation(string searchValue, bool isStrict = false);
        Task<Result<SearchCharacterResults>> SearchCharacter(string searchValue, bool isStrict = false);
    }
}