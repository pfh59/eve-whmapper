using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.EveMapper;

/// <summary>
/// Interface for the Eve Mapper synchronization service.
/// </summary>
public interface IEveMapperRealTimeService : IAsyncDisposable
{
    /// <summary>
    /// Triggered when a user connects.
    /// </summary>
    /// <param name="user">The username of the connected user.</param>
    event Func<int, Task> UserConnected;

    /// <summary>
    /// Triggered when a user disconnects.
    /// </summary>
    /// <param name="accountID">The accountID of the disconnected user.</param>
    event Func<int, Task> UserDisconnected;

    /// <summary>
    /// Triggered when a user updates their position.
    /// </summary>
    /// <param name="accountID">The accountID of the user.</param>
    /// <param name="mapId">The ID of the map where the user is located.</param>
    /// <param name="wormholeId">The ID of the wormhole where the user is located.</param>
    event Func<int, int,int, Task> UserPosition;

/*
    /// <summary>
    /// Triggered when there is an update of users' positions.
    /// </summary>
    /// <param name="usersPosition">A dictionary containing usernames and their corresponding system names.</param>
    event Func<IDictionary<string, string>, Task> UsersPosition;
*/
    /// <summary>
    /// Triggered when a wormhole is added.
    /// </summary>
    /// <param name="accountID">The accountID of the user who added the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was added.</param>
    /// <param name="wormholeId">The ID of the added wormhole.</param>
    event Func<int, int, int, Task> WormholeAdded;

    /// <summary>
    /// Triggered when a wormhole is removed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who removed the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was removed.</param>
    /// <param name="wormholeId">The ID of the removed wormhole.</param>
    event Func<int, int, int, Task> WormholeRemoved;

    /// <summary>
    /// Triggered when a link is added.
    /// </summary>
    /// <param name="accountID">The accountID of the user who added the link.</param>
    /// <param name="mapId">The ID of the map where the link was added.</param>
    /// <param name="linkId">The ID of the added link.</param>
    event Func<int, int, int, Task> LinkAdded;

    /// <summary>
    /// Triggered when a link is removed.
    /// </summary>
    /// <param name="user">The username of the user who removed the link.</param>
    /// <param name="mapId">The ID of the map where the link was removed.</param>
    /// <param name="linkId">The ID of the removed link.</param>
    event Func<int, int, int, Task> LinkRemoved;

    /// <summary>
    /// Triggered when a wormhole is moved.
    /// </summary>
    /// <param name="accountID">The accountID of the user who moved the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was moved.</param>
    /// <param name="wormholeId">The ID of the moved wormhole.</param>
    /// <param name="posX">The new X position of the wormhole.</param>
    /// <param name="posY">The new Y position of the wormhole.</param>
    event Func<int, int, int, double, double, Task> WormholeMoved;

    /// <summary>
    /// Triggered when a link is changed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the link.</param>
    /// <param name="mapId">The ID of the map where the link was changed.</param>
    /// <param name="linkId">The ID of the changed link.</param>
    /// <param name="eol">Indicates if the link is at the end of its life.</param>
    /// <param name="size">The size of the link.</param>
    /// <param name="mass">The mass status of the link.</param>
    event Func<int, int, int, bool, SystemLinkSize, SystemLinkMassStatus, Task> LinkChanged;

    /// <summary>
    /// Triggered when a wormhole name extension is changed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the wormhole name extension.</param>
    /// <param name="mapId">The ID of the map where the wormhole name extension was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed name extension.</param>
    /// <param name="increment">Indicates if the name extension was incremented.</param>
    event Func<int, int, int, bool, Task> WormholeNameExtensionChanged;

    /// <summary>
    /// Triggered when a wormhole signature is changed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the wormhole signature.</param>
    /// <param name="mapId">The ID of the map where the wormhole signature was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed signature.</param>
    event Func<int, int, int, Task> WormholeSignaturesChanged;

    /// <summary>
    /// Triggered when a wormhole is locked or unlocked.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the lock status of the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole lock status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed lock status.</param>
    /// <param name="locked">Indicates if the wormhole is locked.</param>
    event Func<int, int, int, bool, Task> WormholeLockChanged;

    /// <summary>
    /// Triggered when a wormhole system status is changed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the wormhole system status.</param>
    /// <param name="mapId">The ID of the map where the wormhole system status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed system status.</param>
    /// <param name="systemStatus">The new system status of the wormhole.</param>
    event Func<int, int, int, WHSystemStatus, Task> WormholeSystemStatusChanged;

    /// <summary>
    /// Triggered when a map is added.
    /// </summary>
    /// <param name="accountID">The accountID of the user who added the map.</param>
    /// <param name="mapId">The ID of the added map.</param>
    event Func<int, int, Task> MapAdded;

    /// <summary>
    /// Triggered when a map is removed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who removed the map.</param>
    /// <param name="mapId">The ID of the removed map.</param>
    event Func<int, int, Task> MapRemoved;

    /// <summary>
    /// Triggered when a map name is changed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who changed the map name.</param>
    /// <param name="mapId">The ID of the map with the changed name.</param>
    event Func<int, int, string, Task> MapNameChanged;


    /// <summary>
    /// Triggered when all maps are removed.
    /// </summary>
    /// <param name="accountID">The accountID of the user who removed all maps.</param>
    event Func<int, Task> AllMapsRemoved;

    /// <summary>
    /// Triggered when accesses are added to a map.
    /// </summary>
    event Func<int, int, IEnumerable<int>, Task> MapAccessesAdded;

