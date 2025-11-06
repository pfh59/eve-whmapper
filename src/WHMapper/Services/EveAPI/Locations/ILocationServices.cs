using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;

namespace WHMapper.Services.EveAPI.Locations
{
    public interface ILocationServices
    {
        Task<Result<EveLocation>> GetLocation();
        Task<Result<Ship>> GetCurrentShip();
    }
}