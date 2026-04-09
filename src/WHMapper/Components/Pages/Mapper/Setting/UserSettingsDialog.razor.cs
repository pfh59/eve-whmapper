using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHUserSettings;

namespace WHMapper.Components.Pages.Mapper.Setting;

public partial class UserSettingsDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private ILogger<UserSettingsDialog> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHUserSettingService SettingService { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Parameter]
    public WHUserSetting? Settings { get; set; }

    private WHUserSetting? _settings;

    protected override async Task OnInitializedAsync()
    {
        if (Settings != null)
        {
            _settings = CloneSettings(Settings);
        }
        else
        {
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId ?? string.Empty);
            var characterId = primaryAccount?.Id ?? 0;
            var loaded = await SettingService.GetSettingsAsync(characterId);
            _settings = CloneSettings(loaded);
        }

        await base.OnInitializedAsync();
    }

    private void OnKeyCaptured(KeyboardEventArgs e, string propertyName)
    {
        if (_settings == null) return;
        var code = e.Code;
        if (string.IsNullOrEmpty(code))
            return;

        switch (propertyName)
        {
            case nameof(WHUserSetting.KeyLink):
                _settings.KeyLink = code;
                break;
            case nameof(WHUserSetting.KeyDelete):
                _settings.KeyDelete = code;
                break;
            case nameof(WHUserSetting.KeyIncrementExtension):
                _settings.KeyIncrementExtension = code;
                break;
            case nameof(WHUserSetting.KeyDecrementExtension):
                _settings.KeyDecrementExtension = code;
                break;
            case nameof(WHUserSetting.KeyIncrementExtensionAlt):
                _settings.KeyIncrementExtensionAlt = code;
                break;
            case nameof(WHUserSetting.KeyDecrementExtensionAlt):
                _settings.KeyDecrementExtensionAlt = code;
                break;
        }

        StateHasChanged();
    }

    private async Task Save()
    {
        if (_settings == null) return;
        try
        {
            var saved = await SettingService.SaveSettingsAsync(_settings);
            if (saved != null)
            {
                Snackbar.Add("Settings saved", Severity.Success);
                MudDialog.Close(DialogResult.Ok(saved));
            }
            else
            {
                Snackbar.Add("Failed to save settings", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving user settings");
            Snackbar.Add("Error saving settings", Severity.Error);
        }
    }

    private async Task ResetToDefaults()
    {
        if (_settings == null) return;
        try
        {
            await SettingService.ResetToDefaultsAsync(_settings.EveCharacterId);
            _settings = WHUserSetting.CreateDefault(_settings.EveCharacterId);
            Snackbar.Add("Settings reset to defaults", Severity.Info);
            MudDialog.Close(DialogResult.Ok(_settings));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resetting user settings");
            Snackbar.Add("Error resetting settings", Severity.Error);
        }
    }

    private void Cancel() => MudDialog.Cancel();

    private static WHUserSetting CloneSettings(WHUserSetting source)
    {
        return new WHUserSetting(source.EveCharacterId)
        {
            Id = source.Id,
            KeyLink = source.KeyLink,
            KeyDelete = source.KeyDelete,
            KeyIncrementExtension = source.KeyIncrementExtension,
            KeyDecrementExtension = source.KeyDecrementExtension,
            KeyIncrementExtensionAlt = source.KeyIncrementExtensionAlt,
            KeyDecrementExtensionAlt = source.KeyDecrementExtensionAlt,
            ZoomEnabled = source.ZoomEnabled,
            ZoomInverse = source.ZoomInverse,
            AllowMultiSelection = source.AllowMultiSelection,
            LinkSnapping = source.LinkSnapping,
            NodeSpacing = source.NodeSpacing,
            DragThreshold = source.DragThreshold,
        };
    }
}
