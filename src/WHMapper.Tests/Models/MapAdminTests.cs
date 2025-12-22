using System.Collections.Generic;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.MapAdmin;
using Xunit;


namespace WHMapper.Tests.Models;


public class MapAdminTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var whMap = new WHMap("Test Map");
        whMap.Id = 1;
        whMap.WHMapAccesses.Add(new WHMapAccess(1, 12345, "Test Access", WHAccessEntity.Character));

        // Act
        var mapAdmin = new MapAdmin(whMap);

        // Assert
        Assert.Equal(1, mapAdmin.Id);
        Assert.Equal("Test Map", mapAdmin.Name);
        Assert.NotNull(mapAdmin.WHMapAccesses);
        Assert.Single(mapAdmin.WHMapAccesses);
        Assert.False(mapAdmin.ShowAccessDetails);
    }

    [Fact]
    public void Constructor_ShouldHandleNullWHMap()
    {
        // Act
        var mapAdmin = new MapAdmin(null);

        // Assert
        Assert.Equal(-1, mapAdmin.Id);
        Assert.Equal(string.Empty, mapAdmin.Name);
        Assert.Null(mapAdmin.WHMapAccesses);
        Assert.False(mapAdmin.ShowAccessDetails);
    }

    [Fact]
    public void ShowAccessDetails_ShouldBeFalseByDefault()
    {
        // Arrange
        var whMap = new WHMap("Test Map");

        // Act
        var mapAdmin = new MapAdmin(whMap);

        // Assert
        Assert.False(mapAdmin.ShowAccessDetails);
    }

    [Fact]
    public void ShowAccessDetails_ShouldBeSettable()
    {
        // Arrange
        var whMap = new WHMap("Test Map");
        var mapAdmin = new MapAdmin(whMap);

        // Act
        mapAdmin.ShowAccessDetails = true;

        // Assert
        Assert.True(mapAdmin.ShowAccessDetails);
    }
}

