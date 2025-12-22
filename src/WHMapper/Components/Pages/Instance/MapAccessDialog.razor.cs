using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class MapAccessDialog : ComponentBase
{
    private bool _loading = true;
    private int _characterId = 0;
    private IEnumerable<WHMapAccess>? _mapAccesses;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int InstanceId { get; set; }

    [Parameter]
    public WHMap Map { get; set; } = null!;

    [Parameter]
    public IEnumerable<WHInstanceAccess>? InstanceAccesses { get; set; }

    [Inject]
    private IWHInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var characterIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(characterIdClaim, out _characterId);

        await LoadMapAccessesAsync();
    }

    private async Task LoadMapAccessesAsync()
    {
        _loading = true;
        try
        {
            _mapAccesses = await InstanceService.GetMapAccessesAsync(InstanceId, Map.Id, _characterId);
        }
        catch (Exception)
        {
            Snackbar.Add("Error loading map accesses", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));

    private async Task AddAccessFromInstance(WHInstanceAccess instanceAccess)
    {
        try
        {
            var result = await InstanceService.AddMapAccessAsync(
                InstanceId,
                Map.Id,
                instanceAccess.EveEntityId,
                instanceAccess.EveEntityName,
                instanceAccess.EveEntity,
                _characterId);

            if (result != null)
            {
                Snackbar.Add($"Granted map access to {instanceAccess.EveEntityName}", Severity.Success);
                await LoadMapAccessesAsync();
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to grant map access", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error granting map access", Severity.Error);
        }
    }

    private async Task RemoveAccess(WHMapAccess access)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Remove Map Access",
            $"Are you sure you want to remove map access for '{access.EveEntityName}'?",
            yesText: "Remove", cancelText: "Cancel");

        if (confirm == true)
        {
            try
            {
                var success = await InstanceService.RemoveMapAccessAsync(InstanceId, Map.Id, access.Id, _characterId);
                if (success)
                {
                    Snackbar.Add("Map access removed successfully", Severity.Success);
                    await LoadMapAccessesAsync();
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Failed to remove map access", Severity.Error);
                }
            }
            catch (Exception)
            {
                Snackbar.Add("Error removing map access", Severity.Error);
            }
        }
    }

    private async Task ClearAllAccesses()
    {
        var confirm = await DialogService.ShowMessageBox(
            "Remove All Restrictions",
            "Are you sure you want to remove all access restrictions from this map? All users with instance access will be able to view the map.",
            yesText: "Remove All", cancelText: "Cancel");

        if (confirm == true)
        {
            try
            {
                var success = await InstanceService.ClearMapAccessesAsync(InstanceId, Map.Id, _characterId);
                if (success)
                {
                    Snackbar.Add("All map access restrictions removed", Severity.Success);
                    await LoadMapAccessesAsync();
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Failed to remove map access restrictions", Severity.Error);
                }
            }
            catch (Exception)
            {
                Snackbar.Add("Error removing map access restrictions", Severity.Error);
            }
        }
    }
}
