using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveMapper;

public class EveMapperTracker : IEveMapperTracker
{
    private const int TRACK_HIT_IN_MS = 1000;
    private readonly ILogger<EveMapperTracker> _logger;
    private readonly IEveAPIServices _eveAPIServices;
    private readonly IEveOnlineTokenProvider _tokenProvider;

    public event Func<int,EveLocation?,EveLocation, Task>? SystemChanged;
    public event Func<int,Ship?, Ship,Task>? ShipChanged;

    private readonly ConcurrentDictionary<int, EveLocation> _currentLocations;
    private readonly ConcurrentDictionary<int, Ship> _currentShips;
    private readonly ConcurrentDictionary<int, PeriodicTimer> _timers ;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _cts ;
    private readonly SemaphoreSlim _semaphoreSlim ;


    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _tokenProvider = tokenProvider;

        _currentLocations = new ConcurrentDictionary<int, EveLocation>();
        _currentShips = new ConcurrentDictionary<int, Ship>();
        _timers = new ConcurrentDictionary<int, PeriodicTimer>();
        _cts = new ConcurrentDictionary<int, CancellationTokenSource>();
        _semaphoreSlim = new SemaphoreSlim(1,1);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var cts in _cts.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
  

        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _currentLocations.Clear();
        _currentShips.Clear();
        await Task.CompletedTask;
    }

    public async Task StartTracking(int accountID)
    {
        _logger.LogInformation("Starting tracking for account {accountID}", accountID);
        await ClearTracking(accountID);
        _ = Task.Run(() => HandlerackPositionAsync(accountID));
    }

    public Task StopTracking(int accountID)
    {
        _logger.LogInformation("Stopping tracking for account {accountID}", accountID);

        if (_cts.TryGetValue(accountID, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        if(_timers.TryGetValue(accountID, out var timer))
        {
            timer.Dispose();
        }
        _cts.TryRemove(accountID, out _);
        _timers.TryRemove(accountID, out _);

        return Task.CompletedTask;
    }

    private Task ClearTracking(int accountID)
    {
        _currentLocations.TryRemove(accountID, out _);
        _currentShips.TryRemove(accountID, out _);
        return Task.CompletedTask;
    }


    private async Task HandlerackPositionAsync(int accountID)
    {
        if(_timers.ContainsKey(accountID)) 
            return;

        
        var cts = new CancellationTokenSource();
        while(!_cts.TryAdd(accountID, cts))
            await Task.Delay(1);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TRACK_HIT_IN_MS));
        while(!_timers.TryAdd(accountID, timer))
            await Task.Delay(1);

        try
        {
            while (await timer.WaitForNextTickAsync(cts.Token))
            {
                await UpdateCurrentShip(accountID);
                await UpdateCurrentLocation(accountID);
            }
        }
        catch (OperationCanceledException oce)
        {
            _logger.LogInformation(oce, "Operation canceled");
        }
        catch (ObjectDisposedException odex)
        {
            _logger.LogInformation(odex, "Object disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in timer");
        }
        finally
        {
            cts.Dispose();
            timer.Dispose();
            _cts.TryRemove(accountID, out _);
            _timers.TryRemove(accountID, out _);
        }
    }
    

    private async Task UpdateCurrentShip(int accountID)
    {    
        Ship? ship = null;

        _currentShips.TryGetValue(accountID, out var oldShip);

        var token = await _tokenProvider.GetToken(accountID.ToString(), true);
        if (token == null) throw new Exception("Token not found");

        await _semaphoreSlim.WaitAsync();
        try
        {
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            ship = await _eveAPIServices.LocationServices.GetCurrentShip();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
        
        if (ship == null  || oldShip?.ShipItemId == ship.ShipItemId) return;

        _logger.LogInformation("Ship Changed");
        if(oldShip==null)
            while(!_currentShips.TryAdd(accountID, ship))
                await Task.Delay(1);
        else
           while(! _currentShips.TryUpdate(accountID, ship, oldShip))
                await Task.Delay(1);


        if (ship != null)
        {
            if (ShipChanged != null)
            {
                _= ShipChanged.Invoke(accountID, oldShip,ship);
            }
        }

    }

    private async Task UpdateCurrentLocation(int accountID)
    {
        EveLocation? newLocation = null;

        _currentLocations.TryGetValue(accountID, out var oldLocation);

        var token = await _tokenProvider.GetToken(accountID.ToString(), true);
        if (token == null) throw new Exception("Token not found");

        await _semaphoreSlim.WaitAsync();
        try
        {
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            newLocation = await _eveAPIServices.LocationServices.GetLocation();
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        if (newLocation == null || oldLocation?.SolarSystemId == newLocation.SolarSystemId) return;

        _logger.LogInformation("System Changed");
        if(oldLocation==null)
            while(!_currentLocations.TryAdd(accountID, newLocation))
                await Task.Delay(1);
        else
            while(!_currentLocations.TryUpdate(accountID, newLocation, oldLocation))
                await Task.Delay(1);

        if (newLocation != null)
        {
            if (SystemChanged != null)
            {
                _= SystemChanged.Invoke(accountID,oldLocation, newLocation);
            }
        } 
    }

/*
    public Task RefreshAll()
    {
        //Clear all tracking
        foreach (var accountID in _timers.Keys)
        {
            ClearTracking(accountID);
        }
        return Task.CompletedTask;
    }*/

}
