using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.ResponseMessage;

namespace WHMapper.Services.EveAPI.Location
{
    public interface ILocationServices
    {
        Task<EveLocation?> GetLocation();
        Task<Ship?> GetCurrentShip();
    }
}
