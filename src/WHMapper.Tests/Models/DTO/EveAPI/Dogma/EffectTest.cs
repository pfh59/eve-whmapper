using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.Dogma;
using Xunit;

namespace WHMapper.Tests.Models.DTO.EveAPI.Dogma;

public class EffectTest
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesWithDefaultValues()
    {
        // Arrange & Act
        var effect = new Effect();

        // Assert
        Assert.Equal(string.Empty, effect.Description);
        Assert.False(effect.DisallowAutoRepeat);
        Assert.Equal(0, effect.DischargeAttributeId);
        Assert.Equal(string.Empty, effect.DisplayName);
        Assert.Equal(0, effect.DurationAttributeId);
        Assert.Equal(0, effect.EffectCategory);
        Assert.Equal(0, effect.EffectId);
        Assert.False(effect.ElectronicChance);
        Assert.False(effect.FalloffAttributeId);
        Assert.Equal(0, effect.IconId);
        Assert.False(effect.IsAssistance);
        Assert.False(effect.IsOffensive);
        Assert.False(effect.IsWarpSafe);
        Assert.Null(effect.Modifiers);
        Assert.Equal(string.Empty, effect.Name);
        Assert.Equal(0, effect.PostExpression);
        Assert.Equal(0, effect.PreExpression);
        Assert.False(effect.published);
        Assert.Equal(0, effect.RangeAttributeId);
        Assert.False(effect.RangeChance);
        Assert.Equal(0, effect.TrackingSpeedAttributeId);
    }

    [Fact]
    public void Properties_ShouldSetAndGetValuesCorrectly()
    {
        // Arrange
        var effect = new Effect();
        var description = "Test Description";
        var disallowAutoRepeat = true;
        var dischargeAttributeId = 123;
        var displayName = "Test Display Name";
        var durationAttributeId = 456;
        var effectCategory = 789;
        var effectId = 101112;
        var electronicChance = true;
        var falloffAttributeId = true;
        var iconId = 131415;
        var isAssistance = true;
        var isOffensive = true;
        var isWarpSafe = true;
        var modifiers = new Modifier[] { new Modifier() };
        var name = "Test Name";
        var postExpression = 161718;
        var preExpression = 192021;
        var published = true;
        var rangeAttributeId = 222324;
        var rangeChance = true;
        var trackingSpeedAttributeId = 252627;

        // Act
        effect.Description = description;
        effect.DisallowAutoRepeat = disallowAutoRepeat;
        effect.DischargeAttributeId = dischargeAttributeId;
        effect.DisplayName = displayName;
        effect.DurationAttributeId = durationAttributeId;
        effect.EffectCategory = effectCategory;
        effect.EffectId = effectId;
        effect.ElectronicChance = electronicChance;
        effect.FalloffAttributeId = falloffAttributeId;
        effect.IconId = iconId;
        effect.IsAssistance = isAssistance;
        effect.IsOffensive = isOffensive;
        effect.IsWarpSafe = isWarpSafe;
        effect.Modifiers = modifiers;
        effect.Name = name;
        effect.PostExpression = postExpression;
        effect.PreExpression = preExpression;
        effect.published = published;
        effect.RangeAttributeId = rangeAttributeId;
        effect.RangeChance = rangeChance;
        effect.TrackingSpeedAttributeId = trackingSpeedAttributeId;

        // Assert
        Assert.Equal(description, effect.Description);
        Assert.True(effect.DisallowAutoRepeat);
        Assert.Equal(dischargeAttributeId, effect.DischargeAttributeId);
        Assert.Equal(displayName, effect.DisplayName);
        Assert.Equal(durationAttributeId, effect.DurationAttributeId);
        Assert.Equal(effectCategory, effect.EffectCategory);
        Assert.Equal(effectId, effect.EffectId);
        Assert.True(electronicChance);
        Assert.True(falloffAttributeId);
        Assert.Equal(iconId, effect.IconId);
        Assert.True(isAssistance);
        Assert.True(isOffensive);
        Assert.True(isWarpSafe);
        Assert.Equal(modifiers, effect.Modifiers);
        Assert.Equal(name, effect.Name);
        Assert.Equal(postExpression, effect.PostExpression);
        Assert.Equal(preExpression, effect.PreExpression);
        Assert.True(published);
        Assert.Equal(rangeAttributeId, effect.RangeAttributeId);
        Assert.True(rangeChance);
        Assert.Equal(trackingSpeedAttributeId, effect.TrackingSpeedAttributeId);
    }

    [Fact]
    public void Effect_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var effect = new Effect
        {
            Description = "Test Description",
            DisallowAutoRepeat = true,
            DischargeAttributeId = 123,
            DisplayName = "Test Display Name",
            DurationAttributeId = 456,
            EffectCategory = 789,
            EffectId = 101112,
            ElectronicChance = true,
            FalloffAttributeId = true,
            IconId = 131415,
            IsAssistance = true,
            IsOffensive = true,
            IsWarpSafe = true,
            Modifiers = new Modifier[] { new Modifier { Func = "Test Func", Domain = "Test Domain", ModifiedAttributeId = 1, ModifyingAttributeId = 2, EffectId = 3, Operator = 4 } },
            Name = "Test Name",
            PostExpression = 161718,
            PreExpression = 192021,
            published = true,
            RangeAttributeId = 222324,
            RangeChance = true,
            TrackingSpeedAttributeId = 252627
        };

        // Act
        var json = JsonSerializer.Serialize(effect);
        var deserializedEffect = JsonSerializer.Deserialize<Effect>(json);

        // Assert
        Assert.NotNull(deserializedEffect);
        Assert.Equal(effect.Description, deserializedEffect!.Description);
        Assert.Equal(effect.DisallowAutoRepeat, deserializedEffect.DisallowAutoRepeat);
        Assert.Equal(effect.DischargeAttributeId, deserializedEffect.DischargeAttributeId);
        Assert.Equal(effect.DisplayName, deserializedEffect.DisplayName);
        Assert.Equal(effect.DurationAttributeId, deserializedEffect.DurationAttributeId);
        Assert.Equal(effect.EffectCategory, deserializedEffect.EffectCategory);
        Assert.Equal(effect.EffectId, deserializedEffect.EffectId);
        Assert.Equal(effect.ElectronicChance, deserializedEffect.ElectronicChance);
        Assert.Equal(effect.FalloffAttributeId, deserializedEffect.FalloffAttributeId);
        Assert.Equal(effect.IconId, deserializedEffect.IconId);
        Assert.Equal(effect.IsAssistance, deserializedEffect.IsAssistance);
        Assert.Equal(effect.IsOffensive, deserializedEffect.IsOffensive);
        Assert.Equal(effect.IsWarpSafe, deserializedEffect.IsWarpSafe);
        Assert.Equal(effect.Modifiers.Length, deserializedEffect.Modifiers.Length);
        Assert.Equal(effect.Modifiers[0].Func, deserializedEffect.Modifiers[0].Func);
        Assert.Equal(effect.Modifiers[0].Domain, deserializedEffect.Modifiers[0].Domain);
        Assert.Equal(effect.Modifiers[0].ModifiedAttributeId, deserializedEffect.Modifiers[0].ModifiedAttributeId);
        Assert.Equal(effect.Modifiers[0].ModifyingAttributeId, deserializedEffect.Modifiers[0].ModifyingAttributeId);
        Assert.Equal(effect.Modifiers[0].EffectId, deserializedEffect.Modifiers[0].EffectId);
        Assert.Equal(effect.Modifiers[0].Operator, deserializedEffect.Modifiers[0].Operator);
        Assert.Equal(effect.Name, deserializedEffect.Name);
        Assert.Equal(effect.PostExpression, deserializedEffect.PostExpression);
        Assert.Equal(effect.PreExpression, deserializedEffect.PreExpression);
        Assert.Equal(effect.published, deserializedEffect.published);
        Assert.Equal(effect.RangeAttributeId, deserializedEffect.RangeAttributeId);
        Assert.Equal(effect.RangeChance, deserializedEffect.RangeChance);
        Assert.Equal(effect.TrackingSpeedAttributeId, deserializedEffect.TrackingSpeedAttributeId);
    }
}
