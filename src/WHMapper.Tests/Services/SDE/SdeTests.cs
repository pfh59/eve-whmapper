﻿using AutoFixture.Xunit2;
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
            var result = sut.IsExtractionSuccesful();
            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryDoesNotExist_ExtractSuccess_ReturnsFalse(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(@"./Resources/SDE/universe")).Returns(false);
            var result = sut.IsExtractionSuccesful();
            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryRetusnNull_ExtractSuccess_ReturnsFalse(
            [Frozen] Mock<IDirectory> directory,
            SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(@"./Resources/SDE/universe")).Returns(null!);
            var result = sut.IsExtractionSuccesful();
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

        #region ClearSDEResources()
        private string SDEDirectory => @"./Resources/SDE/";

        [Theory, AutoDomainData]
        public async Task WhenSDEDirectoryExists_ClearingSdeResources_RemovesDirectory(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(SDEDirectory)).Returns(true).Verifiable();

            await sut.ClearSDEResources();

            var result = await sut.ClearSDEResources();

            Assert.True(result);
            directory.Verify(x => x.Exists(SDEDirectory), Times.Once);
            directory.Verify(x => x.Delete(SDEDirectory, true));
            directory.VerifyNoOtherCalls();
        }

        [Theory, AutoDomainData]
        public async Task WhenSDEDirectoryDoesNotExists_ClearingSdeResources_DoesNothingAndReturnsTrue(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(SDEDirectory)).Returns(false).Verifiable();

            var result = await sut.ClearSDEResources();

            Assert.True(result);
            directory.Verify(x => x.Exists(SDEDirectory), Times.Once);
            directory.VerifyNoOtherCalls();
        }

        [Theory, AutoDomainData]
        public async Task WhenDirectoryThrowsException_ClearingSdeResources_DoesNothingAndReturnsFalse(
                [Frozen] Mock<IDirectory> directory,
                SDEServices sut
            )
        {
            directory.Setup(x => x.Exists(SDEDirectory)).Throws(new Exception("test")).Verifiable();

            var result = await sut.ClearSDEResources();

            Assert.False(result);
            directory.Verify(x => x.Exists(SDEDirectory), Times.Once);
            directory.VerifyNoOtherCalls();
        }

        #endregion
    }
}
