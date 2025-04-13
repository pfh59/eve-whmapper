using Microsoft.AspNetCore.Components;
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
                    await TrackerServices.StartTracking(account.Id);
                }
            }

            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private Task ToggleTracking(int accountId)
    {
        var account = Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null)
        {
            return Task.CompletedTask;
        }

        account.Tracking = !account.Tracking;
        StateHasChanged();
        return Task.CompletedTask;
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

