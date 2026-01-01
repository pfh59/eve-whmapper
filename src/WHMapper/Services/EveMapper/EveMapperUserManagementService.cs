using System.Collections.Concurrent;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveMapper;

public class EveMapperUserManagementService : IEveMapperUserManagementService
{
    private readonly IEveOnlineTokenProvider _tokenProvider;
    private readonly ICharacterServices _characterServices;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EveMapperUserManagementService> _logger;
    private readonly ConcurrentDictionary<string, WHMapperUser[]> _whMapperUsers;

    public event Func<string, int, Task>? PrimaryAccountChanged;
    public event Func<string, int, Task>? CurrentMapChanged;

    public string? ClientId { get; set; } = string.Empty;

    public EveMapperUserManagementService(
        IEveOnlineTokenProvider tokenProvider,
        ICharacterServices characterServices,
        IServiceProvider serviceProvider,
        ILogger<EveMapperUserManagementService> logger)
    {
        _tokenProvider = tokenProvider;
        _characterServices = characterServices;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _whMapperUsers = new ConcurrentDictionary<string, WHMapperUser[]>();
    }


    public async Task AddAuthenticateWHMapperUser(string clientId,string accountId,UserToken token)
    {
         if(String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        if(String.IsNullOrEmpty(accountId) || String.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentNullException(nameof(accountId), "AccountId cannot be null");
        }

        if (!int.TryParse(accountId, out int id))
        {
            throw new ArgumentException("AccountId must be a valid integer", nameof(accountId));
        }

        Result<Portrait> portraitResult = await _characterServices.GetCharacterPortrait(id);

        Portrait? portrait = null;
        if (portraitResult != null && portraitResult.IsSuccess)
        {
            portrait = portraitResult.Data;
        }   

        var user = new WHMapperUser(id, portrait?.Picture64x64 ?? string.Empty);


        // Check if the user is already in the list
        if (_whMapperUsers.TryGetValue(clientId, out WHMapperUser[]? users) && users.Any(user => user.Id == id))
        {
            await _tokenProvider.SaveToken(token);
            await SetPrimaryAccountAsync(clientId, accountId);
            return;
        }

        // Add or update the user list
        _whMapperUsers.AddOrUpdate(clientId, new[] { user }, (_, existingUsers) => existingUsers.Append(user).ToArray());

        await _tokenProvider.SaveToken(token);
        await SetPrimaryAccountAsync(clientId, accountId);
    }

    public async Task RemoveAuthenticateWHMapperUser(string clientId,string accountId)
    {
        if(String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        if(String.IsNullOrEmpty(accountId) || String.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentNullException(nameof(accountId), "AccountId cannot be null");
        }

        if (!int.TryParse(accountId, out int id))
        {
            throw new ArgumentException("AccountId must be a valid integer", nameof(accountId));
        }

        if (_whMapperUsers.TryGetValue(clientId, out WHMapperUser[]? users))
        {
            var updatedUsers = users.Where(user => user.Id != id).ToArray();
            _whMapperUsers.AddOrUpdate(clientId, updatedUsers, (_, _) => updatedUsers);

            // Only clear the token if the user exists
            if (users.Any(user => user.Id == id))
            {
                await _tokenProvider.ClearToken(accountId);
            }
        }
    }

    public async Task RemoveAuthenticateWHMapperUser(string clientId)
    {
        if(String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        if (_whMapperUsers.TryGetValue(clientId, out WHMapperUser[]? users))
        {
            _whMapperUsers.TryRemove(clientId, out _);
            foreach (var user in users)
            {
                await _tokenProvider.ClearToken(user.Id.ToString());
            }
        }
    }

    public Task<WHMapperUser[]> GetAccountsAsync(string clientId)
    {
        if(String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        if (_whMapperUsers.TryGetValue(clientId, out WHMapperUser[]? users))
        {
            return Task.FromResult(users);
        }

        return Task.FromResult(Array.Empty<WHMapperUser>());
    }

    public async Task<WHMapperUser?> GetPrimaryAccountAsync(string clientId)
    {
        if(String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        var accounts = await GetAccountsAsync(clientId);
        if (accounts != null)
        {
            return accounts.FirstOrDefault(account => account.IsPrimary);
        }
        return null;

    }

    public async Task SetPrimaryAccountAsync(string clientId,string accountId)
    {
        if(String.IsNullOrEmpty(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        if(String.IsNullOrEmpty(accountId))
        {
            throw new ArgumentNullException(nameof(clientId), "AccountId cannot be null");
        }

        var accounts = await GetAccountsAsync(clientId);
        if(accounts == null)
        {
            throw new ArgumentNullException(nameof(accountId), "Account not found");
        }

        //set all accouts IsPrimary to false
        accounts.ToList().ForEach(account => account.IsPrimary = false);
        

        int id = int.Parse(accountId);
        var account = accounts.FirstOrDefault(account => account.Id == id) ?? throw new ArgumentNullException(nameof(accountId), "Account not found");

        account.IsPrimary = true;
        
        // Update map access for all accounts based on the new primary account
        await UpdateAccountsMapAccessAsync(clientId);
        
        // Notify listeners that the primary account has changed
        if (PrimaryAccountChanged != null)
        {
            await PrimaryAccountChanged.Invoke(clientId, id);
        }
    }

    public async Task UpdateAccountsMapAccessAsync(string clientId)
    {
        if (String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        var accounts = await GetAccountsAsync(clientId);
        if (accounts == null || accounts.Length == 0)
        {
            return;
        }

        var primaryAccount = accounts.FirstOrDefault(a => a.IsPrimary);
        if (primaryAccount == null)
        {
            _logger.LogWarning("No primary account found for client {ClientId}", clientId);
            return;
        }

        // Primary account always has map access
        primaryAccount.HasMapAccess = true;

        // Get accessible maps for the primary account using a scoped service
        using var scope = _serviceProvider.CreateScope();
        var mapRepo = scope.ServiceProvider.GetRequiredService<IWHMapRepository>();
        var accessHelper = scope.ServiceProvider.GetRequiredService<IEveMapperAccessHelper>();
        var allMaps = await mapRepo.GetAll();
        
        if (allMaps == null || !allMaps.Any())
        {
            _logger.LogInformation("No maps found in the system");
            // All accounts have no map access except primary who determines access
            foreach (var account in accounts.Where(a => !a.IsPrimary))
            {
                account.HasMapAccess = false;
            }
            return;
        }

        // Get the maps that the primary account has access to
        var primaryAccessibleMapIds = new List<int>();
        foreach (var map in allMaps)
        {
            if (await accessHelper.IsEveMapperMapAccessAuthorized(primaryAccount.Id, map.Id))
            {
                primaryAccessibleMapIds.Add(map.Id);
            }
        }

        _logger.LogInformation("Primary account {PrimaryAccountId} has access to {MapCount} maps", 
            primaryAccount.Id, primaryAccessibleMapIds.Count);

        // Check each secondary account's access to the primary's maps
        foreach (var account in accounts.Where(a => !a.IsPrimary))
        {
            bool hasAccessToAnyPrimaryMap = false;
            
            foreach (var mapId in primaryAccessibleMapIds)
            {
                if (await accessHelper.IsEveMapperMapAccessAuthorized(account.Id, mapId))
                {
                    hasAccessToAnyPrimaryMap = true;
                    break;
                }
            }

            account.HasMapAccess = hasAccessToAnyPrimaryMap;
            
            _logger.LogInformation("Account {AccountId} HasMapAccess: {HasMapAccess}", 
                account.Id, account.HasMapAccess);
        }
    }

    public async Task UpdateAccountsCurrentMapAccessAsync(string clientId, int mapId)
    {
        if (String.IsNullOrEmpty(clientId) || String.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
        }

        var accounts = await GetAccountsAsync(clientId);
        if (accounts == null || accounts.Length == 0)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var accessHelper = scope.ServiceProvider.GetRequiredService<IEveMapperAccessHelper>();

        foreach (var account in accounts)
        {
            var hasAccess = await accessHelper.IsEveMapperMapAccessAuthorized(account.Id, mapId);
            account.HasCurrentMapAccess = hasAccess;
            
            _logger.LogInformation("Account {AccountId} HasCurrentMapAccess for map {MapId}: {HasAccess}", 
                account.Id, mapId, hasAccess);
        }
        
        // Notify listeners that the current map has changed
        if (CurrentMapChanged != null)
        {
            await CurrentMapChanged.Invoke(clientId, mapId);
        }
    }

}
