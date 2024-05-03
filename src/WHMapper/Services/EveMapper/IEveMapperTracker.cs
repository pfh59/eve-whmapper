using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper;

public interface IEveMapperTracker
{   event Func<ESISolarSystem, Task> SystemChanged;
    event Func<Ship,WHMapper.Models.DTO.EveAPI.Universe.Type,Task> ShipChanged;

    Task StartTracking();
    Task StopTracking();
}
