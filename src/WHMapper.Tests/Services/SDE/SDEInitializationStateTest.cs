using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Services.SDE;
using Xunit;

namespace WHMapper.Tests.Services.SDE;

public class SDEInitializationStateTest
{
    private readonly Mock<ILogger<SDEInitializationState>> _mockLogger;
    private readonly SDEInitializationState _sut;

    public SDEInitializationStateTest()
    {
        _mockLogger = new Mock<ILogger<SDEInitializationState>>();
        _sut = new SDEInitializationState(_mockLogger.Object);
    }

    [Fact]
    public void SDEInitializationState_CanBeInstantiated()
    {
        Assert.NotNull(_sut);
    }

    [Fact]
    public void IsInitializationInProgress_InitialValue_IsFalse()
    {
        Assert.False(_sut.IsInitializationInProgress);
    }

    [Fact]
    public void CurrentProgressMessage_InitialValue_IsEmpty()
    {
        Assert.Equal(string.Empty, _sut.CurrentProgressMessage);
    }

    [Fact]
    public void TryAcquireInitializationLock_WhenNotInProgress_ReturnsTrue()
    {
        var result = _sut.TryAcquireInitializationLock();

        Assert.True(result);
        Assert.True(_sut.IsInitializationInProgress);
    }

    [Fact]
    public void TryAcquireInitializationLock_WhenAlreadyInProgress_ReturnsFalse()
    {
        _sut.TryAcquireInitializationLock();

        var result = _sut.TryAcquireInitializationLock();

        Assert.False(result);
    }

    [Fact]
    public void TryAcquireInitializationLock_MultipleCalls_OnlyFirstSucceeds()
    {
        var firstResult = _sut.TryAcquireInitializationLock();
        var secondResult = _sut.TryAcquireInitializationLock();
        var thirdResult = _sut.TryAcquireInitializationLock();

        Assert.True(firstResult);
        Assert.False(secondResult);
        Assert.False(thirdResult);
    }

    [Fact]
    public void ReleaseInitializationLock_ResetsIsInitializationInProgress()
    {
        _sut.TryAcquireInitializationLock();
        Assert.True(_sut.IsInitializationInProgress);

        _sut.ReleaseInitializationLock();

        Assert.False(_sut.IsInitializationInProgress);
    }

    [Fact]
    public void ReleaseInitializationLock_ResetsCurrentProgressMessage()
    {
        _sut.TryAcquireInitializationLock();
        _sut.SetProgressMessage("Test message");

        _sut.ReleaseInitializationLock();

        Assert.Equal(string.Empty, _sut.CurrentProgressMessage);
    }

    [Fact]
    public void ReleaseInitializationLock_AllowsNewLockAcquisition()
    {
        _sut.TryAcquireInitializationLock();
        _sut.ReleaseInitializationLock();

        var result = _sut.TryAcquireInitializationLock();

        Assert.True(result);
    }

    [Fact]
    public void ReleaseInitializationLock_TriggersOnInitializationCompletedEvent()
    {
        var eventTriggered = false;
        _sut.OnInitializationCompleted += () => eventTriggered = true;
        _sut.TryAcquireInitializationLock();

        _sut.ReleaseInitializationLock();

        Assert.True(eventTriggered);
    }

    [Fact]
    public void SetProgressMessage_UpdatesCurrentProgressMessage()
    {
        var message = "Loading SDE data...";

        _sut.SetProgressMessage(message);

        Assert.Equal(message, _sut.CurrentProgressMessage);
    }

    [Fact]
    public void SetProgressMessage_TriggersOnProgressChangedEvent()
    {
        string? receivedMessage = null;
        _sut.OnProgressChanged += msg => receivedMessage = msg;
        var message = "Processing solar systems...";

        _sut.SetProgressMessage(message);

        Assert.Equal(message, receivedMessage);
    }

    [Fact]
    public void SetProgressMessage_TriggersEventWithCorrectMessage()
    {
        var messages = new List<string>();
        _sut.OnProgressChanged += msg => messages.Add(msg);

        _sut.SetProgressMessage("Message 1");
        _sut.SetProgressMessage("Message 2");
        _sut.SetProgressMessage("Message 3");

        Assert.Equal(3, messages.Count);
        Assert.Equal("Message 1", messages[0]);
        Assert.Equal("Message 2", messages[1]);
        Assert.Equal("Message 3", messages[2]);
    }

    [Fact]
    public async Task WaitForInitializationAsync_WhenNotInProgress_ReturnsImmediately()
    {
        await _sut.WaitForInitializationAsync();

        Assert.False(_sut.IsInitializationInProgress);
    }

    [Fact]
    public async Task WaitForInitializationAsync_WaitsUntilInitializationCompletes()
    {
        _sut.TryAcquireInitializationLock();
        var waitTask = _sut.WaitForInitializationAsync();

        Assert.False(waitTask.IsCompleted);

        _sut.ReleaseInitializationLock();
        await waitTask;

        Assert.True(waitTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WaitForInitializationAsync_WithCancellation_ThrowsTaskCanceledException()
    {
        _sut.TryAcquireInitializationLock();
        using var cts = new CancellationTokenSource();

        var waitTask = _sut.WaitForInitializationAsync(cts.Token);
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => waitTask);
    }

    [Fact]
    public async Task WaitForInitializationAsync_MultipleWaiters_AllCompleteWhenReleased()
    {
        _sut.TryAcquireInitializationLock();

        var waitTask1 = _sut.WaitForInitializationAsync();
        var waitTask2 = _sut.WaitForInitializationAsync();
        var waitTask3 = _sut.WaitForInitializationAsync();

        Assert.False(waitTask1.IsCompleted);
        Assert.False(waitTask2.IsCompleted);
        Assert.False(waitTask3.IsCompleted);

        _sut.ReleaseInitializationLock();

        await Task.WhenAll(waitTask1, waitTask2, waitTask3);

        Assert.True(waitTask1.IsCompletedSuccessfully);
        Assert.True(waitTask2.IsCompletedSuccessfully);
        Assert.True(waitTask3.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task ConcurrentLockAcquisition_OnlyOneSucceeds()
    {
        var results = new List<bool>();
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var result = _sut.TryAcquireInitializationLock();
                lock (results)
                {
                    results.Add(result);
                }
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Single(results.Where(r => r));
        Assert.Equal(9, results.Count(r => !r));
    }

    [Fact]
    public void ReleaseInitializationLock_WhenNoLockAcquired_DoesNotThrow()
    {
        var exception = Record.Exception(() => _sut.ReleaseInitializationLock());

        Assert.Null(exception);
    }

    [Fact]
    public void OnProgressChanged_WithNoSubscribers_DoesNotThrow()
    {
        var exception = Record.Exception(() => _sut.SetProgressMessage("Test"));

        Assert.Null(exception);
    }

    [Fact]
    public void OnInitializationCompleted_WithNoSubscribers_DoesNotThrow()
    {
        _sut.TryAcquireInitializationLock();

        var exception = Record.Exception(() => _sut.ReleaseInitializationLock());

        Assert.Null(exception);
    }
}