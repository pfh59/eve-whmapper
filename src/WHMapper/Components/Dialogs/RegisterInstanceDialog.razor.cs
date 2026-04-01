using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Dialogs;

public partial class RegisterInstanceDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private ILogger<RegisterInstanceDialog> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private ICharacterServices CharacterServices { get; set; } = null!;

    [Inject]
    private IEveMapperService EveMapperService { get; set; } = null!;

    [Inject]
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private bool _formIsValid = false;
    private bool _loading = true;
    private bool _registering = false;
    private bool _isAuthenticated = false;
    private bool _alreadyHasInstance = false;
    private int _existingInstanceId = 0;

    private string _instanceName = string.Empty;
    private string _description = string.Empty;
    private WHAccessEntity _ownerType = WHAccessEntity.Character;

    private int _characterId = 0;
    private string _characterName = string.Empty;
    private Character? _characterInfo = null;
    private string _corporationName = string.Empty;
    private string _allianceName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserInfoAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadUserInfoAsync()
    {
        _loading = true;

        try
        {
            if (string.IsNullOrEmpty(UID.ClientId))
                return;

            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
            if (primaryAccount == null)
                return;

            _isAuthenticated = true;
            _characterId = primaryAccount.Id;

            var characterResult = await CharacterServices.GetCharacter(_characterId);
            if (characterResult.IsSuccess && characterResult.Data != null)
            {
                _characterInfo = characterResult.Data;
                _characterName = _characterInfo.Name ?? string.Empty;

                if (_characterInfo.CorporationId > 0)
                {
                    var corp = await EveMapperService.GetCorporation(_characterInfo.CorporationId);
                    _corporationName = corp?.Name ?? "Unknown Corporation";
                }

                if (_characterInfo.AllianceId > 0)
                {
                    var alliance = await EveMapperService.GetAlliance(_characterInfo.AllianceId);
                    _allianceName = alliance?.Name ?? "Unknown Alliance";
                }
            }

            var existingInstances = await InstanceService.GetAdministeredInstancesAsync(_characterId);
            if (existingInstances?.Any() == true)
            {
                _alreadyHasInstance = true;
                _existingInstanceId = existingInstances.First().Id;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user info");
            Snackbar.Add("Error loading user information", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task RegisterInstance()
    {
        if (!_formIsValid || _registering)
            return;

        _registering = true;

        try
        {
            int ownerEntityId;
            string ownerEntityName;

            switch (_ownerType)
            {
                case WHAccessEntity.Character:
                    ownerEntityId = _characterId;
                    ownerEntityName = _characterName;
                    break;
                case WHAccessEntity.Corporation:
                    if (_characterInfo == null || _characterInfo.CorporationId <= 0)
                    {
                        Snackbar.Add("Corporation information not available", Severity.Error);
                        return;
                    }
                    ownerEntityId = _characterInfo.CorporationId;
                    ownerEntityName = _corporationName;
                    break;
                case WHAccessEntity.Alliance:
                    if (_characterInfo == null || _characterInfo.AllianceId <= 0)
                    {
                        Snackbar.Add("Alliance information not available", Severity.Error);
                        return;
                    }
                    ownerEntityId = _characterInfo.AllianceId;
                    ownerEntityName = _allianceName;
                    break;
                default:
                    Snackbar.Add("Invalid owner type selected", Severity.Error);
                    return;
            }

            if (!await InstanceService.CanRegisterAsync(ownerEntityId))
            {
                Snackbar.Add("An instance already exists for this entity", Severity.Warning);
                return;
            }

            var instance = await InstanceService.CreateInstanceAsync(
                _instanceName,
                string.IsNullOrWhiteSpace(_description) ? null : _description,
                ownerEntityId,
                ownerEntityName,
                _ownerType,
                _characterId,
                _characterName);

            if (instance != null)
            {
                Snackbar.Add("Instance created successfully!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(instance.Id));
                
                // Open the admin dialog for the newly created instance
                var parameters = new DialogParameters<AdminInstanceDialog>
                {
                    { x => x.InstanceId, instance.Id }
                };
                var options = new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    CloseButton = true
                };
                await DialogService.ShowAsync<AdminInstanceDialog>("Instance Administration", parameters, options);
            }
            else
            {
                Snackbar.Add("Failed to create instance", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating instance");
            Snackbar.Add("An error occurred while creating the instance", Severity.Error);
        }
        finally
        {
            _registering = false;
        }
    }

    private async Task ManageExistingInstance()
    {
        MudDialog.Close(DialogResult.Ok(_existingInstanceId));
        
        var parameters = new DialogParameters<AdminInstanceDialog>
        {
            { x => x.InstanceId, _existingInstanceId }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true
        };
        await DialogService.ShowAsync<AdminInstanceDialog>("Instance Administration", parameters, options);
    }

    private void Close() => MudDialog.Cancel();
}
