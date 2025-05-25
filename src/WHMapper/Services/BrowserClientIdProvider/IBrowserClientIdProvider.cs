using System;

namespace WHMapper.Services.BrowserClientIdProvider;

public interface IBrowserClientIdProvider
{
    Task<string?> GetClientIdAsync();
}
