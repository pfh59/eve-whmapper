using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class Admin : ComponentBase
{
    private bool _loading = true;
    private bool _isAdmin = false;
    private bool _isOwner = false;
    private int _characterId = 0;

    private WHInstance? _instance = null;
    private IEnumerable<WHMap>? _maps = null;
    private IEnumerable<WHInstanceAdmin>? _admins = null;
    private IEnumerable<WHInstanceAccess>? _accesses = null;

    [Parameter]
    public int InstanceId { get; set; }

    [Inject]
    private ILogger<Admin> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    private IWHInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private IEveMapperSearch EveMapperSearch { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDataAsync()
    {
        _loading = true;

        try
        {
            // Get current user
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var characterIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(characterIdClaim, out _characterId))
            {
                Logger.LogWarning("Could not parse character ID from claims");
                return;
            }

            // Load instance
            _instance = await InstanceService.GetInstanceAsync(InstanceId);
            if (_instance == null)
            {
                Logger.LogWarning("Instance {InstanceId} not found", InstanceId);
                return;
            }

            // Check admin access
            _isAdmin = await InstanceService.IsAdminAsync(InstanceId, _characterId);
            _isOwner = await InstanceService.IsOwnerAsync(InstanceId, _characterId);

            if (_isAdmin)
            {
                // Load related data
                _maps = await InstanceService.GetMapsAsync(InstanceId);
                _admins = await InstanceService.GetAdminsAsync(InstanceId);
                _accesses = await InstanceService.GetAccessesAsync(InstanceId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading instance data");
            Snackbar.Add("Error loading instance data", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task EditInstance()
    {
        var parameters = new DialogParameters<EditInstanceDialog>
        {
            { x => x.Instance, _instance! }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<EditInstanceDialog>("Edit Instance", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task DeleteInstance()
    {
        var confirm = await DialogService.ShowMessageBox(
            "Delete Instance",
            "Are you sure you want to delete this instance? This will delete all maps and data. This action cannot be undone!",
            yesText: "Delete", cancelText: "Cancel");

        if (confirm == true)
        {
            var success = await InstanceService.DeleteInstanceAsync(InstanceId, _characterId);
            if (success)
            {
                Snackbar.Add("Instance deleted successfully", Severity.Success);
                Navigation.NavigateTo("/");
            }
            else
            {
                Snackbar.Add("Failed to delete instance", Severity.Error);
            }
        }
    }

    private async Task AddMap()
    {
        var parameters = new DialogParameters<AddMapDialog>
        {
            { x => x.InstanceId, InstanceId }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AddMapDialog>("Add Map", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task DeleteMap(WHMap map)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Delete Map",
            $"Are you sure you want to delete the map '{map.Name}'? All systems and connections will be lost!",
            yesText: "Delete", cancelText: "Cancel");

        if (confirm == true)
        {
            var success = await InstanceService.DeleteMapAsync(InstanceId, map.Id, _characterId);
            if (success)
            {
                Snackbar.Add("Map deleted successfully", Severity.Success);
                await LoadDataAsync();
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to delete map", Severity.Error);
            }
        }
    }

    private async Task AddAdmin()
    {
        var parameters = new DialogParameters<AddAdminDialog>
        {
            { x => x.InstanceId, InstanceId }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AddAdminDialog>("Add Administrator", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task RemoveAdmin(WHInstanceAdmin admin)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Remove Administrator",
            $"Are you sure you want to remove '{admin.EveCharacterName}' as an administrator?",
            yesText: "Remove", cancelText: "Cancel");

        if (confirm == true)
        {
            var success = await InstanceService.RemoveAdminAsync(InstanceId, admin.EveCharacterId, _characterId);
            if (success)
            {
                Snackbar.Add("Administrator removed successfully", Severity.Success);
                await LoadDataAsync();
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to remove administrator", Severity.Error);
            }
        }
    }

    private async Task AddAccess()
    {
        var parameters = new DialogParameters<AddAccessDialog>
        {
            { x => x.InstanceId, InstanceId }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AddAccessDialog>("Add Access", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task RemoveAccess(WHInstanceAccess access)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Remove Access",
            $"Are you sure you want to remove access for '{access.EveEntityName}'?",
            yesText: "Remove", cancelText: "Cancel");

        if (confirm == true)
        {
            var success = await InstanceService.RemoveAccessAsync(InstanceId, access.Id, _characterId);
            if (success)
            {
                Snackbar.Add("Access removed successfully", Severity.Success);
                await LoadDataAsync();
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to remove access", Severity.Error);
            }
        }
    }

    private async Task ManageMapAccess(WHMap map)
    {
        var parameters = new DialogParameters<MapAccessDialog>
        {
            { x => x.InstanceId, InstanceId },
            { x => x.Map, map },
            { x => x.InstanceAccesses, _accesses }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<MapAccessDialog>($"Manage Access - {map.Name}", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }
}
