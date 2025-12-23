using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using MudBlazor;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Services.EveMapper;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Components.Pages.Mapper;

[Authorize(Policy = "Access")]
public partial class Overview : IAsyncDisposable
{
    private List<WHMap> WHMaps { get; set; } = new List<WHMap>();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private bool _disposed = false;

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

    [Inject]
    private IEveMapperAccessHelper AccessHelper { get; set; } = null!;


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

        // Subscribe to primary account changes to reload maps
        UserManagement.PrimaryAccountChanged += OnPrimaryAccountChanged;

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
            
            // Load maps based on primary account's access, not the authenticated user
            if (PrimaryAccount == null)
            {
                PrimaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            }
            
            if (PrimaryAccount == null)
            {
                Logger.LogWarning("No primary account found, cannot load maps");
                return false;
            }
            
            Logger.LogInformation("Loading maps for primary account {AccountId}", PrimaryAccount.Id);
    
            foreach (var map in allMaps)
            {
                // Check if the primary account has access to this map
                if (await AccessHelper.IsEveMapperMapAccessAuthorized(PrimaryAccount.Id, map.Id))
                {
                    WHMaps.Add(map);
                }
            }
            
            Logger.LogInformation("Loaded {MapCount} maps for primary account {AccountId}", WHMaps.Count, PrimaryAccount.Id);

            _selectedWHMap = WHMaps.FirstOrDefault();
        }

        if(allMaps!=null && allMaps.Any() && _selectedWHMap==null)
            return true;
        else
            return _selectedWHMap != null;
    }

    /// <summary>
    /// Handler for when the primary account changes - reloads maps based on new primary account.
    /// </summary>
    private async Task OnPrimaryAccountChanged(string clientId, int newPrimaryAccountId)
    {
        if (clientId != UID.ClientId)
        {
            return;
        }

        Logger.LogInformation("Primary account changed to {AccountId}, reloading maps", newPrimaryAccountId);
        
        await _semaphoreSlim.WaitAsync();
        try
        {
            _loading = true;
            await InvokeAsync(StateHasChanged);
            
            // Update the primary account reference
            PrimaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            
            // Reload maps for the new primary account
            if (!await RestoreMaps())
            {
                Snackbar?.Add("Error reloading maps after primary account change", Severity.Error);
            }
            else
            {
                Snackbar?.Add($"Maps reloaded for new primary account", Severity.Info);
            }
            
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling primary account change");
            Snackbar?.Add("Error switching primary account", Severity.Error);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        
        // Unsubscribe from primary account changes
        UserManagement.PrimaryAccountChanged -= OnPrimaryAccountChanged;
        
        try
        {
            if (TrackerServices != null)
            {
                await TrackerServices.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error disposing TrackerServices");
        }

        try
        {
            if (RealTimeService != null)
            {
                RealTimeService.MapAdded -= OnMapAdded;
                RealTimeService.MapRemoved -= OnMapRemoved;
                RealTimeService.MapNameChanged -= OnMapNameChanged;
                RealTimeService.AllMapsRemoved -= OnAllMapsRemoved;
                RealTimeService.MapAccessesAdded -= OnMapAccessesAdded;
                RealTimeService.MapAccessRemoved -= OnMapAccessRemoved;
                RealTimeService.MapAllAccessesRemoved -= OnMapAllAccessesRemoved;
                await RealTimeService.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error disposing RealTimeService");
        }
        
        _semaphoreSlim.Dispose();
        GC.SuppressFinalize(this);
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
            if (map != null && PrimaryAccount != null)
            {
                // Check if the primary account has access to this map
                if (await AccessHelper.IsEveMapperMapAccessAuthorized(PrimaryAccount.Id, map.Id))
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
            
            if (PrimaryAccount == null)
            {
                Logger.LogWarning("No primary account found on NotifyMapAccessesAdded");
                return;
            }
            
            // Check if the primary account has access to this map
            var hasAccess = await AccessHelper.IsEveMapperMapAccessAuthorized(PrimaryAccount.Id, mapWithAccessUpdated.Id);
            

            if(map!=null)
            {
                if (hasAccess)
                {
                    foreach (var accessIdToAdd in accessId)
                    {
                        var accessToAdd = mapWithAccessUpdated.WHMapAccesses.FirstOrDefault(a => a.Id == accessIdToAdd);
                        if (accessToAdd != null)
                        {
                            if(map.WHMapAccesses.Any(a => a.Id == accessIdToAdd))
                            {
                                continue;
                            }
                            map.WHMapAccesses.Add(accessToAdd);
                        }
                    }
                }
            }
            else
            {
                if (hasAccess)
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
            if (map != null && PrimaryAccount != null)
            {
                var accessToRemove = map.WHMapAccesses.FirstOrDefault(a => a.Id == accessId);
                if (accessToRemove != null)
                {
                    map.WHMapAccesses.Remove(accessToRemove);

                    // Check if the primary account still has access to this map
                    var hasAccess = await AccessHelper.IsEveMapperMapAccessAuthorized(PrimaryAccount.Id, map.Id);
                    if (!hasAccess)
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
            if (map != null && PrimaryAccount != null)
            {
                map.WHMapAccesses.Clear();
                
                // Check if the primary account still has access to this map
                var hasAccess = await AccessHelper.IsEveMapperMapAccessAuthorized(PrimaryAccount.Id, map.Id);
                if (!hasAccess)
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