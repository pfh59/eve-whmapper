using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Tests.Models.Db;

public class WHSignatureTest
{
    private const int WH_ID = 1;
    private const string SIG_NAME = "WAM-436";
    private const string UNSTABLE_WORMHOLE = "Unstable Wormhole";

    [Theory]
    [InlineData(WHSignatureGroup.Wormhole, UNSTABLE_WORMHOLE, true)]
    [InlineData(WHSignatureGroup.Unknow, UNSTABLE_WORMHOLE, false)]
    [InlineData(WHSignatureGroup.Wormhole, "", false)]
    [InlineData(WHSignatureGroup.Wormhole, " ", false)]
    [InlineData(WHSignatureGroup.Wormhole, null, false)]
    public void IsIdentified_RequiresKnownGroupAndType(WHSignatureGroup group, string? type, bool expected)
    {
        var signature = new WHSignature(WH_ID, SIG_NAME, group, type);

        Assert.Equal(expected, signature.IsIdentified());
    }

    [Fact]
    public void HasSameContent_IgnoresAuditFieldsAndTreatsNullTypeAsEmpty()
    {
        var signature = new WHSignature(WH_ID, SIG_NAME, WHSignatureGroup.Wormhole, null);
        var other = new WHSignature(WH_ID, SIG_NAME, WHSignatureGroup.Wormhole, string.Empty)
        {
            UpdatedBy = "Other pilot",
            Updated = DateTime.UtcNow.AddHours(-1)
        };

        Assert.True(signature.HasSameContent(other));
    }

    [Fact]
    public void HasSameContent_WhenTypeDiffers_ReturnsFalse()
    {
        var signature = new WHSignature(WH_ID, SIG_NAME, WHSignatureGroup.Wormhole, UNSTABLE_WORMHOLE);
        var other = new WHSignature(WH_ID, SIG_NAME, WHSignatureGroup.Wormhole, string.Empty);

        Assert.False(signature.HasSameContent(other));
    }
}
