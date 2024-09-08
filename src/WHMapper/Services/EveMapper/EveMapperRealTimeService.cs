using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;

namespace WHMapper.Services.EveMapper;

public class EveMapperRealTimeService : IEveMapperRealTimeService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<EveMapperRealTimeService> _logger;
    private readonly TokenProvider _tokenProvider;
    private readonly NavigationManager _navigation;

    public event Func<string, Task>? UserConnected;
    public event Func<string, Task>? UserDisconnected;
    public event Func<string, string, Task>? UserPosition;
    public event Func<string, int, int, Task>? WormholeAdded;
    public event Func<string, int, int, Task>? WormholeRemoved;
    public event Func<string, int, int, Task>? LinkAdded;
    public event Func<string, int, int, Task>? LinkRemoved;
    public event Func<string, int, int, double, double, Task>? WormholeMoved;
    public event Func<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus, Task>? LinkChanged;
    public event Func<string, int, int, bool, Task>? WormholeNameExtensionChanged;
    public event Func<string, int, int, Task>? WormholeSignaturesChanged;
    public event Func<string, int, int, bool, Task>? WormholeLockChanged;
    public event Func<string, int, int, WHSystemStatus, Task>? WormholeSystemStatusChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public EveMapperRealTimeService(ILogger<EveMapperRealTimeService> logger, NavigationManager navigation, TokenProvider tokenProvider)
    {
        _logger = logger;
        _tokenProvider = tokenProvider;
        _navigation = navigation;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("/whmappernotificationhub"), options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(_tokenProvider.AccessToken);
            }).Build();

        RegisterHubEvents();
    }

    private void RegisterHubEvents()
    {
        _hubConnection.On<string>("NotifyUserConnected", async (user) => await UserConnected?.Invoke(user));
        _hubConnection.On<string>("NotifyUserDisconnected", async (user) => await UserDisconnected?.Invoke(user));
        _hubConnection.On<string, string>("NotifyUserPosition", async (user, systemName) => await UserPosition?.Invoke(user, systemName));
        _hubConnection.On<string, int, int>("NotifyWormoleAdded", async (user, mapId, wormholeId) => await WormholeAdded?.Invoke(user, mapId, wormholeId));
        _hubConnection.On<string, int, int>("NotifyWormholeRemoved", async (user, mapId, wormholeId) => await WormholeRemoved?.Invoke(user, mapId, wormholeId));
        _hubConnection.On<string, int, int>("NotifyLinkAdded", async (user, mapId, linkId) => await LinkAdded?.Invoke(user, mapId, linkId));
        _hubConnection.On<string, int, int>("NotifyLinkRemoved", async (user, mapId, linkId) => await LinkRemoved?.Invoke(user, mapId, linkId));
        _hubConnection.On<string, int, int, double, double>("NotifyWormoleMoved", async (user, mapId, wormholeId, posX, posY) => await WormholeMoved?.Invoke(user, mapId, wormholeId, posX, posY));
        _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", async (user, mapId, linkId, eol, size, mass) => await LinkChanged?.Invoke(user, mapId, linkId, eol, size, mass));
        _hubConnection.On<string, int, int, bool>("NotifyWormholeNameExtensionChanged", async (user, mapId, wormholeId, increment) => await WormholeNameExtensionChanged?.Invoke(user, mapId, wormholeId, increment));
        _hubConnection.On<string, int, int>("NotifyWormholeSignaturesChanged", async (user, mapId, wormholeId) => await WormholeSignaturesChanged?.Invoke(user, mapId, wormholeId));
        _hubConnection.On<string, int, int, bool>("NotifyWormholeLockChanged", async (user, mapId, wormholeId, locked) => await WormholeLockChanged?.Invoke(user, mapId, wormholeId, locked));
        _hubConnection.On<string, int, int, WHSystemStatus>("NotifyWormholeSystemStatusChanged", async (user, mapId, wormholeId, systemStatus) => await WormholeSystemStatusChanged?.Invoke(user, mapId, wormholeId, systemStatus));
    }

    public async Task<bool> Start()
    {
        try
        {
            await _hubConnection.StartAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start the real-time service.");
            return false;
        }
    }

    public async Task<bool> Stop()
    {
        try
        {
            await _hubConnection.StopAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop the real-time service.");
            return false;
        }
    }

    public async Task NotifyUserPosition(string systemName)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendUserPosition", systemName);
        }
    }

    public async Task NotifyWormoleAdded(int mapId, int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeAdded", mapId, wormholeId);
        }
    }

    public async Task NotifyWormholeRemoved(int mapId, int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeRemoved", mapId, wormholeId);
        }
    }

    public async Task NotifyLinkAdded(int mapId, int linkId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendLinkAdded", mapId, linkId);
        }
    }

    public async Task NotifyLinkRemoved(int mapId, int linkId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendLinkRemoved", mapId, linkId);
        }
    }

    public async Task NotifyWormholeMoved(int mapId, int wormholeId, double posX, double posY)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeMoved", mapId, wormholeId, posX, posY);
        }
    }

    public async Task NotifyLinkChanged(int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendLinkChanged", mapId, linkId, eol, size, mass);
        }
    }

    public async Task NotifyWormholeNameExtensionChanged(int mapId, int wormholeId, bool increment)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeNameExtensionChanged", mapId, wormholeId, increment);
        }
    }

    public async Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
        }
    }

    public async Task NotifyWormholeLockChanged(int mapId, int wormholeId, bool locked)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeLockChanged", mapId, wormholeId, locked);
        }
    }

    public async Task NotifyWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeSystemStatusChanged", mapId, wormholeId, systemStatus);
        }
    }

    public async Task<IDictionary<string, string>> GetConnectedUsersPosition()
    {
        if (_hubConnection is not null)
        {
            return await _hubConnection.InvokeAsync<IDictionary<string, string>>("GetConnectedUsersPosition");
        }

        return new Dictionary<string, string>();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
