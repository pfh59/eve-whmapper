using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveJwkExtensions;
using WHMapper.Services.EveMapper;
using WHMapper.Services.Metrics;

namespace WHMapper.Hubs;

[Authorize(AuthenticationSchemes = EveOnlineJwkDefaults.AuthenticationScheme)]
public class WHMapperNotificationHub(WHMapperStoreMetrics meters, IEveMapperAccessHelper accessHelper) : Hub<IWHMapperNotificationHub>
{
    private readonly static ConnectionMapping<int> _connections = new ConnectionMapping<int>();
    private readonly static ConcurrentDictionary<int, KeyValuePair<int,int>?> _connectedUserPosition = new ConcurrentDictionary<int, KeyValuePair<int,int>?>();
    private readonly static ConcurrentDictionary<int, ConnectionMapping<int>> _mapConnections = new ConcurrentDictionary<int, ConnectionMapping<int>>();

    // Per-connection grants, filled only after a server-side access check. Every scoped Send* method
    // consults them, so a client can never broadcast into a group it was not granted.
    private readonly static ConcurrentDictionary<string, ConcurrentDictionary<int, byte>> _authorizedMaps = new ConcurrentDictionary<string, ConcurrentDictionary<int, byte>>();

    // Value is 1 when the account was an instance admin at join time. Membership is captured while
    // access still exists, which is what lets revocation and deletion events reach the users concerned.
    private readonly static ConcurrentDictionary<string, ConcurrentDictionary<int, byte>> _authorizedInstances = new ConcurrentDictionary<string, ConcurrentDictionary<int, byte>>();

    private bool IsConnectionAuthorizedForMap(int mapId)
        => _authorizedMaps.TryGetValue(Context.ConnectionId, out var maps) && maps.ContainsKey(mapId);

    private bool IsConnectionAuthorizedForInstance(int instanceId)
        => _authorizedInstances.TryGetValue(Context.ConnectionId, out var instances) && instances.ContainsKey(instanceId);

    private bool IsConnectionAdminForInstance(int instanceId)
        => _authorizedInstances.TryGetValue(Context.ConnectionId, out var instances)
            && instances.TryGetValue(instanceId, out var isAdmin) && isAdmin == 1;

    /// <summary>
    /// Guard for instance-scoped broadcasts: the caller must be authenticated, must have joined the
    /// instance group through a server-side access check, and must still be an admin of it. Every
    /// method using this guard corresponds to an admin-only action in the UI.
    /// </summary>
    private async Task<int> RequireInstanceAdminAsync(int instanceId)
    {
        int accountID = CurrentAccountId();
        if (accountID == 0 || !IsConnectionAuthorizedForInstance(instanceId))
            return 0;

        return await accessHelper.IsInstanceAdminAuthorized(accountID, instanceId) ? accountID : 0;
    }

    private string CurrentUser()
    {
        if (Context != null && Context.User != null)
        {
            var nameRes = Context.User.FindFirst("name");
            if (nameRes != null)
                return nameRes.Value;
        }
        return string.Empty;
    }
    private int CurrentAccountId()
    {
        if (Context != null && !String.IsNullOrEmpty(Context.UserIdentifier))
        {
            // UserIdentifier is expected as "CHARACTER:EVE:<id>"; never index blindly into it.
            var parts = Context.UserIdentifier.Split(":");
            if (parts.Length >= 3 && int.TryParse(parts[2], out int res))
                return res;

        }
        return 0;
    }


