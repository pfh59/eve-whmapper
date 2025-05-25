using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI;
using System.Collections.Concurrent;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Models.DTO;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

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
    private readonly ConcurrentDictionary<int, Timer> _timers ;
    private readonly SemaphoreSlim _semaphoreSlim ;

    private string test=Guid.NewGuid().ToString();


    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _tokenProvider = tokenProvider;

        _currentLocations = new ConcurrentDictionary<int, EveLocation>();
        _currentShips = new ConcurrentDictionary<int, Ship>();
        _timers = new ConcurrentDictionary<int, Timer>();
        _semaphoreSlim = new SemaphoreSlim(1,1);
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing EveMapperTracker");
        foreach (var timer in _timers.Values)
        {
            try
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                await timer.DisposeAsync();
            }
            catch (ObjectDisposedException odex)
            {
                _logger.LogWarning(odex, "Timer was already disposed.");
            }
        }

        _timers.Clear();
        _currentLocations.Clear();
        _currentShips.Clear();

        await Task.CompletedTask;
    }

    public async Task StartTracking(int accountID)
    {
        Timer? accountTimer;

        _logger.LogInformation("Starting tracking for account {accountID}", accountID);
        await ClearTracking(accountID);


        if (!_timers.ContainsKey(accountID))
        {
            accountTimer = new Timer(HandleTrackPositionAsync, accountID, Timeout.Infinite, Timeout.Infinite);
            while (!_timers.TryAdd(accountID, accountTimer))
                await Task.Delay(1);

        }
        _timers[accountID].Change(TRACK_HIT_IN_MS, TRACK_HIT_IN_MS);

    }

    public async Task StopTracking(int accountID)
    {
        _logger.LogInformation("Stopping tracking for account {accountID}", accountID);

        if (_timers.TryGetValue(accountID, out var accountTimer))
        {
            if (!accountTimer.Change(Timeout.Infinite, Timeout.Infinite))
            {
                return;
            }
            while (!_timers.TryRemove(accountID, out _))
                await Task.Delay(1);

            await accountTimer.DisposeAsync();
        }
    }

    private Task ClearTracking(int accountID)
    {
        if(_currentLocations.ContainsKey(accountID))
        {
            while (!_currentLocations.TryRemove(accountID, out _))
                Task.Delay(1);
        }

        if(_currentShips.ContainsKey(accountID))
        {
            while (!_currentShips.TryRemove(accountID, out _))
                Task.Delay(1);
        }

        return Task.CompletedTask;
    }


    private async void HandleTrackPositionAsync(object? state)
    {
        int accountID = 0;
        try
        {
            accountID = (int)state!;
            if (_timers.TryGetValue(accountID, out var timer))
            {
                if (!timer.Change(Timeout.Infinite, Timeout.Infinite))
                {
                    _logger.LogWarning("Timer was already disposed.");
                    return;
                }

                await UpdateCurrentShip(accountID);
                await UpdateCurrentLocation(accountID);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Track error");
        }
        finally
        {
            if (_timers.TryGetValue(accountID, out var timer))
                timer.Change(TRACK_HIT_IN_MS, TRACK_HIT_IN_MS);
        }
    }

    
    private async Task UpdateCurrentShip(int accountID)
    {    
        UserToken? token = null;
        Ship? ship = null;
        Ship? oldShip = null;

    
        try
        {
            token = await _tokenProvider.GetToken(accountID.ToString(), true);
            if (token == null)
            {
                _logger.LogError("Failed to retrieve token for account {accountID}.", accountID);
                return; // Exit early if the token is not found
            }

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

            if(_currentShips.ContainsKey(accountID))
            {
                while(!_currentShips.TryGetValue(accountID, out oldShip))
                    await Task.Delay(1);
            }
            else
            {
                oldShip = null;
            }
            
            if (ship == null  || oldShip?.ShipItemId == ship.ShipItemId) return;

            _logger.LogInformation("Ship Changed");
            if(oldShip!=null)
            {
                while(!_currentShips.TryRemove(accountID, out _))
                    await Task.Delay(1);
            }

            while(! _currentShips.TryAdd(accountID, ship))
                await Task.Delay(1);


            if (ship != null)
            {
                if (ShipChanged != null)
                {
                    _= ShipChanged.Invoke(accountID, oldShip,ship);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current ship for account {accountID}", accountID);
        }

    }

    private async Task UpdateCurrentLocation(int accountID)
    {
        EveLocation? newLocation = null;
        EveLocation? oldLocation = null;
        UserToken? token = null;

        try
        {
            token = await _tokenProvider.GetToken(accountID.ToString(), true);
            if (token == null)
            {
                _logger.LogError("Failed to retrieve token for account {accountID}.", accountID);
                return; // Exit early if the token is not found
            }

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


            if(_currentLocations.ContainsKey(accountID))
            {
                while(!_currentLocations.TryGetValue(accountID, out oldLocation))
                    await Task.Delay(1);
            }
            else
            {
                oldLocation = null;
            }         

            if (newLocation == null || oldLocation?.SolarSystemId == newLocation.SolarSystemId) return;

            _logger.LogInformation("System Changed");
            if(oldLocation!=null)
            {
                while(!_currentLocations.TryRemove(accountID, out _))
                    await Task.Delay(1);
            }
        
            while(!_currentLocations.TryAdd(accountID, newLocation))
                await Task.Delay(1);

            if (newLocation != null)
            {
                if (SystemChanged != null)
                {
                    _= SystemChanged.Invoke(accountID,oldLocation, newLocation);
                }
            } 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current location for account {accountID}", accountID);
        }
    }
}
