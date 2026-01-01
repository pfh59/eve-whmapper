using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Mapper.Users;

public partial class Overview : IAsyncDisposable
{
    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService EveMapperUserManagementService {get; set;} = null!;

    [Inject]
    private IEveMapperRealTimeService EveMapperRealTime { get; set; } = null!;    

    [Inject]
    private IEveMapperTracker TrackerServices { get; set; } = null!;

    [Inject]
    private ILogger<Overview> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    /// <summary>
    /// Event triggered when the primary account changes, to notify parent components to reload maps.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPrimaryAccountChanged { get; set; }

    public IEnumerable<WHMapperUser> Accounts { get; private set; } = new List<WHMapperUser>();

    /// <summary>
    /// Indicates if we are on the instances page - disables primary selection and tracking
    /// </summary>
    public bool IsOnInstancesPage { get; private set; } = false;

    /// <summary>
    /// Stores tracking state before pausing when navigating to instances page
    /// </summary>
    private Dictionary<int, bool> _savedTrackingState = new();

    private CancellationTokenSource _cts = new();
    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to primary account changes
        EveMapperUserManagementService.PrimaryAccountChanged += OnPrimaryAccountChangedHandler;
        // Subscribe to current map changes
        EveMapperUserManagementService.CurrentMapChanged += OnCurrentMapChangedHandler;
        // Subscribe to navigation changes
        Navigation.LocationChanged += OnLocationChanged;
        
        // Check initial location
        UpdateInstancesPageState(Navigation.Uri);
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("User Overview component OnParametersSetAsync");
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        _ = Task.Run(async () => await LoadAccountsAsync(_cts.Token));
        Logger.LogInformation("User Overview component OnParametersSetAsync done");

