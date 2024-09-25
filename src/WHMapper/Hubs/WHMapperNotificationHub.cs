﻿using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveJwkExtensions;

namespace WHMapper.Hubs;

[Authorize(AuthenticationSchemes = EveOnlineJwkDefaults.AuthenticationScheme)]
public class WHMapperNotificationHub : Hub<IWHMapperNotificationHub>
{
    private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
    private readonly static ConcurrentDictionary<string, KeyValuePair<int,int>?> _connectedUserPosition = new ConcurrentDictionary<string, KeyValuePair<int,int>?>();


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

    public override async Task OnConnectedAsync()
    {
        
        string userName = CurrentUser();
        _connections.Add(userName, Context.ConnectionId);
        
        if(!_connectedUserPosition.ContainsKey(userName))
        {
            while (!_connectedUserPosition.TryAdd(userName, null))
                    await Task.Delay(1);
        }
        await base.OnConnectedAsync();
        await Clients.AllExcept(Context.ConnectionId).NotifyUserConnected(userName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        
        string userName = CurrentUser();
        _connections.Remove(userName, Context.ConnectionId);
        
        if (_connectedUserPosition.ContainsKey(userName) && _connections.GetConnections(userName).Count() == 0)
        {
            while (!_connectedUserPosition.TryRemove(userName, out _))
                await Task.Delay(1);

            await Clients.AllExcept(Context.ConnectionId).NotifyUserDisconnected(userName);
        }


        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendUserPosition(int mapId,int wormholeId)
    {
        string userName = CurrentUser();
        if (_connectedUserPosition.ContainsKey(userName))
        {
            KeyValuePair<int,int>? res = null;
            while (!_connectedUserPosition.TryGetValue(userName, out res))
                await Task.Delay(1);

            while (!_connectedUserPosition.TryUpdate(userName, new KeyValuePair<int,int>(mapId,wormholeId), res))
                await Task.Delay(1);
        }

        await Clients.AllExcept(Context.ConnectionId).NotifyUserPosition(userName, mapId, wormholeId);
    }

    public async Task SendWormholeAdded(int mapId, int wowrmholeId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormoleAdded(userName, mapId, wowrmholeId);
        }
        
    }

    public async Task SendWormholeRemoved(int mapId, int wowrmholeId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormholeRemoved(userName, mapId, wowrmholeId);
        }
    }

    public async Task SendLinkAdded(int mapId, int linkId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyLinkAdded(userName, mapId, linkId);
        }
    }

    public async Task SendLinkRemoved(int mapId, int linkId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyLinkRemoved(userName, mapId, linkId);
        }
    }

    public async Task SendWormholeMoved(int mapId, int wowrmholeId, double posX, double posY)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormoleMoved(userName, mapId, wowrmholeId, posX, posY);
        }
    }


    public async Task SendLinkChanged(int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyLinkChanged(userName, mapId, linkId, eol, size, mass);
        }
    }

    public async Task SendWormholeNameExtensionChanged(int mapId, int wowrmholeId,bool increment)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormholeNameExtensionChanged(userName, mapId, wowrmholeId,increment);
        }

    }

    public async Task SendWormholeSignaturesChanged(int mapId, int wowrmholeId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormholeSignaturesChanged(userName, mapId, wowrmholeId);
        }
    }

    public async Task SendWormholeLockChanged(int mapId, int wormholeId, bool locked)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormholeLockChanged(userName, mapId, wormholeId, locked);
        }
    }

    public async Task SendWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyWormholeSystemStatusChanged(userName, mapId, wormholeId, systemStatus);
        }
    }

    public Task<IDictionary<string,KeyValuePair<int,int>?>> GetConnectedUsersPosition()
    {
        return Task.FromResult<IDictionary<string,KeyValuePair<int,int>?>>(_connectedUserPosition);
    }

    public async Task SendMapAdded(int mapId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapAdded(userName, mapId);
        }
    }

    public async Task SendMapRemoved(int mapId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapRemoved(userName, mapId);
        }
    }

    public async Task SendMapNameChanged(int mapId, string newName)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapNameChanged(userName, mapId, newName);
        }
    }

    public async Task SendAllMapsRemoved()
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyAllMapsRemoved(userName);
        }
    }

    public async Task SendMapAccessesAdded(int mapId, IEnumerable<int> accessId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapAccessesAdded(userName, mapId, accessId);
        }
    }

    public async Task SendMapAccessRemoved(int mapId, int accessId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapAccessRemoved(userName, mapId, accessId);
        }
    }

    public async Task SendMapAllAccessesRemoved(int mapId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.All.NotifyMapAllAccessesRemoved(userName, mapId);
        }
    }

    public async Task SendUserOnMapConnected(int mapId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyUserOnMapConnected(userName, mapId);
        }
    }

    public async Task SendUserOnMapDisconnected(int mapId)
    {
        string userName = CurrentUser();
        if (!string.IsNullOrEmpty(userName))
        {
            await Clients.AllExcept(Context.ConnectionId).NotifyUserOnMapDisconnected(userName, mapId);
        }
    }

 

}


