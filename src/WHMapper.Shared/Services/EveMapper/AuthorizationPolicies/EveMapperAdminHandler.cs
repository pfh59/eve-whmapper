using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAdminHandler : AuthorizationHandler<EveMapperAdminRequirement>
    {
        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;

        public EveMapperAdminHandler(IEveMapperAccessHelper eveMapperAccessHelper)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EveMapperAdminRequirement requirement)
        {
            var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(characterId))
                return;

            if (await _eveMapperAccessHelper.IsEveMapperAdminAccessAuthorized(Convert.ToInt32(characterId)))
                context.Succeed(requirement);

            return;
        }
    }
}
