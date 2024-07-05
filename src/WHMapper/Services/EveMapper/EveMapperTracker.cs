using Microsoft.AspNetCore.Components.Authorization;
using System.Timers;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;

namespace WHMapper;

public class EveMapperTracker : IEveMapperTracker, IAsyncDisposable
{
    private const int TRACK_HIT_IN_MS = 1000;
    private readonly ILogger _logger;
    private readonly AuthenticationStateProvider _authState;
    private readonly IEveAPIServices? _eveAPIServices;

    private readonly IEveMapperService _eveMapperEntity;

    private System.Timers.Timer? _timer = null!;

    private EveLocation? _currentLocation = null!;
    private SystemEntity? _currentSolarSystem = null!;
    private Ship? _currentShip = null!;
    private ShipEntity _currentShiptInfos = null!;

    public event Func<SystemEntity, Task> SystemChanged = null!;
    public event Func<Ship, ShipEntity, Task> ShipChanged = null!;

    public EveMapperTracker(ILogger<EveMapperTracker> logger, AuthenticationStateProvider authState, IEveAPIServices eveAPI, IEveMapperService eveMapperEntity)
    {
        _logger = logger;
        _authState = authState;
        _eveAPIServices = eveAPI;
        _eveMapperEntity = eveMapperEntity;

        _timer = new System.Timers.Timer(TRACK_HIT_IN_MS);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer != null)
        {
            await StopTracking();
            _timer.Elapsed -= OnTimedEvent;
            _timer.Dispose();
            _currentLocation = null!;
            _currentSolarSystem = null!;
        }
    }

    public Task StartTracking()
    {
        _logger.LogInformation("Start Tracking");
        _currentLocation = null!;
        _currentSolarSystem = null!;
        _timer?.Start();

        return Task.CompletedTask;
    }

    public Task StopTracking()
    {
        _logger.LogInformation("StopTracking Tracking");
        _timer?.Stop();

        return Task.CompletedTask;
    }

    private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var state = await _authState.GetAuthenticationStateAsync();
            if (string.IsNullOrEmpty(state?.User?.Identity?.Name) || _eveAPIServices?.LocationServices == null || _eveMapperEntity == null) return;

            await UpdateCurrentShip();
            await UpdateCurrentLocation();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Track error");
        }
    }

    private async Task UpdateCurrentShip()
    {
        var ship = await _eveAPIServices.LocationServices.GetCurrentShip();
        if (ship == null || _currentShip?.ShipItemId == ship.ShipItemId) return;

        _logger.LogInformation("Ship Changed");
        _currentShip = ship;
        _currentShiptInfos = await _eveMapperEntity.GetShip(ship.ShipTypeId);

        if (_currentShiptInfos != null)
            ShipChanged?.Invoke(_currentShip, _currentShiptInfos);
    }

    private async Task UpdateCurrentLocation()
    {
        var el = await _eveAPIServices.LocationServices.GetLocation();
        if (el == null || _currentLocation?.SolarSystemId == el.SolarSystemId) return;

        _logger.LogInformation("System Changed");
        _currentLocation = el;
        _currentSolarSystem = await _eveMapperEntity.GetSystem(el.SolarSystemId);

        if (_currentSolarSystem != null)
            SystemChanged?.Invoke(_currentSolarSystem);
    }
}
