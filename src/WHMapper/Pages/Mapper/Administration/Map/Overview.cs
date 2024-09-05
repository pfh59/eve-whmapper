using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.DTO.MapAdmin;

namespace WHMapper.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Admin")]
public partial class Overview : ComponentBase
{

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public ILogger<Overview> Logger { get; set; } = null!;

    [Inject]
    private IWHMapRepository DbWHMap { get; set; } = null!;

    //[Inject]
   // private IWHAccessRepository DbWHAccess { get; set; } = null!;

    private IEnumerable<MapAdmin>? Maps { get; set; } = null;
    private MapAdmin _selectedMap = null!;

    //private IEnumerable<WHAccess>? WHAccesses { get; set; } = null;
    private WHAccess _selectedWHAccess = null!;
    

    private MudForm _formMap = null!;
    private bool _successMap = false;

    protected override async Task OnParametersSetAsync()
    {
        await Restore();
        await base.OnParametersSetAsync();
    }


    private async Task Restore()
    {
        if(DbWHMap == null)
        {
            Logger.LogError("DbWHMap is null");
            return;
        }

        var maps = await DbWHMap.GetAll();
        Maps = maps?.Select(m => new MapAdmin(m));
    }

    private Task ShowMapAccess(int mapId)
    {
        if(Maps == null)
        {
            Logger.LogError("Maps is null");
            return Task.CompletedTask;
        }

	    MapAdmin tmpMapAdmin = Maps.First(x => x.Id == mapId);
		tmpMapAdmin.ShowAccessDetails = !tmpMapAdmin.ShowAccessDetails;

        return Task.CompletedTask;
    }

    private async Task OpenAddMap()
    {
        
        if(DbWHMap == null)
        {
            Logger.LogError("DbWHMap is null");
            return;
        }

        var map = new WHMap("New Map "+Guid.NewGuid().ToString());
        map = await DbWHMap.Create(map);
        
    }

    private async Task DeleteAllMaps()
    {

    }




}
