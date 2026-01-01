using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class AddMapDialog : ComponentBase
{
    private MudForm _form = null!;
    private bool _formIsValid = false;
    private bool _creating = false;
    
    private string _mapName = string.Empty;
    private int _characterId = 0;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int InstanceId { get; set; }

    [Inject]
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(UID.ClientId))
        {
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount != null)
            {
                _characterId = primaryAccount.Id;
            }
        }
    }

    private void Cancel() => MudDialog.Cancel();

    private async Task Create()
    {
        if (!_formIsValid || _creating || _characterId <= 0)
            return;

        _creating = true;

        try
        {
            var result = await InstanceService.CreateMapAsync(InstanceId, _mapName, _characterId);
            if (result != null)
            {
                Snackbar.Add("Map created successfully", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to create map", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error creating map", Severity.Error);
        }
        finally
        {
            _creating = false;
        }
    }
}
