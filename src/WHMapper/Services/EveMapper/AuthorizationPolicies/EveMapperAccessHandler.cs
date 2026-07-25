using WHMapper.Services.BrowserClientIdProvider;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies
{
    public class EveMapperAccessHandler : EveMapperRequirementHandlerBase<EveMapperAccessRequirement>
    {
        private readonly IEveMapperAccessHelper _eveMapperAccessHelper;

        public EveMapperAccessHandler(
            IEveMapperAccessHelper eveMapperAccessHelper,
            IEveMapperUserManagementService userManagementService,
            IBrowserClientIdProvider browserClientIdProvider)
            : base(userManagementService, browserClientIdProvider)
        {
            _eveMapperAccessHelper = eveMapperAccessHelper;
        }

        protected override Task<bool> IsAuthorizedAsync(int eveCharacterId)
            => _eveMapperAccessHelper.IsEveMapperUserAccessAuthorized(eveCharacterId);
    }
}
