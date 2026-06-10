using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAdminHandler : EveMapperRequirementHandlerBase<EveMapperAdminRequirement>
    {
        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;

        public EveMapperAdminHandler(
            IEveMapperAccessHelper eveMapperAccessHelper,
            IEveMapperUserManagementService userManagementService,
            IBrowserClientIdProvider browserClientIdProvider)
            : base(userManagementService, browserClientIdProvider)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
        }

        protected override Task<bool> IsAuthorizedAsync(int eveCharacterId)
            => _eveMapperAccessHelper.IsEveMapperAdminAccessAuthorized(eveCharacterId);
    }
}
