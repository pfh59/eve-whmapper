using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WHMapper.Models.DTO.Intel;
using WHMapper.Services.WHIntel;

namespace WHMapper.Tests.Services.WHIntel;

public class WHIntelAiReporterTests
{
    private const int RESIDENT_CORP = 98000001;
    private const int VISITOR_CORP = 98000003;
    private const int HAC_TYPE = 12005;
    private const string REPORT_TEXT = "Residents: Resident Corp\nActivity profile: PVP";
    private const string LANGUAGE = "fr-FR";

    private static readonly DateTime DAY = new(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IChatClient> _chatClient = new();
    private readonly WHIntelAiReporter _sut;
    private IList<ChatMessage>? _sentMessages;

    public WHIntelAiReporterTests()
    {
        _sut = new WHIntelAiReporter(_chatClient.Object, NullLogger<WHIntelAiReporter>.Instance);
    }

    private void SetupResponse(ChatResponse response) =>
        _chatClient
            .Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((messages, _, _) => _sentMessages = messages.ToList())
            .ReturnsAsync(response);

    [Fact]
    public async Task GenerateAsync_SendsInstructionsAndResolvedDataAndReturnsText()
    {
        SetupResponse(new ChatResponse(new ChatMessage(ChatRole.Assistant, "  " + REPORT_TEXT + "\n")));

        string? text = await _sut.GenerateAsync(CreateReport(), LANGUAGE);

        Assert.Equal(REPORT_TEXT, text);
        Assert.True(_sut.IsEnabled);

        var system = _sentMessages!.Single(x => x.Role == ChatRole.System).Text;
        Assert.All(["Residents", "Activity profile", "Active timezone", "Likely fleet composition", "Threat assessment"],
            heading => Assert.Contains(heading, system));
        Assert.Contains("BCP 47", system);
        Assert.Contains("200 words", system);
        Assert.Contains("no question", system);

        var data = _sentMessages!.Single(x => x.Role == ChatRole.User).Text;
        Assert.Contains($"\"language\":\"{LANGUAGE}\"", data);
        Assert.Contains("J165153", data);
        Assert.Contains("Resident Corp", data);
        Assert.Contains("Muninn", data);
        Assert.Contains("Heavy Assault Cruiser", data);
    }

    [Fact]
    public async Task GenerateAsync_RemovesInlineThinking()
    {
        SetupResponse(new ChatResponse(new ChatMessage(ChatRole.Assistant, "<think>\nresidents look active\n</think>\n" + REPORT_TEXT)));

        Assert.Equal(REPORT_TEXT, await _sut.GenerateAsync(CreateReport(), LANGUAGE));
    }

    [Fact]
    public async Task GenerateAsync_WhenResponseIsFiltered_ReturnsNull()
    {
        SetupResponse(new ChatResponse(new ChatMessage(ChatRole.Assistant, "partial")) { FinishReason = ChatFinishReason.ContentFilter });

        Assert.Null(await _sut.GenerateAsync(CreateReport(), LANGUAGE));
    }

    [Fact]
    public async Task GenerateAsync_WhenResponseIsEmpty_ReturnsNull()
    {
        SetupResponse(new ChatResponse(new ChatMessage(ChatRole.Assistant, "   ")));

        Assert.Null(await _sut.GenerateAsync(CreateReport(), LANGUAGE));
    }

    [Fact]
    public async Task GenerateAsync_WhenProviderFails_ReturnsNull()
    {
        _chatClient
            .Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized"));

        Assert.Null(await _sut.GenerateAsync(CreateReport(), LANGUAGE));
    }

    [Fact]
    public async Task GenerateAsync_WhenCancelled_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _chatClient
            .Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GenerateAsync(CreateReport(), LANGUAGE, cancellation.Token));
    }

    [Fact]
    public async Task DisabledReporter_NeverGenerates()
    {
        var disabled = new DisabledWHIntelAiReporter();

        Assert.False(disabled.IsEnabled);
        Assert.Null(await disabled.GenerateAsync(CreateReport(), LANGUAGE));
    }

    private static WHIntelReport CreateReport()
    {
        var killmails = Enumerable.Range(0, 5)
            .Select(i => new KillmailBuilder(DAY.AddDays(-i)).Victim(VISITOR_CORP).Attacker(RESIDENT_CORP, HAC_TYPE, finalBlow: true).Build())
            .ToList();
        var analysis = new WHIntelAnalyzer().Analyze(killmails);

        return new WHIntelReport(
            WHIntelStatus.Available,
            KillmailBuilder.SYSTEM_ID,
            "J165153",
            [new WHIntelKill(1, DAY, 587, 1, VISITOR_CORP, null, 1, RESIDENT_CORP, 1_000_000, false, true)],
            new WHIntelTopLists([new WHIntelTopEntry(1, 2, "Top Pilot", 12)], [], []),
            analysis,
            new Dictionary<int, string> { [RESIDENT_CORP] = "Resident Corp", [HAC_TYPE] = "Muninn" },
            new Dictionary<int, string> { [HAC_TYPE] = "Heavy Assault Cruiser" });
    }
}
