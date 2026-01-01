using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private ISDEServiceManager SDEServices { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService? RealTimeService { get; set; }

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (SDEServices.IsExtractionSuccesful())
        {
            _loading = false;
        }

        // Subscribe to instance access events
        await InitRealTimeServiceEvents();

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!SDEServices.IsExtractionSuccesful())
            {
                await DownloadExtractImportSDE();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task InitRealTimeServiceEvents()
    {
        if (RealTimeService != null)
        {
            RealTimeService.InstanceAccessAdded += OnInstanceAccessAdded;
        }
        await Task.CompletedTask;
    }

    private async Task OnInstanceAccessAdded(int accountID, int instanceId, int accessId)
    {
        try
        {
            // Check if the current user now has access
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                // Refresh the page to re-evaluate the authorization
                Snackbar.Add("You have been granted access to an instance! Refreshing...", Severity.Success);
                await InvokeAsync(() =>
                {
                    // Force navigation to reload the page and re-evaluate authorization
                    Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
                });
            }
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
        }

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
        if (await SDEServices.IsNewSDEAvailable())
        {
            await SetProcessMessage("Removing current SDE package (1/4)");
            if (!await SDEServices.ClearSDEResources())
            {
                Snackbar.Add("Removing current SDE package failed", Severity.Error);
                await Cleaning();
                return;
            }

            await SetProcessMessage("Download SDE package (2/4)");
            if (!await SDEServices.DownloadSDE())
            {
                Snackbar.Add("Download SDE package failed.", Severity.Error);
                await Cleaning();
                return;
            }

            await SetProcessMessage("Extract SDE package (3/4)");
            if (!await SDEServices.ExtractSDE())
            {
                Snackbar.Add("Extract SDE package failed.", Severity.Error);
                await Cleaning();
                return;
            }

            await SetProcessMessage("Initialize SDE cache (4/4)");
            if (!await SDEServices.BuildCache())
            {
                Snackbar.Add("Initialize SDE cache failed.", Severity.Error);
                await Cleaning();
                return;
            }
        }
        await SetLoading(false);
    }
}
