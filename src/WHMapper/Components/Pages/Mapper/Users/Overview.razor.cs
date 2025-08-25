using Microsoft.AspNetCore.Components;
using Mono.TextTemplating;
using System;
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

    public IEnumerable<WHMapperUser> Accounts { get; private set; } = new List<WHMapperUser>();

    private CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
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
            Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
            foreach (var account in Accounts)
            {
                await EveMapperRealTime.Start(account.Id);
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
            await EveMapperUserManagementService.SetPrimaryAccountAsync(UID.ClientId, accountId.ToString());
            StateHasChanged();
        }
    }


    public async ValueTask DisposeAsync()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        _cts?.Dispose();

        foreach (var account in Accounts)
        {
            await EveMapperRealTime.Stop(account.Id);
            await TrackerServices.StopTracking(account.Id);
        }
    }
}

