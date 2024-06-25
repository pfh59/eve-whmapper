using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper;

public interface IEveMapperTracker
{
    event Func<SystemEntity, Task> SystemChanged;
    event Func<Ship, ShipEntity, Task> ShipChanged;
    Task StartTracking();
    Task StopTracking();
}
