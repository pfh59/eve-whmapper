using WHMapper.Shared.Models.DTO.EveAPI.Location;
using WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Shared.Services.EveMapper;

public interface IEveMapperTracker
{
    event Func<SystemEntity, Task> SystemChanged;
    event Func<Ship, ShipEntity, Task> ShipChanged;
    Task StartTracking();
    Task StopTracking();
}
