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
    public event Func<string, int,int, Task>? UserPosition;
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
    public event Func<string, int, Task>? MapAdded;
    public event Func<string, int, Task>? MapRemoved;
    public event Func<string, int, string, Task>? MapNameChanged;
    public event Func<string, Task>? AllMapsRemoved;
    public event Func<string, int, IEnumerable<int>, Task>? MapAccessesAdded;
    public event Func<string, int, int, Task>? MapAccessRemoved;
    public event Func<string, int, Task>? MapAllAccessesRemoved;
    public event Func<string, int, Task>? UserOnMapConnected;
    public event Func<string, int, Task>? UserOnMapDisconnected;

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
        _hubConnection.On<string>("NotifyUserConnected", async (user) => 
        {
            if (UserConnected != null)
            {
                await UserConnected.Invoke(user);
            }
        });
        _hubConnection.On<string>("NotifyUserDisconnected", async (user) => 
        {
            if (UserDisconnected != null)
            {
                await UserDisconnected.Invoke(user);
            }
        });
        _hubConnection.On<string, int,int>("NotifyUserPosition", async (user, mapId,wormholeId) => 
        {
            if (UserPosition != null)
            {
                await UserPosition.Invoke(user, mapId,wormholeId);
            }
        });
        _hubConnection.On<string, int, int>("NotifyWormoleAdded", async (user, mapId, wormholeId) => 
        {
            if (WormholeAdded != null)
            {
                await WormholeAdded.Invoke(user, mapId, wormholeId);
            }
        });
        _hubConnection.On<string, int, int>("NotifyWormholeRemoved", async (user, mapId, wormholeId) => 
        {
            if (WormholeRemoved != null)
            {
                await WormholeRemoved.Invoke(user, mapId, wormholeId);
            }
        });
        _hubConnection.On<string, int, int>("NotifyLinkAdded", async (user, mapId, linkId) => 
        {
            if (LinkAdded != null)
            {
                await LinkAdded.Invoke(user, mapId, linkId);
            }
        });
        _hubConnection.On<string, int, int>("NotifyLinkRemoved", async (user, mapId, linkId) => 
        {
            if (LinkRemoved != null)
            {
                await LinkRemoved.Invoke(user, mapId, linkId);
            }
        });
        _hubConnection.On<string, int, int, double, double>("NotifyWormoleMoved", async (user, mapId, wormholeId, posX, posY) => 
        {
            if (WormholeMoved != null)
            {
                await WormholeMoved.Invoke(user, mapId, wormholeId, posX, posY);
            }
        });
        _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", async (user, mapId, linkId, eol, size, mass) => 
        {
            if (LinkChanged != null)
            {
                await LinkChanged.Invoke(user, mapId, linkId, eol, size, mass);
            }
        });
        _hubConnection.On<string, int, int, bool>("NotifyWormholeNameExtensionChanged", async (user, mapId, wormholeId, increment) => 
        {
            if (WormholeNameExtensionChanged != null)
            {
                await WormholeNameExtensionChanged.Invoke(user, mapId, wormholeId, increment);
            }
        });
        _hubConnection.On<string, int, int>("NotifyWormholeSignaturesChanged", async (user, mapId, wormholeId) => 
        {
            if (WormholeSignaturesChanged != null)
            {
                await WormholeSignaturesChanged.Invoke(user, mapId, wormholeId);
            }
        });
        _hubConnection.On<string, int, int, bool>("NotifyWormholeLockChanged", async (user, mapId, wormholeId, locked) => 
        {
            if (WormholeLockChanged != null)
            {
                await WormholeLockChanged.Invoke(user, mapId, wormholeId, locked);
            }
        });
        _hubConnection.On<string, int, int, WHSystemStatus>("NotifyWormholeSystemStatusChanged", async (user, mapId, wormholeId, systemStatus) => 
        {
            if (WormholeSystemStatusChanged != null)
            {
                await WormholeSystemStatusChanged.Invoke(user, mapId, wormholeId, systemStatus);
            }
        });
        _hubConnection.On<string, int>("NotifyMapAdded", async (user, mapId) => 
        {
            if (MapAdded != null)
            {
                await MapAdded.Invoke(user, mapId);
            }
        });
        _hubConnection.On<string, int>("NotifyMapRemoved", async (user, mapId) => 
        {
            if (MapRemoved != null)
            {
                await MapRemoved.Invoke(user, mapId);
            }
        });
        _hubConnection.On<string, int, string>("NotifyMapNameChanged", async (user, mapId, newName) => 
        {
            if (MapNameChanged != null)
            {
                await MapNameChanged.Invoke(user, mapId, newName);
            }
        });
        _hubConnection.On<string>("NotifyAllMapsRemoved", async (user) => 
        {
            if (AllMapsRemoved != null)
            {
                await AllMapsRemoved.Invoke(user);
            }
        });
        _hubConnection.On<string, int, IEnumerable<int>>("NotifyMapAccessesAdded", async (user, mapId, accessId) => 
        {
            if (MapAccessesAdded != null)
            {
                await MapAccessesAdded.Invoke(user, mapId, accessId);
            }
        });

        _hubConnection.On<string, int, int>("NotifyMapAccessRemoved", async (user, mapId, accessId) => 
        {
            if (MapAccessRemoved != null)
            {
                await MapAccessRemoved.Invoke(user, mapId, accessId);
            }
        });

        _hubConnection.On<string, int>("NotifyMapAllAccessesRemoved", async (user, mapId) => 
        {
            if (MapAllAccessesRemoved != null)
            {
                await MapAllAccessesRemoved.Invoke(user, mapId);
            }
        });
        _hubConnection.On<string, int>("NotifyUserOnMapConnected", async (user, mapId) => 
        {
            if (UserOnMapConnected != null)
            {
                await UserOnMapConnected.Invoke(user, mapId);
            }
        });
        _hubConnection.On<string, int>("NotifyUserOnMapDisconnected", async (user, mapId) => 
        {
            if (UserOnMapDisconnected != null)
            {
                await UserOnMapDisconnected.Invoke(user, mapId);
            }
        });
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

    public async Task NotifyUserPosition(int mapId,int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendUserPosition", mapId, wormholeId);
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

    public async Task<IDictionary<string, KeyValuePair<int,int>?>> GetConnectedUsersPosition()
    {
        if (_hubConnection is not null)
        {
            return await _hubConnection.InvokeAsync<IDictionary<string, KeyValuePair<int,int>?>>("GetConnectedUsersPosition");
        }

        return new Dictionary<string, KeyValuePair<int,int>?>();
    }

    public async Task NotifyMapAdded(int mapId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapAdded", mapId);
        }
    }

    public async Task NotifyMapRemoved(int mapId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapRemoved", mapId);
        }
    }

    public async Task NotifyMapNameChanged(int mapId, string newName)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapNameChanged", mapId, newName);
        }
    }

    public async Task NotifyAllMapsRemoved()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendAllMapsRemoved");
        }
    }

    public async Task NotifyMapAccessesAdded(int mapId, IEnumerable<int> accessId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapAccessesAdded", mapId, accessId);
        }
    }

    public async Task NotifyMapAccessRemoved(int mapId, int accessId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapAccessRemoved", mapId, accessId);
        }
    }

    public async Task NotifyMapAllAccessesRemoved(int mapId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMapAllAccessesRemoved", mapId);
        }
    }

    public async Task NotifyUserOnMapConnected(int mapId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendUserOnMapConnected", mapId);
        }
    }

    public async Task NotifyUserOnMapDisconnected(int mapId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendUserOnMapDisconnected", mapId);
        }
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