        await base.OnParametersSetAsync();

    }

    private async Task<bool> LoadAccountsAsync(CancellationToken cancellationToken)
    {
        if(cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning("LoadAccountsAsync cancelled");
            return false;
        }

        if (EveMapperUserManagementService != null && UID != null && !String.IsNullOrEmpty(UID.ClientId))
        {
            // Update map access status for all accounts based on primary account's maps
            await EveMapperUserManagementService.UpdateAccountsMapAccessAsync(UID.ClientId);
            
            Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
            foreach (var account in Accounts)
            {
                await EveMapperRealTime.Start(account.Id);
                
                if (account.IsPrimary)
                {
                    // Primary account always has map access - ensure tracking is enabled
                    if (!account.Tracking)
                    {
                        Logger.LogInformation("Enabling tracking for primary account {AccountId}", account.Id);
                        account.Tracking = true;
                    }
                }
                else if (!account.HasMapAccess && account.Tracking)
                {
                    // Auto-disable tracking for secondary accounts without map access
                    Logger.LogInformation("Disabling tracking for account {AccountId} - no map access", account.Id);
                    account.Tracking = false;
                    await TrackerServices.StopTracking(account.Id);
                }
            }
        }
        await InvokeAsync(StateHasChanged);
        return true;
    }

    private async Task ToggleTracking(int accountId)
    {
        var account = Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null)
        {
            return;
        }

        // Prevent enabling tracking for accounts without access to the current map
        if (!account.Tracking && !account.HasCurrentMapAccess)
        {
            Logger.LogWarning("Cannot enable tracking for account {AccountId} - no access to current map", accountId);
            return;
        }

        account.Tracking = !account.Tracking;
        if (account.Tracking)
        {
            await TrackerServices.StartTracking(account.Id);
        }
        else
        {
            await TrackerServices.StopTracking(account.Id);
        }

        StateHasChanged();
        return;
    }

    private async Task SetPrimaryAccount(int accountId)
    {
        if (!string.IsNullOrEmpty(UID.ClientId))
        {
            Logger.LogInformation("Setting primary account to {AccountId}", accountId);
            
            // Stop tracking for all accounts before switching primary
            foreach (var account in Accounts)
            {
                if (account.Tracking)
                {
                    Logger.LogInformation("Stopping tracking for account {AccountId} before primary switch", account.Id);
                    await TrackerServices.StopTracking(account.Id);
                }
            }
            
            // Set the new primary account - this will trigger OnPrimaryAccountChangedHandler
            // which will reload maps and manage tracking based on map access
            await EveMapperUserManagementService.SetPrimaryAccountAsync(UID.ClientId, accountId.ToString());
            StateHasChanged();
        }
    }

    private async Task OnPrimaryAccountChangedHandler(string clientId, int newPrimaryAccountId)
    {
        if (clientId != UID.ClientId)
        {
            return;
        }

        Logger.LogInformation("Primary account changed to {AccountId}, updating tracking status", newPrimaryAccountId);
        
        // Reload accounts to get updated HasMapAccess status
        Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
        
        // Manage tracking based on map access
        foreach (var account in Accounts)
        {
            if (account.IsPrimary)
            {
                // Primary account always has map access - ensure tracking is enabled
                if (!account.Tracking)
                {
                    Logger.LogInformation("Enabling tracking for primary account {AccountId}", account.Id);
                    account.Tracking = true;
                }
                await TrackerServices.StartTracking(account.Id);
            }
            else if (!account.HasMapAccess)
            {
                // Disable tracking for accounts without map access
                if (account.Tracking)
                {
                    Logger.LogInformation("Disabling tracking for account {AccountId} - no map access after primary change", account.Id);
                    account.Tracking = false;
                }
                await TrackerServices.StopTracking(account.Id);
            }
            else if (account.HasMapAccess && account.Tracking)
            {
                // Restart tracking for secondary accounts with access
                await TrackerServices.StartTracking(account.Id);
            }
        }
        
        await InvokeAsync(StateHasChanged);
        
        // Notify parent to reload maps
        if (OnPrimaryAccountChanged.HasDelegate)
        {
            await OnPrimaryAccountChanged.InvokeAsync(newPrimaryAccountId);
        }
    }

    private async Task OnCurrentMapChangedHandler(string clientId, int mapId)
    {
        if (clientId != UID.ClientId)
        {
            return;
        }

        Logger.LogInformation("Current map changed to {MapId}, updating tracking status based on map access", mapId);
        
        // Reload accounts to get updated HasCurrentMapAccess status
        Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
        
        // Don't manage tracking if we're on instances page
        if (IsOnInstancesPage)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }
        
        // Manage tracking based on current map access
        foreach (var account in Accounts)
        {
            if (account.HasCurrentMapAccess)
            {
                // Enable tracking for accounts with access to the current map
                if (!account.Tracking)
                {
                    Logger.LogInformation("Enabling tracking for account {AccountId} - has access to map {MapId}", account.Id, mapId);
                    account.Tracking = true;
                }
                await TrackerServices.StartTracking(account.Id);
            }
            else
            {
                // Disable tracking for accounts without access to the current map
                if (account.Tracking)
                {
                    Logger.LogInformation("Disabling tracking for account {AccountId} - no access to map {MapId}", account.Id, mapId);
                    account.Tracking = false;
                }
                await TrackerServices.StopTracking(account.Id);
            }
        }
        
        await InvokeAsync(StateHasChanged);
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var wasOnInstancesPage = IsOnInstancesPage;
        UpdateInstancesPageState(e.Location);
        
        if (wasOnInstancesPage != IsOnInstancesPage)
        {
            if (IsOnInstancesPage)
            {
                await PauseTrackingForInstancesPage();
            }
            else
            {
                await ResumeTrackingFromInstancesPage();
            }
        }
        
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateInstancesPageState(string uri)
    {
        var relativeUri = new Uri(uri).PathAndQuery;
        IsOnInstancesPage = relativeUri.StartsWith("/instances", StringComparison.OrdinalIgnoreCase) ||
                           relativeUri.StartsWith("/instance/", StringComparison.OrdinalIgnoreCase) ||
                           relativeUri.StartsWith("/register", StringComparison.OrdinalIgnoreCase);
    }

    private async Task PauseTrackingForInstancesPage()
    {
        Logger.LogInformation("Navigating to instances page - pausing tracking for all accounts");
        
        // Save current tracking state and stop all tracking
        _savedTrackingState.Clear();
        foreach (var account in Accounts)
        {
            _savedTrackingState[account.Id] = account.Tracking;
            if (account.Tracking)
            {
                Logger.LogInformation("Stopping tracking for account {AccountId}", account.Id);
                account.Tracking = false;
                await TrackerServices.StopTracking(account.Id);
            }
        }
    }

    private async Task ResumeTrackingFromInstancesPage()
    {
        Logger.LogInformation("Returning from instances page - restoring tracking state");
        
        // Restore tracking state from before navigating to instances
        foreach (var account in Accounts)
        {
            if (_savedTrackingState.TryGetValue(account.Id, out var wasTracking) && wasTracking)
            {
                // Only restore tracking if account still has map access
                if (account.HasCurrentMapAccess)
                {
                    Logger.LogInformation("Restoring tracking for account {AccountId}", account.Id);
                    account.Tracking = true;
                    await TrackerServices.StartTracking(account.Id);
                }
            }
        }
        
        _savedTrackingState.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        
        // Unsubscribe from events
        EveMapperUserManagementService.PrimaryAccountChanged -= OnPrimaryAccountChangedHandler;
        EveMapperUserManagementService.CurrentMapChanged -= OnCurrentMapChangedHandler;
        Navigation.LocationChanged -= OnLocationChanged;
        
        // Cancel pending operations
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        _cts?.Dispose();

        // Stop services for all accounts
        foreach (var account in Accounts)
        {
            try
            {
                await EveMapperRealTime.Stop(account.Id);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error stopping RealTimeService for account {AccountId}", account.Id);
            }
            
            try
            {
                await TrackerServices.StopTracking(account.Id);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error stopping tracking for account {AccountId}", account.Id);
            }
        }
        
        GC.SuppressFinalize(this);
    }
}
