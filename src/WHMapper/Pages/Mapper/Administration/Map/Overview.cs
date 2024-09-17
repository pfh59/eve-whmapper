using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.DTO.MapAdmin;
using System.Linq;
using WHMapper.Repositories.WHAccesses;

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
    [Inject]
    private IWHAccessRepository DbWHAccess { get; set; } = null!;

    private IList<MapAdmin>? Maps { get; set; } = null;
    private MapAdmin _selectedMap = null!;
    private WHAccess _selectedWHAccess = null!;

    private IEnumerable<WHAccess>? WHAccesses { get; set; } = null;

   

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
            await Restore();
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
            await Restore();
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
            //notidy map deletion
            await Restore();
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
            await Restore();
        }
    }

    private async Task RemoveAccess(int accessId)
    {
        if (Maps == null)
        {
            Logger.LogError("Maps is null");
            Snackbar.Add("Maps is null", Severity.Error);
            return;
        }

/*
        var parameters = new DialogParameters { { "AccessId", accessId } };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<Delete>("Delete Access", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            //notidy access deletion
            await Restore();
        }*/
    }
}
