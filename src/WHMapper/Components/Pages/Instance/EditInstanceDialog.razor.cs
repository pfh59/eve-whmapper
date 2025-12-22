using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class EditInstanceDialog : ComponentBase
{
    private MudForm _form = null!;
    private bool _formIsValid = false;
    private bool _saving = false;
    
    private string _name = string.Empty;
    private string? _description;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public WHInstance Instance { get; set; } = null!;

    [Inject]
    private IWHInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override void OnInitialized()
    {
        _name = Instance.Name;
        _description = Instance.Description;
    }

    private void Cancel() => MudDialog.Cancel();

    private async Task Save()
    {
        if (!_formIsValid || _saving)
            return;

        _saving = true;

        try
        {
            var result = await InstanceService.UpdateInstanceAsync(Instance.Id, _name, _description);
            if (result != null)
            {
                Snackbar.Add("Instance updated successfully", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to update instance", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error updating instance", Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }
}
