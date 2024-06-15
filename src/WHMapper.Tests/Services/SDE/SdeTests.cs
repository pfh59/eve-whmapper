using AutoFixture.Xunit2;
using Microsoft.Win32.SafeHandles;
using Moq;
using System.IO;
using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using WHMapper.Services.SDE;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System.IO.Enumeration;
using System.IO.Compression;

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
        #region DownloadSDE()
        [Theory, AutoDomainData]
        public async Task WhenFilesAreDownloadable_WhenDownloadingFiles_FilesystemContainsFiles(
            Mock<ISDEDataSupplier> dataSupplier,
            ILogger<SDEServices> logger
        )
        {
            var mockFileSystem = new MockFileSystem();

            var contentBytes = Encoding.UTF8.GetBytes("whatever");
            var stream = new MemoryStream(contentBytes);
            dataSupplier.Setup(x => x.GetSDEDataStreamAsync()).ReturnsAsync(stream);
            dataSupplier.Setup(x => x.GetChecksum()).Returns("1234");

            var sut = new SDEServices(logger, null, mockFileSystem, dataSupplier.Object);

            await sut.DownloadSDE();

            var test = mockFileSystem.AllFiles;

            Assert.True(mockFileSystem.FileExists("C:/Resources/SDE/SDE.zip"));
            Assert.True(mockFileSystem.FileExists("C:/Resources/SDE/checksum"));
            Assert.Equal(2, mockFileSystem.AllFiles.Count());
        }
        #endregion

        #region ExtractSDE()
        [Theory, AutoMoqData]
        public async Task WhenZipFileDoesntExist_WhenExtracting_ReturnsFalse (
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServices sut
            )
        {
            fileSystem
                .Setup(x => x.File.Exists(It.IsAny<string>()))
                .Returns(false);

            var result = await sut.ExtractSDE();
            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task WhenValidZipFileExists_WhenExtracting_ExtractsArchiveAndReturnsTrue (
            ILogger<SDEServices> logger
            )
        {
            byte[] fileContent = [80, 75, 3, 4, 20, 0, 0, 0, 0, 0, 193, 78, 207, 88, 229, 118, 24, 3, 3, 0, 0, 0, 3, 0, 0, 0, 7, 0, 0, 0, 115, 100, 101, 46, 116, 120, 116, 83, 68, 69, 80, 75, 1, 2, 20, 0, 20, 0, 0, 0, 0, 0, 193, 78, 207, 88, 229, 118, 24, 3, 3, 0, 0, 0, 3, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 1, 0, 32, 0, 0, 0, 0, 0, 0, 0, 115, 100, 101, 46, 116, 120, 116, 80, 75, 5, 6, 0, 0, 0, 0, 1, 0, 1, 0, 53, 0, 0, 0, 40, 0, 0, 0, 0, 0];

            var filesystem = new MockFileSystem();
            var data = new MockFileData(fileContent);
            filesystem.AddFile("C:/Resources/SDE/SDE.Zip", data);

            var sut = new SDEServices(logger, null!, filesystem, null!);

            var result = await sut.ExtractSDE();
            Assert.True(result);
            Assert.True(filesystem.FileExists("C:/Resources/SDE/universe/sde.txt"));
        }        
        
        [Theory, AutoMoqData]
        public async Task WhenInvalidZipFileExists_WhenExtracting_ReturnsFalse (
            ILogger<SDEServices> logger
            )
        {
            byte[] fileContent = [80, 75, 3, 4, 20, 0, 0, 0, 0, 0];

            var filesystem = new MockFileSystem();
            var data = new MockFileData(fileContent);
            filesystem.AddFile("C:/Resources/SDE/SDE.Zip", data);

            var sut = new SDEServices(logger, null!, filesystem, null!);

            var result = await sut.ExtractSDE();
            Assert.False(result);
        }
        #endregion
    }
}
