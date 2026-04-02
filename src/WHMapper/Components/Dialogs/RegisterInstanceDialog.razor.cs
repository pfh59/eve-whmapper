using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
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
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private IInstanceRegistrationHelper RegistrationHelper { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private bool _formIsValid = false;
    private bool _loading = true;
    private bool _registering = false;

    private string _instanceName = string.Empty;
    private string _description = string.Empty;
    private WHAccessEntity _ownerType = WHAccessEntity.Character;

    private InstanceRegistrationContext _context = new();

    // Expose context properties for razor bindings
    private bool _isAuthenticated => _context.IsAuthenticated;
    private bool _alreadyHasInstance => _context.AlreadyHasInstance;
    private int _existingInstanceId => _context.ExistingInstanceId;
    private string _corporationName => _context.CorporationName;
    private string _allianceName => _context.AllianceName;

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
            _context = await RegistrationHelper.LoadRegistrationContextAsync(UID.ClientId);
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
            var instance = await RegistrationHelper.RegisterInstanceAsync(
                _context, _instanceName, _description, _ownerType);

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
        catch (InvalidOperationException ex)
        {
            Snackbar.Add(ex.Message, Severity.Warning);
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
