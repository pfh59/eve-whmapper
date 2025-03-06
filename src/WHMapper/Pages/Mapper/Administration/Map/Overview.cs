using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.DTO.MapAdmin;
using System.Linq;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Services.EveMapper;

namespace WHMapper.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Admin")]
public partial class Overview : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public ILogger<Overview> Logger { get; set; } = null!;

    [Inject]
    private IWHMapRepository DbWHMap { get; set; } = null!;
    [Inject]
    private IWHAccessRepository DbWHAccess { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService EveMapperRealTimeService { get; set; } = null!;

    private IList<MapAdmin>? Maps { get; set; } = null;
    private MapAdmin _selectedMap = null!;
    private WHAccess _selectedWHAccess = null!;

    private IEnumerable<WHAccess>? WHAccesses { get; set; } = null;

 
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if(EveMapperRealTimeService!=null)
        {
            //await EveMapperRealTimeService.Start();
        }
    }

    



    protected override async Task OnParametersSetAsync()
    {
        await Restore();
        await base.OnParametersSetAsync();
    }

    private async Task Restore()
    {
        if (DbWHMap == null)
        {
            Logger.LogError("DbWHMap is null");
            return;
        }

        WHAccesses = await DbWHAccess.GetAll();

        var maps = await DbWHMap.GetAll();
        Maps = maps?.Select(m => new MapAdmin(m)).ToList();
    }

    private Task ShowMapAccess(int mapId)
    {
        if (Maps == null)
        {
            Logger.LogError("Maps is null");
            Snackbar.Add("Maps is null", Severity.Error);
            return Task.CompletedTask;
        }

        var tmpMapAdmin = Maps.FirstOrDefault(x => x.Id == mapId);
        if (tmpMapAdmin != null)
        {
            tmpMapAdmin.ShowAccessDetails = !tmpMapAdmin.ShowAccessDetails;
        }

        return Task.CompletedTask;
    }

    private async Task OpenAddMap()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<Add>("Add Map", new DialogParameters(), options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            var newMap = result.Data as WHMap;
            if (newMap == null)
            {
                Logger.LogError("newMap is null");
                Snackbar.Add("newMap is null", Severity.Error);
                return;
            }
            if(Maps == null)
            {
                Maps=new List<MapAdmin>();
            }
            
            Maps.Add(new MapAdmin(newMap));
           // EveMapperRealTimeService?.NotifyMapAdded(newMap.Id);
        }
    }

    private async Task DeleteAllMaps()
    {
        if (Maps == null)
        {
            Logger.LogError("Maps is null");
            Snackbar.Add("Maps is null", Severity.Error);
            return;
        }

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<Delete>("Delete All Maps", new DialogParameters(), options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            Maps.Clear();
            //EveMapperRealTimeService?.NotifyAllMapsRemoved();
        }
    }

    private async Task DeleteMap(int mapId)
    {
        if (Maps == null)
        {
            Logger.LogError("Maps is null");
            return;
        }

        var parameters = new DialogParameters { { "MapId", mapId } };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<Delete>("Delete Map", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            ((List<MapAdmin>)Maps).RemoveAll(x => x.Id == mapId);
            //EveMapperRealTimeService?.NotifyMapRemoved(mapId);
        }
    }

    private async Task OpenAddAccess(int mapId)
    {
        if (Maps == null)
        {
            Logger.LogError("Maps is null");
            return;
        }

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var parameters = new DialogParameters { { "MapId", mapId} };
        var dialog = await DialogService.ShowAsync<AddAccess>("Map Access Manager", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            List<WHAccess> accesses = result.Data as List<WHAccess> ?? new List<WHAccess>();
            
            MapAdmin? map = Maps.FirstOrDefault(x => x.Id == mapId);
            if (map == null)
            {
                Logger.LogError("map is null");
                Snackbar.Add("map is null", Severity.Error);
                return;
            }

            foreach (var access in accesses)
            {
                if (map.WHMapAccesses is HashSet<WHAccess> accessList)
                {
                    accessList.Add(access);
                }
                else
                {
                    Logger.LogError("WHMapAccesses is not a List<WHAccess>");
                    Snackbar.Add("WHMapAccesses is not a List<WHAccess>", Severity.Error);
                }
            }

           // EveMapperRealTimeService?.NotifyMapAccessesAdded(mapId, accesses.Select(x => x.Id).ToList());
        }
    }

    private async Task RemoveAccess(int mapId,int accessId)
    {

        var parameters = new DialogParameters { {"AccessId", accessId },{ "MapId",mapId } };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<RemoveAccess>("Remove Access", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            MapAdmin map = Maps.FirstOrDefault(x => x.Id == mapId);
            if (map == null)
            {
                Logger.LogError("map is null");
                Snackbar.Add("map is null", Severity.Error);
                return;
            }

            if (map.WHMapAccesses is HashSet<WHAccess> accessList)
            {
                accessList.RemoveWhere(x => x.Id == accessId);
            }
            else
            {
                Logger.LogError("WHMapAccesses is not a List<WHAccess>");
                Snackbar.Add("WHMapAccesses is not a List<WHAccess>", Severity.Error);
            }

            //EveMapperRealTimeService?.NotifyMapAccessRemoved(mapId, accessId);
        }
    }

    private async Task RemoveAllAccesses(int mapId)
    {
        var parameters = new DialogParameters { { "MapId", mapId } };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<RemoveAccess>("Remove All Accesses", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            MapAdmin? map = Maps.FirstOrDefault(x => x.Id == mapId);
            if (map == null)
            {
                Logger.LogError("map is null");
                Snackbar.Add("map is null", Severity.Error);
                return;
            }
            if (map.WHMapAccesses is HashSet<WHAccess> accessList)
            {
                accessList.Clear();
            }
            else
            {
                Logger.LogError("WHMapAccesses is not a List<WHAccess>");
                Snackbar.Add("WHMapAccesses is not a List<WHAccess>", Severity.Error);
            }
            
           // EveMapperRealTimeService?.NotifyMapAllAccessesRemoved(mapId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (EveMapperRealTimeService != null)
        {
            //await EveMapperRealTimeService.Stop();
            await EveMapperRealTimeService.DisposeAsync();
        }
    }
}
