using WHMapper.Models.DTO.Intel;

namespace WHMapper.Tests.Models.DTO.Intel;

public class WHIntelAiReportTests
{
    [Fact]
    public void GetSections_SplitsKnownHeadingsFromText()
    {
        var report = new WHIntelAiReport(WHIntelAiStatus.Generated,
            "Residents: Resident Corp lives here.\n\nactivity profile:  Mostly PVE.\nUnexpected line", DateTime.UtcNow);

        var sections = report.GetSections();

        Assert.Equal(3, sections.Count);
        Assert.Equal(new WHIntelAiSection("Residents", "Resident Corp lives here."), sections[0]);
        Assert.Equal(new WHIntelAiSection("Activity profile", "Mostly PVE."), sections[1]);
        Assert.Equal(new WHIntelAiSection(null, "Unexpected line"), sections[2]);
    }

    [Fact]
    public void GetSections_WhenNoText_ReturnsEmpty()
    {
        Assert.Empty(WHIntelAiReport.WithStatus(WHIntelAiStatus.Unavailable).GetSections());
    }
}
