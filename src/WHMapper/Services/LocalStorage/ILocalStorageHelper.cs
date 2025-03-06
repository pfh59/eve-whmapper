using System;

namespace WHMapper.Services.LocalStorage;

public interface ILocalStorageHelper
{
    Task<string?> GetOrCreateClientIdAsync();
}
