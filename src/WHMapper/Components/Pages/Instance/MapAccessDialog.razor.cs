using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Components.Dialogs;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
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
    private IEveMapperInstanceService InstanceService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService RealTimeService { get; set; } = null!;

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
                
                // Notify all connected users about the new map access
                await RealTimeService.NotifyMapAccessesAdded(_characterId, Map.Id, new[] { result.Id });
                
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
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            { x => x.ContentText, $"Are you sure you want to remove map access for '{access.EveEntityName}'?" },
            { x => x.ConfirmText, "Remove" },
            { x => x.CancelText, "Cancel" },
            { x => x.ButtonColor, Color.Error }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Remove Map Access", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            try
            {
                var accessId = access.Id;
                var success = await InstanceService.RemoveMapAccessAsync(InstanceId, Map.Id, accessId, _characterId);
                if (success)
                {
                    Snackbar.Add("Map access removed successfully", Severity.Success);
                    
                    // Notify all connected users about the removed map access
                    await RealTimeService.NotifyMapAccessRemoved(_characterId, Map.Id, accessId);
                    
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
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            { x => x.ContentText, "Are you sure you want to remove all access restrictions from this map? All users with instance access will be able to view the map." },
            { x => x.ConfirmText, "Remove All" },
            { x => x.CancelText, "Cancel" },
            { x => x.ButtonColor, Color.Error }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Remove All Restrictions", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            try
            {
                var success = await InstanceService.ClearMapAccessesAsync(InstanceId, Map.Id, _characterId);
                if (success)
                {
                    Snackbar.Add("All map access restrictions removed", Severity.Success);
                    
                    // Notify all connected users that all map accesses have been removed
                    await RealTimeService.NotifyMapAllAccessesRemoved(_characterId, Map.Id);
                    
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
