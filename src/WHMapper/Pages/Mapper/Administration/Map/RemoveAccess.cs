
using WHMapper.Repositories.WHMaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WHMapper.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Access")]
public partial class RemoveAccess : ComponentBase
{
    private const string MSG_DELETE_ACCESS = "Do you really want to delete this access?";
    private const string MSG_DELETE_ACCESSES = "Do you really want to delete all accesses?";

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

    [Parameter]
    public int? AccessId { get; set; }


    private async Task Submit()
    {

        if(AccessId.HasValue && MapId.HasValue)
        {
            await DeleteMapAccess();
        }
        else if (!AccessId.HasValue && MapId.HasValue)
        {
            await DeleteMapAccesses();
        }
        else
        {
            LogError("Bad map parameters");
            Snackbar.Add("Bad map parameters", Severity.Error);
        }
    }

    private async Task DeleteMapAccess()
    {
        try
        {
            if(!MapId.HasValue || !AccessId.HasValue)
            {
                LogError("Bad map parameters");
                Snackbar.Add("Bad map parameters", Severity.Error);
                return;
            }

            var result = await DbWHMap.DeleteMapAccess(MapId.Value, AccessId.Value);
            if (result)
            {
                LogSuccess("Map access removed");
                Snackbar.Add("Map access removed", Severity.Success);
                MudDialog?.Close(DialogResult.Ok(AccessId.Value));
            }
            else
            {
                LogError("Map access not removed");
                Snackbar.Add("Map access not removed", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            LogError("Error removing map access");
            Snackbar.Add("Error removing map access", Severity.Error);
        }
    }

    private async Task DeleteMapAccesses()
    {
        try
        {
            if (!MapId.HasValue)
            {
                LogError("Bad map parameters");
                Snackbar.Add("Bad map parameters", Severity.Error);
                return;
            }

            var result = await DbWHMap.DeleteMapAccesses(MapId.Value);
            if (result)
            {
                LogSuccess("Map accesses removed");
                Snackbar.Add("Map accesses removed", Severity.Success);
                MudDialog?.Close(DialogResult.Ok(true));
            }
            else
            {
                LogError("Map accesses not removed");
                Snackbar.Add("Map accesses not removed", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            LogError("Error removing map accesses");
            Snackbar.Add("Error removing map accesses", Severity.Error);
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