using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.SDE;

namespace WHMapper.Components.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    private bool _loading = true;
    private string _init_process_msg = string.Empty;
    private bool _disposed = false;
    private bool _isWaitingForOtherInitialization = false;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private ISDEServiceManager SDEServices { get; set; } = null!;

    [Inject]
    private ISDEInitializationState SDEInitializationState { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService? RealTimeService { get; set; }

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (SDEServices.IsExtractionSuccesful())
        {
            _loading = false;
        }
        else if (SDEInitializationState.IsInitializationInProgress)
        {
            // Another user is already initializing the SDE, subscribe to progress updates
            _isWaitingForOtherInitialization = true;
            _init_process_msg = SDEInitializationState.CurrentProgressMessage;
            SDEInitializationState.OnProgressChanged += OnSDEProgressChanged;
            SDEInitializationState.OnInitializationCompleted += OnSDEInitializationCompleted;
        }

        // Subscribe to instance access events
        await InitRealTimeServiceEvents();

        // Subscribe to primary account changes to refresh authorization state
        UserManagement.PrimaryAccountChanged += OnPrimaryAccountChanged;

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!SDEServices.IsExtractionSuccesful())
            {
                if (_isWaitingForOtherInitialization)
                {
                    // Wait for the other initialization to complete
                    await WaitForOtherInitializationAsync();
                }
                else
                {
                    await DownloadExtractImportSDE();
                }
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnSDEProgressChanged(string message)
    {
        InvokeAsync(() =>
        {
            _init_process_msg = message;
            StateHasChanged();
        });
    }

    private void OnSDEInitializationCompleted()
    {
        InvokeAsync(() =>
        {
            _loading = false;
            StateHasChanged();
        });
    }

    private async Task WaitForOtherInitializationAsync()
    {
        try
        {
            await SDEInitializationState.WaitForInitializationAsync();
            
            // Verify the SDE was successfully extracted
            if (SDEServices.IsExtractionSuccesful())
            {
                await SetLoading(false);
            }
            else
            {
                // The other initialization failed, try our own
                _isWaitingForOtherInitialization = false;
                await DownloadExtractImportSDE();
            }
        }
        catch (TaskCanceledException)
        {
            // Handle cancellation gracefully
            await SetLoading(false);
        }
    }

    private async Task InitRealTimeServiceEvents()
    {
        if (RealTimeService != null && !string.IsNullOrEmpty(UID?.ClientId))
        {
            RealTimeService.InstanceAccessAdded += OnInstanceAccessAdded;
            RealTimeService.InstanceAccessRemoved += OnInstanceAccessRemoved;

            // Start the RealTimeService for the primary account so we can receive instance access notifications
            // even when the user doesn't have access to any instance yet
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount != null)
            {
                await RealTimeService.Start(primaryAccount.Id);
            }
        }
    }

    private async Task OnInstanceAccessAdded(int accountID, int instanceId, int accessId)
    {
        try
        {
            if (string.IsNullOrEmpty(UID?.ClientId))
                return;

            // accountID is the sender (admin who added access), not the receiver
            // We need to check if the access was granted to one of our accounts
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount == null)
                return;

            // Don't react to our own notifications
            if (primaryAccount.Id == accountID)
                return;

            // Refresh the page to re-evaluate the authorization
            // The policy will check if we now have access
            Snackbar.Add("You have been granted access to an instance! Refreshing...", Severity.Success);
            await InvokeAsync(() =>
            {
                // Force navigation to reload the page and re-evaluate authorization
                Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            });
        }
        catch (Exception)
        {
            // Silently handle errors
        }
    }

    private async Task OnInstanceAccessRemoved(int accountID, int instanceId, int accessId)
    {
        try
        {
            if (string.IsNullOrEmpty(UID?.ClientId))
                return;

            // accountID is the sender (admin who removed access), not the receiver
            // We need to check if our access was revoked
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount == null)
                return;

            // Don't react to our own notifications
            if (primaryAccount.Id == accountID)
                return;

            // Refresh the page to re-evaluate the authorization
            // The policy will check if we still have access
            Snackbar.Add("Your access to an instance has been revoked. Refreshing...", Severity.Warning);
            await InvokeAsync(() =>
            {
                // Force navigation to reload the page and re-evaluate authorization
                Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            });
        }
        catch (Exception)
        {
            // Silently handle errors
        }
    }

    private async Task OnPrimaryAccountChanged(string clientId, int newPrimaryAccountId)
    {
        try
        {
            // Only react to changes for our client
            if (clientId != UID.ClientId)
                return;

            // Force navigation to reload the page and re-evaluate authorization
            // The new primary account may have different instance access
            await InvokeAsync(() =>
            {
                Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            });
        }
        catch (Exception)
        {
            // Silently handle errors
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (RealTimeService != null)
        {
            RealTimeService.InstanceAccessAdded -= OnInstanceAccessAdded;
            RealTimeService.InstanceAccessRemoved -= OnInstanceAccessRemoved;
        }

        // Unsubscribe from primary account changes
        UserManagement.PrimaryAccountChanged -= OnPrimaryAccountChanged;

        // Unsubscribe from SDE initialization events
        SDEInitializationState.OnProgressChanged -= OnSDEProgressChanged;
        SDEInitializationState.OnInitializationCompleted -= OnSDEInitializationCompleted;

        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }

    private async Task SetProcessMessage(string message)
    {
        await InvokeAsync(() =>
        {
            _init_process_msg = message;
            StateHasChanged();
        });
    }

    private async Task SetLoading(bool loading)
    {
        await InvokeAsync(() =>
        {
            _loading = loading;
            StateHasChanged();
        });
    }

    private async Task Cleaning()
    {
        await SetProcessMessage("Cleaning ... ");
        await SDEServices.ClearCache();
    }

    private async Task DownloadExtractImportSDE()
    {
        // Try to acquire the initialization lock
        if (!SDEInitializationState.TryAcquireInitializationLock())
        {
            // Another user started initialization between our check and now
            // Subscribe to progress updates and wait
            _isWaitingForOtherInitialization = true;
            _init_process_msg = SDEInitializationState.CurrentProgressMessage;
            SDEInitializationState.OnProgressChanged += OnSDEProgressChanged;
            SDEInitializationState.OnInitializationCompleted += OnSDEInitializationCompleted;
            await WaitForOtherInitializationAsync();
            return;
        }

        try
        {
            if (await SDEServices.IsNewSDEAvailable())
            {
                await SetProcessMessageWithGlobalUpdate("Removing current SDE package (1/4)");
                if (!await SDEServices.ClearSDEResources())
                {
                    Snackbar.Add("Removing current SDE package failed", Severity.Error);
                    await Cleaning();
                    return;
                }

                await SetProcessMessageWithGlobalUpdate("Download SDE package (2/4)");
                if (!await SDEServices.DownloadSDE())
                {
                    Snackbar.Add("Download SDE package failed.", Severity.Error);
                    await Cleaning();
                    return;
                }

                await SetProcessMessageWithGlobalUpdate("Extract SDE package (3/4)");
                if (!await SDEServices.ExtractSDE())
                {
                    Snackbar.Add("Extract SDE package failed.", Severity.Error);
                    await Cleaning();
                    return;
                }

                await SetProcessMessageWithGlobalUpdate("Initialize SDE cache (4/4)");
                if (!await SDEServices.BuildCache())
                {
                    Snackbar.Add("Initialize SDE cache failed.", Severity.Error);
                    await Cleaning();
                    return;
                }
            }
            await SetLoading(false);
        }
        finally
        {
            // Always release the lock when done
            SDEInitializationState.ReleaseInitializationLock();
        }
    }

    private async Task SetProcessMessageWithGlobalUpdate(string message)
    {
        // Update both local UI and global state for other users
        SDEInitializationState.SetProgressMessage(message);
        await SetProcessMessage(message);
    }
}
