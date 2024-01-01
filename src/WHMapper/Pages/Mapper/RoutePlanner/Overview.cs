
using Blazor.Diagrams.Core.Layers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.EveAPI;

namespace WHMapper.Pages.Mapper.RoutePlanner
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {
        private IEnumerable<EveRoute>? _myRoutes = null;
        private IEnumerable<EveRoute>? _globalRoutes = null;

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
        public EveSystemNodeModel CurrentSystemNode { get; set; } = null!;

        [Parameter]
        public LinkLayer CurrentLinks { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            _logger.LogInformation("OnInitializedAsync");
        }

        protected async override Task OnParametersSetAsync()
        {
            await Restore();
        }

        private async Task Restore()
        {
            if (CurrentSystemNode != null && CurrentLinks!=null)
            {
                int[][]? mapConnections = null;
                if(CurrentLinks.Count() > 0)
                {
                    int[][]  mapConnectionsSens1 = CurrentLinks.Select(x => new int [2] {((EveSystemNodeModel)x.Source.Model).SolarSystemId,((EveSystemNodeModel)x.Target.Model).SolarSystemId}).ToArray<int[]>();
                    int[][]  mapConnectionsSens2 = CurrentLinks.Select(x => new int [2] {((EveSystemNodeModel)x.Target.Model).SolarSystemId,((EveSystemNodeModel)x.Source.Model).SolarSystemId}).ToArray<int[]>();
                    mapConnections= mapConnectionsSens1.Concat(mapConnectionsSens2).ToArray<int[]>(); 

                }


                _logger.LogInformation("Restore routes from {0}", CurrentSystemNode.Name);
                _myRoutes = await EveMapperRoutePlannerHelper.GetMyRoutes(CurrentSystemNode.SolarSystemId,mapConnections);
                _globalRoutes = await EveMapperRoutePlannerHelper.GetRoutesForAll(CurrentSystemNode.SolarSystemId,mapConnections);
            }
            else
            {
                _logger.LogInformation("CurrentSystemNode is null");
            }
        }

        private async Task AddRoute()
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                DisableBackdropClick = true,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters();
            //parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId);

            var dialog = DialogService.Show<Add>("Add Route", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Canceled)
            {
                await Restore();
            }
        }

        private async Task DelRoute(EveRoute route)
        {
            var parameters = new DialogParameters();
            parameters.Add("RouteId", route.Id);
            var dialog = DialogService.Show<Delete>("Delete Route", parameters);
            DialogResult result = await dialog.Result;
            
            if (!result.Canceled)
            {
                ShowRoute(route,false);
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
            var routeShowed = _globalRoutes.Where(x=>x.IsShowed==true).FirstOrDefault();
            if(routeShowed!=null && (routeShowed.Id!=route.Id))
            {
                    await ShowRoute(routeShowed,false);
            }
            else
            {
                routeShowed = _myRoutes.Where(x=>x.IsShowed==true).FirstOrDefault();
                if(routeShowed!=null && (routeShowed.Id!=route.Id))
                {

                    await ShowRoute(routeShowed,false);
                }
            }

            await ShowRoute(route,!route.IsShowed);
        }

        private async Task ShowRoute(EveRoute route,bool show)
        {          
            route.IsShowed=show;
            var linkOnRoute = CurrentLinks.Where(x => route.Route.Contains(((EveSystemNodeModel)x.Source.Model).SolarSystemId) && route.Route.Contains(((EveSystemNodeModel)x.Target.Model).SolarSystemId));
            foreach(var link in linkOnRoute)
            {
                ((EveSystemLinkModel)link).IsRouteWaypoint=show;
                ((EveSystemNodeModel)link.Source.Model).IsRouteWaypoint=show;
                ((EveSystemNodeModel)link.Target.Model).IsRouteWaypoint=show;

                ((EveSystemLinkModel)link).Refresh();
                ((EveSystemNodeModel)link.Source.Model).Refresh();
                ((EveSystemNodeModel)link.Target.Model).Refresh();
            }
        }
    }
}
