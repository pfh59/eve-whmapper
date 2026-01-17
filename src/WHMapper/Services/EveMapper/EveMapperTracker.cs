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
    private readonly ConcurrentDictionary<int, Task[]> _trackingTasks = new();
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly SemaphoreSlim _trackingLock = new(1, 1);

    private readonly string _instanceId = Guid.NewGuid().ToString()[..8];
    private bool _disposed = false;


    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveOnlineTokenProvider tokenProvider)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _tokenProvider = tokenProvider;

        _currentLocations = new ConcurrentDictionary<int, EveLocation>();
        _currentShips = new ConcurrentDictionary<int, Ship>();
        _semaphoreSlim = new SemaphoreSlim(1,1);
        
        _logger.LogInformation("EveMapperTracker instance {InstanceId} created", _instanceId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        
        _logger.LogInformation("EveMapperTracker instance {InstanceId} disposing...", _instanceId);
        
        // Cancel all tracking tokens
        foreach (var kvp in _ctss)
        {
            try
            {
                if (kvp.Value != null && !kvp.Value.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancelling tracking for account {AccountId}", kvp.Key);
                    kvp.Value.Cancel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cancelling token for account {AccountId}", kvp.Key);
            }
        }
        
        // Wait for all tasks to complete with timeout
        var allTasks = _trackingTasks.Values.SelectMany(t => t).ToArray();
        if (allTasks.Length > 0)
        {
            _logger.LogInformation("Waiting for {TaskCount} tracking tasks to complete...", allTasks.Length);
            try
            {
                await Task.WhenAll(allTasks).WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout waiting for tracking tasks to complete");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error waiting for tracking tasks");
            }
        }
        
        // Dispose all CancellationTokenSources
        foreach (var kvp in _ctss)
        {
            try
            {
                kvp.Value?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing CTS for account {AccountId}", kvp.Key);
            }
        }
        
        _ctss.Clear();
        _trackingTasks.Clear();
        _currentLocations.Clear();
        _currentShips.Clear();
        
        // Clear all event handlers to prevent memory leaks
        SystemChanged = null;
        ShipChanged = null;
        TrackingLocationRetryRequested = null;
        TrackingShipRetryRequested = null;
        _semaphoreSlim.Dispose();
        _trackingLock.Dispose();
        
        _logger.LogInformation("EveMapperTracker instance {InstanceId} disposed", _instanceId);
    }

    public async Task StartTracking(int accountID)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot start tracking - tracker instance {InstanceId} is disposed", _instanceId);
            return;
        }
        
        await _trackingLock.WaitAsync();
        try
        {
            _logger.LogInformation("[{InstanceId}] StartTracking called for account {AccountId}", _instanceId, accountID);

            // Stop existing tracking first if any
            if (_ctss.ContainsKey(accountID))
            {
                _logger.LogInformation("[{InstanceId}] Stopping existing tracking for account {AccountId} before starting new one", _instanceId, accountID);
                await StopTrackingInternal(accountID);
            }

            // Create new CancellationTokenSource
            var cts = new CancellationTokenSource();
            if (!_ctss.TryAdd(accountID, cts))
            {
                _logger.LogError("[{InstanceId}] Failed to add CTS for account {AccountId}", _instanceId, accountID);
                cts.Dispose();
                return;
            }

            await ClearTracking(accountID);
            
            // Start tracking tasks and store references
            var locationTask = Task.Run(() => HandleTrackLocationAsync(accountID, cts.Token), cts.Token);
            var shipTask = Task.Run(() => HandleTrackShipAsync(accountID, cts.Token), cts.Token);
            
            _trackingTasks[accountID] = new[] { locationTask, shipTask };

            _logger.LogInformation("[{InstanceId}] Tracking started for account {AccountId}", _instanceId, accountID);
        }
        finally
        {
            _trackingLock.Release();
        }
    }

    public async Task StopTracking(int accountID)
    {
        if (_disposed)
        {
            return;
        }
        
        await _trackingLock.WaitAsync();
        try
        {
            await StopTrackingInternal(accountID);
        }
        finally
        {
            _trackingLock.Release();
        }
    }
    
    private async Task StopTrackingInternal(int accountID)
    {
        _logger.LogInformation("[{InstanceId}] StopTrackingInternal called for account {AccountId}", _instanceId, accountID);

        // Cancel the token
        if (_ctss.TryRemove(accountID, out var cts))
        {
            if (cts != null)
            {
                if (!cts.IsCancellationRequested)
                {
                    _logger.LogInformation("[{InstanceId}] Cancelling CTS for account {AccountId}", _instanceId, accountID);
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Already disposed
                    }
                }
                
                // Wait for tasks to complete
                if (_trackingTasks.TryRemove(accountID, out var tasks))
                {
                    try
                    {
                        _logger.LogInformation("[{InstanceId}] Waiting for tracking tasks to complete for account {AccountId}", _instanceId, accountID);
                        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(3));
                    }
                    catch (TimeoutException)
                    {
                        _logger.LogWarning("[{InstanceId}] Timeout waiting for tracking tasks for account {AccountId}", _instanceId, accountID);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[{InstanceId}] Error waiting for tasks for account {AccountId}", _instanceId, accountID);
                    }
                }
                
                try
                {
                    cts.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed
                }
            }
        }

        _logger.LogInformation("[{InstanceId}] Tracking stopped for account {AccountId}", _instanceId, accountID);
    }

    private async Task ClearTracking(int accountID)
    {
        _currentLocations.TryRemove(accountID, out _);
        _currentShips.TryRemove(accountID, out _);
        await Task.CompletedTask;
    }

    private async Task HandleTrackLocationAsync(int accountID, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{InstanceId}] HandleTrackLocationAsync started for account {AccountId}", _instanceId, accountID);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TRACK_LOCATION_HIT_IN_MS));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await timer.WaitForNextTickAsync(cancellationToken))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var userToken = await _tokenProvider.GetToken(accountID.ToString(), true);
                if (userToken == null)
                {
                    _logger.LogError("[{InstanceId}] Failed to retrieve token for account {AccountId}.", _instanceId, accountID);
                    break;
                }

                await UpdateCurrentLocation(userToken, accountID, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{InstanceId}] Error in HandleTrackLocationAsync for account {AccountId}", _instanceId, accountID);
        }
        finally
        {
            _logger.LogInformation("[{InstanceId}] HandleTrackLocationAsync ended for account {AccountId}", _instanceId, accountID);
        }
    }

    private async Task HandleTrackShipAsync(int accountID, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{InstanceId}] HandleTrackShipAsync started for account {AccountId}", _instanceId, accountID);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TRACK_SHIP_HIT_IN_MS));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await timer.WaitForNextTickAsync(cancellationToken))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var userToken = await _tokenProvider.GetToken(accountID.ToString(), true);
                if (userToken == null)
                {
                    _logger.LogError("[{InstanceId}] Failed to retrieve token for account {AccountId}.", _instanceId, accountID);
                    break;
                }

                await UpdateCurrentShip(userToken, accountID, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{InstanceId}] Error in HandleTrackShipAsync for account {AccountId}", _instanceId, accountID);
        }
        finally
        {
            _logger.LogInformation("[{InstanceId}] HandleTrackShipAsync ended for account {AccountId}", _instanceId, accountID);
        }
    }
    
    private async Task UpdateCurrentShip(UserToken token, int accountID, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        Ship? ship = null;
        Ship? oldShip = null;

        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            Result<Ship> shipResult = await _eveAPIServices.LocationServices.GetCurrentShip();
            if (shipResult.IsSuccess)
            {
                ship = shipResult.Data;
            }
            else if(shipResult.StatusCode == (int)HttpStatusCode.TooManyRequests) // Rate limited
            {
                _logger.LogWarning("Rate limited when fetching ship for account {accountID}", accountID);
                try
                {
                    var handler = TrackingShipRetryRequested;
                    if (handler != null)
                    {
                        await handler.Invoke(accountID);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{InstanceId}] Error invoking TrackingShipRetryRequested for account {AccountId}", _instanceId, accountID);
                }
                await Task.Delay(shipResult.RetryAfter ?? TimeSpan.FromSeconds(1), cancellationToken);
                return;
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _currentShips.TryGetValue(accountID, out oldShip);
        
        if (ship == null || oldShip?.ShipItemId == ship.ShipItemId) return;

        _logger.LogInformation("Ship Changed for account {AccountId}", accountID);
        
        _currentShips.AddOrUpdate(accountID, ship, (_, _) => ship);

        if (ship != null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var handler = ShipChanged;
                if (handler != null)
                {
                    await handler.Invoke(accountID, oldShip, ship);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{InstanceId}] Error invoking ShipChanged event for account {AccountId}", _instanceId, accountID);
            }
        }
    }

    private async Task UpdateCurrentLocation(UserToken token, int accountID, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        EveLocation? newLocation = null;
        EveLocation? oldLocation = null;

        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            await _eveAPIServices.SetEveCharacterAuthenticatication(token);
            Result<EveLocation> newLocationResult = await _eveAPIServices.LocationServices.GetLocation();
            if (newLocationResult.IsSuccess)
            {
                newLocation = newLocationResult.Data;
            }
            else if(newLocationResult.StatusCode == (int)HttpStatusCode.TooManyRequests) // Rate limited
            {
                _logger.LogWarning("Rate limited when fetching location for account {accountID}", accountID);
                try
                {
                    var handler = TrackingLocationRetryRequested;
                    if (handler != null)
                    {
                        await handler.Invoke(accountID);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{InstanceId}] Error invoking TrackingLocationRetryRequested for account {AccountId}", _instanceId, accountID);
                }
                await Task.Delay(newLocationResult.RetryAfter ?? TimeSpan.FromSeconds(1), cancellationToken);
                return;
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _currentLocations.TryGetValue(accountID, out oldLocation);

        if (newLocation == null || oldLocation?.SolarSystemId == newLocation.SolarSystemId) return;

        _logger.LogInformation("System Changed for account {AccountId}", accountID);
        
        _currentLocations.AddOrUpdate(accountID, newLocation, (_, _) => newLocation);

        if (newLocation != null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var handler = SystemChanged;
                if (handler != null)
                {
                    await handler.Invoke(accountID, oldLocation, newLocation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{InstanceId}] Error invoking SystemChanged event for account {AccountId}", _instanceId, accountID);
            }
        }
    }
}
