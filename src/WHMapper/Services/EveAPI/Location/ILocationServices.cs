using WHMapper.Models.DTO.EveAPI.Location;

namespace WHMapper.Services.EveAPI.Location
{
    public interface ILocationServices
    {
        Task<EveLocation?> GetLocation();
        Task<Ship?> GetCurrentShip();
    }
}
