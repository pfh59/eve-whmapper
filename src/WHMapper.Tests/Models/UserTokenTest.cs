using System;

using WHMapper.Models.DTO;
using Xunit;

namespace WHMapper.Tests.Models;


public class UserTokenTest
{
    [Fact]
    public void UserToken_CanBeInstantiated()
    {
        var userToken = new UserToken();
        Assert.NotNull(userToken);
    }

    [Fact]
    public void UserToken_Properties_CanBeSetAndRetrieved()
    {
        var userToken = new UserToken
        {
            AccountId = "12345",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            Expiry = new DateTime(2023, 12, 31)
        };

        Assert.Equal("12345", userToken.AccountId);
        Assert.Equal("access_token", userToken.AccessToken);
        Assert.Equal("refresh_token", userToken.RefreshToken);
        Assert.Equal(new DateTime(2023, 12, 31), userToken.Expiry);
    }

    [Fact]
    public void UserToken_Expiry_SetCorrectly()
    {
        var expiryDate = new DateTime(2023, 12, 31);
        var userToken = new UserToken
        {
            Expiry = expiryDate
        };

        Assert.Equal(expiryDate, userToken.Expiry);
    }
}
