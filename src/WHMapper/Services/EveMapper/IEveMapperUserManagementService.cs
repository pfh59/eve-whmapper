using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperUserManagementService
{
    /// <summary>
    /// Event triggered when the primary account changes.
    /// </summary>
    event Func<string, int, Task>? PrimaryAccountChanged;
    
    /// <summary>
    /// Event triggered when the current map changes, providing the client ID and new map ID.
    /// </summary>
    event Func<string, int, Task>? CurrentMapChanged;
    
    Task AddAuthenticateWHMapperUser(string clientId,string accountId,UserToken token);
    Task RemoveAuthenticateWHMapperUser(string clientId,string accountId);
    Task RemoveAuthenticateWHMapperUser(string clientId);
    Task<WHMapperUser[]> GetAccountsAsync(string clientId);
    Task<WHMapperUser?> GetPrimaryAccountAsync(string clientId);
    Task SetPrimaryAccountAsync(string clientId,string accountId);
    
    /// <summary>
    /// Updates map access status for all accounts based on the primary account's accessible maps.
    /// Should be called when primary account changes or when map access changes.
    /// </summary>
    Task UpdateAccountsMapAccessAsync(string clientId);
    
    /// <summary>
    /// Updates the current map access status for all accounts based on a specific map.
    /// Should be called when the user switches to a different map.
    /// </summary>
    /// <param name="clientId">The client identifier</param>
    /// <param name="mapId">The ID of the currently selected map</param>
    Task UpdateAccountsCurrentMapAccessAsync(string clientId, int mapId);
}
