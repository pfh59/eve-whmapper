using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class EditInstanceDialog : ComponentBase
{
    private bool _formIsValid = false;
    private bool _saving = false;
    
    private string? _name;
    private string? _description;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public WHInstance Instance { get; set; } = null!;

    /// <summary>
    /// Character performing the edit; must be an administrator of the instance.
    /// </summary>
    [Parameter]
    public int RequestingCharacterId { get; set; }

    [Inject]
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override void OnInitialized()
    {
        _name = Instance.Name;
        _description = Instance.Description;
    }

    private string NormalizedName => _name?.Trim() ?? string.Empty;

    private string? NormalizedDescription => string.IsNullOrWhiteSpace(_description) ? null : _description.Trim();

    private bool HasChanges =>
        NormalizedName != Instance.Name || NormalizedDescription != Instance.Description;

    private void Cancel() => MudDialog.Cancel();

    private async Task Save()
    {
        if (!_formIsValid || _saving || !HasChanges)
            return;

        _saving = true;

        try
        {
            var result = await InstanceService.UpdateInstanceAsync(
                Instance.Id, NormalizedName, NormalizedDescription, RequestingCharacterId);
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
