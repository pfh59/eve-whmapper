using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using System;

namespace WHMapper.Services.EveMapper;

public class EveMapperTracker : IEveMapperTracker, IAsyncDisposable
{
    private const int TRACK_HIT_IN_MS = 1000;
    private readonly ILogger _logger;
    private readonly IEveAPIServices? _eveAPIServices;

    private readonly IEveMapperService _eveMapperEntity;

    private Timer? _timer = null!;

    private EveLocation? _currentLocation = null!;
    private SystemEntity? _currentSolarSystem = null!;
    private Ship? _currentShip = null!;
    private ShipEntity _currentShiptInfos = null!;

    public event Func<SystemEntity, Task> SystemChanged = null!;
    public event Func<Ship, ShipEntity, Task> ShipChanged = null!;

    public EveMapperTracker(ILogger<EveMapperTracker> logger, IEveAPIServices eveAPI, IEveMapperService eveMapperEntity)
    {
        _logger = logger;
        _eveAPIServices = eveAPI;
        _eveMapperEntity = eveMapperEntity;

        _timer = new Timer(TrackPosition, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer != null)
        {
            await StopTracking();
            _currentLocation = null!;
            _currentSolarSystem = null!;
        }
    }

    public Task StartTracking()
    {
        _logger.LogInformation("Start Tracking");
        _currentLocation = null!;
        _currentSolarSystem = null!;
        _timer?.Change(0, TRACK_HIT_IN_MS);

        return Task.CompletedTask;
    }

    public Task StopTracking()
    {
        _logger.LogInformation("StopTracking Tracking");
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        return Task.CompletedTask;
    }

    private async void TrackPosition(object? state)
    {
        try
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            await UpdateCurrentShip();
            await UpdateCurrentLocation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Track error");
        }
        finally
        {
            _timer?.Change(TRACK_HIT_IN_MS, TRACK_HIT_IN_MS);
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
