using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper;

public interface IEveMapperTracker
{    event Func<SolarSystem, Task> SystemChanged;

    Task StartTracking();
    Task StopTracking();
}
