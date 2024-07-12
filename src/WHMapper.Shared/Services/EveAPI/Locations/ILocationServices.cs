using WHMapper.Shared.Models.DTO.EveAPI.Location;

namespace WHMapper.Shared.Services.EveAPI.Locations
{
    public interface ILocationServices
    {
        Task<EveLocation?> GetLocation();
        Task<Ship?> GetCurrentShip();
    }
}
