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
    event Func<string, Task> UserConnected;

    /// <summary>
    /// Triggered when a user disconnects.
    /// </summary>
    /// <param name="user">The username of the disconnected user.</param>
    event Func<string, Task> UserDisconnected;

    /// <summary>
    /// Triggered when a user updates their position.
    /// </summary>
    /// <param name="user">The username of the user.</param>
    /// <param name="systemName">The name of the system where the user is located.</param>
    event Func<string, string, Task> UserPosition;

    /// <summary>
    /// Triggered when there is an update of users' positions.
    /// </summary>
    /// <param name="usersPosition">A dictionary containing usernames and their corresponding system names.</param>
    event Func<IDictionary<string, string>, Task> UsersPosition;

    /// <summary>
    /// Triggered when a wormhole is added.
    /// </summary>
    /// <param name="user">The username of the user who added the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was added.</param>
    /// <param name="wormholeId">The ID of the added wormhole.</param>
    event Func<string, int, int, Task> WormholeAdded;

    /// <summary>
    /// Triggered when a wormhole is removed.
    /// </summary>
    /// <param name="user">The username of the user who removed the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was removed.</param>
    /// <param name="wormholeId">The ID of the removed wormhole.</param>
    event Func<string, int, int, Task> WormholeRemoved;

    /// <summary>
    /// Triggered when a link is added.
    /// </summary>
    /// <param name="user">The username of the user who added the link.</param>
    /// <param name="mapId">The ID of the map where the link was added.</param>
    /// <param name="linkId">The ID of the added link.</param>
    event Func<string, int, int, Task> LinkAdded;

    /// <summary>
    /// Triggered when a link is removed.
    /// </summary>
    /// <param name="user">The username of the user who removed the link.</param>
    /// <param name="mapId">The ID of the map where the link was removed.</param>
    /// <param name="linkId">The ID of the removed link.</param>
    event Func<string, int, int, Task> LinkRemoved;

    /// <summary>
    /// Triggered when a wormhole is moved.
    /// </summary>
    /// <param name="user">The username of the user who moved the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole was moved.</param>
    /// <param name="wormholeId">The ID of the moved wormhole.</param>
    /// <param name="posX">The new X position of the wormhole.</param>
    /// <param name="posY">The new Y position of the wormhole.</param>
    event Func<string, int, int, double, double, Task> WormholeMoved;

    /// <summary>
    /// Triggered when a link is changed.
    /// </summary>
    /// <param name="user">The username of the user who changed the link.</param>
    /// <param name="mapId">The ID of the map where the link was changed.</param>
    /// <param name="linkId">The ID of the changed link.</param>
    /// <param name="eol">Indicates if the link is at the end of its life.</param>
    /// <param name="size">The size of the link.</param>
    /// <param name="mass">The mass status of the link.</param>
    event Func<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus, Task> LinkChanged;

    /// <summary>
    /// Triggered when a wormhole name extension is changed.
    /// </summary>
    /// <param name="user">The username of the user who changed the wormhole name extension.</param>
    /// <param name="mapId">The ID of the map where the wormhole name extension was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed name extension.</param>
    /// <param name="increment">Indicates if the name extension was incremented.</param>
    event Func<string, int, int, bool, Task> WormholeNameExtensionChanged;

    /// <summary>
    /// Triggered when a wormhole signature is changed.
    /// </summary>
    /// <param name="user">The username of the user who changed the wormhole signature.</param>
    /// <param name="mapId">The ID of the map where the wormhole signature was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed signature.</param>
    event Func<string, int, int, Task> WormholeSignaturesChanged;

    /// <summary>
    /// Triggered when a wormhole is locked or unlocked.
    /// </summary>
    /// <param name="user">The username of the user who changed the lock status of the wormhole.</param>
    /// <param name="mapId">The ID of the map where the wormhole lock status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed lock status.</param>
    /// <param name="locked">Indicates if the wormhole is locked.</param>
    event Func<string, int, int, bool, Task> WormholeLockChanged;

    /// <summary>
    /// Triggered when a wormhole system status is changed.
    /// </summary>
    /// <param name="user">The username of the user who changed the wormhole system status.</param>
    /// <param name="mapId">The ID of the map where the wormhole system status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed system status.</param>
    /// <param name="systemStatus">The new system status of the wormhole.</param>
    event Func<string, int, int, WHSystemStatus, Task> WormholeSystemStatusChanged;


    /// <summary>
    /// Gets a value indicating whether the real-time service is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Starts the real-time service.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    Task<bool> Start();

    /// <summary>
    /// Stops the real-time service.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    Task<bool> Stop();

    /// <summary>
    /// Notifies the server of the user's position.
    /// </summary>
    /// <param name="systemName">The name of the system where the user is located.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyUserPosition(string systemName);

    /// <summary>
    /// Notifies the server that a wormhole has been added.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole was added.</param>
    /// <param name="wormholeId">The ID of the added wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormoleAdded(int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a wormhole has been removed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole was removed.</param>
    /// <param name="wormholeId">The ID of the removed wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeRemoved(int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a link has been added.
    /// </summary>
    /// <param name="mapId">The ID of the map where the link was added.</param>
    /// <param name="linkId">The ID of the added link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkAdded(int mapId, int linkId);

    /// <summary>
    /// Notifies the server that a link has been removed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the link was removed.</param>
    /// <param name="linkId">The ID of the removed link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkRemoved(int mapId, int linkId);

    /// <summary>
    /// Notifies the server that a wormhole has been moved.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole was moved.</param>
    /// <param name="wormholeId">The ID of the moved wormhole.</param>
    /// <param name="posX">The new X position of the wormhole.</param>
    /// <param name="posY">The new Y position of the wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeMoved(int mapId, int wormholeId, double posX, double posY);

    /// <summary>
    /// Notifies the server that a link has been changed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the link was changed.</param>
    /// <param name="linkId">The ID of the changed link.</param>
    /// <param name="eol">Indicates if the link is at the end of its life.</param>
    /// <param name="size">The size of the link.</param>
    /// <param name="mass">The mass status of the link.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyLinkChanged(int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass);

    /// <summary>
    /// Notifies the server that a wormhole name extension has been changed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole name extension was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed name extension.</param>
    /// <param name="increment">Indicates if the name extension was incremented.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeNameExtensionChanged(int mapId, int wormholeId, bool increment);

    /// <summary>
    /// Notifies the server that a wormhole signature has been changed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole signature was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed signature.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId);

    /// <summary>
    /// Notifies the server that a wormhole lock status has been changed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole lock status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed lock status.</param>
    /// <param name="locked">Indicates if the wormhole is locked.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeLockChanged(int mapId, int wormholeId, bool locked);

    /// <summary>
    /// Notifies the server that a wormhole system status has been changed.
    /// </summary>
    /// <param name="mapId">The ID of the map where the wormhole system status was changed.</param>
    /// <param name="wormholeId">The ID of the wormhole with the changed system status.</param>
    /// <param name="systemStatus">The new system status of the wormhole.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus);
}