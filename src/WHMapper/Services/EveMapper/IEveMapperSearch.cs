using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.EveMapper
{
    public interface IEveMapperSearch
    {
        public const int MIN_SEARCH_CHARACTERS = 3;

        IEnumerable<SDESolarSystem>? Systems {get;}

        Task<IEnumerable<string>?> SearchSystem(string value);
        IEnumerable<string> ValidateSearchType(string value);
    }
}
