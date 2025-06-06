using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using MudBlazor;
using Microsoft.AspNetCore.Authorization;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;
using WHMapper.Services.EveMapper;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Components.Pages.Mapper;

[Authorize(Policy = "Access")]
public partial class Overview : IAsyncDisposable
{
    private List<WHMap> WHMaps { get; set; } = new List<WHMap>();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private int _selectedWHMapIndex = 0;
    private int SelectedWHMapIndex
    {
        get
        {
            return _selectedWHMapIndex;
        }
        set
        {
            if (_selectedWHMapIndex != value)
            {
                _selectedWHMapIndex = value;
            }
        }
    }

    private bool _loading = true;

    [Inject]
    public ILogger<Overview> Logger { get; set; } = null!;
    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private  AuthenticationStateProvider _authenticationStateProvider {get;set;} = null!;
    [Inject]
    private IAuthorizationService _authorizationService { get; set; } = null!;

    [Inject]
    IEveMapperRealTimeService? RealTimeService {get;set;} = null!;
    
    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    IWHMapRepository DbWHMaps { get; set; } = null!;

    [Inject]
    private IEveMapperTracker TrackerServices { get; set; } = null!;

    [Inject]
    private IPasteServices PasteServices { get; set; } = null!;


    private WHMap? _selectedWHMap = null!;
    private WHMapperUser? PrimaryAccount  { get; set; } = null!;


    protected override async Task OnInitializedAsync()
    {

        if (!string.IsNullOrEmpty(UID.ClientId) && !string.IsNullOrWhiteSpace(UID.ClientId))
        {
            PrimaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
        }
        else
        {
            Logger.LogError("ClientId is null");
            Snackbar?.Add("Error: ClientId is null", Severity.Error);
        }

        if (!await InitRealTimeService())
        {
            Snackbar?.Add("RealTimeService Initialization error", Severity.Error);
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if(!await RestoreMaps())
            {
                Snackbar?.Add("Mapper Initialization error", Severity.Error);
            }
            _loading = false;
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task<bool> RestoreMaps()
    {
        var allMaps = await DbWHMaps.GetAll();
        if (allMaps == null || !allMaps.Any())
        {
            _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
            if (_selectedWHMap != null)
            {
                await RestoreMaps();
            }
        }
        else
        {
            WHMaps.Clear();
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
    
            foreach (var map in allMaps)
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, map.Id, "Map");
                if (authorizationResult.Succeeded)
                {
                    WHMaps.Add(map);
                }
            }

            _selectedWHMap = WHMaps.FirstOrDefault();
        }

        if(allMaps!=null && allMaps.Any() && _selectedWHMap==null)
            return true;
        else
            return _selectedWHMap != null;
    }

    public async ValueTask DisposeAsync()
    {
        if (TrackerServices != null)
        {
            await TrackerServices.DisposeAsync();

        }

        if(RealTimeService!=null)
        {
            
            RealTimeService.MapAdded -= OnMapAdded;
            RealTimeService.MapRemoved -= OnMapRemoved;
            RealTimeService.MapNameChanged -= OnMapNameChanged;
            RealTimeService.AllMapsRemoved -= OnAllMapsRemoved;
            RealTimeService.MapAccessesAdded-=OnMapAccessesAdded;
            RealTimeService.MapAccessRemoved-=OnMapAccessRemoved;
            RealTimeService.MapAllAccessesRemoved-=OnMapAllAccessesRemoved;
            await RealTimeService.DisposeAsync();
            
        }
    }

