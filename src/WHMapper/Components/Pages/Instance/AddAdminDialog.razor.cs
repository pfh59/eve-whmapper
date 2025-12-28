using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class AddAdminDialog : ComponentBase
{
    private MudForm _form = null!;
    private bool _formIsValid = false;
    private bool _adding = false;
    
    private CharactereEntity? _selectedCharacter;
    private int _characterId = 0;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int InstanceId { get; set; }

    [Inject]
    private IWHInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private IEveMapperSearch EveMapperSearch { get; set; } = null!;

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

    private async Task<IEnumerable<CharactereEntity>> SearchCharacter(string? value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
            return Enumerable.Empty<CharactereEntity>();

        try
        {
            var results = await EveMapperSearch.SearchCharactere(value, cancellationToken);
            return results ?? Enumerable.Empty<CharactereEntity>();
        }
        catch
        {
            return Enumerable.Empty<CharactereEntity>();
        }
    }

    private async Task Add()
    {
        if (_selectedCharacter == null || _adding || _characterId <= 0)
            return;

        _adding = true;

        try
        {
            var result = await InstanceService.AddAdminAsync(
                InstanceId, 
                _selectedCharacter.Id, 
                _selectedCharacter.Name, 
                _characterId);
            
            if (result != null)
            {
                Snackbar.Add($"Added {_selectedCharacter.Name} as administrator", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to add administrator", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error adding administrator", Severity.Error);
        }
        finally
        {
            _adding = false;
        }
    }
}
