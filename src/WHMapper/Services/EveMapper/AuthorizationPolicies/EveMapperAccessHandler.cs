using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAccessHandler : AuthorizationHandler<EveMapperAccessRequirement>
    {

        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;
        private readonly IEveMapperUserManagementService _userManagementService;
        private readonly IBrowserClientIdProvider _browserClientIdProvider;

        public EveMapperAccessHandler(
            IEveMapperAccessHelper eveMapperAccessHelper,
            IEveMapperUserManagementService userManagementService,
            IBrowserClientIdProvider browserClientIdProvider)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
            _userManagementService = userManagementService;
            _browserClientIdProvider = browserClientIdProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EveMapperAccessRequirement requirement)
        {
            var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(characterId))
                return;

            // Try to get the primary account from the user management service
            // This handles multi-account scenarios where the user selects which account to use
            var clientId = await _browserClientIdProvider.GetClientIdAsync();
            if (!string.IsNullOrEmpty(clientId))
            {
                var primaryAccount = await _userManagementService.GetPrimaryAccountAsync(clientId);
                if (primaryAccount != null)
                {
                    // Check access for the primary account only
                    if (await _eveMapperAccessHelper.IsEveMapperUserAccessAuthorized(primaryAccount.Id))
                    {
                        context.Succeed(requirement);
                    }
                    return;
                }
            }

            // Fallback: if no primary account is set, check the authenticated character
            if (await _eveMapperAccessHelper.IsEveMapperUserAccessAuthorized(Convert.ToInt32(characterId)))
            {
                context.Succeed(requirement);
            }

            return;
        }
    }
}
