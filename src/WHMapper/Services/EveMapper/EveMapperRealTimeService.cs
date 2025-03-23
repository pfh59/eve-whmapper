using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Security.Claims;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Services.LocalStorage;

namespace WHMapper.Services.EveMapper;

public class EveMapperRealTimeService : IEveMapperRealTimeService
{
    private readonly ConcurrentDictionary<int, HubConnection> _hubConnection = new ConcurrentDictionary<int, HubConnection>();
    private readonly ILogger<EveMapperRealTimeService> _logger;
    private readonly NavigationManager _navigation;
    private readonly IEveOnlineTokenProvider _tokenProvider;

    public event Func<int, Task>? UserConnected;
    public event Func<int, Task>? UserDisconnected;
    public event Func<int, int,int, Task>? UserPosition;
    public event Func<int, int, int, Task>? WormholeAdded;
    public event Func<int, int, int, Task>? WormholeRemoved;
    public event Func<int, int, int, Task>? LinkAdded;
    public event Func<int, int, int, Task>? LinkRemoved;
    public event Func<int, int, int, double, double, Task>? WormholeMoved;
    public event Func<int, int, int, bool, SystemLinkSize, SystemLinkMassStatus, Task>? LinkChanged;
    public event Func<int, int, int, bool, Task>? WormholeNameExtensionChanged;
    public event Func<int, int, int, Task>? WormholeSignaturesChanged;
    public event Func<int, int, int, bool, Task>? WormholeLockChanged;
    public event Func<int, int, int, WHSystemStatus, Task>? WormholeSystemStatusChanged;
    public event Func<int, int, Task>? MapAdded;
    public event Func<int, int, Task>? MapRemoved;
    public event Func<int, int, string, Task>? MapNameChanged;
    public event Func<int, Task>? AllMapsRemoved;
    public event Func<int, int, IEnumerable<int>, Task>? MapAccessesAdded;
    public event Func<int, int, int, Task>? MapAccessRemoved;
    public event Func<int, int, Task>? MapAllAccessesRemoved;
    public event Func<int, int, Task>? UserOnMapConnected;
    public event Func<int, int, Task>? UserOnMapDisconnected;

