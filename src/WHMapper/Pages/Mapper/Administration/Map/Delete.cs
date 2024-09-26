using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Access")]
public partial class Delete : ComponentBase
{
    private const string MSG_DELETE_MAP = "Do you really want to delete this map?";
    private const string MSG_DELETE_MAPS = "Do you really want to delete all maps?";

    [Inject]
    private ILogger<Delete> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHMapRepository DbWHMap { get; set; } = null!;

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int? MapId { get; set; }

    private async Task Submit()
    {
        if (!MapId.HasValue)
        {
            await DeleteMaps();
        }
        else if (MapId.Value > 0)
        {
            await DeleteMap();
        }
        else
        {
            LogError("Bad map parameters");
        }
    }

    private async Task DeleteMap()
    {
        if (DbWHMap == null)
        {
            LogError("DbWHMap is null");
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        if (MapId.HasValue && MapId.Value > 0)
        {
            bool isDeleted = await DbWHMap.DeleteById(MapId.Value);
            if (isDeleted)
            {
                LogSuccess("Map is successfully deleted");
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                LogError("No Map deleted");
                MudDialog.Close(DialogResult.Ok(false));
            }
        }
        else
        {
            LogError("Bad map parameters");
            MudDialog.Close(DialogResult.Ok(false));
        }
    }

    private async Task DeleteMaps()
    {
        if (DbWHMap == null)
        {
            LogError("DbWHMap is null");
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        bool result = await DbWHMap.DeleteAll();
        if (result)
        {
            LogSuccess("All Maps are successfully deleted");
             MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            LogError("No Map deleted");
             MudDialog.Close(DialogResult.Ok(false));
        }
       
    }

    private void Cancel()
    {
        MudDialog?.Cancel();
    }

    private void LogError(string message)
    {
        Logger.LogError(message);
        Snackbar.Add(message, Severity.Error);
    }

    private void LogSuccess(string message)
    {
        Logger.LogInformation(message);
        Snackbar.Add(message, Severity.Success);
    }
}
