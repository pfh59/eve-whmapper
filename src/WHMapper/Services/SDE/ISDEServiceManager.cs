namespace WHMapper.Services.SDE
{
    public interface ISDEServiceManager
    {
        bool IsExtractionSuccesful();
        Task<bool> IsNewSDEAvailable();
        Task<bool> DownloadSDE();
        Task<bool> ExtractSDE();
        Task<bool> BuildCache();
        Task<bool> ClearCache();
        Task<bool> ClearSDEResources();
    }
}
