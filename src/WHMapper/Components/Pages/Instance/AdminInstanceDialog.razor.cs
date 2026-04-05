using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;

namespace WHMapper.Components.Pages.Instance;

public partial class AdminInstanceDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int InstanceId { get; set; }

    [Inject]
    private ILogger<AdminInstanceDialog> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService RealTimeService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private bool _loading = true;
    private bool _isAdmin = false;
    private bool _isOwner = false;
    private int _characterId = 0;

    private WHInstance? _instance = null;
    private IEnumerable<WHMap>? _maps = null;
    private IEnumerable<WHInstanceAdmin>? _admins = null;
    private IEnumerable<WHInstanceAccess>? _accesses = null;

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

            _instance = await InstanceService.GetInstanceAsync(InstanceId);
            if (_instance == null)
            {
                Logger.LogWarning("Instance {InstanceId} not found", InstanceId);
                return;
            }

            _isAdmin = await InstanceService.IsAdminAsync(InstanceId, _characterId);
            _isOwner = await InstanceService.IsOwnerAsync(InstanceId, _characterId);

            if (_isAdmin)
            {
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
        if (!await ShowConfirmationAsync("Delete Instance",
            "Are you sure you want to delete this instance? This will delete all maps and data. This action cannot be undone!",
            "Delete"))
            return;

        var success = await InstanceService.DeleteInstanceAsync(InstanceId, _characterId);
        if (success)
        {
            Snackbar.Add("Instance deleted successfully", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            Snackbar.Add("Failed to delete instance", Severity.Error);
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
        if (!await ShowConfirmationAsync("Delete Map",
            $"Are you sure you want to delete the map '{map.Name}'? All systems and connections will be lost!",
            "Delete"))
            return;

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
        if (!await ShowConfirmationAsync("Remove Administrator",
            $"Are you sure you want to remove '{admin.EveCharacterName}' as an administrator?",
            "Remove"))
            return;

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
        if (!await ShowConfirmationAsync("Remove Access",
            $"Are you sure you want to remove access for '{access.EveEntityName}'?",
            "Remove"))
            return;

        var accessId = access.Id;
        var (success, removedMapAccesses) = await InstanceService.RemoveAccessAsync(InstanceId, accessId, _characterId);
        if (success)
        {
            Snackbar.Add("Access removed successfully", Severity.Success);
            await RealTimeService.NotifyInstanceAccessRemoved(_characterId, InstanceId, accessId);
            
            foreach (var mapAccess in removedMapAccesses)
            {
                foreach (var removedAccessId in mapAccess.Value)
                {
                    await RealTimeService.NotifyMapAccessRemoved(_characterId, mapAccess.Key, removedAccessId);
                }
            }
            
            await LoadDataAsync();
            StateHasChanged();
        }
        else
        {
            Snackbar.Add("Failed to remove access", Severity.Error);
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

    private async Task<bool> ShowConfirmationAsync(string title, string content, string confirmText = "Delete")
    {
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            { x => x.ContentText, content },
            { x => x.ConfirmText, confirmText },
            { x => x.CancelText, "Cancel" },
            { x => x.ButtonColor, Color.Error }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<ConfirmationDialog>(title, parameters, options);
        var result = await dialog.Result;
        return result != null && !result.Canceled;
    }

    private void Close() => MudDialog.Cancel();
}
