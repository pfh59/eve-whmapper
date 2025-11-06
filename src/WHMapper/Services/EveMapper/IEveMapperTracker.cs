using WHMapper.Models.DTO.EveAPI.Location;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperTracker : IAsyncDisposable
{
    event Func<int,EveLocation?,EveLocation, Task>? SystemChanged;
    event Func<int, Ship?, Ship, Task>? ShipChanged;
    event Func<int,Task>? TrackingLocationRetryRequested;
    event Func<int,Task>? TrackingShipRetryRequested;
    Task StartTracking(int accountID);
    Task StopTracking(int accountID);

}
