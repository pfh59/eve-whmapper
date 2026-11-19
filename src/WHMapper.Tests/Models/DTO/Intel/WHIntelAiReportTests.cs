using WHMapper.Models.DTO.Intel;

namespace WHMapper.Tests.Models.DTO.Intel;

public class WHIntelAiReportTests
{
    [Fact]
    public void GetSections_SplitsLeadingHeadingFromText()
    {
        var report = new WHIntelAiReport(WHIntelAiStatus.Generated,
            "Résidents : Resident Corp vit ici.\n\nProfil d'activité: Surtout PVE.\nUne phrase sans titre mais avec deux-points plus loin dans le texte : ici", DateTime.UtcNow);

        var sections = report.GetSections();

        Assert.Equal(3, sections.Count);
        Assert.Equal(new WHIntelAiSection("Résidents", "Resident Corp vit ici."), sections[0]);
        Assert.Equal(new WHIntelAiSection("Profil d'activité", "Surtout PVE."), sections[1]);
        Assert.Null(sections[2].Heading);
    }

    [Fact]
    public void GetSections_WhenNoText_ReturnsEmpty()
    {
        Assert.Empty(WHIntelAiReport.WithStatus(WHIntelAiStatus.Unavailable).GetSections());
    }
}
