
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.EveAPI;

namespace WHMapper.Pages.Mapper.RoutePlanner
{
    public partial class Overview : ComponentBase
    {
        private IEnumerable<WHRoute>? _myRoutes = null;
        private IEnumerable<WHRoute>? _globalRoutes = null;

        [Inject]
        private IEveMapperRoutePlannerHelper EveMapperRoutePlannerHelper { get; set; } = null!;

        [Inject]
        private  IEveAPIServices EveAPIService { get; set; } = null!;
        
        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;


        [Inject]
        private ILogger<Overview> _logger {get;set;} = null!;


        [Parameter]
        public EveSystemNodeModel CurrentSystemNode { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            _logger.LogInformation("OnInitializedAsync");
        }

        protected async override Task OnParametersSetAsync()
        {

            if (CurrentSystemNode != null)
            {
                await Restore();
            }
        }

        private async Task Restore()
        {
            _logger.LogInformation("LoadRoutes");
            _myRoutes = await EveMapperRoutePlannerHelper.GetMyRoutes();
            _globalRoutes = await EveMapperRoutePlannerHelper.GetRoutesForAll();
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
                //await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
        }
    }
}
