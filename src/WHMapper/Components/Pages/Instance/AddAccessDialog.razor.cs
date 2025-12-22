using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class AddAccessDialog : ComponentBase
{
    private MudForm _form = null!;
    private bool _formIsValid = false;
    private bool _adding = false;
    
    private AEveEntity? _selectedEntity;
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
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var characterIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(characterIdClaim, out _characterId);
    }

    private void Cancel() => MudDialog.Cancel();

    private async Task<IEnumerable<AEveEntity>> SearchEntity(string? value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
            return Enumerable.Empty<AEveEntity>();

        try
        {
            var results = await EveMapperSearch.SearchEveEntities(value, cancellationToken);
            return results ?? Enumerable.Empty<AEveEntity>();
        }
        catch
        {
            return Enumerable.Empty<AEveEntity>();
        }
    }

    private async Task Add()
    {
        if (_selectedEntity == null || _adding || _characterId <= 0)
            return;

        _adding = true;

        try
        {
            WHAccessEntity entityType = _selectedEntity.EntityType switch
            {
                EveEntityEnums.Character => WHAccessEntity.Character,
                EveEntityEnums.Corporation => WHAccessEntity.Corporation,
                EveEntityEnums.Alliance => WHAccessEntity.Alliance,
                _ => WHAccessEntity.Character
            };

            var result = await InstanceService.AddAccessAsync(
                InstanceId, 
                _selectedEntity.Id, 
                _selectedEntity.Name, 
                entityType,
                _characterId);
            
            if (result != null)
            {
                Snackbar.Add($"Granted access to {_selectedEntity.Name}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to grant access", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error granting access", Severity.Error);
        }
        finally
        {
            _adding = false;
        }
    }
}
