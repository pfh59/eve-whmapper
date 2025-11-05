using System.Collections.Concurrent;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveMapper;

public class EveMapperUserManagementService : IEveMapperUserManagementService
{
    private readonly IEveOnlineTokenProvider _tokenProvider;
    private readonly ICharacterServices _characterServices;
    private readonly ConcurrentDictionary<string, WHMapperUser[]> _whMapperUsers;

    public string? ClientId { get; set; } = string.Empty;

    public EveMapperUserManagementService(IEveOnlineTokenProvider tokenProvider,ICharacterServices characterServices)
    {
        _tokenProvider = tokenProvider;
        _characterServices = characterServices;

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
        if (portraitResult.IsSuccess)
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
    }

}
