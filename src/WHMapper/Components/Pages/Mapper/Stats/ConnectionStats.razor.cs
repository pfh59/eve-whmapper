using Microsoft.AspNetCore.Components;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Mapper.Stats;

public partial class ConnectionStats : IAsyncDisposable
{
    [Inject]
    private IEveMapperRealTimeService EveMapperRealTime { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService EveMapperUserManagementService { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private ILogger<ConnectionStats> Logger { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    private int? CurrentMapId { get; set; }
    private int MapUserCount { get; set; }
    private int TotalUserCount { get; set; }

    private int? _primaryAccountId;
    private bool _disposed;

    protected override Task OnInitializedAsync()
    {
        EveMapperRealTime.UserConnected += OnUserConnected;
        EveMapperRealTime.UserDisconnected += OnUserDisconnected;
        EveMapperRealTime.UserOnMapConnected += OnMapPresenceChanged;
        EveMapperRealTime.UserOnMapDisconnected += OnMapPresenceChanged;
        EveMapperUserManagementService.CurrentMapChanged += OnCurrentMapChanged;
        EveMapperUserManagementService.PrimaryAccountChanged += OnPrimaryAccountChanged;

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        // The hub is started asynchronously by the Users.Overview component once accounts
        // are loaded. Wait until the primary account is known and the hub is connected
        // before issuing the initial queries.
        for (int i = 0; i < 30 && !_disposed; i++)
        {
            if (!_primaryAccountId.HasValue)
            {
                var primary = await TryGetPrimaryAccountAsync();
                if (primary.HasValue)
                {
                    _primaryAccountId = primary;
                }
            }

            if (_primaryAccountId.HasValue && await EveMapperRealTime.IsConnected(_primaryAccountId.Value))
            {
                await RefreshTotalAsync();
                if (CurrentMapId.HasValue)
                    await RefreshMapAsync();
                return;
            }

            await Task.Delay(500);
        }
    }

    private async Task<int?> TryGetPrimaryAccountAsync()
    {
        if (string.IsNullOrEmpty(UID.ClientId))
            return null;

        try
        {
            var primary = await EveMapperUserManagementService.GetPrimaryAccountAsync(UID.ClientId);
            return primary?.Id;
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to resolve primary account");
            return null;
        }
    }

    private async Task OnCurrentMapChanged(string clientId, int mapId)
    {
        if (clientId != UID.ClientId)
            return;

        CurrentMapId = mapId;
        await RefreshMapAsync();
    }

    private async Task OnPrimaryAccountChanged(string clientId, int newPrimaryAccountId)
    {
        if (clientId != UID.ClientId)
            return;

        _primaryAccountId = newPrimaryAccountId;
        await RefreshTotalAsync();
        await RefreshMapAsync();
    }

    private Task OnUserConnected(int accountId) => RefreshTotalAsync();

    private Task OnUserDisconnected(int accountId) => RefreshTotalAsync();

    private async Task OnMapPresenceChanged(int accountId, int mapId)
    {
        if (CurrentMapId.HasValue && mapId == CurrentMapId.Value)
        {
            await RefreshMapAsync();
        }
    }

    private async Task RefreshTotalAsync()
    {
        if (_disposed || !_primaryAccountId.HasValue)
            return;

        try
        {
            TotalUserCount = await EveMapperRealTime.GetTotalConnectedUsers(_primaryAccountId.Value);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to refresh total connected users");
        }
    }

    private async Task RefreshMapAsync()
    {
        if (_disposed || !_primaryAccountId.HasValue || !CurrentMapId.HasValue)
            return;

        try
        {
            MapUserCount = await EveMapperRealTime.GetUserCountOnMap(_primaryAccountId.Value, CurrentMapId.Value);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to refresh map user count");
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        _disposed = true;

        EveMapperRealTime.UserConnected -= OnUserConnected;
        EveMapperRealTime.UserDisconnected -= OnUserDisconnected;
        EveMapperRealTime.UserOnMapConnected -= OnMapPresenceChanged;
        EveMapperRealTime.UserOnMapDisconnected -= OnMapPresenceChanged;
        EveMapperUserManagementService.CurrentMapChanged -= OnCurrentMapChanged;
        EveMapperUserManagementService.PrimaryAccountChanged -= OnPrimaryAccountChanged;

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
