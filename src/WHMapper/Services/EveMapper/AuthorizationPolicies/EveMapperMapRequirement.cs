using Microsoft.AspNetCore.Authorization;
using System;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies;

public class EveMapperMapRequirement: IAuthorizationRequirement
{
    public EveMapperMapRequirement()
    {
    }
}
