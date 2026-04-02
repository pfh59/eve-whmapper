using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using Xunit;

namespace WHMapper.Tests.Models.DTO;

public class InstanceRegistrationContextTest
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var context = new InstanceRegistrationContext();

        Assert.False(context.IsAuthenticated);
        Assert.False(context.AlreadyHasInstance);
        Assert.Equal(0, context.ExistingInstanceId);
        Assert.Equal(0, context.CharacterId);
        Assert.Equal(string.Empty, context.CharacterName);
        Assert.Null(context.CharacterInfo);
        Assert.Equal(string.Empty, context.CorporationName);
        Assert.Equal(string.Empty, context.AllianceName);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var character = new Character
        {
            Name = "Test Pilot",
            CorporationId = 100,
            AllianceId = 200
        };

        var context = new InstanceRegistrationContext
        {
            IsAuthenticated = true,
            AlreadyHasInstance = true,
            ExistingInstanceId = 42,
            CharacterId = 123,
            CharacterName = "Test Pilot",
            CharacterInfo = character,
            CorporationName = "Test Corp",
            AllianceName = "Test Alliance"
        };

        Assert.True(context.IsAuthenticated);
        Assert.True(context.AlreadyHasInstance);
        Assert.Equal(42, context.ExistingInstanceId);
        Assert.Equal(123, context.CharacterId);
        Assert.Equal("Test Pilot", context.CharacterName);
        Assert.Same(character, context.CharacterInfo);
        Assert.Equal("Test Corp", context.CorporationName);
        Assert.Equal("Test Alliance", context.AllianceName);
    }

    [Fact]
    public void CharacterInfo_CanBeSetToNull()
    {
        var context = new InstanceRegistrationContext
        {
            CharacterInfo = new Character { Name = "Pilot" }
        };

        context.CharacterInfo = null;

        Assert.Null(context.CharacterInfo);
    }
}
