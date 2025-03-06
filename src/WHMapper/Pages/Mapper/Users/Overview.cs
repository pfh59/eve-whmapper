using Microsoft.AspNetCore.Components;
using System;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveMapper;
using WHMapper.Services.LocalStorage;

namespace WHMapper.Pages.Mapper.Users;

public partial class Overview : ComponentBase, IAsyncDisposable
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
        if(EveMapperUserManagementService != null && UID !=null && String.IsNullOrEmpty(UID.ClientId))
        {
            Accounts = await EveMapperUserManagementService.GetAccountsAsync(UID.ClientId);
            foreach (var account in Accounts)
            {
                await EveMapperRealTime.Start(account.Id);
                await TrackerServices.StartTracking(account.Id);
            }
        }
    }


    private Task ToggleTracking(WHMapperUser account)
    {
        account.Tracking = !account.Tracking;

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

