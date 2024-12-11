using System;

namespace WHMapper.Services.EveOAuthProvider.Validators;

public interface IEveOnlineAccessTokenValidator
{
   Task<bool> ValidateAsync(string accessToken);
}
