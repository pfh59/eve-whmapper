using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.Dogma;
using Xunit;

namespace WHMapper.Tests.Models.DTO.EveAPI.Dogma;

public class AttributeTest
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesWithDefaultValues()
    {
        // Arrange & Act
        var attribute = new WHMapper.Models.DTO.EveAPI.Dogma.Attribute();

        // Assert
        Assert.Equal(0, attribute.AttributeId);
        Assert.Equal(0f, attribute.DefaultValue);
        Assert.Equal(string.Empty, attribute.Description);
        Assert.Equal(string.Empty, attribute.DisplayName);
        Assert.False(attribute.HighIsGood);
        Assert.Equal(0, attribute.IconId);
        Assert.Equal(string.Empty, attribute.Name);
        Assert.False(attribute.Published);
        Assert.False(attribute.Stackable);
        Assert.Equal(0, attribute.UnitId);
    }

    [Fact]
    public void Properties_ShouldSetAndGetValuesCorrectly()
    {
        // Arrange
        var attribute = new WHMapper.Models.DTO.EveAPI.Dogma.Attribute();
        var attributeId = 123;
        var defaultValue = 1.23f;
        var description = "Test Description";
        var displayName = "Test Display Name";
        var highIsGood = true;
        var iconId = 456;
        var name = "Test Name";
        var published = true;
        var stackable = true;
        var unitId = 789;

        // Act
        attribute.AttributeId = attributeId;
        attribute.DefaultValue = defaultValue;
        attribute.Description = description;
        attribute.DisplayName = displayName;
        attribute.HighIsGood = highIsGood;
        attribute.IconId = iconId;
        attribute.Name = name;
        attribute.Published = published;
        attribute.Stackable = stackable;
        attribute.UnitId = unitId;

        // Assert
        Assert.Equal(attributeId, attribute.AttributeId);
        Assert.Equal(defaultValue, attribute.DefaultValue);
        Assert.Equal(description, attribute.Description);
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.True(attribute.HighIsGood);
        Assert.Equal(iconId, attribute.IconId);
        Assert.Equal(name, attribute.Name);
        Assert.True(attribute.Published);
        Assert.True(attribute.Stackable);
        Assert.Equal(unitId, attribute.UnitId);
    }

    [Fact]
    public void Attribute_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var attribute = new WHMapper.Models.DTO.EveAPI.Dogma.Attribute()
        {
            AttributeId = 123,
            DefaultValue = 1.23f,
            Description = "Test Description",
            DisplayName = "Test Display Name",
            HighIsGood = true,
            IconId = 456,
            Name = "Test Name",
            Published = true,
            Stackable = true,
            UnitId = 789
        };

        // Act
        var json = JsonSerializer.Serialize(attribute);
        var deserializedAttribute = JsonSerializer.Deserialize<WHMapper.Models.DTO.EveAPI.Dogma.Attribute>(json);

        // Assert
        Assert.NotNull(deserializedAttribute);
        Assert.Equal(attribute.AttributeId, deserializedAttribute!.AttributeId);
        Assert.Equal(attribute.DefaultValue, deserializedAttribute.DefaultValue);
        Assert.Equal(attribute.Description, deserializedAttribute.Description);
        Assert.Equal(attribute.DisplayName, deserializedAttribute.DisplayName);
        Assert.Equal(attribute.HighIsGood, deserializedAttribute.HighIsGood);
        Assert.Equal(attribute.IconId, deserializedAttribute.IconId);
        Assert.Equal(attribute.Name, deserializedAttribute.Name);
        Assert.Equal(attribute.Published, deserializedAttribute.Published);
        Assert.Equal(attribute.Stackable, deserializedAttribute.Stackable);
        Assert.Equal(attribute.UnitId, deserializedAttribute.UnitId);
    }
}
