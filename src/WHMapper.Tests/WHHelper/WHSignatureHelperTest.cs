using System;
using System.Collections.ObjectModel;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.WHColor;
using WHMapper.Services.WHSignature;
using WHMapper.Services.WHSignatures;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

[Collection("C9-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class WHSignatureHelperTest
{
    private const int WH_ID = 1;
    private const string SCAN_USER = "FOOBAR";
    private const string DSCAN = "IGU-360\tCosmic Signature\t\t\t0,0%\t37,21 AU\nWAM-436\tCosmic Signature\tWormhole\tUnstable Wormhole\t100,0%\t101 km\nRNN-835\tCosmic Signature\t\t\t0,0%\t21,98 AU\nETT-010\tCosmic Signature\t\t\t0,0%\t27,03 AU\nHBO-538\tCosmic Signature\t\t\t0,0%\t38,17 AU\nOBF-800\tCosmic Signature\t\t\t0,0%\t35,48 AU\nBNU-740\tCosmic Signature\t\t\t0,0%\t34,86 AU\nAWU-108\tCosmic Signature\tGas Site\t\t0,0%\t26,19 AU\nDXY-229\tCosmic Signature\tGas Site\tSizeable Perimeter Reservoir\t100,0%\t28,07 AU\nQBJ-502\tCosmic Signature\tRelic Site\tRuined Guristas Monument Site\t100,0%\t25,45 AU\nXQX-010\tCosmic Signature\tWormhole\tUnstable Wormhole\t100,0%\t23,50 AU";
    private const string FIRST_SIG_NAME = "IGU-360";
    private const string LAST_SIG_NAME = "XQX-010";
    private const string UNSTABLE_WORMHOLE = "Unstable Wormhole";

    private IWHSignatureHelper _whHelper;

    public WHSignatureHelperTest()
    {
        _whHelper = new WHSignatureHelper(null!);
    }


    [Fact]
    public async Task Validate_Scan_Result_Test()
    {
        bool emptyRes = await _whHelper.ValidateScanResult(string.Empty);
        Assert.False(emptyRes);

        bool valideScan = await _whHelper.ValidateScanResult(DSCAN);
        Assert.True(valideScan);

        bool invalidateScan = await _whHelper.ValidateScanResult(SCAN_USER);
        Assert.False(invalidateScan);
    }

    [Fact]
    public async Task Parse_Scan_Result_Test()
    {
        var emptyRes = await _whHelper.ParseScanResult(SCAN_USER, WH_ID,string.Empty);
        Assert.NotNull(emptyRes);
        Assert.Empty(emptyRes);

        var parseDSCAN1= await _whHelper.ParseScanResult(SCAN_USER, WH_ID,DSCAN);
        Assert.NotNull(parseDSCAN1);
        Assert.NotEmpty(parseDSCAN1);
        Assert.Equal(11, parseDSCAN1.Count());

        var firstSig = parseDSCAN1.First();

        Assert.Equal(FIRST_SIG_NAME, firstSig.Name);
        Assert.Equal(WHSignatureGroup.Unknow,firstSig.Group);
        Assert.NotNull(firstSig.Type);
        Assert.Equal(string.Empty, firstSig.Type);

        var lastSig = parseDSCAN1.Last();
        Assert.Equal(LAST_SIG_NAME, lastSig.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, lastSig.Group);
        Assert.NotNull(lastSig.Type);
        Assert.Equal(UNSTABLE_WORMHOLE, lastSig.Type);
    }

    [Fact]
    public async Task Import_Scan_Result_Test()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>  _whHelper.ImportScanResult(SCAN_USER, WH_ID, string.Empty, false));
        Assert.Equal("Bad signatures format", ex.Message);

        /*
        bool valideScan = await _whHelper.ImportScanResult(SCAN_USER, WH_ID, DSCAN, false);
        Assert.True(valideScan);

        bool invalidateScan = await _whHelper.ImportScanResult(SCAN_USER, WH_ID, SCAN_USER, false);
        Assert.False(invalidateScan);

        bool valideScan2 = await _whHelper.ImportScanResult(SCAN_USER, WH_ID, DSCAN, true);
        Assert.True(valideScan2);*/
    }

    [Fact]
    public async Task Analyzed_Signature_Test()
    {
        var currentSystemSigs = new Collection<WHMapper.Models.Db.WHSignature>
        {
            new WHSignature(0, FIRST_SIG_NAME,WHSignatureGroup.Unknow,string.Empty,SCAN_USER),
            new WHSignature(0, LAST_SIG_NAME,WHSignatureGroup.Wormhole,UNSTABLE_WORMHOLE,SCAN_USER)
        };

        var parsedSigs = new Collection<WHMapper.Models.Db.WHSignature>
        {
            new WHSignature(0, FIRST_SIG_NAME, WHSignatureGroup.Unknow,string.Empty,SCAN_USER),
            new WHMapper.Models.Db.WHSignature(0, LAST_SIG_NAME, WHSignatureGroup.Wormhole,UNSTABLE_WORMHOLE,SCAN_USER)      
        };

        var analyzedSigs = await _whHelper.AnalyzedSignatures(parsedSigs, currentSystemSigs, false);
        Assert.NotNull(analyzedSigs);
        Assert.NotEmpty(analyzedSigs);
        Assert.Equal(2, analyzedSigs.Count());

        var firstSig = analyzedSigs.First();
        Assert.Equal(FIRST_SIG_NAME, firstSig.Name);
        Assert.Equal(WHAnalizedSignatureEnums.toUpdate, firstSig.Status);
        Assert.Equal(WHSignatureGroup.Unknow.ToString(), firstSig.Group);
        Assert.Equal(string.Empty, firstSig.Type);

        parsedSigs.RemoveAt(0);
        parsedSigs.Add(new WHSignature(0, "NEW_SIG", WHSignatureGroup.Unknow, string.Empty, SCAN_USER));

        analyzedSigs = await _whHelper.AnalyzedSignatures(parsedSigs, currentSystemSigs, true);
        Assert.NotNull(analyzedSigs);
        Assert.NotEmpty(analyzedSigs);
        Assert.Equal(3, analyzedSigs.Count());
        Assert.Equal(WHAnalizedSignatureEnums.toAdd, analyzedSigs.First().Status);
        Assert.Equal(WHAnalizedSignatureEnums.toUpdate, analyzedSigs.ElementAt(1).Status);
        Assert.Equal(WHAnalizedSignatureEnums.toDelete, analyzedSigs.Last().Status);
    }

}


