using Microsoft.AspNetCore.Components;
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

    /// <summary>
    /// Event triggered when the primary account changes, to notify parent components to reload maps.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPrimaryAccountChanged { get; set; }

    public IEnumerable<WHMapperUser> Accounts { get; private set; } = new List<WHMapperUser>();

    private CancellationTokenSource _cts = new();
    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to primary account changes
        EveMapperUserManagementService.PrimaryAccountChanged += OnPrimaryAccountChangedHandler;
        // Subscribe to current map changes
        EveMapperUserManagementService.CurrentMapChanged += OnCurrentMapChangedHandler;
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

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        
        // Unsubscribe from events
        EveMapperUserManagementService.PrimaryAccountChanged -= OnPrimaryAccountChangedHandler;
        EveMapperUserManagementService.CurrentMapChanged -= OnCurrentMapChangedHandler;
        
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