    public EveMapperRealTimeService(ILogger<EveMapperRealTimeService> logger, NavigationManager navigation,IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _navigation = navigation;
        _tokenProvider = tokenProvider;
    }
    public async Task<bool> Start(int accountID)
    {
        try
        {
            HubConnection? hubConnection = await GetHubConnection(accountID);
            if (hubConnection is null)
            {
                var token = await _tokenProvider.GetToken(accountID.ToString(), true);
                if (token == null) 
                    throw new Exception("Token not found");

        
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigation.ToAbsoluteUri("/whmappernotificationhub"), options =>
                    {
                        options.AccessTokenProvider = async () => token.AccessToken;
                    })
                    .WithAutomaticReconnect()
                    .Build();
    
        
                hubConnection.On<int>("NotifyUserConnected", async (accountID) => 
                {
                    if (UserConnected != null)
                    {
                        await UserConnected.Invoke(accountID);
                    }
                });
                hubConnection.On<int>("NotifyUserDisconnected", async (accountID) => 
                {
                    if (UserDisconnected != null)
                    {
                        await UserDisconnected.Invoke(accountID);
                    }
                });
                hubConnection.On<int, int,int>("NotifyUserPosition", async (accountID, mapId,wormholeId) => 
                {
                    if (UserPosition != null)
                    {
                        await UserPosition.Invoke(accountID, mapId,wormholeId);
                    }
                });
                hubConnection.On<int, int, int>("NotifyWormoleAdded", async (accountID, mapId, wormholeId) => 
                {
                    if (WormholeAdded != null)
                    {
                        await WormholeAdded.Invoke(accountID, mapId, wormholeId);
                    }
                });
                hubConnection.On<int, int, int>("NotifyWormholeRemoved", async (accountID, mapId, wormholeId) => 
                {
                    if (WormholeRemoved != null)
                    {
                        await WormholeRemoved.Invoke(accountID, mapId, wormholeId);
                    }
                });
                hubConnection.On<int, int, int>("NotifyLinkAdded", async (accountID, mapId, linkId) => 
                {
                    if (LinkAdded != null)
                    {
                        await LinkAdded.Invoke(accountID, mapId, linkId);
                    }
                });
                hubConnection.On<int, int, int>("NotifyLinkRemoved", async (accountID, mapId, linkId) => 
                {
                    if (LinkRemoved != null)
                    {
                        await LinkRemoved.Invoke(accountID, mapId, linkId);
                    }
                });
                hubConnection.On<int, int, int, double, double>("NotifyWormoleMoved", async (accountID, mapId, wormholeId, posX, posY) => 
                {
                    if (WormholeMoved != null)
                    {
                        await WormholeMoved.Invoke(accountID, mapId, wormholeId, posX, posY);
                    }
                });
                hubConnection.On<int, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", async (accountID, mapId, linkId, eol, size, mass) => 
                {
                    if (LinkChanged != null)
                    {
                        await LinkChanged.Invoke(accountID, mapId, linkId, eol, size, mass);
                    }
                });
                hubConnection.On<int, int, int, bool>("NotifyWormholeNameExtensionChanged", async (accountID, mapId, wormholeId, increment) => 
                {
                    if (WormholeNameExtensionChanged != null)
                    {
                        await WormholeNameExtensionChanged.Invoke(accountID, mapId, wormholeId, increment);
                    }
                });
                hubConnection.On<int, int, int>("NotifyWormholeSignaturesChanged", async (accountID, mapId, wormholeId) => 
                {
                    if (WormholeSignaturesChanged != null)
                    {
                        await WormholeSignaturesChanged.Invoke(accountID, mapId, wormholeId);
                    }
                });
                hubConnection.On<int, int, int, bool>("NotifyWormholeLockChanged", async (accountID, mapId, wormholeId, locked) => 
                {
                    if (WormholeLockChanged != null)
                    {
                        await WormholeLockChanged.Invoke(accountID, mapId, wormholeId, locked);
                    }
                });
                hubConnection.On<int, int, int, WHSystemStatus>("NotifyWormholeSystemStatusChanged", async (accountID, mapId, wormholeId, systemStatus) => 
                {
                    if (WormholeSystemStatusChanged != null)
                    {
                        await WormholeSystemStatusChanged.Invoke(accountID, mapId, wormholeId, systemStatus);
                    }
                });
                hubConnection.On<int, int>("NotifyMapAdded", async (accountID, mapId) => 
                {
                    if (MapAdded != null)
                    {
                        await MapAdded.Invoke(accountID, mapId);
                    }
                });
                hubConnection.On<int, int>("NotifyMapRemoved", async (accountID, mapId) => 
                {
                    if (MapRemoved != null)
                    {
                        await MapRemoved.Invoke(accountID, mapId);
                    }
                });
                hubConnection.On<int, int, string>("NotifyMapNameChanged", async (accountID, mapId, newName) => 
                {
                    if (MapNameChanged != null)
                    {
                        await MapNameChanged.Invoke(accountID, mapId, newName);
                    }
                });
                hubConnection.On<int>("NotifyAllMapsRemoved", async (accountID) => 
                {
                    if (AllMapsRemoved != null)
                    {
                        await AllMapsRemoved.Invoke(accountID);
                    }
                });
                hubConnection.On<int, int, IEnumerable<int>>("NotifyMapAccessesAdded", async (accountID, mapId, accessId) => 
                {
                    if (MapAccessesAdded != null)
                    {
                        await MapAccessesAdded.Invoke(accountID, mapId, accessId);
                    }
                });

                hubConnection.On<int, int, int>("NotifyMapAccessRemoved", async (accountID, mapId, accessId) => 
                {
                    if (MapAccessRemoved != null)
                    {
                        await MapAccessRemoved.Invoke(accountID, mapId, accessId);
                    }
                });

                hubConnection.On<int, int>("NotifyMapAllAccessesRemoved", async (accountID, mapId) => 
                {
                    if (MapAllAccessesRemoved != null)
                    {
                        await MapAllAccessesRemoved.Invoke(accountID, mapId);
                    }
                });
                hubConnection.On<int, int>("NotifyUserOnMapConnected", async (accountID, mapId) => 
                {
                    if (UserOnMapConnected != null)
                    {
                        await UserOnMapConnected.Invoke(accountID, mapId);
                    }
                });
                hubConnection.On<int, int>("NotifyUserOnMapDisconnected", async (accountID, mapId) => 
                {
                    if (UserOnMapDisconnected != null)
                    {
                        await UserOnMapDisconnected.Invoke(accountID, mapId);
                    }
                });
            

                while(!_hubConnection.TryAdd(accountID, hubConnection))
                    await Task.Delay(1);
            }

            if(hubConnection.State == HubConnectionState.Connected)
                return true;
   
