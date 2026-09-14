using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Services.WHSignatures;

namespace WHMapper.Tests.WHHelper;

public class WHSignatureHelperImportTests
{
    private const int WH_ID = 1;
    private const string SCAN_USER = "FOOBAR";
    private const string UNRESOLVED_SIG_NAME = "IGU-360";
    private const string WORMHOLE_SIG_NAME = "WAM-436";
    private const string UNSTABLE_WORMHOLE = "Unstable Wormhole";

    // One unresolved signature and one signature resolved as a wormhole.
    private static readonly string SCAN = string.Join(Environment.NewLine,
        $"{UNRESOLVED_SIG_NAME}\tCosmic Signature\t\t\t0,0%\t37,21 AU",
        $"{WORMHOLE_SIG_NAME}\tCosmic Signature\tWormhole\t{UNSTABLE_WORMHOLE}\t100,0%\t101 km");

    [Fact]
    public async Task ImportScanResult_WhenScanIsUnchanged_ReportsNoChange()
    {
        var existingSignatures = new List<WHSignature>
        {
            new(WH_ID, UNRESOLVED_SIG_NAME, WHSignatureGroup.Unknow, string.Empty),
            new(WH_ID, WORMHOLE_SIG_NAME, WHSignatureGroup.Wormhole, UNSTABLE_WORMHOLE)
        };
        var sut = new WHSignatureHelper(CreateRepository(existingSignatures).Object);

        var result = await sut.ImportScanResult(SCAN_USER, WH_ID, SCAN, false);

        Assert.True(result.Persisted);
        Assert.Equal(0, result.FullyScannedCreatedCount);
        Assert.Equal(0, result.FullyScannedChangedCount);
    }

    [Fact]
    public async Task ImportScanResult_WhenGroupIsResolved_ReportsOneChange()
    {
        var existingSignatures = new List<WHSignature>
        {
            new(WH_ID, UNRESOLVED_SIG_NAME, WHSignatureGroup.Unknow, string.Empty),
            new(WH_ID, WORMHOLE_SIG_NAME, WHSignatureGroup.Unknow, string.Empty)
        };
        var sut = new WHSignatureHelper(CreateRepository(existingSignatures).Object);

        var result = await sut.ImportScanResult(SCAN_USER, WH_ID, SCAN, false);

        Assert.True(result.Persisted);
        Assert.Equal(0, result.FullyScannedCreatedCount);
        Assert.Equal(1, result.FullyScannedChangedCount);
        Assert.Equal(WHSignatureGroup.Wormhole, existingSignatures[1].Group);
        Assert.Equal(UNSTABLE_WORMHOLE, existingSignatures[1].Type);
    }

    [Fact]
    public async Task ImportScanResult_WhenSignaturesAreNew_CountsOnlyFullyScannedAsCreated()
    {
        var repository = CreateRepository(new List<WHSignature>());
        var sut = new WHSignatureHelper(repository.Object);

        var result = await sut.ImportScanResult(SCAN_USER, WH_ID, SCAN, false);

        Assert.True(result.Persisted);
        // Both signatures are created, but only the one at 100% counts.
        repository.Verify(r => r.Create(It.Is<IEnumerable<WHSignature>>(sigs => sigs.Count() == 2)), Times.Once);
        Assert.Equal(1, result.FullyScannedCreatedCount);
        Assert.Equal(0, result.FullyScannedChangedCount);
    }

    [Fact]
    public async Task ImportScanResult_WhenGroupIsResolvedBelowFullSignal_ReportsNoChange()
    {
        var existingSignatures = new List<WHSignature>
        {
            new(WH_ID, WORMHOLE_SIG_NAME, WHSignatureGroup.Unknow, string.Empty)
        };
        var sut = new WHSignatureHelper(CreateRepository(existingSignatures).Object);
        string partialScan = $"{WORMHOLE_SIG_NAME}\tCosmic Signature\tWormhole\t\t42,5%\t12,3 AU";

        var result = await sut.ImportScanResult(SCAN_USER, WH_ID, partialScan, false);

        Assert.True(result.Persisted);
        Assert.Equal(WHSignatureGroup.Wormhole, existingSignatures[0].Group);
        Assert.Equal(0, result.FullyScannedChangedCount);
    }

    [Theory]
    [InlineData("100,0%")]
    [InlineData("100.0%")]
    [InlineData("100.0 %")]
    public async Task ImportScanResult_WhenSignalStrengthUsesAnyDecimalSeparator_CountsFullyScanned(string signalStrength)
    {
        var sut = new WHSignatureHelper(CreateRepository(new List<WHSignature>()).Object);
        string scan = $"{WORMHOLE_SIG_NAME}\tCosmic Signature\tWormhole\t{UNSTABLE_WORMHOLE}\t{signalStrength}\t101 km";

        var result = await sut.ImportScanResult(SCAN_USER, WH_ID, scan, false);

        Assert.Equal(1, result.FullyScannedCreatedCount);
    }

    private static Mock<IWHSignatureRepository> CreateRepository(List<WHSignature> existingSignatures)
    {
        var repository = new Mock<IWHSignatureRepository>();
        repository.Setup(r => r.GetByWHId(WH_ID)).ReturnsAsync(existingSignatures);
        repository.Setup(r => r.Update(It.IsAny<IEnumerable<WHSignature>>()))
            .ReturnsAsync((IEnumerable<WHSignature> signatures) => signatures.Cast<WHSignature?>().ToList());
        repository.Setup(r => r.Create(It.IsAny<IEnumerable<WHSignature>>()))
            .ReturnsAsync((IEnumerable<WHSignature> signatures) => signatures.Cast<WHSignature?>().ToList());
        return repository;
    }
}
