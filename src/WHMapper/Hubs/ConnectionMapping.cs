namespace WHMapper.Hubs;

public class ConnectionMapping<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();

    public int Count
    {
        get
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string>? connections=null;
            if (!_connections.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                return connections.ToList();
            }

            return Enumerable.Empty<string>();
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string>? connections=null;
            if (!_connections.TryGetValue(key, out connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }

    public int AddAndGetCount(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }
            connections.Add(connectionId);
            return connections.Count;
        }
    }

    public int RemoveAndGetCount(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
                return 0;

            connections.Remove(connectionId);
            int count = connections.Count;
            if (count == 0)
                _connections.Remove(key);
            return count;
        }
    }

    public bool TryRemove(T key, string connectionId, out int remainingCount)
    {
        remainingCount = 0;
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
                return false;
            if (!connections.Remove(connectionId))
                return false;
            remainingCount = connections.Count;
            if (remainingCount == 0)
                _connections.Remove(key);
            return true;
        }
    }
}
