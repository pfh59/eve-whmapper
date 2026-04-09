using WHMapper.Models.Db;
using WHMapper.Repositories.WHUserSettings;

namespace WHMapper.Services.WHUserSettings
{
    public class WHUserSettingService : IWHUserSettingService
    {
        private readonly ILogger<WHUserSettingService> _logger;
        private readonly IWHUserSettingRepository _repository;

        public event Func<WHUserSetting, Task>? OnSettingsChanged;

        public WHUserSettingService(ILogger<WHUserSettingService> logger, IWHUserSettingRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<WHUserSetting> GetSettingsAsync(int eveCharacterId)
        {
            var settings = await _repository.GetByCharacterId(eveCharacterId);
            return settings ?? WHUserSetting.CreateDefault(eveCharacterId);
        }

        public async Task<WHUserSetting?> SaveSettingsAsync(WHUserSetting settings)
        {
            var existing = await _repository.GetByCharacterId(settings.EveCharacterId);
            WHUserSetting? result;
            if (existing != null)
            {
                settings.Id = existing.Id;
                result = await _repository.Update(existing.Id, settings);
            }
            else
            {
                result = await _repository.Create(settings);
            }

            if (result != null && OnSettingsChanged != null)
                await OnSettingsChanged.Invoke(result);

            return result;
        }

        public async Task<bool> ResetToDefaultsAsync(int eveCharacterId)
        {
            var deleted = await _repository.DeleteByCharacterId(eveCharacterId);
            if (deleted && OnSettingsChanged != null)
                await OnSettingsChanged.Invoke(WHUserSetting.CreateDefault(eveCharacterId));
            return deleted;
        }
    }
}
