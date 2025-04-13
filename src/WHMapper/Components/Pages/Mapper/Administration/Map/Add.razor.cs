using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Components.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Admin")]
public partial class Add
{
    private MudForm _form = null!;
    private bool _success = false;

    private string _mapName = null!;

    [Inject]
    private ILogger<Add> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHMapRepository DbMap {get;set;}=null!;

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; } = null!;



    private async Task Submit()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            // Check if map name already exists
            var existingMap = await DbMap.GetByNameAsync(_mapName);
            if (existingMap != null)
            {
                Snackbar.Add("Map name already exists. Please choose a different name.", Severity.Warning);
                return;
            }

            WHMap newMap = new WHMap(_mapName);
            if(await DbMap.Create(newMap) == null)
            {
                Snackbar.Add("Error while creating map", Severity.Error);
                MudDialog.Close(DialogResult.Cancel);
            }
            else
            {   
                MudDialog.Close(DialogResult.Ok(newMap));
            }
        }
        else
        {
            Logger.LogError("Error while validating form");
            Snackbar?.Add("Error while validating form", Severity.Error);
            MudDialog.Close(DialogResult.Cancel);
        }
    }
            

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
