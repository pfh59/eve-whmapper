using System;
using WHMapper.Models.Db;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperAccountService
{
    public Task<bool> RegisterAccount(int characterId);

    public Task<bool> UnregisterAccount(int characterId);

    public Task<WHMainAccount?> GetAccount(int characterId);

    public Task<bool> AddAdditionalCharacter(int mainAccountId, int characterId);

    public Task<bool> RemoveAdditionalCharacter(int characterId);
}
