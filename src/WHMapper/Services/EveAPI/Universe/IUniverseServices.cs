using System.Threading.Tasks;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public interface IUniverseServices
    {

        Task<int[]?> GetSystems();
        Task<ESISolarSystem?> GetSystem(int system_id);
        Task<Star?> GetStar(int star_id);
        Task<Group?> GetGroup(int group_id);
        Task<int[]?> GetGroups();
        Task<Category?> GetCategory(int category_id);
        Task<int[]?> GetCategories();
        Task<Models.DTO.EveAPI.Universe.Type?> GetType(int type_id);
        Task<int[]?> GetTypes();
        Task<Stargate?> GetStargate(int stargate_id);
        Task<int[]?> GetContellations();
        Task<Constellation?> GetConstellation(int constellatio_id);
        Task<int[]?> GetRegions();
        Task<Region?> GetRegion(int region_id);
    }
}
