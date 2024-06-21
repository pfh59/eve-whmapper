using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAccessHandler : AuthorizationHandler<EveMapperAccessRequirement>
    {

        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;

        public EveMapperAccessHandler(IEveMapperAccessHelper eveMapperAccessHelper)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EveMapperAccessRequirement requirement)
        {
            var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(characterId))
                return;

            if (await _eveMapperAccessHelper.IsEveMapperUserAccessAuthorized(Convert.ToInt32(characterId)))
                context.Succeed(requirement);

            return;
        }
    }
}
