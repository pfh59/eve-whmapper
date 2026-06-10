using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public abstract class EveMapperRequirementHandlerBase<TRequirement>
        : AuthorizationHandler<TRequirement>
        where TRequirement : IAuthorizationRequirement
    {
        private readonly IEveMapperUserManagementService _userManagementService;
        private readonly IBrowserClientIdProvider _browserClientIdProvider;

        protected EveMapperRequirementHandlerBase(
            IEveMapperUserManagementService userManagementService,
            IBrowserClientIdProvider browserClientIdProvider)
        {
            _userManagementService = userManagementService;
            _browserClientIdProvider = browserClientIdProvider;
        }

        // Implemented by each handler to call the matching access-helper method.
        protected abstract Task<bool> IsAuthorizedAsync(int eveCharacterId);

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, TRequirement requirement)
        {
            var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(characterId) || !int.TryParse(characterId, out int authenticatedCharacterId))
                return;

            // Multi-account handling: a single browser (client_uid) may group several authenticated
            // EVE characters and act on behalf of a selected "primary" account.
            // The client_uid cookie is client-supplied and must NOT be trusted on its own: only honor
            // the primary-account decision once the authenticated identity is proven to belong to that
            // group. Otherwise a fixed/guessed client_uid pointing at a privileged primary account would
            // let any authenticated character inherit its access.
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
                        if (await IsAuthorizedAsync(primaryAccount.Id))
                        {
                            context.Succeed(requirement);
                        }
                        return;
                    }
                }
            }

            // Fallback: check the authenticated character directly
            if (await IsAuthorizedAsync(authenticatedCharacterId))
            {
                context.Succeed(requirement);
            }
        }
    }
}
