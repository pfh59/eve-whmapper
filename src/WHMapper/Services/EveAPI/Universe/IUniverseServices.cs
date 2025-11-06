using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public interface IUniverseServices
    {
        Task<Result<int[]>> GetSystems();
        Task<Result<ESISolarSystem>> GetSystem(int system_id);
        Task<Result<Star>> GetStar(int star_id);
        Task<Result<Group>> GetGroup(int group_id);
        Task<Result<int[]>> GetGroups();
        Task<Result<Category>> GetCategory(int category_id);
        Task<Result<int[]>> GetCategories();
        Task<Result<Models.DTO.EveAPI.Universe.Type>> GetType(int type_id);
        Task<Result<int[]>> GetTypes();
        Task<Result<Stargate>> GetStargate(int stargate_id);
        Task<Result<int[]>> GetContellations();
        Task<Result<Constellation>> GetConstellation(int constellatio_id);
        Task<Result<int[]>> GetRegions();
        Task<Result<Region>> GetRegion(int region_id);
    }
}