    private async Task<bool> InitRealTimeService()
    {
        try
        {
            if(RealTimeService==null) 
            {
                Logger.LogError("RealTimeService is null");
                return false;
            }
            
            RealTimeService.MapAdded += OnMapAdded;
            RealTimeService.MapRemoved += OnMapRemoved;
            RealTimeService.MapNameChanged += OnMapNameChanged;
            RealTimeService.AllMapsRemoved += OnAllMapsRemoved;
            RealTimeService.MapAccessesAdded+=OnMapAccessesAdded;
            RealTimeService.MapAccessRemoved+=OnMapAccessRemoved;
            RealTimeService.MapAllAccessesRemoved+=OnMapAllAccessesRemoved;

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "InitRealTimeService error");
            return false;
        }
    } 

    #region RealTimeService User Events
    
    private async Task OnMapAdded(int accountID, int mapId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            //Check if map is already in the list
            if(WHMaps.Any(m=>m.Id==mapId))
            {
                return;
            }

            var map = await DbWHMaps.GetById(mapId);
            if (map != null)
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, map.Id, "Map");
                if (authorizationResult.Succeeded)
                {
                    WHMaps.Add(map);
                    Snackbar?.Add($"Map {map.Name} added", Severity.Info);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapAdded error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
        
    }

    private async Task OnMapRemoved(int accountID, int mapId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            var map = WHMaps.FirstOrDefault(m => m.Id == mapId);
            if (map != null)
            {
                WHMaps.Remove(map);
                Snackbar?.Add($"Map {map.Name} deleted", Severity.Info);
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapDeleted error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnMapNameChanged(int accountID, int mapId, string newName)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            var map = WHMaps.FirstOrDefault(m => m.Id == mapId);
            if (map != null && map.Name != newName)
            {
                map.Name = newName;
                Snackbar?.Add($"Map {map.Name} renamed to {newName}", Severity.Info);
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapNameChanged error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnAllMapsRemoved(int accountID)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (!WHMaps.Any())
            {
                return;
            }

            WHMaps.Clear();
            Snackbar?.Add("All maps deleted", Severity.Info);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while handling OnAllMapsRemoved");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnMapAccessesAdded(int accountID, int mapId, IEnumerable<int> accessId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {       
        
            var map = WHMaps.FirstOrDefault(m => m.Id == mapId);

            var mapWithAccessUpdated = await DbWHMaps.GetById(mapId);
            if(mapWithAccessUpdated==null)
            {
                Logger.LogError("Map not found on NotifyMapAccessesAdded");
                Snackbar?.Add("Map not found on NotifyMapAccessesAdded", Severity.Error);
                return;
            }
             var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, mapWithAccessUpdated.Id, "Map");
            

            if(map!=null)
            {
                if (authorizationResult.Succeeded)
                {
                    foreach (var accessIdToAdd in accessId)
                    {
                        var accessToAdd = mapWithAccessUpdated.WHAccesses.FirstOrDefault(a => a.Id == accessIdToAdd);
                        if (accessToAdd != null)
                        {
                            if(map.WHAccesses.Any(a => a.Id == accessIdToAdd))
                            {
                                continue;
                            }
                            map.WHAccesses.Add(accessToAdd);
                        }
                    }
                }
            }
            else
            {
                if (authorizationResult.Succeeded)
                {
                    WHMaps.Add(mapWithAccessUpdated);
                    Snackbar?.Add($"Map {mapWithAccessUpdated.Name} added", Severity.Info);
                    await InvokeAsync(StateHasChanged);
                }
            }
                      
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapAccessesAdded error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnMapAccessRemoved(int accountID, int mapId, int accessId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            var map = WHMaps.FirstOrDefault(m => m.Id == mapId);
            if (map != null)
            {
                var accessToRemove = map.WHAccesses.FirstOrDefault(a => a.Id == accessId);
                if (accessToRemove != null)
                {
                    map.WHAccesses.Remove(accessToRemove);

                    var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
    
                    var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, map.Id, "Map");
                    if (!authorizationResult.Succeeded)
                    {
                        WHMaps.Remove(map);
                        Snackbar?.Add($"Access has been removed from map {map.Name}", Severity.Info);
                        await InvokeAsync(StateHasChanged);
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapAccessRemoved error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnMapAllAccessesRemoved(int accountID, int mapId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            var map = WHMaps.FirstOrDefault(m => m.Id == mapId);
            if (map != null)
            {
                map.WHAccesses.Clear();
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
    
                var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, map.Id, "Map");
                if (!authorizationResult.Succeeded)
                {
                    WHMaps.Remove(map);
                    Snackbar?.Add($"All accesses removed from map {map.Name}", Severity.Info);
                    await InvokeAsync(StateHasChanged);
                }
            }  
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyMapAllAccessesRemoved error");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    #endregion

    #region Map Tab Events
        private Task CloseMapTab(MudTabPanel panel)
        {
            if(panel!=null)
            {
                int mapId = (int)panel.ID;
                var map = WHMaps.FirstOrDefault(m => m.Id == mapId);
                if(map!=null)
                {
                    WHMaps.Remove(map);
                    Logger.LogInformation($"Map {map.Name} closed");
                    StateHasChanged();
                }
                
            }
            return Task.CompletedTask;
        }
     #endregion

}