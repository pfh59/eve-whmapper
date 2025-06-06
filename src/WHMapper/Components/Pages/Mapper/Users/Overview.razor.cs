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

    public IEnumerable<WHMapperUser> Accounts { get; private set; } = new List<WHMapperUser>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if(EveMapperUserManagementService != null && UID !=null && !String.IsNullOrEmpty(UID.ClientId))
            {
                Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
                foreach (var account in Accounts)
                {
                    await EveMapperRealTime.Start(account.Id);
                }
            }

            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
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
        foreach (var account in Accounts)
        {
            await EveMapperRealTime.Stop(account.Id);
            await TrackerServices.StopTracking(account.Id);
        }
    }
}

