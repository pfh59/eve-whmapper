namespace WHMapper.Services.SDE
{
    /// <summary>
    /// Interface for managing the global SDE initialization state across all users.
    /// This service should be registered as a Singleton to ensure state is shared.
    /// </summary>
    public interface ISDEInitializationState
    {
        /// <summary>
        /// Gets whether an SDE initialization is currently in progress.
        /// </summary>
        bool IsInitializationInProgress { get; }

        /// <summary>
        /// Gets the current initialization progress message.
        /// </summary>
        string CurrentProgressMessage { get; }

        /// <summary>
        /// Attempts to acquire the initialization lock.
        /// </summary>
        /// <returns>True if the lock was acquired, false if initialization is already in progress.</returns>
        bool TryAcquireInitializationLock();

        /// <summary>
        /// Releases the initialization lock.
        /// </summary>
        void ReleaseInitializationLock();

        /// <summary>
        /// Updates the current progress message.
        /// </summary>
        /// <param name="message">The progress message to display.</param>
        void SetProgressMessage(string message);

        /// <summary>
        /// Waits asynchronously until the initialization is complete.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that completes when initialization is done.</returns>
        Task WaitForInitializationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Event raised when the progress message changes.
        /// </summary>
        event Action<string>? OnProgressChanged;

        /// <summary>
        /// Event raised when initialization completes.
        /// </summary>
        event Action? OnInitializationCompleted;
    }
}
