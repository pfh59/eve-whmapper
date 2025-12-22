using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class Overview : ComponentBase
{
    private bool _loading = true;
    private int _characterId = 0;
    private IEnumerable<WHInstance>? _instances;

    [Inject]
    private ILogger<Overview> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    private ICharacterServices CharacterServices { get; set; } = null!;

    [Inject]
    private IWHInstanceService InstanceService { get; set; } = null!;

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
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var characterIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(characterIdClaim, out _characterId))
            {
                Logger.LogWarning("Could not parse character ID");
                return;
            }

            // Get character info for corporation and alliance
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
            WHAccessEntity.Character => "👤",
            WHAccessEntity.Corporation => "🏢",
            WHAccessEntity.Alliance => "🌐",
            _ => ""
        };
    }
}
