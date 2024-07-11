using Microsoft.AspNetCore.Authorization;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAdminRequirement : IAuthorizationRequirement
    {
        public EveMapperAdminRequirement()
        {
        }
    }
}
