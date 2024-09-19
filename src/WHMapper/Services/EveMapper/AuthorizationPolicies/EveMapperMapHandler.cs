using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

namespace WHMapper.Services.EveMapper.AuthorizationPolicies;

public class EveMapperMapHandler: AuthorizationHandler<EveMapperMapRequirement>
{

    private readonly IEveMapperAccessHelper _eveMapperAccessHelper;

    public EveMapperMapHandler(IEveMapperAccessHelper eveMapperAccessHelper)
    {
        _eveMapperAccessHelper = eveMapperAccessHelper;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EveMapperMapRequirement requirement)
    {
        var characterId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(characterId))
            return;

        var pendingRequirements = context.PendingRequirements.ToList();
        if (pendingRequirements.Count > 0)
        {
            foreach (var pendingRequirement in pendingRequirements)
            {
                if (pendingRequirement is EveMapperMapRequirement)
                {
                    // get mapId from resource, passed in from blazor 
                    //  page component
                    var resource = context.Resource?.ToString();
                    var hasParsed = int.TryParse(resource, out int mapId);

                    if (hasParsed)
                    {                       
                        // check if user has access to map
                        if (await _eveMapperAccessHelper.IsEveMapperMapAccessAuthorized(Convert.ToInt32(characterId),mapId))
                            context.Succeed(requirement);
                    }
                }
            }
        }



        return;
    }
}
