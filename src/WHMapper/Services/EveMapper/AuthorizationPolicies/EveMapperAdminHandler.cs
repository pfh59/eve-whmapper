using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAdminHandler : AuthorizationHandler<EveMapperAdminRequirement>
    {
        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;
        private readonly IEveMapperUserManagementService _userManagementService;
        private readonly IBrowserClientIdProvider _browserClientIdProvider;

        public EveMapperAdminHandler(IEveMapperAccessHelper eveMapperAccessHelper,            
        IEveMapperUserManagementService userManagementService,
            IBrowserClientIdProvider browserClientIdProvider)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
            _userManagementService = userManagementService;
            _browserClientIdProvider = browserClientIdProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EveMapperAdminRequirement requirement)
        {
            var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(characterId) || !int.TryParse(characterId, out int authenticatedCharacterId))
                return;

            // Multi-account handling: a single browser (client_uid) may group several authenticated
            // EVE characters and act on behalf of a selected "primary" account.
            // The client_uid cookie is client-supplied and must NOT be trusted on its own: only honor
            // the primary-account decision once the authenticated identity is proven to belong to that
            // group. Otherwise a fixed/guessed client_uid pointing at an admin primary account would
            // let any authenticated character inherit admin rights.
            var clientId = await _browserClientIdProvider.GetClientIdAsync();
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var accounts = await _userManagementService.GetAccountsAsync(clientId);
                if (accounts.Any(a => a.Id == authenticatedCharacterId))
                {
                    var primaryAccount = await _userManagementService.GetPrimaryAccountAsync(clientId);
                    if (primaryAccount != null)
                    {
                        // Check access for the primary account only
                        if (await _eveMapperAccessHelper.IsEveMapperAdminAccessAuthorized(primaryAccount.Id))
                        {
                            context.Succeed(requirement);
                        }
                        return;
                    }
                }
            }

            // Fallback: check the authenticated character directly
            if (await _eveMapperAccessHelper.IsEveMapperAdminAccessAuthorized(authenticatedCharacterId))
            {
                context.Succeed(requirement);
            }

            return;
        }
    }
}
