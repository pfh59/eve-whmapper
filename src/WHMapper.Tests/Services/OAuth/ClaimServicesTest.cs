using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
using Xunit;


namespace WHMapper.Tests.Services.OAuth;


public class ClaimServicesTest
{
    private const string _token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkpXVC1TaWduYXR1cmUtS2V5IiwidHlwIjoiSldUIn0.eyJzY3AiOlsiZXNpLWxvY2F0aW9uLnJlYWRfbG9jYXRpb24udjEiLCJlc2ktbG9jYXRpb24ucmVhZF9zaGlwX3R5cGUudjEiLCJlc2ktdWkub3Blbl93aW5kb3cudjEiLCJlc2ktdWkud3JpdGVfd2F5cG9pbnQudjEiLCJlc2ktc2VhcmNoLnNlYXJjaF9zdHJ1Y3R1cmVzLnYxIl0sImp0aSI6Ijk0ZjA2YjVhLTQwMTYtNDJhOC05MTNmLWQwZmYwNWNjZWZhOCIsImtpZCI6IkpXVC1TaWduYXR1cmUtS2V5Iiwic3ViIjoiQ0hBUkFDVEVSOkVWRToxMjIyNDAxNTQ5IiwiYXpwIjoiMzllZWUzMTliMmQxNGE0YTliYTNlOWViMWIxNDEwYjgiLCJ0ZW5hbnQiOiJ0cmFucXVpbGl0eSIsInRpZXIiOiJsaXZlIiwicmVnaW9uIjoid29ybGQiLCJhdWQiOlsiMzllZWUzMTliMmQxNGE0YTliYTNlOWViMWIxNDEwYjgiLCJFVkUgT25saW5lIl0sIm5hbWUiOiJTa290b3RvdW50YSIsIm93bmVyIjoiaWJMQzA0WGRuZVJ2YThwYlVGbGlTQlFYeXVvPSIsImV4cCI6MTczODYxOTk3MywiaWF0IjoxNzM4NjE4NzczLCJpc3MiOiJodHRwczovL2xvZ2luLmV2ZW9ubGluZS5jb20ifQ.T-kPx-3OG0fe6hK6tYGF7F_-56GclqWLXpID868VEQM255v5f1WPFnzx_c3gllKWufkNHTUEXHxEKTjhgGeJJJPdiAuvg3UR24_lst2Y6nZBR1kXZIdVjNQTecLcs3Jw4IYO92K1rkbUlVGEVq7vc8hXA--MYvmQ65GuoLmPMpujsoyJM2d85k7x6GvMYuBWSlVdfT866Z03uTj1MHxbFhTbeByJ5OOqtQjwdui_E2tn7cEXOmqxIKAzRbCvsNlb1lumVqfoiQbol6LDiKbE23yXax-60z7anPpmc_jw1SAuCjaQmGXqIVptkS8lD_DWYTJEcWKlYbAlpci4qfql2Q";
    private readonly Mock<JsonWebTokenHandler> _mockTokenHandler;
    private readonly ClaimServices _claimServices;

    public ClaimServicesTest()
    {
        _mockTokenHandler = new Mock<JsonWebTokenHandler>();
        _claimServices = new ClaimServices();
    }

    [Fact]
    public async Task ExtractClaimsFromEVEToken_ValidToken_ReturnsClaims()
    {
        // Act
        var result = await _claimServices.ExtractClaimsFromEVEToken(_token);
        // Assert
        Assert.Contains(result, claim => claim.Type == ClaimTypes.Name && claim.Value == "Skototounta");
    }

    [Fact]
    public async Task ExtractClaimsFromEVEToken_NullOrEmptyToken_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _claimServices.ExtractClaimsFromEVEToken(null));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _claimServices.ExtractClaimsFromEVEToken(string.Empty));
    }

    [Fact]
    public async Task ExtractClaimsFromEVEToken_InvalidToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var token = "invalid-token";
        _mockTokenHandler.Setup(handler => handler.ReadJsonWebToken(token)).Returns((JsonWebToken)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claimServices.ExtractClaimsFromEVEToken(token));
    }

    [Fact]
    public async Task ExtractClaimsFromEVEToken_MissingRequiredClaims_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claimServices.ExtractClaimsFromEVEToken("bad_token"));
    }
}
