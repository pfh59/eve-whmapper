using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.Services.SDE
{
    public class SdeServicesTests
    {
        #region IsNewSDEAvailabile()
        [Theory, AutoDomainData]
        public void WhenDirectoryExists_ExtractSuccess_ReturnsTrue(
                [Frozen] Mock<IFileSystem> fileSystem,
                SDEServiceManager sut
            )
        {
            fileSystem.Setup(x => x.Directory.Exists(@"./Resources/SDE/universe")).Returns(true);
            var result = sut.IsExtractionSuccesful();
            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryDoesNotExist_ExtractSuccess_ReturnsFalse(
                [Frozen] Mock<IFileSystem> fileSystem,
                SDEServiceManager sut
            )
        {
            fileSystem.Setup(x => x.Directory.Exists(@"./Resources/SDE/universe")).Returns(false);
            var result = sut.IsExtractionSuccesful();
            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public void WhenDirectoryReturnsNull_ExtractSuccess_ReturnsFalse(
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServiceManager sut
            )
        {
            fileSystem.Setup(x => x.Directory.Exists(@"./Resources/SDE/universe")).Returns(null!);
            var result = sut.IsExtractionSuccesful();
            Assert.False(result);
        }

        [Theory, AutoDomainData]
        public async Task WhenChecksumIsEqual_RequestingNewSdeAvailable_ReadsOnceAndReturnsFalse(
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServiceManager sut,
            string checksum
            )
        {
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksum);
            fileSystem.Setup(x => x.File.ReadLines(It.IsAny<string>())).Returns([checksum]).Verifiable();

            var result = await sut.IsNewSDEAvailable();

            Assert.False(result);
            fileSystem.Verify(x => x.File.ReadLines(It.IsAny<string>()), Times.Once());
        }

        [Theory, AutoDomainData]
        public async Task WhenLocalChecksumIsEmpty_RequestingNewSdeAvailable_ReturnsTrue(
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServiceManager sut,
            string checksumA
        )
        {
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksumA);
            fileSystem.Setup(x => x.File.ReadLines(It.IsAny<string>())).Returns([""]).Verifiable();

            var result = await sut.IsNewSDEAvailable();

            Assert.True(result);
            fileSystem.Verify(x => x.File.ReadLines(It.IsAny<string>()), Times.Once());
        }

        [Theory, AutoDomainData]
        public async Task WhenChecksumDiffers_RequestingNewSdeAvailable_ReturnsTrue(
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServiceManager sut,
            string checksumA,
            string checksumB
        )
        {
            fileSystem.Setup(x => x.File.ReadLines(It.IsAny<string>())).Returns([checksumB]).Verifiable();
            dataSupplier.Setup(x => x.GetChecksum()).Returns(checksumA);

            var result = await sut.IsNewSDEAvailable();

            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public async Task WhenSuppliedChecksumIsNull_RequestingNewSdeAvailable_ReturnsTrue(
            [Frozen] Mock<ISDEDataSupplier> dataSupplier,
            [Frozen] Mock<IFile> file,
            SDEServiceManager sut,
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
                [Frozen] Mock<IFileSystem> filesystem,
                SDEServiceManager sut
            )
        {
            filesystem.Setup(x => x.Directory.Exists(SDEDirectory)).Returns(true).Verifiable();

            var result = await sut.ClearSDEResources();

            Assert.True(result);
            filesystem.Verify(x => x.Directory.Exists(SDEDirectory), Times.Once);
            filesystem.Verify(x => x.Directory.Delete(SDEDirectory, true));
            filesystem.VerifyNoOtherCalls();
        }

        [Theory, AutoDomainData]
        public async Task WhenSDEDirectoryDoesNotExists_ClearingSdeResources_DoesNothingAndReturnsTrue(
                [Frozen] Mock<IFileSystem> fileSystem,
                SDEServiceManager sut
            )
        {
            fileSystem.Setup(x => x.Directory.Exists(SDEDirectory)).Returns(false).Verifiable();

            var result = await sut.ClearSDEResources();

            Assert.True(result);
            fileSystem.Verify(x => x.Directory.Exists(SDEDirectory), Times.Once);
            fileSystem.VerifyNoOtherCalls();
        }

        [Theory, AutoDomainData]
        public async Task WhenDirectoryThrowsException_ClearingSdeResources_DoesNothingAndReturnsFalse(
                [Frozen] Mock<IFileSystem> fileSystem,
                SDEServiceManager sut
            )
        {
            fileSystem.Setup(x => x.Directory.Exists(SDEDirectory)).Throws(new Exception("test")).Verifiable();

            var result = await sut.ClearSDEResources();

            Assert.False(result);
            fileSystem.Verify(x => x.Directory.Exists(SDEDirectory), Times.Once);
            fileSystem.VerifyNoOtherCalls();
        }

        #endregion
        #region DownloadSDE()
        [Theory, AutoDomainData]
        public async Task WhenFilesAreDownloadable_WhenDownloadingFiles_FilesystemContainsFiles(
            Mock<ISDEDataSupplier> dataSupplier,
            ILogger<SDEServiceManager> logger
        )
        {
            var mockFileSystem = new MockFileSystem();

            var contentBytes = Encoding.UTF8.GetBytes("whatever");
            var stream = new MemoryStream(contentBytes);
            dataSupplier.Setup(x => x.GetSDEDataStreamAsync()).ReturnsAsync(stream);
            dataSupplier.Setup(x => x.GetChecksum()).Returns("1234");

            var sut = new SDEServiceManager(logger, mockFileSystem, dataSupplier.Object, null);

            await sut.DownloadSDE();

            Assert.Contains(mockFileSystem.AllFiles, x => x.EndsWith("sde.zip"));
            Assert.Contains(mockFileSystem.AllFiles, x => x.EndsWith("checksum"));
            Assert.Equal(2, mockFileSystem.AllFiles.Count());
        }
        #endregion

        #region ExtractSDE()
        [Theory, AutoMoqData]
        public async Task WhenZipFileDoesntExist_WhenExtracting_ReturnsFalse(
            [Frozen] Mock<IFileSystem> fileSystem,
            SDEServiceManager sut
            )
        {
            fileSystem
                .Setup(x => x.File.Exists(It.IsAny<string>()))
                .Returns(false);

            var result = await sut.ExtractSDE();
            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task WhenValidZipFileExists_WhenExtracting_ExtractsArchiveAndReturnsTrue(
            ILogger<SDEServiceManager> logger
            )
        {
            byte[] fileContent = [80, 75, 3, 4, 20, 0, 0, 0, 0, 0, 193, 78, 207, 88, 229, 118, 24, 3, 3, 0, 0, 0, 3, 0, 0, 0, 7, 0, 0, 0, 115, 100, 101, 46, 116, 120, 116, 83, 68, 69, 80, 75, 1, 2, 20, 0, 20, 0, 0, 0, 0, 0, 193, 78, 207, 88, 229, 118, 24, 3, 3, 0, 0, 0, 3, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 1, 0, 32, 0, 0, 0, 0, 0, 0, 0, 115, 100, 101, 46, 116, 120, 116, 80, 75, 5, 6, 0, 0, 0, 0, 1, 0, 1, 0, 53, 0, 0, 0, 40, 0, 0, 0, 0, 0];

            var filesystem = new MockFileSystem();
            var data = new MockFileData(fileContent);
            filesystem.AddFile("C:/Resources/SDE/SDE.Zip", data);

            var sut = new SDEServiceManager(logger, filesystem, null!, null!);

            var result = await sut.ExtractSDE();
            Assert.True(result);
            Assert.Contains(filesystem.AllFiles, x => x.EndsWith("sde.txt"));
        }

        [Theory, AutoMoqData]
        public async Task WhenInvalidZipFileExists_WhenExtracting_ReturnsFalse(
            ILogger<SDEServiceManager> logger
            )
        {
            byte[] fileContent = [80, 75, 3, 4, 20, 0, 0, 0, 0, 0];

            var filesystem = new MockFileSystem();
            var data = new MockFileData(fileContent);
            filesystem.AddFile("C:/Resources/SDE/SDE.Zip", data);

            var sut = new SDEServiceManager(logger, filesystem, null!, null!);

            var result = await sut.ExtractSDE();
            Assert.False(result);
        }
        #endregion

        #region BuildCache()

        #endregion
    }
}