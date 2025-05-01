using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI;
using System.Collections.Concurrent;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Models.DTO;

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

    private string test=Guid.NewGuid().ToString();


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
            if (!cts.IsCancellationRequested)
            {
                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await cts.CancelAsync();
                    }
                    catch (ObjectDisposedException odex)
                    {
                        _logger.LogWarning(odex,"CancellationTokenSource was already disposed.");
                    }
                }
            }
           
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
        _ = Task.Run(() => HandleTrackPositionAsync(accountID));
    }

    public async Task StopTracking(int accountID)
    {
        _logger.LogInformation("Stopping tracking for account {accountID}", accountID);

        if (_cts.TryGetValue(accountID, out var cts))
        {
            if (!cts.IsCancellationRequested)
            {
                try
                {
                    await cts.CancelAsync();
                }
                catch (ObjectDisposedException odex)
                {
                    _logger.LogWarning(odex,"CancellationTokenSource for account {accountID} was already disposed.", accountID);
                }
            }
            cts.Dispose();
        }

        if(_timers.TryGetValue(accountID, out var timer))
        {
            timer.Dispose();
        }
        _cts.TryRemove(accountID, out _);
        _timers.TryRemove(accountID, out _);
    }

    private Task ClearTracking(int accountID)
    {
        _currentLocations.TryRemove(accountID, out _);
        _currentShips.TryRemove(accountID, out _);
        return Task.CompletedTask;
    }


    private async Task HandleTrackPositionAsync(int accountID)
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
                await UpdateCurrentShip(accountID,cts);
                await UpdateCurrentLocation(accountID,cts);
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
            if(!cts.IsCancellationRequested)
            {
                try
                {
                    await cts.CancelAsync();
                }
                catch (ObjectDisposedException odex)
                {
                    _logger.LogWarning(odex,"CancellationTokenSource for account {accountID} was already disposed.", accountID);
                }
            }
            timer.Dispose();
            _cts.TryRemove(accountID, out _);
            _timers.TryRemove(accountID, out _);
        }
    }
    
    private async Task UpdateCurrentShip(int accountID, CancellationTokenSource? cts)
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
                if(cts != null && !cts.IsCancellationRequested)
                {
                    try
                    {
                        await cts.CancelAsync();
                    }
                    catch (ObjectDisposedException odex)
                    {
                        _logger.LogWarning(odex,"CancellationTokenSource for account {accountID} was already disposed.", accountID);
                    }
                }
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

            while(!_currentShips.TryGetValue(accountID, out oldShip))
                await Task.Delay(1);
            
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

    private async Task UpdateCurrentLocation(int accountID,CancellationTokenSource? cts)
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
                if(cts != null && !cts.IsCancellationRequested)
                {
                    try
                    {
                        await cts.CancelAsync();
                    }
                    catch (ObjectDisposedException odex)
                    {
                        _logger.LogWarning(odex,"CancellationTokenSource for account {accountID} was already disposed.", accountID);
                    }
                }
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

             while(!_currentLocations.TryGetValue(accountID, out  oldLocation))
                await Task.Delay(1);
         

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
