using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper;

public interface IEveMapperTracker
{    event Func<ESISolarSystem, Task> SystemChanged;

    Task StartTracking();
    Task StopTracking();
}
