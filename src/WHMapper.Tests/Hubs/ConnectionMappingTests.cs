using WHMapper.Hubs;

namespace WHMapper.Tests.Hubs;

public class ConnectionMappingTests
{
    [Fact]
    public void AddAndGetCount_FirstInsertion_ReturnsOne()
    {
        var mapping = new ConnectionMapping<int>();

        int count = mapping.AddAndGetCount(42, "conn-1");

        Assert.Equal(1, count);
    }

    [Fact]
    public void AddAndGetCount_SecondConnectionSameKey_ReturnsTwo()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        int count = mapping.AddAndGetCount(42, "conn-2");

        Assert.Equal(2, count);
    }

    [Fact]
    public void AddAndGetCount_DuplicateConnectionId_DoesNotIncrement()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        int count = mapping.AddAndGetCount(42, "conn-1");

        Assert.Equal(1, count);
    }

    [Fact]
    public void AddAndGetCount_DifferentKeys_TrackedIndependently()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(1, "conn-A");

        int countKey1 = mapping.AddAndGetCount(1, "conn-B");
        int countKey2 = mapping.AddAndGetCount(2, "conn-C");

        Assert.Equal(2, countKey1);
        Assert.Equal(1, countKey2);
    }

    [Fact]
    public void RemoveAndGetCount_LastConnection_ReturnsZeroAndRemovesKey()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        int remaining = mapping.RemoveAndGetCount(42, "conn-1");

        Assert.Equal(0, remaining);
        Assert.Empty(mapping.GetConnections(42));
    }

    [Fact]
    public void RemoveAndGetCount_OneOfMany_ReturnsRemainingCount()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");
        mapping.AddAndGetCount(42, "conn-2");
        mapping.AddAndGetCount(42, "conn-3");

        int remaining = mapping.RemoveAndGetCount(42, "conn-2");

        Assert.Equal(2, remaining);
    }

    [Fact]
    public void RemoveAndGetCount_UnknownKey_ReturnsZero()
    {
        var mapping = new ConnectionMapping<int>();

        int remaining = mapping.RemoveAndGetCount(999, "conn-1");

        Assert.Equal(0, remaining);
    }

    [Fact]
    public void RemoveAndGetCount_UnknownConnectionId_ReturnsExistingCount()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        int remaining = mapping.RemoveAndGetCount(42, "conn-unknown");

        Assert.Equal(1, remaining);
    }

    [Fact]
    public async Task AddAndGetCount_ConcurrentAddsForSameKey_OnlyOneReturnsOne()
    {
        var mapping = new ConnectionMapping<int>();
        const int parallelTasks = 100;
        int firstConnectionCount = 0;

        var tasks = Enumerable.Range(0, parallelTasks).Select(i => Task.Run(() =>
        {
            int count = mapping.AddAndGetCount(42, $"conn-{i}");
            if (count == 1)
                Interlocked.Increment(ref firstConnectionCount);
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(1, firstConnectionCount);
        Assert.Equal(parallelTasks, mapping.GetConnections(42).Count());
    }

    [Fact]
    public async Task RemoveAndGetCount_ConcurrentRemovesForSameKey_OnlyOneReturnsZero()
    {
        var mapping = new ConnectionMapping<int>();
        const int parallelTasks = 100;
        var connectionIds = Enumerable.Range(0, parallelTasks).Select(i => $"conn-{i}").ToArray();

        foreach (var id in connectionIds)
            mapping.AddAndGetCount(42, id);

        int lastDisconnectCount = 0;

        var tasks = connectionIds.Select(id => Task.Run(() =>
        {
            int remaining = mapping.RemoveAndGetCount(42, id);
            if (remaining == 0)
                Interlocked.Increment(ref lastDisconnectCount);
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(1, lastDisconnectCount);
        Assert.Empty(mapping.GetConnections(42));
    }

    [Fact]
    public void TryRemove_ExistingConnection_ReturnsTrueAndRemainingCount()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");
        mapping.AddAndGetCount(42, "conn-2");

        bool removed = mapping.TryRemove(42, "conn-1", out int remaining);

        Assert.True(removed);
        Assert.Equal(1, remaining);
    }

    [Fact]
    public void TryRemove_LastConnectionForKey_ReturnsTrueAndZero()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        bool removed = mapping.TryRemove(42, "conn-1", out int remaining);

        Assert.True(removed);
        Assert.Equal(0, remaining);
        Assert.Empty(mapping.GetConnections(42));
    }

    [Fact]
    public void TryRemove_UnknownKey_ReturnsFalse()
    {
        var mapping = new ConnectionMapping<int>();

        bool removed = mapping.TryRemove(99, "conn-1", out int remaining);

        Assert.False(removed);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public void TryRemove_UnknownConnectionId_ReturnsFalse()
    {
        var mapping = new ConnectionMapping<int>();
        mapping.AddAndGetCount(42, "conn-1");

        bool removed = mapping.TryRemove(42, "conn-unknown", out int remaining);

        Assert.False(removed);
        Assert.Equal(0, remaining);
        Assert.Single(mapping.GetConnections(42));
    }

    [Fact]
    public async Task AddRemoveAndGetCount_MixedConcurrentOperations_FinalCardinalIsCorrect()
    {
        var mapping = new ConnectionMapping<int>();
        const int totalConnections = 200;
        var connectionIds = Enumerable.Range(0, totalConnections).Select(i => $"conn-{i}").ToArray();

        foreach (var id in connectionIds)
            mapping.AddAndGetCount(42, id);

        // Remove half concurrently
        var toRemove = connectionIds.Take(totalConnections / 2).ToArray();
        var removeTasks = toRemove.Select(id => Task.Run(() => mapping.RemoveAndGetCount(42, id)));
        await Task.WhenAll(removeTasks);

        Assert.Equal(totalConnections / 2, mapping.GetConnections(42).Count());
    }
}
