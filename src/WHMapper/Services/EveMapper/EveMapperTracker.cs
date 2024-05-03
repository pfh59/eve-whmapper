
using System.Timers;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveJwtAuthenticationStateProvider;

namespace WHMapper;

public class EveMapperTracker : IEveMapperTracker,IAsyncDisposable
{  
    private const int TRACK_HIT_IN_MS = 1000;
    private readonly ILogger _logger;
    private readonly AuthenticationStateProvider _authState ;
    private readonly IEveAPIServices? _eveAPIServices;


    private System.Timers.Timer? _timer=null!;


    private EveLocation? _currentLocation = null!;
    private ESISolarSystem? _currentSolarSystem = null!;
    private Ship? _currentShip = null!;
    private WHMapper.Models.DTO.EveAPI.Universe.Type? _currentShiptInfos = null!;

    public event Func< ESISolarSystem, Task> SystemChanged =null!;
    public event Func<Ship,WHMapper.Models.DTO.EveAPI.Universe.Type,Task> ShipChanged=null!;

    public EveMapperTracker(ILogger<EveMapperTracker> logger,AuthenticationStateProvider authState,IEveAPIServices eveAPI)
    {
        _logger=logger;
        _authState=authState;
        _eveAPIServices=eveAPI;

        _timer = new System.Timers.Timer(TRACK_HIT_IN_MS);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;

    }

    public async ValueTask DisposeAsync()
    {
        if(_timer!=null)
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
            if (!string.IsNullOrEmpty(state?.User?.Identity?.Name) && _eveAPIServices!=null && _eveAPIServices.LocationServices!=null)
            {
                EveLocation? el = await _eveAPIServices.LocationServices.GetLocation();
                Ship? ship = await _eveAPIServices.LocationServices.GetCurrentShip();

                if(ship!=null && (_currentShip == null || _currentShip.ShipItemId != ship.ShipItemId))
                {
                    _logger.LogInformation("Ship Changed");
                    _currentShip = ship;
                    _currentShiptInfos = await _eveAPIServices.UniverseServices.GetType(ship!.ShipTypeId);
                    if(_currentShip!=null && _currentShiptInfos!=null)
                        ShipChanged?.Invoke(_currentShip,_currentShiptInfos);
                }

                if (el != null && (_currentLocation == null || _currentLocation.SolarSystemId != el.SolarSystemId) )
                {   
                    _logger.LogInformation("System Changed");
                    _currentLocation = el;
                    _currentSolarSystem = await _eveAPIServices.UniverseServices.GetSystem(el.SolarSystemId);
                    if(_currentSolarSystem!=null)
                        SystemChanged?.Invoke(_currentSolarSystem);
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Track error");
        }
    }
}
