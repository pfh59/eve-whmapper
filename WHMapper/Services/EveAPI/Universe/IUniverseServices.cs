using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public interface IUniverseServices
    {
        Task<SolarSystem> GetSystem(int system_id);
        Task<Star> GetStar(int star_id);
        Task<Group> GetGroup(int group_id);
        Task<int[]> GetGroups();
        Task<Category> GetCategory(int category_id);
        Task<int[]> GetCategories();
        Task<Models.DTO.EveAPI.Universe.Type> GetType(int type_id);
        Task<int[]> GetTypes();
    }
}
