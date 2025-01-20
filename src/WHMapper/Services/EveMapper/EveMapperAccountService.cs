using System;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAdditionnalAccounts;
using WHMapper.Repositories.WHMainAccounts;

namespace WHMapper.Services.EveMapper;

public class EveMapperAccountService : IEveMapperAccountService
{
    private readonly ILogger<EveMapperAccountService> _logger;
    private readonly IWHMainAccountRepository _mainAccountRepository;
   private readonly IWHAdditionnalAccountRepository _additionnalAccountRepository;


    public EveMapperAccountService(ILogger<EveMapperAccountService> logger,IWHMainAccountRepository mainAccountRepository, IWHAdditionnalAccountRepository additionnalAccountRepository)
    {
        _logger = logger;
        _mainAccountRepository = mainAccountRepository;
        _additionnalAccountRepository = additionnalAccountRepository;
    }

    public async Task<bool> RegisterAccount(int characterId)
    {
        var account = new WHMainAccount(characterId);
        return await _mainAccountRepository.Create(account) != null;
    }

    public async Task<bool> UnregisterAccount(int characterId)
    {
        return await _mainAccountRepository.DeleteById(characterId);
    }

    public async Task<WHMainAccount?> GetAccount(int characterId)
    {
        return await _mainAccountRepository.GetById(characterId);
    }

    public async Task<bool> AddAdditionalCharacter(int mainAccountId, int characterId)
    {
        var mainAccount = await _mainAccountRepository.GetById(mainAccountId);
        if(mainAccount == null)
        {
            _logger.LogError("Main account not found : {MainAccountId}", mainAccountId);
            return false;
        }

        var account = new WHAdditionnalAccount(characterId);
        account.MainAccount = mainAccount;
        return await _additionnalAccountRepository.Create(account) != null;
    }

    public async Task<bool> RemoveAdditionalCharacter(int characterId)
    {
        return await _additionnalAccountRepository.DeleteById(characterId);
    }

}
