using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public interface IUniverseServices
    {
        Task<SolarSystem> GetSystem(int system_id);
        Task<Star> GetStar(int star_id);
    }
}
