using Microsoft.AspNetCore.Authorization;


namespace WHMapper.Services.EveMapper.AuthorizationPolicies;

public class EveMapperMapRequirement: IAuthorizationRequirement
{
    public EveMapperMapRequirement()
    {
    }
}
