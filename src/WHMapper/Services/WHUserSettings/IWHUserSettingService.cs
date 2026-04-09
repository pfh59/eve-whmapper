using WHMapper.Models.Db;

namespace WHMapper.Services.WHUserSettings
{
    public interface IWHUserSettingService
    {
        event Func<WHUserSetting, Task>? OnSettingsChanged;
        Task<WHUserSetting> GetSettingsAsync(int eveCharacterId);
        Task<WHUserSetting?> SaveSettingsAsync(WHUserSetting settings);
        Task<bool> ResetToDefaultsAsync(int eveCharacterId);
    }
}
