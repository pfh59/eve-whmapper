using Microsoft.AspNetCore.Authorization;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAccessRequirement : IAuthorizationRequirement
    {
        public EveMapperAccessRequirement()
        {
        }
    }
}