    public override async Task OnConnectedAsync()
    {
        int accountID = CurrentAccountId();
        if (accountID == 0)
        {
            await base.OnConnectedAsync();
            return;
        }

        int countAfterAdd = _connections.AddAndGetCount(accountID, Context.ConnectionId);
        bool isFirstConnection = countAfterAdd == 1;

        if (isFirstConnection)
        {
            _connectedUserPosition.TryAdd(accountID, null);
            meters.ConnectUser();
        }

        await base.OnConnectedAsync();

        // Subscribe to accessible instances here so the client never has to send an instance id.
        foreach (var instanceId in await accessHelper.GetAccessibleInstanceIdsAsync(accountID))
        {
            await JoinInstanceGroupAsync(instanceId);
        }

        if (isFirstConnection)
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyUserConnected(accountID);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        int accountID = CurrentAccountId();
        if (accountID == 0)
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        int remaining = _connections.RemoveAndGetCount(accountID, Context.ConnectionId);
        bool wasLastConnection = remaining == 0;

        if (wasLastConnection)
        {
            _connectedUserPosition.TryRemove(accountID, out _);
            meters.DisconnectUser();
            await Clients.AllExcept(Context.ConnectionId).NotifyUserDisconnected(accountID);
        }

        foreach (var entry in _mapConnections)
        {
            var mapId = entry.Key;
            if (entry.Value.TryRemove(accountID, Context.ConnectionId, out int remainingOnMap) && remainingOnMap == 0)
            {
                await Clients.OthersInGroup($"map:{mapId}").NotifyUserOnMapDisconnected(accountID, mapId);
            }
        }

        _authorizedMaps.TryRemove(Context.ConnectionId, out _);
        _authorizedInstances.TryRemove(Context.ConnectionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinMap(int mapId)
    {
        int accountID = CurrentAccountId();
        if (accountID == 0)
            throw new HubException("Unauthorized.");

        // Never trust the client-supplied mapId on its own.
        if (!await accessHelper.IsEveMapperMapAccessAuthorized(accountID, mapId))
            throw new HubException("Access to the requested map is not authorized.");

        var maps = _authorizedMaps.GetOrAdd(Context.ConnectionId, _ => new ConcurrentDictionary<int, byte>());
        maps[mapId] = 0;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"map:{mapId}");

        // Map access implies instance access, so join the owning instance group too.
        var instanceId = await accessHelper.GetMapInstanceIdAsync(mapId);
        if (instanceId.HasValue)
            await JoinInstanceGroupAsync(instanceId.Value);
    }

    /// <summary>
    /// Explicitly subscribe to instance-scoped events. Used by clients that are not currently
    /// viewing a map (instance list, home page) and would otherwise never join a group.
    /// </summary>
    public async Task JoinInstance(int instanceId)
    {
        int accountID = CurrentAccountId();
        if (accountID == 0)
            throw new HubException("Unauthorized.");

        if (!await accessHelper.IsEveMapperInstanceAccessAuthorized(accountID, instanceId))
            throw new HubException("Access to the requested instance is not authorized.");

        await JoinInstanceGroupAsync(instanceId);
    }

    public async Task LeaveInstance(int instanceId)
    {
        if (_authorizedInstances.TryGetValue(Context.ConnectionId, out var instances))
            instances.TryRemove(instanceId, out _);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"instance:{instanceId}");
    }