    /// <summary>
    /// Triggered when an access is removed from a map.
    /// </summary>
    /// <param name="accountID">The accountID of the user who removed the access.</param>
    event Func<int, int, int, Task> MapAccessRemoved;

    /// <summary>
    /// Triggered when all accesses are removed from a map.
    /// </summary>
    /// <param name="accountID">The accountID of the user who removed all accesses.</param>
    event Func<int, int, Task> MapAllAccessesRemoved;

    /// <summary>
    /// Triggered when a user connects to a map.
    /// </summary>
    /// <param name="accountID">The accountID of the user who connected to the map.</param>
    /// <param name="mapId">The ID of the map where the user connected.</param>
    event Func<int, int, Task> UserOnMapConnected;

    /// <summary>
    /// Triggered when a user disconnects from a map.
    /// </summary>
    /// <param name="accountID">The accountID of the user who disconnected from the map.</param>
    /// <param name="mapId">The ID of the map where the user disconnected.</param>
    event Func<int, int, Task> UserOnMapDisconnected;

    /// <summary>
    /// Starts the real-time service.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    Task<bool> Start(int accountID);

    /// <summary>
    /// Stops the real-time service.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    Task<bool> Stop(int accountID);

    /// <summary>
    /// Notifies the server of the user's position.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the user is located.</param>
    /// <param name="wormholeId">The ID of the wormhole where the user is located.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyUserPosition(int accountID, int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a wormhole has been added.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole was added.</param>
    /// <param name="wormholeId">The ID of the added wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormoleAdded(int accountID, int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a wormhole has been removed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole was removed.</param>
    /// <param name="wormholeId">The ID of the removed wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeRemoved(int accountID, int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a link has been added.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the link was added.</param>
    /// <param name="linkId">The ID of the added link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkAdded(int accountID, int mapId, int linkId);

    /// <summary>
    /// Notifies the server that a link has been removed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the link was removed.</param>
    /// <param name="linkId">The ID of the removed link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkRemoved(int accountID, int mapId, int linkId);

    /// <summary>
    /// Notifies the server that a wormhole has been moved.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole was moved.</param>
    /// <param name="wormholeId">The ID of the moved wormhole.</param>
    /// <param name="posX">The new X position of the wormhole.</param>
    /// <param name="posY">The new Y position of the wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeMoved(int accountID, int mapId, int wormholeId, double posX, double posY);

    /// <summary>
    /// Notifies the server that a link has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the link was changed.</param>
    /// <param name="linkId">The ID of the changed link.</param>
    /// <param name="eol">Indicates if the link is at the end of its life.</param>
    /// <param name="size">The size of the link.</param>
    /// <param name="mass">The mass status of the link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkChanged(int accountID, int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass);

    /// <summary>
    /// Notifies the server that a wormhole name extension has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole name extension was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed name extension.</param>
    /// <param name="increment">Indicates if the name extension was incremented.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeNameExtensionChanged(int accountID, int mapId, int wormholeId, bool increment);

    /// <summary>
    /// Notifies the server that a wormhole signature has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole signature was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed signature.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeSignaturesChanged(int accountID, int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a wormhole lock status has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole lock status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed lock status.</param>
    /// <param name="locked">Indicates if the wormhole is locked.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeLockChanged(int accountID, int mapId, int wormholeId, bool locked);

    /// <summary>
    /// Notifies the server that a wormhole system status has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the wormhole system status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed system status.</param>
    /// <param name="systemStatus">The new system status of the wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeSystemStatusChanged(int accountID, int mapId, int wormholeId, WHSystemStatus systemStatus);

    /// <summary>
    /// Gets the position of connected users.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of usernames and their corresponding positions.</returns>
    Task<IDictionary<int, KeyValuePair<int, int>?>> GetConnectedUsersPosition(int accountID);

    /// <summary>
    /// Notifies the server that a map has been added.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the added map.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapAdded(int accountID, int mapId);

    /// <summary>
    /// Notifies the server that a map has been removed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the removed map.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapRemoved(int accountID, int mapId);

    /// <summary>
    /// Notifies the server that a map name has been changed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map with the changed name.</param>
    /// <param name="newName">The new name of the map.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapNameChanged(int accountID, int mapId, string newName);

    /// <summary>
    /// Notifies the server that all maps have been removed.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyAllMapsRemoved(int accountID);

    /// <summary>
    /// Notifies the server that accesses have been added to a map.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where accesses were added.</param>
    /// <param name="accessIds">The IDs of the added accesses.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapAccessesAdded(int accountID, int mapId, IEnumerable<int> accessIds);

    /// <summary>
    /// Notifies the server that an access has been removed from a map.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the access was removed.</param>
    /// <param name="accessId">The ID of the removed access.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapAccessRemoved(int accountID, int mapId, int accessId);

    /// <summary>
    /// Notifies the server that all accesses have been removed from a map.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where all accesses were removed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyMapAllAccessesRemoved(int accountID, int mapId);

    /// <summary>
    /// Notifies the server that a user has connected to a map.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the user connected.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyUserOnMapConnected(int accountID, int mapId);

    /// <summary>
    /// Notifies the server that a user has disconnected from a map.
    /// </summary>
    /// <param name="accountID">The ID of the account.</param>
    /// <param name="mapId">The ID of the map where the user disconnected.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyUserOnMapDisconnected(int accountID, int mapId);

    /// <summary>
    /// Checks if the account is connected.
    /// </summary>
    /// <param name="accountID"></param>
    /// <returns></returns>
    Task<bool> IsConnected(int accountID);
}