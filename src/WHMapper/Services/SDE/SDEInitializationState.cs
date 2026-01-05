namespace WHMapper.Services.SDE
{
    /// <summary>
    /// Manages the global SDE initialization state across all users.
    /// This is a Singleton service that ensures only one SDE initialization can occur at a time.
    /// </summary>
    public class SDEInitializationState : ISDEInitializationState
    {
        private readonly object _lock = new object();
        private readonly ILogger<SDEInitializationState> _logger;
        private volatile bool _isInitializationInProgress;
        private string _currentProgressMessage = string.Empty;
        private TaskCompletionSource<bool>? _initializationCompletionSource;

        public SDEInitializationState(ILogger<SDEInitializationState> logger)
        {
            _logger = logger;
        }

        public bool IsInitializationInProgress => _isInitializationInProgress;

        public string CurrentProgressMessage => _currentProgressMessage;

        public event Action<string>? OnProgressChanged;

        public event Action? OnInitializationCompleted;

        public bool TryAcquireInitializationLock()
        {
            lock (_lock)
            {
                if (_isInitializationInProgress)
                {
                    _logger.LogInformation("SDE initialization already in progress, another user will wait");
                    return false;
                }

                _isInitializationInProgress = true;
                _initializationCompletionSource = new TaskCompletionSource<bool>();
                _logger.LogInformation("SDE initialization lock acquired");
                return true;
            }
        }

        public void ReleaseInitializationLock()
        {
            lock (_lock)
            {
                _isInitializationInProgress = false;
                _currentProgressMessage = string.Empty;
                
                // Signal all waiting tasks that initialization is complete
                _initializationCompletionSource?.TrySetResult(true);
                _initializationCompletionSource = null;
                
                _logger.LogInformation("SDE initialization lock released");
                
                // Notify subscribers that initialization is complete
                OnInitializationCompleted?.Invoke();
            }
        }

        public void SetProgressMessage(string message)
        {
            _currentProgressMessage = message;
            _logger.LogDebug("SDE initialization progress: {Message}", message);
            OnProgressChanged?.Invoke(message);
        }

        public async Task WaitForInitializationAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<bool>? tcs;
            
            lock (_lock)
            {
                if (!_isInitializationInProgress)
                {
                    return;
                }
                tcs = _initializationCompletionSource;
            }

            if (tcs != null)
            {
                _logger.LogInformation("Waiting for SDE initialization to complete");
                
                // Wait for either completion or cancellation
                using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
                
                try
                {
                    await tcs.Task.ConfigureAwait(false);
                    _logger.LogInformation("SDE initialization wait completed");
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("SDE initialization wait was cancelled");
                    throw;
                }
            }
        }
    }
}
