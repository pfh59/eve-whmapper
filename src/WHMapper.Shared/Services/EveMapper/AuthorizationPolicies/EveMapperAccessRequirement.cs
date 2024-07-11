using Microsoft.AspNetCore.Authorization;

namespace WHMapper.Shared.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAccessRequirement : IAuthorizationRequirement
    {
        public EveMapperAccessRequirement()
        {
        }
    }
}
