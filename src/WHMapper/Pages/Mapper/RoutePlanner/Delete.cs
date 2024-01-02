using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WHMapper.Pages.Mapper.RoutePlanner
{

    [Authorize(Policy = "Access")]
    public partial class Delete: Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string MSG_DELETE_ROUTE = "Do you want to delete this route?";
        private const string MSG_ROUTE_DEL_SUCCESS = "Route successfully deleted";
        private const string MSG_ROUTE_DEL_FAIL = "Route not deleted";

        [Inject]
        private IEveMapperRoutePlannerHelper EveMapperRoutePlannerHelper { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public ILogger<Delete> Logger { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;


        [Parameter] 
        public int RouteId { get; set; }


        private void Cancel()
        {
            MudDialog?.Cancel();
        }

        private async Task Submit()
        {
            if (RouteId > 0)
                await DeleteRoute();
        }

        private async Task DeleteRoute()
        {
            if (RouteId > 0)
            {
                var result = await EveMapperRoutePlannerHelper.DeleteRoute(RouteId);
                if (result)
                {
                    Snackbar.Add(MSG_ROUTE_DEL_SUCCESS, Severity.Success);
                    MudDialog?.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add(MSG_ROUTE_DEL_FAIL, Severity.Error);
                    MudDialog?.Close(DialogResult.Ok(false));
                }
            }
        }



  

        
    }
}
