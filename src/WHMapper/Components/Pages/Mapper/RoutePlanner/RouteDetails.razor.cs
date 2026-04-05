using System;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHColor;

namespace WHMapper.Components.Pages.Mapper.RoutePlanner;


public partial class RouteDetails
{
    [Parameter]
    public int[]? RouteSystems { get; set; }

    [Inject]
    private IWHColorHelper WHColorHelper { get; set; } = null!;

    [Inject]
    private IEveMapperService EveMapperService { get; set; } = null!;

    private List<RouteSystemDetail> _routeSystemDetails = new List<RouteSystemDetail>();

    public RouteDetails()
    {
    }

    protected override Task OnParametersSetAsync()
    {
        if(RouteSystems != null)
        {
            Task.Run(() => Load());
        }
        return base.OnParametersSetAsync();
    }

    private async Task Load()
    {
        List<RouteSystemDetail> details = new List<RouteSystemDetail>();
        if(RouteSystems != null)
        {
            foreach(var systemId in RouteSystems)
            {
                var systemInfo = await EveMapperService.GetSystem(systemId);
                if(systemInfo != null)
                {
                    details.Add(new RouteSystemDetail(systemId, systemInfo.Name, WHColorHelper.GetSecurityStatusColor(systemInfo.SecurityStatus)));
                }
            }
        }
        _routeSystemDetails = details;
        await InvokeAsync(StateHasChanged);
    }
}
