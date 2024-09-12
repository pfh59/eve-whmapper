using System.Collections.Frozen;
using Blazor.Diagrams.Core.Layers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Pages.Mapper.Search;

namespace WHMapper.Pages.Mapper.RoutePlanner
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {
        private const string MSG_ADD_ROUTE_DB_ERROR = "Add route db error";

        private bool _loading =true;
        private IEnumerable<EveRoute>? _myRoutes = null;
        private IEnumerable<EveRoute>? _globalRoutes = null;

        private EveRoute? _showedRoute = null!;

        private RouteType _routeType = RouteType.Shortest;
        private RouteType RType {
            get
            {
                return _routeType;
            }
            set
            {
                _routeType=value;
                Task.Run(() => Restore());
            }
        }
        


        private bool _isEditable = false;

        [Inject]
        private IEveMapperRoutePlannerHelper EveMapperRoutePlannerHelper { get; set; } = null!;

        
        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;


        [Inject]
        private ILogger<Overview> _logger {get;set;} = null!;

        [Parameter]
        public int? CurrentMapId {  get; set; } = null!;

        [Parameter]
        public EveSystemNodeModel CurrentSystemNode {get;set;} = null!;
    

        [Parameter]
        public LinkLayer CurrentLinks { get; set; } = null!;

        protected override Task OnParametersSetAsync()
        {

            if(CurrentMapId.HasValue)
            {
                Task.Run(() => Restore());
            }
            return base.OnParametersSetAsync();
        }

        private async Task Restore()
        {
            try
            {
                var currentSystem = CurrentSystemNode;
                var currentLinks = CurrentLinks;
                if (currentSystem != null && currentLinks!=null && CurrentMapId.HasValue)
                {
                    _loading=true;
                    await InvokeAsync(() => {
                        StateHasChanged();
                    });
                    FrozenSet<RouteConnection> mapConnections = null!;
                    if(currentLinks.Count() > 0)
                    {
                        var  mapConnectionsSens1 = currentLinks.Select(x=> new RouteConnection(
                                ((EveSystemNodeModel)x.Source!.Model!).SolarSystemId,((EveSystemNodeModel)x.Source!.Model!).SecurityStatus,
                                ((EveSystemNodeModel)x.Target!.Model!).SolarSystemId,((EveSystemNodeModel)x.Target!.Model!).SecurityStatus
                            )).ToList();

                        var  mapConnectionsSens2 = currentLinks.Select(x=> new RouteConnection(
                                ((EveSystemNodeModel)x.Target!.Model!).SolarSystemId,((EveSystemNodeModel)x.Target!.Model!).SecurityStatus,
                                ((EveSystemNodeModel)x.Source!.Model!).SolarSystemId,((EveSystemNodeModel)x.Source!.Model!).SecurityStatus
                                
                        )).ToList();
                        
                        mapConnections= mapConnectionsSens1.Concat(mapConnectionsSens2).ToFrozenSet();
                    }


                    _logger.LogInformation("Restore routes from {0}", currentSystem.Name);
                    var globalRouteTmp= await EveMapperRoutePlannerHelper.GetRoutesForAll(CurrentMapId.Value,currentSystem.SolarSystemId,RType,mapConnections);
                    var myRoutesTmp= await EveMapperRoutePlannerHelper.GetMyRoutes(CurrentMapId.Value,currentSystem.SolarSystemId,RType,mapConnections);

                    if(_showedRoute!=null)
                    {
                        var globalRouteShowed = globalRouteTmp?.Where(x=>x.Id==_showedRoute.Id).FirstOrDefault();
                        var myRouteShowed = myRoutesTmp?.Where(x=>x.Id==_showedRoute.Id).FirstOrDefault();

                        await ShowRoute(_showedRoute,false);
                        
                        _globalRoutes = globalRouteTmp;
                        _myRoutes = myRoutesTmp;
                    

                        if(globalRouteShowed!=null && globalRouteShowed.IsAvailable)
                            await ShowRoute(globalRouteShowed,true);
                        

                        if(myRouteShowed!=null && myRouteShowed.IsAvailable)
                            await ShowRoute(myRouteShowed,true);
                        
                    }
                    else
                    {
                        _globalRoutes = globalRouteTmp;
                        _myRoutes = myRoutesTmp;
                    }

    
                    await InvokeAsync(() => {
                        _loading=false;
                        StateHasChanged();
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Error on restore routes");
                 await InvokeAsync(() => {
                    _loading=false;
                    StateHasChanged();
                });
            }
        }

        private async Task AddRoute(bool global)
        {
            if(!CurrentMapId.HasValue)
            {
                _logger.LogError("No map selected");
                Snackbar.Add("No map selected", Severity.Error);
                return;
            }

            DialogOptions disableBackdropClick = new DialogOptions()
            {
                BackdropClick=false,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters();

            var dialog = await DialogService.ShowAsync<SearchSystem>("Search and Add Route Dialog", parameters, disableBackdropClick);
            DialogResult? result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data != null)
            {
                if (CurrentMapId.HasValue  && result.Data!=null)
                {
                    SystemEntity solarSystem = (SystemEntity)result.Data;
                    var route = await EveMapperRoutePlannerHelper.AddRoute(CurrentMapId.Value,solarSystem.Id, global);

                    if (route == null)
                    {
                        _logger.LogError(MSG_ADD_ROUTE_DB_ERROR);
                        Snackbar?.Add(MSG_ADD_ROUTE_DB_ERROR, Severity.Error);
                        return;
                    }
                    else
                    {
                        Snackbar?.Add(String.Format("{0} route successfully added",solarSystem.Name), Severity.Success);
                        await Restore();
                    }
                }
                else
                {
                    _logger.LogError("OpenSearchAndAddDialog, unable to find selected map to notify wormhole added");
                    Snackbar?.Add("Unable to find selected map to notify wormhole added", Severity.Warning);
                }
            }
        }

        private async Task DelRoute(EveRoute route)
        {
            var parameters = new DialogParameters();
            parameters.Add("RouteId", route.Id);
            var dialog = await DialogService.ShowAsync<Delete>("Delete Route", parameters);
            DialogResult? result = await dialog.Result;
            
            if (result != null && !result.Canceled)
            {
                await ShowRoute(route,false);
                await Restore();

            }

            _isEditable=false;
            StateHasChanged();
            
        }

        private async Task Edit()
        {
            _isEditable = !_isEditable;
            await Task.CompletedTask;
        }


        private async Task ToggleShowRoute(EveRoute route)
        {
            //search if route already showed
            
            var routeShowed = ((_globalRoutes==null) ? null : _globalRoutes.Where(x=>x.IsShowed==true).FirstOrDefault());
            if(routeShowed!=null && (routeShowed.Id!=route.Id))
            {
                    await ShowRoute(routeShowed,false);
            }
            else
            {
                routeShowed = ((_myRoutes==null) ? null : _myRoutes.Where(x=>x.IsShowed==true).FirstOrDefault());
    
                if(routeShowed!=null && (routeShowed.Id!=route.Id))
                {

                    await ShowRoute(routeShowed,false);
                }
            }

            await ShowRoute(route,!route.IsShowed);
        }

        private Task ShowRoute(EveRoute route,bool show)
        {          
            if(route==null)
            {
                _showedRoute=null!;
                return Task.CompletedTask;
            }
                
            route.IsShowed=show;
            var linkOnRoute = CurrentLinks.Where(x => route.Route!.Contains(((EveSystemNodeModel)x.Source!.Model!).SolarSystemId) && route.Route!.Contains(((EveSystemNodeModel)x.Target!.Model!).SolarSystemId));
            foreach(var link in linkOnRoute)
            {
                if(link!=null)
                {
                    ((EveSystemLinkModel)link).IsRouteWaypoint=show;
                    ((EveSystemNodeModel)link.Source!.Model!).IsRouteWaypoint=show;
                    ((EveSystemNodeModel)link.Target!.Model!).IsRouteWaypoint=show;

                    ((EveSystemLinkModel)link).Refresh();
                    ((EveSystemNodeModel)link.Source.Model).Refresh();
                    ((EveSystemNodeModel)link.Target.Model).Refresh();
                }
            }
            if(show)
            {
                _showedRoute=route;
            }
            else
            {
                _showedRoute=null!;
            }
            return Task.CompletedTask;
        }
    }
}
