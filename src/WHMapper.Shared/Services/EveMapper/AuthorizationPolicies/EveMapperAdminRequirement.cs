using Microsoft.AspNetCore.Authorization;

namespace WHMapper.Shared.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAdminRequirement : IAuthorizationRequirement
    {
        public EveMapperAdminRequirement()
        {
        }
    }
}
