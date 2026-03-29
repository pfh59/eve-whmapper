using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveJwkExtensions;
using WHMapper.Services.Metrics;

namespace WHMapper.Hubs;

[Authorize(AuthenticationSchemes = EveOnlineJwkDefaults.AuthenticationScheme)]
public class WHMapperNotificationHub(WHMapperStoreMetrics meters) : Hub<IWHMapperNotificationHub>
{
    private readonly static ConnectionMapping<int> _connections = new ConnectionMapping<int>();
    private readonly static ConcurrentDictionary<int, KeyValuePair<int,int>?> _connectedUserPosition = new ConcurrentDictionary<int, KeyValuePair<int,int>?>();


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
            var accountID = Context.UserIdentifier.Split(":")[2];
            if (int.TryParse(accountID, out int res))
                return res;

        }
        return 0;
    }
     

    public override async Task OnConnectedAsync()
    {
        int accountID = CurrentAccountId();
        _connections.Add(accountID, Context.ConnectionId);
        
        bool isNewUser = false;
        _connectedUserPosition.GetOrAdd(accountID, _ =>
        {
            isNewUser = true;
            return null;
        });

        if (isNewUser)
        {
            meters.ConnectUser();
        }

        await base.OnConnectedAsync();
        await Clients.AllExcept(Context.ConnectionId).NotifyUserConnected(accountID);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        int accountID = CurrentAccountId();
        
        _connections.Remove(accountID, Context.ConnectionId);
        
        if (!_connections.GetConnections(accountID).Any() && _connectedUserPosition.TryRemove(accountID, out _))
        {
            meters.DisconnectUser();
            await Clients.AllExcept(Context.ConnectionId).NotifyUserDisconnected(accountID);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinMap(int mapId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"map:{mapId}");
    }

    public async Task LeaveMap(int mapId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"map:{mapId}");
    }

    public async Task SendUserPosition(int mapId,int wormholeId)
    {
        int accountID = CurrentAccountId();
        _connectedUserPosition.AddOrUpdate(
            accountID,
            new KeyValuePair<int, int>(mapId, wormholeId),
            (_, _) => new KeyValuePair<int, int>(mapId, wormholeId));

        await Clients.OthersInGroup($"map:{mapId}").NotifyUserPosition(accountID, mapId, wormholeId);
    }

    public async Task SendWormholeAdded(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.AddSystem();
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormoleAdded(accountID, mapId, wowrmholeId);
        }
        
    }

    public async Task SendWormholeRemoved(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.DeleteSystem();
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeRemoved(accountID, mapId, wowrmholeId);
        }
    }

    public async Task SendLinkAdded(int mapId, int linkId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.AddLink();
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkAdded(accountID, mapId, linkId);
        }
    }

    public async Task SendLinkRemoved(int mapId, int linkId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.DeleteLink();
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkRemoved(accountID, mapId, linkId);
        }
    }

    public async Task SendWormholeMoved(int mapId, int wowrmholeId, double posX, double posY)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormoleMoved(accountID, mapId, wowrmholeId, posX, posY);
        }
    }


    public async Task SendLinkChanged(int mapId, int linkId, int eolStatus, SystemLinkSize size, int mass)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyLinkChanged(accountID, mapId, linkId, eolStatus, size, mass);
        }
    }

    public async Task SendWormholeNameExtensionChanged(int mapId, int wowrmholeId,char? extension)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeNameExtensionChanged(accountID, mapId, wowrmholeId,extension);
        }

    }

    public async Task SendWormholeSignaturesChanged(int mapId, int wowrmholeId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeSignaturesChanged(accountID, mapId, wowrmholeId);
        }
    }

    public async Task SendWormholeLockChanged(int mapId, int wormholeId, bool locked)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeLockChanged(accountID, mapId, wormholeId, locked);
        }
    }

    public async Task SendWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeSystemStatusChanged(accountID, mapId, wormholeId, systemStatus);
        }
    }
    
    public async Task SendWormholeAlternateNameChanged(int mapId, int wormholeId, string? alternateName)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyWormholeAlternateNameChanged(accountID, mapId, wormholeId, alternateName);
        }
    }

    public Task<IDictionary<int, KeyValuePair<int, int>?>> GetConnectedUsersPosition()
    {
        return Task.FromResult<IDictionary<int, KeyValuePair<int, int>?>>(_connectedUserPosition);
    }

    public async Task SendMapAdded(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.CreateMap();
            await Clients.All.NotifyMapAdded(accountID, mapId);
        }
    }

    public async Task SendMapRemoved(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            meters.DeleteMap();
            await Clients.All.NotifyMapRemoved(accountID, mapId);
        }
    }

    public async Task SendMapNameChanged(int mapId, string newName)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyMapNameChanged(accountID, mapId, newName);
        }
    }

    public async Task SendAllMapsRemoved()
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyAllMapsRemoved(accountID);
        }
    }

    public async Task SendMapAccessesAdded(int mapId, IEnumerable<int> accessId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyMapAccessesAdded(accountID, mapId, accessId);
        }
    }

    public async Task SendMapAccessRemoved(int mapId, int accessId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyMapAccessRemoved(accountID, mapId, accessId);
        }
    }

    public async Task SendMapAllAccessesRemoved(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyMapAllAccessesRemoved(accountID, mapId);
        }
    }

    public async Task SendUserOnMapConnected(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyUserOnMapConnected(accountID, mapId);
        }
    }

    public async Task SendUserOnMapDisconnected(int mapId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.OthersInGroup($"map:{mapId}").NotifyUserOnMapDisconnected(accountID, mapId);
        }
    }

    public async Task SendInstanceAccessAdded(int instanceId, int accessId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyInstanceAccessAdded(accountID, instanceId, accessId);
        }
    }

    public async Task SendInstanceAccessRemoved(int instanceId, int accessId)
    {
        int accountID = CurrentAccountId();
        if(accountID != 0)
        {
            await Clients.All.NotifyInstanceAccessRemoved(accountID, instanceId, accessId);
        }
    }

 

}