    private async Task JoinInstanceGroupAsync(int instanceId)
    {
        int accountID = CurrentAccountId();
        bool isAdmin = accountID != 0 && await accessHelper.IsInstanceAdminAuthorized(accountID, instanceId);

        var instances = _authorizedInstances.GetOrAdd(Context.ConnectionId, _ => new ConcurrentDictionary<int, byte>());
        instances[instanceId] = isAdmin ? (byte)1 : (byte)0;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"instance:{instanceId}");
    }

    public async Task LeaveMap(int mapId)
    {
        if (_authorizedMaps.TryGetValue(Context.ConnectionId, out var maps))
            maps.TryRemove(mapId, out _);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"map:{mapId}");
    }

    public async Task SendUserPosition(int mapId,int wormholeId)
    {
        int accountID = CurrentAccountId();
        if (accountID == 0 || !IsConnectionAuthorizedForMap(mapId))
            return;

        _connectedUserPosition.AddOrUpdate(
            accountID,
            new KeyValuePair<int, int>(mapId, wormholeId),
            (_, _) => new KeyValuePair<int, int>(mapId, wormholeId));

        await Clients.OthersInGroup($"map:{mapId}").NotifyUserPosition(accountID, mapId, wormholeId);
    }

    public async Task SendWormholeAdded(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            meters.AddSystem();
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormoleAdded(accountID, mapId, wowrmholeId);
        }
        
    }

    public async Task SendWormholeRemoved(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            meters.DeleteSystem();
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeRemoved(accountID, mapId, wowrmholeId);
        }
    }

    public async Task SendLinkAdded(int mapId, int linkId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            meters.AddLink();
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkAdded(accountID, mapId, linkId);
        }
    }

    public async Task SendLinkRemoved(int mapId, int linkId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            meters.DeleteLink();
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkRemoved(accountID, mapId, linkId);
        }
    }

    public async Task SendWormholeMoved(int mapId, int wowrmholeId, double posX, double posY)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormoleMoved(accountID, mapId, wowrmholeId, posX, posY);
        }
    }


    public async Task SendLinkChanged(int mapId, int linkId, int eolStatus, SystemLinkSize size, int mass)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkChanged(accountID, mapId, linkId, eolStatus, size, mass);
        }
    }

    public async Task SendWormholeNameExtensionChanged(int mapId, int wowrmholeId,char? extension)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeNameExtensionChanged(accountID, mapId, wowrmholeId,extension);
        }

    }

    public async Task SendWormholeSignaturesChanged(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeSignaturesChanged(accountID, mapId, wowrmholeId);
        }
    }

    public async Task SendWormholeLockChanged(int mapId, int wormholeId, bool locked)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeLockChanged(accountID, mapId, wormholeId, locked);
        }
    }

    public async Task SendWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeSystemStatusChanged(accountID, mapId, wormholeId, systemStatus);
        }
    }
    
    public async Task SendWormholeAlternateNameChanged(int mapId, int wormholeId, string? alternateName)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeAlternateNameChanged(accountID, mapId, wormholeId, alternateName);
        }
    }

    /// <summary>
    /// Positions are restricted to the maps this connection has been authorized to join, and a
    /// snapshot is returned rather than the live global dictionary.
    /// </summary>
    public Task<IDictionary<int, KeyValuePair<int, int>?>> GetConnectedUsersPosition()
    {
        IDictionary<int, KeyValuePair<int, int>?> visible = new Dictionary<int, KeyValuePair<int, int>?>();

        if (CurrentAccountId() == 0)
            return Task.FromResult(visible);

        foreach (var entry in _connectedUserPosition)
        {
            // Only expose a character's position when the caller shares the map that character is on.
            if (entry.Value.HasValue && IsConnectionAuthorizedForMap(entry.Value.Value.Key))
                visible[entry.Key] = entry.Value;
        }

        return Task.FromResult(visible);
    }

    public async Task SendMapAdded(int instanceId, int mapId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            meters.CreateMap();
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapAdded(accountID, mapId);
        }
    }

    public async Task SendMapRemoved(int instanceId, int mapId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            meters.DeleteMap();
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapRemoved(accountID, mapId);
        }
    }

    public async Task SendMapNameChanged(int instanceId, int mapId, string newName)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapNameChanged(accountID, mapId, newName);
        }
    }

    public async Task SendAllMapsRemoved(int instanceId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyAllMapsRemoved(accountID);
        }
    }

    public async Task SendMapAccessesAdded(int instanceId, int mapId, IEnumerable<int> accessId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapAccessesAdded(accountID, mapId, accessId);
        }
    }

    public async Task SendMapAccessRemoved(int instanceId, int mapId, int accessId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapAccessRemoved(accountID, mapId, accessId);
        }
    }

    public async Task SendMapAllAccessesRemoved(int instanceId, int mapId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyMapAllAccessesRemoved(accountID, mapId);
        }
    }

    public async Task SendUserOnMapConnected(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            var mapping = _mapConnections.GetOrAdd(mapId, _ => new ConnectionMapping<int>());
            mapping.AddAndGetCount(accountID, Context.ConnectionId);
            await Clients.OthersInGroup($"map:{mapId}").NotifyUserOnMapConnected(accountID, mapId);
        }
    }

    public async Task SendUserOnMapDisconnected(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAuthorizedForMap(mapId))
        {
            if (_mapConnections.TryGetValue(mapId, out var mapping))
            {
                mapping.TryRemove(accountID, Context.ConnectionId, out _);
            }
            await Clients.OthersInGroup($"map:{mapId}").NotifyUserOnMapDisconnected(accountID, mapId);
        }
    }

    public Task<int> GetTotalConnectedUsers()
    {
        return Task.FromResult(CurrentAccountId() == 0 ? 0 : _connections.Count);
    }

    public Task<int> GetUserCountOnMap(int mapId)
    {
        if (CurrentAccountId() == 0 || !IsConnectionAuthorizedForMap(mapId))
            return Task.FromResult(0);

        return Task.FromResult(_mapConnections.TryGetValue(mapId, out var mapping) ? mapping.Count : 0);
    }

    /// <summary>
    /// An access grant has to reach a character that, by definition, was not yet a member of the
    /// instance group. The audience is therefore resolved from the freshly persisted access rules
    /// rather than from group membership.
    /// </summary>
    public async Task SendInstanceAccessAdded(int instanceId, int accessId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID == 0)
            return;

        var targets = new List<string>();
        foreach (var connectedAccountId in _connections.GetKeys())
        {
            if (connectedAccountId == accountID)
                continue;

            if (await accessHelper.IsEveMapperInstanceAccessAuthorized(connectedAccountId, instanceId))
                targets.AddRange(_connections.GetConnections(connectedAccountId));
        }

        if (targets.Count > 0)
            await Clients.Clients(targets).NotifyInstanceAccessAdded(accountID, instanceId, accessId);
    }

    public async Task SendInstanceAccessRemoved(int instanceId, int accessId)
    {
        int accountID = await RequireInstanceAdminAsync(instanceId);
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyInstanceAccessRemoved(accountID, instanceId, accessId);
        }
    }

    public async Task SendInstanceRemoved(int instanceId)
    {
        // The instance row is already gone by the time this is called, so admin rights cannot be
        // re-queried; the status recorded when the connection joined the group is used instead.
        int accountID = CurrentAccountId();
        if(accountID != 0 && IsConnectionAdminForInstance(instanceId))
        {
            await Clients.OthersInGroup($"instance:{instanceId}").NotifyInstanceRemoved(accountID, instanceId);
        }
    }

}


