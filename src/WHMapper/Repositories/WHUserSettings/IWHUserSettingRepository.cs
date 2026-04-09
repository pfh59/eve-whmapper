using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHUserSettings
{
    public interface IWHUserSettingRepository : IDefaultRepository<WHUserSetting, int>
    {
        Task<WHUserSetting?> GetByCharacterId(int eveCharacterId);
        Task<bool> DeleteByCharacterId(int eveCharacterId);
    }
}
