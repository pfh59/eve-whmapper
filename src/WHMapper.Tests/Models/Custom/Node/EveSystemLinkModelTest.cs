using Blazor.Diagrams.Core.Models;
using Moq;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using Xunit;

namespace WHMapper.Tests.Models.Custom.Node
{
    public class EveSystemLinkModelTest
    {
        private static WHSystemLink CreateWHSystemLink(int id = 1, SystemLinkSize size = SystemLinkSize.Large, 
            SystemLinkMassStatus massStatus = SystemLinkMassStatus.Normal, 
            SystemLinkEolStatus eolStatus = SystemLinkEolStatus.Normal)
        {
            var whLink = new WHSystemLink(1, 2, 3)
            {
                Id = id,
                Size = size,
                MassStatus = massStatus,
                EndOfLifeStatus = eolStatus
            };
            return whLink;
        }

        private static EveSystemNodeModel CreateNodeModel()
        {
            var whSystem = new WHSystem(1, 1, "Test System", 0.5f, 0.0, 0.0);
            return new EveSystemNodeModel(whSystem, null, string.Empty, string.Empty);
        }

        [Fact]
        public void Constructor_ShouldSetMarkersAndLabel()
        {
            // Arrange
            var whLink = CreateWHSystemLink(size: SystemLinkSize.Small);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();

            // Act
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Assert
            Assert.NotNull(linkModel.SourceMarker);
            Assert.NotNull(linkModel.TargetMarker);
            Assert.Single(linkModel.Labels);
        }

        [Fact]
        public void Id_ShouldReturnWhLinkId()
        {
            // Arrange
            var whLink = CreateWHSystemLink(id: 42);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            var result = linkModel.Id;

            // Assert
            Assert.Equal(42, result);
        }

        [Theory]
        [InlineData(SystemLinkEolStatus.Normal)]
        [InlineData(SystemLinkEolStatus.EOL4h)]
        [InlineData(SystemLinkEolStatus.EOL1h)]
        public void EndOfLifeStatus_GetSet_ShouldWork(SystemLinkEolStatus status)
        {
            // Arrange
            var whLink = CreateWHSystemLink();
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.EndOfLifeStatus = status;

            // Assert
            Assert.Equal(status, linkModel.EndOfLifeStatus);
        }

        [Fact]
        public void IsEoL_Get_ShouldReturnFalse_WhenStatusIsNormal()
        {
            // Arrange
            var whLink = CreateWHSystemLink(eolStatus: SystemLinkEolStatus.Normal);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act & Assert
            Assert.False(linkModel.IsEoL);
        }

        [Theory]
        [InlineData(SystemLinkEolStatus.EOL4h)]
        [InlineData(SystemLinkEolStatus.EOL1h)]
        public void IsEoL_Get_ShouldReturnTrue_WhenStatusIsNotNormal(SystemLinkEolStatus status)
        {
            // Arrange
            var whLink = CreateWHSystemLink(eolStatus: status);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act & Assert
            Assert.True(linkModel.IsEoL);
        }

        [Fact]
        public void IsEoL_Set_True_ShouldSetStatusToEOL4h()
        {
            // Arrange
            var whLink = CreateWHSystemLink(eolStatus: SystemLinkEolStatus.Normal);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.IsEoL = true;

            // Assert
            Assert.Equal(SystemLinkEolStatus.EOL4h, linkModel.EndOfLifeStatus);
        }

        [Fact]
        public void IsEoL_Set_False_ShouldSetStatusToNormal()
        {
            // Arrange
            var whLink = CreateWHSystemLink(eolStatus: SystemLinkEolStatus.EOL4h);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.IsEoL = false;

            // Assert
            Assert.Equal(SystemLinkEolStatus.Normal, linkModel.EndOfLifeStatus);
        }

        [Fact]
        public void Size_Set_Small_ShouldAddLabelS()
        {
            // Arrange
            var whLink = CreateWHSystemLink(size: SystemLinkSize.Large);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.Size = SystemLinkSize.Small;

            // Assert
            Assert.Equal(SystemLinkSize.Small, linkModel.Size);
            Assert.Single(linkModel.Labels);
            Assert.Equal("S", ((LinkLabelModel)linkModel.Labels[0]).Content);
        }

        [Fact]
        public void Size_Set_Medium_ShouldAddLabelM()
        {
            // Arrange
            var whLink = CreateWHSystemLink(size: SystemLinkSize.Large);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.Size = SystemLinkSize.Medium;

            // Assert
            Assert.Equal(SystemLinkSize.Medium, linkModel.Size);
            Assert.Single(linkModel.Labels);
            Assert.Equal("M", ((LinkLabelModel)linkModel.Labels[0]).Content);
        }

        [Fact]
        public void Size_Set_Large_ShouldHaveNoLabel()
        {
            // Arrange
            var whLink = CreateWHSystemLink(size: SystemLinkSize.Small);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.Size = SystemLinkSize.Large;

            // Assert
            Assert.Equal(SystemLinkSize.Large, linkModel.Size);
            Assert.Empty(linkModel.Labels);
        }

        [Fact]
        public void Size_Set_XLarge_ShouldAddLabelXL()
        {
            // Arrange
            var whLink = CreateWHSystemLink(size: SystemLinkSize.Large);
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.Size = SystemLinkSize.XLarge;

            // Assert
            Assert.Equal(SystemLinkSize.XLarge, linkModel.Size);
            Assert.Single(linkModel.Labels);
            Assert.Equal("XL", ((LinkLabelModel)linkModel.Labels[0]).Content);
        }

        [Theory]
        [InlineData(SystemLinkMassStatus.Normal)]
        [InlineData(SystemLinkMassStatus.Critical)]
        [InlineData(SystemLinkMassStatus.Verge)]
        public void MassStatus_GetSet_ShouldWork(SystemLinkMassStatus status)
        {
            // Arrange
            var whLink = CreateWHSystemLink();
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.MassStatus = status;

            // Assert
            Assert.Equal(status, linkModel.MassStatus);
        }

        [Fact]
        public void IsRouteWaypoint_DefaultValue_ShouldBeFalse()
        {
            // Arrange
            var whLink = CreateWHSystemLink();
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Assert
            Assert.False(linkModel.IsRouteWaypoint);
        }

        [Fact]
        public void IsRouteWaypoint_SetTrue_ShouldReturnTrue()
        {
            // Arrange
            var whLink = CreateWHSystemLink();
            var sourceNode = CreateNodeModel();
            var targetNode = CreateNodeModel();
            var linkModel = new EveSystemLinkModel(whLink, sourceNode, targetNode);

            // Act
            linkModel.IsRouteWaypoint = true;

            // Assert
            Assert.True(linkModel.IsRouteWaypoint);
        }
    }
}