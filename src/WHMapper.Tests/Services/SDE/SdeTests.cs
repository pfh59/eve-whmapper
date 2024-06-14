using AutoFixture.Xunit2;
using Moq;
using System.IO.Abstractions;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.Services.SDE
{
    public class SdeTests
    {
        #region IsNewSDEAvailabile()
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
        public async Task WhenChecksumIsEqual_RequestingNewSdeAvailable_ReturnsFalse (
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFile> file,
            SDEServices sut,
            string checksum
            )
        {
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksum);
            file.Setup(x => x.ReadLines(It.IsAny<string>())).Returns([checksum]);

            var result = await sut.IsNewSDEAvailable();

            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public async Task WhenChecksumDiffers_RequestingNewSdeAvailable_ReturnsTrue (
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFile> file,
            SDEServices sut,
            string checksumA,
            string checksumB
        )
        {
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksumA);
            file.Setup(x => x.ReadLines(It.IsAny<string>())).Returns([checksumB]);

            var result = await sut.IsNewSDEAvailable();

            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public async Task WhenSuppliedChecksumIsNull_RequestingNewSdeAvailable_ReturnsTrue(
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFile> file,
            SDEServices sut,
            string checksumB
)
        {
            string checksumA = null!;
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksumA);
            file.Setup(x => x.ReadLines(It.IsAny<string>())).Returns([checksumB]);

            var result = await sut.IsNewSDEAvailable();

            Assert.False(result);
        }
        #endregion
    }
}
