using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class InstancesDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private ILogger<InstancesDialog> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private ICharacterServices CharacterServices { get; set; } = null!;

    [Inject]
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private bool _loading = true;
    private int _characterId = 0;
    private IEnumerable<WHInstance>? _instances;

    protected override async Task OnInitializedAsync()
    {
        await LoadInstancesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadInstancesAsync()
    {
        _loading = true;

        try
        {
            if (string.IsNullOrEmpty(UID.ClientId))
            {
                Logger.LogWarning("ClientId is null");
                return;
            }

            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount == null)
            {
                Logger.LogWarning("Could not get primary account");
                return;
            }
            _characterId = primaryAccount.Id;

            var characterResult = await CharacterServices.GetCharacter(_characterId);
            if (!characterResult.IsSuccess || characterResult.Data == null)
            {
                Logger.LogWarning("Could not get character info");
                return;
            }

            var character = characterResult.Data;
            _instances = await InstanceService.GetAccessibleInstancesAsync(
                _characterId,
                character.CorporationId > 0 ? character.CorporationId : null,
                character.AllianceId > 0 ? character.AllianceId : null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading instances");
            Snackbar.Add("Error loading instances", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private bool IsAdmin(WHInstance instance)
    {
        return instance.Administrators?.Any(a => a.EveCharacterId == _characterId) == true;
    }

    private string GetOwnerTypeIcon(WHAccessEntity ownerType)
    {
        return ownerType switch
        {
            WHAccessEntity.Character => "\ud83d\udc64",
            WHAccessEntity.Corporation => "\ud83c\udfe2",
            WHAccessEntity.Alliance => "\ud83c\udf10",
            _ => ""
        };
    }

    private async Task ManageInstance(int instanceId)
    {
        var parameters = new DialogParameters<AdminInstanceDialog>
        {
            { x => x.InstanceId, instanceId }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = true
        };

        var dialog = await DialogService.ShowAsync<AdminInstanceDialog>("Manage Instance", parameters, options);
        await dialog.Result;
        
        // Reload instances after admin dialog closes in case of changes
        await LoadInstancesAsync();

        // Auto-close if no instances remain (e.g. last instance was deleted)
        if (_instances?.Any() != true)
        {
            MudDialog.Close();
            return;
        }

        StateHasChanged();
    }

    private async Task CreateNewInstance()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = true
        };

        var dialog = await DialogService.ShowAsync<RegisterInstanceDialog>("Create Instance", options);
        await dialog.Result;
        
        // Reload instances after creation dialog closes
        await LoadInstancesAsync();
        StateHasChanged();
    }

    private void Close() => MudDialog.Cancel();
}
