using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;
using System.Security.Cryptography;

namespace WHMapper.Services.LocalStorage;

public class LocalStorageHelper : ILocalStorageHelper
{
    private readonly ILogger<LocalStorageHelper> _logger;
    private readonly ProtectedLocalStorage _localStorage;
    public LocalStorageHelper(ILogger<LocalStorageHelper> logger, ProtectedLocalStorage localStorage)
    {
        _logger = logger;
        _localStorage = localStorage;
    }


    public async Task<string?> GetOrCreateClientIdAsync()
    {
        try
        {
            var res = await _localStorage.GetAsync<string>("ClientId");
            if (!res.Success || string.IsNullOrEmpty(res.Value))
            {
                var clientId = Guid.NewGuid().ToString();
                await _localStorage.SetAsync("ClientId", clientId);
                return clientId;
            }
            return res.Value;
        }
        catch(CryptographicException ex)
        {
            _logger.LogError(ex, "An error occurred while generating a new ClientId.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting or creating the ClientId.");
            return null;
        }
    }

    public async Task<bool> ClearClientIdAsync()
    {
        try
        {
            await _localStorage.DeleteAsync("ClientId");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while clearing the ClientId from localStorage.");
            return false;
        }
    }

    
}
