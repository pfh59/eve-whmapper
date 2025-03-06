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

    private readonly ConcurrentDictionary<int, EveLocation> _currentLocations = new();
    private readonly ConcurrentDictionary<int, Ship> _currentShips = new();
    private readonly ConcurrentDictionary<int, Timer> _timers = new();
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _tokenProvider = tokenProvider;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var timer in _timers.Values)
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            timer?.Dispose();
        }

        _currentLocations.Clear();
        _currentShips.Clear();
        await Task.CompletedTask;
    }

    public async Task StartTracking(int accountID)
    {
        _logger.LogInformation("Starting tracking for account {accountID}", accountID);
        await ClearTracking(accountID);

        if (_timers.TryGetValue(accountID, out var timer))
        {
            timer.Change(0, TRACK_HIT_IN_MS);
        }
        else
        {
            timer = new Timer(TrackPosition, accountID, 0, TRACK_HIT_IN_MS);
            _timers.TryAdd(accountID, timer);
        }
    }

    public Task StopTracking(int accountID)
    {
        _logger.LogInformation("Stopping tracking for account {accountID}", accountID);
        if (_timers.TryGetValue(accountID, out var timer))
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        return Task.CompletedTask;
    }

    private Task ClearTracking(int accountID)
    {
        _currentLocations.TryRemove(accountID, out _);
        _currentShips.TryRemove(accountID, out _);
        return Task.CompletedTask;
    }

    private async void TrackPosition(object? state)
    {
        if (state is not int accountID) return;

        if (_timers.TryGetValue(accountID, out var timer))
        {
            try
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                await UpdateCurrentShip(accountID);
                await UpdateCurrentLocation(accountID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Track error");
            }
            finally
            {
                timer.Change(TRACK_HIT_IN_MS, TRACK_HIT_IN_MS);
            }
        }
        else
        {
            _logger.LogWarning("Timer not found for account {accountID}", accountID);
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
        //var shipInfos = await _eveMapperEntity.GetShip(ship.ShipTypeId);
        _currentShips[accountID] = ship;

        if (ship != null)
        {
            if (ShipChanged != null)
            {
                await ShipChanged.Invoke(accountID, oldShip,ship);
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
        //var solarSystem = await _eveMapperEntity.GetSystem(newLocation.SolarSystemId);
        _currentLocations[accountID] = newLocation;

        if (newLocation != null)
        {
            if (SystemChanged != null)
            {
                await SystemChanged.Invoke(accountID,oldLocation, newLocation);
            }
        }
    }

    public Task StopAllTracking()
    {
        foreach (var timer in _timers.Values)
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        return Task.CompletedTask;
    }
}
