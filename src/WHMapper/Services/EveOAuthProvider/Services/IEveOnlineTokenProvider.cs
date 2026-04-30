using WHMapper.Models.DTO;

namespace WHMapper.Services.EveOAuthProvider.Services;

public interface IEveOnlineTokenProvider
{
    Task SaveToken(UserToken token);
    Task<UserToken?> GetToken(string accountId, bool autoRefreshed = false);
    Task ClearToken(string accountId);
    Task<bool> IsTokenExpire(string accountId);
    Task RefreshAccessToken(string accountId);
    Task RevokeToken(string accountId);


}