            await hubConnection.StartAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start the real-time service.");
            return false;
        }
    }

    private async Task<HubConnection?> GetHubConnection(int accountID)
    {
        HubConnection? hubConnection = null;

        if (!_hubConnection.ContainsKey(accountID))
            return hubConnection;
        

        while (!_hubConnection.TryGetValue(accountID, out hubConnection))
            await Task.Delay(1);

        return hubConnection;
    }

    public async Task<bool> Stop(int accountID)
    {
        try
        {
            HubConnection? hubConnection = await GetHubConnection(accountID);
            if (hubConnection is null)
            {
                return false;
            }

            await hubConnection.StopAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop the real-time service.");
            return false;
        }
    }

    public async Task NotifyUserPosition(int accountID,int mapId,int wormholeId)
    {
        
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendUserPosition", mapId, wormholeId);
        }
    }

    public async Task NotifyWormoleAdded(int accountID,int mapId, int wormholeId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeAdded", mapId, wormholeId);
        }
    }

    public async Task NotifyWormholeRemoved(int accountID,int mapId, int wormholeId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeRemoved", mapId, wormholeId);
        }
    }

    public async Task NotifyLinkAdded(int accountID,int mapId, int linkId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendLinkAdded", mapId, linkId);
        }
    }

    public async Task NotifyLinkRemoved(int accountID,int mapId, int linkId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendLinkRemoved", mapId, linkId);
        }
    }

    public async Task NotifyWormholeMoved(int accountID,int mapId, int wormholeId, double posX, double posY)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeMoved", mapId, wormholeId, posX, posY);
        }
    }

    public async Task NotifyLinkChanged(int accountID,int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendLinkChanged", mapId, linkId, eol, size, mass);
        }
    }

    public async Task NotifyWormholeNameExtensionChanged(int accountID,int mapId, int wormholeId, bool increment)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeNameExtensionChanged", mapId, wormholeId, increment);
        }
    }

    public async Task NotifyWormholeSignaturesChanged(int accountID,int mapId, int wormholeId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
        }
    }

    public async Task NotifyWormholeLockChanged(int accountID,int mapId, int wormholeId, bool locked)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeLockChanged", mapId, wormholeId, locked);
        }
    }

    public async Task NotifyWormholeSystemStatusChanged(int accountID,int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWormholeSystemStatusChanged", mapId, wormholeId, systemStatus);
        }
    }

    public async Task<IDictionary<int, KeyValuePair<int,int>?>> GetConnectedUsersPosition(int accountID)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            return await hubConnection.InvokeAsync<IDictionary<int, KeyValuePair<int,int>?>>("GetConnectedUsersPosition");
        }

        return new Dictionary<int, KeyValuePair<int,int>?>();
    }

    public async Task NotifyMapAdded(int accountID,int mapId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapAdded", mapId);
        }
    }

    public async Task NotifyMapRemoved(int accountID,int mapId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapRemoved", mapId);
        }
    }

    public async Task NotifyMapNameChanged(int accountID,int mapId, string newName)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapNameChanged", mapId, newName);
        }
    }

    public async Task NotifyAllMapsRemoved(int accountID)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendAllMapsRemoved");
        }
    }

    public async Task NotifyMapAccessesAdded(int accountID,int mapId, IEnumerable<int> accessId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapAccessesAdded", mapId, accessId);
        }
    }

    public async Task NotifyMapAccessRemoved(int accountID,int mapId, int accessId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapAccessRemoved", mapId, accessId);
        }
    }

    public async Task NotifyMapAllAccessesRemoved(int accountID,int mapId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMapAllAccessesRemoved", mapId);
        }
    }

    public async Task NotifyUserOnMapConnected(int accountID,int mapId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendUserOnMapConnected", mapId);
        }
    }

    public async Task NotifyUserOnMapDisconnected(int accountID,int mapId)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendUserOnMapDisconnected", mapId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            foreach (var connection in _hubConnection.Values)
            {
                try
                {
                    await connection.StopAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to stop the real-time service.");
                }
            }
            _hubConnection.Clear();

            //ckear the events
            UserConnected = null;
            UserDisconnected = null;
            UserPosition = null;
            WormholeAdded = null;
            WormholeRemoved = null;
            LinkAdded = null;
            LinkRemoved = null;
            WormholeMoved = null;
            LinkChanged = null;
            WormholeNameExtensionChanged = null;
            WormholeSignaturesChanged = null;
            WormholeLockChanged = null;
            WormholeSystemStatusChanged = null;
            MapAdded = null;
            MapRemoved = null;
            MapNameChanged = null;
            AllMapsRemoved = null;
            MapAccessesAdded = null;
            MapAccessRemoved = null;
            MapAllAccessesRemoved = null;
            UserOnMapConnected = null;
            UserOnMapDisconnected = null;
        }
    }

    public async Task<bool> IsConnected(int accountID)
    {
        HubConnection? hubConnection = await GetHubConnection(accountID);
        if (hubConnection is not null)
        {
            return hubConnection.State == HubConnectionState.Connected;
        }

        return false;
    }
}
