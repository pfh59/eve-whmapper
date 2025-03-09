using System;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperUserManagementService
{
    Task AddAuthenticateWHMapperUser(string clientId,string accountId,UserToken token);
    Task RemoveAuthenticateWHMapperUser(string clientId,string accountId);
    Task RemoveAuthenticateWHMapperUser(string clientId);
    Task<WHMapperUser[]> GetAccountsAsync(string clientId);
    Task<WHMapperUser?> GetPrimaryAccountAsync(string clientId);
    Task SetPrimaryAccountAsync(string clientId,string accountId);
}
