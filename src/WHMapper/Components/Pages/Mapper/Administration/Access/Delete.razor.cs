using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;

namespace WHMapper.Components.Pages.Mapper.Administration.Access;

[Authorize(Policy = "Admin")]
public partial class Delete : ComponentBase
{
    private const string MSG_ACCESS_DELETE = "Do you really want to delete this access?";
    private const string MSG_ACCESS_DB_NULL = "DbWHAccesses is null";
    private const string MSG_ACCESS_DELETE_SUCCESSFULL = "Access successfully deleted";
    private const string MSG_ACCESS_NO_DELETE = "No access deleted";
    private const string MSG_ACCESS_BAD_PARAMETER = "Bad access parameters";

    private const string MSG_ADMIN_DELETE= "Do you really want to delete this admin?";
    private const string MSG_ADMIN_DB_NULL = "DbWHAdmins is null";
    private const string MSG_ADMIN_DELETE_SUCCESSFULL = "Admin successfully deleted";
    private const string MSG_ADMIN_NO_DELETE = "No admin deleted";
    private const string MSG_ADMIN_BAD_PARAMETER = "Bad admin parameters";

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHAccessRepository DbWHAccesses { get; set; } = null!;

    [Inject]
    private IWHAdminRepository DbWHAdmin { get; set; } = null!;

    [Inject]
    public ILogger<Delete> Logger { get; set; } = null!;

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; } = null!;


    [Parameter]
    public int AccessId { get; set; }

    [Parameter]
    public int AdminId { get; set; }


    private async Task Submit()
    {
        if(AccessId>0)
            await DeleteAccess();

        if (AdminId > 0)
            await DeleteAdmin();
    }

    private async Task DeleteAccess()
    {
        if(DbWHAccesses == null)
        {
            Logger.LogError(MSG_ACCESS_DB_NULL);
            Snackbar.Add(MSG_ACCESS_DB_NULL, Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
        }
            

        if (AccessId > 0)
        {
            if (DbWHAccesses != null  && await DbWHAccesses.DeleteById(AccessId))
            {
                Logger.LogInformation(MSG_ACCESS_DELETE_SUCCESSFULL);
                Snackbar.Add(MSG_ACCESS_DELETE_SUCCESSFULL, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Logger.LogError(MSG_ACCESS_NO_DELETE);
                Snackbar.Add(MSG_ACCESS_NO_DELETE, Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }
        else
        {
            Logger.LogError(MSG_ACCESS_BAD_PARAMETER);
            Snackbar.Add(MSG_ACCESS_BAD_PARAMETER, Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
        }
    }

    private async Task DeleteAdmin()
    {
        if (DbWHAdmin == null)
        {
            Logger.LogError(MSG_ADMIN_BAD_PARAMETER);
            Snackbar.Add(MSG_ADMIN_BAD_PARAMETER, Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
        }


        if (AdminId > 0)
        {
            if (DbWHAdmin != null && await DbWHAdmin.DeleteById(AdminId))
            {
                Logger.LogInformation(MSG_ADMIN_DELETE_SUCCESSFULL);
                Snackbar.Add(MSG_ADMIN_DELETE_SUCCESSFULL, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Logger.LogError(MSG_ADMIN_NO_DELETE);
                Snackbar.Add(MSG_ADMIN_NO_DELETE, Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }
        else
        {
            Logger.LogError(MSG_ADMIN_BAD_PARAMETER);
            Snackbar.Add(MSG_ADMIN_BAD_PARAMETER, Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
        }
    }

    private void Cancel()
    {
        MudDialog?.Cancel();
    }
}


