using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.EveMapper;

/// <summary>
/// Client side of the SignalR notification hub: one connection per EVE account, exposing incoming
/// hub notifications as events and outgoing hub calls as Notify* methods.
/// </summary>
public interface IEveMapperRealTimeService : IAsyncDisposable
{
    event Func<int, Task> UserConnected;
    event Func<int, Task> UserDisconnected;
    event Func<int, int,int, Task> UserPosition;
    event Func<int, int, int, Task> WormholeAdded;
    event Func<int, int, int, Task> WormholeRemoved;
    event Func<int, int, int, Task> LinkAdded;
    event Func<int, int, int, Task> LinkRemoved;
    event Func<int, int, int, double, double, Task> WormholeMoved;
    event Func<int, int, int, SystemLinkEolStatus, SystemLinkSize, SystemLinkMassStatus, Task> LinkChanged;
    event Func<int, int, int, char?, Task> WormholeNameExtensionChanged;
    event Func<int, int, int, Task> WormholeSignaturesChanged;
    event Func<int, int, int, bool, Task> WormholeLockChanged;
    event Func<int, int, int, WHSystemStatus, Task> WormholeSystemStatusChanged;
    event Func<int, int,int, string?, Task> WormholeAlternateNameChanged;
    event Func<int, int, Task> MapAdded;
    event Func<int, int, Task> MapRemoved;
    event Func<int, int, string, Task> MapNameChanged;
    event Func<int, Task> AllMapsRemoved;
    event Func<int, int, IEnumerable<int>, Task> MapAccessesAdded;
    event Func<int, int, int, Task> MapAccessRemoved;
    event Func<int, int, Task> MapAllAccessesRemoved;
    event Func<int, int, Task> UserOnMapConnected;
    event Func<int, int, Task> UserOnMapDisconnected;
    event Func<int, int, int, Task> InstanceAccessAdded;
    event Func<int, int, int, Task> InstanceAccessRemoved;
    event Func<int, int, Task> InstanceRemoved;

    Task<bool> Start(int accountID);
    Task<bool> Stop(int accountID);
    Task<bool> IsConnected(int accountID);

    Task NotifyUserPosition(int accountID, int mapId, int wormholeId);
    Task NotifyWormoleAdded(int accountID, int mapId, int wormholeId);
    Task NotifyWormholeRemoved(int accountID, int mapId, int wormholeId);
    Task NotifyLinkAdded(int accountID, int mapId, int linkId);
    Task NotifyLinkRemoved(int accountID, int mapId, int linkId);
    Task NotifyWormholeMoved(int accountID, int mapId, int wormholeId, double posX, double posY);
    Task NotifyLinkChanged(int accountID, int mapId, int linkId, SystemLinkEolStatus eolStatus, SystemLinkSize size, SystemLinkMassStatus mass);
    Task NotifyWormholeNameExtensionChanged(int accountID, int mapId, int wormholeId, char? extension);
    Task NotifyWormholeSignaturesChanged(int accountID, int mapId, int wormholeId);
    Task NotifyWormholeLockChanged(int accountID, int mapId, int wormholeId, bool locked);
    Task NotifyWormholeSystemStatusChanged(int accountID, int mapId, int wormholeId, WHSystemStatus systemStatus);
    Task NotifyAlternameNameChanged(int accountID, int mapId, int wormholeId, string? alternateName);
    Task NotifyUserOnMapConnected(int accountID, int mapId);
    Task NotifyUserOnMapDisconnected(int accountID, int mapId);

    // Instance-scoped notifications: the hub authorizes the caller against instanceId before
    // broadcasting, so it has to be passed explicitly even when a mapId is also present.
    Task NotifyMapAdded(int accountID, int instanceId, int mapId);
    Task NotifyMapRemoved(int accountID, int instanceId, int mapId);
    Task NotifyMapNameChanged(int accountID, int instanceId, int mapId, string newName);
    Task NotifyAllMapsRemoved(int accountID, int instanceId);
    Task NotifyMapAccessesAdded(int accountID, int instanceId, int mapId, IEnumerable<int> accessIds);
    Task NotifyMapAccessRemoved(int accountID, int instanceId, int mapId, int accessId);
    Task NotifyMapAllAccessesRemoved(int accountID, int instanceId, int mapId);
    Task NotifyInstanceAccessAdded(int accountID, int instanceId, int accessId);
    Task NotifyInstanceAccessRemoved(int accountID, int instanceId, int accessId);
    Task NotifyInstanceRemoved(int accountID, int instanceId);

    /// <summary>
    /// Joining is what grants the connection the right to send and receive map- and
    /// instance-scoped events; the hub re-checks access server-side.
    /// </summary>
    Task JoinMap(int accountID, int mapId);
    Task LeaveMap(int accountID, int mapId);
    Task JoinInstance(int accountID, int instanceId);
    Task LeaveInstance(int accountID, int instanceId);

    Task<IDictionary<int, KeyValuePair<int, int>?>> GetConnectedUsersPosition(int accountID);
    Task<int> GetTotalConnectedUsers(int accountID);
    Task<int> GetUserCountOnMap(int accountID, int mapId);
}
