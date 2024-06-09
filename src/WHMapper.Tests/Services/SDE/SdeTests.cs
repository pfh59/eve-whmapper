using AutoFixture.Xunit2;
using Moq;
using System.IO.Abstractions;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.Services.SDE
{
    public class SdeTests
    {
        [Theory, AutoDomainData]
        public void WhenDirectoryExists_ExtractSuccess_ReturnsTrue(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(@"./Resources/SDE/universe")).Returns(true);
            var result = sut.ExtractSuccess;
            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryDoesNotExist_ExtractSuccess_ReturnsFalse(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(@"./Resources/SDE/universe")).Returns(false);
            var result = sut.ExtractSuccess;
            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryRetusnNull_ExtractSuccess_ReturnsFalse(
            [Frozen] Mock<IDirectory> directory,
            SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(@"./Resources/SDE/universe")).Returns(null!);
            var result = sut.ExtractSuccess;
            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public void 

    }
}
