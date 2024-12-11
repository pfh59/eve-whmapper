using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace WHMapper.Services.EveOAuthProvider.Services;

public interface IClaimServices
{
    IEnumerable<Claim> ExtractClaimsFromEVEToken([NotNull] string token);
}
