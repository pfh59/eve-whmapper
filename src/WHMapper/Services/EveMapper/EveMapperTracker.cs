using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI;
using System.Collections.Concurrent;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Models.DTO;
using System.Net;


namespace WHMapper.Services.EveMapper;

public class EveMapperTracker : IEveMapperTracker
{
    #region Constant Tracking Intervals
    //With this configuration will use 1170 tokens per 15minutes for the group Location. 
    //See more about ESI rate limits here: https://developers.eveonline.com/docs/services/esi/rate-limiting/
    private const int TRACK_LOCATION_HIT_IN_MS = 2000;
    private const int TRACK_SHIP_HIT_IN_MS = 10000;
    #endregion
    private readonly ILogger<EveMapperTracker> _logger;
    private readonly IEveAPIServices _eveAPIServices;
    private readonly IEveOnlineTokenProvider _tokenProvider;

    public event Func<int,EveLocation?,EveLocation, Task>? SystemChanged;
    public event Func<int, Ship?, Ship, Task>? ShipChanged;

    public event Func<int, Task>? TrackingLocationRetryRequested;
    public event Func<int, Task>? TrackingShipRetryRequested;

    private readonly ConcurrentDictionary<int, EveLocation> _currentLocations;
    private readonly ConcurrentDictionary<int, Ship> _currentShips;
  
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _ctss = new();
    private readonly SemaphoreSlim _semaphoreSlim;

    private string test=Guid.NewGuid().ToString();


    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _tokenProvider = tokenProvider;

        _currentLocations = new ConcurrentDictionary<int, EveLocation>();
        _currentShips = new ConcurrentDictionary<int, Ship>();
        _semaphoreSlim = new SemaphoreSlim(1,1);
    }

    public async ValueTask DisposeAsync()
    {       
        foreach (var kvp in _ctss)
        {
            if (kvp.Value != null && !kvp.Value.IsCancellationRequested)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
        }
        _ctss.Clear();
        _currentLocations.Clear();
        _currentShips.Clear();

        await Task.CompletedTask;
    }

    public async Task StartTracking(int accountID)
    {
        CancellationTokenSource? cts = null;
        _logger.LogInformation("Starting tracking for account {accountID}", accountID);

        if (_ctss.ContainsKey(accountID))
        {
            while (!_ctss.TryGetValue(accountID, out cts))
                await Task.Delay(1);

            if (cts != null && cts.IsCancellationRequested)
            {
                cts.Dispose();
                while (!_ctss.TryRemove(accountID, out _))
                    await Task.Delay(1);

                cts = new CancellationTokenSource();
                while (!_ctss.TryAdd(accountID, cts))
                    await Task.Delay(1);
            }
        }
        else
        {
            cts = new CancellationTokenSource();
            while (!_ctss.TryAdd(accountID, cts))
                await Task.Delay(1);
        }


        await ClearTracking(accountID);
        _ = Task.Run(() => HandleTrackLocationAsync(cts!.Token, accountID));
        _ = Task.Run(() => HandleTrackShipAsync(cts!.Token, accountID));

        _logger.LogInformation("Tracking started for account {accountID}", accountID);
    }

    public async Task StopTracking(int accountID)
    {
        CancellationTokenSource? cts = null;
        _logger.LogInformation("Stopping tracking for account {accountID}", accountID);

        if (_ctss.ContainsKey(accountID))
        {
            while (!_ctss.TryGetValue(accountID, out cts))
                await Task.Delay(1);

            if (cts != null)
            {
                if (!cts.IsCancellationRequested)
                    cts.Cancel();

                cts.Dispose();
            }
            
            while (!_ctss.TryRemove(accountID, out _))
                 await Task.Delay(1);
            
        }

        _logger.LogInformation("Tracking stopped for account {accountID}", accountID);
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

    private async Task HandleTrackLocationAsync(CancellationToken cancellationToken, int accountID)
    {
        _logger.LogInformation("HandleTrackLocationAsync started for account {accountID}", accountID);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TRACK_LOCATION_HIT_IN_MS));

        try
        {
            while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
            {
                var userToken = await _tokenProvider.GetToken(accountID.ToString(), true);
                if (userToken == null)
                {
                    _logger.LogError("Failed to retrieve token for account {accountID}.", accountID);
                    break;
                }

                await UpdateCurrentLocation(userToken, accountID);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "HandleTrackLocationAsync operation canceled for account {accountID}", accountID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleTrackLocationAsync for account {accountID}", accountID);
        }
    } 
    private async Task HandleTrackShipAsync(CancellationToken cancellationToken, int accountID)
    {
        _logger.LogInformation("HandleTrackShipAsync started for account {accountID}", accountID);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TRACK_SHIP_HIT_IN_MS));

        try
        {
            while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
            {
                var userToken = await _tokenProvider.GetToken(accountID.ToString(), true);
                if (userToken == null)
                {
                    _logger.LogError("Failed to retrieve token for account {accountID}.", accountID);
                    break;
                }

                await UpdateCurrentShip(userToken, accountID);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "HandleTrackShipAsync operation canceled for account {accountID}", accountID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleTrackShipAsync for account {accountID}", accountID);
        }
    }
    
    private async Task UpdateCurrentShip(UserToken token,int accountID)
    {
        Ship? ship = null;
        Ship? oldShip = null;

        await _semaphoreSlim.WaitAsync();
        try
        {
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            Result<Ship> shipResult = await _eveAPIServices.LocationServices.GetCurrentShip();
            if (shipResult.IsSuccess)
            {
                ship = shipResult.Data;
            }
            else if(shipResult.StatusCode == (int)HttpStatusCode.TooManyRequests) // Rate limited
            {
                _logger.LogWarning("Rate limited when fetching ship for account {accountID}", accountID);
                _ = TrackingShipRetryRequested?.Invoke(accountID);
                await Task.Delay(shipResult.RetryAfter ?? TimeSpan.FromSeconds(1));
                return;
            }
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
          _= ShipChanged?.Invoke(accountID, oldShip,ship);
        }
    }

    private async Task UpdateCurrentLocation(UserToken token,int accountID)
    {
        EveLocation? newLocation = null;
        EveLocation? oldLocation = null;

        await _semaphoreSlim.WaitAsync();
        try
        {
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            Result<EveLocation> newLocationResult = await _eveAPIServices.LocationServices.GetLocation();
            if (newLocationResult.IsSuccess)
            {
                newLocation = newLocationResult.Data;
            }
            else if(newLocationResult.StatusCode == (int)HttpStatusCode.TooManyRequests) // Rate limited
            {
                _logger.LogWarning("Rate limited when fetching location for account {accountID}", accountID);
                _ = TrackingLocationRetryRequested?.Invoke(accountID);
                await Task.Delay(newLocationResult.RetryAfter ?? TimeSpan.FromSeconds(1));
                return;
            }
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
            _= SystemChanged?.Invoke(accountID,oldLocation, newLocation);
        }
    }
}
