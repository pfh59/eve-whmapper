using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.EveMapper
{
    public interface IEveMapperSearch
    {
        public const int MIN_SEARCH_SYSTEM_CHARACTERS = 3;
        public const int MIN_SEARCH_ENTITY_CHARACTERS = 5;

        Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value, CancellationToken cancellationToken);
        Task<IEnumerable<CharactereEntity>?> SearchCharactere(string value, CancellationToken cancellationToken);
        Task<IEnumerable<CorporationEntity>?> SearchCorporation(string value, CancellationToken cancellationToken);
        Task<IEnumerable<AllianceEntity>?> SearchAlliance(string value, CancellationToken cancellationToken);
        Task<IEnumerable<AEveEntity>?> SearchEveEntities(string value, CancellationToken cancellationToken);

        IEnumerable<string> ValidateSearchType(string value);
    }
}
