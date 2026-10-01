using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;

namespace WHMapper.Tests;

/// <summary>
/// Assertions on mocked <see cref="ILogger{TCategoryName}"/> instances.
/// </summary>
public static class LoggerMockExtensions
{
    /// <summary>
    /// Verifies the number of entries logged at <paramref name="level"/>, whatever their message or exception.
    /// </summary>
    [SuppressMessage("Performance", "CA1873", Justification = "Moq only inspects the expression tree; Log is never invoked.")]
    public static void VerifyLog<TCategoryName>(this Mock<ILogger<TCategoryName>> loggerMock, LogLevel level, Times times) =>
        loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
}
