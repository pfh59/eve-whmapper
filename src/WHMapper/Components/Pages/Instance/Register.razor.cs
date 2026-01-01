using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class Register : ComponentBase
{
    private MudForm _form = null!;
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

    [Inject]
    private ILogger<Register> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

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
            if (!string.IsNullOrEmpty(UID.ClientId))
            {
                var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
                if (primaryAccount != null)
                {
                    _isAuthenticated = true;
                    _characterId = primaryAccount.Id;

                    // Get character info from ESI
                    var characterResult = await CharacterServices.GetCharacter(_characterId);
                    if (characterResult.IsSuccess && characterResult.Data != null)
                    {
                        _characterInfo = characterResult.Data;
                        _characterName = _characterInfo.Name ?? string.Empty;

                        // Get corporation name
                        if (_characterInfo.CorporationId > 0)
                        {
                            var corp = await EveMapperService.GetCorporation(_characterInfo.CorporationId);
                            _corporationName = corp?.Name ?? "Unknown Corporation";
                        }

                        // Get alliance name
                        if (_characterInfo.AllianceId > 0)
                        {
                            var alliance = await EveMapperService.GetAlliance(_characterInfo.AllianceId);
                            _allianceName = alliance?.Name ?? "Unknown Alliance";
                        }
                    }

                    // Check if user already has an instance
                    var existingInstances = await InstanceService.GetAdministeredInstancesAsync(_characterId);
                    _alreadyHasInstance = existingInstances?.Any() == true;
                    if (_alreadyHasInstance && existingInstances != null)
                    {
                        _existingInstanceId = existingInstances.First().Id;
                    }
                }
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

            // Check if can register
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
                Navigation.NavigateTo($"/instance/{instance.Id}/admin");
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
}
